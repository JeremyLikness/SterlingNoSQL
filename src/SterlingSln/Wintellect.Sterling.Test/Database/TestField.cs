using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.Database;
using Wintellect.Sterling.IsolatedStorage;

namespace Wintellect.Sterling.Test.Database
{
    public class TestObjectField
    {
        public int Key;
        public string Data;
    }

    public class TestObjectFieldDatabase : BaseDatabaseInstance
    {
        public override string Name
        {
            get { return "TestObjectFieldDatabase"; }
        }

        protected override System.Collections.Generic.List<ITableDefinition> _RegisterTables()
        {
            return new System.Collections.Generic.List<ITableDefinition>
            {
                CreateTableDefinition<TestObjectField,int>(dataDefinition => dataDefinition.Key)
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
            var iso = new IsoStorageHelper();
            {
                iso.Purge(PathProvider.BASE);
            }
            _engine = new SterlingEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<TestObjectFieldDatabase>();
        }

        [TestMethod]
        public void TestData()
        {
            var testNull = new TestObjectField {Key = 1, Data = "data"};

            _databaseInstance.Save(testNull);

            var loadedTestNull = _databaseInstance.Load<TestObjectField>(1);

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
            var iso = new IsoStorageHelper();
            {
                iso.Purge(PathProvider.BASE);
            }
        }

    }

}