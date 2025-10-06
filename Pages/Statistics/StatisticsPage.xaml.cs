using StoDamageMeter.ViewModels;
using System.Windows.Controls;
using Microsoft.Extensions.Logging;

namespace StoDamageMeter.Pages.Statistics
{
    /// <summary>
    /// Interaction logic for StatisticsPage.xaml
    /// </summary>
    public partial class StatisticsPage : Page
    {
        private readonly ILogger<StatisticsPage> _logger;

        public StatisticsPage(ILogger<StatisticsPage> logger)
        {
            InitializeComponent();
            _logger = logger;
        }
    }
}