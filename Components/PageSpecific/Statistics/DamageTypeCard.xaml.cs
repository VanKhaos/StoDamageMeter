using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using StoDamageMeter.Models;

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
            SetupColumns();
        }

        private void SetupColumns()
        {
            var columns = new List<DataGridColumnConfig>
            {
                // # Reihenfolge
                new DataGridColumnConfig
                {
                    Header = "#",
                    BindingPath = "Index",
                    ColumnType = DataGridColumnType.Template,
                    TemplateType = DataGridTemplateType.Badge,
                    Width = new DataGridLength(50),
                    HeaderAlignment = DataGridHeaderAlignment.Center,
                           BadgeVariant = StoDamageMeter.Components.Shared.BadgeVariant.Outline
                },

                // Schadensart
                new DataGridColumnConfig
                {
                    Header = "Schadensart",
                    BindingPath = "DamageType",
                    ColumnType = DataGridColumnType.Text,
                    StyleType = DataGridStyleType.Name,
                    Width = new DataGridLength(1, DataGridLengthUnitType.Star)
                },

                // DPS
                new DataGridColumnConfig
                {
                    Header = "DPS",
                    BindingPath = "DPS",
                    ColumnType = DataGridColumnType.Text,
                    StyleType = DataGridStyleType.DPS,
                    StringFormat = "{}{0:N0}",
                    Width = new DataGridLength(80),
                    HeaderAlignment = DataGridHeaderAlignment.Right
                },

                // Durchschnittsschaden
                new DataGridColumnConfig
                {
                    Header = "Ø Schaden",
                    BindingPath = "FormattedAverageDamage",
                    ColumnType = DataGridColumnType.Text,
                    StyleType = DataGridStyleType.Average,
                    Width = new DataGridLength(100),
                    HeaderAlignment = DataGridHeaderAlignment.Right
                },

                // Gesamtschaden
                new DataGridColumnConfig
                {
                    Header = "Gesamt",
                    BindingPath = "TotalDamage",
                    ColumnType = DataGridColumnType.Text,
                    StyleType = DataGridStyleType.Total,
                    StringFormat = "{}{0:N0}",
                    Width = new DataGridLength(120),
                    HeaderAlignment = DataGridHeaderAlignment.Right
                },

                // Kritische Treffer
                new DataGridColumnConfig
                {
                    Header = "Krit. Treffer",
                    BindingPath = "CriticalHits",
                    ColumnType = DataGridColumnType.Text,
                    StyleType = DataGridStyleType.Critical,
                    Width = new DataGridLength(100),
                    HeaderAlignment = DataGridHeaderAlignment.Right
                },

                // Kritische Rate
                new DataGridColumnConfig
                {
                    Header = "Krit. Rate",
                    BindingPath = "FormattedCriticalRate",
                    ColumnType = DataGridColumnType.Text,
                    StyleType = DataGridStyleType.Critical,
                    Width = new DataGridLength(100),
                    HeaderAlignment = DataGridHeaderAlignment.Right
                },

                // Anteil
                new DataGridColumnConfig
                {
                    Header = "Anteil",
                    BindingPath = "DamageShare",
                    ColumnType = DataGridColumnType.Progress,
                    TemplateType = DataGridTemplateType.Progress,
                    TextBindingPath = "FormattedDamageShare",
                    ProgressHeight = 16,
                    Width = new DataGridLength(150)
                }
            };

            DamageTypeDataGrid.Columns = columns;
        }

        private void GenericDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Hier können spezifische Aktionen für die Schadensarten-Auswahl implementiert werden
            // z.B. Details anzeigen, Filter setzen, etc.
        }
    }
}