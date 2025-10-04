using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace StoDamageMeter.Converters
{
    /// <summary>
    /// Converter to get the row index in a DataGrid
    /// </summary>
    public class RowIndexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DataGridRow row)
            {
                return (row.GetIndex() + 1).ToString();
            }
            return "0";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
