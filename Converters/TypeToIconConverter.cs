using System;
using System.Globalization;
using System.Windows.Data;
using StoDamageMeter.Components.Shared;

namespace StoDamageMeter.Converters
{
    /// <summary>
    /// Converter to convert weapon type to icon
    /// </summary>
    public class TypeToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string type)
            {
                return type switch
                {
                    "companion" => IconType.Users,
                    "weapon" => IconType.Sword,
                    _ => IconType.Sword
                };
            }
            return IconType.Sword;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
