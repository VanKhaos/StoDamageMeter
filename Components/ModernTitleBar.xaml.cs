using System.Windows;
using System.Windows.Controls;

namespace StoDamageMeter.Components
{
    /// <summary>
    /// Interaction logic for ModernTitleBar.xaml
    /// </summary>
    public partial class ModernTitleBar : UserControl
    {
        public ModernTitleBar()
        {
            InitializeComponent();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.Close();
        }
    }
}
