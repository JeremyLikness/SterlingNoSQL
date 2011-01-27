using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.Database;
using Wintellect.Sterling.IsolatedStorage;

namespace Wintellect.Sterling.Test.Database
{
    public class TestNullObjectField
    {
        public int Key;
        public string Data;
    }

    public class TestNullObjectFieldDatabase : BaseDatabaseInstance
    {
        public override string Name
        {
            get { return "TestNullObjectFieldDatabase"; }
        }

        protected override System.Collections.Generic.List<ITableDefinition> _RegisterTables()
        {
            return new System.Collections.Generic.List<ITableDefinition>
            {
                CreateTableDefinition<TestNullObjectField,int>(dataDefinition => dataDefinition.Key)
            };
        }
    }

    [Tag("Field")]
    [Tag("Database")]
    [TestClass]
    public class TestField
    {
        private SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;

        [TestInitialize]
        public void TestInit()
        {
            using (var iso = new IsoStorageHelper())
            {
                iso.Purge(PathProvider.BASE);
            }
            _engine = new SterlingEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<TestNullObjectFieldDatabase>();
        }

        [TestMethod]
        public void TestData()
        {
            var testNull = new TestNullObjectField {Key = 1, Data = "data"};

            _databaseInstance.Save(testNull);

            var loadedTestNull = _databaseInstance.Load<TestNullObjectField>(1);

            // The values in the deserialized class should be populated.
            Assert.IsNotNull(loadedTestNull);
            Assert.IsNotNull(loadedTestNull.Data);
            Assert.IsNotNull(loadedTestNull.Key);
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

    }

}