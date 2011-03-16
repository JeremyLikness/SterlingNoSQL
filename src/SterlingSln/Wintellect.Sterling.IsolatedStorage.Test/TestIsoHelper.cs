using System.IO;
using System.IO.IsolatedStorage;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.Exceptions;

namespace Wintellect.Sterling.IsolatedStorage.Test
{
    [Tag("IsolatedStorage")]
    [TestClass]
    public class TestIsoHelper
    {
        private const string PATH1= "Path1";
        private const string PATH2 = "Path1/Path2";
        private const string FILEPATH1 = "File.txt";
        private const string FILEPATH2 = "Path1/Path2/File2.txt";
        private const string TEXT1 = "Sample text";
        private const string TEXT2 = "More text";

        /// <summary>
        ///     Test construction and nesting of the helper
        /// </summary>
        [TestMethod]
        public void TestConstruction()
        {
            var helper1 = new IsoStorageHelper();
            {
                // just do this to confirm we can interact, we don't care about the result
                Assert.IsFalse(helper1.FileExists(FILEPATH1), "File should not exist.");

                var helper2 = new IsoStorageHelper();
                {
                    // again, just making sure this works when we are nested
                    Assert.IsFalse(helper2.FileExists(FILEPATH2), "File should not exist in nested assert.");
                }
            }
        }

        /// <summary>
        ///     Test we can read and write a file successfully 
        /// </summary>
        [TestMethod]
        public void TestWriter()
        {
            var helper1 = new IsoStorageHelper();
            {
                using (var bw = helper1.GetWriter(FILEPATH1))
                {
                    bw.Write(TEXT1);
                }

                // this should give us an exception because we haven't created the directory
                var exception = false;

                try
                {
                    using (var bw = helper1.GetWriter(FILEPATH2))
                    {
                        bw.Write(TEXT2);
                    }
                }
                catch(SterlingIsolatedStorageException)
                {
                    exception = true;
                }

                Assert.IsTrue(exception, "Failed to capture an isolated storage exception from Sterling.");
            }

            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                Assert.IsTrue(iso.FileExists(FILEPATH1), "File was not created.");

                using (var br = new BinaryReader(iso.OpenFile(FILEPATH1, FileMode.Open, FileAccess.Read)))
                {
                    var actual = br.ReadString();
                    Assert.AreEqual(TEXT1, actual, "Text mismatch when reading from isolated storage.");
                }

                iso.DeleteFile(FILEPATH1);
            }            
        }

        /// <summary>
        ///     Test file exists and the reader 
        /// </summary>
        [TestMethod]
        public void TestReaderAndExists()
        {
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var bw = new BinaryWriter(iso.OpenFile(FILEPATH1, FileMode.Create, FileAccess.Write)))
                {
                    bw.Write(TEXT1);
                }
            }

            var helper = new IsoStorageHelper();
            {
                Assert.IsTrue(helper.FileExists(FILEPATH1), "File exists for valid path failed.");
                Assert.IsFalse(helper.FileExists(FILEPATH2), "False positive from helper for file path that doesn't exist.");

                using (var br = helper.GetReader(FILEPATH1))
                {
                    var actual = br.ReadString();
                    Assert.AreEqual(TEXT1, actual, "Text mismatch when reading from isolated storage.");
                }
            }
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                iso.DeleteFile(FILEPATH1);
            }
        }

        /// <summary>
        ///     Ensures that a directory exists
        /// </summary>
        [TestMethod]
        public void TestEnsureDirectory()
        {
            var helper = new IsoStorageHelper();
            {
                helper.EnsureDirectory(PATH1);
                helper.EnsureDirectory(PATH1); // second time to make sure it doesn't error out
                helper.EnsureDirectory(PATH2);
            }

            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                Assert.IsTrue(iso.DirectoryExists(PATH2), "Second path was not created.");
                iso.DeleteDirectory(PATH2);
                iso.DeleteDirectory(PATH1);
            }
        }

        /// <summary>
        ///     Test that the directory purge works
        /// </summary>
        [TestMethod]
        public void TestPurge()
        {
            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {                
                iso.CreateDirectory(PATH1);
                iso.CreateDirectory(PATH2);

                using (var bw = new BinaryWriter(iso.OpenFile(FILEPATH2,FileMode.Create,FileAccess.Write)))
                {
                    bw.Write(TEXT2);
                }
            }

            var helper = new IsoStorageHelper();
            {
                helper.Purge(PATH2);
            }

            using (var iso = IsolatedStorageFile.GetUserStoreForApplication())
            {
                Assert.IsFalse(iso.FileExists(FILEPATH2), "Purge failed: file still exists.");
                Assert.IsFalse(iso.DirectoryExists(PATH2), "Purge failed: directory still exists.");
                Assert.IsTrue(iso.DirectoryExists(PATH1), "Purge failed: higher directory no longer exists.");
                iso.DeleteDirectory(PATH1);               
            }
        }
    }
}
