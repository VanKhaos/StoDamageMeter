using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Microsoft.Win32;

namespace StoDamageMeter
{
    public partial class LandingWindow : Window
    {
        private bool _isDragging = false;
        private Point _lastMousePosition;
        private LiveCombatOverlay? _overlay = null;
        private bool _isPinned = false;
        private DispatcherTimer? _hideTimer = null;
        private double _zoomFactor = 1.2; // Default Zoom-Faktor

        public LandingWindow()
        {
            InitializeComponent();
            SetZoom(_zoomFactor);
            
            // Keyboard Shortcuts für Zoom
            KeyDown += LandingWindow_KeyDown;
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

                Left += deltaX;
                Top += deltaY;
            }
        }

        private void LogoImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isDragging = false;
            LogoImage.ReleaseMouseCapture();
        }

        #endregion

        #region Hover Animations

        private void ShowButtons(object sender, MouseEventArgs e)
        {
            // Timer stoppen und zurücksetzen falls er läuft
            if (_hideTimer != null)
            {
                _hideTimer.Stop();
                _hideTimer = null;
            }
            
            // Alle Icons und Buttons einblenden
            var fadeInAnimation = (Storyboard)FindResource("FadeInAnimation");
            fadeInAnimation?.Begin();
            
            // Buttons klickbar machen
            IconPanel.IsHitTestVisible = true;
            ClosePinPanel.IsHitTestVisible = true;
        }

        private void StartHideTimer(object sender, MouseEventArgs e)
        {
            // Timer stoppen falls er bereits läuft
            _hideTimer?.Stop();
            
            // Neuen Timer erstellen für 2,5 Sekunden Verzögerung
            _hideTimer = new DispatcherTimer();
            _hideTimer.Interval = TimeSpan.FromSeconds(2.5);
            _hideTimer.Tick += (s, args) =>
            {
                _hideTimer?.Stop();
                _hideTimer = null;
                
                // Alle Icons und Buttons ausblenden
                var fadeOutAnimation = (Storyboard)FindResource("FadeOutAnimation");
                fadeOutAnimation?.Begin();
                
                // Nach Animation Buttons nicht mehr klickbar machen
                if (fadeOutAnimation != null)
                {
                    fadeOutAnimation.Completed += (s2, e2) =>
                    {
                        IconPanel.IsHitTestVisible = false;
                        ClosePinPanel.IsHitTestVisible = false;
                    };
                }
            };
            _hideTimer.Start();
        }

        #endregion

        #region Button Click Handlers

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
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
                    PinIcon.Text = "📍"; // Leerer Pin (zeigt "Unpin" an)
                    PinButton.ToolTip = "Unpin Window";
                }
                else
                {
                    // Fenster entpinnen
                    Topmost = false;
                    PinIcon.Text = "📌"; // Gefüllter Pin (zeigt "Pin" an)
                    PinButton.ToolTip = "Pin Window";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Fehler beim Pin/Unpin: {ex.Message}", "Fehler", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DashboardButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Implement Dashboard functionality
            MessageBox.Show("Dashboard wird noch implementiert...", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
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

        private void LogButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // File Dialog für Combat Log auswählen
                var openFileDialog = new OpenFileDialog
                {
                    Title = "Combat Log auswählen",
                    Filter = "Log Files (*.log)|*.log|All Files (*.*)|*.*",
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
            // Zoom-Faktor begrenzen (1.0 - 3.0)
            _zoomFactor = Math.Max(1.0, Math.Min(3.0, factor));
            
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
        }

        #endregion
    }
}
