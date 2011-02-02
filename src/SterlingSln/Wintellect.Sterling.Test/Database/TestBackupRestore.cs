using System.IO;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.IsolatedStorage;
using Wintellect.Sterling.Test.Helpers;

namespace Wintellect.Sterling.Test.Database
{
    [Tag("Backup")]
    [Tag("Database")]
    [TestClass]
    public class TestBackupRestore
    {
        private SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;

        [TestCleanup]
        public void TestCleanup()
        {
            _engine.Dispose();
            _databaseInstance = null;
            var iso = new IsoStorageHelper();
            {
                iso.Purge(PathProvider.BASE);
            }
        }

        [TestMethod]
        public void TestBackupAndRestore()
        {
            // activate the engine and store the data
            _engine = new SterlingEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<TestDatabaseInstance>();

            // test saving and reloading
            var expected = TestModel.MakeTestModel();

            _databaseInstance.Save(expected);

            // now back it up
            var memStream = new MemoryStream();

            byte[] databaseBuffer;

            using (var binaryWriter = new BinaryWriter(memStream))
            {
                _engine.SterlingDatabase.Backup<TestDatabaseInstance>(binaryWriter);
                binaryWriter.Flush();
                databaseBuffer = memStream.GetBuffer();
            }

            // now purge the database
            _databaseInstance.Purge();

            var actual = _databaseInstance.Load<TestModel>(expected.Key);

            // confirm the database is gone
            Assert.IsNull(actual, "Purge failed, was able to load the test model.");

            _databaseInstance = null;

            // shut it all down
            _engine.Dispose();
            _engine = null;
            
            // get a new engine
            _engine = new SterlingEngine();
            
            // activate it and grab the database again
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<TestDatabaseInstance>();

            // restore it
            _engine.SterlingDatabase.Restore<TestDatabaseInstance>(new BinaryReader(new MemoryStream(databaseBuffer)));

            _engine.Dispose();
            _engine = null;

            // get a new engine
            _engine = new SterlingEngine();

            // activate it and grab the database again
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<TestDatabaseInstance>();            

            actual = _databaseInstance.Load<TestModel>(expected.Key);

            Assert.IsNotNull(actual, "Load failed.");

            Assert.AreEqual(expected.Key, actual.Key, "Load failed: key mismatch.");
            Assert.AreEqual(expected.Data, actual.Data, "Load failed: data mismatch.");
            Assert.IsNull(actual.Data2, "Load failed: suppressed data property not valid on de-serialize.");
            Assert.IsNotNull(actual.SubClass, "Load failed: sub class is null.");
            Assert.IsNull(actual.SubClass2, "Load failed: supressed sub class should be null.");
            Assert.AreEqual(expected.SubClass.NestedText, actual.SubClass.NestedText,
                            "Load failed: sub class text mismtach.");
            Assert.AreEqual(expected.SubStruct.NestedId, actual.SubStruct.NestedId,
                            "Load failed: sub struct id mismtach.");
            Assert.AreEqual(expected.SubStruct.NestedString, actual.SubStruct.NestedString,
                            "Load failed: sub class string mismtach.");
        }
    }
}