using System;
using System.Globalization;
using System.Windows.Data;
using StoDamageMeter.Components.Shared;

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
                    "player" => IconType.User,
                    "companion" => IconType.Users,
                    "kitmodul" => IconType.Settings,
                    "npc" => IconType.Shield,
                    _ => IconType.User
                };
            }
            return IconType.User;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
