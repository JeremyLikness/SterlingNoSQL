namespace SterlingRecipes.Models
{
    /// <summary>
    ///     Foods
    /// </summary>
    public class FoodModel : BaseModel<IngredientModel>
    {
        public string FoodName { get; set; }        
    }
}