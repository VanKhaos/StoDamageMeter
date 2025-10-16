using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using StoDamageMeter.Models;
using StoDamageMeter.Services;
using StoDamageMeter.ViewModels;

namespace StoDamageMeter.Components.LiveCombat
{
    public partial class LiveCombatView : UserControl
    {
        private LiveCombatViewModel? _viewModel;
        private readonly CombatStatsRenderer _statsRenderer;
        private LiveCombatOverlay? _overlayWindow;
        
        /// <summary>
        /// Prüft ob das Live Combat Overlay aktuell geöffnet/sichtbar ist
        /// </summary>
        public bool IsOverlayVisible => _overlayWindow != null && _overlayWindow.IsVisible;

        public LiveCombatView()
        {
            InitializeComponent();
            
            // Lade den GLEICHEN Expander-Style wie im Dashboard (NoToggleIconExpanderStyle)
            var expanderStyle = Application.Current.TryFindResource("NoToggleIconExpanderStyle") as Style;
            _statsRenderer = new CombatStatsRenderer(expanderStyle!);
        }

        public void SetViewModel(LiveCombatViewModel viewModel)
        {
            _viewModel = viewModel;
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_viewModel == null) return;

            Console.WriteLine($"🔔 PropertyChanged: {e.PropertyName}");

            Dispatcher.Invoke(() =>
            {
                switch (e.PropertyName)
                {
                    case nameof(LiveCombatViewModel.StatusText):
                        Console.WriteLine($"  → Updating StatusText: {_viewModel.StatusText}");
                        UpdateStatusText();
                        break;
                    case nameof(LiveCombatViewModel.CurrentCombat):
                        Console.WriteLine($"  → CurrentCombat changed! Updating UI...");
                        UpdateCombatInfo();
                        UpdateLiveStats();
                        break;
                    case nameof(LiveCombatViewModel.CombatDuration):
                        if (CombatDurationText != null)
                        {
                            CombatDurationText.Text = _viewModel.CombatDuration;
                        }
                        break;
                    case nameof(LiveCombatViewModel.TotalDPS):
                        if (TotalDPSText != null)
                        {
                            TotalDPSText.Text = $"Total DPS: {_viewModel.TotalDPS:N0}";
                        }
                        break;
                }
            });
        }

