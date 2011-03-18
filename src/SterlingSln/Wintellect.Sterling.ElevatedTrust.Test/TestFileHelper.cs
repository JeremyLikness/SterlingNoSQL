using System;
using System.IO;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Wintellect.Sterling.ElevatedTrust.Test
{
    [Tag("IsolatedStorage")]
    [TestClass]
    public class TestIsoHelper
    {
        private const string PATH1= "Path1";
        private const string PATH2 = "Path1\\Path2";
        private const string FILEPATH1 = "File.txt";
        private const string FILEPATH2 = "Path1\\Path2\\File2.txt";
        private const string TEXT1 = "Sample text";
        private const string TEXT2 = "More text";

        private readonly string _root = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                                                     "Sterling Database");

        private string _AsPath(string path)
        {
            var pathModified = Path.Combine(_root, path);
            return pathModified;
        }

        /// <summary>
        ///     Test construction and nesting of the helper
        /// </summary>
        [TestMethod]
        public void TestConstruction()
        {
            var helper1 = new FileSystemHelper();
            {
                // just do this to confirm we can interact, we don't care about the result
                Assert.IsFalse(helper1.FileExists(_AsPath(FILEPATH1)), "File should not exist.");

                var helper2 = new FileSystemHelper();
                {
                    // again, just making sure this works when we are nested
                    Assert.IsFalse(helper2.FileExists(_AsPath(FILEPATH2)), "File should not exist in nested assert.");
                }
            }
        }

        /// <summary>
        ///     Test we can read and write a file successfully 
        /// </summary>
        [TestMethod]
        public void TestWriter()
        {
            var helper1 = new FileSystemHelper();
            {
                using (var bw = helper1.GetWriter(_AsPath(FILEPATH1)))
                {
                    bw.Write(TEXT1);
                }

                // this should give us an exception because we haven't created the directory
                var exception = false;

                try
                {
                    using (var bw = helper1.GetWriter(_AsPath(FILEPATH2)))
                    {
                        bw.Write(TEXT2);
                    }
                }
                catch(SterlingElevatedTrustException)
                {
                    exception = true;
                }

                Assert.IsTrue(exception, "Failed to capture an elevated trust exception from Sterling.");
            }

            Assert.IsTrue(File.Exists(_AsPath(FILEPATH1)), "File was not created.");

                using (var br = new BinaryReader(File.OpenRead(_AsPath(FILEPATH1))))
                {
                    var actual = br.ReadString();
                    Assert.AreEqual(TEXT1, actual, "Text mismatch when reading from isolated storage.");
                }

            File.Delete(_AsPath(FILEPATH1));                    
        }

        /// <summary>
        ///     Test file exists and the reader 
        /// </summary>
        [TestMethod]
        public void TestReaderAndExists()
        {
            
                using (var bw = new BinaryWriter(File.Open(_AsPath(FILEPATH1), FileMode.Create, FileAccess.Write)))
                {
                    bw.Write(TEXT1);
                }
            

            var helper = new FileSystemHelper();
            
                Assert.IsTrue(helper.FileExists(_AsPath(FILEPATH1)), "File exists for valid path failed.");
                Assert.IsFalse(helper.FileExists(_AsPath(FILEPATH2)), "False positive from helper for file path that doesn't exist.");

                using (var br = helper.GetReader(_AsPath(FILEPATH1)))
                {
                    var actual = br.ReadString();
                    Assert.AreEqual(TEXT1, actual, "Text mismatch when reading from isolated storage.");
                }
            
            
                File.Delete(_AsPath(FILEPATH1));
            
        }

        /// <summary>
        ///     Ensures that a directory exists
        /// </summary>
        [TestMethod]
        public void TestEnsureDirectory()
        {
            var helper = new FileSystemHelper();
            {
                helper.EnsureDirectory(_AsPath(PATH1));
                helper.EnsureDirectory(_AsPath(PATH1)); // second time to make sure it doesn't error out
                helper.EnsureDirectory(_AsPath(PATH2));
            }

         
                Assert.IsTrue(Directory.Exists(_AsPath(PATH2)), "Second path was not created.");
                Directory.Delete(_AsPath(PATH2));
                Directory.Delete(_AsPath(PATH1));
            
        }

        /// <summary>
        ///     Test that the directory purge works
        /// </summary>
        [TestMethod]
        public void TestPurge()
        {
                           
                Directory.CreateDirectory(_AsPath(PATH1));
                Directory.CreateDirectory(_AsPath(PATH2));

                using (var bw = new BinaryWriter(File.Open(_AsPath(FILEPATH2),FileMode.Create,FileAccess.Write)))
                {
                    bw.Write(TEXT2);
                }
            

            var helper = new FileSystemHelper();
            
                helper.Purge(_AsPath(PATH2));
            
                Assert.IsFalse(File.Exists(_AsPath(FILEPATH2)), "Purge failed: file still exists.");
                Assert.IsFalse(Directory.Exists(_AsPath(PATH2)), "Purge failed: directory still exists.");
                Assert.IsTrue(Directory.Exists(_AsPath(PATH1)), "Purge failed: higher directory no longer exists.");
                Directory.Delete(_AsPath(PATH1));               
            
        }
    }
}
