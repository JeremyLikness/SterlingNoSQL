using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.Database;
using Wintellect.Sterling.Exceptions;
using Wintellect.Sterling.IsolatedStorage;

namespace Wintellect.Sterling.Test.Database
{    
    public class TriggerClass
    {
        public int Id { get; set; }
        public string Data { get; set; }
        public bool IsDirty { get; set; }
    }

    public class TriggerClassTrigger : BaseSterlingTrigger<TriggerClass, int>
    {
        private static int _nextKey;
        private static readonly object _mutex = new object();

        public TriggerClassTrigger(ISterlingDatabaseInstance database)
        {
            _nextKey = (from key
                            in database.Query<TriggerClass, int>()
                        orderby key.Key descending
                        select key.Key).FirstOrDefault() + 1;
        }

        public override bool BeforeSave(TriggerClass instance)
        {
            if (instance.Id == 5) return false;
            
            if (instance.Id > 0) return true;

            Monitor.Enter(_mutex);
            instance.Id = _nextKey++;
            Monitor.Exit(_mutex);
                       
            return true;
        }

        public override void AfterSave(TriggerClass instance)
        {
            instance.IsDirty = false;
        }

        public override bool BeforeDelete(int key)
        {
            return key != 99;
        }
    }

    public class TriggerDatabase : BaseDatabaseInstance
    {       

        /// <summary>
        ///     The name of the database instance
        /// </summary>
        public override string Name
        {
            get { return "Trigger"; }
        }

        /// <summary>
        ///     Method called from the constructor to register tables
        /// </summary>
        /// <returns>The list of tables for the database</returns>
        protected override List<ITableDefinition> _RegisterTables()
        {
            return new List<ITableDefinition>
                           {
                               CreateTableDefinition<TriggerClass, int>(e => e.Id)
                           };
        }
    }

    [Tag("Trigger")]
    [Tag("Database")]
    [TestClass]
    public class TestTriggers
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
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<TriggerDatabase>();
            _databaseInstance.RegisterTrigger(new TriggerClassTrigger(_databaseInstance));
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
        public void TestTriggerBeforeSaveWithSuccess()
        {
            var key1 = _databaseInstance.Save<TriggerClass,int>(new TriggerClass {Data = Guid.NewGuid().ToString()});
            var key2 = _databaseInstance.Save<TriggerClass, int>(new TriggerClass { Data = Guid.NewGuid().ToString() });
            Assert.IsTrue(key1 > 0, "Trigger failed: key is not greater than 0.");
            Assert.IsTrue(key2 > 0, "Save failed: second key is not greater than 0.");
            Assert.IsTrue(key2 - key1 == 1, "Save failed: second key isn't one greater than first key.");
        }

        [TestMethod]
        public void TestTriggerBeforeSaveWithFailure()
        {
            var handled = false;
            try
            {
                _databaseInstance.Save<TriggerClass, int>(new TriggerClass { Id = 5, Data = Guid.NewGuid().ToString() });            
            }
            catch(SterlingTriggerException)
            {
                handled = true;
            }

            Assert.IsTrue(handled, "Save failed: trigger did not throw exception");

            var actual = _databaseInstance.Load<TriggerClass>(5);

            Assert.IsNull(actual, "Trigger failed: instance was saved.");
        }

        [TestMethod]
        public void TestTriggerAfterSave()
        {
            var target = new TriggerClass {Data = Guid.NewGuid().ToString(), IsDirty = true};
            _databaseInstance.Save<TriggerClass, int>(target);
            Assert.IsFalse(target.IsDirty, "Trigger failed: is dirty flag was not reset.");
        }

        [TestMethod]
        public void TestTriggerBeforeDelete()
        {
            var instance1 = new TriggerClass {Data = Guid.NewGuid().ToString()};
            _databaseInstance.Save<TriggerClass, int>(instance1);
            var key2 = _databaseInstance.Save<TriggerClass, int>(new TriggerClass { Id = 99, Data = Guid.NewGuid().ToString() });

            _databaseInstance.Delete(instance1); // should be no problem

            var handled = false;

            try
            {
                _databaseInstance.Delete(typeof(TriggerClass), key2);
            }
            catch(SterlingTriggerException)
            {
                handled = true;
            }

            Assert.IsTrue(handled, "Trigger failed to throw exception for delete operation on key = 5.");
        }
    }
}