        private void UpdateStatusText()
        {
            if (_viewModel == null) return;

            var isActive = _viewModel.CurrentCombat != null;

            // Zeige/Verstecke Combat-Info-Panel
            if (CombatInfoPanel != null)
            {
                CombatInfoPanel.Visibility = isActive ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private void UpdateCombatInfo()
        {
            if (_viewModel?.CurrentCombat == null) return;

            var combat = _viewModel.CurrentCombat;

            // Combat-Type und Icon
            if (CombatTypeIcon != null && CombatTypeText != null)
            {
                CombatTypeIcon.Text = combat.Icon;
                CombatTypeText.Text = $"{combat.Type} Combat";
            }

            // Duration wird von ViewModel automatisch aktualisiert

            // Total DPS
            if (TotalDPSText != null)
            {
                TotalDPSText.Text = $"Total DPS: {combat.TotalDPS:N0}";
            }
        }

        private void UpdateLiveStats()
        {
            if (_viewModel == null || LiveStatsItemsControl == null)
            {
                Console.WriteLine("❌ UpdateLiveStats: ViewModel or Control is NULL");
                return;
            }

            // 💾 WICHTIG: Expander-States speichern VOR dem Clear
            var expandedStates = new Dictionary<string, bool>();
            foreach (var item in LiveStatsItemsControl.Items)
            {
                if (item is Expander expander && expander.Header is Grid headerGrid)
                {
                    // Spielername aus dem Header extrahieren
                    var nameText = headerGrid.Children.OfType<TextBlock>()
                        .FirstOrDefault(tb => tb.Name == "PlayerNameText");
                    if (nameText != null && !string.IsNullOrEmpty(nameText.Text))
                    {
                        expandedStates[nameText.Text] = expander.IsExpanded;
                    }
                }
            }

            // Clear alte Stats
            LiveStatsItemsControl.Items.Clear();

            Console.WriteLine($"🔄 UpdateLiveStats: CurrentCombat={(_viewModel.CurrentCombat != null ? "SET" : "NULL")}, LivePlayerStats.Count={_viewModel.LivePlayerStats.Count}, SavedStates={expandedStates.Count}");

            if (_viewModel.CurrentCombat == null || _viewModel.LivePlayerStats.Count == 0)
            {
                Console.WriteLine("⚪ Showing Empty State (no data)");
                // Zeige Empty State
                if (EmptyLiveStatsPanel != null)
                {
                    EmptyLiveStatsPanel.Visibility = Visibility.Visible;
                }
                if (LiveStatsScrollViewer != null)
                {
                    LiveStatsScrollViewer.Visibility = Visibility.Collapsed;
                }
                return;
            }

            Console.WriteLine("🟢 Hiding Empty State, showing table");
            // Verstecke Empty State, zeige Tabelle
            if (EmptyLiveStatsPanel != null)
            {
                EmptyLiveStatsPanel.Visibility = Visibility.Collapsed;
            }
            if (LiveStatsScrollViewer != null)
            {
                LiveStatsScrollViewer.Visibility = Visibility.Visible;
            }

            // Render Player-Stats mit CombatStatsRenderer
            try
            {
                Console.WriteLine($"🎨 Rendering {_viewModel.CurrentCombat.Players.Count} players...");
                _statsRenderer.RenderCombatStats(
                    LiveStatsItemsControl,
                    _viewModel.CurrentCombat,
                    "TotalDamageWithCompanions",
                    false // Absteigend sortiert
                );
                Console.WriteLine($"✅ Rendered {LiveStatsItemsControl.Items.Count} items to LiveStatsItemsControl");
                
                // ♻️ Expander-States WIEDERHERSTELLEN nach dem Rendering
                int restoredCount = 0;
                foreach (var item in LiveStatsItemsControl.Items)
                {
                    if (item is Expander expander && expander.Header is Grid headerGrid)
                    {
                        var nameText = headerGrid.Children.OfType<TextBlock>()
                            .FirstOrDefault(tb => tb.Name == "PlayerNameText");
                        if (nameText != null && !string.IsNullOrEmpty(nameText.Text))
                        {
                            if (expandedStates.TryGetValue(nameText.Text, out bool wasExpanded))
                            {
                                expander.IsExpanded = wasExpanded;
                                if (wasExpanded) restoredCount++;
                            }
                        }
                    }
                }
                if (restoredCount > 0)
                {
                    Console.WriteLine($"♻️ Restored {restoredCount} expanded states");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Render error: {ex.Message}");
                var errorText = new TextBlock
                {
                    Text = $"Fehler beim Rendern: {ex.Message}",
                    Foreground = Brushes.Red,
                    FontSize = 12,
                    Margin = new Thickness(10)
                };
                LiveStatsItemsControl.Items.Add(errorText);
            }
        }

        public async Task StartLiveMode(string logPath)
        {
            if (_viewModel != null)
            {
                await _viewModel.StartLiveParsing(logPath);
            }
        }

        public async Task StopLiveMode()
        {
            if (_viewModel != null)
            {
                await _viewModel.StopLiveParsing();
            }
        }

        private void ShowOverlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (_overlayWindow == null)
            {
                _overlayWindow = new LiveCombatOverlay();
                
                if (_viewModel != null)
                {
                    _overlayWindow.SetViewModel(_viewModel);
                }
                
                _overlayWindow.Show();
            }
            else
            {
                if (_overlayWindow.Visibility == Visibility.Visible)
                {
                    _overlayWindow.Activate();
                }
                else
                {
                    _overlayWindow.Show();
                }
            }
        }

        /// <summary>
        /// Cleanup-Methode zum Schließen des Overlays beim Beenden der Anwendung
        /// </summary>
        public void Cleanup()
        {
            if (_overlayWindow != null)
            {
                _overlayWindow.ForceClose();
                _overlayWindow = null;
            }
        }
    }
}


