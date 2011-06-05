namespace SterlingRecipes.Models
{
    /// <summary>
    ///     Category - Breakfast, Lunch, Dinner, Snack, Dessert
    /// </summary>
    public class CategoryModel : BaseModel<CategoryModel>
    {
        public string CategoryName { get; set; }
    }
}
