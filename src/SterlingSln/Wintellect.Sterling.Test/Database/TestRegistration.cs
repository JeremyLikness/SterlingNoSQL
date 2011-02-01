using System;
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
using Wintellect.Sterling.Exceptions;
using Wintellect.Sterling.IsolatedStorage;
using Wintellect.Sterling.Test.Helpers;

namespace Wintellect.Sterling.Test.Database
{
    [Tag("DatabaseRegistration")]
    [TestClass]
    public class TestRegistration
    {
        [TestCleanup]
        public void TestTeardown()
        {
            var iso = new IsoStorageHelper();
            {
                iso.Purge(PathProvider.BASE);
            }
        }

        [TestMethod]
        public void TestDatabaseRegistration()
        {
            using (var engine = new SterlingEngine())
            {
                var db = engine.SterlingDatabase;

                // test not activated yet 
                var raiseError = false;

                try
                {
                    db.RegisterDatabase<TestDatabaseInstance>();
                }
                catch(SterlingNotReadyException)
                {
                    raiseError = true;
                }

                Assert.IsTrue(raiseError, "Sterling did not throw activation error.");

                engine.Activate();

                var testDb2 = db.RegisterDatabase<TestDatabaseInstance>();

                Assert.IsNotNull(testDb2, "Database registration returned null.");
                Assert.IsInstanceOfType(testDb2, typeof(TestDatabaseInstance), "Incorrect database type returned.");
            
                Assert.AreEqual("TestDatabase", testDb2.Name, "Incorrect database name.");

                // test duplicate registration
                raiseError = false;

                try
                {
                    db.RegisterDatabase<TestDatabaseInstance>();
                }
                catch(SterlingDuplicateDatabaseException)
                {
                    raiseError = true;
                }

                Assert.IsTrue(raiseError, "Sterling did not capture the duplicate database.");

                // test bad database (no table definitions) 
                raiseError = false;

                try
                {
                    db.RegisterDatabase<DupDatabaseInstance>();
                }
                catch (SterlingDuplicateTypeException)
                {
                    raiseError = true;
                }

                Assert.IsTrue(raiseError, "Sterling did not catch the duplicate type registration.");
            }
        }
    }
}
