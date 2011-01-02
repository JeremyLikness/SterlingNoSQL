using System;
using System.Collections.Generic;
using Wintellect.Sterling.Database;

namespace Wintellect.Sterling.Test.Helpers
{
    public class TestDatabaseInstance : BaseDatabaseInstance
    {
        public const string DATAINDEX = "IndexData";

        /// <summary>
        ///     The name of the database instance
        /// </summary>
        public override string Name
        {
            get { return "TestDatabase"; }
        }

        public static string GetCompositeKey(TestCompositeClass testClass)
        {
            if (testClass == null)
                return string.Empty;

            return string.Format("{0}-{1}-{2}-{3}", testClass.Key1, testClass.Key2, testClass.Key3,
                                 testClass.Key4);
        }


        /// <summary>
        ///     Method called from the constructor to register tables
        /// </summary>
        /// <returns>The list of tables for the database</returns>
        protected override List<ITableDefinition> _RegisterTables()
        {
            return new List<ITableDefinition>
                       {
                           CreateTableDefinition<TestModel, int>(testModel => testModel.Key)
                               .WithIndex<TestModel, string, int>(DATAINDEX, t => t.Data)
                               .WithIndex<TestModel, DateTime, string, int>("IndexDateData",
                                                                            t => Tuple.Create(t.Date, t.Data)),
                           CreateTableDefinition<TestForeignModel, Guid>(t => t.Key),
                           CreateTableDefinition<TestAggregateModel, string>(t => t.Key),
                           CreateTableDefinition<TestAggregateListModel, int>(t => t.ID), 
                           CreateTableDefinition<TestListModel, int>(t => t.ID),
                           CreateTableDefinition<TestDerivedClassAModel, Guid>(t => t.Key),
                           CreateTableDefinition<TestDerivedClassBModel, Guid>(t => t.Key),
                           CreateTableDefinition<TestClassWithArray, int>(t => t.ID),
                           CreateTableDefinition<TestClassWithStruct, int>(t => t.ID),
                           CreateTableDefinition<TestClassWithDictionary, int>(t => t.ID),
                           CreateTableDefinition<TestCompositeClass, string>(GetCompositeKey)
                              
                       };
        }
    }    
}
