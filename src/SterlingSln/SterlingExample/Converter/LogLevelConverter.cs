using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using Wintellect.Sterling;

namespace SterlingExample.Converter
{
    public class LogLevelConverter : IValueConverter 
    {
        /// <summary>
        /// Modifies the source data before passing it to the target for display in the UI.
        /// </summary>
        /// <returns>
        /// The value to be passed to the target dependency property.
        /// </returns>
        /// <param name="value">The source data being passed to the target.</param><param name="targetType">The <see cref="T:System.Type"/> of data expected by the target dependency property.</param><param name="parameter">An optional parameter to be used in the converter logic.</param><param name="culture">The culture of the conversion.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var brush = new SolidColorBrush(Colors.Transparent);

            if (value is SterlingLogLevel)
            {
                switch((SterlingLogLevel)value)
                {
                    case SterlingLogLevel.Verbose:
                        brush = new SolidColorBrush(Colors.LightGray);
                        break;

                    case SterlingLogLevel.Information:
                        brush = new SolidColorBrush(Colors.White);
                        break;

                    case SterlingLogLevel.Warning:
                        brush = new SolidColorBrush(Colors.Yellow);
                        break;

                    case SterlingLogLevel.Error:
                        brush = new SolidColorBrush(Colors.Red);
                        break;

                    case SterlingLogLevel.Critical:
                        brush = new SolidColorBrush(Colors.Purple);
                        break;
                }
            }

            return brush;
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
