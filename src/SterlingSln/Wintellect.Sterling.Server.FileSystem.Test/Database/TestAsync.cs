using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.Test.Helpers;

namespace Wintellect.Sterling.Server.FileSystem.Test.Database
{
    [TestClass]
    public class TestAsync
    {
        private SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;
        private List<TestModel> _modelList;
        //private DateTime _startTime;

        private const int MODELS = 1000;

        [TestInitialize]
        public void TestInit()
        {
            //_startTime = DateTime.Now;
            _engine = new SterlingEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<TestDatabaseInstance>(new FileSystemDriver());
            _databaseInstance.Purge();
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
            _databaseInstance.Purge();
            _engine.Dispose();
            _databaseInstance = null;
        }

        [TestMethod]
        public void TestSave()
        {
            var testTrigger = new AutoResetEvent(false);
            var bw = _databaseInstance.SaveAsync((IList)_modelList);
            bw.WorkerReportsProgress = false;
            bw.RunWorkerCompleted += (o, e) =>
            {
                Assert.IsFalse(e.Cancelled,
                               "Asynchronous save was canceled.");
                Assert.IsNull(e.Error,
                              "Asynchronous save failed with error.");
                testTrigger.Set();
            };
            bw.RunWorkerAsync();
            testTrigger.WaitOne();
            var count = _databaseInstance.Query<TestModel, int>().Count();
            Assert.AreEqual(MODELS, count, "Invalid model count.");
        }

        [TestMethod]
        public void TestSaveWithCancel()
        {
            var testTrigger = new AutoResetEvent(false);

            var bw = _databaseInstance.SaveAsync((IList)_modelList);
            bw.WorkerSupportsCancellation = true;
            bw.RunWorkerCompleted += (o, e) =>
            {
                Assert.IsTrue(e.Cancelled,
                              "Asynchronous save was not canceled.");
                Assert.IsNull(e.Error,
                              "Asynchronous save failed with error.");
                testTrigger.Set();
            };
            bw.RunWorkerAsync();
            bw.CancelAsync();
            testTrigger.WaitOne();
        }

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
        }

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
        }
    }
}