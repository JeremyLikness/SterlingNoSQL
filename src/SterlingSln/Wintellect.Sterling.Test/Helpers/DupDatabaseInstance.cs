using System;
using System.Collections.Generic;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Wintellect.Sterling.Database;

namespace Wintellect.Sterling.Test.Helpers
{
    public class DupDatabaseInstance : BaseDatabaseInstance 
    {
        /// <summary>
        ///     The name of the database instance
        /// </summary>
        public override string Name
        {
            get { return "Duplicate Database"; }
        }

        /// <summary>
        ///     Method called from the constructor to register tables
        /// </summary>
        /// <returns>The list of tables for the database</returns>
        protected override List<ITableDefinition> _RegisterTables()
        {
            return new List<ITableDefinition>
                       {
                           CreateTableDefinition<TestModel, int>(testModel => testModel.Key),
                           CreateTableDefinition<TestModel, string>(testModel => testModel.Data)
                       };
        }
    }
}
