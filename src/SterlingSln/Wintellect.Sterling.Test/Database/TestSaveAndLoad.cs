using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.Exceptions;
using Wintellect.Sterling.IsolatedStorage;
using Wintellect.Sterling.Test.Helpers;

namespace Wintellect.Sterling.Test.Database
{    
    [Tag("SaveAndLoad")]
    [TestClass]
    public class TestSaveAndLoad
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
        public void TestSaveExceptions()
        {
            var raiseException = false;
            try
            {
                _databaseInstance.Save(this);
            }
            catch(SterlingTableNotFoundException)
            {
                raiseException = true;
            }

            Assert.IsTrue(raiseException, "Sterling did not raise exception for unknown type.");
        }

        [TestMethod]
        public void TestSave()
        {
            // test saving and reloading
            var expected = TestModel.MakeTestModel();

            _databaseInstance.Save(expected);

            var actual = _databaseInstance.Load<TestModel>(expected.Key);

            Assert.IsNotNull(actual, "Load failed.");

            Assert.AreEqual(expected.Key, actual.Key, "Load failed: key mismatch.");
            Assert.AreEqual(expected.Data, actual.Data, "Load failed: data mismatch.");           
        }

        [TestMethod]
        public void TestSaveForeign()
        {
            var expected = TestAggregateModel.MakeAggregateModel();

            _databaseInstance.Save(expected);

            var actual = _databaseInstance.Load<TestAggregateModel>(expected.Key);
            var actualTestModel = _databaseInstance.Load<TestModel>(expected.TestModelInstance.Key);
            var actualForeignModel = _databaseInstance.Load<TestForeignModel>(expected.TestForeignInstance.Key);

            Assert.AreEqual(expected.Key, actual.Key, "Load with foreign key failed: key mismatch.");
            Assert.AreEqual(expected.TestForeignInstance.Key, actual.TestForeignInstance.Key, "Load failed: foreign key mismatch.");
            Assert.AreEqual(expected.TestForeignInstance.Data, actual.TestForeignInstance.Data, "Load failed: foreign data mismatch.");
            Assert.AreEqual(expected.TestModelInstance.Key, actual.TestModelInstance.Key, "Load failed: test model key mismatch.");
            Assert.AreEqual(expected.TestModelInstance.Data, actual.TestModelInstance.Data, "Load failed: test model data mismatch.");
            Assert.AreEqual(expected.TestForeignInstance.Key, actualForeignModel.Key, "Load failed: foreign key mismatch on direct load.");
            Assert.AreEqual(expected.TestForeignInstance.Data, actualForeignModel.Data, "Load failed: foreign data mismatch on direct load.");
            Assert.AreEqual(expected.TestModelInstance.Key, actualTestModel.Key, "Load failed: test model key mismatch on direct load.");
            Assert.AreEqual(expected.TestModelInstance.Data, actualTestModel.Data, "Load failed: test model data mismatch on direct load.");
        }

        [TestMethod]
        public void TestSaveForeignNull()
        {
            var expected = TestAggregateModel.MakeAggregateModel();
            expected.TestForeignInstance = null;

            _databaseInstance.Save(expected);

            var actual = _databaseInstance.Load<TestAggregateModel>(expected.Key);
            var actualTestModel = _databaseInstance.Load<TestModel>(expected.TestModelInstance.Key);
            
            Assert.AreEqual(expected.Key, actual.Key, "Load with foreign key failed: key mismatch.");
            Assert.IsNull(actual.TestForeignInstance, "Load failed: foreign key not set to null.");
            Assert.AreEqual(expected.TestModelInstance.Key, actual.TestModelInstance.Key, "Load failed: test model key mismatch.");
            Assert.AreEqual(expected.TestModelInstance.Data, actual.TestModelInstance.Data, "Load failed: test model data mismatch.");
            Assert.AreEqual(expected.TestModelInstance.Key, actualTestModel.Key, "Load failed: test model key mismatch on direct load.");
            Assert.AreEqual(expected.TestModelInstance.Data, actualTestModel.Data, "Load failed: test model data mismatch on direct load.");
        }
    }
}
