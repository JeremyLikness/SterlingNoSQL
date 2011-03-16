using System;
using System.Collections.Generic;
using SterlingExample.Model;
using Wintellect.Sterling.Database;

namespace SterlingExample.Database
{
    public class FoodDatabase : BaseDatabaseInstance
    {
        /// <summary>
        ///     The name of the database instance
        /// </summary>
        public override string Name
        {
            get { return "Type Database"; }
        }

        public const string FOOD_GROUP_NAME = "FoodGroup_GroupName";
        public const string FOOD_DESCRIPTION_DESC_GROUP = "FoodDescription_Description_Group";
        public const string NUTR_DEFINITION_UNITS_DESC = "NutrientDefinition_Units_Description";
        public const string NUTR_DEFINITION_SORT = "NutrientDefinition_Sort";
        
        /// <summary>
        ///     Method called from the constructor to register tables
        /// </summary>
        /// <returns>The list of tables for the database</returns>
        protected override List<ITableDefinition> RegisterTables()
        {
            return new List<ITableDefinition>
                       {
                           CreateTableDefinition<FoodGroup, int>(fg => fg.Id)
                               .WithIndex<FoodGroup, string, int>(FOOD_GROUP_NAME, fg => fg.GroupName),
                           CreateTableDefinition<FoodDescription, int>(fd => fd.Id)
                               .WithIndex<FoodDescription, string, int, int>(FOOD_DESCRIPTION_DESC_GROUP,
                                                                                fd =>
                                                                                Tuple.Create(fd.Description, fd.FoodGroupId)),
                           CreateTableDefinition<NutrientDefinition,int>(nd=>nd.Id)
                               .WithIndex<NutrientDefinition,string,string,int>(NUTR_DEFINITION_UNITS_DESC,
                               nd=>Tuple.Create(nd.UnitOfMeasure,nd.Description))
                               .WithIndex<NutrientDefinition,int,int>(NUTR_DEFINITION_SORT,
                               nd=>nd.SortOrder)
                       };
        }
    }
}