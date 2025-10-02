using System;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace StoDamageMeter.Services
{
    /// <summary>
    /// Debug-Logging Service für Entwicklung und Debugging
    /// </summary>
    public class DebugLogger : IDebugLogger
    {
        private readonly ILogger<DebugLogger> _logger;
        private readonly string _logFilePath;
        private readonly object _lockObject = new();

        public DebugLogger(ILogger<DebugLogger> logger)
        {
            _logger = logger;
            _logFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "debug.log");

            // Log-Datei initialisieren
            InitializeLogFile();
        }

        private void InitializeLogFile()
        {
            try
            {
                var logDirectory = Path.GetDirectoryName(_logFilePath);
                if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
                {
                    Directory.CreateDirectory(logDirectory);
                }

                // Log-Datei mit Header initialisieren
                var header = $"\n{new string('=', 80)}\n" +
                           $"DEBUG LOG - {DateTime.Now:yyyy-MM-dd HH:mm:ss}\n" +
                           $"{new string('=', 80)}\n\n";

                File.WriteAllText(_logFilePath, header);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Initialisieren der Debug-Log-Datei");
            }
        }

        public void LogDebug(string message)
        {
            var logMessage = $"[DEBUG] {DateTime.Now:HH:mm:ss.fff} - {message}";
            WriteToFile(logMessage);
            _logger.LogDebug(message);
        }

        public void LogInfo(string message)
        {
            var logMessage = $"[INFO]  {DateTime.Now:HH:mm:ss.fff} - {message}";
            WriteToFile(logMessage);
            _logger.LogInformation(message);
        }

        public void LogWarning(string message)
        {
            var logMessage = $"[WARN]  {DateTime.Now:HH:mm:ss.fff} - {message}";
            WriteToFile(logMessage);
            _logger.LogWarning(message);
        }

        public void LogError(string message, Exception? exception = null)
        {
            var logMessage = $"[ERROR] {DateTime.Now:HH:mm:ss.fff} - {message}";
            if (exception != null)
            {
                logMessage += $"\nException: {exception.Message}\nStack: {exception.StackTrace}";
            }
            WriteToFile(logMessage);
            _logger.LogError(exception, message);
        }

        public void LogObject(string name, object obj)
        {
            try
            {
                var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions
                {
                    WriteIndented = true,
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });
                var logMessage = $"[OBJECT] {DateTime.Now:HH:mm:ss.fff} - {name}:\n{json}";
                WriteToFile(logMessage);
            }
            catch (Exception ex)
            {
                LogError($"Fehler beim Serialisieren von Objekt '{name}'", ex);
            }
        }

        public void LogCombatLine(int lineNumber, string line, string? parsedData = null)
        {
            var logMessage = $"[COMBAT] {DateTime.Now:HH:mm:ss.fff} - Zeile {lineNumber}:\n" +
                           $"Original: {line}\n";

            if (!string.IsNullOrEmpty(parsedData))
            {
                logMessage += $"Parsed: {parsedData}\n";
            }

            logMessage += $"{new string('-', 60)}\n";

            WriteToFile(logMessage);
        }

        private void WriteToFile(string message)
        {
            lock (_lockObject)
            {
                try
                {
                    File.AppendAllText(_logFilePath, message + "\n");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Fehler beim Schreiben in Debug-Log-Datei");
                }
            }
        }
    }
}
