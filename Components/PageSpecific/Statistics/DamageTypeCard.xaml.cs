using System.ComponentModel;
using System.Windows.Controls;

namespace StoDamageMeter.Components.PageSpecific.Statistics
{
    /// <summary>
    /// Interaction logic for DamageTypeCard.xaml
    /// </summary>
    public partial class DamageTypeCard : UserControl
    {
        public DamageTypeCard()
        {
            InitializeComponent();
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
    }
}
