using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Extensions.Logging;

namespace StoDamageMeter.Services
{
    /// <summary>
    /// Zentraler Service für das Window Binding aller Anwendungsfenster
    /// </summary>
    public class ApplicationWindowBindingService : IDisposable
    {
        private readonly WindowBindingService _windowBinding;
        private readonly DispatcherTimer _focusCheckTimer;
        private readonly ILogger<ApplicationWindowBindingService> _logger;
        private readonly List<Window> _managedWindows;
        private readonly Dictionary<Window, WindowBindingState> _windowStates;
        
        private bool _isDisposed = false;
        private bool _bindToStoWindow = true;

        public ApplicationWindowBindingService(ILogger<ApplicationWindowBindingService> logger)
        {
            _logger = logger;
            _managedWindows = new List<Window>();
            _windowStates = new Dictionary<Window, WindowBindingState>();
            
            _windowBinding = new WindowBindingService();
            
            // Fokus-Check Timer (alle 500ms)
            _focusCheckTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(500)
            };
            _focusCheckTimer.Tick += FocusCheckTimer_Tick;
            _focusCheckTimer.Start();
            
            _logger.LogInformation("ApplicationWindowBindingService initialized");
        }

        /// <summary>
        /// Registriert ein Fenster für das Window Binding
        /// </summary>
        public void RegisterWindow(Window window)
        {
            if (_isDisposed) return;
            
            if (!_managedWindows.Contains(window))
            {
                _managedWindows.Add(window);
                _windowStates[window] = new WindowBindingState();
                
                // Event-Handler für User-Interaktionen
                // Event-Handler für Cleanup registrieren
                window.Closed += (s, e) => UnregisterWindow(window);
                
                _logger.LogInformation($"Window registered: {window.GetType().Name}");
            }
        }

        /// <summary>
        /// Entfernt ein Fenster aus dem Window Binding
        /// </summary>
        public void UnregisterWindow(Window window)
        {
            if (_managedWindows.Contains(window))
            {
                _managedWindows.Remove(window);
                _windowStates.Remove(window);
                _logger.LogInformation($"Window unregistered: {window.GetType().Name}");
            }
        }

        /// <summary>
        /// Markiert ein Fenster als manuell geöffnet
        /// </summary>
        public void MarkWindowAsManuallyOpened(Window window)
        {
            if (_windowStates.ContainsKey(window))
            {
                _windowStates[window].WasManuallyOpened = true;
                _logger.LogInformation($"Window marked as manually opened: {window.GetType().Name}");
            }
        }


        private void FocusCheckTimer_Tick(object? sender, EventArgs e)
        {
            if (_isDisposed || !_bindToStoWindow) return;

            // Prüfe ob STO noch läuft - wenn nicht, beende die Anwendung
            if (!_windowBinding.IsStoProcessRunning())
            {
                _logger.LogInformation("STO process not running - shutting down application");
                Application.Current.Dispatcher.Invoke(() => {
                    Application.Current.Shutdown();
                });
                return;
            }

            bool isStoActive = _windowBinding.IsStoWindowActive();


            foreach (var window in _managedWindows.ToArray())
            {
                if (!_windowStates.ContainsKey(window)) continue;
                
                var state = _windowStates[window];
                
                // Nur Window-Binding aktivieren wenn Fenster bereits manuell geöffnet wurde
                if (!state.WasManuallyOpened) continue;

                // WICHTIG: Whitelist-Ansatz - prüfe ob STO oder unsere Anwendung im Fokus ist
                uint foregroundProcessId = _windowBinding.GetForegroundProcessId();
                uint ourProcessId = (uint)System.Diagnostics.Process.GetCurrentProcess().Id;
                uint stoProcessId = _windowBinding.GetStoProcessId();
                
                // Fenster ist "fokussiert" wenn STO oder unsere Anwendung im Vordergrund ist
                bool isWindowFocused = foregroundProcessId == ourProcessId || foregroundProcessId == stoProcessId;


                // Einfache Logik: Fenster immer anzeigen wenn STO läuft
                if (isStoActive || isWindowFocused)
                {
                    // STO aktiv oder unsere Anwendung im Fokus → Fenster anzeigen
                    if (window.WindowState == WindowState.Minimized)
                    {
                        state.IsChangingVisibility = true;
                        window.WindowState = WindowState.Normal;
                        window.Visibility = Visibility.Visible;
                        window.Topmost = true; // Immer im Vordergrund wenn sichtbar
                        
                        // Flag nach kurzer Verzögerung zurücksetzen
                        _ = Task.Delay(200).ContinueWith(_ => {
                            state.IsChangingVisibility = false;
                        }, TaskScheduler.FromCurrentSynchronizationContext());
                    }
                    else if (window.Visibility == Visibility.Visible && !window.Topmost)
                    {
                        // Fenster ist bereits sichtbar, aber nicht topmost → topmost setzen
                        window.Topmost = true;
                    }
                }
                else
                {
                    // STO nicht aktiv und unsere Anwendung nicht im Fokus → Fenster minimieren (bleibt in Taskbar)
                    if (window.Visibility == Visibility.Visible)
                    {
                        state.IsChangingVisibility = true;
                        window.WindowState = WindowState.Minimized;
                        
                        // Flag nach kurzer Verzögerung zurücksetzen
                        _ = Task.Delay(200).ContinueWith(_ => {
                            state.IsChangingVisibility = false;
                        }, TaskScheduler.FromCurrentSynchronizationContext());
                    }
                }
            }
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                _focusCheckTimer?.Stop();
                _windowBinding?.Dispose();
                _logger.LogInformation("ApplicationWindowBindingService disposed");
            }
        }
    }

    /// <summary>
    /// Zustand eines verwalteten Fensters
    /// </summary>
    public class WindowBindingState
    {
        public bool WasManuallyOpened { get; set; } = false;
        public bool IsChangingVisibility { get; set; } = false;
    }
}
