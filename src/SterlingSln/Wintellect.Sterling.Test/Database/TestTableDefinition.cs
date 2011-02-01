using System.Linq;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.Database;
using Wintellect.Sterling.IsolatedStorage;
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
            _testAccessCount = 0;
            SterlingFactory.GetPathProvider().GetDatabasePath(_testDatabase.Name); // set this up in the indices
            SterlingFactory.GetPathProvider().GetTablePath<TestModel>(_testDatabase.Name); // set up the table path
            _target = new TableDefinition<TestModel, int>(SterlingFactory.GetPathProvider(), _testDatabase.Name, new DefaultSerializer(),
                                                        _GetTestModelByKey, t => t.Key);
        }

        [TestCleanup]
        public void TestDone()
        {
            var iso = new IsoStorageHelper();
            {
                iso.Purge(PathProvider.BASE);
            }
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
