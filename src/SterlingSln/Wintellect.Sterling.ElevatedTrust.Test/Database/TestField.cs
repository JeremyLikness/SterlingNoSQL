using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.Database;

namespace Wintellect.Sterling.ElevatedTrust.Test.Database
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

        protected override System.Collections.Generic.List<ITableDefinition> RegisterTables()
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
            FileSystemHelper.PurgeAll();
            _engine = new SterlingEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<TestObjectFieldDatabase>(new ElevatedTrustDriver());
            _databaseInstance.Purge();
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
            _databaseInstance.Purge();
            _engine.Dispose();
            _databaseInstance = null;            
        }

    }

}