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

        // Filter-Properties
        [ObservableProperty]
        private StatisticsFilter _currentFilter = new();

        [ObservableProperty]
        private List<WeaponStatistics> _filteredWeaponStatistics = new();

        [ObservableProperty]
        private List<DamageTypeStatistic> _filteredDamageTypeStatistics = new();

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

            // Filter-Änderungen überwachen
            CurrentFilter.PropertyChanged += OnFilterChanged;
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
                // Setze alle auf nicht ausgewählt
                foreach (var period in CombatPeriods)
                {
                    period.IsSelected = false;
                }

                SelectedPeriod = CombatPeriods.First();
                SelectedPeriod.IsSelected = true;
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
        private void SelectPeriod(CombatPeriod period)
        {
            IsLoading = true;
            LoadingMessage = "Analysiere Kampfdaten...";

            try
            {
                // Setze alle auf nicht ausgewählt
                foreach (var p in CombatPeriods)
                {
                    p.IsSelected = false;
                }

                SelectedPeriod = period;
                period.IsSelected = true;
                _logger.LogInformation("Zeitraum ausgewählt: {StartTime} - {EndTime}", period.StartTime, period.EndTime);
                _logger.LogInformation("SelectedPeriod.Entries.Count: {EntryCount}", period.Entries.Count);

                // Aktualisiere alle Statistiken für den ausgewählten Zeitraum
                UpdateWeaponStatistics();
                UpdatePlayerSummary();
                UpdateDamageTypeStatistics();

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
            _logger.LogInformation("=== UpdateWeaponStatistics aufgerufen ===");
            _logger.LogInformation("SelectedPeriod ist null: {IsNull}", SelectedPeriod == null);
            _logger.LogInformation("SelectedPlayer ist leer: {IsEmpty}", string.IsNullOrEmpty(SelectedPlayer));
            _logger.LogInformation("SelectedPlayer Wert: '{PlayerName}'", SelectedPlayer ?? "NULL");

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

                // Zeige die ersten 5 Rohdaten-Einträge für den AUSGEWÄHLTEN Kampf
                _logger.LogInformation("=== ROHDATEN für AUSGEWÄHLTEN Kampf (erste 5 Einträge) ===");
                var sampleEntries = SelectedPeriod.Entries.Take(5).ToList();
                for (int i = 0; i < sampleEntries.Count; i++)
                {
                    var entry = sampleEntries[i];
                    _logger.LogInformation("--- Eintrag {Index} ---", i + 1);
                    _logger.LogInformation("ORIGINAL ZEILE: {OriginalLine}", entry.OriginalLine);
                    _logger.LogInformation("GEPARST: Player: '{PlayerName}' | Source: '{SourceName}' | Target: '{TargetName}' | Attack: '{AttackName}' | DamageType: '{DamageType}' | Damage: {RawDamage} | IsRelevant: {IsRelevant} | IsCompanion: {IsCompanion}",
                        entry.PlayerInfo?.CharName ?? "NULL",
                        entry.SourceEntity?.Name ?? "NULL",
                        entry.TargetEntity?.Name ?? "NULL",
                        entry.AttackName,
                        entry.DamageType,
                        entry.RawDamage,
                        entry.IsRelevant,
                        entry.IsCompanionDamage);
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

                // Filtere die neuen Daten
                ApplyFilters();
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

            _logger.LogInformation("=== UpdateDamageTypeStatistics für Spieler '{PlayerName}' ===", SelectedPlayer);

            // GLEICHER FILTER WIE Waffen-Statistiken: Nur relevante Einträge des Spielers
            var playerEntries = SelectedPeriod.Entries
                .Where(e => e.PlayerInfo?.CharName == SelectedPlayer && e.IsRelevant)
                .ToList();

            _logger.LogInformation("Gefilterte Einträge für Schadensarten: {EntryCount}", playerEntries.Count);

            // Zeige die ersten 5 gefilterten Einträge
            var sampleEntries = playerEntries.Take(5).ToList();
            for (int i = 0; i < sampleEntries.Count; i++)
            {
                var entry = sampleEntries[i];
                _logger.LogInformation("Schadensart {Index}: {AttackName} | {DamageType} | {RawDamage} Schaden",
                    i + 1, entry.AttackName, entry.DamageType, entry.RawDamage);
            }

            var totalDamage = playerEntries.Sum(e => e.RawDamage);
            var combatDuration = SelectedPeriod.Duration.TotalSeconds;

            _logger.LogInformation("Gesamtschaden für Schadensarten: {TotalDamage}", totalDamage);

            var damageByType = playerEntries
                .GroupBy(e => e.DamageType)
                .Select(g =>
                {
                    var damageType = g.Key;
                    var groupDamage = g.Sum(e => e.RawDamage);

                    return new DamageTypeStatistic
                    {
                        DamageType = damageType,
                        TotalDamage = groupDamage,
                        HitCount = g.Count(),
                        MissCount = 0, // Keine Misses, da wir nur relevante Einträge verwenden
                        CriticalHits = g.Count(e => e.IsCritical),
                        AverageDamage = g.Average(e => e.RawDamage),
                        DamageShare = totalDamage > 0
                            ? (groupDamage / totalDamage) * 100
                            : 0,
                        DPS = combatDuration > 0 ? groupDamage / combatDuration : 0,
                        Accuracy = 100.0 // Für Schadensarten ist die Genauigkeit immer 100%, da nur getroffene Einträge erfasst werden
                    };
                })
                .OrderByDescending(s => s.TotalDamage)
                .ToList();

            _logger.LogInformation("=== FINALE Schadensarten-Statistiken ===");
            _logger.LogInformation("Schadensarten-Statistiken aktualisiert: {DamageTypeCount} Schadensarten für Spieler '{PlayerName}'",
                damageByType.Count, SelectedPlayer);

            // Zeige Details der generierten Schadensarten-Statistiken
            foreach (var damageType in damageByType)
            {
                _logger.LogInformation("Schadensart: {DamageType} | Schaden: {TotalDamage} | DPS: {DPS} | Treffer: {HitCount}",
                    damageType.DamageType, damageType.TotalDamage, damageType.DPS, damageType.HitCount);
            }

            _logger.LogInformation("=== Ende UpdateDamageTypeStatistics ===");

            DamageTypeStatistics = damageByType;
            OnPropertyChanged(nameof(DamageTypeStatistics));

            // Filtere die neuen Daten
            ApplyFilters();
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

        /// <summary>
        /// Reagiert auf Filter-Änderungen
        /// </summary>
        private void OnFilterChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ApplyFilters();
        }

        /// <summary>
        /// Wendet die aktuellen Filter auf die Statistiken an
        /// </summary>
        private void ApplyFilters()
        {
            _logger.LogInformation("=== ApplyFilters aufgerufen ===");
            _logger.LogInformation("Filter aktiv: {IsActive}", CurrentFilter.IsActive);

            if (!CurrentFilter.IsActive)
            {
                // Keine Filter aktiv - zeige alle Daten
                FilteredWeaponStatistics = new List<WeaponStatistics>(WeaponStatistics);
                FilteredDamageTypeStatistics = new List<DamageTypeStatistic>(DamageTypeStatistics);
                _logger.LogInformation("Keine Filter aktiv - alle Daten angezeigt");
                return;
            }

            // Filtere Waffen-Statistiken
            FilteredWeaponStatistics = WeaponStatistics.Where(FilterWeaponStatistics).ToList();

            // Filtere Schadensarten-Statistiken
            FilteredDamageTypeStatistics = DamageTypeStatistics.Where(FilterDamageTypeStatistics).ToList();

            _logger.LogInformation("Filter angewendet - Waffen: {WeaponCount}, Schadensarten: {DamageTypeCount}",
                FilteredWeaponStatistics.Count, FilteredDamageTypeStatistics.Count);
        }

        /// <summary>
        /// Prüft ob eine Waffen-Statistik den Filter-Kriterien entspricht
        /// </summary>
        private bool FilterWeaponStatistics(WeaponStatistics weapon)
        {
            // Zeit-Filter (falls implementiert)
            if (CurrentFilter.StartTime.HasValue && SelectedPeriod?.StartTime < CurrentFilter.StartTime.Value)
                return false;
            if (CurrentFilter.EndTime.HasValue && SelectedPeriod?.EndTime > CurrentFilter.EndTime.Value)
                return false;

            // Schaden-Filter
            if (CurrentFilter.MinDamage.HasValue && weapon.AverageDamage < CurrentFilter.MinDamage.Value)
                return false;
            if (CurrentFilter.MaxDamage.HasValue && weapon.AverageDamage > CurrentFilter.MaxDamage.Value)
                return false;

            // Waffen-Filter
            if (CurrentFilter.IncludedWeapons.Any() && !CurrentFilter.IncludedWeapons.Contains(weapon.Name))
                return false;
            if (CurrentFilter.ExcludedWeapons.Contains(weapon.Name))
                return false;

            return true;
        }

        /// <summary>
        /// Prüft ob eine Schadensarten-Statistik den Filter-Kriterien entspricht
        /// </summary>
        private bool FilterDamageTypeStatistics(DamageTypeStatistic damageType)
        {
            // Zeit-Filter (falls implementiert)
            if (CurrentFilter.StartTime.HasValue && SelectedPeriod?.StartTime < CurrentFilter.StartTime.Value)
                return false;
            if (CurrentFilter.EndTime.HasValue && SelectedPeriod?.EndTime > CurrentFilter.EndTime.Value)
                return false;

            // Schaden-Filter
            if (CurrentFilter.MinDamage.HasValue && damageType.AverageDamage < CurrentFilter.MinDamage.Value)
                return false;
            if (CurrentFilter.MaxDamage.HasValue && damageType.AverageDamage > CurrentFilter.MaxDamage.Value)
                return false;

            // Schadensarten-Filter
            if (CurrentFilter.IncludedDamageTypes.Any() && !CurrentFilter.IncludedDamageTypes.Contains(damageType.DamageType))
                return false;

            return true;
        }

        /// <summary>
        /// Setzt alle Filter zurück
        /// </summary>
        [RelayCommand]
        private void ResetFilters()
        {
            _logger.LogInformation("=== ResetFilters aufgerufen ===");
            CurrentFilter.Reset();
            _logger.LogInformation("Alle Filter zurückgesetzt");
        }

        /// <summary>
        /// Wendet die aktuellen Filter an (manuell)
        /// </summary>
        [RelayCommand]
        private void ApplyFiltersCommand()
        {
            _logger.LogInformation("=== ApplyFilters Command aufgerufen ===");
            ApplyFilters();
        }

        /// <summary>
        /// Togglet die Sichtbarkeit des Filter-Panels
        /// </summary>
        [ObservableProperty]
        private bool _isFilterPanelVisible = false;

        [RelayCommand]
        private void ToggleFilterPanel()
        {
            IsFilterPanelVisible = !IsFilterPanelVisible;
            _logger.LogInformation("Filter-Panel Sichtbarkeit: {IsVisible}", IsFilterPanelVisible);
        }

        /// <summary>
        /// Wendet einen vordefinierten Filter an
        /// </summary>
        [RelayCommand]
        private void ApplyFilterPreset(string presetName)
        {
            _logger.LogInformation("=== ApplyFilterPreset aufgerufen: {PresetName} ===", presetName);

            switch (presetName)
            {
                case "HighDamage":
                    CurrentFilter.MinDamage = 1000;
                    _logger.LogInformation("High Damage Filter angewendet (Min: 1000)");
                    break;
                case "CriticalHits":
                    CurrentFilter.MinDamage = 500; // Kritische Treffer haben meist höheren Schaden
                    _logger.LogInformation("Critical Hits Filter angewendet (Min: 500)");
                    break;
                case "TopWeapons":
                    // Zeige nur Top 5 Waffen
                    var topWeapons = WeaponStatistics
                        .OrderByDescending(w => w.TotalDamage)
                        .Take(5)
                        .Select(w => w.Name)
                        .ToList();
                    CurrentFilter.IncludedWeapons = topWeapons;
                    _logger.LogInformation("Top Weapons Filter angewendet: {WeaponCount} Waffen", topWeapons.Count);
                    break;
                default:
                    _logger.LogWarning("Unbekannter Filter-Preset: {PresetName}", presetName);
                    break;
            }
        }
    }
}
