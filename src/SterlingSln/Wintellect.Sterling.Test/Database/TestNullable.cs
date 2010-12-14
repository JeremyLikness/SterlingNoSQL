using System.Collections.Generic;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.Database;
using Wintellect.Sterling.IsolatedStorage;

namespace Wintellect.Sterling.Test.Database
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
        protected override List<ITableDefinition> _RegisterTables()
        {
            return new List<ITableDefinition>
                           {
                               CreateTableDefinition<NullableClass, int>(n => n.Id)
                           };
        }
    }

    [Tag("Nullable")]
    [Tag("Database")]
    [TestClass]
    public class TestNullable
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
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<NullableDatabase>();
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