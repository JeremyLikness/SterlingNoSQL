using System.Linq;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.IsolatedStorage;
using Wintellect.Sterling.Test.Helpers;

namespace Wintellect.Sterling.Test.Database
{
    [Tag("Purge")]
    [TestClass]
    public class TestPurge
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
        public void TestPurgeAction()
        {
            // save a few objects
            var sample = TestModel.MakeTestModel();
            _databaseInstance.Save(sample);
            _databaseInstance.Save(TestModel.MakeTestModel());
            _databaseInstance.Save(TestModel.MakeTestModel());

            _databaseInstance.Purge();

            // query should be empty
            Assert.IsFalse(_databaseInstance.Query<TestModel, int>().Any(), "Purge failed: key list still exists.");

            // load should be empty
            var actual = _databaseInstance.Load<TestModel>(sample.Key);

            Assert.IsNull(actual, "Purge failed: was able to load item.");
        }
    }
}