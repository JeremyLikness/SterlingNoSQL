using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using SterlingExample.DbGenerator.RDA;
using SterlingExample.Model;

namespace SterlingExample.DbGenerator.ViewModel
{
    /// <summary>
    ///     View model to build the database
    /// </summary>
    public class BuildViewModel : BaseNotify
    {
        /// <summary>
        ///     The list of tasks
        /// </summary>
        private readonly List<Tuple<string, bool, Action<BackgroundWorker, DoWorkEventArgs>>> _workflow =
            new List<Tuple<string, bool, Action<BackgroundWorker, DoWorkEventArgs>>>
                {
                    Tuple.Create<string, bool, Action<BackgroundWorker, DoWorkEventArgs>>
                        ("Purge the Database", true, _Step1Worker),
                    Tuple.Create<string, bool, Action<BackgroundWorker, DoWorkEventArgs>>
                        ("Build Food Groups", false, _Step2Worker),
                    Tuple.Create<string, bool, Action<BackgroundWorker, DoWorkEventArgs>>
                        ("Build Nutrient Definitions", false, _Step3Worker),
                    Tuple.Create<string, bool, Action<BackgroundWorker, DoWorkEventArgs>>
                        ("Load Nutrient Data", false, _Step4Worker),                    
                     Tuple.Create<string, bool, Action<BackgroundWorker, DoWorkEventArgs>>(
                        "Build Food Descriptions", false, _Step5Worker),
                    Tuple.Create<string, bool, Action<BackgroundWorker, DoWorkEventArgs>>(
                        "Flush Indexes", true, _Step6Worker)
                };

        private int _idx;

        public BuildViewModel()
        {
            TasksComplete = new ObservableCollection<string>();
            if (DesignerProperties.IsInDesignTool)
            {
                Step = "Sample step...";
                StepType = false;
                ProgressPct = 50;
                Total = 50;
                TasksComplete.Add("Task 1");
                TasksComplete.Add("Task 2");
                return;
            }

            SterlingService.Current.RebuildRequested += BeginBuild;
        }

        /// <summary>
        ///     Entry point
        /// </summary>
        public void BeginBuild(object sender, EventArgs args)
        {
            SterlingService.Current.RebuildRequested -= BeginBuild; // unhook
            Step = string.Empty;
            _DoWork();
        }

        /// <summary>
        ///     Main workflow - iterates the list of tasks to perform
        /// </summary>
        private void _DoWork()
        {
            if (Step != null)
            {
                TasksComplete.Add(Step);
            }

            if (_idx >= _workflow.Count)
            {
                SterlingService.Current.RebuildRequested += BeginBuild; // rehook
                SterlingService.Current.Navigator.NavigateToDownloadView();
                return;
            }

            var workItem = _workflow[_idx++];

            Step = workItem.Item1;
            StepType = workItem.Item2;

            if (StepType)
            {
                Total = 0;
            }

            _MakeWorker(workItem.Item3, _DoWork);
        }

        /// <summary>
        ///     Step 1 - purge
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private static void _Step1Worker(BackgroundWorker o, DoWorkEventArgs e)
        {
            SterlingService.Current.Database.Purge();          
        }

        /// <summary>
        ///     Step 2 - food groups
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private static void _Step2Worker(BackgroundWorker o, DoWorkEventArgs e)
        {
            var foodGroups = (from fg in Parsers.GetFoodGroups() select fg).ToList();
            var progress = 0;
            foreach (var foodGroup in foodGroups)
            {
                SterlingService.Current.Database.Save(foodGroup);
                o.ReportProgress((int) (++progress*100.0/foodGroups.Count));
            }
            SterlingService.Current.Database.Flush();            
        }

        /// <summary>
        ///     Step 3 - nutrient definitions
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private static void _Step3Worker(BackgroundWorker o, DoWorkEventArgs e)
        {
            var nutrDefs = (from nd in Parsers.GetNutrientDefinitions() select nd).ToList();
            var progress = 0;
            foreach (var nutrDef in nutrDefs)
            {
                SterlingService.Current.Database.Save(nutrDef);
                o.ReportProgress((int)(++progress * 100.0 / nutrDefs.Count), progress);
            }
            SterlingService.Current.Database.Flush();            
        }

