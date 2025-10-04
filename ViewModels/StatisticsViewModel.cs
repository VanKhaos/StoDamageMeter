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
        private readonly WeaponStatisticsService _weaponStatisticsService;
        private readonly ILogger<StatisticsViewModel> _logger;

        [ObservableProperty]
        private List<CombatPeriod> _combatPeriods = new();

        [ObservableProperty]
        private CombatPeriod? _selectedPeriod;

        [ObservableProperty]
        private List<string> _availablePlayers = new();

        [ObservableProperty]
        private string _selectedPlayer = string.Empty;

        [ObservableProperty]
        private List<WeaponStatistics> _weaponStatistics = new();

        // Neue Properties für erweiterte Statistiken
        [ObservableProperty]
        private PlayerCombatSummary? _playerSummary;

        [ObservableProperty]
        private List<DamageTypeStatistic> _damageTypeStatistics = new();


        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _loadingMessage = string.Empty;

        public StatisticsViewModel(
            CombatPeriodService combatPeriodService,
            WeaponStatisticsService weaponStatisticsService,
            ILogger<StatisticsViewModel> logger)
        {
            _combatPeriodService = combatPeriodService;
            _weaponStatisticsService = weaponStatisticsService;
            _logger = logger;
        }

        /// <summary>
        /// Aktualisiert die verfügbaren Spieler basierend auf den Combat Log Einträgen
        /// </summary>
        public void UpdateAvailablePlayers(List<CombatLogEntry> allEntries)
        {
            _logger.LogInformation("=== UpdateAvailablePlayers aufgerufen ===");
            _logger.LogInformation("Anzahl Einträge: {EntryCount}", allEntries?.Count ?? 0);

            if (allEntries == null)
            {
                AvailablePlayers.Clear();
                OnPropertyChanged(nameof(AvailablePlayers));
                return;
            }

            // Sammle alle Spieler aus allen Einträgen
            var players = _combatPeriodService.GetAvailablePlayers(allEntries);
            _logger.LogInformation("CombatPeriodService.GetAvailablePlayers() zurückgegeben: {PlayerCount} Spieler", players.Count);

            AvailablePlayers = players;

            _logger.LogInformation("Verfügbare Spieler aktualisiert: {PlayerCount} Spieler gefunden", AvailablePlayers.Count);
            foreach (var player in AvailablePlayers)
            {
                _logger.LogInformation("Spieler: '{PlayerName}'", player);
            }

            // Explizit Property-Change Notification auslösen
            OnPropertyChanged(nameof(AvailablePlayers));

            // Automatisch den Spieler mit den meisten Einträgen auswählen (wahrscheinlich der eigene Anwender)
            if (AvailablePlayers.Any() && string.IsNullOrEmpty(SelectedPlayer))
            {
                var mostActivePlayer = GetMostActivePlayer(allEntries);
                SelectedPlayer = mostActivePlayer;
                _logger.LogInformation("Spieler mit den meisten Einträgen automatisch ausgewählt: '{PlayerName}'", SelectedPlayer);
                OnPropertyChanged(nameof(SelectedPlayer));
            }

            _logger.LogInformation("=== UpdateAvailablePlayers beendet ===");
        }

        /// <summary>
        /// Ermittelt den Spieler mit den meisten Einträgen im Combatlog
        /// </summary>
        private string GetMostActivePlayer(List<CombatLogEntry> allEntries)
        {
            _logger.LogInformation("=== GetMostActivePlayer aufgerufen ===");
            _logger.LogInformation("Gesamtanzahl Einträge: {TotalEntries}", allEntries.Count);

            // Zeige die ersten 5 Rohdaten-Einträge zur Analyse
            _logger.LogInformation("=== ROHDATEN-Analyse (erste 5 Einträge) ===");
            var sampleEntries = allEntries.Take(5).ToList();
            for (int i = 0; i < sampleEntries.Count; i++)
            {
                var entry = sampleEntries[i];
                _logger.LogInformation("Rohdaten {Index}: {Timestamp} | Player: '{PlayerName}' | Attack: '{AttackName}' | Damage: {RawDamage} | IsRelevant: {IsRelevant}",
                    i + 1, entry.Timestamp, entry.PlayerInfo?.CharName ?? "NULL", entry.AttackName, entry.RawDamage, entry.IsRelevant);
            }

            var relevantEntries = allEntries.Where(e => e.IsRelevant && !string.IsNullOrEmpty(e.PlayerInfo?.CharName)).ToList();
            _logger.LogInformation("Relevante Einträge mit Spielernamen: {RelevantCount}", relevantEntries.Count);

            var playerEntryCounts = relevantEntries
                .GroupBy(e => e.PlayerInfo.CharName)
                .Select(g => new
                {
                    PlayerName = g.Key,
                    EntryCount = g.Count(),
                    TotalDamage = g.Sum(e => e.RawDamage)
                })
                .OrderByDescending(p => p.EntryCount)
                .ThenByDescending(p => p.TotalDamage)
                .ToList();

            _logger.LogInformation("=== Spieler-Aktivitäts-Analyse ===");
            _logger.LogInformation("Gefundene Spieler: {PlayerCount}", playerEntryCounts.Count);
            foreach (var player in playerEntryCounts)
            {
                _logger.LogInformation("  {PlayerName}: {EntryCount} Einträge, {TotalDamage:N0} Schaden",
                    player.PlayerName, player.EntryCount, player.TotalDamage);
            }

            var mostActivePlayer = playerEntryCounts.FirstOrDefault()?.PlayerName ?? AvailablePlayers.FirstOrDefault() ?? string.Empty;

            _logger.LogInformation("Meist aktiver Spieler: '{MostActivePlayer}'", mostActivePlayer);
            _logger.LogInformation("=== GetMostActivePlayer beendet ===");

            return mostActivePlayer;
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

                // Aktualisiere alle Statistiken für den automatisch ausgewählten Zeitraum
                UpdateWeaponStatistics();
                UpdatePlayerSummary();
                UpdateDamageTypeStatistics();
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
        private async Task SelectPeriod(CombatPeriod period)
        {
            IsLoading = true;
            LoadingMessage = "Analysiere Kampfdaten...";

            try
            {
                SelectedPeriod = period;
                _logger.LogInformation("Zeitraum ausgewählt: {StartTime} - {EndTime}", period.StartTime, period.EndTime);
                _logger.LogInformation("SelectedPeriod.Entries.Count: {EntryCount}", period.Entries.Count);

                // Aktualisiere alle Statistiken für den ausgewählten Zeitraum
                await Task.Run(() =>
                {
                    UpdateWeaponStatistics();
                    UpdatePlayerSummary();
                    UpdateDamageTypeStatistics();
                });

                // Explizit Property-Change Notification auslösen
                OnPropertyChanged(nameof(SelectedPeriod));
            }
            finally
            {
                IsLoading = false;
                LoadingMessage = string.Empty;
            }
        }

        /// <summary>
        /// Aktualisiert die Waffen-Statistiken basierend auf dem ausgewählten Zeitraum
        /// </summary>
        private void UpdateWeaponStatistics()
        {
            if (SelectedPeriod == null || string.IsNullOrEmpty(SelectedPlayer))
            {
                WeaponStatistics.Clear();
                _logger.LogInformation("Waffen-Statistiken geleert - kein Zeitraum oder Spieler ausgewählt");
                return;
            }

            try
            {
                _logger.LogInformation("=== UpdateWeaponStatistics für Spieler '{PlayerName}' ===", SelectedPlayer);
                _logger.LogInformation("SelectedPeriod.Entries.Count: {EntryCount}", SelectedPeriod.Entries.Count);
                _logger.LogInformation("SelectedPeriod.Duration: {Duration}", SelectedPeriod.Duration);

                // Zeige die ersten 10 Rohdaten-Einträge
                _logger.LogInformation("=== ROHDATEN (erste 10 Einträge) ===");
                var sampleEntries = SelectedPeriod.Entries.Take(10).ToList();
                for (int i = 0; i < sampleEntries.Count; i++)
                {
                    var entry = sampleEntries[i];
                    _logger.LogInformation("Eintrag {Index}: {Timestamp} | {PlayerName} | {AttackName} | {DamageType} | {RawDamage} | {IsRelevant}",
                        i + 1, entry.Timestamp, entry.PlayerInfo?.CharName ?? "N/A", entry.AttackName, entry.DamageType, entry.RawDamage, entry.IsRelevant);
                }

                // Filtere Einträge für den ausgewählten Spieler
                var playerEntries = SelectedPeriod.Entries
                    .Where(e => e.PlayerInfo?.CharName == SelectedPlayer && e.IsRelevant)
                    .ToList();

                _logger.LogInformation("=== GEFILTERTE Einträge für Spieler '{PlayerName}' ===", SelectedPlayer);
                _logger.LogInformation("Gefilterte Einträge: {PlayerEntryCount}", playerEntries.Count);

                // Zeige die ersten 10 gefilterten Einträge
                var samplePlayerEntries = playerEntries.Take(10).ToList();
                for (int i = 0; i < samplePlayerEntries.Count; i++)
                {
                    var entry = samplePlayerEntries[i];
                    _logger.LogInformation("Gefiltert {Index}: {Timestamp} | {AttackName} | {DamageType} | {RawDamage}",
                        i + 1, entry.Timestamp, entry.AttackName, entry.DamageType, entry.RawDamage);
                }

                var duration = SelectedPeriod.Duration.TotalSeconds;
                var weaponStats = _weaponStatisticsService.GetPlayerWeaponStatistics(
                    SelectedPeriod.Entries,
                    SelectedPlayer,
                    duration);

                WeaponStatistics = weaponStats;

                _logger.LogInformation("=== FINALE Waffen-Statistiken ===");
                _logger.LogInformation("Waffen-Statistiken aktualisiert: {WeaponCount} Waffen für Spieler '{PlayerName}'",
                    weaponStats.Count, SelectedPlayer);

                // Zeige Details der generierten Waffen-Statistiken
                foreach (var weapon in weaponStats)
                {
                    _logger.LogInformation("Waffe: {Name} | Typ: {Type} | Schaden: {TotalDamage} | DPS: {DPS} | Verwendungen: {TotalUses}",
                        weapon.Name, weapon.Type, weapon.TotalDamage, weapon.DPS, weapon.TotalUses);
                }

                _logger.LogInformation("=== Ende UpdateWeaponStatistics ===");

                // Explizit Property-Change Notification auslösen
                OnPropertyChanged(nameof(WeaponStatistics));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Aktualisieren der Waffen-Statistiken");
                WeaponStatistics.Clear();
            }
        }

        /// <summary>
        /// Aktualisiert die Spieler-Zusammenfassung
        /// </summary>
        private void UpdatePlayerSummary()
        {
            if (SelectedPeriod == null || string.IsNullOrEmpty(SelectedPlayer))
            {
                PlayerSummary = null;
                return;
            }

            var playerEntries = SelectedPeriod.Entries
                .Where(e => e.PlayerInfo.CharName == SelectedPlayer && e.IsRelevant)
                .ToList();

            if (!playerEntries.Any())
            {
                PlayerSummary = null;
                return;
            }

            var totalDamage = playerEntries.Sum(e => e.RawDamage);
            var criticalHits = playerEntries.Count(e => e.IsCritical);
            var totalHits = playerEntries.Count;
            var duration = SelectedPeriod.Duration.TotalSeconds;
            var dps = duration > 0 ? totalDamage / duration : 0;
            var criticalRate = totalHits > 0 ? (double)criticalHits / totalHits * 100 : 0;

            PlayerSummary = new PlayerCombatSummary
            {
                PlayerName = SelectedPlayer,
                TotalDamage = totalDamage,
                DPS = dps,
                CriticalHits = criticalHits,
                TotalHits = totalHits,
                CriticalRate = criticalRate,
                AverageDamage = totalHits > 0 ? totalDamage / totalHits : 0,
                CombatDuration = SelectedPeriod.Duration,
                StartTime = SelectedPeriod.StartTime,
                EndTime = SelectedPeriod.EndTime
            };

            OnPropertyChanged(nameof(PlayerSummary));
        }

        /// <summary>
        /// Aktualisiert die Schadensarten-Statistiken
        /// </summary>
        private void UpdateDamageTypeStatistics()
        {
            if (SelectedPeriod == null || string.IsNullOrEmpty(SelectedPlayer))
            {
                DamageTypeStatistics.Clear();
                return;
            }

            // Alle Einträge für den Spieler (sowohl Treffer als auch Verfehlte)
            var allPlayerEntries = SelectedPeriod.Entries
                .Where(e => e.PlayerInfo.CharName == SelectedPlayer)
                .ToList();

            // Nur Treffer (relevante Einträge mit Schaden > 0)
            var hitEntries = allPlayerEntries
                .Where(e => e.IsRelevant)
                .ToList();

            // Verfehlte Angriffe (Einträge mit Schaden = 0)
            var missEntries = allPlayerEntries
                .Where(e => !e.IsRelevant && e.RawDamage == 0)
                .ToList();

            var totalDamage = hitEntries.Sum(e => e.RawDamage);
            var combatDuration = SelectedPeriod.Duration.TotalSeconds;

            var damageByType = hitEntries
                .GroupBy(e => e.DamageType)
                .Select(g =>
                {
                    var damageType = g.Key;
                    var missCount = missEntries.Count(e => e.DamageType == damageType);

                    return new DamageTypeStatistic
                    {
                        DamageType = damageType,
                        TotalDamage = g.Sum(e => e.RawDamage),
                        HitCount = g.Count(),
                        MissCount = missCount,
                        CriticalHits = g.Count(e => e.IsCritical),
                        AverageDamage = g.Average(e => e.RawDamage),
                        DamageShare = totalDamage > 0
                            ? (g.Sum(e => e.RawDamage) / totalDamage) * 100
                            : 0,
                        DPS = combatDuration > 0 ? g.Sum(e => e.RawDamage) / combatDuration : 0,
                        Accuracy = 100.0 // Für Schadensarten ist die Genauigkeit immer 100%, da nur getroffene Einträge erfasst werden
                    };
                })
                .OrderByDescending(s => s.TotalDamage)
                .ToList();

            DamageTypeStatistics = damageByType;
            OnPropertyChanged(nameof(DamageTypeStatistics));
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
