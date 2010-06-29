using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.IsolatedStorage;
using Wintellect.Sterling.Test.Helpers;

namespace Wintellect.Sterling.Test.Database
{
    [Tag("Serializers")]
    [TestClass]
    public class TestSerializers
    {
        private SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;

        [TestInitialize]
        public void TestInit()
        {
            _engine = new SterlingEngine();
            _engine.SterlingDatabase.RegisterSerializer<TestSerializer>();
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
            var expected = TestClassWithStruct.MakeTestClassWithStruct();
            var key = _databaseInstance.Save(expected);
            var actual = _databaseInstance.Load<TestClassWithStruct>(key);
            Assert.IsNotNull(actual, "Save/load failed: model is null.");
            Assert.AreEqual(expected.ID, actual.ID, "Save/load failed: key mismatch.");
            Assert.IsNotNull(actual.Structs, "Save/load failed: list not initialized.");
            Assert.AreEqual(expected.Structs.Count, actual.Structs.Count, "Save/load failed: list size mismatch.");
            Assert.AreEqual(expected.Structs[0].Date, actual.Structs[0].Date, "Save/load failed: date mismatch.");
            Assert.AreEqual(expected.Structs[0].Value, actual.Structs[0].Value, "Save/load failed: value mismatch.");
        }
    }
}
