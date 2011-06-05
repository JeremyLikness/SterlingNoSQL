using SterlingRecipes.Models;

namespace SterlingRecipes.Database
{
    /// <summary>
    ///     Use this to avoid foreign key, used for work in progress
    /// </summary>
    public class IngredientCacheModel
    {
        public int IngredientId;
        public MeasureAmountModel Amount;
        public FoodModel Food;
    }
}