using System.Collections.Generic;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.Database;

namespace Wintellect.Sterling.IsolatedStorage.Test.Database
{
    public interface IInterface
    {
        int Id { get; }
        int Value { get; }
    }

    public class InterfaceClass : IInterface
    {
        public int Id { get; set; }
        public int Value { get; set; }        
    }

    public class TargetClass
    {
        public int Id { get; set; }
        public IInterface SubInterface { get; set; }
    }

    public class InterfaceDatabase : BaseDatabaseInstance
    {
        /// <summary>
        ///     The name of the database instance
        /// </summary>
        public override string Name
        {
            get { return "Interface"; }
        }

        /// <summary>
        ///     Method called from the constructor to register tables
        /// </summary>
        /// <returns>The list of tables for the database</returns>
        protected override List<ITableDefinition> RegisterTables()
        {
            return new List<ITableDefinition>
                           {
                               CreateTableDefinition<TargetClass, int>(n => n.Id)
                           };
        }
    }

    [Tag("Interface")]
    [Tag("Database")]
    [TestClass]
    public class TestInterfaceProperty
    {
        private SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;

        [TestInitialize]
        public void TestInit()
        {
            IsoStorageHelper.PurgeAll();
            _engine = new SterlingEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<InterfaceDatabase>(new IsolatedStorageDriver());
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
        public void TestInterface()
        {
            var test = new TargetClass { Id = 1, SubInterface = new InterfaceClass { Id = 5, Value = 6 }};
            
            _databaseInstance.Save(test);
            
            var actual = _databaseInstance.Load<TargetClass>(1);
            
            Assert.AreEqual(test.Id, actual.Id, "Failed to load class with interface property: key mismatch.");
            Assert.IsNotNull(test.SubInterface, "Failed to load class with interface property: interface property is null.");
            Assert.AreEqual(test.SubInterface.Id, actual.SubInterface.Id, "Failed to load class with interface property: interface id mismatch.");
            Assert.AreEqual(test.SubInterface.Value, actual.SubInterface.Value, "Failed to load class with interface property: value mismatch.");            
        }       
    }
}