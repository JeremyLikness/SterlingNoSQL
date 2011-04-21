using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;

namespace SterlingExample.ViewModel
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
                        ("Restore the Database (this may take several minutes)", true, _Step2Worker),
                    Tuple.Create<string, bool, Action<BackgroundWorker, DoWorkEventArgs>>
                        ("Restart the Database", true, _Step3Worker)                    
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
                SterlingService.Current.Navigator.NavigateToMainView();
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
            SterlingService.Current.Database.Flush();
            SterlingService.Current.Database.Purge();  
            SterlingService.ShutDownDatabase();
            SterlingService.StartUpEngine();            
        }

        /// <summary>
        ///     Step 2 - database
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private static void _Step2Worker(BackgroundWorker o, DoWorkEventArgs e)
        {
            using (var stream =  Application.GetResourceStream(new Uri("DatabaseImage/database.sdb", UriKind.Relative)).Stream)
            {
                using (var br = new BinaryReader(stream))
                {
                    SterlingService.StartUpDatabase();
                    SterlingService.RestoreDatabase(br);
                }
            }      
            SterlingService.ShutDownDatabase();
        }        


        /// <summary>
        ///     Step 4 - reset
        /// </summary>
        /// <param name="o"></param>
        /// <param name="e"></param>
        private static void _Step3Worker(BackgroundWorker o, DoWorkEventArgs e)
        {
            SterlingService.StartUpEngine();
            SterlingService.StartUpDatabase();
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