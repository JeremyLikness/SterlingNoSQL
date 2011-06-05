using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SterlingRecipes.Contracts;
using SterlingRecipes.Models;
using UltraLight.MVVM.Contracts;

namespace SterlingRecipes.SampleData
{
    /// <summary>
    ///     Design time recipe data
    /// </summary>
    public class DesignRecipeViewModel : IRecipeViewModel
    {
        public void EditRecipe(RecipeModel recipe)
        {
            return;
        }

        public void AddRecipe()
        {
            return;
        }

        public string RecipeName
        {
            get { return "Grandma's Chicken Soup"; }
        }

        /// <summary>
        ///     Back request
        /// </summary>
        /// <returns>Used to process a back request. Return true to cancel.</returns>
        public bool CancelBackRequest()
        {
            return false;
        }


        /// <summary>
        ///     To to visual state 
        /// </summary>
        public Action<string, bool> GoToVisualState { get; set; }

        public string Title
        {
            get { return "Edit Recipe"; }
        }

        public IEnumerable<CategoryModel> Categories
        {
            get { return DesignMainViewModel.CategoryData; }
        }

        public CategoryModel RecipeCategory
        {
            get { return DesignMainViewModel.CategoryData.First(); }
        }

        public ObservableCollection<IngredientModel> Ingredients
        {
            get
            {
                var observable = new ObservableCollection<IngredientModel>
                                     {
                                         new IngredientModel
                                             {
                                                 Id = 1,
                                                 Amount = new MeasureAmountModel
                                                              {
                                                                  Units = 4,
                                                                  Measure = new MeasureModel
                                                                                {
                                                                                    Id = 1,
                                                                                    Abbreviation = "oz",
                                                                                    FullMeasure = "Ounces"
                                                                                }
                                                              },
                                                 Food = new FoodModel
                                                            {
                                                                Id = 1,
                                                                FoodName = "Chicken"
                                                            },
                                                 Order = 1
                                             },
                                         new IngredientModel
                                             {
                                                 Id = 2,
                                                 Amount = new MeasureAmountModel
                                                              {
                                                                  Units = 8,
                                                                  Measure = new MeasureModel
                                                                                {
                                                                                    Id = 2,
                                                                                    Abbreviation = "cup",
                                                                                    FullMeasure = "Cup"
                                                                                }
                                                              },
                                                 Food = new FoodModel
                                                            {
                                                                Id = 2,
                                                                FoodName = "Broth"
                                                            },
                                                 Order = 1
                                             }
                                     };
                return observable;
            }
        }

        public IngredientModel SelectedIngredient
        {
            get { return Ingredients[0]; }
        }

        public string Instructions
        {
            get
            {
                return
                    "Mix all of that stuff together. Pour in the broth (it's good and tasty). Add the chicken. Heat it up and wait until it tastes great.";
            }
        }

        public IActionCommand<object> AddIngredient
        {
            get { return null; }
        }

        public IActionCommand<IngredientModel> DeleteIngredient
        {
            get { return null; }
        }

        public IActionCommand<object> SaveRecipe
        {
            get { return null; }
        }

        public IActionCommand<object> Cancel
        {
            get { return null; }
        }

        public Action GoBack { get; set; }

        public IDialog Dialog { get; set; }
        public Action<string> RequestNavigation { get; set; }
        public IEnumerable<IActionCommand<object>> ApplicationBindings { get; set; }
    }
}