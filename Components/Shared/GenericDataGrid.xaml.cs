using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using StoDamageMeter.Components.Shared;
using StoDamageMeter.Models;

namespace StoDamageMeter.Components.Shared
{
    /// <summary>
    /// Generische DataGrid-Komponente für vielseitige Tabellen
    /// </summary>
    public partial class GenericDataGrid : UserControl
    {
        #region Dependency Properties

        /// <summary>
        /// ItemsSource für die DataGrid
        /// </summary>
        public static readonly DependencyProperty ItemsSourceProperty =
            DependencyProperty.Register(nameof(ItemsSource), typeof(System.Collections.IEnumerable),
                typeof(GenericDataGrid), new PropertyMetadata(null));

        public System.Collections.IEnumerable ItemsSource
        {
            get => (System.Collections.IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }

        /// <summary>
        /// Titel des Headers
        /// </summary>
        public static readonly DependencyProperty HeaderTitleProperty =
            DependencyProperty.Register(nameof(HeaderTitle), typeof(string),
                typeof(GenericDataGrid), new PropertyMetadata("Tabelle"));

        public string HeaderTitle
        {
            get => (string)GetValue(HeaderTitleProperty);
            set => SetValue(HeaderTitleProperty, value);
        }

        /// <summary>
        /// Untertitel des Headers
        /// </summary>
        public static readonly DependencyProperty HeaderSubtitleProperty =
            DependencyProperty.Register(nameof(HeaderSubtitle), typeof(string),
                typeof(GenericDataGrid), new PropertyMetadata(""));

        public string HeaderSubtitle
        {
            get => (string)GetValue(HeaderSubtitleProperty);
            set => SetValue(HeaderSubtitleProperty, value);
        }

        /// <summary>
        /// Icon für den Header
        /// </summary>
        public static readonly DependencyProperty HeaderIconProperty =
            DependencyProperty.Register(nameof(HeaderIcon), typeof(IconType),
                typeof(GenericDataGrid), new PropertyMetadata(IconType.BarChart));

        public IconType HeaderIcon
        {
            get => (IconType)GetValue(HeaderIconProperty);
            set => SetValue(HeaderIconProperty, value);
        }

        /// <summary>
        /// Spalten-Konfiguration
        /// </summary>
        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.Register(nameof(Columns), typeof(List<DataGridColumnConfig>),
                typeof(GenericDataGrid), new PropertyMetadata(new List<DataGridColumnConfig>(), OnColumnsChanged));

        public List<DataGridColumnConfig> Columns
        {
            get => (List<DataGridColumnConfig>)GetValue(ColumnsProperty);
            set => SetValue(ColumnsProperty, value);
        }

        /// <summary>
        /// Standard-Sortierung (Property-Name)
        /// </summary>
        public static readonly DependencyProperty DefaultSortPropertyNameProperty =
            DependencyProperty.Register(nameof(DefaultSortPropertyName), typeof(string),
                typeof(GenericDataGrid), new PropertyMetadata(""));

        public string DefaultSortPropertyName
        {
            get => (string)GetValue(DefaultSortPropertyNameProperty);
            set => SetValue(DefaultSortPropertyNameProperty, value);
        }

        /// <summary>
        /// Standard-Sortierungsrichtung
        /// </summary>
        public static readonly DependencyProperty DefaultSortDirectionProperty =
            DependencyProperty.Register(nameof(DefaultSortDirection), typeof(ListSortDirection),
                typeof(GenericDataGrid), new PropertyMetadata(ListSortDirection.Descending));

        public ListSortDirection DefaultSortDirection
        {
            get => (ListSortDirection)GetValue(DefaultSortDirectionProperty);
            set => SetValue(DefaultSortDirectionProperty, value);
        }

        /// <summary>
        /// Maximale Höhe der DataGrid
        /// </summary>
        public static readonly DependencyProperty MaxHeightProperty =
            DependencyProperty.Register(nameof(MaxHeight), typeof(double),
                typeof(GenericDataGrid), new PropertyMetadata(400.0));

        public double MaxHeight
        {
            get => (double)GetValue(MaxHeightProperty);
            set => SetValue(MaxHeightProperty, value);
        }

        /// <summary>
        /// Minimale Breite der DataGrid
        /// </summary>
        public static readonly DependencyProperty MinWidthProperty =
            DependencyProperty.Register(nameof(MinWidth), typeof(double),
                typeof(GenericDataGrid), new PropertyMetadata(900.0));

        public double MinWidth
        {
            get => (double)GetValue(MinWidthProperty);
            set => SetValue(MinWidthProperty, value);
        }

        /// <summary>
        /// Nachricht wenn keine Daten vorhanden
        /// </summary>
        public static readonly DependencyProperty NoDataMessageProperty =
            DependencyProperty.Register(nameof(NoDataMessage), typeof(string),
                typeof(GenericDataGrid), new PropertyMetadata("Keine Daten verfügbar"));

        public string NoDataMessage
        {
            get => (string)GetValue(NoDataMessageProperty);
            set => SetValue(NoDataMessageProperty, value);
        }

        /// <summary>
        /// Style-Variante für die gesamte Komponente
        /// </summary>
        public static readonly DependencyProperty StyleVariantProperty =
            DependencyProperty.Register(nameof(StyleVariant), typeof(DataGridStyleVariant),
                typeof(GenericDataGrid), new PropertyMetadata(DataGridStyleVariant.Default, OnStyleVariantChanged));

        public DataGridStyleVariant StyleVariant
        {
            get => (DataGridStyleVariant)GetValue(StyleVariantProperty);
            set => SetValue(StyleVariantProperty, value);
        }

        /// <summary>
        /// Header-Style-Variante
        /// </summary>
        public static readonly DependencyProperty HeaderStyleVariantProperty =
            DependencyProperty.Register(nameof(HeaderStyleVariant), typeof(DataGridHeaderStyleVariant),
                typeof(GenericDataGrid), new PropertyMetadata(DataGridHeaderStyleVariant.Default));

        public DataGridHeaderStyleVariant HeaderStyleVariant
        {
            get => (DataGridHeaderStyleVariant)GetValue(HeaderStyleVariantProperty);
            set => SetValue(HeaderStyleVariantProperty, value);
        }

        #endregion

        #region Events

        /// <summary>
        /// Event das ausgelöst wird wenn eine Zeile ausgewählt wird
        /// </summary>
        public event EventHandler<SelectionChangedEventArgs>? SelectionChanged;

        #endregion

        public GenericDataGrid()
        {
            InitializeComponent();
            Loaded += (s, e) => ApplyStyleVariant();
        }

        /// <summary>
        /// Wird aufgerufen wenn sich die Spalten-Konfiguration ändert
        /// </summary>
        private static void OnColumnsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GenericDataGrid control)
            {
                control.GenerateColumns();
            }
        }

