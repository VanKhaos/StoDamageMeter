using StoDamageMeter.ViewModels;
using System.Windows.Controls;

namespace StoDamageMeter.Pages.About
{
    /// <summary>
    /// Interaction logic for AboutPage.xaml
    /// </summary>
    public partial class AboutPage : Page
    {
        public AboutPage(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
