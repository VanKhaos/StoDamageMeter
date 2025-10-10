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
    private CombatData? _currentCombatData;
    private string? _currentLogPath;

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
            
            // Store log path for combat details loading
            _currentLogPath = logPath;
            
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
            System.Diagnostics.Debug.WriteLine($"Combat selected: {selectedCombat.Date} {selectedCombat.Time}");
            _ = LoadCombatDetailsAsync(selectedCombat);
        }
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

            // Populate TreeView
            PopulateCombatStatsTreeView(_currentCombatData);

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

    private void PopulateCombatStatsTreeView(CombatData combatData)
    {
        CombatStatsItemsControl.Items.Clear();

        // Sortiere Players nach DPS (inkl. Companions, absteigend)
        var sortedPlayers = combatData.Players.Values
            .OrderByDescending(p => p.DpsWithCompanions)
            .ToList();

        foreach (var player in sortedPlayers)
        {
            // Container für Player und Abilities
            var playerContainer = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 4)
            };

            // Player Row mit Expander
            var playerExpander = new Expander
            {
                IsExpanded = false,
                Background = new SolidColorBrush(Color.FromRgb(26, 26, 26)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(0)
            };

            // Player Header Grid (sichtbar auch wenn collapsed)
            var playerHeaderGrid = new Grid
            {
                Background = new SolidColorBrush(Color.FromRgb(26, 26, 26))
            };
            
            playerHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
            playerHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            playerHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            playerHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            playerHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            playerHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            playerHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            // Player Name mit Icon
            var playerNamePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(8, 8, 8, 8)
            };
            
            var expandIcon = new System.Windows.Controls.TextBlock
            {
                Text = "▶",
                FontSize = 10,
                Foreground = new SolidColorBrush(Color.FromRgb(91, 155, 213)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0)
            };
            
            var playerNameText = new System.Windows.Controls.TextBlock
            {
                Text = player.Name ?? "Unknown",
                FontSize = 12,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Center
            };
            
            playerNamePanel.Children.Add(expandIcon);
            playerNamePanel.Children.Add(playerNameText);
            
            Grid.SetColumn(playerNamePanel, 0);
            playerHeaderGrid.Children.Add(playerNamePanel);

            // Player Stats (mit Companions)
            var playerStats = new[]
            {
                CreateTableCell($"{player.DpsWithCompanions:N0}", false),
                CreateTableCell($"{player.TotalDamageWithCompanions:N0}", false),
                CreateTableCell($"{player.Debuff:F1}%", false),
                CreateTableCell($"{player.MaxOneHit:N0}", false),
                CreateTableCell($"{player.CritPercent:F1}%", false),
                CreateTableCell($"{player.AccuracyPercent:F1}%", false)
            };

            for (int i = 0; i < playerStats.Length; i++)
            {
                Grid.SetColumn(playerStats[i], i + 1);
                playerHeaderGrid.Children.Add(playerStats[i]);
            }

            // Expander Icon ändern bei Expand/Collapse
            playerExpander.Expanded += (s, e) => expandIcon.Text = "▼";
            playerExpander.Collapsed += (s, e) => expandIcon.Text = "▶";

            playerExpander.Header = playerHeaderGrid;

            // Player Content: Abilities + Companions (gemischt nach Total Damage sortiert)
            var playerContentPanel = new StackPanel
            {
                Background = new SolidColorBrush(Color.FromRgb(16, 16, 16)),
                Margin = new Thickness(0)
            };

            // Erstelle eine Liste aller Items (Abilities + Companions) mit Total Damage zum Sortieren
            var allItems = new List<(double totalDamage, bool isCompanion, object item)>();
            
            // Füge Abilities hinzu
            foreach (var ability in player.Abilities)
            {
                allItems.Add((ability.TotalDamage, false, ability));
            }
            
            // Füge Companions hinzu
            foreach (var companion in player.Companions)
            {
                allItems.Add((companion.TotalDamage, true, companion));
            }
            
            // Sortiere alles nach Total Damage
            var sortedItems = allItems.OrderByDescending(x => x.totalDamage).ToList();

            // Rendere Items in sortierter Reihenfolge
            foreach (var (totalDamage, isCompanion, item) in sortedItems)
            {
                if (!isCompanion)
                {
                    // Ability
                    var ability = (AbilityStatistics)item;
                    var abilityContainer = new Border
                    {
                        Background = new SolidColorBrush(Color.FromRgb(16, 16, 16)),
                        Margin = new Thickness(0, 1, 0, 0),
                        Padding = new Thickness(32, 0, 0, 0) // Links-Einrückung
                    };
                    
                    var abilityGrid = new Grid();
                    
                    abilityGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                    abilityGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    abilityGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    abilityGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    abilityGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    abilityGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                    abilityGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                    var abilityNameText = new System.Windows.Controls.TextBlock
                    {
                        Text = ability.Name,
                        FontSize = 11,
                        Foreground = new SolidColorBrush(Color.FromRgb(176, 176, 176)),
                        VerticalAlignment = VerticalAlignment.Center,
                        Padding = new Thickness(8, 6, 8, 6),
                        Opacity = 0.9
                    };
                    Grid.SetColumn(abilityNameText, 0);
                    abilityGrid.Children.Add(abilityNameText);

                    var abilityStats = new[]
                    {
                        CreateTableCell($"{ability.Dps:N0}", false, 0.85),
                        CreateTableCell($"{ability.TotalDamage:N0}", false, 0.85),
                        CreateTableCell("-", false, 0.85),
                        CreateTableCell($"{ability.MaxHit:N0}", false, 0.85),
                        CreateTableCell($"{ability.CritPercent:F1}%", false, 0.85),
                        CreateTableCell($"{ability.AccuracyPercent:F1}%", false, 0.85)
                    };

                    for (int i = 0; i < abilityStats.Length; i++)
                    {
                        Grid.SetColumn(abilityStats[i], i + 1);
                        abilityGrid.Children.Add(abilityStats[i]);
                    }

                    abilityContainer.Child = abilityGrid;
                    playerContentPanel.Children.Add(abilityContainer);
                }
                else
                {
                    // Companion
                    var companion = (CompanionStatistics)item;
                    
                    // Companion Expander (eingerückt wie Abilities)
                    var companionExpander = new Expander
                {
                    IsExpanded = false,
                    Background = new SolidColorBrush(Color.FromRgb(20, 20, 20)),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                    BorderThickness = new Thickness(0, 1, 0, 0),
                    Padding = new Thickness(0),
                    Margin = new Thickness(32, 1, 0, 0) // Einrückung wie Abilities
                };

                // Companion Header Grid
                var companionHeaderGrid = new Grid
                {
                    Background = new SolidColorBrush(Color.FromRgb(20, 20, 20))
                };
                
                companionHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                companionHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                companionHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                companionHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                companionHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                companionHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                companionHeaderGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                // Companion Name mit Icon
                var companionNamePanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(8, 6, 8, 6)
                };
                
                var companionExpandIcon = new System.Windows.Controls.TextBlock
                {
                    Text = "▶",
                    FontSize = 9,
                    Foreground = new SolidColorBrush(Color.FromRgb(91, 155, 213)),
                    VerticalAlignment = VerticalAlignment.Center,
                    Margin = new Thickness(0, 0, 6, 0)
                };
                
                var companionNameText = new System.Windows.Controls.TextBlock
                {
                    Text = $"🤖 {companion.Name ?? "Unknown"}",
                    FontSize = 11,
                    FontWeight = FontWeights.Normal,
                    Foreground = new SolidColorBrush(Color.FromRgb(176, 176, 176)),
                    VerticalAlignment = VerticalAlignment.Center,
                    Opacity = 0.95
                };
                
                companionNamePanel.Children.Add(companionExpandIcon);
                companionNamePanel.Children.Add(companionNameText);
                
                Grid.SetColumn(companionNamePanel, 0);
                companionHeaderGrid.Children.Add(companionNamePanel);

                // Companion Stats
                var companionStats = new[]
                {
                    CreateTableCell($"{companion.Dps:N0}", false, 0.8),
                    CreateTableCell($"{companion.TotalDamage:N0}", false, 0.8),
                    CreateTableCell($"{companion.Debuff:F1}%", false, 0.8),
                    CreateTableCell($"{companion.MaxOneHit:N0}", false, 0.8),
                    CreateTableCell($"{companion.CritPercent:F1}%", false, 0.8),
                    CreateTableCell($"{companion.AccuracyPercent:F1}%", false, 0.8)
                };

                for (int i = 0; i < companionStats.Length; i++)
                {
                    Grid.SetColumn(companionStats[i], i + 1);
                    companionHeaderGrid.Children.Add(companionStats[i]);
                }

                // Expander Icon ändern
                companionExpander.Expanded += (s, e) => companionExpandIcon.Text = "▼";
                companionExpander.Collapsed += (s, e) => companionExpandIcon.Text = "▶";

                companionExpander.Header = companionHeaderGrid;

                // Companion Abilities
                if (companion.Abilities.Count > 0)
                {
                    var companionAbilitiesPanel = new StackPanel
                    {
                        Background = new SolidColorBrush(Color.FromRgb(12, 12, 12)),
                        Margin = new Thickness(0)
                    };

                    var sortedCompanionAbilities = companion.Abilities
                        .OrderByDescending(a => a.Dps)
                        .ToList();

                    foreach (var ability in sortedCompanionAbilities)
                    {
                        var abilityContainer = new Border
                        {
                            Background = new SolidColorBrush(Color.FromRgb(12, 12, 12)),
                            Margin = new Thickness(0, 1, 0, 0),
                            Padding = new Thickness(32, 0, 0, 0) // Zusätzliche Einrückung (relativ zum Companion)
                        };
                        
                        var abilityGrid = new Grid();
                        
                        abilityGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(2, GridUnitType.Star) });
                        abilityGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        abilityGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        abilityGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        abilityGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        abilityGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
                        abilityGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                        var abilityNameText = new System.Windows.Controls.TextBlock
                        {
                            Text = ability.Name,
                            FontSize = 10,
                            Foreground = new SolidColorBrush(Color.FromRgb(156, 156, 156)),
                            VerticalAlignment = VerticalAlignment.Center,
                            Padding = new Thickness(8, 5, 8, 5),
                            Opacity = 0.85
                        };
                        Grid.SetColumn(abilityNameText, 0);
                        abilityGrid.Children.Add(abilityNameText);

                        var abilityStats = new[]
                        {
                            CreateTableCell($"{ability.Dps:N0}", false, 0.75),
                            CreateTableCell($"{ability.TotalDamage:N0}", false, 0.75),
                            CreateTableCell("-", false, 0.75),
                            CreateTableCell($"{ability.MaxHit:N0}", false, 0.75),
                            CreateTableCell($"{ability.CritPercent:F1}%", false, 0.75),
                            CreateTableCell($"{ability.AccuracyPercent:F1}%", false, 0.75)
                        };

                        for (int i = 0; i < abilityStats.Length; i++)
                        {
                            Grid.SetColumn(abilityStats[i], i + 1);
                            abilityGrid.Children.Add(abilityStats[i]);
                        }

                        abilityContainer.Child = abilityGrid;
                        companionAbilitiesPanel.Children.Add(abilityContainer);
                    }

                    companionExpander.Content = companionAbilitiesPanel;
                }

                    playerContentPanel.Children.Add(companionExpander);
                }
            }

            playerExpander.Content = playerContentPanel;

            playerContainer.Children.Add(playerExpander);
            CombatStatsItemsControl.Items.Add(playerContainer);
        }
    }

    private System.Windows.Controls.TextBlock CreateTableCell(string text, bool isHeader, double opacity = 1.0)
    {
        return new System.Windows.Controls.TextBlock
        {
            Text = text,
            FontSize = isHeader ? 12 : 11,
            Foreground = isHeader 
                ? new SolidColorBrush(Colors.White) 
                : new SolidColorBrush(Color.FromRgb(176, 176, 176)),
            HorizontalAlignment = HorizontalAlignment.Right,
            VerticalAlignment = VerticalAlignment.Center,
            Padding = new Thickness(8, 6, 8, 6),
            Opacity = opacity,
            FontWeight = isHeader ? FontWeights.SemiBold : FontWeights.Normal
        };
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