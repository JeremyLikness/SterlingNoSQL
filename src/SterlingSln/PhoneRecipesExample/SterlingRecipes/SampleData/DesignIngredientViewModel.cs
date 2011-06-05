using System;
using System.Collections.Generic;
using System.Linq;
using SterlingRecipes.Contracts;
using SterlingRecipes.Models;
using UltraLight.MVVM.Contracts;

namespace SterlingRecipes.SampleData
{
    /// <summary>
    ///     Design data for ingredients
    /// </summary>
    public class DesignIngredientViewModel : IIngredientViewModel
    {
        public string Title
        {
            get { return "Edit Ingredient"; }
        }

        public string Units
        {
            get { return "4.5"; }
        }

        public void AddIngredient()
        {
            return;
        }

        /// <summary>
        ///     To to visual state 
        /// </summary>
        public Action<string, bool> GoToVisualState { get; set; }

        /// <summary>
        ///     Back request
        /// </summary>
        /// <returns>Used to process a back request. Return true to cancel.</returns>
        public bool CancelBackRequest()
        {
            return false;
        }


        public void EditIngredient(IngredientModel ingredient)
        {
            return;
        }

        public string FoodText
        {
            get { return "almond"; }
        }

        public IEnumerable<FoodModel> Food
        {
            get
            {
                return new List<FoodModel>
                           {
                               new FoodModel {Id = 1, FoodName = "Almond Butter"},
                               new FoodModel {Id = 2, FoodName = "Almond Milk"}
                           };
            }
        }

        public FoodModel SelectedFood
        {
            get { return Food.First(); }
        }

        public IEnumerable<MeasureModel> Measures
        {
            get
            {
                return new List<MeasureModel>
                           {
                               new MeasureModel {Id = 1, Abbreviation = "oz", FullMeasure = "Ounce"},
                               new MeasureModel {Id = 2, Abbreviation = "cup", FullMeasure = "Cup"},
                               new MeasureModel {Id = 3, Abbreviation = "tsp", FullMeasure = "Teaspoon"}
                           };
            }
        }

        public MeasureModel SelectedMeasure
        {
            get { return Measures.First(); }
        }

        public IActionCommand<object> SaveIngredient
        {
            get { return null; }
        }

        public IActionCommand<object> Cancel
        {
            get { return null; }
        }

        public IActionCommand<object> AddFood
        {
            get { return null; }
        }

        public IActionCommand<object> FoodSearch
        {
            get { return null; }
        }

        public Action GoBack { get; set; }

        public IDialog Dialog { get; set; }
        public Action<string> RequestNavigation { get; set; }
        public IEnumerable<IActionCommand<object>> ApplicationBindings { get; set; }
    }
}