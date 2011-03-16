using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.Database;

namespace Wintellect.Sterling.IsolatedStorage.Test.Database
{

    public class ByteStreamData
    {
        public string Id { get; set; }

        public string Data { get; set; }
    }

    public class TestByteStreamInterceptorDatabase : BaseDatabaseInstance
    {        
        public override string Name
        {
            get { return "TestByteStreamInterceptorDatabase"; }
        }

        protected override System.Collections.Generic.List<ITableDefinition> RegisterTables()
        {
            return new System.Collections.Generic.List<ITableDefinition>
            {
                CreateTableDefinition<ByteStreamData,string>(dataDefinition => dataDefinition.Id)
            };
        }
    }

    public class ByteInterceptor : BaseSterlingByteInterceptor
    {
        override public byte[] Save(byte[] sourceStream)
        {
            var retVal = new byte[sourceStream.Length];
            for (var x = 0; x < sourceStream.Length; x++)
            {
                retVal[x] = (byte)(sourceStream[x] ^ 0x80); // xor
            }
            return retVal;
        }

        override public byte[] Load(byte[] sourceStream)
        {
            var retVal = new byte[sourceStream.Length];
            for (var x = 0; x < sourceStream.Length; x++)
            {
                retVal[x] = (byte)(sourceStream[x] ^ 0x80); // xor
            }
            return retVal;
        }
    }

    public class ByteInterceptor2 : BaseSterlingByteInterceptor
    {
        override public byte[] Save(byte[] sourceStream)
        {
            var retVal = new byte[sourceStream.Length];
            for (var x = 0; x < sourceStream.Length; x++)
            {
                retVal[x] = (byte)(sourceStream[x] ^ 0x22); // xor
            }
            return retVal;
        }

        override public byte[] Load(byte[] sourceStream)
        {
            var retVal = new byte[sourceStream.Length];
            for (var x = 0; x < sourceStream.Length; x++)
            {
                retVal[x] = (byte)(sourceStream[x] ^ 0x22); // xor
            }
            return retVal;
        }
    }

    [Tag("Byte")]
    [Tag("Database")]
    [TestClass]
    public class TestByteStreamInterceptor
    {
        private SterlingEngine _engine;
        private ISterlingDatabaseInstance _databaseInstance;

        [TestInitialize]
        public void TestInit()
        {
            IsoStorageHelper.PurgeAll();
            _engine = new SterlingEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<TestByteStreamInterceptorDatabase>(new IsolatedStorageDriver());
            _databaseInstance.Purge();
        }

        [TestMethod]
        public void TestData()
        {
            const string data = "Data to be intercepted";

            var byteStreamData = new ByteStreamData {Id = "data", Data = data};

            _databaseInstance.RegisterInterceptor<ByteInterceptor>();
            _databaseInstance.RegisterInterceptor<ByteInterceptor2>();

            _databaseInstance.Save(byteStreamData);

            var loadedByteStreamData = _databaseInstance.Load<ByteStreamData>("data");

            Assert.AreEqual(data, loadedByteStreamData.Data, "Byte interceptor test failed: data does not match");

            _databaseInstance.UnRegisterInterceptor<ByteInterceptor2>();

            try
            {
                loadedByteStreamData = _databaseInstance.Load<ByteStreamData>("data");
            }
            catch
            {
                loadedByteStreamData = null;
            }

            Assert.IsTrue(loadedByteStreamData == null || !(data.Equals(loadedByteStreamData.Data)), 
                "Byte interceptor test failed: Sterling deserialized intercepted data without interceptor.");

            _databaseInstance.RegisterInterceptor<ByteInterceptor2>();

            loadedByteStreamData = _databaseInstance.Load<ByteStreamData>("data");

            Assert.AreEqual(data, loadedByteStreamData.Data, "Byte interceptor test failed: data does not match");
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _databaseInstance.Purge();
            _engine.Dispose();
            _databaseInstance = null;            
        }

    }
}
