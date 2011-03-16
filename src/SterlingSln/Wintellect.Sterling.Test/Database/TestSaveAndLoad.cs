﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.Exceptions;
using Wintellect.Sterling.Test.Helpers;

namespace Wintellect.Sterling.Test.Database
{    
    [Tag("SaveAndLoad")]
    [TestClass]
    public class TestSaveAndLoad
    {
        private SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;
        private readonly ISterlingDriver _driver = new MemoryDriver();

        [TestInitialize]
        public void TestInit()
        {
            _engine = new SterlingEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<TestDatabaseInstance>(_driver);
            _databaseInstance.Purge();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _engine.Dispose();
            _databaseInstance.Purge();
            _databaseInstance = null;            
        }

        [TestMethod]
        public void TestSaveExceptions()
        {
            var raiseException = false;
            try
            {
                _databaseInstance.Save(this);
            }
            catch(SterlingTableNotFoundException)
            {
                raiseException = true;
            }

            Assert.IsTrue(raiseException, "Sterling did not raise exception for unknown type.");
        }

        [TestMethod]
        public void TestSave()
        {
            // test saving and reloading
            var expected = TestModel.MakeTestModel();

            _databaseInstance.Save(expected);

            var actual = _databaseInstance.Load<TestModel>(expected.Key);

            Assert.IsNotNull(actual, "Load failed.");

            Assert.AreEqual(expected.Key, actual.Key, "Load failed: key mismatch.");
            Assert.AreEqual(expected.Data, actual.Data, "Load failed: data mismatch.");
            Assert.IsNull(actual.Data2, "Load failed: suppressed data property not valid on de-serialize.");
            Assert.IsNotNull(actual.SubClass, "Load failed: sub class is null.");
            Assert.IsNull(actual.SubClass2, "Load failed: supressed sub class should be null.");           
            Assert.AreEqual(expected.SubClass.NestedText, actual.SubClass.NestedText, "Load failed: sub class text mismtach.");
            Assert.AreEqual(expected.SubStruct.NestedId, actual.SubStruct.NestedId, "Load failed: sub struct id mismtach.");
            Assert.AreEqual(expected.SubStruct.NestedString, actual.SubStruct.NestedString, "Load failed: sub class string mismtach.");
        }

