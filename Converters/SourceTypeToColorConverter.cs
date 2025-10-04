using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace StoDamageMeter.Converters
{
    /// <summary>
    /// Converter um SourceType zu Farbe zu konvertieren
    /// </summary>
    public class SourceTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string sourceType)
            {
                return sourceType switch
                {
                    "player" => new SolidColorBrush(Color.FromRgb(255, 215, 0)), // Gold
                    "companion" => new SolidColorBrush(Color.FromRgb(0, 191, 255)), // Blue
                    "kitmodul" => new SolidColorBrush(Color.FromRgb(50, 205, 50)), // Green
                    _ => new SolidColorBrush(Color.FromRgb(255, 215, 0)) // Gold als Standard
                };
            }
            return new SolidColorBrush(Color.FromRgb(255, 215, 0)); // Gold als Standard
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
