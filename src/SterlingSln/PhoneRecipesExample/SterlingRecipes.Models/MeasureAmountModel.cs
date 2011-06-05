namespace SterlingRecipes.Models
{
    /// <summary>
    ///     A measurement for an ingredient
    /// </summary>
    public class MeasureAmountModel
    {
        /// <summary>
        ///     Amount 
        /// </summary>
        public double Units { get; set; }

        /// <summary>
        ///     Measure type
        /// </summary>
        public MeasureModel Measure { get; set; }
    }
}