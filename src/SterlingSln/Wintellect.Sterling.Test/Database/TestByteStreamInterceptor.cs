using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.Database;
using Wintellect.Sterling.IsolatedStorage;

namespace Wintellect.Sterling.Test.Database
{

    public class ByteStreamData
    {
        public string ID
        {
            get { return _id; }
            set { _id = value; }
        }

        public string Data
        {
            get { return _data; }
            set { _data = value; }
        }

        private string _id;
        private string _data;
    }

    public class TestByteStreamInterceptorDatabase : BaseDatabaseInstance
    {
        public class ByteInterceptor : BaseSterlingByteInterceptor
        {
            override public byte[] Save(byte[] sourceStream)
            {
                return sourceStream;
            }

            override public byte[] Load(byte[] sourceStream)
            {
                return sourceStream;
            }
        }

        public override string Name
        {
            get { return "TestByteStreamInterceptorDatabase"; }
        }

        protected override System.Collections.Generic.List<ITableDefinition> _RegisterTables()
        {
            return new System.Collections.Generic.List<ITableDefinition>
            {
                CreateTableDefinition<ByteStreamData,string>(dataDefinition => dataDefinition.ID)
            };
        }
    }

    public class ByteStreamTestIntercept : TestByteStreamInterceptorDatabase.ByteInterceptor
    {
        public override byte[] Load(byte[] sourceStream)
        {
            return sourceStream;
        }

        public override byte[] Save(byte[] sourceStream)
        {
            return sourceStream;
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
            using (var iso = new IsoStorageHelper())
            {
                iso.Purge(PathProvider.BASE);
            }
            _engine = new SterlingEngine();
            _engine.Activate();
            _databaseInstance = _engine.SterlingDatabase.RegisterDatabase<TestByteStreamInterceptorDatabase>();
        }
        [TestMethod]
        public void TestData()
        {
            const string data = "Data to be intercepted";

            ByteStreamData byteStreamData = new ByteStreamData();
            byteStreamData.ID = "data";
            byteStreamData.Data = data;


            ByteStreamTestIntercept testInterceptor = new ByteStreamTestIntercept();
            _databaseInstance.RegisterInterceptor<ByteStreamTestIntercept>(testInterceptor);

            _databaseInstance.Save<ByteStreamData>(byteStreamData);

            ByteStreamData loadedByteStreamData = _databaseInstance.Load<ByteStreamData>("data");

            Assert.AreEqual(data, loadedByteStreamData.Data);
            _databaseInstance.UnRegisterInterceptor();

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

    }
}
