using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using StoDamageMeter.Services;
using StoDamageMeter.Models;
using StoDamageMeter.ViewModels;

namespace StoDamageMeter
{
    public partial class LandingWindow : Window
    {
        private bool _isDragging = false;
        private Point _lastMousePosition;
        private LiveCombatOverlay? _overlay = null;
        private bool _isPinned = false;
        private double _zoomFactor = 1.2; // Default Zoom-Faktor
        private readonly UpdateCheckService _updateCheckService;
        private LiveCombatViewModel? _liveCombatViewModel;
        
        // STO Window Binding
        private ApplicationWindowBindingService? _windowBindingService;
        
        // Global selected log file path
        public static string? SelectedLogFilePath { get; set; }
        
        // Global combat data cache
        public static List<CombatInfo>? CachedCombatData { get; set; }
        
        // Update menu items based on log file status
        private void UpdateLogFileMenuItemColor()
        {
            bool hasValidLogFile = !string.IsNullOrEmpty(SelectedLogFilePath) && File.Exists(SelectedLogFilePath);
            
            if (SelectLogFileMenuItem != null)
            {
                if (hasValidLogFile)
                {
                    // Log file selected and exists - Green
                    SelectLogFileMenuItem.Foreground = new SolidColorBrush(Color.FromRgb(0x4C, 0xAF, 0x50)); // Green
                }
                else
                {
                    // No log file selected or file doesn't exist - Red
                    SelectLogFileMenuItem.Foreground = new SolidColorBrush(Color.FromRgb(0xFF, 0x6B, 0x6B)); // Red
                }
            }
            
            // Enable/disable other menu items based on log file status
            if (StatisticsMenuItem != null)
                StatisticsMenuItem.IsEnabled = hasValidLogFile;
            
            if (GraphMenuItem != null)
                GraphMenuItem.IsEnabled = hasValidLogFile;
            
            if (OverlayMenuItem != null)
                OverlayMenuItem.IsEnabled = hasValidLogFile;
        }

        private async Task LoadStatisticsDataAsync()
        {
            if (string.IsNullOrEmpty(SelectedLogFilePath) || !File.Exists(SelectedLogFilePath))
                return;

            try
            {
                // Show loading text
                LoadingSpinnerGrid.Visibility = Visibility.Visible;
                
                // Get backend service
                var backendService = App.ServiceProvider.GetRequiredService<IOSCRBackendService>();
                
                // Load combat data
                var response = await backendService.GetAvailableCombatsWithProgressAsync(
                    SelectedLogFilePath, 
                    30 // maxCombats - neueste 30 Combats
                );
                
                // Small delay for visual effect
                await Task.Delay(500);
                
                if (response.Success && response.Combats != null)
                {
                    // Cache the combat data
                    CachedCombatData = response.Combats.OrderByDescending(c => c.Date).ThenByDescending(c => c.Time).ToList();
                    
                    // Start Live Combat Tracking
                    if (_liveCombatViewModel != null)
                    {
                        _ = _liveCombatViewModel.StartLiveParsing(SelectedLogFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error but don't show MessageBox to avoid interrupting user
                System.Diagnostics.Debug.WriteLine($"Error loading statistics data: {ex.Message}");
            }
            finally
            {
                // Hide loading text after a short delay
                await Task.Delay(500);
                LoadingSpinnerGrid.Visibility = Visibility.Collapsed;
            }
        }


        public LandingWindow()
        {
            InitializeComponent();
            _updateCheckService = App.ServiceProvider.GetRequiredService<UpdateCheckService>();
            
           // Initialize Live Combat ViewModel
           var backendService = App.ServiceProvider.GetRequiredService<IOSCRBackendService>();
           var fileWatcher = App.ServiceProvider.GetRequiredService<CombatLogWatcherService>();
           var logger = App.ServiceProvider.GetRequiredService<ILogger<LiveCombatViewModel>>();
           _liveCombatViewModel = new LiveCombatViewModel(backendService, fileWatcher, logger, Dispatcher);
           
           // Initialize Window Binding Service
           _windowBindingService = App.ServiceProvider.GetRequiredService<ApplicationWindowBindingService>();
            
            SetZoom(_zoomFactor);

            // Set default logo
            LogoImage.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/app_icon.png"));

            // Update log file menu item color on startup
            UpdateLogFileMenuItemColor();

            // Keyboard Shortcuts für Zoom
            KeyDown += LandingWindow_KeyDown;

            // Check for updates on startup (fire-and-forget, non-blocking)
            Loaded += OnLoaded;
            
           // Register this window for STO Window Binding
           _windowBindingService?.RegisterWindow(this);
           
           // ContextMenu Interaktions-Handler
        }

        #region Window Drag & Drop

        private void LogoImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Nur verschieben wenn nicht gepinnt
            if (!_isPinned)
            {
                _isDragging = true;
                _lastMousePosition = e.GetPosition(this);
                LogoImage.CaptureMouse();
            }
        }

        private void LogoImage_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && !_isPinned)
            {
                Point currentPosition = e.GetPosition(this);
                double deltaX = currentPosition.X - _lastMousePosition.X;
                double deltaY = currentPosition.Y - _lastMousePosition.Y;

                double newLeft = Left + deltaX;
                double newTop = Top + deltaY;

                // STO-Grenzen prüfen und Clipping anwenden
                var windowBinding = new WindowBindingService();
                if (windowBinding.TryGetStoBounds(out var stoBounds))
                {
                    // Begrenze auf STO-Fenster
                    newLeft = Math.Max(stoBounds.Left, Math.Min(newLeft, stoBounds.Right - Width));
                    newTop = Math.Max(stoBounds.Top, Math.Min(newTop, stoBounds.Bottom - Height));
                }

                Left = newLeft;
                Top = newTop;
            }
        }

