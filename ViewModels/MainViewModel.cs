using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Microsoft.Extensions.Logging;
using StoDamageMeter.Models;
using StoDamageMeter.Services;
using System.Windows;

namespace StoDamageMeter.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ICombatLogService _combatLogService;
        private readonly ILogger<MainViewModel> _logger;

        [ObservableProperty]
        private string _selectedLogFile = string.Empty;

        [ObservableProperty]
        private string _lastUpdate = string.Empty;

        [ObservableProperty]
        private bool _isWatching = false;

        [ObservableProperty]
        private bool _isProcessing = false;

        [ObservableProperty]
        private string _statusMessage = "Bereit";

        [ObservableProperty]
        private CombatLogResult? _currentResult;

        [ObservableProperty]
        private CombatLogStatistics? _statistics;

        [ObservableProperty]
        private int _playerCount = 0;

        [ObservableProperty]
        private bool _isLoading = false;

        [ObservableProperty]
        private string _loadingMessage = string.Empty;

        [ObservableProperty]
        private string _loadingTime = string.Empty;

        [ObservableProperty]
        private string _currentPageTitle = "Dashboard";

        [ObservableProperty]
        private string _currentPageIcon = "DataUsage24";

        public MainViewModel(
            ICombatLogService combatLogService,
            ILogger<MainViewModel> logger)
        {
            _combatLogService = combatLogService;
            _logger = logger;

            // Event-Handler für Live-Updates
            _combatLogService.DataUpdated += OnCombatLogDataUpdated;
        }

        [RelayCommand]
        public async Task SelectLogFile()
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Title = "Combatlog-Datei auswählen",
                    Filter = "Log-Dateien (*.log)|*.log|Alle Dateien (*.*)|*.*",
                    DefaultExt = "log"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    SelectedLogFile = openFileDialog.FileName;
                    await ProcessCombatLogFile(SelectedLogFile);
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Fehler: {ex.Message}";
                _logger.LogError(ex, "Fehler beim Auswählen der Log-Datei");
            }
        }

        [RelayCommand]
        public async Task ProcessCombatLogFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) || IsProcessing)
                return;

            try
            {
                IsProcessing = true;
                IsLoading = true;
                LoadingMessage = "📖 Lade Datei...";

                // Stoppe vorherige Überwachung
                if (IsWatching)
                {
                    _combatLogService.StopWatching();
                }

                // Verarbeite Datei
                var result = await _combatLogService.ProcessCombatLogFileAsync(filePath);

                if (result.Success)
                {
                    CurrentResult = result;
                    Statistics = result.Statistics;
                    PlayerCount = result.Statistics.PlayerCount;
                    StatusMessage = $"Verarbeitet: {result.Statistics.RelevantEntries} Einträge, {result.Statistics.PlayerCount} Spieler";
                }
                else
                {
                    StatusMessage = $"Fehler: {result.ErrorMessage}";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Fehler: {ex.Message}";
                _logger.LogError(ex, "Fehler beim Verarbeiten der Combatlog-Datei");
            }
            finally
            {
                IsProcessing = false;
                IsLoading = false;
                LoadingMessage = string.Empty;
                LoadingTime = string.Empty;
            }
        }

        [RelayCommand]
        public async Task StartWatching()
        {
            if (string.IsNullOrEmpty(SelectedLogFile))
                return;

            try
            {
                await _combatLogService.StartWatchingAsync(SelectedLogFile);
                IsWatching = true;
                StatusMessage = "Live-Überwachung aktiv";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Fehler: {ex.Message}";
                _logger.LogError(ex, "Fehler beim Starten der Überwachung");
            }
        }

        [RelayCommand]
        public void StopWatching()
        {
            try
            {
                _combatLogService.StopWatching();
                IsWatching = false;
                StatusMessage = "Live-Überwachung gestoppt";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Fehler: {ex.Message}";
                _logger.LogError(ex, "Fehler beim Stoppen der Überwachung");
            }
        }

        private void OnCombatLogDataUpdated(object? sender, CombatLogDataUpdatedEventArgs e)
        {
            try
            {
                // Update Statistics und PlayerCount
                Statistics = e.Result.Statistics;
                PlayerCount = e.Result.Statistics.PlayerCount;
                LastUpdate = DateTime.Now.ToString("HH:mm:ss");
                StatusMessage = $"Live-Update: {e.NewEntries.Count} neue Einträge";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Verarbeiten des Live-Updates");
            }
        }

        public void UpdatePageTitle(string pageTag)
        {
            CurrentPageTitle = pageTag switch
            {
                "Dashboard" => "Dashboard",
                "LiveTracking" => "Live Tracking",
                "Statistics" => "Statistiken",
                "Configuration" => "Konfiguration",
                "About" => "Über",
                _ => "Dashboard"
            };

            CurrentPageIcon = pageTag switch
            {
                "Dashboard" => "Home24",
                "LiveTracking" => "DataUsage24",
                "Statistics" => "ChartMultiple24",
                "Configuration" => "Settings24",
                "About" => "Info24",
                _ => "Home24"
            };
        }

        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
        }
    }
}