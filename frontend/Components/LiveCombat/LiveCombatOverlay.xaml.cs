using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using StoDamageMeter.ViewModels;

namespace StoDamageMeter.Components.LiveCombat
{
    public partial class LiveCombatOverlay : Window
    {
        private LiveCombatViewModel? _viewModel;
        private DispatcherTimer? _durationUpdateTimer;
        private bool _isPinned = false;
        private double _playerFontSize = 16; // Standard-Schriftgröße (erhöht auf 16)
        
        // Demo-Modus für Entwicklung
        private bool _isDemoMode = false;
        private DispatcherTimer? _demoUpdateTimer;
        private System.Collections.ObjectModel.ObservableCollection<Models.PlayerStatistics> _demoPlayers = new();
        private DateTime _demoStartTime;
        private Random _random = new Random();
        
        // Spieler-Farben für konsistente Zeilen-Hintergründe
        private System.Collections.Generic.Dictionary<string, Color> _playerColors = new();
        private readonly Color[] _availableColors = new[]
        {
            // Erste 5 Farben: Maximal unterscheidbar für Standard 5-Spieler-Teams
            Color.FromArgb(50, 33, 150, 243),    // #1 Blau (Bright Blue)
            Color.FromArgb(50, 76, 175, 80),     // #2 Grün (Green)
            Color.FromArgb(50, 255, 152, 0),     // #3 Orange (Orange)
            Color.FromArgb(50, 156, 39, 176),    // #4 Lila (Purple)
            Color.FromArgb(50, 244, 67, 54),     // #5 Rot (Red)
            
            // Farben 6-10: Zusätzliche deutlich unterscheidbare Töne
            Color.FromArgb(50, 0, 188, 212),     // #6 Cyan (Cyan)
            Color.FromArgb(50, 255, 235, 59),    // #7 Gelb (Yellow)
            Color.FromArgb(50, 233, 30, 99),     // #8 Pink (Pink)
            Color.FromArgb(50, 139, 195, 74),    // #9 Hellgrün (Light Green)
            Color.FromArgb(50, 255, 87, 34),     // #10 Deep Orange (Deep Orange)
            
            // Farben 11-15: Ergänzende Töne für größere Teams
            Color.FromArgb(50, 103, 58, 183),    // #11 Deep Purple (Deep Purple)
            Color.FromArgb(50, 0, 150, 136),     // #12 Teal (Teal)
            Color.FromArgb(50, 205, 220, 57),    // #13 Lime (Lime)
            Color.FromArgb(50, 121, 85, 72),     // #14 Brown (Brown)
            Color.FromArgb(50, 96, 125, 139)     // #15 Blue Grey (Blue Grey)
        };
        private int _nextColorIndex = 0;
        
        // Rank-Tracking für Animationen
        private class PlayerRowInfo
        {
            public string PlayerName { get; set; } = "";
            public int LastRank { get; set; }
            public Border? RowBorder { get; set; }
        }
        private System.Collections.Generic.Dictionary<string, PlayerRowInfo> _playerRowCache = new();

        public LiveCombatOverlay()
        {
            InitializeComponent();
            
            // Optimierte Höhe für 5 Spieler
            Height = 200;
            
            // Timer für Duration-Updates (1x pro Sekunde statt bei jedem Event)
            _durationUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _durationUpdateTimer.Tick += DurationTimer_Tick;
        }

        private void DurationTimer_Tick(object? sender, EventArgs e)
        {
            if (_viewModel != null && DurationText != null && _viewModel.CurrentCombat != null)
            {
                DurationText.Text = $"⏱ {_viewModel.CombatDuration}";
            }
        }

        public void SetViewModel(LiveCombatViewModel viewModel)
        {
            _viewModel = viewModel;
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            
            // Initial Update
            UpdateOverlay();
        }

        private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (_viewModel == null) return;

            Dispatcher.Invoke(() =>
            {
                switch (e.PropertyName)
                {
                    case nameof(LiveCombatViewModel.CurrentCombat):
                    case nameof(LiveCombatViewModel.LivePlayerStats):
                        UpdateOverlay();
                        break;
                    // Duration wird jetzt vom Timer aktualisiert, nicht hier
                }
            });
        }

