using SterlingRecipes.Models;

namespace SterlingRecipes.Messages
{
    /// <summary>
    ///     Message for ingredients
    /// </summary>
    public class IngredientMessage
    {
        public IngredientAction Action { get; set; }
        public IngredientModel Model { get; set; }

        public static IngredientMessage Create(IngredientAction action, IngredientModel model)
        {
            return new IngredientMessage {Action = action, Model = model};
        }
    }
}