using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StoDamageMeter.Models;

namespace StoDamageMeter.Services
{
    /// <summary>
    /// Hauptservice für Combatlog-Verarbeitung und -Überwachung
    /// </summary>
    public class CombatLogService : ICombatLogService, IDisposable
    {
        private readonly ICombatLogParser _parser;
        private readonly IDebugLogger _debugLogger;
        private readonly ILogger<CombatLogService> _logger;
        private FileSystemWatcher? _fileWatcher;
        private string _currentFilePath = string.Empty;
        private long _lastFileSize = 0;

        public bool IsWatching => _fileWatcher?.EnableRaisingEvents ?? false;
        public event EventHandler<CombatLogDataUpdatedEventArgs>? DataUpdated;

        public CombatLogService(
            ICombatLogParser parser,
            IDebugLogger debugLogger,
            ILogger<CombatLogService> logger)
        {
            _parser = parser;
            _debugLogger = debugLogger;
            _logger = logger;
        }

        public async Task<CombatLogResult> ProcessCombatLogFileAsync(string filePath)
        {
            _debugLogger.LogInfo($"Verarbeite Combatlog-Datei: {filePath}");

            var result = new CombatLogResult
            {
                FilePath = filePath,
                ProcessedAt = DateTime.Now
            };

            try
            {
                if (!File.Exists(filePath))
                {
                    result.ErrorMessage = "Datei nicht gefunden";
                    _debugLogger.LogError($"Combatlog-Datei nicht gefunden: {filePath}");
                    return result;
                }

                // Parse Combatlog
                var entries = await _parser.ParseCombatLogFileAsync(filePath);
                result.Entries = entries;
                result.Success = true;

                // Berechne Statistiken
                result.Statistics = CalculateStatistics(entries);

                _debugLogger.LogInfo($"Combatlog erfolgreich verarbeitet: {entries.Count} Einträge");
                _debugLogger.LogObject("Combatlog-Statistiken", result.Statistics);
            }
            catch (Exception ex)
            {
                result.ErrorMessage = ex.Message;
                _debugLogger.LogError($"Fehler beim Verarbeiten der Combatlog-Datei: {filePath}", ex);
                _logger.LogError(ex, "Fehler beim Verarbeiten der Combatlog-Datei");
            }

            return result;
        }

        public async Task StartWatchingAsync(string filePath)
        {
            try
            {
                StopWatching(); // Stoppe vorherige Überwachung

                if (!File.Exists(filePath))
                {
                    _debugLogger.LogError($"Combatlog-Datei für Überwachung nicht gefunden: {filePath}");
                    return;
                }

                _currentFilePath = filePath;
                _lastFileSize = new FileInfo(filePath).Length;

                _fileWatcher = new FileSystemWatcher
                {
                    Path = Path.GetDirectoryName(filePath) ?? "",
                    Filter = Path.GetFileName(filePath),
                    NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite
                };

                _fileWatcher.Changed += OnFileChanged;
                _fileWatcher.EnableRaisingEvents = true;

                _debugLogger.LogInfo($"Überwachung gestartet für: {filePath}");

                // Initiale Verarbeitung
                await ProcessAndNotifyAsync();
            }
            catch (Exception ex)
            {
                _debugLogger.LogError($"Fehler beim Starten der Überwachung: {filePath}", ex);
                _logger.LogError(ex, "Fehler beim Starten der Überwachung");
            }
        }

        public void StopWatching()
        {
            if (_fileWatcher != null)
            {
                _fileWatcher.EnableRaisingEvents = false;
                _fileWatcher.Changed -= OnFileChanged;
                _fileWatcher.Dispose();
                _fileWatcher = null;

                _debugLogger.LogInfo("Überwachung gestoppt");
            }
        }

        private async void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            try
            {
                // Warte kurz um sicherzustellen, dass die Datei vollständig geschrieben wurde
                await Task.Delay(100);

                var currentSize = new FileInfo(_currentFilePath).Length;
                if (currentSize > _lastFileSize)
                {
                    _lastFileSize = currentSize;
                    await ProcessAndNotifyAsync();
                }
            }
            catch (Exception ex)
            {
                _debugLogger.LogError("Fehler beim Verarbeiten der Datei-Änderung", ex);
            }
        }

        private async Task ProcessAndNotifyAsync()
        {
            try
            {
                var result = await ProcessCombatLogFileAsync(_currentFilePath);
                if (result.Success)
                {
                    var eventArgs = new CombatLogDataUpdatedEventArgs
                    {
                        Result = result,
                        NewEntries = result.Entries // TODO: Nur neue Einträge ermitteln
                    };

                    DataUpdated?.Invoke(this, eventArgs);
                }
            }
            catch (Exception ex)
            {
                _debugLogger.LogError("Fehler beim Verarbeiten und Benachrichtigen", ex);
            }
        }

        private static CombatLogStatistics CalculateStatistics(List<CombatLogEntry> entries)
        {
            var stats = new CombatLogStatistics
            {
                TotalEntries = entries.Count,
                RelevantEntries = entries.Count(e => e.IsRelevant),
                CriticalHits = entries.Count(e => e.IsCritical),
                DoTEvents = entries.Count(e => e.IsDoT),
                TotalDamage = entries.Where(e => e.IsRelevant).Sum(e => e.RawDamage)
            };

            if (stats.RelevantEntries > 0)
            {
                stats.AverageDamage = stats.TotalDamage / stats.RelevantEntries;
                stats.CriticalRate = (double)stats.CriticalHits / stats.RelevantEntries * 100;
            }

            // Combat-Dauer berechnen
            if (entries.Count > 1)
            {
                var firstEntry = entries.MinBy(e => e.Timestamp);
                var lastEntry = entries.MaxBy(e => e.Timestamp);
                if (firstEntry != null && lastEntry != null)
                {
                    stats.CombatDuration = lastEntry.Timestamp - firstEntry.Timestamp;
                    if (stats.CombatDuration.TotalSeconds > 0)
                    {
                        stats.DPS = stats.TotalDamage / stats.CombatDuration.TotalSeconds;
                    }
                }
            }

            // Schadensarten gruppieren
            stats.DamageByType = entries
                .Where(e => e.IsRelevant)
                .GroupBy(e => e.DamageType)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.RawDamage));

            // Angriffe gruppieren
            stats.DamageByAttack = entries
                .Where(e => e.IsRelevant)
                .GroupBy(e => e.AttackName)
                .ToDictionary(g => g.Key, g => g.Sum(e => e.RawDamage));

            stats.AttackCounts = entries
                .Where(e => e.IsRelevant)
                .GroupBy(e => e.AttackName)
                .ToDictionary(g => g.Key, g => g.Count());

            return stats;
        }

        public void Dispose()
        {
            StopWatching();
        }
    }
}
