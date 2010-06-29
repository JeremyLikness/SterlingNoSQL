using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using SterlingExample.Database;
using SterlingExample.Model;

namespace SterlingExample.Converter
{
    /// <summary>
    ///     Gets the count of food items for a specific food group
    /// </summary>
    public class FoodGroupCountConverter : IValueConverter 
    {
        private readonly Dictionary<int,int> _foodCounts = new Dictionary<int,int>();

        /// <summary>
        /// Modifies the source data before passing it to the target for display in the UI.
        /// </summary>
        /// <returns>
        /// The value to be passed to the target dependency property.
        /// </returns>
        /// <param name="value">The source data being passed to the target.</param><param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property.</param><param name="parameter">An optional parameter to be used in the converter logic.</param><param name="culture">The culture of the conversion.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var count = DesignerProperties.IsInDesignTool ? 500 : 0;
            
            var foodGroup = value as FoodGroup;
            if (foodGroup != null && !DesignerProperties.IsInDesignTool)
            {
                if (_foodCounts.ContainsKey(foodGroup.Id))
                {
                    count = _foodCounts[foodGroup.Id];
                }
                else
                {
                    count =
                        (from index in
                             SterlingService.Current.Database.Query<FoodDescription, string, int, int>(
                                 FoodDatabase.FOOD_DESCRIPTION_DESC_GROUP)
                         where index.Index.Item2.Equals(foodGroup.Id)
                         select index).Count();
                    _foodCounts.Add(foodGroup.Id,count);
                }
            }

            return string.Format("There are {0} food items in this food group.", count);
        }

        /// <summary>
        /// Modifies the target data before passing it to the source object.  This method is called only in <see cref="F:System.Windows.Data.BindingMode.TwoWay"/> bindings.
        /// </summary>
        /// <returns>
        /// The value to be passed to the source object.
        /// </returns>
        /// <param name="value">The target data being passed to the source.</param><param name="targetType">The <see cref="T:System.Type"/> of data expected by the source object.</param><param name="parameter">An optional parameter to be used in the converter logic.</param><param name="culture">The culture of the conversion.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
