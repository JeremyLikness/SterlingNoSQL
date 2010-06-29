using System;
using System.IO.IsolatedStorage;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.Exceptions;
using Wintellect.Sterling.IsolatedStorage;
using Wintellect.Sterling.Test.Helpers;

namespace Wintellect.Sterling.Test.Database
{
    [Tag("PathProvider")]
    [TestClass]
    public class TestPathProvider
    {
        /// <summary>
        ///     The target provider
        /// </summary>
        private PathProvider _target;

        /// <summary>
        ///     Test database
        /// </summary>
        private readonly ISterlingDatabaseInstance _testDatabase = new TestDatabaseInterfaceInstance();

        /// <summary>
        ///     Set up the provider
        /// </summary>
        [TestInitialize]
        public void Init()
        {
            _target = new PathProvider(SterlingFactory.GetLogger());
        }

        /// <summary>
        ///     Clean it up!
        /// </summary>
        [TestCleanup]
        public void TestCleanup()
        {
            using (var iso = new IsoStorageHelper())
            {
                iso.Purge(PathProvider.BASE);
            }          
        }

        /// <summary>
        ///     Test the construction
        /// </summary>
        [TestMethod]
        public void TestBaseProvider()
        {
            using(var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // ensure it created the base sterling path
                Assert.IsTrue(iso.DirectoryExists(PathProvider.BASE), "Path provider did not create sterling directory.");                
            }
        }

        /// <summary>
        ///     Test paths to the databases
        /// </summary>
        [TestMethod]
        public void TestDatabasePaths()
        {
            // test the invalid contract
            var raisedError = false; 

            try
            {
                _target.GetDatabasePath(string.Empty);
            }
            catch(ArgumentNullException)
            {
                raisedError = true;
            }

            Assert.IsTrue(raisedError, "Sterling did not raise an exception for an empty database.");

            var expected = string.Format("{0}0/", PathProvider.BASE);

            var testDirectory = _target.GetDatabasePath(_testDatabase.Name);

            Assert.AreEqual(expected, testDirectory, "Test directory was incorrect.");
            Assert.AreEqual(1, _target.NextDb, "Next db counter invalid.");

            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                Assert.IsTrue(iso.FileExists(string.Format(PathProvider.DB,PathProvider.BASE)), "Sterling did not serialize the table master.");
            }            
        }

        /// <summary>
        ///     Test deserializing the database when a new path provider is created
        /// </summary>
        [TestMethod]
        public void TestDatabasePathDeserialization()
        {
            var expected = string.Format("{0}0/", PathProvider.BASE);

            // this will create and serialize the directory
            _target.GetDatabasePath(_testDatabase.Name);

            var newPathProvider = new PathProvider(SterlingFactory.GetLogger());
            Assert.AreEqual(1, newPathProvider.NextDb, "Re-serialized path provider does not have correct database index.");

            var testDirectory = newPathProvider.GetDatabasePath(_testDatabase.Name);
            
            Assert.AreEqual(expected, testDirectory, "Test directory was incorrect after de-serialization.");          
        }

        /// <summary>
        ///     Test the table paths
        /// </summary>
        [TestMethod]
        public void TestTablePaths()
        {
            var expected = string.Format("{0}0/0/", PathProvider.BASE);

            // this will create and serialize the directory
            _target.GetDatabasePath(_testDatabase.Name);

            // check for invalid database
            var raiseError = false; 

            try
            {
                _target.GetTablePath("Bad Database", GetType());
            }
            catch(SterlingDatabaseNotFoundException)
            {
                raiseError = true;
            }

            Assert.IsTrue(raiseError, "Sterling did not raise the database not found exception.");

            // this should give us a table directory
            var actual = _target.GetTablePath<TestDatabaseInterfaceInstance>(_testDatabase.Name);

            Assert.AreEqual(expected, actual);           
        }

        /// <summary>
        ///     Test the table paths
        /// </summary>
        [TestMethod]
        public void TestTableSerialization()
        {
            // this will create and serialize the directory
            _target.GetDatabasePath(_testDatabase.Name);
            
            // this should give us a table directory
            _target.GetTablePath<TestDatabaseInterfaceInstance>(_testDatabase.Name);
            _target.GetTablePath<TestPathProvider>(_testDatabase.Name);

            var newPathProvider = new PathProvider(SterlingFactory.GetLogger());
            Assert.AreEqual(2, newPathProvider.NextTable, "Path provider did not deserialize the table indices.");

            var expected = string.Format("{0}0/1/", PathProvider.BASE);
            Assert.AreEqual(expected, newPathProvider.GetTablePath<TestPathProvider>(_testDatabase.Name),
                            "Unexpected path returned from new path provider.");
            
        }

        /// <summary>
        ///     Test the table paths
        /// </summary>
        [TestMethod]
        public void TestKeyPaths()
        {
            var expected = string.Format(PathProvider.KEY, string.Format("{0}0/0/", PathProvider.BASE));

            // this will create and serialize the directory
            _target.GetDatabasePath(_testDatabase.Name);            

            // this should give us a table directory
            var actual = _target.GetKeysPath<TestDatabaseInterfaceInstance>(_testDatabase.Name);

            Assert.AreEqual(expected, actual);           
        }
    }
}
