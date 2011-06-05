using System.Collections.Generic;
using System.Collections.ObjectModel;
using SterlingRecipes.Models;
using UltraLight.MVVM.Contracts;

namespace SterlingRecipes.Contracts
{
    /// <summary>
    ///     Contract for the main view model
    /// </summary>
    public interface IMainViewModel : IViewModel
    {
        /// <summary>
        /// A collection for ItemViewModel objects.
        /// </summary>
        ObservableCollection<CategoryModel> Categories { get; }

        CategoryModel CurrentCategory { get; }

        IEnumerable<RecipeModel> Recipes { get; }

        RecipeModel CurrentRecipe { get; }

        IActionCommand<object> EditRecipe { get; }

        IActionCommand<object> AddRecipe { get; }

        IActionCommand<object> DeleteRecipe { get; }
    }
}