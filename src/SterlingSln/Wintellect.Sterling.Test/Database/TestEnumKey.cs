using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.Database;

namespace Wintellect.Sterling.Test.Database
{
    public enum Keys
    {
        Key0,
        Key1,
        Key2
    }

#if SILVERLIGHT

    [Tag("Database")]
#endif
    [TestClass]
    public class TestEnumKey
    {
        private SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;
        private ISterlingDriver _sterlingDriver;

        [TestInitialize]
        public void TestInit()
        {
            Init();
            ClearStorage();
        }

        private void ClearStorage()
        {
            if (_sterlingDriver != null)
            {
                _sterlingDriver.Purge();
            }
            else
            {
                if (_databaseInstance != null) _databaseInstance.Purge();
            }
        }


        [TestCleanup]
        public void TestCleanup()
        {
            ClearStorage();
            Cleanup();
        }

        private void Init()
        {
            _engine = new SterlingEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<EnumKeyDatabase>();
        }

        private void Cleanup()
        {
            _engine.Dispose();
            _databaseInstance = null;
            _engine = null;
            _sterlingDriver = null;
        }


        [TestMethod]
        public void TestMultipleEnumSaveAndLoad()
        {
            var test1 = new EnumKeyTestClass { Id = Keys.Key0, Name = "Name1" };
            var test2 = new EnumKeyTestClass { Id = Keys.Key1, Name = "Name2" };

            _databaseInstance.Save(test1);
            _databaseInstance.Save(test2);
            _databaseInstance.Flush();
            var actual1 = _databaseInstance.Load<EnumKeyTestClass>(Keys.Key0);
            var actual2 = _databaseInstance.Load<EnumKeyTestClass>(Keys.Key1);

            Assert.AreEqual(test1.Id, actual1.Id, "Failed to load enum: key 1 mismatch.");
            Assert.AreEqual(test1.Name, actual1.Name, "Failed to load enum: value 1 mismatch.");

            Assert.AreEqual(test2.Id, actual2.Id, "Failed to load enum: key 2 mismatch.");
            Assert.AreEqual(test2.Name, actual2.Name, "Failed to load enum: value 2 mismatch.");
        }

        [TestMethod]
        public void TestEnumKeySaveAndLoad()
        {
            var test = new EnumKeyTestClass { Id = Keys.Key0, Name = "Sample" };
            _databaseInstance.Save(test);
            _databaseInstance.Flush();
            var actual = _databaseInstance.Load<EnumKeyTestClass>(Keys.Key0);
            Assert.AreEqual(test.Id, actual.Id, "Failed to load enum: key mismatch.");
            Assert.AreEqual(test.Name, actual.Name, "Failed to load enum: value mismatch.");
        }

        [TestMethod]
        public void TestEnumKeySaveAndLoadAndReInit()
        {
            var test1 = new EnumKeyTestClass { Id = Keys.Key1, Name = "Sample" };
            _databaseInstance.Save(test1);
            _databaseInstance.Flush();
            var actual1 = _databaseInstance.Load<EnumKeyTestClass>(Keys.Key1);
            Assert.AreEqual(test1.Id, actual1.Id, "Failed to load enum: key mismatch.");
            Assert.AreEqual(test1.Name, actual1.Name, "Failed to load enum: value mismatch.");

            Cleanup();
            Init();

            var test = new EnumKeyTestClass { Id = Keys.Key0, Name = "Sample" };
            _databaseInstance.Save(test);
            _databaseInstance.Flush();
            var actual = _databaseInstance.Load<EnumKeyTestClass>(Keys.Key0);
            Assert.AreEqual(test.Id, actual.Id, "Failed to load enum: key mismatch.");
            Assert.AreEqual(test.Name, actual.Name, "Failed to load enum: value mismatch.");
            Assert.AreEqual(test.Name, actual.Name, "Failed to load enum: value mismatch.");
        }

        #region EnumKeyTestClass

        public class EnumKeyTestClass
        {
            public Keys Id { get; set; }

            public string Name { get; set; }
        }

        #endregion


        #region EnumKeyDatabase

        public class EnumKeyDatabase : BaseDatabaseInstance
        {
            /// <summary>
            ///     The name of the database instance
            /// </summary>
            public override string Name
            {
                get { return "EnumKey"; }
            }

            /// <summary>
            ///     Method called from the constructor to register tables
            /// </summary>
            /// <returns>The list of tables for the database</returns>
            protected override List<ITableDefinition> RegisterTables()
            {
                return new List<ITableDefinition>
                           {
                               CreateTableDefinition<EnumKeyTestClass, Keys>(e => e.Id)
                           };
            }
        }

        #endregion
    }
}
