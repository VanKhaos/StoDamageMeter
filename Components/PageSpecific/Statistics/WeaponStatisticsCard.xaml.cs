using System.ComponentModel;
using System.Windows.Controls;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Windows;

namespace StoDamageMeter.Components.PageSpecific.Statistics
{
    /// <summary>
    /// Interaction logic for WeaponStatisticsCard.xaml
    /// </summary>
    public partial class WeaponStatisticsCard : UserControl
    {
        private readonly ILogger<WeaponStatisticsCard> _logger;

        public WeaponStatisticsCard()
        {
            InitializeComponent();
            // Einfacher Logger ohne Dependency Injection
            _logger = null; // Wir verwenden Console.WriteLine für Debugging
        }

        private void DataGrid_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                // Finde die Gesamtschaden-Spalte und sortiere standardmäßig absteigend
                foreach (var column in dataGrid.Columns)
                {
                    if (column is DataGridTextColumn textColumn &&
                        textColumn.Binding is System.Windows.Data.Binding binding &&
                        binding.Path.Path == "TotalDamage")
                    {
                        // Sortiere nach Gesamtschaden (absteigend)
                        dataGrid.Items.SortDescriptions.Clear();
                        dataGrid.Items.SortDescriptions.Add(
                            new SortDescription("TotalDamage", ListSortDirection.Descending));

                        // Markiere die Spalte als sortiert
                        column.SortDirection = ListSortDirection.Descending;
                        break;
                    }
                }
            }
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                Console.WriteLine("=== DataGrid Selection Changed ===");
                Console.WriteLine($"Selected Items Count: {dataGrid.SelectedItems.Count}");
                Console.WriteLine($"Selected Index: {dataGrid.SelectedIndex}");

                if (dataGrid.SelectedItem != null)
                {
                    Console.WriteLine($"Selected Item Type: {dataGrid.SelectedItem.GetType().Name}");

                    // Log properties of the selected item
                    var selectedItem = dataGrid.SelectedItem;
                    var properties = selectedItem.GetType().GetProperties();
                    foreach (var prop in properties)
                    {
                        try
                        {
                            var value = prop.GetValue(selectedItem);
                            Console.WriteLine($"  {prop.Name}: {value}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"  {prop.Name}: Error reading value - {ex.Message}");
                        }
                    }

                    // Log original CombatLog entries if available
                    if (selectedItem is Models.WeaponStatistics weaponStat)
                    {
                        Console.WriteLine("=== Original CombatLog Entries ===");
                        Console.WriteLine($"OriginalEntries is null: {weaponStat.OriginalEntries == null}");
                        if (weaponStat.OriginalEntries != null)
                        {
                            Console.WriteLine($"Number of original entries: {weaponStat.OriginalEntries.Count}");

                            if (weaponStat.OriginalEntries.Any())
                            {
                                // Show first few entries as examples
                                var sampleEntries = weaponStat.OriginalEntries.Take(3);
                                foreach (var entry in sampleEntries)
                                {
                                    Console.WriteLine($"  Entry: {entry.OriginalLine}");
                                    Console.WriteLine($"    SourceEntity: {entry.SourceEntity?.Name} | Tag: {entry.SourceEntity?.EntityTag}");
                                    Console.WriteLine($"    AttackName: {entry.AttackName}");
                                    Console.WriteLine($"    RawDamage: {entry.RawDamage}");
                                    Console.WriteLine($"    IsCritical: {entry.IsCritical}");
                                    Console.WriteLine($"    IsCompanionDamage: {entry.IsCompanionDamage}");
                                }

                                if (weaponStat.OriginalEntries.Count > 3)
                                {
                                    Console.WriteLine($"  ... and {weaponStat.OriginalEntries.Count - 3} more entries");
                                }
                            }
                            else
                            {
                                Console.WriteLine("  No entries in OriginalEntries list");
                            }
                        }
                        else
                        {
                            Console.WriteLine("  OriginalEntries is null!");
                        }
                        Console.WriteLine("=== End Original CombatLog Entries ===");
                    }
                }

                Console.WriteLine($"Added Items: {e.AddedItems.Count}");
                Console.WriteLine($"Removed Items: {e.RemovedItems.Count}");

                foreach (var addedItem in e.AddedItems)
                {
                    Console.WriteLine($"  Added: {addedItem?.ToString() ?? "null"}");
                }

                foreach (var removedItem in e.RemovedItems)
                {
                    Console.WriteLine($"  Removed: {removedItem?.ToString() ?? "null"}");
                }

                // Log visual state of the selected row
                if (dataGrid.SelectedItem != null)
                {
                    Console.WriteLine("=== Visual State Analysis ===");
                    var selectedIndex = dataGrid.SelectedIndex;
                    if (selectedIndex >= 0)
                    {
                        var row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(selectedIndex);
                        if (row != null)
                        {
                            Console.WriteLine($"Row Background: {row.Background}");
                            Console.WriteLine($"Row IsSelected: {row.IsSelected}");
                            Console.WriteLine($"Row IsMouseOver: {row.IsMouseOver}");

                            // Simple approach: just log that we found the row
                            Console.WriteLine($"Row found for index {selectedIndex}");
                            Console.WriteLine($"DataGrid has {dataGrid.Columns.Count} columns");

                            // Log column information and try to get cell backgrounds
                            for (int i = 0; i < dataGrid.Columns.Count; i++)
                            {
                                var column = dataGrid.Columns[i];
                                Console.WriteLine($"  Column {i}: {column.Header} (Type: {column.GetType().Name})");

                                // Try to get the cell for this column
                                try
                                {
                                    // Use a different approach to find cells
                                    var cellsPanel = FindVisualChild<DataGridCellsPanel>(row);
                                    if (cellsPanel != null && i < cellsPanel.Children.Count)
                                    {
                                        var cell = cellsPanel.Children[i] as DataGridCell;
                                        if (cell != null)
                                        {
                                            Console.WriteLine($"    Cell Background: {cell.Background}");
                                            Console.WriteLine($"    Cell IsSelected: {cell.IsSelected}");
                                        }
                                        else
                                        {
                                            Console.WriteLine($"    Cell is null for column {i}");
                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine($"    CellsPanel not found or column {i} not available");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine($"    Error getting cell for column {i}: {ex.Message}");
                                }
                            }
                        }
                        else
                        {
                            Console.WriteLine($"Row not found for index {selectedIndex}");
                        }
                    }
                }

                Console.WriteLine("=== End Selection Changed ===");
            }
        }

        private T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            if (parent == null) return null;

            for (int i = 0; i < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = System.Windows.Media.VisualTreeHelper.GetChild(parent, i);
                if (child is T result)
                {
                    return result;
                }

                var childOfChild = FindVisualChild<T>(child);
                if (childOfChild != null)
                {
                    return childOfChild;
                }
            }

            return null;
        }

    }
}
