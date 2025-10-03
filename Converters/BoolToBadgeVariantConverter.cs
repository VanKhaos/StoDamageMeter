using System;
using System.Globalization;
using System.Windows.Data;
using StoDamageMeter.Components.Shared;

namespace StoDamageMeter.Converters
{
    /// <summary>
    /// Converter to convert boolean values to BadgeVariant
    /// </summary>
    public class BoolToBadgeVariantConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                return boolValue ? BadgeVariant.Success : BadgeVariant.Secondary;
            }
            return BadgeVariant.Secondary;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
