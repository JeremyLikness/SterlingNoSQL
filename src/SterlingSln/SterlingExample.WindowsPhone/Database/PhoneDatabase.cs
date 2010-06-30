using System.Collections.Generic;
using System.Linq;
using SterlingExample.WindowsPhone.ViewModels;
using Wintellect.Sterling.Database;
using System;

namespace SterlingExample.WindowsPhone.Database
{
    public class PhoneDatabase : BaseDatabaseInstance 
    {
        public override string Name
        {
            get { return "Phone Database"; }
        }

        public const string INDEX = "MainIndex";

        protected override List<ITableDefinition> _RegisterTables()
        {
            return new List<ITableDefinition>
                        {
                            CreateTableDefinition<ItemViewModel, string>(i => i.LineOne)
                                .WithIndex<ItemViewModel, string, string, string>(INDEX, i => Tuple.Create(i.LineTwo,i.LineThree))
                        };
        }
        
        /// <summary>
        ///     Check to see if data has been prepared
        /// </summary>
        public void CheckData()
        {

            var database = DatabaseService.Current.Database;
            
            var count = (from keys in database.Query<ItemViewModel, string>() select keys).Count();
            
            if (count > 1) return;

            var items = new List<ItemViewModel> {
                                                        new ItemViewModel { LineOne = "runtime one", LineTwo = "Maecenas praesent accumsan bibendum", LineThree = "Facilisi faucibus habitant inceptos interdum lobortis nascetur pharetra placerat pulvinar sagittis senectus sociosqu", },
                                                        new ItemViewModel { LineOne = "runtime two", LineTwo = "Dictumst eleifend facilisi faucibus", LineThree = "Suscipit torquent ultrices vehicula volutpat maecenas praesent accumsan bibendum dictumst eleifend facilisi faucibus", },
                                                        new ItemViewModel { LineOne = "runtime three", LineTwo = "Habitant inceptos interdum lobortis", LineThree = "Habitant inceptos interdum lobortis nascetur pharetra placerat pulvinar sagittis senectus sociosqu suscipit torquent", },
                                                        new ItemViewModel { LineOne = "runtime four", LineTwo = "Nascetur pharetra placerat pulvinar", LineThree = "Ultrices vehicula volutpat maecenas praesent accumsan bibendum dictumst eleifend facilisi faucibus habitant inceptos", },
                                                        new ItemViewModel { LineOne = "runtime five", LineTwo = "Maecenas praesent accumsan bibendum", LineThree = "Maecenas praesent accumsan bibendum dictumst eleifend facilisi faucibus habitant inceptos interdum lobortis nascetur", },
                                                        new ItemViewModel { LineOne = "runtime six", LineTwo = "Dictumst eleifend facilisi faucibus", LineThree = "Pharetra placerat pulvinar sagittis senectus sociosqu suscipit torquent ultrices vehicula volutpat maecenas praesent", },
                                                        new ItemViewModel { LineOne = "runtime seven", LineTwo = "Habitant inceptos interdum lobortis", LineThree = "Accumsan bibendum dictumst eleifend facilisi faucibus habitant inceptos interdum lobortis nascetur pharetra placerat", },
                                                        new ItemViewModel { LineOne = "runtime eight", LineTwo = "Nascetur pharetra placerat pulvinar", LineThree = "Pulvinar sagittis senectus sociosqu suscipit torquent ultrices vehicula volutpat maecenas praesent accumsan bibendum", },
                                                        new ItemViewModel { LineOne = "runtime nine", LineTwo = "Maecenas praesent accumsan bibendum", LineThree = "Facilisi faucibus habitant inceptos interdum lobortis nascetur pharetra placerat pulvinar sagittis senectus sociosqu", },
                                                        new ItemViewModel { LineOne = "runtime ten", LineTwo = "Dictumst eleifend facilisi faucibus", LineThree = "Suscipit torquent ultrices vehicula volutpat maecenas praesent accumsan bibendum dictumst eleifend facilisi faucibus", },
                                                        new ItemViewModel { LineOne = "runtime eleven", LineTwo = "Habitant inceptos interdum lobortis", LineThree = "Habitant inceptos interdum lobortis nascetur pharetra placerat pulvinar sagittis senectus sociosqu suscipit torquent", },
                                                        new ItemViewModel { LineOne = "runtime twelve", LineTwo = "Nascetur pharetra placerat pulvinar", LineThree = "Ultrices vehicula volutpat maecenas praesent accumsan bibendum dictumst eleifend facilisi faucibus habitant inceptos", },
                                                        new ItemViewModel { LineOne = "runtime thirteen", LineTwo = "Maecenas praesent accumsan bibendum", LineThree = "Maecenas praesent accumsan bibendum dictumst eleifend facilisi faucibus habitant inceptos interdum lobortis nascetur", },
                                                        new ItemViewModel { LineOne = "runtime fourteen", LineTwo = "Dictumst eleifend facilisi faucibus", LineThree = "Pharetra placerat pulvinar sagittis senectus sociosqu suscipit torquent ultrices vehicula volutpat maecenas praesent", },
                                                        new ItemViewModel { LineOne = "runtime fifteen", LineTwo = "Habitant inceptos interdum lobortis", LineThree = "Accumsan bibendum dictumst eleifend facilisi faucibus habitant inceptos interdum lobortis nascetur pharetra placerat", },
                                                        new ItemViewModel { LineOne = "runtime sixteen", LineTwo = "Nascetur pharetra placerat pulvinar", LineThree = "Pulvinar sagittis senectus sociosqu suscipit torquent ultrices vehicula volutpat maecenas praesent accumsan bibendum", },
                                                    };
            foreach(var item in items)
            {
                database.Save(item);
            }
        }
    }
}
