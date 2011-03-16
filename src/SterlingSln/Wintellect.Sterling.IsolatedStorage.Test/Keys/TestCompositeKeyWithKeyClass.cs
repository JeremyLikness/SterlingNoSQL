using System;
using System.Collections.Generic;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.Test.Helpers;

namespace Wintellect.Sterling.IsolatedStorage.Test.Keys
{
    [Tag("CompositeKey")]
    [TestClass]
    public class TestCompositeKeyWithKeyClass
    {
        private SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;

        [TestInitialize]
        public void TestInit()
        {
            IsoStorageHelper.PurgeAll();
            _engine = new SterlingEngine();
            _engine.SterlingDatabase.RegisterSerializer<TestCompositeSerializer>();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<TestDatabaseInstanceComposite>(new IsolatedStorageDriver());
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
        public void TestSave()
        {
            var random = new Random();
            // test saving and reloading
            var list = new List<TestCompositeClass>();
            for (var x = 0; x < 100; x++)
            {
                var testClass = new TestCompositeClass
                {
                    Key1 = random.Next(),
                    Key2 = random.Next().ToString(),
                    Key3 = Guid.NewGuid(),
                    Key4 = DateTime.Now.AddMinutes(-1 * random.Next(100)),
                    Data = Guid.NewGuid().ToString()
                };
                list.Add(testClass);
                _databaseInstance.Save(testClass);
            }

            for (var x = 0; x < 100; x++)
            {
                var actual = _databaseInstance.Load<TestCompositeClass>(new TestCompositeKeyClass(list[x].Key1,
                    list[x].Key2,list[x].Key3,list[x].Key4));
                Assert.IsNotNull(actual, "Load failed.");
                Assert.AreEqual(list[x].Data, actual.Data, "Load failed: data mismatch.");
            }
        }
    }
}