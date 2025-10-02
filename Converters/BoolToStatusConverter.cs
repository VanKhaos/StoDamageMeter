using System.Globalization;
using System.Windows.Data;

namespace StoDamageMeter.Converters
{
    public class BoolToStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool isWatching)
            {
                return isWatching ? "Überwacht" : "Gestoppt";
            }
            return "Unbekannt";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
