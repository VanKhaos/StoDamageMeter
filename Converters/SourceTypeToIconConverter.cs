using System;
using System.Globalization;
using System.Windows.Data;

namespace StoDamageMeter.Converters
{
    /// <summary>
    /// Converter um SourceType zu Icon-Namen zu konvertieren
    /// </summary>
    public class SourceTypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string sourceType)
            {
                return sourceType switch
                {
                    "player" => "User",
                    "companion" => "People",
                    "kitmodul" => "Settings",
                    _ => "User"
                };
            }
            return "User";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
