using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StoDamageMeter.Converters
{
    /// <summary>
    /// Converter to show/hide weapon text blocks
    /// </summary>
    public class WeaponVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string type)
            {
                return type == "weapon" ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
