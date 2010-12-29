using System.Collections.Generic;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.Database;
using Wintellect.Sterling.IsolatedStorage;

namespace Wintellect.Sterling.Test.Database
{
    public class CycleClass
    {
        public int Id { get; set; }
        public int Value { get; set; }
        public CycleClass ChildCycle { get; set; }
    }

    public class CycleDatabase : BaseDatabaseInstance
    {
        /// <summary>
        ///     The name of the database instance
        /// </summary>
        public override string Name
        {
            get { return "Cycle"; }
        }

        /// <summary>
        ///     Method called from the constructor to register tables
        /// </summary>
        /// <returns>The list of tables for the database</returns>
        protected override List<ITableDefinition> _RegisterTables()
        {
            return new List<ITableDefinition>
                           {
                               CreateTableDefinition<CycleClass, int>(n => n.Id)
                           };
        }
    }

    [Tag("Cycle")]
    [Tag("Database")]
    [TestClass]
    public class TestCycle
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
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<CycleDatabase>();
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
        public void TestCycleNegativeCase()
        {
            var test = new CycleClass { Id = 1, Value = 1 };
            var child = new CycleClass {Id = 2, Value = 5 };            
            test.ChildCycle = child;
            
            _databaseInstance.Save(test);
            var actual = _databaseInstance.Load<CycleClass>(1);
            Assert.AreEqual(test.Id, actual.Id, "Failed to load cycle with non-null child: key mismatch.");
            Assert.AreEqual(test.Value, actual.Value, "Failed to load cycle with non-null child: value mismatch.");
            Assert.IsNotNull(test.ChildCycle, "Failed to load cycle with non-null child: child is null.");
            Assert.AreEqual(child.Id, actual.ChildCycle.Id, "Failed to load cycle with non-null child: child key mismatch.");
            Assert.AreEqual(child.Value, actual.ChildCycle.Value, "Failed to load cycle with non-null child: value mismatch.");
            
            actual = _databaseInstance.Load<CycleClass>(2);
            Assert.AreEqual(child.Id, actual.Id, "Failed to load cycle with non-null child: key mismatch on direct child load.");
            Assert.AreEqual(child.Value, actual.Value, "Failed to load cycle with non-null child: value mismatch on direct child load.");            
        }

        // This line commented because the condition is not yet handled
        //[TestMethod] 
        public void TestCyclePositiveCase()
        {
            var test = new CycleClass { Id = 1, Value = 1 };
            var child = new CycleClass { Id = 2, Value = 5 };
            test.ChildCycle = child;
            child.ChildCycle = test; // this creates our cycle condition

            _databaseInstance.Save(test);
            var actual = _databaseInstance.Load<CycleClass>(1);
            Assert.AreEqual(test.Id, actual.Id, "Failed to load cycle with non-null child: key mismatch.");
            Assert.AreEqual(test.Value, actual.Value, "Failed to load cycle with non-null child: value mismatch.");
            Assert.IsNotNull(test.ChildCycle, "Failed to load cycle with non-null child: child is null.");
            Assert.AreEqual(child.Id, actual.ChildCycle.Id, "Failed to load cycle with non-null child: child key mismatch.");
            Assert.AreEqual(child.Value, actual.ChildCycle.Value, "Failed to load cycle with non-null child: value mismatch.");

            actual = _databaseInstance.Load<CycleClass>(2);
            Assert.AreEqual(child.Id, actual.Id, "Failed to load cycle with non-null child: key mismatch on direct child load.");
            Assert.AreEqual(child.Value, actual.Value, "Failed to load cycle with non-null child: value mismatch on direct child load.");
        }        

    }
}