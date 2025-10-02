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
        private readonly IDebugLogger _debugLogger;
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

        public MainViewModel(
            ICombatLogService combatLogService,
            IDebugLogger debugLogger,
            ILogger<MainViewModel> logger)
        {
            _combatLogService = combatLogService;
            _debugLogger = debugLogger;
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
                    Title = "Combatlog auswählen",
                    Filter = "Log-Dateien (*.log)|*.log|Text-Dateien (*.txt)|*.txt|Alle Dateien (*.*)|*.*",
                    Multiselect = false
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    await ProcessCombatLogFile(openFileDialog.FileName);
                }
            }
            catch (Exception ex)
            {
                _debugLogger.LogError("Fehler beim Auswählen der Log-Datei", ex);
                StatusMessage = $"Fehler: {ex.Message}";
            }
        }

        [RelayCommand]
        public async Task ProcessCombatLogFile(string filePath)
        {
            try
            {
                IsProcessing = true;
                StatusMessage = "Verarbeite Combatlog...";
                _debugLogger.LogInfo($"Verarbeite Combatlog-Datei: {filePath}");

                // Stoppe vorherige Überwachung
                if (IsWatching)
                {
                    _combatLogService.StopWatching();
                }

                // Verarbeite Datei
                var result = await _combatLogService.ProcessCombatLogFileAsync(filePath);

                if (result.Success)
                {
                    SelectedLogFile = filePath;
                    CurrentResult = result;
                    Statistics = result.Statistics;
                    LastUpdate = result.ProcessedAt.ToString("HH:mm:ss");
                    StatusMessage = $"Verarbeitet: {result.Statistics.RelevantEntries} Einträge";

                    _debugLogger.LogInfo($"Combatlog erfolgreich verarbeitet: {result.Statistics.RelevantEntries} relevante Einträge");
                }
                else
                {
                    StatusMessage = $"Fehler: {result.ErrorMessage}";
                    _debugLogger.LogError($"Fehler beim Verarbeiten: {result.ErrorMessage}");
                }
            }
            catch (Exception ex)
            {
                _debugLogger.LogError("Fehler beim Verarbeiten der Combatlog-Datei", ex);
                StatusMessage = $"Fehler: {ex.Message}";
            }
            finally
            {
                IsProcessing = false;
            }
        }

        [RelayCommand]
        public async Task StartWatching()
        {
            try
            {
                if (string.IsNullOrEmpty(SelectedLogFile))
                {
                    StatusMessage = "Keine Datei ausgewählt";
                    return;
                }

                StatusMessage = "Starte Überwachung...";
                _debugLogger.LogInfo($"Starte Überwachung für: {SelectedLogFile}");

                await _combatLogService.StartWatchingAsync(SelectedLogFile);
                IsWatching = true;
                StatusMessage = "Überwachung aktiv";
            }
            catch (Exception ex)
            {
                _debugLogger.LogError("Fehler beim Starten der Überwachung", ex);
                StatusMessage = $"Fehler: {ex.Message}";
            }
        }

        [RelayCommand]
        public void StopWatching()
        {
            try
            {
                _combatLogService.StopWatching();
                IsWatching = false;
                StatusMessage = "Überwachung gestoppt";
                _debugLogger.LogInfo("Überwachung gestoppt");
            }
            catch (Exception ex)
            {
                _debugLogger.LogError("Fehler beim Stoppen der Überwachung", ex);
                StatusMessage = $"Fehler: {ex.Message}";
            }
        }

        private void OnCombatLogDataUpdated(object? sender, CombatLogDataUpdatedEventArgs e)
        {
            try
            {
                // UI-Thread verwenden für Updates
                Application.Current.Dispatcher.Invoke(() =>
                {
                    CurrentResult = e.Result;
                    Statistics = e.Result.Statistics;
                    LastUpdate = DateTime.Now.ToString("HH:mm:ss");
                    StatusMessage = $"Live: {e.NewEntries.Count} neue Einträge";
                });

                _debugLogger.LogInfo($"Live-Update: {e.NewEntries.Count} neue Einträge verarbeitet");
            }
            catch (Exception ex)
            {
                _debugLogger.LogError("Fehler beim Verarbeiten des Live-Updates", ex);
            }
        }

        protected override void OnPropertyChanged(System.ComponentModel.PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            // Debug-Logging für wichtige Property-Änderungen
            if (e.PropertyName == nameof(IsWatching))
            {
                _debugLogger.LogDebug($"IsWatching geändert: {IsWatching}");
            }
            else if (e.PropertyName == nameof(Statistics))
            {
                _debugLogger.LogDebug($"Statistics aktualisiert: {Statistics?.RelevantEntries} Einträge");
            }
        }
    }
}
