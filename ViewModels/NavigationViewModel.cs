using CommunityToolkit.Mvvm.ComponentModel;

namespace StoDamageMeter.ViewModels
{
    /// <summary>
    /// ViewModel für die Navigation zwischen Seiten
    /// </summary>
    public partial class NavigationViewModel : ObservableObject
    {
        [ObservableProperty]
        private string _currentPageTitle = "Dashboard";

        [ObservableProperty]
        private string _currentPageIcon = "DataUsage24";

        /// <summary>
        /// Aktualisiert den Titel und das Icon der aktuellen Seite
        /// </summary>
        /// <param name="pageTag">Tag der Seite (z.B. "Dashboard", "Statistics")</param>
        public void UpdatePageTitle(string pageTag)
        {
            CurrentPageTitle = pageTag switch
            {
                "Dashboard" => "Dashboard",
                "LiveTracking" => "Live Tracking",
                "Statistics" => "Statistiken",
                "CustomStyles" => "Custom Styles",
                "Configuration" => "Konfiguration",
                "About" => "Über",
                _ => "Dashboard"
            };

            CurrentPageIcon = pageTag switch
            {
                "Dashboard" => "Home24",
                "LiveTracking" => "DataUsage24",
                "Statistics" => "ChartMultiple24",
                "CustomStyles" => "Color24",
                "Configuration" => "Settings24",
                "About" => "Info24",
                _ => "Home24"
            };
        }
    }
}
