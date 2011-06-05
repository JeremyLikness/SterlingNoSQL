namespace SterlingRecipes.Models
{
    /// <summary>
    ///     A measurement, i.e. tsp = tablespoon
    /// </summary>
    public class MeasureModel : BaseModel<MeasureModel>
    {
        public string Abbreviation { get; set; }
        public string FullMeasure { get; set; } 
    }
}