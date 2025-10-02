using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace StoDamageMeter.Converters
{
    /// <summary>
    /// Konvertiert String zu Visibility (leer/null -> Visible, sonst -> Collapsed)
    /// </summary>
    public class InvertStringToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                return string.IsNullOrWhiteSpace(stringValue) ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
