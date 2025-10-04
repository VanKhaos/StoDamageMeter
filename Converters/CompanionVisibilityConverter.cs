using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StoDamageMeter.Converters
{
    /// <summary>
    /// Converter to show/hide companion buttons
    /// </summary>
    public class CompanionVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string type)
            {
                return type == "companion" ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
