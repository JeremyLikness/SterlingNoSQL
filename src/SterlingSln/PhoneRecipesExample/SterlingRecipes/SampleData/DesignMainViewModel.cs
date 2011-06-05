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
    ///     Design data for main view model
    /// </summary>
    public class DesignMainViewModel : IMainViewModel
    {
        public static readonly List<CategoryModel> CategoryData = new List<CategoryModel>
                                                                      {
                                                                          new CategoryModel
                                                                              {Id = 1, CategoryName = "Breakfast"},
                                                                          new CategoryModel
                                                                              {Id = 1, CategoryName = "Lunch"}
                                                                      };

        public DesignMainViewModel()
        {
            Categories = new ObservableCollection<CategoryModel>();
            foreach (var category in CategoryData)
            {
                Categories.Add(category);
            }
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

        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        public ObservableCollection<CategoryModel> Categories { get; private set; }

        public CategoryModel CurrentCategory
        {
            get { return Categories[1]; }
        }

        public IEnumerable<RecipeModel> Recipes
        {
            get
            {
                return new List<RecipeModel>
                           {
                               new RecipeModel {Id = 1, Name = "Grandma's Apple Cobbler"},
                               new RecipeModel {Id = 2, Name = "Mama's Chicken Noodle Soup"}
                           };
            }
        }

        public RecipeModel CurrentRecipe
        {
            get { return Recipes.First(); }
        }

        public bool IsDataLoaded
        {
            get { return true; }
        }

        public void LoadData()
        {
            return;
        }

        public IActionCommand<object> EditRecipe
        {
            get { return null; }
        }

        public IActionCommand<object> AddRecipe
        {
            get { return null; }
        }

        public IActionCommand<object> DeleteRecipe
        {
            get { return null; }
        }

        public Action GoBack { get; set; }

        public IDialog Dialog { get; set; }
        public Action<string> RequestNavigation { get; set; }
        public IEnumerable<IActionCommand<object>> ApplicationBindings { get; set; }
    }
}