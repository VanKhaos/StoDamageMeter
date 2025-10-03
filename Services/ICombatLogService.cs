using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using StoDamageMeter.Models;

namespace StoDamageMeter.Services
{
    /// <summary>
    /// Interface für Combatlog-Service (Hauptservice für Combatlog-Verarbeitung)
    /// </summary>
    public interface ICombatLogService
    {
        /// <summary>
        /// Lädt und verarbeitet eine Combatlog-Datei
        /// </summary>
        Task<CombatLogResult> ProcessCombatLogFileAsync(string filePath);

        /// <summary>
        /// Startet die Überwachung einer Combatlog-Datei für Live-Updates
        /// </summary>
        Task StartWatchingAsync(string filePath);

        /// <summary>
        /// Stoppt die Überwachung
        /// </summary>
        void StopWatching();

        /// <summary>
        /// Ist der Service aktuell aktiv?
        /// </summary>
        bool IsWatching { get; }

        /// <summary>
        /// Event das ausgelöst wird wenn neue Daten verfügbar sind
        /// </summary>
        event EventHandler<CombatLogDataUpdatedEventArgs>? DataUpdated;
    }

    /// <summary>
    /// Ergebnis der Combatlog-Verarbeitung
    /// </summary>
    public class CombatLogResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public List<CombatLogEntry> Entries { get; set; } = new();
        public CombatLogStatistics Statistics { get; set; } = new();
        public string FilePath { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; } = DateTime.Now;
    }

    /// <summary>
    /// Statistiken aus dem Combatlog
    /// </summary>
    public class CombatLogStatistics
    {
        public int TotalEntries { get; set; }
        public int RelevantEntries { get; set; }
        public int CriticalHits { get; set; }
        public int DoTEvents { get; set; }
        public double TotalDamage { get; set; }
        public double AverageDamage { get; set; }
        public double DPS { get; set; }
        public TimeSpan CombatDuration { get; set; }
        public Dictionary<string, double> DamageByType { get; set; } = new();
        public Dictionary<string, double> DamageByAttack { get; set; } = new();
        public Dictionary<string, int> AttackCounts { get; set; } = new();
        public double CriticalRate { get; set; }
        public int PlayerCount { get; set; }
    }

    /// <summary>
    /// Event-Args für DataUpdated Event
    /// </summary>
    public class CombatLogDataUpdatedEventArgs : EventArgs
    {
        public CombatLogResult Result { get; set; } = new();
        public List<CombatLogEntry> NewEntries { get; set; } = new();
    }
}
