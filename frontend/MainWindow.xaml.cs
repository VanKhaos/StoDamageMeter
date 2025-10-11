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
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : FluentWindow
{
    private readonly IOSCRBackendService _backendService;
    private readonly CombatStatsRenderer _statsRenderer;
    private readonly CombatLogWatcherService _logWatcher;
    private readonly ILogger<MainWindow> _logger;
    private LiveCombatViewModel? _liveCombatViewModel;
    private CancellationTokenSource? _loadingCancellation;
    private List<CombatInfo>? _loadedCombats;
    private CombatData? _currentCombatData;
    private string? _currentLogPath;
    
    // Sorting state
    private string _currentSortColumn = "DpsWithCompanions"; // Default
    private bool _sortAscending = false; // Default: descending

    public MainWindow()
    {
        InitializeComponent();
        
        // Backend Service aus DI Container holen
        _backendService = App.ServiceProvider.GetRequiredService<IOSCRBackendService>();
        _logWatcher = App.ServiceProvider.GetRequiredService<CombatLogWatcherService>();
        _logger = App.ServiceProvider.GetRequiredService<ILogger<MainWindow>>();
        
        // Stats Renderer initialisieren
        _statsRenderer = new CombatStatsRenderer((Style)this.FindResource("NoToggleIconExpanderStyle"));
        
        // Progress Event abonnieren
        _backendService.AnalysisProgress += OnAnalysisProgress;
        
        // Component Events verbinden
        CombatListViewComponent.CombatSelected += OnCombatSelected;
        StatsHeaderComponent.ColumnHeaderClicked += OnColumnHeaderClicked;
        
        // TEST LOG
        _logger.LogInformation("=== MainWindow Constructor - Logging Test ===");
        
        // Live Combat View Model initialisieren
        InitializeLiveCombatViewModel();
        
        // Initial Status Check
        _ = CheckBackendStatusAsync();
    }

    private void InitializeLiveCombatViewModel()
    {
        var logger = App.ServiceProvider.GetRequiredService<ILogger<LiveCombatViewModel>>();
        _liveCombatViewModel = new LiveCombatViewModel(
            _backendService,
            _logWatcher,
            logger,
            Dispatcher
        );

        _liveCombatViewModel.CombatCompleted += OnLiveCombatCompleted;

        // Set ViewModel to LiveCombatView Component
        LiveCombatViewComponent.SetViewModel(_liveCombatViewModel);
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
                    // Combat-Liste neu laden
                    await LoadCombatListAsync(_currentLogPath);
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

    private async void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        // DEBUG: Console Output falls Logging nicht funktioniert
        Console.WriteLine($"[DEBUG] Tab changed to index: {MainTabControl.SelectedIndex}");
        
        var logger = App.ServiceProvider.GetRequiredService<ILogger<MainWindow>>();
        logger.LogInformation($"Tab changed to index: {MainTabControl.SelectedIndex}");
        
        if (MainTabControl.SelectedIndex == 1) // Live Combat Tab
        {
            Console.WriteLine($"[DEBUG] Live Combat tab selected. LogPath: {_currentLogPath ?? "NULL"}");
            logger.LogInformation($"Live Combat tab selected. LogPath: {_currentLogPath ?? "NULL"}");
            
            // Starte Live-Modus wenn Log-Path vorhanden ist
            if (!string.IsNullOrEmpty(_currentLogPath) && _liveCombatViewModel != null)
            {
                Console.WriteLine("[DEBUG] Starting live parsing...");
                logger.LogInformation("Starting live parsing...");
                await _liveCombatViewModel.StartLiveParsing(_currentLogPath);
            }
            else
            {
                Console.WriteLine($"[DEBUG] Cannot start - LogPath empty: {string.IsNullOrEmpty(_currentLogPath)}, ViewModel null: {_liveCombatViewModel == null}");
                logger.LogWarning($"Cannot start live parsing. LogPath null: {string.IsNullOrEmpty(_currentLogPath)}, ViewModel null: {_liveCombatViewModel == null}");
            }
        }
        else // Historical Damage Out Tab
        {
            Console.WriteLine("[DEBUG] Historical tab selected");
            logger.LogInformation("Historical tab selected, stopping live parsing");
            
            // Stoppe Live-Modus
            if (_liveCombatViewModel != null)
            {
                await _liveCombatViewModel.StopLiveParsing();
            }
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
            // Progress in UI anzeigen
            LoadingProgressBar.Value = e.ProgressPercentage;
            LoadingStatusText.Text = e.Message;
            
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
        
        // In Log-Datei schreiben
        try
        {
            var logsDir = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            System.IO.Directory.CreateDirectory(logsDir);
            var logFile = System.IO.Path.Combine(logsDir, "frontend_debug.log");
            System.IO.File.AppendAllText(logFile, logMessage + Environment.NewLine);
        }
        catch
        {
            // Ignore file logging errors
        }
    }

    private async void BrowseButton_Click(object sender, RoutedEventArgs e)
    {
        var openFileDialog = new OpenFileDialog
        {
            Title = "Select Combat Log File",
            Filter = "Log files (*.log)|*.log|All files (*.*)|*.*",
            DefaultExt = "log"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            LogFilePathTextBox.Text = openFileDialog.FileName;
            
            // Automatisch Combat-Liste laden
            await LoadCombatListAsync(openFileDialog.FileName);
        }
    }

    private async Task LoadCombatListAsync(string logPath)
    {
        try
        {
            AppendResult($"=== Starting LoadCombatListAsync ===");
            AppendResult($"Log path: {logPath}");
            
            // Store log path for combat details loading
            _currentLogPath = logPath;
            
            // Cancel previous loading
            _loadingCancellation?.Cancel();
            _loadingCancellation = new CancellationTokenSource();

            // Show progress UI
            LoadingProgressBar.Visibility = Visibility.Visible;
            LoadingStatusText.Visibility = Visibility.Visible;
            BrowseButton.IsEnabled = false;

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

            // Sort combats by date and time (newest first)
            var sortedCombats = response.Combats
                .OrderByDescending(c => c.Date)
                .ThenByDescending(c => c.Time)
                .ToList();

            _loadedCombats = sortedCombats;

            // Update UI
            Dispatcher.Invoke(() =>
            {
                CombatListViewComponent.SetCombats(sortedCombats);
                
                // Automatisch ersten Combat auswählen
                if (sortedCombats.Count > 0)
                {
                    CombatListViewComponent.SelectFirst();
                }
            });

            AppendResult($"Loaded {sortedCombats.Count} combats from log file");
            
            // ✅ Live Combat Tab aktivieren nach erfolgreichem Laden
            Dispatcher.Invoke(() =>
            {
                LiveCombatTab.IsEnabled = true;
                _logger.LogInformation("Live Combat tab enabled after combat log loaded");
            });
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
                LoadingProgressBar.Visibility = Visibility.Collapsed;
                LoadingStatusText.Visibility = Visibility.Collapsed;
                BrowseButton.IsEnabled = true;
            });
        }
    }

    private async void AnalyzeButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var logPath = LogFilePathTextBox.Text;
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
