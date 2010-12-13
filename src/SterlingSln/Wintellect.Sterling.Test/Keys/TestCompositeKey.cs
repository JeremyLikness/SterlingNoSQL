using System;
using System.Collections.Generic;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.IsolatedStorage;
using Wintellect.Sterling.Test.Helpers;

namespace Wintellect.Sterling.Test.Keys
{
    [Tag("CompositeKey")]
    [TestClass]
    public class TestCompositeKey
    {
        private SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;

        [TestInitialize]
        public void TestInit()
        {
            _engine = new SterlingEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<TestDatabaseInstance>();
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

        private static string GetKey(TestCompositeClass testClass)
        {
            return string.Format("{0}-{1}-{2}-{3}", testClass.Key1, testClass.Key2, testClass.Key3,
                                 testClass.Key4);
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
                                        Key4 = DateTime.Now.AddMinutes(-1*random.Next(100)),
                                        Data = Guid.NewGuid().ToString()
                                    };
                list.Add(testClass);
                _databaseInstance.Save(testClass);
            }

            for (var x = 0; x < 100; x++)
            {
                var actual = _databaseInstance.Load<TestCompositeClass>(GetKey(list[x]));
                Assert.IsNotNull(actual, "Load failed.");
                Assert.AreEqual(GetKey(list[x]), GetKey(actual), "Load failed: key mismatch.");
                Assert.AreEqual(list[x].Data, actual.Data, "Load failed: data mismatch.");
            }
        }
    }
}