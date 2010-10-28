using System;
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

namespace WindowsPhoneSterling.Sterling
{
    public class ItemDatabase : BaseDatabaseInstance 
    {

        public override string Name
        {
            get { return "ItemDatabase"; }
        }

        protected override System.Collections.Generic.List<ITableDefinition> _RegisterTables()
        {
            return new System.Collections.Generic.List<ITableDefinition>
            {
                CreateTableDefinition<ItemViewModel,int>(i=>i.Id)
            };
        }
    }
}
