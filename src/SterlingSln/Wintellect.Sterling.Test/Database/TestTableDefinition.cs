﻿using System.Linq;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.Database;
using Wintellect.Sterling.Serialization;
using Wintellect.Sterling.Test.Helpers;

namespace Wintellect.Sterling.Test.Database
{
    [Tag("TableDefinition")]
    [TestClass]
    public class TestTableDefinition
    {
        private readonly TestModel[] _models = new[]
                                          {
                                              TestModel.MakeTestModel(), TestModel.MakeTestModel(),
                                              TestModel.MakeTestModel()
                                          };

        private TableDefinition<TestModel, int> _target;
        private readonly ISterlingDatabaseInstance _testDatabase = new TestDatabaseInterfaceInstance();
        private int _testAccessCount;

        /// <summary>
        ///     Fetcher - also flag the fetch
        /// </summary>
        /// <param name="key">The key</param>
        /// <returns>The model</returns>
        private TestModel _GetTestModelByKey(int key)
        {
            _testAccessCount++;
            return (from t in _models where t.Key.Equals(key) select t).FirstOrDefault();
        }

        [TestInitialize]
        public void TestInit()
        {
            var serializer = new AggregateSerializer();
            serializer.AddSerializer(new DefaultSerializer());
            serializer.AddSerializer(new ExtendedSerializer());
            _testAccessCount = 0;
            _target = new TableDefinition<TestModel, int>(new MemoryDriver(_testDatabase.Name, serializer, SterlingFactory.GetLogger().Log),
                                                        _GetTestModelByKey, t => t.Key);
        }        

        [TestMethod]
        public void TestConstruction()
        {
            Assert.AreEqual(typeof(TestModel), _target.TableType, "Table type mismatch.");
            Assert.AreEqual(typeof(int), _target.KeyType, "Key type mismatch.");
            var key = _target.FetchKey(_models[1]);
            Assert.AreEqual(_models[1].Key, key, "Key mismatch after fetch key invoked.");
        }
    }
}
