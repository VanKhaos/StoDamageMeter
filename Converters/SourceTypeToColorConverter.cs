using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows;

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
                    "player" => Application.Current.TryFindResource("StarfleetGold") as SolidColorBrush ?? new SolidColorBrush(Color.FromRgb(255, 215, 0)),
                    "companion" => Application.Current.TryFindResource("StarfleetBlue") as SolidColorBrush ?? new SolidColorBrush(Color.FromRgb(0, 191, 255)),
                    "kitmodul" => Application.Current.TryFindResource("StarfleetGreen") as SolidColorBrush ?? new SolidColorBrush(Color.FromRgb(50, 205, 50)),
                    "npc" => Application.Current.TryFindResource("WarningOrange") as SolidColorBrush ?? new SolidColorBrush(Color.FromRgb(255, 165, 0)),
                    _ => Application.Current.TryFindResource("StarfleetGold") as SolidColorBrush ?? new SolidColorBrush(Color.FromRgb(255, 215, 0))
                };
            }
            return Application.Current.TryFindResource("StarfleetGold") as SolidColorBrush ?? new SolidColorBrush(Color.FromRgb(255, 215, 0));
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
