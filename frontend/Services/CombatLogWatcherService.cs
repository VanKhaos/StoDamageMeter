using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace StoDamageMeter.Services
{
    /// <summary>
    /// Event-Args für neue Log-Zeilen
    /// </summary>
    public class NewLogLinesEventArgs : EventArgs
    {
        public List<string> Lines { get; set; } = new List<string>();
        public long CurrentByteOffset { get; set; }
        public int TotalLinesProcessed { get; set; }
    }

    /// <summary>
    /// Event-Args für FileWatcher-Fehler
    /// </summary>
    public class FileWatcherErrorEventArgs : EventArgs
    {
        public string ErrorMessage { get; set; } = string.Empty;
        public Exception? Exception { get; set; }
    }

    /// <summary>
    /// Service für Live-Überwachung der Combat-Log-Datei
    /// </summary>
    public class CombatLogWatcherService : IDisposable
    {
        private readonly ILogger<CombatLogWatcherService> _logger;
        private FileSystemWatcher? _watcher;
        private Timer? _debounceTimer;
        private readonly Queue<string> _pendingLines = new();
        private long _currentByteOffset;
        private string? _currentLogPath;
        private bool _isWatching;
        private readonly object _lock = new();
        private int _totalLinesProcessed;
        private string? _incompleteLineBuffer;

        public event EventHandler<NewLogLinesEventArgs>? NewLinesDetected;
        public event EventHandler<FileWatcherErrorEventArgs>? WatcherError;

        public long CurrentByteOffset => _currentByteOffset;
        public bool IsWatching => _isWatching;
        public int TotalLinesProcessed => _totalLinesProcessed;

        public CombatLogWatcherService(ILogger<CombatLogWatcherService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Startet die Überwachung einer Combat-Log-Datei
        /// </summary>
        /// <param name="logPath">Pfad zur Combat-Log-Datei</param>
        /// <param name="startOffset">Byte-Offset ab dem gelesen werden soll (0 = Anfang, -1 = Ende)</param>
        public void StartWatching(string logPath, long startOffset = -1)
        {
            if (_isWatching)
            {
                _logger.LogWarning("FileWatcher is already watching. Stop first before starting again.");
                return;
            }

            if (!File.Exists(logPath))
            {
                var error = $"Log file not found: {logPath}";
                _logger.LogError(error);
                OnWatcherError(error, null);
                return;
            }

            try
            {
                _currentLogPath = logPath;
                
                // Setze Start-Offset: -1 = Ende der Datei (nur neue Zeilen), 0 = Anfang
                if (startOffset < 0)
                {
                    var fileInfo = new FileInfo(logPath);
                    _currentByteOffset = fileInfo.Length;
                }
                else
                {
                    _currentByteOffset = startOffset;
                }

                _logger.LogInformation($"Starting FileWatcher for {logPath} at offset {_currentByteOffset}");

                // FileSystemWatcher erstellen
                var directory = Path.GetDirectoryName(logPath);
                var fileName = Path.GetFileName(logPath);

                _watcher = new FileSystemWatcher(directory ?? ".", fileName)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName | NotifyFilters.CreationTime,
                    EnableRaisingEvents = true
                };

                _watcher.Changed += OnFileChanged;
                _watcher.Created += OnFileCreated;
                _watcher.Deleted += OnFileDeleted;
                _watcher.Error += OnWatcherErrorEvent;

                // Debounce-Timer (100ms für schnellere Reaktion)
                _debounceTimer = new Timer(ProcessPendingLines, null, Timeout.Infinite, Timeout.Infinite);

                _isWatching = true;

                _logger.LogInformation("FileWatcher started successfully");
                
                // WICHTIG: Initiale Zeilen sofort verarbeiten (nicht auf FileChanged warten!)
                ProcessPendingLines(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error starting FileWatcher");
                OnWatcherError("Failed to start FileWatcher", ex);
            }
        }

        /// <summary>
        /// Stoppt die Überwachung
        /// </summary>
        public void StopWatching()
        {
            if (!_isWatching)
            {
                return;
            }

            _logger.LogInformation("Stopping FileWatcher");

            _isWatching = false;

            if (_watcher != null)
            {
                _watcher.EnableRaisingEvents = false;
                _watcher.Changed -= OnFileChanged;
                _watcher.Created -= OnFileCreated;
                _watcher.Deleted -= OnFileDeleted;
                _watcher.Error -= OnWatcherErrorEvent;
                _watcher.Dispose();
                _watcher = null;
            }

            if (_debounceTimer != null)
            {
                _debounceTimer.Dispose();
                _debounceTimer = null;
            }

            lock (_lock)
            {
                _pendingLines.Clear();
            }

            _logger.LogInformation("FileWatcher stopped");
        }

        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            if (!_isWatching || string.IsNullOrEmpty(_currentLogPath))
            {
                return;
            }

            // Debouncing: Timer neu starten bei jeder Änderung (100ms für schnellere Updates)
            _debounceTimer?.Change(100, Timeout.Infinite);
        }

        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            _logger.LogInformation($"🆕 File created event detected: {e.Name}");
            
            // Reset Offset auf 0 (Datei ist neu, von Anfang lesen)
            _currentByteOffset = 0;
            _incompleteLineBuffer = null;
            
            // Sofort neue Zeilen verarbeiten
            _debounceTimer?.Change(100, Timeout.Infinite);
        }

        private void OnFileDeleted(object sender, FileSystemEventArgs e)
        {
            _logger.LogWarning($"🗑️ File deleted event detected: {e.Name}");
            
            // Reset Offset (falls Datei neu erstellt wird)
            _currentByteOffset = 0;
            _incompleteLineBuffer = null;
            
            lock (_lock)
            {
                _pendingLines.Clear();
            }
        }

        private void ProcessPendingLines(object? state)
        {
            if (!_isWatching || string.IsNullOrEmpty(_currentLogPath))
            {
                return;
            }

            try
            {
                // Lese neue Zeilen ab aktuellem Offset
                var newLines = ReadNewLines(_currentLogPath, ref _currentByteOffset);

                if (newLines.Count > 0)
                {
                    lock (_lock)
                    {
                        foreach (var line in newLines)
                        {
                            _pendingLines.Enqueue(line);
                        }
                        _totalLinesProcessed += newLines.Count;
                    }

                    // Trigger Event
                    var eventArgs = new NewLogLinesEventArgs
                    {
                        Lines = newLines,
                        CurrentByteOffset = _currentByteOffset,
                        TotalLinesProcessed = _totalLinesProcessed
                    };

                    _logger.LogInformation($"NewLinesDetected: {newLines.Count} lines, Total: {_totalLinesProcessed}");
                    
                    NewLinesDetected?.Invoke(this, eventArgs);

                    _logger.LogInformation($"Event invoked for {newLines.Count} new lines");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing new log lines");
                OnWatcherError("Error reading new lines", ex);
            }
        }

        private List<string> ReadNewLines(string filePath, ref long currentOffset)
        {
            var lines = new List<string>();

            try
            {
                // Öffne Datei mit FileShare.ReadWrite (STO schreibt noch)
                using var fileStream = new FileStream(
                    filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite);

                var fileSize = fileStream.Length;

                if (currentOffset >= fileSize)
                {
                    // Keine neuen Daten
                    return lines;
                }

                // Seek zu aktuellem Offset
                fileStream.Seek(currentOffset, SeekOrigin.Begin);

                using var reader = new StreamReader(fileStream, Encoding.UTF8);

                // Handle unvollständige Zeile vom letzten Read
                string? currentLine = _incompleteLineBuffer;

                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }

                    // Check ob Zeile vollständig ist (reader.EndOfStream bei letzter Zeile)
                    if (reader.EndOfStream && !line.EndsWith("\n") && fileStream.Position < fileSize)
                    {
                        // Unvollständige Zeile - für nächsten Read speichern
                        _incompleteLineBuffer = line;
                        break;
                    }

                    // Füge vorherige unvollständige Zeile hinzu
                    if (!string.IsNullOrEmpty(currentLine))
                    {
                        line = currentLine + line;
                        currentLine = null;
                        _incompleteLineBuffer = null;
                    }

                    if (!string.IsNullOrWhiteSpace(line))
                    {
                        lines.Add(line);
                    }
                }

                // Update Offset
                currentOffset = fileStream.Position;
            }
            catch (IOException ex)
            {
                // Datei könnte gesperrt sein - retry beim nächsten Mal
                _logger.LogWarning(ex, "IOException while reading log file (file might be locked)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error reading log file");
                throw;
            }

            return lines;
        }

        private void OnWatcherErrorEvent(object sender, ErrorEventArgs e)
        {
            var ex = e.GetException();
            _logger.LogError(ex, "FileSystemWatcher error");
            OnWatcherError("FileSystemWatcher encountered an error", ex);
        }

        private void OnWatcherError(string message, Exception? exception)
        {
            var eventArgs = new FileWatcherErrorEventArgs
            {
                ErrorMessage = message,
                Exception = exception
            };

            WatcherError?.Invoke(this, eventArgs);
        }

        public void Dispose()
        {
            StopWatching();
        }
    }
}

