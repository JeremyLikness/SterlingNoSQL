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
using Wintellect.Sterling.Test.Helpers;
using Wintellect.Sterling.IsolatedStorage;

namespace Wintellect.Sterling.Test.Database
{
    [Tag("Array")]
    [TestClass]
    public class TestArrays
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

        [TestMethod]
        public void TestNullArray()
        {
            var expected = TestClassWithArray.MakeTestClassWithArray();
            expected.BaseClassArray = null;
            expected.ClassArray = null;
            expected.ValueTypeArray = null;
            var key = _databaseInstance.Save(expected);
            var actual = _databaseInstance.Load<TestClassWithArray>(key);
            
            Assert.IsNotNull(actual, "Save/load failed: model is null.");
            Assert.AreEqual(expected.ID, actual.ID, "Save/load failed: key mismatch.");
            Assert.IsNotNull(actual.BaseClassArray, "Save/load: array is null");
            Assert.IsNotNull(actual.ClassArray, "Save/load: array is null");
            Assert.IsNotNull(actual.ValueTypeArray, "Save/load: array is null");
            Assert.AreEqual(0, actual.BaseClassArray.Length, "Save/load failed: array size mismatch.");
            Assert.AreEqual(0, actual.ClassArray.Length, "Save/load failed: array size mismatch.");
            Assert.AreEqual(0, actual.ValueTypeArray.Length, "Save/load failed: array size mismatch.");
        }

        [TestMethod]
        public void TestArray()
        {
            var expected = TestClassWithArray.MakeTestClassWithArray();
            var key = _databaseInstance.Save(expected);
            var actual = _databaseInstance.Load<TestClassWithArray>(key);
            
            Assert.IsNotNull(actual, "Save/load failed: model is null.");
            Assert.AreEqual(expected.ID, actual.ID, "Save/load failed: key mismatch.");
            Assert.IsNotNull(actual.BaseClassArray, "Save/load failed: array not initialized.");
            Assert.IsNotNull(actual.ClassArray, "Save/load failed: array not initialized.");
            Assert.IsNotNull(actual.ValueTypeArray, "Save/load failed: array not initialized.");
            Assert.AreEqual(expected.BaseClassArray.Length, actual.BaseClassArray.Length, "Save/load failed: array size mismatch.");
            Assert.AreEqual(expected.ClassArray.Length, actual.ClassArray.Length, "Save/load failed: array size mismatch.");
            Assert.AreEqual(expected.ValueTypeArray.Length, actual.ValueTypeArray.Length, "Save/load failed: array size mismatch.");
            
            for (var x = 0; x < expected.BaseClassArray.Length; x++)
            {
                Assert.AreEqual(expected.BaseClassArray[x].Key, actual.BaseClassArray[x].Key, "Save/load failed: key mismatch.");
                Assert.AreEqual(expected.BaseClassArray[x].BaseProperty, actual.BaseClassArray[x].BaseProperty, "Save/load failed: data mismatch.");
            }

            for (var x = 0; x < expected.ClassArray.Length; x++)
            {
                Assert.AreEqual(expected.ClassArray[x].Key, actual.ClassArray[x].Key, "Save/load failed: key mismatch.");
                Assert.AreEqual(expected.ClassArray[x].Data, actual.ClassArray[x].Data, "Save/load failed: data mismatch.");
            }

            for (var x = 0; x < expected.ValueTypeArray.Length; x++)
            {
                Assert.AreEqual(expected.ValueTypeArray[x], actual.ValueTypeArray[x], "Save/load failed: value mismatch.");
            }
        }
    }
}