        [TestMethod]
        public void TestSaveShutdownReInitialize()
        {
            _databaseInstance.Purge();

            // test saving and reloading
            var expected1 = TestModel.MakeTestModel();
            var expected2 = TestModel.MakeTestModel();

            expected2.GuidNullable = null;

            var expectedComplex = new TestComplexModel
                                      {
                                          Id = 5,
                                          Dict = new Dictionary<string, string>(),
                                          Models = new ObservableCollection<TestModel>()
                                      };
            for (var x = 0; x < 10; x++)
            {
                expectedComplex.Dict.Add(Guid.NewGuid().ToString(), Guid.NewGuid().ToString());
                expectedComplex.Models.Add(TestModel.MakeTestModel());
            }
            
            _databaseInstance.Save(expected1);
            _databaseInstance.Save(expected2);
            _databaseInstance.Save(expectedComplex);

            _databaseInstance.Flush();
            
            // shut it down

            _engine.Dispose();
            var driver = _databaseInstance.Driver; 
            _databaseInstance = null;

            SterlingFactory.Initialize(); // simulate an application restart

            // bring it back up
            _engine = new SterlingEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<TestDatabaseInstance>(driver);

            var actual1 = _databaseInstance.Load<TestModel>(expected1.Key);
            var actual2 = _databaseInstance.Load<TestModel>(expected2.Key);
            
            Assert.IsNotNull(actual1, "Load failed for 1.");
            Assert.AreEqual(expected1.Key, actual1.Key, "Load failed (1): key mismatch.");
            Assert.AreEqual(expected1.Data, actual1.Data, "Load failed(1): data mismatch.");
            Assert.IsNotNull(actual1.SubClass, "Load failed (1): sub class is null.");
            Assert.AreEqual(expected1.SubClass.NestedText, actual1.SubClass.NestedText, "Load failed (1): sub class text mismtach.");
            Assert.AreEqual(expected1.GuidNullable, actual1.GuidNullable, "Load failed (1): nullable Guid mismtach.");

            Assert.IsNotNull(actual2, "Load failed for 2.");
            Assert.AreEqual(expected2.Key, actual2.Key, "Load failed (2): key mismatch.");
            Assert.AreEqual(expected2.Data, actual2.Data, "Load failed (2): data mismatch.");
            Assert.IsNotNull(actual2.SubClass, "Load failed (2): sub class is null.");
            Assert.AreEqual(expected2.SubClass.NestedText, actual2.SubClass.NestedText, "Load failed (2): sub class text mismatch.");
            Assert.IsNull(expected2.GuidNullable, "Load failed (2): nullable Guid was not loaded as null.");

            //insert a third 
            var expected3 = TestModel.MakeTestModel();
            _databaseInstance.Save(expected3);

            actual1 = _databaseInstance.Load<TestModel>(expected1.Key);
            actual2 = _databaseInstance.Load<TestModel>(expected2.Key);
            var actual3 = _databaseInstance.Load<TestModel>(expected3.Key);

            Assert.IsNotNull(actual1, "Load failed for 1.");
            Assert.AreEqual(expected1.Key, actual1.Key, "Load failed (1): key mismatch.");
            Assert.AreEqual(expected1.Data, actual1.Data, "Load failed(1): data mismatch.");
            Assert.IsNotNull(actual1.SubClass, "Load failed (1): sub class is null.");
            Assert.AreEqual(expected1.SubClass.NestedText, actual1.SubClass.NestedText, "Load failed (1): sub class text mismtach.");

            Assert.IsNotNull(actual2, "Load failed for 2.");
            Assert.AreEqual(expected2.Key, actual2.Key, "Load failed (2): key mismatch.");
            Assert.AreEqual(expected2.Data, actual2.Data, "Load failed (2): data mismatch.");
            Assert.IsNotNull(actual2.SubClass, "Load failed (2): sub class is null.");
            Assert.AreEqual(expected2.SubClass.NestedText, actual2.SubClass.NestedText, "Load failed (2): sub class text mismtach.");

            Assert.IsNotNull(actual3, "Load failed for 3.");
            Assert.AreEqual(expected3.Key, actual3.Key, "Load failed (3): key mismatch.");
            Assert.AreEqual(expected3.Data, actual3.Data, "Load failed (3): data mismatch.");
            Assert.IsNotNull(actual3.SubClass, "Load failed (3): sub class is null.");
            Assert.AreEqual(expected3.SubClass.NestedText, actual3.SubClass.NestedText, "Load failed (3): sub class text mismtach.");

            // load the complex 
            var actualComplex = _databaseInstance.Load<TestComplexModel>(5);
            Assert.IsNotNull(actualComplex, "Load failed (complex): object is null.");
            Assert.AreEqual(5, actualComplex.Id, "Load failed: id mismatch.");
            Assert.IsNotNull(actualComplex.Dict, "Load failed: dictionary is null.");
            foreach(var key in expectedComplex.Dict.Keys)
            {
                var value = expectedComplex.Dict[key];
                Assert.IsTrue(actualComplex.Dict.Contains(key), "Load failed: dictionary is missing key.");
                Assert.AreEqual(value, actualComplex.Dict[key], "Load failed: dictionary has invalid value.");
            }

            Assert.IsNotNull(actualComplex.Models, "Load failed: complex missing the model collection.");

            foreach(var model in expectedComplex.Models)
            {
                var targetModel = actualComplex.Models.Where(m => m.Key.Equals(model.Key)).FirstOrDefault();
                Assert.IsNotNull(targetModel, "Load failed for nested model.");
                Assert.AreEqual(model.Key, targetModel.Key, "Load failed for nested model: key mismatch.");
                Assert.AreEqual(model.Data, targetModel.Data, "Load failed for nested model: data mismatch.");
                Assert.IsNotNull(targetModel.SubClass, "Load failed for nested model: sub class is null.");
                Assert.AreEqual(model.SubClass.NestedText, targetModel.SubClass.NestedText, "Load failed for nested model: sub class text mismtach.");
            }

        }
        
