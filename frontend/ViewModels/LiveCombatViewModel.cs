using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;
using StoDamageMeter.Models;
using StoDamageMeter.Services;

namespace StoDamageMeter.ViewModels
{
    /// <summary>
    /// ViewModel für Live-Combat-Tracking
    /// </summary>
    public class LiveCombatViewModel : INotifyPropertyChanged, IDisposable
    {
        private readonly IOSCRBackendService _backendService;
        private readonly CombatLogWatcherService _fileWatcher;
        private readonly ILogger<LiveCombatViewModel> _logger;
        private readonly Dispatcher _dispatcher;
        
        private bool _isActive;
        private string _statusText = "Warte auf Combat...";
        private string? _currentLogPath;
        private CombatData? _currentCombat;
        private int _newLinesProcessed;
        private string _combatDuration = "00:00";
        private double _totalDPS;
        private List<string> _activeCombatLines = new();
        private DateTime? _lastUpdateTime;
        private Timer? _durationTimer;
        private DateTime? _combatStartTime;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<CombatData>? CombatCompleted;

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }

        public CombatData? CurrentCombat
        {
            get => _currentCombat;
            set => SetProperty(ref _currentCombat, value);
        }

        public int NewLinesProcessed
        {
            get => _newLinesProcessed;
            set => SetProperty(ref _newLinesProcessed, value);
        }

        public string CombatDuration
        {
            get => _combatDuration;
            set => SetProperty(ref _combatDuration, value);
        }

        public double TotalDPS
        {
            get => _totalDPS;
            set => SetProperty(ref _totalDPS, value);
        }

        public ObservableCollection<PlayerStatistics> LivePlayerStats { get; } = new();

        public LiveCombatViewModel(
            IOSCRBackendService backendService,
            CombatLogWatcherService fileWatcher,
            ILogger<LiveCombatViewModel> logger,
            Dispatcher dispatcher)
        {
            _backendService = backendService;
            _fileWatcher = fileWatcher;
            _logger = logger;
            _dispatcher = dispatcher;

            _fileWatcher.NewLinesDetected += OnNewLinesDetected;
            _fileWatcher.WatcherError += OnWatcherError;
        }

        /// <summary>
        /// Startet Live-Parsing für eine Combat-Log-Datei
        /// </summary>
        public async Task StartLiveParsing(string logPath)
        {
            _logger.LogInformation($"StartLiveParsing called for: {logPath}");
            
            if (IsActive)
            {
                _logger.LogWarning("Live parsing already active");
                return;
            }

            try
            {
                _currentLogPath = logPath;
                _activeCombatLines.Clear();
                _combatStartTime = null;
                NewLinesProcessed = 0;

                // Prüfe Datei-Größe
                var fileInfo = new System.IO.FileInfo(logPath);
                var fileSize = fileInfo.Length;
                _logger.LogInformation($"Log file size: {fileSize} bytes");
                
                // Starte FileWatcher:
                // - Wenn Datei < 500KB: Von Anfang lesen (0)
                // - Sonst: Letzte 500KB lesen (für kürzlich beendete Combats)
                long startOffset = fileSize > 512000 ? fileSize - 512000 : 0;
                
                _fileWatcher.StartWatching(logPath, startOffset);

                IsActive = true;
                StatusText = "Warte auf Combat...";

                _logger.LogInformation($"Live parsing started for {logPath} from offset {startOffset}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting live parsing");
                StatusText = $"Fehler: {ex.Message}";
            }
        }

        /// <summary>
        /// Stoppt Live-Parsing
        /// </summary>
        public async Task StopLiveParsing()
        {
            if (!IsActive)
            {
                return;
            }

            try
            {
                _fileWatcher.StopWatching();
                _durationTimer?.Dispose();
                _durationTimer = null;

                // Finaler Combat-Check
                if (_activeCombatLines.Count >= 20 && CurrentCombat != null)
                {
                    await FinalizeCombat();
                }

                IsActive = false;
                StatusText = "Gestoppt";
                _activeCombatLines.Clear();
                _currentCombat = null;
                _combatStartTime = null;

                _dispatcher.Invoke(() =>
                {
                    LivePlayerStats.Clear();
                });

                _logger.LogInformation("Live parsing stopped");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping live parsing");
            }
        }