        private void LogoImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            LogoImage.ReleaseMouseCapture();
        }

        private void LogoImage_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Update log file menu item color before showing context menu
            UpdateLogFileMenuItemColor();
            
            // ContextMenu wird automatisch angezeigt durch XAML-Definition
            // Hier könnten wir zusätzliche Logik hinzufügen falls nötig
        }

        #endregion

        protected override void OnContentRendered(EventArgs e)
        {
            base.OnContentRendered(e);
            
            // Markiere als manuell geöffnet
            _windowBindingService?.MarkWindowAsManuallyOpened(this);
        }

        private void PositionWindowInSto(Window window)
        {
            try
            {
                var windowBinding = new WindowBindingService();
                
                if (windowBinding.TryGetStoBounds(out var stoBounds))
                {
                    // Positioniere das Fenster innerhalb des STO-Fensters
                    window.Left = stoBounds.Left + 50; // 50px vom linken Rand
                    window.Top = stoBounds.Top + 50;   // 50px vom oberen Rand
                    
                    // Stelle sicher, dass das Fenster nicht außerhalb des STO-Fensters ist
                    if (window.Left + window.Width > stoBounds.Right)
                        window.Left = stoBounds.Right - window.Width - 10;
                    if (window.Top + window.Height > stoBounds.Bottom)
                        window.Top = stoBounds.Bottom - window.Height - 10;
                }
            }
            catch (Exception ex)
            {
                // Falls STO nicht läuft oder Fehler auftreten, verwende Standard-Position
                System.Diagnostics.Debug.WriteLine($"Could not position window in STO: {ex.Message}");
            }
        }

        #region Button Click Handlers

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            // Fenster nur ausblenden, nicht schließen
            this.Visibility = Visibility.Collapsed;
        }

        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            // Context Menu Close soll die Anwendung wirklich schließen
            // Stop live combat tracking before shutdown
            if (_liveCombatViewModel != null && _liveCombatViewModel.IsActive)
            {
                _ = _liveCombatViewModel.StopLiveParsing();
            }
            
            // Cleanup STO Window Binding
            // Window Binding Service wird automatisch von der App verwaltet
            
            Application.Current.Shutdown();
        }

        private void PinButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _isPinned = !_isPinned;
                
                if (_isPinned)
                {
                    // Fenster festpinnen
                    Topmost = true;
                    PinMenuItem.Header = "📍 Unpin";
                }
                else
                {
                    // Fenster entpinnen
                    Topmost = false;
                    PinMenuItem.Header = "📌 Pin";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Pin/Unpin: {ex.Message}", "Fehler", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void StatisticsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Öffne CombatStatistic Window
                var statisticsWindow = new CombatStatistic();
                
                // Positioniere das Fenster innerhalb des STO-Fensters
                PositionWindowInSto(statisticsWindow);
                
                statisticsWindow.Show();
                
                // Log-Datei wird automatisch geladen durch AutoLoadLogFileAsync im Loaded Event
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Öffnen der Statistics: {ex.Message}", "Fehler", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void GraphButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement Graph functionality
            MessageBox.Show("Graph wird noch implementiert...", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void OverlayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_overlay == null || !_overlay.IsVisible)
                {
                    // Overlay erstellen oder wieder anzeigen
                    if (_overlay == null)
                    {
                        _overlay = new LiveCombatOverlay();
                        _overlay.Closed += (s, args) => _overlay = null; // Reset bei Schließen
                        
                        // Connect ViewModel to Overlay
                        if (_liveCombatViewModel != null)
                        {
                            _overlay.SetViewModel(_liveCombatViewModel);
                        }
                    }
                    _overlay.Show();
                }
                else
                {
                    // Overlay verstecken (aber nicht schließen)
                    _overlay.Hide();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Toggle des Overlays: {ex.Message}", "Fehler", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SelectLogFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Title = "Select Combat Log File",
                    Filter = "Log files (*.log)|*.log",
                    DefaultExt = "log"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    // Stop current live combat tracking if running
                    if (_liveCombatViewModel != null && _liveCombatViewModel.IsActive)
                    {
                        _ = _liveCombatViewModel.StopLiveParsing();
                    }
                    
                    // Store selected log file path globally
                    SelectedLogFilePath = openFileDialog.FileName;
                    
                    // Update menu item color
                    UpdateLogFileMenuItemColor();
                    
                    // Start loading statistics data immediately
                    _ = LoadStatisticsDataAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Auswählen der Log-Datei: {ex.Message}", "Fehler", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LogButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // File Dialog für Combat Log auswählen
                var openFileDialog = new OpenFileDialog
                {
                    Title = "Combat Log auswählen",
                    Filter = "Log Files (*.log)|*.log",
                    InitialDirectory = @"C:\Program Files (x86)\Star Trek Online\Star Trek Online\Live\logs\GameClient",
                    FileName = "combatlog.log"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string selectedFile = openFileDialog.FileName;
                    MessageBox.Show($"Combat Log ausgewählt:\n{selectedFile}", "Info", 
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    
                    // TODO: Hier könnte die Log-Datei an den Backend-Service weitergegeben werden
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Auswählen der Log-Datei: {ex.Message}", "Fehler", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        #endregion

        #region Zoom Functionality

        private void SetZoom(double factor)
        {
            // Zoom-Faktor begrenzen (0.5 - 3.0) - jetzt auch kleiner als 1.0 möglich
            _zoomFactor = Math.Max(0.3, Math.Min(3.0, factor));
            
            // Window-Größe basierend auf Basis-Größe (100x100) und Zoom-Faktor
            const double baseSize = 100.0;
            double newSize = baseSize * _zoomFactor;
            
            Width = newSize;
            Height = newSize;
        }

        private void LandingWindow_KeyDown(object sender, KeyEventArgs e)
        {
            // Keyboard Shortcuts für Zoom
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                switch (e.Key)
                {
                    case Key.Add:
                    case Key.OemPlus:
                        SetZoom(_zoomFactor + 0.1);
                        e.Handled = true;
                        break;
                        
                    case Key.Subtract:
                    case Key.OemMinus:
                        SetZoom(_zoomFactor - 0.1);
                        e.Handled = true;
                        break;
                        
                    case Key.D0:
                        SetZoom(1.2); // Reset auf Standard
                        e.Handled = true;
                        break;
                }
            }
            
            // Verhindere dass die Anwendung sich schließt bei anderen Shortcuts
            if (e.Key == Key.Escape)
            {
                // ESC soll die Anwendung nur ausblenden, nicht schließen
                this.Hide();
                e.Handled = true;
            }
        }

        #endregion

        #region Update Check Methods

        private async void OnLoaded(object sender, RoutedEventArgs e)
        {
            // Check for updates (fire-and-forget, non-blocking)
            _ = CheckForUpdatesAsync();
        }

        private async Task CheckForUpdatesAsync()
        {
            try
            {
                var updateInfo = await _updateCheckService.CheckForUpdatesAsync();
                
                if (updateInfo?.UpdateAvailable == true)
                {
                    Dispatcher.Invoke(() =>
                    {
                        UpdateAvailableMenuItem.Header = $"🔔 Update Available - v{updateInfo.LatestVersion}";
                        UpdateAvailableMenuItem.Visibility = Visibility.Visible;
                        UpdateAvailableMenuItem.Tag = updateInfo.ReleaseUrl;
                        
                        // Wechsle zu grünem Logo für Update-Benachrichtigung
                        LogoImage.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/DPS_Meter_Logo_Green.png"));
                    });
                }
                else
                {
                    Dispatcher.Invoke(() =>
                    {
                        UpdateAvailableMenuItem.Visibility = Visibility.Collapsed;
                        
                        // Wechsle zurück zu normalem Logo
                        LogoImage.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/app_icon.png"));
                    });
                }
            }
            catch (Exception ex)
            {
                // Silent fail - Update check nicht kritisch
                System.Diagnostics.Debug.WriteLine($"Update check failed: {ex.Message}");
            }
        }


        private void UpdateAvailableMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (UpdateAvailableMenuItem.Tag is string url)
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
        }

        #endregion
    }
}
