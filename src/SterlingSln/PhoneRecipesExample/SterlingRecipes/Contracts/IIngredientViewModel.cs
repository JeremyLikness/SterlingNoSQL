using System.Collections.Generic;
using SterlingRecipes.Models;
using UltraLight.MVVM.Contracts;

namespace SterlingRecipes.Contracts
{
    /// <summary>
    ///     Contract for ingredients
    /// </summary>
    public interface IIngredientViewModel : IViewModel
    {
        string FoodText { get; }

        string Title { get; }

        string Units { get; }

        void AddIngredient();

        void EditIngredient(IngredientModel ingredient);

        IEnumerable<FoodModel> Food { get; }

        FoodModel SelectedFood { get; }

        IEnumerable<MeasureModel> Measures { get; }

        MeasureModel SelectedMeasure { get; }

        IActionCommand<object> SaveIngredient { get; }

        IActionCommand<object> Cancel { get; }

        IActionCommand<object> AddFood { get; }

        IActionCommand<object> FoodSearch { get; }
    }
}