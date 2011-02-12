namespace SterlingExample.Model
{
    /// <summary>
    ///     An element of nutrient data
    /// </summary>
    public struct NutrientDataElement
    {
        public int NutrientDefinitionId { get; set; }
        public double AmountPerHundredGrams { get; set; }
    }
}
