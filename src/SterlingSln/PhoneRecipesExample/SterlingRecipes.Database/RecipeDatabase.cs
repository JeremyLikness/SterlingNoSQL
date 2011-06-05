using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using SterlingRecipes.Models;
using Wintellect.Sterling;
using Wintellect.Sterling.Database;

namespace SterlingRecipes.Database
{
    /// <summary>
    ///     Set up for the Sterling database
    /// </summary>
    public class RecipeDatabase : BaseDatabaseInstance
    {        
        /// <summary>
        ///     Index on food names
        /// </summary>
        public const string IDX_FOOD_NAME = "IDX_FoodModel_Name";

        /// <summary>
        ///     Index on recipe names
        /// </summary>
        public const string IDX_RECIPE_CATEGORYID_NAME = "IDX_RecipeModel_CategoryId_RecipeName";

        /// <summary>
        ///     Sets up the tables and corresponding keys
        /// </summary>
        /// <returns></returns>
        protected override List<ITableDefinition> RegisterTables()
        {
            return new List<ITableDefinition>
                       {
                           // tombstone helper
                           CreateTableDefinition<TombstoneModel, Type>(c => c.SyncType),
                           // ingredient on a recipe
                           CreateTableDefinition<IngredientModel, int>(i => i.Id),
                           // category 
                           CreateTableDefinition<CategoryModel, int>(c => c.Id),
                           // food item 
                           CreateTableDefinition<FoodModel, int>(f => f.Id)
                               .WithIndex<FoodModel, string, int>(IDX_FOOD_NAME, f => f.FoodName),
                           // measurement 
                           CreateTableDefinition<MeasureModel, int>(m => m.Id),
                           // recipe
                           CreateTableDefinition<RecipeModel, int>(r => r.Id)
                               .WithIndex<RecipeModel, int, string, int>(IDX_RECIPE_CATEGORYID_NAME,
                                                                         r => Tuple.Create(r.Category.Id, r.Name))
                       };
        }

        /// <summary>
        ///     Check to see if the database has been populated. If not, populate it.
        /// </summary>
        /// <param name="database">The database instance</param>
        public static void CheckAndCreate(ISterlingDatabaseInstance database)
        {
            const string FILE_MEASURES = @"/SterlingRecipes.Database;component/Measures.txt";
            const string FILE_CATEGORIES = @"/SterlingRecipes.Database;component/Categories.txt";
            const string FILE_FOOD = @"/SterlingRecipes.Database;component/Food.txt";

            // register the triggers
            database.RegisterTrigger(new IdentityTrigger<FoodModel>(database));
            database.RegisterTrigger(new IdentityTrigger<RecipeModel>(database));
            database.RegisterTrigger(new IdentityTrigger<IngredientModel>(database));

            // Categories are short so we'll do them last and check them first
            // if any are here we've already set things up because users can't delete these
            if (database.Query<CategoryModel, int>().Any()) return;

            // get rid of old data
            database.Truncate(typeof (MeasureModel));
            database.Truncate(typeof (FoodModel));

            var idx = 0;

            foreach (var measure in _ParseFromResource(FILE_MEASURES,
                                                      line =>
                                                      new MeasureModel
                                                          {Id = ++idx, Abbreviation = line[0], FullMeasure = line[1]}))
            {
                database.Save(measure);
            }

            // sample foods auto-generate the id
            foreach (var food in
                _ParseFromResource(FILE_FOOD, line => new FoodModel {FoodName = line[0]})
                    .Where(food => !string.IsNullOrEmpty(food.FoodName)))
            {
                database.Save(food);
            }

            var idx1 = 0;

            foreach (var category in _ParseFromResource(FILE_CATEGORIES,
                                                       line =>
                                                       new CategoryModel {Id = ++idx1, CategoryName = line[0]}))
            {
                database.Save(category);
            }

            // at this point the user is good, but let's give tem a sample recipe as well
            // create a sample recipe for "AB&J Sushi Rolls" used with permission from http://www.lizziemariecuisine.com          
            var recipe = new RecipeModel
                             {
                                 Category = database.Load<CategoryModel>(2),
                                 Name = "AB&J Sushi Roll",
                                 Instructions =
                                     "Remove crust and flatten bread with rolling pin by rolling bread on cutting board. Spread a thin layer of almond butter and jam onto bread. Roll up slice of bread to look like a sushi roll. Cut bread into four bite-size pieces using a serrated knife.",
                                 Ingredients = new List<IngredientModel>()
                             };

            // save to get a key
            database.Save(recipe);

            var slice = database.Load<MeasureModel>(16);
            var tbsp = database.Load<MeasureModel>(12);

            var ingredient1 = new IngredientModel
                                  {
                                      Recipe = recipe,
                                      Food = new FoodModel {FoodName = "Whole Wheat Bread"},
                                      Amount = new MeasureAmountModel
                                                   {
                                                       Units = 1.0,
                                                       Measure = slice
                                                   },
                                      Order = 1
                                  };

            // get an id
            database.Save(ingredient1);

            // add ingredients
            recipe.Ingredients.Add(ingredient1);

            var ingredient2 = new IngredientModel
                                  {
                                      Recipe = recipe,
                                      Food = new FoodModel {FoodName = "All-Natural Almond Butter"},
                                      Amount = new MeasureAmountModel
                                                   {
                                                       Units = 1.0,
                                                       Measure = tbsp
                                                   },
                                      Order = 2
                                  };

            // get an id
            database.Save(ingredient2);

            recipe.Ingredients.Add(ingredient2);

            var ingredient3 = new IngredientModel
                                  {
                                      Recipe = recipe,
                                      Food = new FoodModel {FoodName = "All-Natural Blackberry Jam"},
                                      Amount = new MeasureAmountModel
                                                   {
                                                       Units = 1.0,
                                                       Measure = tbsp
                                                   },
                                      Order = 3
                                  };

            // get an id
            database.Save(ingredient3);
            recipe.Ingredients.Add(ingredient3);
            database.Save(recipe);

            // get indexes written to disk
            database.Flush();
        }

        /// <summary>
        ///     Signature for a parser
        /// </summary>
        /// <typeparam name="T">Type to parse</typeparam>
        /// <param name="parts">Split line</param>
        /// <returns>Constructed entity</returns>
// ReSharper disable TypeParameterCanBeVariant
        private delegate T Parser<T>(string[] parts);
// ReSharper restore TypeParameterCanBeVariant

        /// <summary>
        ///     Parse a list from the resource
        /// </summary>
        /// <typeparam name="T">The type to parse</typeparam>
        /// <param name="resourceName">The embedded resource</param>
        /// <param name="parser">The parser strategy</param>
        /// <returns>The list</returns>
        private static IEnumerable<T> _ParseFromResource<T>(string resourceName, Parser<T> parser)
        {
            using (var stream = Application.GetResourceStream(new Uri(resourceName, UriKind.Relative)).Stream)
            {
                using (var reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        var line = reader.ReadLine();
                        if (string.IsNullOrEmpty(line))
                            break;
                        yield return parser(line.Split(','));
                    }
                }
            }
        }
    }
}