        /// <summary>
        ///     Step 4 - nutrient data
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private static void _Step4Worker(BackgroundWorker o, DoWorkEventArgs e)
        {
            _nutrientData = new Dictionary<int, List<NutrientDataElement>>();

            var progress = 0;

            o.ReportProgress(50, 0);
            var nutrientList = Parsers.GetNutrientData().ToList();            

            foreach (var nutrientData in nutrientList)
            {
                var foodDescriptionId = nutrientData.Item1;
                var nutrientRefId = nutrientData.Item2;
                var amount = nutrientData.Item3; 
                
                if (!_nutrientData.ContainsKey(foodDescriptionId))
                {
                    _nutrientData.Add(foodDescriptionId,new List<NutrientDataElement>());
                }

                _nutrientData[foodDescriptionId].Add(new NutrientDataElement
                                                         {
                                                             NutrientDefinitionId = nutrientRefId,
                                                             AmountPerHundredGrams = amount
                                                         });
                if (progress % 1000 == 0)
                {
                    o.ReportProgress((int) (++progress*50.0/nutrientList.Count) + 50, progress);
                }                
            }
            nutrientList.Clear();
        }

        private static Dictionary<int, List<NutrientDataElement>> _nutrientData;

        /// <summary>
        ///     Step 5 - food descriptions
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private static void _Step5Worker(BackgroundWorker o, DoWorkEventArgs e)
        {            
            // parse these bad boys
            var size = 0;
            var totalCount = 0;

            var foodDescriptions = Parsers.GetFoodDescriptions().ToList();

            foreach (var foodDescription in foodDescriptions)
            {
                if (_nutrientData.ContainsKey(foodDescription.Id))
                {
                    foodDescription.Nutrients = _nutrientData[foodDescription.Id];
                    totalCount += foodDescription.Nutrients.Count;
                }
                else
                {
                    totalCount++;
                }
                SterlingService.Current.Database.Save(foodDescription);
                o.ReportProgress((int) (++size*100.0/foodDescriptions.Count), totalCount);
            }

            _nutrientData.Clear();            
        }

        /// <summary>
        ///     Step 6 - flush indexes
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private static void _Step6Worker(BackgroundWorker o, DoWorkEventArgs e)
        {                        
            SterlingService.Current.Database.Flush();            
        }

        /// <summary>
        ///     Make a worker
        /// </summary>
        /// <param name="work">The work to do</param>
        /// <param name="completed">What to do when done</param>
        private void _MakeWorker(Action<BackgroundWorker, DoWorkEventArgs> work, Action completed)
        {
            var bw = new BackgroundWorker {WorkerReportsProgress = true};
            bw.DoWork += (o, e) => work(bw, e);
            bw.ProgressChanged += (o, e) =>
                                      {
                                          ProgressPct = e.ProgressPercentage;
                                          Total = (e.UserState is int) ? (int) e.UserState : e.ProgressPercentage;
                                      };
            bw.RunWorkerCompleted += (o, e) => completed();
            bw.RunWorkerAsync();
        }

        /// <summary>
        ///     Completed tasks
        /// </summary>
        public ObservableCollection<string> TasksComplete { get; private set; }

        private string _step;

        /// <summary>
        ///     Current step
        /// </summary>
        public string Step
        {
            get { return _step; }
            set
            {
                _step = value;
                RaisePropertyChanged(() => Step);
            }
        }

        private bool _stepType;

        /// <summary>
        ///     True if the progress of this step is indeterminate
        /// </summary>
        public bool StepType
        {
            get { return _stepType; }
            set
            {
                _stepType = value;
                RaisePropertyChanged(() => StepType);
            }
        }

        private int _progressPct;

        /// <summary>
        ///     Percentage completed
        /// </summary>
        public int ProgressPct
        {
            get { return _progressPct; }
            set
            {
                _progressPct = value;
                RaisePropertyChanged(() => ProgressPct);
            }
        }

        private int _total;

        /// <summary>
        ///     Total items processed in this step
        /// </summary>
        public int Total
        {
            get { return _total; }
            set
            {
                _total = value;
                RaisePropertyChanged(() => Total);
            }
        }
    }
}