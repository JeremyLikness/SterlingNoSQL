namespace SterlingRecipes.Models
{
    /// <summary>
    ///     Ingredients
    /// </summary>
    public class IngredientModel : BaseModel<IngredientModel>
    {
        /// <summary>
        ///     Food this is for
        /// </summary>
        public FoodModel Food { get; set; }

        /// <summary>
        ///     Amount this is for
        /// </summary>
        public MeasureAmountModel Amount { get; set; }

        /// <summary>
        ///     Recipe it belongs to
        /// </summary>
        public RecipeModel Recipe { get; set; } 

        /// <summary>
        ///     Order - in the future may want to move the list around
        /// </summary>
        public int Order { get; set; }
    }
}