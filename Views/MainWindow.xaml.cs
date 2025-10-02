using StoDamageMeter.ViewModels;
using System.Windows;
using System.Windows.Controls;
using Wpf.Ui.Controls;

namespace StoDamageMeter.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private readonly MainViewModel _viewModel;

        public MainWindow(MainViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
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
            ConfigurationButton.Click += OnNavigationButtonClick;
            AboutButton.Click += OnNavigationButtonClick;
            ThemeButton.Click += OnNavigationButtonClick;

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
            switch (pageTag)
            {
                case "Dashboard":
                    ContentFrame.Navigate(new Pages.DashboardPage(_viewModel));
                    break;
                case "LiveTracking":
                    ContentFrame.Navigate(new Pages.LiveTrackingPage(_viewModel));
                    break;
                case "Statistics":
                    ContentFrame.Navigate(new Pages.StatisticsPage(_viewModel));
                    break;
                case "Configuration":
                    ContentFrame.Navigate(new Pages.ConfigurationPage(_viewModel));
                    break;
                case "About":
                    ContentFrame.Navigate(new Pages.AboutPage());
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