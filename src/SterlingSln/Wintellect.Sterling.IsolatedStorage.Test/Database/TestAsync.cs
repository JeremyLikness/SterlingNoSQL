using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.Test.Helpers;

namespace Wintellect.Sterling.IsolatedStorage.Test.Database
{
    [Tag("Async")]
    [TestClass]
    public class TestAsync : SilverlightTest
    {
        private SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;
        private List<TestModel> _modelList;
        //private DateTime _startTime;

#if WINDOWS_PHONE
        private const int MODELS = 100;
#else
        private const int MODELS = 500;
#endif

        [TestInitialize]
        public void TestInit()
        {
            IsoStorageHelper.PurgeAll();            
            //_startTime = DateTime.Now;
            _engine = new SterlingEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<TestDatabaseInstance>(new IsolatedStorageDriver());
            _modelList = new List<TestModel>();
            for (var i = 0; i < MODELS; i++)
            {
                _modelList.Add(TestModel.MakeTestModel());
            }
        }

        /// <summary>
        ///     Clean up
        /// </summary>
        /// <remarks>
        ///     Uncomment the top block to display the time for each operation
        /// </remarks>
        [TestCleanup]
        public void TestCleanup()
        {
            //var duration = DateTime.Now - _startTime;

            //var dispatcher = Deployment.Current.Dispatcher;
            //Action action = () => MessageBox.Show(duration.ToString());

            //if (dispatcher.CheckAccess())
            //{
            //    action();
            //}
            //else
            //{
            //    dispatcher.BeginInvoke(action);
            //}


            _databaseInstance.Purge();
            _engine.Dispose();
            _databaseInstance = null;
            
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
            var progress = new ProgressBar {IsIndeterminate = true};
            grid.Children.Add(progress);
            grid.Children.Add(textBlock);
            grid.Loaded += (sender, args) =>
                               {
                                   var bw = _databaseInstance.SaveAsync((IList) _modelList);
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
            var progress = new ProgressBar {Minimum = 0, Maximum = 100, Value = 0};
            grid.Children.Add(progress);
            grid.Children.Add(textBlock);
            grid.Loaded += (sender, args) =>
                               {
                                   var bw = _databaseInstance.SaveAsync((IList) _modelList);
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

        [Asynchronous]
        [Tag("Concurrent")]
        [TestMethod]
        public void TestConcurrentSaveAndLoad()
        {
            var saveEvent = new ManualResetEvent(false);
            var loadEvent = new ManualResetEvent(false);
            
            // Initialize the DB with some data.
            foreach (var item in _modelList)
            {
                _databaseInstance.Save(item);
            }

            var savedCount = 0;
            var save = new BackgroundWorker();

            var errorMsg = string.Empty;

            save.DoWork += (o, e) =>
                               {
                                   try
                                   {
                                       foreach (var item in _modelList)
                                       {
                                           _databaseInstance.Save(item);
                                           savedCount++;
                                       }

                                       if (MODELS != savedCount)
                                       {
                                           throw new Exception("Save failed: Not all models were saved.");
                                       }
                                   }
                                   catch (Exception ex)
                                   {
                                       errorMsg = ex.AsExceptionString();
                                   }
                                   finally
                                   {
                                       saveEvent.Set();
                                   }
                               };
            var load = new BackgroundWorker();
            load.DoWork += (o, e) =>
                               {
                                   try
                                   {
                                       var query = from key in _databaseInstance.Query<TestModel, int>()
                                                   select key.LazyValue.Value;
                                       query.Count();

                                       var list = new List<TestModel>(query);

                                       if (list.Count < 1)
                                       {
                                           throw new Exception("Test failed: did not load any items.");
                                       }
                                   }
                                   catch (Exception ex)
                                   {
                                       errorMsg = ex.AsExceptionString();
                                   }
                                   finally
                                   {
                                       loadEvent.Set();
                                   }
                               };

            save.RunWorkerAsync();
            load.RunWorkerAsync();

            saveEvent.WaitOne(60000);
            loadEvent.WaitOne(60000);

            Assert.IsTrue(string.IsNullOrEmpty(errorMsg), string.Format("Failed concurrent load: {0}", errorMsg));

            EnqueueTestComplete();
        }

        [Asynchronous]
        [Tag("Concurrent")]
        [TestMethod]
        public void TestConcurrentSaveAndLoadWithIndex()
        {
            var saveEvent = new ManualResetEvent(false);
            var loadEvent = new ManualResetEvent(false);
            
            // Initialize the DB with some data.
            foreach (var item in _modelList)
            {
                _databaseInstance.Save(item);
            }

            var savedCount = 0;
            var save = new BackgroundWorker();

            var errorMsg = string.Empty;

            save.DoWork += (o, e) =>
                               {
                                   try
                                   {
                                       foreach (var item in _modelList)
                                       {
                                           _databaseInstance.Save(item);
                                           savedCount++;
                                       }

                                       if (MODELS != savedCount)
                                       {
                                           throw new Exception("Save failed: Not all models were saved.");
                                       }
                                   }
                                   catch (Exception ex)
                                   {
                                       errorMsg = ex.AsExceptionString();
                                   }
                                   finally
                                   {
                                       saveEvent.Set();
                                   }
                               };

            var load = new BackgroundWorker();
            load.DoWork += (o, e) =>
                               {
                                   try
                                   {
                                       var now = DateTime.Now;
                                       var query =
                                           from key in
                                               _databaseInstance.Query<TestModel, DateTime, string, int>("IndexDateData")
                                           where key.Index.Item1.Month == now.Month
                                           select key.LazyValue.Value;

                                       var list = new List<TestModel>(query);

                                       if (list.Count < 1)
                                       {
                                           throw new Exception("Test failed: did not load any models.");
                                       }
                                   }
                                   catch (Exception ex)
                                   {
                                       errorMsg = ex.AsExceptionString();
                                   }
                                   finally
                                   {
                                       loadEvent.Set();
                                   }
                               };

            save.RunWorkerAsync();
            load.RunWorkerAsync();

            saveEvent.WaitOne(60000);
            loadEvent.WaitOne(60000); 

            Assert.IsTrue(string.IsNullOrEmpty(errorMsg), string.Format("Concurrent test failed: {0}", errorMsg));

            EnqueueTestComplete();
        }
    }
}