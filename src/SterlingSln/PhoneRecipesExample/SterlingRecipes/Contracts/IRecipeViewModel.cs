using System.Collections.Generic;
using System.Collections.ObjectModel;
using SterlingRecipes.Models;
using UltraLight.MVVM.Contracts;

namespace SterlingRecipes.Contracts
{
    /// <summary>
    ///     Contract for the recipe view model
    /// </summary>
    public interface IRecipeViewModel : IViewModel
    {
        void EditRecipe(RecipeModel recipe);

        void AddRecipe();

        string RecipeName { get; }

        string Title { get; }

        CategoryModel RecipeCategory { get; }

        IEnumerable<CategoryModel> Categories { get; }

        ObservableCollection<IngredientModel> Ingredients { get; }

        IngredientModel SelectedIngredient { get; }

        string Instructions { get; }

        IActionCommand<object> AddIngredient { get; }
        IActionCommand<IngredientModel> DeleteIngredient { get; }
        IActionCommand<object> SaveRecipe { get; }
        IActionCommand<object> Cancel { get; }
    }
}