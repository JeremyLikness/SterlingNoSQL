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
        public class ItemTrigger : BaseSterlingTrigger<ItemViewModel, int>
        {
            private int _nextId;

            public ItemTrigger(int nextId)
            {
                _nextId = nextId;
            }

            public override bool BeforeSave(ItemViewModel instance)
            {
                if (instance.Id < 1)
                {
                    instance.Id = _nextId++;
                }

                return true;
            }

            public override void AfterSave(ItemViewModel instance)
            {
                return;
            }

            public override bool BeforeDelete(int key)
            {
                return true;
            }
        }


        public override string Name
        {
            get { return "ItemDatabase"; }
        }

        protected override System.Collections.Generic.List<ITableDefinition> RegisterTables()
        {
            return new System.Collections.Generic.List<ITableDefinition>
            {
                CreateTableDefinition<ItemViewModel,int>(i=>i.Id)
            };
        }
    }
}
