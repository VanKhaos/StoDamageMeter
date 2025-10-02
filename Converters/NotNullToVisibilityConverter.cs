using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StoDamageMeter.Converters
{
    /// <summary>
    /// Konvertiert null zu Collapsed, sonst zu Visible
    /// </summary>
    public class NotNullToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