        /// <summary>
        /// Wird aufgerufen wenn sich die Style-Variante ändert
        /// </summary>
        private static void OnStyleVariantChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GenericDataGrid control)
            {
                control.ApplyStyleVariant();
            }
        }

        /// <summary>
        /// Generiert die DataGrid-Spalten basierend auf der Konfiguration
        /// </summary>
        private void GenerateColumns()
        {
            var dataGrid = FindName("DataGrid") as DataGrid;
            if (dataGrid == null) return;

            dataGrid.Columns.Clear();

            foreach (var columnConfig in Columns)
            {
                DataGridColumn column = columnConfig.ColumnType switch
                {
                    DataGridColumnType.Text => CreateTextColumn(columnConfig),
                    DataGridColumnType.Template => CreateTemplateColumn(columnConfig),
                    DataGridColumnType.Progress => CreateProgressColumn(columnConfig),
                    _ => CreateTextColumn(columnConfig)
                };

                dataGrid.Columns.Add(column);
            }
        }

        /// <summary>
        /// Erstellt eine Text-Spalte
        /// </summary>
        private DataGridTextColumn CreateTextColumn(DataGridColumnConfig config)
        {
            var column = new DataGridTextColumn
            {
                Header = config.Header,
                Binding = new Binding(config.BindingPath) { StringFormat = config.StringFormat },
                Width = config.Width,
                ElementStyle = GetElementStyle(config.StyleType),
                HeaderStyle = GetHeaderStyle(config.HeaderAlignment)
            };

            return column;
        }

        /// <summary>
        /// Erstellt eine Template-Spalte
        /// </summary>
        private DataGridTemplateColumn CreateTemplateColumn(DataGridColumnConfig config)
        {
            var column = new DataGridTemplateColumn
            {
                Header = config.Header,
                Width = config.Width,
                HeaderStyle = GetHeaderStyle(config.HeaderAlignment)
            };

            // Erstelle CellTemplate basierend auf TemplateType
            column.CellTemplate = config.TemplateType switch
            {
                DataGridTemplateType.Badge => CreateBadgeTemplate(config),
                DataGridTemplateType.Icon => CreateIconTemplate(config),
                _ => CreateBadgeTemplate(config)
            };

            return column;
        }

        /// <summary>
        /// Erstellt eine Progress-Spalte
        /// </summary>
        private DataGridTemplateColumn CreateProgressColumn(DataGridColumnConfig config)
        {
            var column = new DataGridTemplateColumn
            {
                Header = config.Header,
                Width = config.Width,
                HeaderStyle = GetHeaderStyle(config.HeaderAlignment)
            };

            column.CellTemplate = CreateProgressTemplate(config);
            return column;
        }

        /// <summary>
        /// Erstellt ein Badge-Template
        /// </summary>
        private DataTemplate CreateBadgeTemplate(DataGridColumnConfig config)
        {
            var template = new DataTemplate();
            var factory = new FrameworkElementFactory(typeof(StarfleetBadge));

            // Für die Index-Spalte verwenden wir den RowIndexConverter
            if (config.BindingPath == "Index")
            {
                var binding = new Binding("RelativeSource={RelativeSource AncestorType=DataGridRow}, Converter={StaticResource RowIndexConverter}, ConverterParameter=Fixed");
                factory.SetBinding(StarfleetBadge.TextProperty, binding);
            }
            else
            {
                factory.SetBinding(StarfleetBadge.TextProperty, new Binding(config.BindingPath));
            }

            factory.SetValue(StarfleetBadge.VariantProperty, config.BadgeVariant);
            template.VisualTree = factory;
            return template;
        }

        /// <summary>
        /// Erstellt ein Icon-Template
        /// </summary>
        private DataTemplate CreateIconTemplate(DataGridColumnConfig config)
        {
            var template = new DataTemplate();
            var factory = new FrameworkElementFactory(typeof(StarfleetIcon));
            factory.SetBinding(StarfleetIcon.IconProperty, new Binding(config.BindingPath));
            factory.SetValue(StarfleetIcon.SizeProperty, config.IconSize);
            factory.SetBinding(StarfleetIcon.ColorProperty, new Binding(config.ColorBindingPath));
            template.VisualTree = factory;
            return template;
        }

        /// <summary>
        /// Erstellt ein Progress-Template
        /// </summary>
        private DataTemplate CreateProgressTemplate(DataGridColumnConfig config)
        {
            var template = new DataTemplate();
            var factory = new FrameworkElementFactory(typeof(StarfleetProgressBar));
            factory.SetBinding(StarfleetProgressBar.ValueProperty, new Binding(config.BindingPath));
            factory.SetBinding(StarfleetProgressBar.TextProperty, new Binding(config.TextBindingPath));
            factory.SetValue(StarfleetProgressBar.HeightProperty, config.ProgressHeight);
            template.VisualTree = factory;
            return template;
        }

        /// <summary>
        /// Holt den Element-Style basierend auf dem Style-Typ
        /// </summary>
        private Style? GetElementStyle(DataGridStyleType styleType)
        {
            return styleType switch
            {
                DataGridStyleType.Name => FindResource("DataGridNameStyle") as Style,
                DataGridStyleType.DPS => FindResource("DataGridDPStyle") as Style,
                DataGridStyleType.Average => FindResource("DataGridAverageStyle") as Style,
                DataGridStyleType.Total => FindResource("DataGridTotalStyle") as Style,
                DataGridStyleType.Critical => FindResource("DataGridCriticalStyle") as Style,
                DataGridStyleType.Usage => FindResource("DataGridUsageStyle") as Style,
                _ => null
            };
        }

        /// <summary>
        /// Holt den Header-Style basierend auf der Ausrichtung
        /// </summary>
        private Style? GetHeaderStyle(DataGridHeaderAlignment alignment)
        {
            return alignment switch
            {
                DataGridHeaderAlignment.Center => FindResource("DataGridHeaderCentering") as Style,
                DataGridHeaderAlignment.Right => FindResource("DataGridHeaderRightAlign") as Style,
                _ => null
            };
        }

        /// <summary>
        /// DataGrid Loaded Event
        /// </summary>
        private void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is DataGrid dataGrid && !string.IsNullOrEmpty(DefaultSortPropertyName))
            {
                // Sortiere nach der Standard-Property
                dataGrid.Items.SortDescriptions.Clear();
                dataGrid.Items.SortDescriptions.Add(
                    new SortDescription(DefaultSortPropertyName, DefaultSortDirection));

                // Markiere die entsprechende Spalte als sortiert
                var column = dataGrid.Columns.FirstOrDefault(c =>
                    c is DataGridTextColumn textCol &&
                    textCol.Binding is Binding binding &&
                    binding.Path.Path == DefaultSortPropertyName);

                if (column != null)
                {
                    column.SortDirection = DefaultSortDirection;
                }
            }
        }

        /// <summary>
        /// DataGrid Selection Changed Event
        /// </summary>
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectionChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Wendet die Style-Variante an
        /// </summary>
        private void ApplyStyleVariant()
        {
            var border = FindName("MainBorder") as Border;
            var headerPanel = FindName("HeaderPanel") as StackPanel;
            var dataGrid = FindName("DataGrid") as DataGrid;

            if (border == null || headerPanel == null || dataGrid == null) return;

            // Wende Card-Style basierend auf Variante an
            border.Style = GetCardStyle();

            // Wende Header-Style basierend auf Variante an
            ApplyHeaderStyle(headerPanel);

            // Wende DataGrid-Style basierend auf Variante an
            dataGrid.Style = GetDataGridStyle();
        }

        /// <summary>
        /// Holt den Card-Style basierend auf der Variante
        /// </summary>
        private Style? GetCardStyle()
        {
            return StyleVariant switch
            {
                DataGridStyleVariant.Default => FindResource("StarfleetCardStyle") as Style,
                DataGridStyleVariant.Compact => FindResource("StarfleetCardCompactStyle") as Style,
                DataGridStyleVariant.Detailed => FindResource("StarfleetCardDetailedStyle") as Style,
                DataGridStyleVariant.Minimal => FindResource("StarfleetCardMinimalStyle") as Style,
                DataGridStyleVariant.Dark => FindResource("StarfleetCardDarkStyle") as Style,
                DataGridStyleVariant.Light => FindResource("StarfleetCardLightStyle") as Style,
                DataGridStyleVariant.Accent => FindResource("StarfleetCardAccentStyle") as Style,
                DataGridStyleVariant.Gaming => FindResource("StarfleetCardGamingStyle") as Style,
                DataGridStyleVariant.Corporate => FindResource("StarfleetCardCorporateStyle") as Style,
                DataGridStyleVariant.Retro => FindResource("StarfleetCardRetroStyle") as Style,
                _ => FindResource("StarfleetCardStyle") as Style
            };
        }

        /// <summary>
        /// Wendet den Header-Style basierend auf der Variante an
        /// </summary>
        private void ApplyHeaderStyle(StackPanel headerPanel)
        {
            switch (HeaderStyleVariant)
            {
                case DataGridHeaderStyleVariant.Default:
                    // Standard Header - nichts ändern
                    break;
                case DataGridHeaderStyleVariant.Compact:
                    headerPanel.Margin = new Thickness(15, 15, 15, 8);
                    break;
                case DataGridHeaderStyleVariant.Detailed:
                    headerPanel.Margin = new Thickness(25, 25, 25, 15);
                    break;
                case DataGridHeaderStyleVariant.Minimal:
                    headerPanel.Margin = new Thickness(10, 10, 10, 5);
                    break;
                case DataGridHeaderStyleVariant.IconOnly:
                    // Verstecke Text-Elemente
                    foreach (var child in headerPanel.Children)
                    {
                        if (child is TextBlock textBlock)
                        {
                            textBlock.Visibility = Visibility.Collapsed;
                        }
                    }
                    break;
                case DataGridHeaderStyleVariant.TextOnly:
                    // Verstecke Icon-Elemente
                    foreach (var child in headerPanel.Children)
                    {
                        if (child is StackPanel iconPanel && iconPanel.Children.Count > 0)
                        {
                            var icon = iconPanel.Children[0] as StarfleetIcon;
                            if (icon != null)
                            {
                                icon.Visibility = Visibility.Collapsed;
                            }
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Holt den DataGrid-Style basierend auf der Variante
        /// </summary>
        private Style? GetDataGridStyle()
        {
            return StyleVariant switch
            {
                DataGridStyleVariant.Default => FindResource("DataGridStyle") as Style,
                DataGridStyleVariant.Compact => FindResource("DataGridCompactStyle") as Style,
                DataGridStyleVariant.Detailed => FindResource("DataGridDetailedStyle") as Style,
                DataGridStyleVariant.Minimal => FindResource("DataGridMinimalStyle") as Style,
                DataGridStyleVariant.Dark => FindResource("DataGridDarkStyle") as Style,
                DataGridStyleVariant.Light => FindResource("DataGridLightStyle") as Style,
                DataGridStyleVariant.Accent => FindResource("DataGridAccentStyle") as Style,
                DataGridStyleVariant.Gaming => FindResource("DataGridGamingStyle") as Style,
                DataGridStyleVariant.Corporate => FindResource("DataGridCorporateStyle") as Style,
                DataGridStyleVariant.Retro => FindResource("DataGridRetroStyle") as Style,
                _ => FindResource("DataGridStyle") as Style
            };
        }
    }
}
