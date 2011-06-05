using System.Collections.Generic;

namespace SterlingRecipes.Models
{
    /// <summary>
    ///     A recipe
    /// </summary>
    public class RecipeModel : BaseModel<RecipeModel>
    {
        /// <summary>
        ///     Name of the recipe
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Category the recipe belongs to
        /// </summary>
        public CategoryModel Category { get; set; }

        /// <summary>
        ///     Ingredients in the recipe
        /// </summary>
        public List<IngredientModel> Ingredients { get; set; }

        /// <summary>
        ///     Instructions to put the recipe together
        /// </summary>
        public string Instructions { get; set; }
    }
}