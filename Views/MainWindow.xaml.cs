using StoDamageMeter.ViewModels;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;
using Microsoft.Extensions.DependencyInjection;

namespace StoDamageMeter.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private readonly MainViewModel _viewModel;
        private readonly IServiceProvider _serviceProvider;

        public MainWindow(MainViewModel viewModel, IServiceProvider serviceProvider)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _serviceProvider = serviceProvider;
            DataContext = viewModel;

            // Setup navigation
            SetupNavigation();
        }

        private void SetupNavigation()
        {
            // Handle navigation button clicks
            DashboardButton.Click += OnNavigationButtonClick;
            LiveTrackingButton.Click += OnNavigationButtonClick;
            StatisticsButton.Click += OnNavigationButtonClick;
            CustomStylesButton.Click += OnNavigationButtonClick;
            ConfigurationButton.Click += OnNavigationButtonClick;
            AboutButton.Click += OnNavigationButtonClick;

            // Set initial page
            NavigateToPage("Dashboard");
        }

        private void OnNavigationButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is string tag)
            {
                NavigateToPage(tag);
            }
        }

        private void NavigateToPage(string pageTag)
        {
            // Update page title and icon
            _viewModel.UpdatePageTitle(pageTag);

            switch (pageTag)
            {
                case "Dashboard":
                    var dashboardPage = _serviceProvider.GetRequiredService<Pages.Dashboard.DashboardPage>();
                    dashboardPage.DataContext = _viewModel;
                    ContentFrame.Navigate(dashboardPage);
                    break;
                case "LiveTracking":
                    var liveTrackingPage = _serviceProvider.GetRequiredService<Pages.LiveTracking.LiveTrackingPage>();
                    liveTrackingPage.DataContext = _viewModel;
                    ContentFrame.Navigate(liveTrackingPage);
                    break;
                case "Statistics":
                    var statisticsPage = _serviceProvider.GetRequiredService<Pages.Statistics.StatisticsPage>();
                    statisticsPage.DataContext = _viewModel;
                    ContentFrame.Navigate(statisticsPage);
                    break;
                case "CustomStyles":
                    var customStylesPage = _serviceProvider.GetRequiredService<Pages.CustomStyles.CustomStylesPage>();
                    customStylesPage.DataContext = _viewModel;
                    ContentFrame.Navigate(customStylesPage);
                    break;
                case "Configuration":
                    var configurationPage = _serviceProvider.GetRequiredService<Pages.Configuration.ConfigurationPage>();
                    configurationPage.DataContext = _viewModel;
                    ContentFrame.Navigate(configurationPage);
                    break;
                case "About":
                    var aboutPage = _serviceProvider.GetRequiredService<Pages.About.AboutPage>();
                    aboutPage.DataContext = _viewModel;
                    ContentFrame.Navigate(aboutPage);
                    break;
                case "Theme":
                    // Toggle theme logic here
                    break;
            }
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void TitleBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                // Double-click to maximize/restore
                WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            }
            else
            {
                // Single click to drag
                DragMove();
            }
        }
    }
}