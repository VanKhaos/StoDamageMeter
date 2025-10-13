using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace StoDamageMeter.Components.Shared
{
    public partial class FilterBar : UserControl
    {
        public event EventHandler<string>? FilterChanged;

        public string CurrentFilter { get; private set; } = "All";

        public FilterBar()
        {
            InitializeComponent();
        }

        private void OnFilterChanged(object sender, RoutedEventArgs e)
        {
            if (sender is not ToggleButton clickedButton || clickedButton.Tag is not string filter)
                return;

            // Deselect other buttons
            var buttons = new[] { AllFilterButton, SpaceFilterButton, GroundFilterButton };
            foreach (var btn in buttons)
            {
                if (btn != clickedButton)
                {
                    btn.IsChecked = false;
                }
            }

            // Ensure at least one is selected
            if (clickedButton.IsChecked != true)
            {
                clickedButton.IsChecked = true;
                return;
            }

            CurrentFilter = filter;
            FilterChanged?.Invoke(this, filter);
        }

        public void SetFilter(string filter)
        {
            CurrentFilter = filter;
            
            AllFilterButton.IsChecked = filter == "All";
            SpaceFilterButton.IsChecked = filter == "Space";
            GroundFilterButton.IsChecked = filter == "Ground";
        }
    }
}








