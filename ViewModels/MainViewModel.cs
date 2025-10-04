using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using StoDamageMeter.Models;
using StoDamageMeter.Services;

namespace StoDamageMeter.ViewModels
{
    /// <summary>
    /// Haupt-ViewModel das alle anderen ViewModels koordiniert
    /// </summary>
    public partial class MainViewModel : ObservableObject
    {
        private readonly ILogger<MainViewModel> _logger;

        // Child ViewModels
        public CombatLogViewModel CombatLog { get; }
        public StatisticsViewModel Statistics { get; }
        public NavigationViewModel Navigation { get; }

        public MainViewModel(
            CombatLogViewModel combatLogViewModel,
            StatisticsViewModel statisticsViewModel,
            NavigationViewModel navigationViewModel,
            ILogger<MainViewModel> logger)
        {
            CombatLog = combatLogViewModel;
            Statistics = statisticsViewModel;
            Navigation = navigationViewModel;
            _logger = logger;

            // Event-Handler für Combat Log Updates
            CombatLog.DataUpdated += OnCombatLogDataUpdated;
        }

        /// <summary>
        /// Event-Handler für Combat Log Updates - koordiniert die Child ViewModels
        /// </summary>
        private void OnCombatLogDataUpdated(object? sender, CombatLogDataUpdatedEventArgs e)
        {
            try
            {
                // Combat Log Daten aktualisiert - koordiniere Child ViewModels

                // Aktualisiere Statistics ViewModel
                if (e.Result?.Entries != null)
                {
                    Statistics.UpdateAvailablePlayers(e.Result.Entries);
                    Statistics.UpdateCombatPeriods(e.Result.Entries);
                }
                else
                {
                    _logger.LogWarning("Keine Einträge verfügbar - UpdateAvailablePlayers wird nicht aufgerufen");
                }

                // OnCombatLogDataUpdated beendet
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Koordinieren der Child ViewModels");
            }
        }

        /// <summary>
        /// Delegiert die Navigation an das NavigationViewModel
        /// </summary>
        public void UpdatePageTitle(string pageTag)
        {
            Navigation.UpdatePageTitle(pageTag);
        }
    }
}