using StoDamageMeter.ViewModels;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace StoDamageMeter.Pages.Dashboard
{
    /// <summary>
    /// Interaction logic for DashboardPage.xaml
    /// </summary>
    public partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            InitializeComponent();
            // DataContext wird über MainWindow gesetzt
        }
    }
}
