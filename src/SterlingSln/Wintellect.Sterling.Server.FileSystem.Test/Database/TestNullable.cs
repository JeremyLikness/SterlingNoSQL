using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.Database;

namespace Wintellect.Sterling.Server.FileSystem.Test.Database
{
    public class NullableClass
    {
        public int Id { get; set; }
        public int? Value { get; set; }
    }

    public class NullableDatabase : BaseDatabaseInstance
    {
        /// <summary>
        ///     The name of the database instance
        /// </summary>
        public override string Name
        {
            get { return "Nullable"; }
        }

        /// <summary>
        ///     Method called from the constructor to register tables
        /// </summary>
        /// <returns>The list of tables for the database</returns>
        protected override List<ITableDefinition> RegisterTables()
        {
            return new List<ITableDefinition>
                           {
                               CreateTableDefinition<NullableClass, int>(n => n.Id)
                           };
        }
    }

    [TestClass]
    public class TestNullable
    {                
        private SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;

        [TestInitialize]
        public void TestInit()
        {
            FileSystemHelper.PurgeAll();
            _engine = new SterlingEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<NullableDatabase>(new FileSystemDriver());
            _databaseInstance.Purge();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _databaseInstance.Purge();
            _engine.Dispose();
            _databaseInstance = null;            
        }

        [TestMethod]
        public void TestNotNull()
        {
            var test = new NullableClass {Id = 1, Value = 1};
            _databaseInstance.Save(test);
            var actual = _databaseInstance.Load<NullableClass>(1);
            Assert.AreEqual(test.Id, actual.Id, "Failed to load nullable with nullable set: key mismatch.");
            Assert.AreEqual(test.Value, actual.Value, "Failed to load nullable with nullable set: value mismatch.");
        }

        [TestMethod]
        public void TestNull()
        {
            var test = new NullableClass { Id = 1, Value = null };
            _databaseInstance.Save(test);
            var actual = _databaseInstance.Load<NullableClass>(1);
            Assert.AreEqual(test.Id, actual.Id, "Failed to load nullable with nullable set: key mismatch.");
            Assert.IsNull(actual.Value, "Failed to load nullable with nullable set: value mismatch.");
        }
    }
}