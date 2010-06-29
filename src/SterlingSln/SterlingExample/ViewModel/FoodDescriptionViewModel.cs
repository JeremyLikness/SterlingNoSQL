using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using SterlingExample.Database;
using SterlingExample.Model;

namespace SterlingExample.ViewModel
{
    /// <summary>
    ///     Food description
    /// </summary>
    public class FoodDescriptionViewModel : BaseNotify
    {
        private readonly NutrientDescription[] _nutrients = new[]
                                                                {
                                                                    new NutrientDescription
                                                                        {
                                                                            Description = "Vitamin A",
                                                                            Amount = 1000.0,
                                                                            UnitOfMeasure = "IU"
                                                                        },
                                                                    new NutrientDescription
                                                                        {
                                                                            Description = "Zinc",
                                                                            Amount = 50.0,
                                                                            UnitOfMeasure = "mg"
                                                                        },
                                                                    new NutrientDescription
                                                                        {
                                                                            Description = "Joy",
                                                                            Amount = 10000.0,
                                                                            UnitOfMeasure = "lbs"
                                                                        }
                                                                };

        private readonly ChartData[] _sampleData = new[]
                                                          {
                                                              new ChartData {DataName = "Protein", Value = 200.0},
                                                              new ChartData
                                                                  {DataName = "Carbohydrates", Value = 400.0},
                                                              new ChartData {DataName = "Fat", Value = 800.0}
                                                          };

        public FoodDescriptionViewModel()
        {
            if (DesignerProperties.IsInDesignTool)
            {
                CurrentFoodDescription = new FoodDescription
                                             {
                                                 Abbreviated = "CHOCOLATE",
                                                 CarbohydrateCalories = 900,
                                                 CommonName = string.Empty,
                                                 Description = "Chocolate, dark, delicious",
                                                 FatCalories = 900,
                                                 FoodGroupId = 2500,
                                                 Id = 999,
                                                 InedibleParts = string.Empty,
                                                 Manufacturer = "King's Chocolate",
                                                 NitrogenFactor = 2,
                                                 PctRefuse = 0.25,
                                                 ProteinCalories = 400,
                                                 ScientificName = "Chocolatus Deliciatus"
                                             };
                return;
            }

            // align with context
            FoodDescriptionContext.Current.PropertyChanged += (o, e) => CurrentFoodDescription =
                                                                        ((FoodDescriptionContext) o).
                                                                            CurrentFoodDescription;
        }

        
        private FoodDescription _foodDescription; 
        
        public FoodDescription CurrentFoodDescription
        {
            get { return _foodDescription; }
            set
            {
                _foodDescription = value;
                RaisePropertyChanged(()=>CurrentFoodDescription);
                RaisePropertyChanged(()=>Nutrients);
                RaisePropertyChanged(()=>ProCarbFats);
                RaisePropertyChanged(()=>IsNotEmpty);
            }
        }

        public bool IsNotEmpty
        {
            get { return CurrentFoodDescription != null; }
        }

        /// <summary>
        ///     Synchronize the descriptions
        /// </summary>
        public class NutrientDescription
        {
            public string Description { get; set; }
            public double Amount { get; set; }
            public string UnitOfMeasure { get; set; }
        }


        /// <summary>
        ///     The nutrients
        /// </summary>
        public IEnumerable<NutrientDescription> Nutrients
        {
            get
            {
                if (DesignerProperties.IsInDesignTool)
                {
                    return _nutrients;
                }

                if (CurrentFoodDescription == null || CurrentFoodDescription.Nutrients == null)
                {
                    return new NutrientDescription[0];
                }

                return from n in CurrentFoodDescription.Nutrients
                       join nd in
                           SterlingService.Current.Database.Query<NutrientDefinition, string, string, int>(
                               FoodDatabase.NUTR_DEFINITION_UNITS_DESC)
                           on n.NutrientDefinitionId equals nd.Key
                       join nd2 in
                           SterlingService.Current.Database.Query<NutrientDefinition, int, int>(
                               FoodDatabase.NUTR_DEFINITION_SORT)
                           on nd.Key equals nd2.Key
                       orderby nd2.Index
                       select new NutrientDescription
                                  {
                                      Amount = n.AmountPerHundredGrams,
                                      Description = nd.Index.Item2,
                                      UnitOfMeasure = nd.Index.Item1
                                  };
            }
        }

        /// <summary>
        ///     Data element
        /// </summary>
        public class ChartData
        {
            public string DataName { get; set; }
            public double Value { get; set; } 
        }

        /// <summary>
        ///     Pie chart for breakdown
        /// </summary>
        public IEnumerable<ChartData> ProCarbFats
        {
            get
            {
                if (DesignerProperties.IsInDesignTool)
                {
                    return _sampleData;
                }

                if (CurrentFoodDescription == null)
                {
                    return new ChartData[0];
                }

                var protein =
                   (from p in CurrentFoodDescription.Nutrients
                    where p.NutrientDefinitionId.Equals(203)
                    select p.AmountPerHundredGrams).FirstOrDefault();
                var carb =
                   (from p in CurrentFoodDescription.Nutrients
                    where p.NutrientDefinitionId.Equals(205)
                    select p.AmountPerHundredGrams).FirstOrDefault();
                var fat =
                   (from p in CurrentFoodDescription.Nutrients
                    where p.NutrientDefinitionId.Equals(204)
                    select p.AmountPerHundredGrams).FirstOrDefault();


                return new[]
                           {
                              
                               new ChartData {DataName = "Protein", Value = CurrentFoodDescription.ProteinCalories * protein},
                               new ChartData
                                   {DataName = "Carbohydrates", Value = CurrentFoodDescription.CarbohydrateCalories * carb},
                               new ChartData {DataName = "Fat", Value = CurrentFoodDescription.FatCalories * fat}
                           };
            }
        }
    }
}
