using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace StoDamageMeter.Services
{
    /// <summary>
    /// Service für Window-Binding an Star Trek Online.
    /// Nutzt ausschließlich Standard Windows-APIs (EULA-konform).
    /// </summary>
    public class WindowBindingService
    {
        // Windows API P/Invoke Deklarationen
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsIconic(IntPtr hWnd);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public struct WindowBounds
        {
            public double Left { get; set; }
            public double Top { get; set; }
            public double Right { get; set; }
            public double Bottom { get; set; }
            public double Width => Right - Left;
            public double Height => Bottom - Top;
        }

        private readonly string[] _stoProcessNames = { "GameClient", "StarTrekOnline" };
        private Process? _cachedStoProcess;
        private DateTime _lastProcessCheck = DateTime.MinValue;
        private readonly TimeSpan _processCheckInterval = TimeSpan.FromSeconds(5);

        /// <summary>
        /// Prüft ob Star Trek Online aktuell das aktive/fokussierte Fenster ist.
        /// </summary>
        public bool IsStoWindowActive()
        {
            var stoProcess = GetStoProcess();
            if (stoProcess == null)
                return false;

            IntPtr foregroundWindow = GetForegroundWindow();
            if (foregroundWindow == IntPtr.Zero)
                return false;

            GetWindowThreadProcessId(foregroundWindow, out uint foregroundProcessId);
            
            return foregroundProcessId == stoProcess.Id && !IsIconic(foregroundWindow);
        }

        /// <summary>
        /// Versucht die Fenster-Grenzen von STO zu ermitteln.
        /// </summary>
        public bool TryGetStoBounds(out WindowBounds bounds)
        {
            bounds = new WindowBounds();

            var stoProcess = GetStoProcess();
            if (stoProcess == null)
                return false;

            IntPtr mainWindowHandle = stoProcess.MainWindowHandle;
            if (mainWindowHandle == IntPtr.Zero)
                return false;

            if (!GetWindowRect(mainWindowHandle, out RECT rect))
                return false;

            bounds = new WindowBounds
            {
                Left = rect.Left,
                Top = rect.Top,
                Right = rect.Right,
                Bottom = rect.Bottom
            };

            return true;
        }

        /// <summary>
        /// Findet den STO-Prozess. Nutzt Caching für Performance.
        /// </summary>
        private Process? GetStoProcess()
        {
            // Cache-Check: Prüfe nur alle 5 Sekunden neu
            if (_cachedStoProcess != null && !_cachedStoProcess.HasExited)
            {
                if (DateTime.Now - _lastProcessCheck < _processCheckInterval)
                    return _cachedStoProcess;
            }

            // Prozess neu suchen
            _lastProcessCheck = DateTime.Now;
            _cachedStoProcess = null;

            foreach (var processName in _stoProcessNames)
            {
                var processes = Process.GetProcessesByName(processName);
                if (processes.Length > 0)
                {
                    _cachedStoProcess = processes[0];
                    
                    // Cleanup: Schließe andere Prozess-Handles
                    for (int i = 1; i < processes.Length; i++)
                    {
                        processes[i].Dispose();
                    }
                    
                    return _cachedStoProcess;
                }
            }

            return null;
        }

        /// <summary>
        /// Prüft ob STO überhaupt läuft.
        /// </summary>
        public bool IsStoRunning()
        {
            return GetStoProcess() != null;
        }

        /// <summary>
        /// Cleanup: Disposed gecachte Ressourcen.
        /// </summary>
        public void Dispose()
        {
            _cachedStoProcess?.Dispose();
            _cachedStoProcess = null;
        }
    }
}

