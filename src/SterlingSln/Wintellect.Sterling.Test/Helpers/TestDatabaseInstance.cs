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

        /// <summary>
        ///     Method called from the constructor to register tables
        /// </summary>
        /// <returns>The list of tables for the database</returns>
        protected override List<ITableDefinition> _RegisterTables()
        {
            return new List<ITableDefinition>
                       {
                           CreateTableDefinition<TestModel, int>(testModel => testModel.Key)
                                .WithIndex<TestModel,string,int>(DATAINDEX,t=>t.Data)
                                .WithIndex<TestModel,DateTime,string,int>("IndexDateData",t=>Tuple.Create(t.Date,t.Data)),
                           CreateTableDefinition<TestForeignModel, Guid>(t => t.Key),
                           CreateTableDefinition<TestAggregateModel, string>(t => t.Key),
                           CreateTableDefinition<TestListModel, int>(t => t.ID),
                           CreateTableDefinition<TestClassWithStruct, int>(t => t.ID)
                       };
        }
    }
}
