using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.Exceptions;
using Wintellect.Sterling.Test.Helpers;

namespace Wintellect.Sterling.ElevatedTrust.Test.Database
{
    [Tag("DatabaseRegistration")]
    [TestClass]
    public class TestRegistration
    {        
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
                    db.RegisterDatabase<TestDatabaseInstance>(new ElevatedTrustDriver());
                }
                catch(SterlingNotReadyException)
                {
                    raiseError = true;
                }

                Assert.IsTrue(raiseError, "Sterling did not throw activation error.");

                engine.Activate();

                var testDb2 = db.RegisterDatabase<TestDatabaseInstance>(new ElevatedTrustDriver());

                Assert.IsNotNull(testDb2, "Database registration returned null.");
                Assert.IsInstanceOfType(testDb2, typeof(TestDatabaseInstance), "Incorrect database type returned.");
            
                Assert.AreEqual("TestDatabase", testDb2.Name, "Incorrect database name.");

                // test duplicate registration
                raiseError = false;

                try
                {
                    db.RegisterDatabase<TestDatabaseInstance>(new ElevatedTrustDriver());
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
                    db.RegisterDatabase<DupDatabaseInstance>(new ElevatedTrustDriver());
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