        [TestMethod]
        public void TestSaveForeign()
        {
            var expected = TestAggregateModel.MakeAggregateModel();

            _databaseInstance.Save(expected);

            var actual = _databaseInstance.Load<TestAggregateModel>(expected.Key);
            var actualTestModel = _databaseInstance.Load<TestModel>(expected.TestModelInstance.Key);
            var actualForeignModel = _databaseInstance.Load<TestForeignModel>(expected.TestForeignInstance.Key);
            var actualDerivedModel = _databaseInstance.Load<TestDerivedClassAModel>(expected.TestBaseClassInstance.Key);

            Assert.AreEqual(expected.Key, actual.Key, "Load with foreign key failed: key mismatch.");
            Assert.AreEqual(expected.TestForeignInstance.Key, actual.TestForeignInstance.Key, "Load failed: foreign key mismatch.");
            Assert.AreEqual(expected.TestForeignInstance.Data, actual.TestForeignInstance.Data, "Load failed: foreign data mismatch.");
            Assert.AreEqual(expected.TestModelInstance.Key, actual.TestModelInstance.Key, "Load failed: test model key mismatch.");
            Assert.AreEqual(expected.TestModelInstance.Data, actual.TestModelInstance.Data, "Load failed: test model data mismatch.");
            Assert.AreEqual(expected.TestForeignInstance.Key, actualForeignModel.Key, "Load failed: foreign key mismatch on direct load.");
            Assert.AreEqual(expected.TestForeignInstance.Data, actualForeignModel.Data, "Load failed: foreign data mismatch on direct load.");
            Assert.AreEqual(expected.TestModelInstance.Key, actualTestModel.Key, "Load failed: test model key mismatch on direct load.");
            Assert.AreEqual(expected.TestModelInstance.Data, actualTestModel.Data, "Load failed: test model data mismatch on direct load.");

            Assert.AreEqual(expected.TestBaseClassInstance.Key, actual.TestBaseClassInstance.Key, "Load failed: base class key mismatch.");
            Assert.AreEqual(expected.TestBaseClassInstance.BaseProperty, actual.TestBaseClassInstance.BaseProperty, "Load failed: base class data mismatch.");
            Assert.AreEqual(expected.TestBaseClassInstance.GetType(), actual.TestBaseClassInstance.GetType(), "Load failed: base class type mismatch.");
        }

        [TestMethod]
        public void TestSaveForeignNull()
        {
            var expected = TestAggregateModel.MakeAggregateModel();
            expected.TestForeignInstance = null;

            _databaseInstance.Save(expected);

            var actual = _databaseInstance.Load<TestAggregateModel>(expected.Key);
            var actualTestModel = _databaseInstance.Load<TestModel>(expected.TestModelInstance.Key);
            
            Assert.AreEqual(expected.Key, actual.Key, "Load with foreign key failed: key mismatch.");
            Assert.IsNull(actual.TestForeignInstance, "Load failed: foreign key not set to null.");
            Assert.AreEqual(expected.TestModelInstance.Key, actual.TestModelInstance.Key, "Load failed: test model key mismatch.");
            Assert.AreEqual(expected.TestModelInstance.Data, actual.TestModelInstance.Data, "Load failed: test model data mismatch.");
            Assert.AreEqual(expected.TestModelInstance.Key, actualTestModel.Key, "Load failed: test model key mismatch on direct load.");
            Assert.AreEqual(expected.TestModelInstance.Data, actualTestModel.Data, "Load failed: test model data mismatch on direct load.");
        }
    }
}
