using StoDamageMeter.ViewModels;
using System.Windows.Controls;

namespace StoDamageMeter.Pages
{
    /// <summary>
    /// Interaction logic for ConfigurationPage.xaml
    /// </summary>
    public partial class ConfigurationPage : Page
    {
        public ConfigurationPage(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
