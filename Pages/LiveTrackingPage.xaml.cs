using StoDamageMeter.ViewModels;
using System.Windows.Controls;

namespace StoDamageMeter.Pages
{
    /// <summary>
    /// Interaction logic for LiveTrackingPage.xaml
    /// </summary>
    public partial class LiveTrackingPage : Page
    {
        public LiveTrackingPage(MainViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}
