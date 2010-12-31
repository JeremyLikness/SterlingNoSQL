using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.IsolatedStorage;
using Wintellect.Sterling.Test.Helpers;

namespace Wintellect.Sterling.Test.Database
{
    [Tag("Async")]
    [TestClass]
    public class TestAsync : SilverlightTest 
    {
        private SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;
        private List<TestModel> _modelList;

#if WINDOWS_PHONE
        private const int MODELS = 100; 
#else
        private const int MODELS = 1000; 
#endif

        [TestInitialize]
        public void TestInit()
        {
            _engine = new SterlingEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<TestDatabaseInstance>();
            _modelList = new List<TestModel>();
            for (var i = 0; i < MODELS; i++)
            {
                _modelList.Add(TestModel.MakeTestModel());
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _engine.Dispose();
            _databaseInstance = null;
            using (var iso = new IsoStorageHelper())
            {
                iso.Purge(PathProvider.BASE);
            }
        }

        [Asynchronous]
        [TestMethod]
        public void TestSave()
        {
            var grid = new Grid();
            var textBlock = new TextBlock();
            textBlock.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            textBlock.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            var progress = new ProgressBar {Minimum = 0, Maximum = 100, Value = 0};
            grid.Children.Add(progress);
            grid.Children.Add(textBlock);            
            grid.Loaded += (sender, args) =>
                               {
                                   var bw = _databaseInstance.SaveAsync((IList) _modelList);
                                   bw.WorkerReportsProgress = true;
                                   bw.ProgressChanged += (o, e) =>
                                                             {
                                                                 textBlock.Text = string.Format("{0}%",
                                                                                                e.ProgressPercentage);
                                                                 progress.Value = e.ProgressPercentage;
                                                             };
                                   bw.RunWorkerCompleted += (o, e) =>
                                                                {
                                                                    Assert.IsFalse(e.Cancelled,
                                                                                   "Asynchronous save was canceled.");
                                                                    Assert.IsNull(e.Error,
                                                                                  "Asynchronous save failed with error.");
                                                                    EnqueueTestComplete();                                                                    
                                                                };
                                   bw.RunWorkerAsync();
                               };
            TestPanel.Children.Add(grid);
        }

        [Asynchronous]
        [TestMethod]
        public void TestSaveNoProgress()
        {
            var grid = new Grid();
            var textBlock = new TextBlock();
            textBlock.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            textBlock.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            var progress = new ProgressBar { IsIndeterminate = true };
            grid.Children.Add(progress);
            grid.Children.Add(textBlock);
            grid.Loaded += (sender, args) =>
            {
                var bw = _databaseInstance.SaveAsync((IList)_modelList);
                bw.RunWorkerCompleted += (o, e) =>
                {
                    Assert.IsFalse(e.Cancelled,
                                   "Asynchronous save was canceled.");
                    Assert.IsNull(e.Error,
                                  "Asynchronous save failed with error.");
                    EnqueueTestComplete();                    
                };
                bw.RunWorkerAsync();
            };
            TestPanel.Children.Add(grid);
        }

        [Asynchronous]
        [TestMethod]
        public void TestSaveWithCancel()
        {
            var grid = new Grid();
            var textBlock = new TextBlock();
            textBlock.SetValue(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            textBlock.SetValue(FrameworkElement.VerticalAlignmentProperty, VerticalAlignment.Center);
            var progress = new ProgressBar { Minimum = 0, Maximum = 100, Value = 0 };
            grid.Children.Add(progress);
            grid.Children.Add(textBlock);
            grid.Loaded += (sender, args) =>
            {
                var bw = _databaseInstance.SaveAsync((IList)_modelList);
                bw.WorkerReportsProgress = true;
                bw.WorkerSupportsCancellation = true;
                bw.ProgressChanged += (o, e) =>
                {
                    textBlock.Text = string.Format("{0}%",
                                                   e.ProgressPercentage);
                    progress.Value = e.ProgressPercentage;
                    if (e.ProgressPercentage > 50)
                    {
                        bw.CancelAsync();
                    }
                };
                bw.RunWorkerCompleted += (o, e) =>
                                             {
                                                 Assert.IsTrue(e.Cancelled,
                                                                "Asynchronous save was not canceled.");
                                                 Assert.IsNull(e.Error,
                                                               "Asynchronous save failed with error.");
                                                 EnqueueTestComplete();                                                 
                                             };
                bw.RunWorkerAsync();
            };
            TestPanel.Children.Add(grid);
        }
    }
}