        private void UpdateOverlay()
        {
            if (PlayersItemsControl == null)
            {
                return;
            }

            // Demo-Modus: Nutze Demo-Daten
            if (_isDemoMode)
            {
                UpdateOverlayDemo();
                return;
            }

            // Normal-Modus: ViewModel benötigt
            if (_viewModel == null)
            {
                return;
            }

            if (_viewModel.CurrentCombat == null || _viewModel.LivePlayerStats.Count == 0)
            {
                // Show empty state
                _durationUpdateTimer?.Stop();
                
                if (EmptyPanel != null)
                {
                    EmptyPanel.Visibility = Visibility.Visible;
                }
                if (DataScrollViewer != null)
                {
                    DataScrollViewer.Visibility = Visibility.Collapsed;
                }
                if (TitleText != null)
                {
                    TitleText.Text = "🚀 Live Combat";
                }
                if (DurationText != null)
                {
                    DurationText.Text = "";
                }
                return;
            }

            // Show data
            if (EmptyPanel != null)
            {
                EmptyPanel.Visibility = Visibility.Collapsed;
            }
            if (DataScrollViewer != null)
            {
                DataScrollViewer.Visibility = Visibility.Visible;
            }

            // Starte Timer für Duration-Updates
            if (_durationUpdateTimer != null && !_durationUpdateTimer.IsEnabled)
            {
                _durationUpdateTimer.Start();
            }

            // Update title (kompakt)
            if (TitleText != null && _viewModel.CurrentCombat != null)
            {
                TitleText.Text = $"🚀 {_viewModel.CurrentCombat.Type}";
            }
            if (DurationText != null)
            {
                DurationText.Text = $"⏱ {_viewModel.CombatDuration}";
            }

            // Sort by Total Damage (descending)
            var sortedPlayers = _viewModel.LivePlayerStats
                .OrderByDescending(p => p.TotalDamageWithCompanions)
                .ToList();

            // Track rank changes for animations
            var newRanks = new System.Collections.Generic.Dictionary<string, int>();
            int rank = 1;
            foreach (var player in sortedPlayers)
            {
                var playerName = player.Name ?? "Unknown";
                newRanks[playerName] = rank;
                rank++;
            }

            // Clear and rebuild player list
            PlayersItemsControl.Items.Clear();

            rank = 1;
            foreach (var player in sortedPlayers)
            {
                var playerName = player.Name ?? "Unknown";
                var playerRow = CreatePlayerRow(player, rank);
                PlayersItemsControl.Items.Add(playerRow);
                
                // Check for rank change and trigger animation
                if (_playerRowCache.ContainsKey(playerName))
                {
                    var cachedInfo = _playerRowCache[playerName];
                    if (cachedInfo.LastRank != rank && cachedInfo.LastRank > 0)
                    {
                        // Rank hat sich geändert - Animation starten
                        AnimateRankChange(playerRow);
                    }
                }
                
                // Update cache
                _playerRowCache[playerName] = new PlayerRowInfo
                {
                    PlayerName = playerName,
                    LastRank = rank,
                    RowBorder = playerRow
                };
                
                rank++;
            }
        }

        private Color GetPlayerColor(string playerName)
        {
            if (!_playerColors.ContainsKey(playerName))
            {
                _playerColors[playerName] = _availableColors[_nextColorIndex % _availableColors.Length];
                _nextColorIndex++;
            }
            return _playerColors[playerName];
        }

        private void AnimateRankChange(Border rowBorder)
        {
            // Scale-Animation für Rank-Wechsel (250ms, sehr schnell)
            var scaleTransform = new ScaleTransform(1.0, 1.0);
            rowBorder.RenderTransform = scaleTransform;
            rowBorder.RenderTransformOrigin = new Point(0.5, 0.5);

            var scaleAnimation = new System.Windows.Media.Animation.DoubleAnimation
            {
                From = 1.15,
                To = 1.0,
                Duration = TimeSpan.FromMilliseconds(250), // Schnell und flüssig
                EasingFunction = new System.Windows.Media.Animation.CubicEase { EasingMode = System.Windows.Media.Animation.EasingMode.EaseOut }
            };

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        }

        private Border CreatePlayerRow(Models.PlayerStatistics player, int rank)
        {
            var playerName = player.Name ?? "Unknown";
            
            // Spieler-spezifische Farbe holen (bleibt konstant für diesen Spieler)
            var playerColor = GetPlayerColor(playerName);
            
            // Kompakte Zeile mit spieler-spezifischer Hintergrundfarbe
            var rowBorder = new Border
            {
                Background = new SolidColorBrush(playerColor), // Spieler-Farbe (transparent)
                BorderBrush = new SolidColorBrush(Color.FromArgb(40, 255, 255, 255)),
                BorderThickness = new Thickness(0, 0, 0, 1),
                Padding = new Thickness(8, 6, 8, 6) // Kompakteres Padding
            };

            var grid = new Grid();
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(85, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(100, GridUnitType.Pixel) });

