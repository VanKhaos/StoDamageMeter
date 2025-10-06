using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using StoDamageMeter.Models;
using StoDamageMeter.Services;
using System.Linq;

namespace StoDamageMeter.ViewModels
{
    /// <summary>
    /// ViewModel für die Statistiken-Seite
    /// </summary>
    public partial class StatisticsViewModel : ObservableObject
    {
        private readonly CombatPeriodService _combatPeriodService;
        private readonly CombatLogViewModel _combatLogViewModel;
        private readonly ILogger<StatisticsViewModel> _logger;

        [ObservableProperty]
        private List<CombatPeriod> _combatPeriods = new();

        [ObservableProperty]
        private CombatPeriod? _selectedPeriod;

        [ObservableProperty]
        private List<string> _availablePlayers = new();

        [ObservableProperty]
        private List<PlayerSummary> _playerSummaries = new();

        [ObservableProperty]
        private List<PlayerSummary> _filteredPlayerSummaries = new();

        [ObservableProperty]
        private string _searchText = string.Empty;

        [ObservableProperty]
        private string _selectedPlayer = string.Empty;

        [ObservableProperty]
        private PlayerCombatSummary? _playerSummary;

        [ObservableProperty]
        private List<DamageTypeStatistic> _damageTypeStatistics = new();

        [ObservableProperty]
        private StatisticsFilter _currentFilter = new();

        [ObservableProperty]
        private List<DamageTypeStatistic> _filteredDamageTypeStatistics = new();

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [RelayCommand]
        private void SelectPlayer(string playerName)
        {
            try
            {
                SelectedPlayer = playerName;
                _logger.LogInformation("Spieler ausgewählt: {PlayerName}", playerName);

                // Hier könnten weitere Aktionen folgen, z.B. Navigation zur Statistics-Seite
                // oder Update der Statistiken für den ausgewählten Spieler
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Auswählen des Spielers: {PlayerName}", playerName);
            }
        }

        public StatisticsViewModel(CombatPeriodService combatPeriodService, CombatLogViewModel combatLogViewModel, ILogger<StatisticsViewModel> logger)
        {
            _combatPeriodService = combatPeriodService;
            _combatLogViewModel = combatLogViewModel;
            _logger = logger;
        }

        /// <summary>
        /// Filtert die Spieler basierend auf dem Suchtext
        /// </summary>
        partial void OnSearchTextChanged(string value)
        {
            FilterPlayers();
        }

        private void FilterPlayers()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredPlayerSummaries = PlayerSummaries;
            }
            else
            {
                FilteredPlayerSummaries = PlayerSummaries
                    .Where(p => p.Name.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }
        }

        // Einfache Methoden für MainViewModel
        public void UpdateCombatPeriods()
        {
            // Placeholder - wird später implementiert
        }

        public void UpdateAvailablePlayers()
        {
            try
            {
                if (_combatLogViewModel?.CurrentResult?.Entries != null)
                {
                    var players = _combatPeriodService.GetAvailablePlayers(_combatLogViewModel.CurrentResult.Entries);
                    AvailablePlayers = players;

                    // Erstelle PlayerSummary-Objekte für die Tabelle
                    var playerSummaries = new List<PlayerSummary>();
                    foreach (var playerName in players)
                    {
                        var playerEntries = _combatLogViewModel.CurrentResult.Entries
                            .Where(e => e.PlayerInfo.CharName == playerName && e.IsRelevant)
                            .ToList();

                        if (playerEntries.Any())
                        {
                            var totalDamage = playerEntries.Sum(e => e.DamageWithResistance);
                            var hits = playerEntries.Count;
                            var duration = _combatLogViewModel.CurrentResult.Statistics?.CombatDuration.TotalSeconds ?? 1.0;
                            var dps = duration > 0 ? totalDamage / duration : 0;
                            var averageDamage = hits > 0 ? totalDamage / hits : 0;

                            playerSummaries.Add(new PlayerSummary
                            {
                                Name = playerName,
                                DPS = dps,
                                AverageDamage = averageDamage,
                                TotalDamage = totalDamage,
                                Kills = 0, // TODO: Kills implementieren
                                Hits = hits,
                                Duration = duration
                            });
                        }
                    }

                    // Sortiere nach DPS (absteigend)
                    PlayerSummaries = playerSummaries.OrderByDescending(p => p.DPS).ToList();
                    FilteredPlayerSummaries = PlayerSummaries;

                    _logger.LogInformation("AvailablePlayers aktualisiert: {PlayerCount} Spieler gefunden", players.Count);
                }
                else
                {
                    AvailablePlayers = new List<string>();
                    PlayerSummaries = new List<PlayerSummary>();
                    FilteredPlayerSummaries = new List<PlayerSummary>();
                    _logger.LogWarning("Keine Combat Log Einträge verfügbar für UpdateAvailablePlayers");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Aktualisieren der verfügbaren Spieler");
                AvailablePlayers = new List<string>();
                PlayerSummaries = new List<PlayerSummary>();
                FilteredPlayerSummaries = new List<PlayerSummary>();
            }
        }
    }
}