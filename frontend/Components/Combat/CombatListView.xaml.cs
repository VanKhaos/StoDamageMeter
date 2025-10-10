using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using StoDamageMeter.Models;

namespace StoDamageMeter.Components.Combat
{
    public partial class CombatListView : UserControl
    {
        public event EventHandler<CombatInfo>? CombatSelected;

        public ObservableCollection<CombatInfo> Combats { get; } = new();

        public CombatListView()
        {
            InitializeComponent();
            CombatListBox.ItemsSource = Combats;
        }

        private void OnCombatSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CombatListBox.SelectedItem is CombatInfo combat)
            {
                CombatSelected?.Invoke(this, combat);
            }
        }

        public void SetCombats(IEnumerable<CombatInfo> combats)
        {
            Combats.Clear();
            foreach (var combat in combats)
            {
                Combats.Add(combat);
            }
        }

        public void ClearSelection()
        {
            CombatListBox.SelectedItem = null;
        }

        public void SelectFirst()
        {
            if (Combats.Count > 0)
            {
                CombatListBox.SelectedIndex = 0;
            }
        }
    }
}

