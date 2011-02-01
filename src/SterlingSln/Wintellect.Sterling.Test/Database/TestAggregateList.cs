using System;
using System.Net;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Silverlight.Testing;
using Wintellect.Sterling.Test.Helpers;
using Wintellect.Sterling.IsolatedStorage;

namespace Wintellect.Sterling.Test.Database
{
    [Tag("AggregateList")]
    [TestClass]
    public class TestAggregateList
    {
        private SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;

        [TestInitialize]
        public void TestInit()
        {
            _engine = new SterlingEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<TestDatabaseInstance>();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _engine.Dispose();
            _databaseInstance = null;
            var iso = new IsoStorageHelper();
            {
                iso.Purge(PathProvider.BASE);
            }
        }

        [TestMethod]
        public void TestNullList()
        {
            var expected = TestAggregateListModel.MakeTestAggregateListModel();
            expected.Children = null;
            var key = _databaseInstance.Save(expected);
            var actual = _databaseInstance.Load<TestAggregateListModel>(key);
            Assert.IsNotNull(actual, "Save/load failed: model is null.");
            Assert.AreEqual(expected.ID, actual.ID, "Save/load failed: key mismatch.");
            Assert.IsNotNull(actual.Children, "Save/load failed: list not initialized.");
            Assert.AreEqual(0, actual.Children.Count, "Save/load failed: list size mismatch.");
        }

        [TestMethod]
        public void TestEmptyList()
        {
            var expected = TestAggregateListModel.MakeTestAggregateListModel();
            expected.Children.Clear();
            var key = _databaseInstance.Save(expected);
            var actual = _databaseInstance.Load<TestAggregateListModel>(key);
            Assert.IsNotNull(actual, "Save/load failed: model is null.");
            Assert.AreEqual(expected.ID, actual.ID, "Save/load failed: key mismatch.");
            Assert.IsNotNull(actual.Children, "Save/load failed: list not initialized.");
            Assert.AreEqual(0, actual.Children.Count, "Save/load failed: list size mismatch.");
        }

        [TestMethod]
        public void TestList()
        {
            var expected = TestAggregateListModel.MakeTestAggregateListModel();
            var key = _databaseInstance.Save(expected);
            var actual = _databaseInstance.Load<TestAggregateListModel>(expected.ID);
            Assert.IsNotNull(actual, "Save/load failed: model is null.");
            Assert.AreEqual(expected.ID, actual.ID, "Save/load failed: key mismatch.");
            Assert.IsNotNull(actual.Children, "Save/load failed: list not initialized.");
            Assert.AreEqual(expected.Children.Count, actual.Children.Count, "Save/load failed: list size mismatch.");

            for (var x = 0; x < expected.Children.Count; x++)
            {
                Assert.AreEqual(expected.Children[x].Key, actual.Children[x].Key, "Save/load failed: key mismatch.");
                Assert.AreEqual(expected.Children[x].BaseProperty, actual.Children[x].BaseProperty, "Save/load failed: data mismatch.");
                Assert.AreEqual(expected.Children[x].GetType(), actual.Children[x].GetType(), "Save/load failed: type mismatch.");
            }
        }
    }
}
