using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace StoDamageMeter.Converters
{
    /// <summary>
    /// Converter to get the row index in a DataGrid
    /// Uses the visual index instead of the data index to handle sorting and virtualization correctly
    /// Supports both single binding (DataGridRow) and multi binding (DataGridRow, DataGrid)
    /// </summary>
    public class RowIndexConverter : IValueConverter, IMultiValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DataGridRow row)
            {
                // Check if we should use fixed numbering (for DamageType table)
                bool useFixedNumbering = parameter?.ToString() == "Fixed";
                return GetRowIndex(row, null, useFixedNumbering);
            }
            return "0";
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values?.Length >= 1 && values[0] is DataGridRow row)
            {
                var dataGrid = values.Length > 1 ? values[1] as DataGrid : null;
                // Check if we should use fixed numbering (for DamageType table)
                bool useFixedNumbering = parameter?.ToString() == "Fixed";
                return GetRowIndex(row, dataGrid, useFixedNumbering);
            }
            return "0";
        }

        private string GetRowIndex(DataGridRow row, DataGrid? dataGrid, bool useFixedNumbering)
        {
            if (useFixedNumbering)
            {
                // For fixed numbering, always use the original data index
                return (row.GetIndex() + 1).ToString();
            }
            else
            {
                // For dynamic numbering, use the visual index (for WeaponStatistics table)
                dataGrid ??= FindParent<DataGrid>(row);
                
                if (dataGrid != null)
                {
                    // Get the visual index of the row in the DataGrid
                    var visualIndex = dataGrid.ItemContainerGenerator.IndexFromContainer(row);
                    if (visualIndex >= 0)
                    {
                        return (visualIndex + 1).ToString();
                    }
                }
                
                // Fallback to GetIndex() if visual index is not available
                return (row.GetIndex() + 1).ToString();
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Helper method to find parent of specific type
        /// </summary>
        private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parentObject = System.Windows.Media.VisualTreeHelper.GetParent(child);

            if (parentObject == null) return null;

            if (parentObject is T parent)
                return parent;
            else
                return FindParent<T>(parentObject);
        }
    }
}
