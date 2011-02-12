using System.Collections.Generic;

namespace SterlingExample.Model
{
    public class FoodDescription
    {
        public FoodDescription()
        {
            Nutrients = new List<NutrientDataElement>();
        }

        /// <summary>
        ///     Unique id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///     The food group this food belongs to
        /// </summary>
        public int FoodGroupId { get; set; }

        /// <summary>
        ///     Description
        /// </summary>
        public string Description { get; set; }

        public string Abbreviated { get; set; }

        public string CommonName { get; set; }

        public string Manufacturer { get; set; }

        public string InedibleParts { get; set; }

        public double PctRefuse { get; set; }

        public string ScientificName { get; set; }

        public double NitrogenFactor { get; set; }

        public double ProteinCalories { get; set; }

        public double FatCalories { get; set; }

        public double CarbohydrateCalories { get; set; }

        public List<NutrientDataElement> Nutrients { get; set; }
    }
}
