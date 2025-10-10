using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StoDamageMeter.Components.Combat
{
    public partial class CombatStatsHeader : UserControl
    {
        public event EventHandler<string>? ColumnHeaderClicked;

        private string _currentSortColumn = "DpsWithCompanions";
        private bool _sortAscending = false;

        public CombatStatsHeader()
        {
            InitializeComponent();
            UpdateColumnHeaderIndicators();
        }

        private void OnColumnHeaderClick(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button || button.Tag is not string columnName)
                return;

            // Toggle oder neue Spalte
            if (_currentSortColumn == columnName)
                _sortAscending = !_sortAscending;
            else
            {
                _currentSortColumn = columnName;
                _sortAscending = false;
            }

            UpdateColumnHeaderIndicators();
            ColumnHeaderClicked?.Invoke(this, columnName);
        }

        public void SetSortColumn(string columnName, bool ascending)
        {
            _currentSortColumn = columnName;
            _sortAscending = ascending;
            UpdateColumnHeaderIndicators();
        }

        private void UpdateColumnHeaderIndicators()
        {
            var headerButtons = new[]
            {
                (DpsHeaderButton, "DpsWithCompanions", "DPS"),
                (TotalDamageHeaderButton, "TotalDamageWithCompanions", "Total Damage"),
                (MaxHitHeaderButton, "MaxOneHit", "Max Hit"),
                (CritHeaderButton, "CritPercent", "Crit %"),
                (AttacksHeaderButton, "Attacks", "Attacks")
            };

            var starTrekBlue = new SolidColorBrush(Color.FromRgb(91, 155, 213));
            var gray = new SolidColorBrush(Color.FromRgb(176, 176, 176));

            foreach (var (button, column, baseText) in headerButtons)
            {
                bool isActive = _currentSortColumn == column;
                string arrow = isActive ? (_sortAscending ? " ▲" : " ▼") : "";
                
                button.Content = baseText + arrow;
                button.Foreground = isActive ? starTrekBlue : gray;
                button.FontWeight = isActive ? FontWeights.Bold : FontWeights.SemiBold;
            }
        }
    }
}

