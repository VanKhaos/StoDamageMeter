using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StoDamageMeter.Services;
using StoDamageMeter.Models;
using StoDamageMeter.ViewModels;
using System.Text.Json;
using Wpf.Ui.Controls;

namespace StoDamageMeter;

/// <summary>
/// Interaction logic for CombatStatistic.xaml
/// </summary>
    public partial class CombatStatistic : Window
    {
        private readonly IOSCRBackendService _backendService;
        private readonly CombatStatsRenderer _statsRenderer;
        private readonly CombatLogWatcherService _logWatcher;
        private readonly ILogger<CombatStatistic> _logger;
        private CancellationTokenSource? _loadingCancellation;
        private List<CombatInfo>? _loadedCombats;
        private CombatData? _currentCombatData;
        private string? _currentLogPath;
        private bool _isPinned = false;
    
    // Sorting state
    private string _currentSortColumn = "TotalDamageWithCompanions"; // Default
    private bool _sortAscending = false; // Default: descending

    public CombatStatistic()
    {
        InitializeComponent();
        
        // Backend Service aus DI Container holen
        _backendService = App.ServiceProvider.GetRequiredService<IOSCRBackendService>();
        _logWatcher = App.ServiceProvider.GetRequiredService<CombatLogWatcherService>();
        _logger = App.ServiceProvider.GetRequiredService<ILogger<CombatStatistic>>();
        
        // Stats Renderer initialisieren
        _statsRenderer = new CombatStatsRenderer((Style)this.FindResource("NoToggleIconExpanderStyle"));
        
        // Progress Event abonnieren
        _backendService.AnalysisProgress += OnAnalysisProgress;
        
        // Component Events verbinden
        CombatListViewComponent.CombatSelected += OnCombatSelected;
        StatsHeaderComponent.ColumnHeaderClicked += OnColumnHeaderClicked;
        
        // TEST LOG
        _logger.LogInformation("=== CombatStatistic Constructor - Logging Test ===");
        
        
        // Initial Status Check
        _ = CheckBackendStatusAsync();
        
        // Load combat list if a log file is already selected
        Loaded += async (s, e) => await LoadCombatListIfAvailableAsync();
    }


    private async void OnLiveCombatCompleted(object? sender, CombatData completedCombat)
    {
        _logger.LogInformation($"✅ Live Combat completed: {completedCombat.Type} - {completedCombat.TotalDPS:N0} DPS, Duration: {completedCombat.Duration:F1}s");
        Console.WriteLine($"✅ Live Combat completed: {completedCombat.Type} - {completedCombat.TotalDPS:N0} DPS");

        // Combat-Liste automatisch aktualisieren wenn ein Log geladen ist
        if (!string.IsNullOrEmpty(_currentLogPath))
        {
            _logger.LogInformation("🔄 Refreshing combat list after completed live combat");
            Console.WriteLine("🔄 Refreshing combat list...");
            
            await Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    // Combat-Liste neu laden (OHNE Trimmen - isInitialLoad = false)
                    // Live-Parsing wird automatisch in LoadCombatListAsync neu gestartet
                    await LoadCombatListAsync(_currentLogPath, isInitialLoad: false);
                    _logger.LogInformation("✅ Combat list refreshed successfully");
                    Console.WriteLine("✅ Combat list refreshed");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Failed to refresh combat list after live combat");
                    Console.WriteLine($"❌ Failed to refresh combat list: {ex.Message}");
                }
            });
        }
    }


    private async Task CheckBackendStatusAsync()
    {
        try
        {
            var isAvailable = await _backendService.IsBackendAvailableAsync();
            UpdateStatusIndicator(isAvailable);
        }
        catch (Exception ex)
        {
            UpdateStatusIndicator(false, $"Error: {ex.Message}");
        }
    }

    private void UpdateStatusIndicator(bool isAvailable, string? message = null)
    {
        Dispatcher.Invoke(() =>
        {
            // Status wird später in der Sidebar angezeigt
            // Für jetzt nur in der Konsole loggen
            System.Diagnostics.Debug.WriteLine($"Backend Status: {(isAvailable ? "Available" : "Not Available")} - {message}");
        });
    }

    private void OnAnalysisProgress(object? sender, CombatAnalysisProgressEventArgs e)
    {
        Dispatcher.Invoke(() =>
        {
            
            System.Diagnostics.Debug.WriteLine($"Analysis Progress: {e.Message} ({e.ProgressPercentage}%)");
        });
    }

        private string GetDetailedErrorMessage(Exception ex)
        {
            var details = new System.Text.StringBuilder();
            details.AppendLine($"Error: {ex.Message}");
            
            if (ex.InnerException != null)
            {
                details.AppendLine($"\nDetails: {ex.InnerException.Message}");
                
                if (ex.InnerException.InnerException != null)
                {
                    details.AppendLine($"\nAdditional Info: {ex.InnerException.InnerException.Message}");
                }
            }
            
            // Wenn es ein Backend-Fehler ist, extrahiere die echte Fehlermeldung
            if (ex.Message.Contains("Backend returned error:"))
            {
                var match = System.Text.RegularExpressions.Regex.Match(ex.Message, @"Backend returned error:\s*(.+)");
                if (match.Success)
                {
                    details.AppendLine($"\nBackend Error: {match.Groups[1].Value}");
                }
            }
            
            return details.ToString();
        }

        private void AppendResult(string message)
    {
        // Results werden später in der Data Table angezeigt
        var logMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";
        
        // In Debug-Konsole schreiben
        System.Diagnostics.Debug.WriteLine(logMessage);
        
        // In Log-Datei schreiben (nur in Debug-Builds)
        #if DEBUG
        try
        {
            var logsDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            System.IO.Directory.CreateDirectory(logsDir);
            var logFile = System.IO.Path.Combine(logsDir, "frontend_debug.log");
            
            // Prüfe Dateigröße und rotiere bei > 5 MB
            var fileInfo = new System.IO.FileInfo(logFile);
            if (fileInfo.Exists && fileInfo.Length > 5 * 1024 * 1024)
            {
                // Rotiere: frontend_debug.log -> frontend_debug.log.1
                var oldFile = logFile + ".1";
                if (System.IO.File.Exists(oldFile))
                    System.IO.File.Delete(oldFile);
                System.IO.File.Move(logFile, oldFile);
            }
            
            System.IO.File.AppendAllText(logFile, logMessage + Environment.NewLine);
        }
        catch
        {
            // Ignore file logging errors
        }
        #endif
    }

    private async Task LoadCombatListIfAvailableAsync()
    {
        // Prüfe ob bereits gecachte Daten vorhanden sind
        if (LandingWindow.CachedCombatData != null && LandingWindow.CachedCombatData.Count > 0)
        {
            // Verwende die gecachten Daten
            await LoadCombatListFromCacheAsync();
            return;
        }

        // Falls keine gecachten Daten vorhanden sind, lade sie neu
        string? logFilePath = LandingWindow.SelectedLogFilePath;
        
        if (string.IsNullOrEmpty(logFilePath) || !File.Exists(logFilePath))
        {
            return;
        }

        // Lade die Combat List
        await LoadCombatListAsync(logFilePath);
    }

    private async Task LoadCombatListFromCacheAsync()
    {
        try
        {
            AppendResult($"=== Loading Combat List from Cache ===");
            AppendResult($"Cached combats: {LandingWindow.CachedCombatData?.Count ?? 0}");
            
            if (LandingWindow.CachedCombatData == null || LandingWindow.CachedCombatData.Count == 0)
            {
                AppendResult("No cached combat data available");
                return;
            }

            // Setze die Combat List mit den gecachten Daten
            CombatListViewComponent.SetCombats(LandingWindow.CachedCombatData);
            
            AppendResult($"✅ Combat list loaded from cache: {LandingWindow.CachedCombatData.Count} combats");
            
            // Setze den aktuellen Log-Pfad für Combat-Details
            _currentLogPath = LandingWindow.SelectedLogFilePath;
        }
        catch (Exception ex)
        {
            AppendResult($"❌ Error loading combat list from cache: {ex.Message}");
            _logger.LogError(ex, "Error loading combat list from cache");
        }
    }

    private async Task AutoLoadLogFileAsync()
    {
        // Verwende NUR die vom Landing Window ausgewählte Log-Datei
        string? logFilePath = LandingWindow.SelectedLogFilePath;
        
        if (string.IsNullOrEmpty(logFilePath) || !File.Exists(logFilePath))
        {
            return;
        }

        try
        {
            // Prüfe ob bereits gecachte Daten vorhanden sind
            if (LandingWindow.CachedCombatData != null && LandingWindow.CachedCombatData.Count > 0)
            {
                // Verwende gecachte Daten
                _loadedCombats = LandingWindow.CachedCombatData;
                
                // WICHTIG: Setze _currentLogPath für Combat-Details
                _currentLogPath = logFilePath;
                
                // Update UI direkt mit gecachten Daten
                Dispatcher.Invoke(() =>
                {
                    CombatListViewComponent.SetCombats(_loadedCombats);
                    
                    // Automatisch ersten Combat auswählen UND laden
                    if (_loadedCombats.Count > 0)
                    {
                        CombatListViewComponent.SelectFirst();
                        _ = LoadCombatDetailsAsync(_loadedCombats[0]);
                    }
                });
                
                _logger.LogInformation($"Loaded {_loadedCombats.Count} combats from cache");
            }
            else
            {
                // Fallback: Lade Daten neu
                await LoadCombatListAsync(logFilePath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error auto-loading combat log: {Message}", ex.Message);
        }
    }

    private async void RefreshButton_Click(object sender, RoutedEventArgs e)
    {
        await AutoLoadLogFileAsync();
    }

    private async Task LoadCombatListAsync(string logPath, bool isInitialLoad = true)
    {
        try
        {
            AppendResult($"=== Starting LoadCombatListAsync ===");
            AppendResult($"Log path: {logPath}");
            
            // Store log path for combat details loading
            _currentLogPath = logPath;
            
            // Backup und Trim der combatlog.log NUR beim initialen Laden
            // NICHT für Backup-Dateien (enthalten "backup" im Namen)
            // NICHT beim Refresh nach Combat-Ende
            if (isInitialLoad && 
                logPath.EndsWith("combatlog.log", StringComparison.OrdinalIgnoreCase) && 
                !logPath.Contains("backup", StringComparison.OrdinalIgnoreCase))
            {
                await CreateBackupAndTrimLogFileAsync(logPath);
            }
            
            // Cancel previous loading
            _loadingCancellation?.Cancel();
            _loadingCancellation = new CancellationTokenSource();


            AppendResult($"Calling GetAvailableCombatsWithProgressAsync...");
            
            // Get available combats
            var response = await _backendService.GetAvailableCombatsWithProgressAsync(
                logPath, 
                maxCombats: 20, 
                _loadingCancellation.Token);
            
            AppendResult($"Response received. Success: {response.Success}");

            if (!response.Success)
            {
                var errorDetails = response.Error ?? "Unbekannter Fehler";
                AppendResult($"Backend returned error: {errorDetails}");
                
                System.Windows.MessageBox.Show(
                    $"Die Combat-Log-Datei konnte nicht gelesen werden.\n\n" +
                    $"Fehler: {errorDetails}\n\n" +
                    $"Bitte prüfe die Datei oscr_api.log für weitere Details.",
                    "Fehler beim Laden",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
                return;
            }

            // Filter: Nur Combats mit Map anzeigen (keine Mock/Test-Daten)
            var validCombats = response.Combats
                .Where(c => !string.IsNullOrEmpty(c.Map) && c.Map != "Unknown") // Nur echte Combats
                .ToList();

            // Sort combats by date and time (newest first)
            var sortedCombats = validCombats
                .OrderByDescending(c => c.Date)
                .ThenByDescending(c => c.Time)
                .ToList();

            _loadedCombats = sortedCombats;

            // Update UI
            Dispatcher.Invoke(() =>
            {
                CombatListViewComponent.SetCombats(sortedCombats);
                
                // Automatisch ersten Combat auswählen UND laden (ohne Tab-Wechsel)
                if (sortedCombats.Count > 0)
                {
                    CombatListViewComponent.SelectFirst(); // Visuell auswählen (Event unterdrückt)
                    _ = LoadCombatDetailsAsync(sortedCombats[0]); // Manuell laden ohne Event
                }
            });

            AppendResult($"Loaded {sortedCombats.Count} combats from log file");
            
            
        }
        catch (OperationCanceledException)
        {
            AppendResult("Combat list loading was cancelled");
        }
        catch (Exception ex)
        {
            string errorDetails = GetDetailedErrorMessage(ex);
            
            AppendResult($"Failed to load combat list: {errorDetails}");
            
            System.Windows.MessageBox.Show(
                $"Die Combat-Log-Datei konnte nicht gelesen werden.\n\n" +
                $"{errorDetails}\n\n" +
                $"Log-Dateien zur Fehlersuche:\n" +
                $"- logs/oscr_backend.log (Backend-Fehler)\n" +
                $"- logs/frontend_debug.log (Frontend-Fehler)\n\n" +
                $"Beide Dateien befinden sich im logs/ Ordner der Anwendung.",
                "Fehler beim Laden",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
        finally
        {
            // Hide progress UI
            Dispatcher.Invoke(() =>
            {

            });
        }
    }

    private async void AnalyzeButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var logPath = LandingWindow.SelectedLogFilePath;
            if (string.IsNullOrEmpty(logPath) || !System.IO.File.Exists(logPath))
            {
                AppendResult("Please select a valid log file first.");
                return;
            }

            AppendResult($"Starting combat analysis for: {logPath}");
            var response = await _backendService.AnalyzeCombatLogAsync(logPath, maxCombats: 2);
            
            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { WriteIndented = true });
            AppendResult($"Combat Analysis Response:\n{json}");
        }
        catch (Exception ex)
        {
            AppendResult($"Combat analysis failed: {ex.Message}");
        }
    }

    private void OnCombatSelected(object? sender, CombatInfo selectedCombat)
    {
        System.Diagnostics.Debug.WriteLine($"Combat selected: {selectedCombat.Date} {selectedCombat.Time}");
        
        
        _ = LoadCombatDetailsAsync(selectedCombat);
    }

    private async Task LoadCombatDetailsAsync(CombatInfo combat)
    {
        try
        {
            if (string.IsNullOrEmpty(_currentLogPath))
            {
                System.Windows.MessageBox.Show("No log file selected", "Error", 
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            // Show loading state
            EmptyCombatStatsPanel.Visibility = Visibility.Collapsed;
            LoadingCombatStatsPanel.Visibility = Visibility.Visible;
            CombatStatsScrollViewer.Visibility = Visibility.Collapsed;
            LoadingCombatStatsText.Text = $"Loading combat {combat.Id} statistics...";

            // Cancel previous loading if any
            _loadingCancellation?.Cancel();
            _loadingCancellation = new CancellationTokenSource();

            // Analyze single combat
            var response = await _backendService.AnalyzeSingleCombatAsync(
                _currentLogPath, 
                combat.Id,
                _loadingCancellation.Token);

            if (!response.Success || response.Combats.Count == 0)
            {
                throw new Exception(response.Error ?? "No combat data received");
            }

            _currentCombatData = response.Combats[0];

            // Render Stats mit Service
            _statsRenderer.RenderCombatStats(
                CombatStatsItemsControl,
                _currentCombatData,
                _currentSortColumn,
                _sortAscending);
            
            // Update column header indicators to show initial sort
            StatsHeaderComponent.SetSortColumn(_currentSortColumn, _sortAscending);

            // Show data
            LoadingCombatStatsPanel.Visibility = Visibility.Collapsed;
            CombatStatsScrollViewer.Visibility = Visibility.Visible;
        }
        catch (OperationCanceledException)
        {
            // User cancelled - ignore
        }
        catch (Exception ex)
        {
            LoadingCombatStatsPanel.Visibility = Visibility.Collapsed;
            EmptyCombatStatsPanel.Visibility = Visibility.Visible;

            System.Windows.MessageBox.Show(
                $"Failed to load combat statistics:\n\n{ex.Message}", 
                "Error", 
                System.Windows.MessageBoxButton.OK, 
                System.Windows.MessageBoxImage.Error);
        }
    }

    private string FindDefaultCombatLogPath()
    {
        try
        {
            // Search in common STO installation directories
            var possiblePaths = new[]
            {
                @"C:\Program Files (x86)\Steam\steamapps\common\Star Trek Online\Star Trek Online\Live\logs\GameClient\combatlog.log",
                @"C:\Program Files\Steam\steamapps\common\Star Trek Online\Star Trek Online\Live\logs\GameClient\combatlog.log",
                @"D:\Steam\steamapps\common\Star Trek Online\Star Trek Online\Live\logs\GameClient\combatlog.log",
                @"E:\Steam\steamapps\common\Star Trek Online\Star Trek Online\Live\logs\GameClient\combatlog.log"
            };

            // Check all drives for SteamLibrary
            foreach (var drive in System.IO.DriveInfo.GetDrives().Where(d => d.IsReady && d.DriveType == System.IO.DriveType.Fixed))
            {
                var steamLibraryPath = System.IO.Path.Combine(drive.RootDirectory.FullName, @"SteamLibrary\steamapps\common\Star Trek Online\Star Trek Online\Live\logs\GameClient\combatlog.log");
                if (System.IO.File.Exists(steamLibraryPath))
                {
                    return steamLibraryPath;
                }
            }

            foreach (var path in possiblePaths)
            {
                if (System.IO.File.Exists(path))
                {
                    return path;
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error finding default combat log path: {ex.Message}");
        }

        return string.Empty;
    }

    /// <summary>
    /// Event-Handler für Spalten-Header-Klicks
    /// </summary>
    private void OnColumnHeaderClicked(object? sender, string columnName)
    {
        // Toggle Richtung wenn gleiche Spalte, sonst absteigend als Default
        if (_currentSortColumn == columnName)
        {
            _sortAscending = !_sortAscending;
        }
        else
        {
            _currentSortColumn = columnName;
            _sortAscending = false; // Neue Spalte startet mit absteigend
        }

        // Update header component
        StatsHeaderComponent.SetSortColumn(_currentSortColumn, _sortAscending);

        // Re-render mit neuer Sortierung
        if (_currentCombatData != null)
        {
            _statsRenderer.RenderCombatStats(
                CombatStatsItemsControl,
                _currentCombatData,
                _currentSortColumn,
                _sortAscending);
        }
    }

    private DateTime? ParseCombatLogTimestamp(string logLine)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(logLine))
                return null;

            // Zeitstempel ist am Anfang der Zeile bis zum ersten ::
            var parts = logLine.Split(new[] { "::" }, StringSplitOptions.None);
            if (parts.Length < 2)
                return null;

            var timestampStr = parts[0].Trim();
            
            // Format: YY:MM:DD:HH:MM:SS.ms
            var timeParts = timestampStr.Split(':');
            if (timeParts.Length < 6)
                return null;

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
        catch
        {
            return null;
        }
    }

    private async Task CreateBackupAndTrimLogFileAsync(string logPath)
    {
        try
        {
            var fileInfo = new System.IO.FileInfo(logPath);
            var fileSizeMB = fileInfo.Length / (1024.0 * 1024.0);
            
            _logger.LogInformation($"Processing combatlog.log - Size: {fileSizeMB:F2} MB");
            
            // Backup erstellen (immer!)
            var backupTimestamp = DateTime.Now.ToString("yyyy-MM-dd_HHmmss");
            var directory = System.IO.Path.GetDirectoryName(logPath);
            var fileName = System.IO.Path.GetFileNameWithoutExtension(logPath);
            var backupPath = System.IO.Path.Combine(directory ?? ".", $"{fileName}_backup_{backupTimestamp}.log");
            
            System.IO.File.Copy(logPath, backupPath, overwrite: false);
            _logger.LogInformation($"✅ Backup created: {backupPath}");
            AppendResult($"Backup created: {System.IO.Path.GetFileName(backupPath)}");
            
            // Alle Combats auslesen
            var response = await _backendService.GetAvailableCombatsWithProgressAsync(logPath, maxCombats: 999, CancellationToken.None);
            
            if (!response.Success)
            {
                _logger.LogWarning($"Could not read combats for trimming: {response.Error}");
                return;
            }
            
            _logger.LogInformation($"Found {response.Combats.Count} combats in log file");
            
            // Nur trimmen wenn mehr als 30 Combats vorhanden
            if (response.Combats.Count <= 30)
            {
                _logger.LogInformation($"No trimming needed: {response.Combats.Count} combats (≤ 30)");
                return;
            }
            
            _logger.LogInformation($"Trimming: {response.Combats.Count} → 30 combats");
            
            // DEBUG: Zeige erste und letzte Combats VOR dem Sortieren
            _logger.LogInformation($"First combat in list: {response.Combats.First().Date} {response.Combats.First().Time} (Byte {response.Combats.First().ByteStart})");
            _logger.LogInformation($"Last combat in list: {response.Combats.Last().Date} {response.Combats.Last().Time} (Byte {response.Combats.Last().ByteStart})");
            
            // Letzte 30 Combats behalten
            // WICHTIG: Backend gibt Combats bereits vom ältesten zum neuesten sortiert (nach ByteStart)
            // Wir nehmen die LETZTEN 30 (= neuesten)
            var combatsToKeep = response.Combats
                .OrderBy(c => c.ByteStart) // Sicherstellen dass nach Position sortiert
                .TakeLast(30) // Die LETZTEN 30 = neuesten
                .ToList();
            
            // DEBUG: Zeige was wir behalten
            _logger.LogInformation($"Keeping combats from {combatsToKeep.First().Date} {combatsToKeep.First().Time} to {combatsToKeep.Last().Date} {combatsToKeep.Last().Time}");
            
            if (combatsToKeep.Count == 0)
            {
                _logger.LogWarning("No combats to keep after filtering");
                return;
            }
            
            // NEUE STRATEGIE: Verwende Timestamps statt Byte-Positionen!
            // Viel zuverlässiger weil wir nicht mit Byte-Berechnungen kämpfen müssen
            var tempPath = logPath + ".tmp";
            
            // Hole Start- und End-Timestamps der zu behaltenden Combats
            var firstCombat = combatsToKeep.First();
            var lastCombat = combatsToKeep.Last();
            
            // Parse Timestamps (Format: "YYYY-MM-DD HH:MM:SS.f")
            var startTimestamp = DateTime.ParseExact(
                $"{firstCombat.Date} {firstCombat.Time}",
                "yyyy-MM-dd HH:mm:ss.f",
                System.Globalization.CultureInfo.InvariantCulture);
            var endTimestamp = DateTime.ParseExact(
                $"{lastCombat.Date} {lastCombat.Time}",
                "yyyy-MM-dd HH:mm:ss.f",
                System.Globalization.CultureInfo.InvariantCulture);
            
            _logger.LogInformation($"Keeping combats from {startTimestamp:yyyy-MM-dd HH:mm:ss} to {endTimestamp:yyyy-MM-dd HH:mm:ss}");
            _logger.LogInformation($"Using timestamp-based filtering (robust and reliable!)");
            
            long linesWritten = 0;
            long totalLinesRead = 0;
            
            using (var reader = new System.IO.StreamReader(logPath, System.Text.Encoding.UTF8))
            using (var writer = new System.IO.StreamWriter(tempPath, false, System.Text.Encoding.UTF8))
            {
                string? line;
                while ((line = reader.ReadLine()) != null)
                {
                    totalLinesRead++;
                    
                    // Extrahiere Timestamp aus der Log-Zeile
                    // Format: YY:MM:DD:HH:MM:SS.ms::...
                    var lineTimestamp = ParseCombatLogTimestamp(line);
                    
                    // Behalte Zeile wenn sie im Zeitbereich liegt
                    if (lineTimestamp.HasValue && lineTimestamp.Value >= startTimestamp && lineTimestamp.Value <= endTimestamp.AddMinutes(5))
                    {
                        await writer.WriteLineAsync(line);
                        linesWritten++;
                    }
                }
            }
            
            _logger.LogInformation($"Read {totalLinesRead} total lines, wrote {linesWritten} lines to trimmed file");
            
            // Ersetze Original mit getrimmter Datei
            System.IO.File.Delete(logPath);
            System.IO.File.Move(tempPath, logPath);
            
            var newFileInfo = new System.IO.FileInfo(logPath);
            var newSizeMB = newFileInfo.Length / (1024.0 * 1024.0);
            _logger.LogInformation($"✅ Trimming complete: {fileSizeMB:F2} MB → {newSizeMB:F2} MB (kept {combatsToKeep.Count} combats)");
            AppendResult($"Combat log trimmed: {fileSizeMB:F2} MB → {newSizeMB:F2} MB (kept last 30 combats)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during backup and trim");
            AppendResult($"Warning: Could not trim combat log: {ex.Message}");
            // Nicht kritisch, fortfahren
        }
    }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            
            
            base.OnClosing(e);
        }

        #region Title Bar Events

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Only allow dragging if not pinned
            if (!_isPinned)
            {
                this.DragMove();
            }
        }

        private void PinButton_Click(object sender, RoutedEventArgs e)
        {
            _isPinned = !_isPinned;
            
            // Update icon
            PinIcon.Text = _isPinned ? "📍" : "📌";
            
            // Update title bar cursor to show if draggable
            TitleBarBorder.Cursor = _isPinned ? Cursors.Arrow : Cursors.SizeAll;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        #endregion

    protected override void OnClosed(EventArgs e)
    {
        // Event abmelden
        if (_backendService != null)
        {
            _backendService.AnalysisProgress -= OnAnalysisProgress;
        }
        base.OnClosed(e);
    }
}