        private async void OnNewLinesDetected(object? sender, NewLogLinesEventArgs e)
        {
            if (!IsActive || e.Lines.Count == 0)
            {
                return;
            }

            try
            {
                _lastUpdateTime = DateTime.Now;
                NewLinesProcessed += e.Lines.Count;

                // Prüfe Combat-Trennung: Zeit-Gap oder Combat-Type-Wechsel
                if (_activeCombatLines.Count > 0)
                {
                    var firstBufferLineTime = ParseLogLineTimestamp(_activeCombatLines.First());
                    var lastBufferLineTime = ParseLogLineTimestamp(_activeCombatLines.Last());
                    var firstNewLineTime = ParseLogLineTimestamp(e.Lines.First());
                    
                    var lastBufferCombatType = DetectCombatType(_activeCombatLines.Last());
                    var firstNewCombatType = DetectCombatType(e.Lines.First());

                    // Prüfe Combat-Type-Wechsel (Space ↔ Ground)
                    if (lastBufferCombatType != null && firstNewCombatType != null && 
                        lastBufferCombatType != firstNewCombatType)
                    {
                        _logger.LogInformation($"🔄 Combat type changed: {lastBufferCombatType} → {firstNewCombatType}");
                        Console.WriteLine($"🔄 Combat type changed: {lastBufferCombatType} → {firstNewCombatType}");
                        await FinalizeCombat();
                        
                        // Setze zurück für neuen Combat
                        _activeCombatLines.Clear();
                        _combatStartTime = null;
                        StatusText = "Warte auf Combat...";
                        
                        _logger.LogInformation($"✅ Buffer cleared, ready for new {firstNewCombatType} combat");
                        Console.WriteLine($"✅ Buffer cleared for new {firstNewCombatType} combat");
                    }
                    // Prüfe Zeit-Gap zwischen aufeinanderfolgenden Zeilen
                    else if (lastBufferLineTime.HasValue && firstNewLineTime.HasValue)
                    {
                        var timeDiff = (firstNewLineTime.Value - lastBufferLineTime.Value).TotalSeconds;
                        
                        // Nur loggen wenn >5 Sekunden (zu viel Spam sonst)
                        if (timeDiff > 5)
                        {
                            _logger.LogDebug($"⏱️ Time gap: {timeDiff:F1}s");
                        }

                        // Combat-Pause erkannt? → Finalisiere alten Combat
                        // Lange Kämpfe (>1 Minute) sind OK, solange kontinuierlich Zeilen kommen!
                        if (timeDiff > 30)
                        {
                            _logger.LogInformation($"🔴 Combat ended: {timeDiff:F0}s gap detected");
                            Console.WriteLine($"🔴 Combat ended: {timeDiff:F0}s gap - finalizing");
                            await FinalizeCombat();
                            
                            // Setze zurück für neuen Combat
                            _activeCombatLines.Clear();
                            _combatStartTime = null;
                            StatusText = "Warte auf Combat...";
                            
                            _logger.LogInformation($"✅ Buffer cleared, ready for new combat");
                            Console.WriteLine($"✅ Buffer cleared, ready for new combat");
                        }
                    }
                }

                // Füge neue Zeilen zum aktiven Combat hinzu
                _activeCombatLines.AddRange(e.Lines);

                _logger.LogInformation($"📝 Added {e.Lines.Count} lines, total in buffer: {_activeCombatLines.Count}");
                Console.WriteLine($"📝 Added {e.Lines.Count} lines, total in buffer: {_activeCombatLines.Count}");

                // Sofortiges Update ab der ersten Zeile!
                if (_activeCombatLines.Count >= 1)
                {
                    // Starte Duration-Timer beim ersten Mal
                    if (_combatStartTime == null)
                    {
                        _combatStartTime = DateTime.Now;
                        StartDurationTimer();
                        StatusText = "Combat läuft...";
                    }

                    // Aktualisiere Combat-Stats
                    await UpdateCombatStats();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing new lines");
                StatusText = $"Fehler: {ex.Message}";
            }
        }

        private async Task UpdateCombatStats()
        {
            try
            {
                _logger.LogInformation($"📊 UpdateCombatStats: Sending {_activeCombatLines.Count} lines to backend");
                Console.WriteLine($"📊 UpdateCombatStats: Sending {_activeCombatLines.Count} lines to backend");
                
                // DEBUG: Zeige erste und letzte Zeile
                if (_activeCombatLines.Count > 0)
                {
                    Console.WriteLine($"   First line: {_activeCombatLines.First().Substring(0, Math.Min(80, _activeCombatLines.First().Length))}...");
                    Console.WriteLine($"   Last line: {_activeCombatLines.Last().Substring(0, Math.Min(80, _activeCombatLines.Last().Length))}...");
                }
                
                // Backend-Call: Incremental Combat Update
                var request = new
                {
                    action = "incremental_update",
                    logPath = _currentLogPath,
                    combatLines = _activeCombatLines,
                    settings = new { }
                };

                // Serialisiere und sende Request (vereinfacht, nutzt interne Backend-Kommunikation)
                var response = await CallBackendIncrementalUpdate(_activeCombatLines);

                if (response?.Combats != null && response.Combats.Count > 0)
                {
                    var combatData = response.Combats[0];
                    _logger.LogInformation($"✅ Backend returned combat data: {combatData.Players.Count} players, {combatData.TotalDPS:N0} DPS");
                    Console.WriteLine($"✅ Backend returned combat data: {combatData.Players.Count} players, {combatData.TotalDPS:N0} DPS");
                    
                    CurrentCombat = combatData;
                    
                    // Berechne Total DPS aus allen Spielern
                    TotalDPS = combatData.Players.Sum(p => p.DpsWithCompanions);

                    // Update UI
                    _dispatcher.Invoke(() =>
                    {
                        LivePlayerStats.Clear();
                        foreach (var player in combatData.Players.OrderByDescending(p => p.DpsWithCompanions))
                        {
                            LivePlayerStats.Add(player);
                        }
                    });

                    _logger.LogInformation($"✅ UI updated: {LivePlayerStats.Count} players in LivePlayerStats, CurrentCombat is {(CurrentCombat != null ? "SET" : "NULL")}");
                    Console.WriteLine($"✅ UI updated: {LivePlayerStats.Count} players in LivePlayerStats, CurrentCombat is {(CurrentCombat != null ? "SET" : "NULL")}");
                }
                else
                {
                    _logger.LogWarning($"⚠️ Backend returned no combat data! Response: {(response == null ? "NULL" : $"Success={response.Success}, Combats.Count={response.Combats?.Count ?? 0}")}");
                    Console.WriteLine($"⚠️ Backend returned no combat data! Response: {(response == null ? "NULL" : $"Success={response.Success}, Combats.Count={response.Combats?.Count ?? 0}")}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating combat stats");
            }
        }

        private async Task<CombatAnalysisResponse?> CallBackendIncrementalUpdate(List<string> combatLines)
        {
            if (string.IsNullOrEmpty(_currentLogPath))
            {
                return null;
            }

            try
            {
                return await _backendService.IncrementalCombatUpdateAsync(_currentLogPath, combatLines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling backend for incremental update");
                return null;
            }
        }

        private async Task FinalizeCombat()
        {
            if (CurrentCombat == null || _activeCombatLines.Count < 5)
            {
                return;
            }

            _logger.LogInformation($"Finalizing combat: {_activeCombatLines.Count} lines");

            try
            {
                // Stoppe Duration-Timer
                _durationTimer?.Dispose();
                _durationTimer = null;

                // Trigger Combat-Completed Event
                CombatCompleted?.Invoke(this, CurrentCombat);

                // Reset für nächsten Combat
                _activeCombatLines.Clear();
                _currentCombat = null;
                _combatStartTime = null;
                StatusText = "Warte auf nächsten Combat...";

                _dispatcher.Invoke(() =>
                {
                    LivePlayerStats.Clear();
                });

                CombatDuration = "00:00";
                TotalDPS = 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error finalizing combat");
            }
        }

        private void StartDurationTimer()
        {
            _durationTimer = new Timer(_ =>
            {
                if (_combatStartTime.HasValue)
                {
                    var elapsed = DateTime.Now - _combatStartTime.Value;
                    _dispatcher.Invoke(() =>
                    {
                        CombatDuration = $"{(int)elapsed.TotalMinutes:D2}:{elapsed.Seconds:D2}";
                    });
                }
            }, null, 0, 1000); // Update jede Sekunde
        }

        private void OnWatcherError(object? sender, FileWatcherErrorEventArgs e)
        {
            _logger.LogError(e.Exception, $"FileWatcher error: {e.ErrorMessage}");
            _dispatcher.Invoke(() =>
            {
                StatusText = $"Fehler: {e.ErrorMessage}";
            });
        }

        /// <summary>
        /// Erkennt den Combat-Type aus einer Combat-Log-Zeile (Space oder Ground)
        /// </summary>
        private string? DetectCombatType(string logLine)
        {
            if (string.IsNullOrWhiteSpace(logLine))
            {
                return null;
            }

            // Space Combat: Enthält "Space_" in Entity-IDs
            if (logLine.Contains("Space_", StringComparison.OrdinalIgnoreCase))
            {
                return "Space";
            }
            
            // Ground Combat: Enthält "Ground_" in Entity-IDs
            if (logLine.Contains("Ground_", StringComparison.OrdinalIgnoreCase))
            {
                return "Ground";
            }

            // Unbekannt (kann z.B. bei Struktur-Zeilen vorkommen)
            return null;
        }

        /// <summary>
        /// Parst den Zeitstempel aus einer Combat-Log-Zeile
        /// Format: 25:10:11:00:05:01.1 :: ... (YY:MM:DD:HH:MM:SS.ms)
        /// </summary>
        private DateTime? ParseLogLineTimestamp(string logLine)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(logLine))
                {
                    return null;
                }

                // Zeitstempel ist am Anfang der Zeile bis zum ersten ::
                var parts = logLine.Split(new[] { "::" }, StringSplitOptions.None);
                if (parts.Length < 2)
                {
                    return null;
                }

                var timestampStr = parts[0].Trim();
                
                // Format: YY:MM:DD:HH:MM:SS.ms
                var timeParts = timestampStr.Split(':');
                if (timeParts.Length < 6)
                {
                    return null;
                }

                var year = 2000 + int.Parse(timeParts[0]);
                var month = int.Parse(timeParts[1]);
                var day = int.Parse(timeParts[2]);
                var hour = int.Parse(timeParts[3]);
                var minute = int.Parse(timeParts[4]);
                
                // Sekunden können Dezimalstellen haben (SS.ms)
                var secondsParts = timeParts[5].Split('.');
                var second = int.Parse(secondsParts[0]);
                var millisecond = secondsParts.Length > 1 ? int.Parse(secondsParts[1]) * 100 : 0;

                return new DateTime(year, month, day, hour, minute, second, millisecond);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to parse timestamp from log line: {logLine}");
                return null;
            }
        }

        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (!Equals(field, value))
            {
                field = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public void Dispose()
        {
            _fileWatcher.NewLinesDetected -= OnNewLinesDetected;
            _fileWatcher.WatcherError -= OnWatcherError;
            _fileWatcher.Dispose();
            _durationTimer?.Dispose();
        }
    }
}

