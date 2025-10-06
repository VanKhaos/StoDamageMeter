using System.Windows;
using System.Windows.Controls;
using StoDamageMeter.Components.Shared;

namespace StoDamageMeter.Models
{
    /// <summary>
    /// Konfiguration für eine DataGrid-Spalte
    /// </summary>
    public class DataGridColumnConfig
    {
        /// <summary>
        /// Spalten-Header
        /// </summary>
        public string Header { get; set; } = string.Empty;

        /// <summary>
        /// Binding-Pfad für die Daten
        /// </summary>
        public string BindingPath { get; set; } = string.Empty;

        /// <summary>
        /// Spalten-Typ
        /// </summary>
        public DataGridColumnType ColumnType { get; set; } = DataGridColumnType.Text;

        /// <summary>
        /// Template-Typ (nur für Template-Spalten)
        /// </summary>
        public DataGridTemplateType TemplateType { get; set; } = DataGridTemplateType.Badge;

        /// <summary>
        /// Spalten-Breite
        /// </summary>
        public DataGridLength Width { get; set; } = new DataGridLength(1, DataGridLengthUnitType.Star);

        /// <summary>
        /// Style-Typ für Text-Spalten
        /// </summary>
        public DataGridStyleType StyleType { get; set; } = DataGridStyleType.Name;

        /// <summary>
        /// Header-Ausrichtung
        /// </summary>
        public DataGridHeaderAlignment HeaderAlignment { get; set; } = DataGridHeaderAlignment.Left;

        /// <summary>
        /// String-Format für Text-Spalten
        /// </summary>
        public string StringFormat { get; set; } = string.Empty;

        /// <summary>
        /// Badge-Variante (nur für Badge-Templates)
        /// </summary>
        public BadgeVariant BadgeVariant { get; set; } = BadgeVariant.Outline;

        /// <summary>
        /// Icon-Größe (nur für Icon-Templates)
        /// </summary>
        public double IconSize { get; set; } = 20;

        /// <summary>
        /// Color-Binding-Pfad (nur für Icon-Templates)
        /// </summary>
        public string ColorBindingPath { get; set; } = string.Empty;

        /// <summary>
        /// Text-Binding-Pfad (nur für Progress-Templates)
        /// </summary>
        public string TextBindingPath { get; set; } = string.Empty;

        /// <summary>
        /// Progress-Höhe (nur für Progress-Templates)
        /// </summary>
        public double ProgressHeight { get; set; } = 16;
    }

    /// <summary>
    /// Typen von DataGrid-Spalten
    /// </summary>
    public enum DataGridColumnType
    {
        Text,
        Template,
        Progress
    }

    /// <summary>
    /// Typen von DataGrid-Templates
    /// </summary>
    public enum DataGridTemplateType
    {
        Badge,
        Icon,
        Progress
    }

    /// <summary>
    /// Style-Typen für DataGrid-Spalten
    /// </summary>
    public enum DataGridStyleType
    {
        Name,
        DPS,
        Average,
        Total,
        Critical,
        Usage
    }

    /// <summary>
    /// Header-Ausrichtung für DataGrid-Spalten
    /// </summary>
    public enum DataGridHeaderAlignment
    {
        Left,
        Center,
        Right
    }

    /// <summary>
    /// Style-Varianten für die gesamte DataGrid-Komponente
    /// </summary>
    public enum DataGridStyleVariant
    {
        Default,        // Standard Starfleet-Style
        Compact,        // Kompakter Style für weniger Platz
        Detailed,       // Detaillierter Style mit mehr Informationen
        Minimal,        // Minimaler Style ohne Card-Border
        Dark,           // Dunkler Style
        Light,          // Heller Style
        Accent,         // Style mit Akzentfarben
        Gaming,         // Gaming-Style mit grünen Akzenten
        Corporate,      // Business-Style für professionelle Anwendungen
        Retro           // Vintage-Style für Retro-Look
    }

    /// <summary>
    /// Header-Style-Varianten
    /// </summary>
    public enum DataGridHeaderStyleVariant
    {
        Default,        // Standard Header
        Compact,        // Kompakter Header
        Detailed,       // Detaillierter Header mit mehr Info
        Minimal,        // Minimaler Header
        IconOnly,       // Nur Icon, kein Text
        TextOnly        // Nur Text, kein Icon
    }
}
