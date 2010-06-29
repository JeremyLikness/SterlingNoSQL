using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.IsolatedStorage;
using Wintellect.Sterling.Test.Helpers;

namespace Wintellect.Sterling.Test.Database
{
    [Tag("List")]
    [TestClass]
    public class TestLists
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
            using (var iso = new IsoStorageHelper())
            {
                iso.Purge(PathProvider.BASE);
            }
        }

        [TestMethod]
        public void TestNullList()
        {
            var expected = TestListModel.MakeTestListModel();
            expected.Children = null;
            var key = _databaseInstance.Save(expected);
            var actual = _databaseInstance.Load<TestListModel>(key);
            Assert.IsNotNull(actual, "Save/load failed: model is null.");
            Assert.AreEqual(expected.ID, actual.ID, "Save/load failed: key mismatch.");
            Assert.IsNotNull(actual.Children, "Save/load failed: list not initialized.");
            Assert.AreEqual(0, actual.Children.Count, "Save/load failed: list size mismatch.");
        }

        [TestMethod]
        public void TestEmptyList()
        {
            var expected = TestListModel.MakeTestListModel();
            expected.Children.Clear();
            var key = _databaseInstance.Save(expected);
            var actual = _databaseInstance.Load<TestListModel>(key);
            Assert.IsNotNull(actual, "Save/load failed: model is null.");
            Assert.AreEqual(expected.ID, actual.ID, "Save/load failed: key mismatch.");
            Assert.IsNotNull(actual.Children, "Save/load failed: list not initialized.");
            Assert.AreEqual(0, actual.Children.Count, "Save/load failed: list size mismatch.");            
        }

        [TestMethod]
        public void TestList()
        {
            var expected = TestListModel.MakeTestListModel();
            var key = _databaseInstance.Save(expected);
            var actual = _databaseInstance.Load<TestListModel>(key);
            Assert.IsNotNull(actual, "Save/load failed: model is null.");
            Assert.AreEqual(expected.ID, actual.ID, "Save/load failed: key mismatch.");
            Assert.IsNotNull(actual.Children, "Save/load failed: list not initialized.");
            Assert.AreEqual(expected.Children.Count, actual.Children.Count, "Save/load failed: list size mismatch.");
            for (var x = 0; x < expected.Children.Count; x++)
            {
                Assert.AreEqual(expected.Children[x].Key, actual.Children[x].Key, "Save/load failed: key mismatch.");
                Assert.AreEqual(expected.Children[x].Data, actual.Children[x].Data, "Save/load failed: data mismatch.");                
            }
        }
    }
}