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
using StoDamageMeter.Services;
using StoDamageMeter.Models;
using System.Text.Json;
using Wpf.Ui.Controls;

namespace StoDamageMeter;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : FluentWindow
{
    private readonly IOSCRBackendService _backendService;
    private CancellationTokenSource? _loadingCancellation;
    private List<CombatInfo>? _loadedCombats;

    public MainWindow()
    {
        InitializeComponent();
        
        // Backend Service aus DI Container holen
        _backendService = App.ServiceProvider.GetRequiredService<IOSCRBackendService>();
        
        // Progress Event abonnieren
        _backendService.AnalysisProgress += OnAnalysisProgress;
        
        // Initial Status Check
        _ = CheckBackendStatusAsync();
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

    private void AppendResult(string message)
    {
        // Results werden später in der Data Table angezeigt
        var logMessage = $"[{DateTime.Now:HH:mm:ss}] {message}";
        
        // In Debug-Konsole schreiben
        System.Diagnostics.Debug.WriteLine(logMessage);
        
        // In Log-Datei schreiben
        try
        {
            var logFile = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "frontend_debug.log");
            System.IO.File.AppendAllText(logFile, logMessage + Environment.NewLine);
        }
        catch
        {
            // Ignore file logging errors
        }
    }

    // CheckStatusButton_Click entfernt - Button existiert nicht mehr in der neuen UI

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
            
            // Cancel previous loading
            _loadingCancellation?.Cancel();
            _loadingCancellation = new CancellationTokenSource();

            // Show progress UI
            LoadingProgressBar.Visibility = Visibility.Visible;
            LoadingStatusText.Visibility = Visibility.Visible;
            EmptyCombatListText.Visibility = Visibility.Collapsed;
            BrowseButton.IsEnabled = false;

            AppendResult($"Calling GetAvailableCombatsWithProgressAsync...");
            
            // Get available combats
            var response = await _backendService.GetAvailableCombatsWithProgressAsync(
                logPath, 
                maxCombats: 100, 
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
                CombatListView.ItemsSource = sortedCombats;
                
                if (sortedCombats.Count == 0)
                {
                    EmptyCombatListText.Text = "No combats found in this log file.";
                    EmptyCombatListText.Visibility = Visibility.Visible;
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
            var errorMessage = ex.Message;
            var innerException = ex.InnerException?.Message ?? "";
            
            AppendResult($"Failed to load combat list: {errorMessage}");
            if (!string.IsNullOrEmpty(innerException))
            {
                AppendResult($"Inner exception: {innerException}");
            }
            
            System.Windows.MessageBox.Show(
                $"Die Combat-Log-Datei konnte nicht gelesen werden.\n\n" +
                $"Fehler: {errorMessage}\n\n" +
                $"{(!string.IsNullOrEmpty(innerException) ? $"Details: {innerException}\n\n" : "")}" +
                $"Bitte prüfe die Datei oscr_api.log im Deploy-Ordner für weitere Details.",
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

    // HealthCheckButton_Click und ListCombatsButton_Click entfernt - Buttons existieren nicht mehr in der neuen UI

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

    private void CombatListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CombatListView.SelectedItem is CombatInfo selectedCombat)
        {
            // Hier später die Combat-Details anzeigen
            System.Diagnostics.Debug.WriteLine($"Combat selected: {selectedCombat.Date} {selectedCombat.Time}");
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