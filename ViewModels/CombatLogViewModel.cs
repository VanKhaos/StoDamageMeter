using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Microsoft.Extensions.Logging;
using StoDamageMeter.Models;
using StoDamageMeter.Services;

namespace StoDamageMeter.ViewModels
{
    /// <summary>
    /// ViewModel für Combat Log Verarbeitung und Live-Tracking
    /// </summary>
    public partial class CombatLogViewModel : ObservableObject
    {
        private readonly ICombatLogService _combatLogService;
        private readonly ILogger<CombatLogViewModel> _logger;

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

        public event EventHandler<CombatLogDataUpdatedEventArgs>? DataUpdated;

        public CombatLogViewModel(ICombatLogService combatLogService, ILogger<CombatLogViewModel> logger)
        {
            _combatLogService = combatLogService;
            _logger = logger;

            // Event-Handler für Live-Updates
            _combatLogService.DataUpdated += OnCombatLogDataUpdated;
        }

        /// <summary>
        /// Öffnet einen Datei-Dialog zur Auswahl einer Combat Log Datei
        /// </summary>
        [RelayCommand]
        public async Task SelectLogFile()
        {
            _logger.LogInformation("=== SelectLogFile Command aufgerufen ===");
            try
            {
                _logger.LogInformation("Erstelle OpenFileDialog...");
                var openFileDialog = new OpenFileDialog
                {
                    Title = "Combatlog-Datei auswählen",
                    Filter = "Combat Log Dateien (*.txt;*.log;*.combatlog)|*.txt;*.log;*.combatlog|Text-Dateien (*.txt)|*.txt|Log-Dateien (*.log)|*.log|Alle Dateien (*.*)|*.*",
                    DefaultExt = "txt",
                    Multiselect = false,
                    InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                _logger.LogInformation("Zeige File Dialog...");
                var result = openFileDialog.ShowDialog();
                _logger.LogInformation("File Dialog Result: {Result}", result);

                if (result == true)
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
            finally
            {
                _logger.LogInformation("=== SelectLogFile Command beendet ===");
            }
        }

        /// <summary>
        /// Verarbeitet eine Combat Log Datei
        /// </summary>
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

                // Debug-Logging ist jetzt in der Console

                // Stoppe vorherige Überwachung
                if (IsWatching)
                {
                    _combatLogService.StopWatching();
                }

                // Verarbeite Datei
                var result = await _combatLogService.ProcessCombatLogFileAsync(filePath);

                if (result.Success)
                {
                    // Combat Log erfolgreich verarbeitet
                    CurrentResult = result;
                    Statistics = result.Statistics;
                    PlayerCount = result.Statistics.PlayerCount;
                    StatusMessage = $"Verarbeitet: {result.Statistics.RelevantEntries} Einträge, {result.Statistics.PlayerCount} Spieler";

                    // Benachrichtige andere ViewModels über die neuen Daten
                    DataUpdated?.Invoke(this, new CombatLogDataUpdatedEventArgs
                    {
                        Result = result,
                        NewEntries = result.Entries ?? new List<CombatLogEntry>()
                    });
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

        /// <summary>
        /// Startet die Live-Überwachung einer Combat Log Datei
        /// </summary>
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

        /// <summary>
        /// Stoppt die Live-Überwachung
        /// </summary>
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

        /// <summary>
        /// Event-Handler für Live-Updates vom CombatLogService
        /// </summary>
        private void OnCombatLogDataUpdated(object? sender, CombatLogDataUpdatedEventArgs e)
        {
            try
            {
                // Update Statistics und PlayerCount
                Statistics = e.Result.Statistics;
                PlayerCount = e.Result.Statistics.PlayerCount;
                LastUpdate = DateTime.Now.ToString("HH:mm:ss");
                StatusMessage = $"Live-Update: {e.NewEntries.Count} neue Einträge";

                // Update CurrentResult
                CurrentResult = e.Result;

                // Benachrichtige andere ViewModels über die neuen Daten
                DataUpdated?.Invoke(this, e);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Verarbeiten des Live-Updates");
            }
        }


    }
}
