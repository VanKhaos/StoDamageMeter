using System.Windows.Controls;

namespace StoDamageMeter.Components.Shared
{
    /// <summary>
    /// Star Trek themed DataGrid component
    /// </summary>
    public partial class StarfleetTable : UserControl
    {
        public StarfleetTable()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Gets the underlying DataGrid for direct access
        /// </summary>
        public DataGrid DataGrid => StarfleetDataGrid;
    }
}
