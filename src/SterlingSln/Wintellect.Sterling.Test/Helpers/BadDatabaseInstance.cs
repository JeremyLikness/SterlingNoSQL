﻿using System.Collections.Generic;
using Wintellect.Sterling.Database;

namespace Wintellect.Sterling.Test.Helpers
{
    public class BadDatabaseInstance : BaseDatabaseInstance 
    {
        /// <summary>
        ///     The name of the database instance
        /// </summary>
        public override string Name
        {
            get { return "Bad Database"; }
        }

        /// <summary>
        ///     Method called from the constructor to register tables
        /// </summary>
        /// <returns>The list of tables for the database</returns>
        protected override List<ITableDefinition> _RegisterTables()
        {
            return null; 
        }
    }
}
