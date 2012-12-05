using System;
using System.Collections.Generic;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Wintellect.Sterling.Database;
using Wintellect.Sterling.Test.Helpers;

namespace Wintellect.Sterling.IsolatedStorage.Test.Database
{
    public class ChangingTypeFirstVersionDatabase : BaseDatabaseInstance
    {
        public override string Name
        {
            get
            {
                return "ChangingTypeVersion";
            }
        }

        internal override void RegisterTypeResolvers()
        {
            base.RegisterTypeResolvers();
            RegisterTypeResolver(new ChangingTypeFirstToSecondVersionResolver());
        }

        protected override List<ITableDefinition> RegisterTables()
        {
            return new List<ITableDefinition>
                           {
                               CreateTableDefinition<TestChangingTypeFirstVersionClass, string>(n => n.Key)
                           };
        }
    }

    public class ChangingTypeSecondVersionDatabase : BaseDatabaseInstance
    {
        public override string Name
        {
            get
            {
                return "ChangingTypeVersion";
            }
        }

        internal override void RegisterTypeResolvers()
        {
            base.RegisterTypeResolvers();
            RegisterTypeResolver(new ChangingTypeFirstToSecondVersionResolver());
        }

        protected override List<ITableDefinition> RegisterTables()
        {
            return new List<ITableDefinition>
                           {
                               CreateTableDefinition<TestChangingTypeSecondVersionClass, string>(n => n.Key)
                           };
        }
    }

    public class ChangingTypeFirstToSecondVersionResolver : ISterlingTypeResolver
    {
        public Type ResolveTableType(string fullTypeName)
        {
            if (fullTypeName.Contains("TestChangingTypeFirstVersionClass"))
            {
                return typeof(TestChangingTypeSecondVersionClass);
            }

            return null;
        }
    }

#if SILVERLIGHT
    [Tag("ChangingTypeVersion")]
    [Tag("Database")]
#endif
    [TestClass]
    public class TestChangingTypeVersion
    {
        private SterlingEngine _firstEngine;
        private SterlingEngine _secondEngine;
        private ISterlingDatabaseInstance _firstDatabaseInstance;
        private ISterlingDatabaseInstance _secondDatabaseInstance;

        [TestInitialize]
        public void TestInit()
        {
            _firstEngine = new SterlingEngine();
            _firstEngine.Activate();
            _firstDatabaseInstance = _firstEngine.SterlingDatabase.RegisterDatabase<ChangingTypeFirstVersionDatabase>(new IsolatedStorageDriver());
            _firstDatabaseInstance.Purge();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _firstDatabaseInstance.Purge();
            _secondDatabaseInstance.Purge();
            _firstEngine.Dispose();
            _secondEngine.Dispose();
            _firstDatabaseInstance = null;
            _secondDatabaseInstance = null;
        }

        [TestMethod]
        public void TestSaveAndLoad()
        {
            var firstVersion = TestChangingTypeFirstVersionClass.MakeChangingTypeFirstVersionClass();
            _firstDatabaseInstance.Save(firstVersion);
            _firstEngine.Dispose();

            _secondEngine = new SterlingEngine();
            _secondEngine.Activate();
            _secondDatabaseInstance = _firstEngine.SterlingDatabase.RegisterDatabase<ChangingTypeSecondVersionDatabase>(new IsolatedStorageDriver());

            var firstVersionFromUpdatedDatabase = _secondDatabaseInstance.Load<TestChangingTypeSecondVersionClass>(firstVersion.Key);
            Assert.IsNotNull(firstVersionFromUpdatedDatabase);
        }
    }
}