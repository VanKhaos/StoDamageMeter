using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using StoDamageMeter.Models;
using StoDamageMeter.Services;

namespace StoDamageMeter.ViewModels
{
    /// <summary>
    /// ViewModel für die Statistiken-Seite
    /// </summary>
    public partial class StatisticsViewModel : ObservableObject
    {
        private readonly CombatPeriodService _combatPeriodService;
        private readonly ILogger<StatisticsViewModel> _logger;

        [ObservableProperty]
        private List<CombatPeriod> _combatPeriods = new();

        [ObservableProperty]
        private CombatPeriod? _selectedPeriod;

        [ObservableProperty]
        private List<string> _availablePlayers = new();

        [ObservableProperty]
        private string _selectedPlayer = string.Empty;

        public StatisticsViewModel(CombatPeriodService combatPeriodService, ILogger<StatisticsViewModel> logger)
        {
            _combatPeriodService = combatPeriodService;
            _logger = logger;
        }

        /// <summary>
        /// Aktualisiert die verfügbaren Spieler basierend auf den Combat Log Einträgen
        /// </summary>
        public void UpdateAvailablePlayers(List<CombatLogEntry> allEntries)
        {
            if (allEntries == null)
            {
                AvailablePlayers.Clear();
                return;
            }

            // Sammle alle Spieler aus allen Einträgen
            AvailablePlayers = _combatPeriodService.GetAvailablePlayers(allEntries);

            // Automatisch ersten Spieler auswählen
            if (AvailablePlayers.Any() && string.IsNullOrEmpty(SelectedPlayer))
            {
                SelectedPlayer = AvailablePlayers.First();
                _logger.LogInformation("Ersten Spieler automatisch ausgewählt: '{PlayerName}'", SelectedPlayer);
            }
        }

        /// <summary>
        /// Aktualisiert die Kampf-Zeiträume für den ausgewählten Spieler
        /// </summary>
        public void UpdateCombatPeriods(List<CombatLogEntry> allEntries)
        {
            _lastEntries = allEntries; // Speichere für OnPropertyChanged
            _logger.LogInformation("=== UpdateCombatPeriods() gestartet ===");
            _logger.LogInformation("SelectedPlayer: '{SelectedPlayer}'", SelectedPlayer);

            if (allEntries == null || string.IsNullOrEmpty(SelectedPlayer))
            {
                CombatPeriods.Clear();
                SelectedPeriod = null;
                _logger.LogInformation("CombatPeriods geleert - keine Daten oder kein Spieler ausgewählt");
                return;
            }

            _logger.LogInformation("Gesamtanzahl aller Einträge: {TotalEntries}", allEntries.Count);

            // Zeige alle Einträge für den ausgewählten Spieler
            var playerEntries = allEntries
                .Where(e => e.PlayerInfo.CharName == SelectedPlayer && e.IsRelevant)
                .OrderBy(e => e.Timestamp)
                .ToList();

            _logger.LogInformation("Einträge für Spieler '{PlayerName}': {PlayerEntryCount}", SelectedPlayer, playerEntries.Count);

            if (playerEntries.Any())
            {
                _logger.LogInformation("Erster Eintrag: {FirstEntryTime} - {FirstEntryDamage} Schaden",
                    playerEntries.First().Timestamp, playerEntries.First().RawDamage);
                _logger.LogInformation("Letzter Eintrag: {LastEntryTime} - {LastEntryDamage} Schaden",
                    playerEntries.Last().Timestamp, playerEntries.Last().RawDamage);
            }

            // Erkenne Kampf-Zeiträume für den ausgewählten Spieler
            var allPeriods = _combatPeriodService.GetCombatPeriodsForPlayer(allEntries, SelectedPlayer);

            // Nur die letzten 10 Kämpfe anzeigen (neueste zuerst)
            CombatPeriods = allPeriods
                .OrderByDescending(p => p.StartTime)
                .Take(10)
                .ToList();

            // Explizit Property-Change Notification auslösen
            OnPropertyChanged(nameof(CombatPeriods));
            OnPropertyChanged(nameof(SelectedPeriod));

            _logger.LogInformation("CombatPeriods aktualisiert für Spieler '{PlayerName}': {TotalPeriods} Zeiträume gefunden, {DisplayedPeriods} angezeigt",
                SelectedPlayer, allPeriods.Count, CombatPeriods.Count);

            // Zeige Details aller angezeigten Zeiträume
            for (int i = 0; i < CombatPeriods.Count; i++)
            {
                var period = CombatPeriods[i];
                _logger.LogInformation("Zeitraum {Index}: {StartTime} - {EndTime} ({Duration}s) - {EntryCount} Einträge - {TotalDamage:F0} Schaden",
                    i + 1, period.StartTime, period.EndTime, period.Duration.TotalSeconds, period.EntryCount, period.TotalDamage);

                // Debug: Zeige das formatierte Datum für die UI
                var formattedDate = period.StartTime.ToString("dd.MM.yyyy | HH:mm");
                _logger.LogInformation("  → UI Format: '{FormattedDate}'", formattedDate);
            }

            // Automatisch den neuesten Zeitraum auswählen (immer, wenn sich die Liste ändert)
            if (CombatPeriods.Any())
            {
                SelectedPeriod = CombatPeriods.First();
                _logger.LogInformation("Neuesten Zeitraum automatisch ausgewählt: {StartTime}", SelectedPeriod.StartTime);
            }
            else
            {
                SelectedPeriod = null;
                _logger.LogInformation("Keine Zeiträume verfügbar - SelectedPeriod auf null gesetzt");
            }

            _logger.LogInformation("=== UpdateCombatPeriods() beendet ===");
        }

        /// <summary>
        /// Wählt einen Kampf-Zeitraum aus
        /// </summary>
        [RelayCommand]
        private void SelectPeriod(CombatPeriod period)
        {
            SelectedPeriod = period;
            _logger.LogInformation("Zeitraum ausgewählt: {StartTime} - {EndTime}", period.StartTime, period.EndTime);
            _logger.LogInformation("SelectedPeriod.Entries.Count: {EntryCount}", period.Entries.Count);

            // Explizit Property-Change Notification auslösen
            OnPropertyChanged(nameof(SelectedPeriod));
        }

        private List<CombatLogEntry>? _lastEntries;

        /// <summary>
        /// Reagiert auf Änderungen der SelectedPlayer Property
        /// </summary>
        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            // Reagiere auf Änderungen der SelectedPlayer Property
            if (e.PropertyName == nameof(SelectedPlayer) && _lastEntries != null)
            {
                _logger.LogInformation("SelectedPlayer Property geändert zu: '{SelectedPlayer}'", SelectedPlayer);
                UpdateCombatPeriods(_lastEntries);
            }
        }
    }
}
