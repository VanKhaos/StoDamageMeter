using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;

namespace StoDamageMeter.Services
{
    /// <summary>
    /// File-basierter Logging-Service für das Frontend
    /// Analog zum Backend-Logging mit RotatingFileHandler-Äquivalent
    /// </summary>
    public class LoggingService : ILogger
    {
        private readonly string _logFilePath;
        private readonly object _lockObject = new object();
        private const int MaxFileSize = 10 * 1024 * 1024; // 10 MB
        private const int MaxBackupFiles = 3;

        public LoggingService()
        {
            // Bestimme Log-Verzeichnis basierend auf Build-Konfiguration
            string logDirectory;
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Debug-Modus: Logs in Debug-Verzeichnis
                logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            }
            else
            {
                // Release-Modus: Logs in App-Verzeichnis
                logDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
            }

            // Erstelle Log-Verzeichnis falls nicht vorhanden
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            _logFilePath = Path.Combine(logDirectory, "frontend.log");
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel)
        {
            // In Debug-Modus: Alle Log-Level
            // In Release-Modus: Info und höher
            if (System.Diagnostics.Debugger.IsAttached)
                return true;
            
            return logLevel >= LogLevel.Information;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
                return;

            var message = formatter(state, exception);
            var logEntry = FormatLogEntry(logLevel, message, exception);

            WriteToFile(logEntry);
        }

        private string FormatLogEntry(LogLevel logLevel, string message, Exception? exception)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            var level = logLevel.ToString().ToUpper().PadRight(5);
            
            var logEntry = new StringBuilder();
            logEntry.AppendLine($"[{timestamp}] {level} - {message}");
            
            if (exception != null)
            {
                logEntry.AppendLine($"Exception: {exception.GetType().Name}");
                logEntry.AppendLine($"Message: {exception.Message}");
                if (!string.IsNullOrEmpty(exception.StackTrace))
                {
                    logEntry.AppendLine($"StackTrace: {exception.StackTrace}");
                }
            }
            
            return logEntry.ToString();
        }

        private void WriteToFile(string logEntry)
        {
            lock (_lockObject)
            {
                try
                {
                    // Prüfe Dateigröße und rotiere bei Bedarf
                    RotateLogFileIfNeeded();

                    // Schreibe Log-Eintrag
                    File.AppendAllText(_logFilePath, logEntry, Encoding.UTF8);
                }
                catch (Exception ex)
                {
                    // Fallback: Schreibe in Event Log oder ignoriere
                    System.Diagnostics.Debug.WriteLine($"Failed to write to log file: {ex.Message}");
                }
            }
        }

        private void RotateLogFileIfNeeded()
        {
            if (!File.Exists(_logFilePath))
                return;

            var fileInfo = new FileInfo(_logFilePath);
            if (fileInfo.Length < MaxFileSize)
                return;

            // Rotiere Log-Dateien
            var directory = Path.GetDirectoryName(_logFilePath);
            var fileName = Path.GetFileNameWithoutExtension(_logFilePath);
            var extension = Path.GetExtension(_logFilePath);

            // Lösche älteste Backup-Datei
            var oldestBackup = Path.Combine(directory, $"{fileName}.{MaxBackupFiles}{extension}");
            if (File.Exists(oldestBackup))
            {
                File.Delete(oldestBackup);
            }

            // Verschiebe bestehende Backup-Dateien
            for (int i = MaxBackupFiles - 1; i >= 1; i--)
            {
                var currentBackup = Path.Combine(directory, $"{fileName}.{i}{extension}");
                var nextBackup = Path.Combine(directory, $"{fileName}.{i + 1}{extension}");
                
                if (File.Exists(currentBackup))
                {
                    File.Move(currentBackup, nextBackup);
                }
            }

            // Verschiebe aktuelle Log-Datei zu .1
            var firstBackup = Path.Combine(directory, $"{fileName}.1{extension}");
            File.Move(_logFilePath, firstBackup);
        }

        /// <summary>
        /// Loggt eine Debug-Nachricht
        /// </summary>
        public void LogDebug(string message)
        {
            Log(LogLevel.Debug, 0, message, null, (msg, ex) => msg);
        }

        /// <summary>
        /// Loggt eine Info-Nachricht
        /// </summary>
        public void LogInfo(string message)
        {
            Log(LogLevel.Information, 0, message, null, (msg, ex) => msg);
        }

        /// <summary>
        /// Loggt eine Warning-Nachricht
        /// </summary>
        public void LogWarning(string message)
        {
            Log(LogLevel.Warning, 0, message, null, (msg, ex) => msg);
        }

        /// <summary>
        /// Loggt eine Error-Nachricht
        /// </summary>
        public void LogError(string message, Exception? exception = null)
        {
            Log(LogLevel.Error, 0, message, exception, (msg, ex) => msg);
        }

        /// <summary>
        /// Loggt Parsing-Statistiken (analog zum Backend)
        /// </summary>
        public void LogParsingStats(string operation, int totalLines, int processedLines, int skippedLines, int playersFound, double duration = 0)
        {
            var message = $"{operation} Stats: Total={totalLines}, Processed={processedLines}, Skipped={skippedLines}, Players={playersFound}";
            if (duration > 0)
            {
                message += $", Duration={duration:F1}s";
            }
            LogDebug(message);
        }

        /// <summary>
        /// Loggt Player-Statistiken (analog zum Backend)
        /// </summary>
        public void LogPlayerStats(string playerName, double totalDamage, double dps, int attacks)
        {
            LogDebug($"Player {playerName}: {totalDamage:F0} dmg, {dps:F0} DPS, {attacks} attacks");
        }
    }

    /// <summary>
    /// Logging-Service Factory für Dependency Injection
    /// </summary>
    public class LoggingServiceFactory : ILoggerProvider
    {
        public ILogger CreateLogger(string categoryName)
        {
            return new LoggingService();
        }

        public void Dispose()
        {
            // Cleanup falls nötig
        }
    }
}