            // Rank + Player Name
            var namePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal
            };

            var rankText = new TextBlock
            {
                Text = $"{rank}.",
                FontSize = _playerFontSize,
                FontWeight = FontWeights.Bold,
                Foreground = rank == 1 ? new SolidColorBrush(Color.FromRgb(255, 215, 0)) : // Gold
                             rank == 2 ? new SolidColorBrush(Color.FromRgb(192, 192, 192)) : // Silver
                             rank == 3 ? new SolidColorBrush(Color.FromRgb(205, 127, 50)) : // Bronze
                             new SolidColorBrush(Color.FromRgb(176, 176, 176)),
                Margin = new Thickness(0, 0, 6, 0),
                VerticalAlignment = VerticalAlignment.Center
            };

            var playerNameText = new TextBlock
            {
                Text = playerName,
                FontSize = _playerFontSize,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                VerticalAlignment = VerticalAlignment.Center
            };

            namePanel.Children.Add(rankText);
            namePanel.Children.Add(playerNameText);
            Grid.SetColumn(namePanel, 0);
            grid.Children.Add(namePanel);

            // DPS
            var dpsText = new TextBlock
            {
                Text = $"{player.DpsWithCompanions:N0}",
                FontSize = _playerFontSize,
                Foreground = new SolidColorBrush(Color.FromRgb(91, 155, 213)),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                FontWeight = FontWeights.SemiBold
            };
            Grid.SetColumn(dpsText, 1);
            grid.Children.Add(dpsText);

            // Total Damage
            var damageText = new TextBlock
            {
                Text = $"{player.TotalDamageWithCompanions:N0}",
                FontSize = _playerFontSize,
                Foreground = new SolidColorBrush(Color.FromRgb(192, 192, 192)),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            Grid.SetColumn(damageText, 2);
            grid.Children.Add(damageText);

            rowBorder.Child = grid;
            return rowBorder;
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Nur verschiebbar wenn nicht gepinnt
            if (e.ClickCount == 1 && !_isPinned)
            {
                DragMove();
            }
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsPopup != null)
            {
                SettingsPopup.Visibility = Visibility.Visible;
            }
        }

        private void PinButton_Click(object sender, RoutedEventArgs e)
        {
            _isPinned = !_isPinned;
            
            if (PinButton != null)
            {
                PinButton.Content = _isPinned ? "📌" : "📍";
                PinButton.ToolTip = _isPinned ? "Entsperren" : "Verankern";
            }
            
            // Visuelles Feedback
            if (TitleBarBorder != null)
            {
                TitleBarBorder.Background = _isPinned 
                    ? new SolidColorBrush(Color.FromArgb(50, 91, 155, 213)) // Leichtes Blau wenn gepinnt
                    : new SolidColorBrush(Color.FromArgb(51, 0, 0, 0)); // Standard schwarz
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Hide();
        }

        private void FontSizeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _playerFontSize = e.NewValue;
            
            // Label aktualisieren
            if (FontSizeLabel != null)
            {
                FontSizeLabel.Text = ((int)_playerFontSize).ToString();
            }
            
            // Overlay sofort aktualisieren (Live-Vorschau)
            UpdateOverlay();
        }

        private void CloseSettingsPopup_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsPopup != null)
            {
                SettingsPopup.Visibility = Visibility.Collapsed;
            }
        }

        private void SettingsPopup_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Schließen wenn außerhalb des Settings-Fensters geklickt wird
            if (e.Source == SettingsPopup)
            {
                SettingsPopup.Visibility = Visibility.Collapsed;
            }
        }

        // ============================================
        // DEMO-MODUS (für Entwicklung)
        // ============================================

        private void DemoButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isDemoMode)
            {
                // Starte Demo-Modus
                StartDemoMode();
                if (DemoButton != null)
                {
                    DemoButton.Foreground = new SolidColorBrush(Color.FromRgb(76, 175, 80)); // Grün = aktiv
                }
            }
            else
            {
                // Stoppe Demo-Modus
                StopDemoMode();
                if (DemoButton != null)
                {
                    DemoButton.Foreground = new SolidColorBrush(Color.FromRgb(176, 176, 176)); // Grau = inaktiv
                }
            }
        }

        private void StartDemoMode()
        {
            _isDemoMode = true;
            _demoStartTime = DateTime.Now;
            
            // Generiere 5 Demo-Spieler
            _demoPlayers.Clear();
            string[] names = { "Picard", "Riker", "Data", "Worf", "Crusher" };
            
            foreach (var name in names)
            {
                _demoPlayers.Add(new Models.PlayerStatistics
                {
                    Name = name,
                    TotalDamageWithCompanions = _random.Next(100000, 500000),
                    DpsWithCompanions = _random.Next(5000, 25000)
                });
            }
            
            // Timer für Updates (alle 500ms = schnelle Demo)
            _demoUpdateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _demoUpdateTimer.Tick += DemoUpdateTimer_Tick;
            _demoUpdateTimer.Start();
            
            // Initial anzeigen
            UpdateOverlay();
        }

        private void StopDemoMode()
        {
            _isDemoMode = false;
            _demoUpdateTimer?.Stop();
            _demoUpdateTimer = null;
            _demoPlayers.Clear();
            
            // Overlay clearen (zurück zu Empty State)
            if (EmptyPanel != null)
            {
                EmptyPanel.Visibility = Visibility.Visible;
            }
            if (DataScrollViewer != null)
            {
                DataScrollViewer.Visibility = Visibility.Collapsed;
            }
            if (TitleText != null)
            {
                TitleText.Text = "🚀 Live Combat";
            }
            if (DurationText != null)
            {
                DurationText.Text = "";
            }
            if (PlayersItemsControl != null)
            {
                PlayersItemsControl.Items.Clear();
            }
        }

        private void DemoUpdateTimer_Tick(object? sender, EventArgs e)
        {
            // Simuliere Damage-Änderungen (zufällige Increments)
            foreach (var player in _demoPlayers)
            {
                var damageIncrease = _random.Next(5000, 50000);
                player.TotalDamageWithCompanions += damageIncrease;
                
                // DPS basierend auf Zeit berechnen
                var duration = (DateTime.Now - _demoStartTime).TotalSeconds;
                if (duration > 0)
                {
                    player.DpsWithCompanions = (int)(player.TotalDamageWithCompanions / duration);
                }
            }
            
            // Overlay aktualisieren (triggert Rank-Änderungen und Animationen)
            UpdateOverlay();
        }

        private void UpdateOverlayDemo()
        {
            // Show data
            if (EmptyPanel != null)
            {
                EmptyPanel.Visibility = Visibility.Collapsed;
            }
            if (DataScrollViewer != null)
            {
                DataScrollViewer.Visibility = Visibility.Visible;
            }

            // Update title
            if (TitleText != null)
            {
                TitleText.Text = "🚀 Demo Combat";
            }
            
            // Update duration
            if (DurationText != null)
            {
                var duration = DateTime.Now - _demoStartTime;
                DurationText.Text = $"⏱ {duration.Minutes:D2}:{duration.Seconds:D2}";
            }

            // Sort by Total Damage (descending)
            var sortedPlayers = _demoPlayers
                .OrderByDescending(p => p.TotalDamageWithCompanions)
                .ToList();

            // Track rank changes for animations
            var newRanks = new System.Collections.Generic.Dictionary<string, int>();
            int rank = 1;
            foreach (var player in sortedPlayers)
            {
                var playerName = player.Name ?? "Unknown";
                newRanks[playerName] = rank;
                rank++;
            }

            // Clear and rebuild player list
            PlayersItemsControl.Items.Clear();

            rank = 1;
            foreach (var player in sortedPlayers)
            {
                var playerName = player.Name ?? "Unknown";
                var playerRow = CreatePlayerRow(player, rank);
                PlayersItemsControl.Items.Add(playerRow);
                
                // Check for rank change and trigger animation
                if (_playerRowCache.ContainsKey(playerName))
                {
                    var cachedInfo = _playerRowCache[playerName];
                    if (cachedInfo.LastRank != rank && cachedInfo.LastRank > 0)
                    {
                        // Rank hat sich geändert - Animation starten
                        AnimateRankChange(playerRow);
                    }
                }
                
                // Update cache
                _playerRowCache[playerName] = new PlayerRowInfo
                {
                    PlayerName = playerName,
                    LastRank = rank,
                    RowBorder = playerRow
                };
                
                rank++;
            }
        }

        // ============================================
        // WINDOW LIFECYCLE
        // ============================================

        private bool _forceClose = false;

        /// <summary>
        /// Erzwingt das Schließen des Fensters (nicht nur Hide)
        /// </summary>
        public void ForceClose()
        {
            _forceClose = true;
            _durationUpdateTimer?.Stop();
            _demoUpdateTimer?.Stop(); // Demo-Timer auch stoppen
            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            // Nur verstecken, außer ForceClose wurde aufgerufen
            if (!_forceClose)
            {
                e.Cancel = true;
                Hide();
            }
            else
            {
                _durationUpdateTimer?.Stop();
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            // Cleanup
            _durationUpdateTimer?.Stop();
            _demoUpdateTimer?.Stop(); // Demo-Timer auch stoppen
            
            // Unsubscribe from events
            if (_viewModel != null)
            {
                _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            }
            base.OnClosed(e);
        }
    }
}

