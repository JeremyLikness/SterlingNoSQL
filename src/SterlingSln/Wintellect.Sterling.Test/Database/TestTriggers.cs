using System;
using System.Collections.Generic;
using System.Linq;
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
    
#if WINDOWS_PHONE 
    // for an unknown reason the presence of this class causes the Silverlight unit test harness to fail. This test will
    // be disabled on the phone until the cause is phone. The reference/test project does use triggers and is an integration teset
    // that validates their use.
#else
    public class TriggerClassTestTrigger : BaseSterlingTrigger<TriggerClass, int>
    {
        public const int BADSAVE = 5;
        public const int BADDELETE = 99;

        private int _nextKey;
        
        public TriggerClassTestTrigger(int nextKey)
        {
            _nextKey = nextKey;
        }

        public override bool BeforeSave(TriggerClass instance)
        {
            if (instance.Id == BADSAVE) return false;
            
            if (instance.Id > 0) return true;

            instance.Id = _nextKey++;                       
            return true;
        }

        public override void AfterSave(TriggerClass instance)
        {
            instance.IsDirty = false;
        }

        public override bool BeforeDelete(int key)
        {
            return key != BADDELETE;
        }
    }

    public class TriggerDatabase : BaseDatabaseInstance
    {       

        /// <summary>
        ///     The name of the database instance
        /// </summary>
        public override string Name
        {
            get { return "TriggerDatabase"; }
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
            var iso = new IsoStorageHelper();
            {
                iso.Purge(PathProvider.BASE);
            }
            _engine = new SterlingEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<TriggerDatabase>();

            // get the next key in the database for auto-assignment
            var nextKey = _databaseInstance.Query<TriggerClass, int>().Any() ?
                (from keys in _databaseInstance.Query<TriggerClass, int>()
                 select keys.Key).Max() + 1 : 1;

            _databaseInstance.RegisterTrigger(new TriggerClassTestTrigger(nextKey));
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _databaseInstance.Purge();
            _engine.Dispose();
            _databaseInstance = null;
            var iso = new IsoStorageHelper();
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
                _databaseInstance.Save<TriggerClass, int>(new TriggerClass { Id = TriggerClassTestTrigger.BADSAVE, Data = Guid.NewGuid().ToString() });            
            }
            catch(SterlingTriggerException)
            {
                handled = true;
            }

            Assert.IsTrue(handled, "Save failed: trigger did not throw exception");

            var actual = _databaseInstance.Load<TriggerClass>(TriggerClassTestTrigger.BADSAVE);

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
            var key2 = _databaseInstance.Save<TriggerClass, int>(new TriggerClass { Id = TriggerClassTestTrigger.BADDELETE, Data = Guid.NewGuid().ToString() });

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
#endif
}
