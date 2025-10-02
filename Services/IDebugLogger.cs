using System;

namespace StoDamageMeter.Services
{
    /// <summary>
    /// Interface für Debug-Logging Service
    /// </summary>
    public interface IDebugLogger
    {
        /// <summary>
        /// Loggt eine Debug-Nachricht
        /// </summary>
        void LogDebug(string message);

        /// <summary>
        /// Loggt eine Info-Nachricht
        /// </summary>
        void LogInfo(string message);

        /// <summary>
        /// Loggt eine Warnung
        /// </summary>
        void LogWarning(string message);

        /// <summary>
        /// Loggt einen Fehler
        /// </summary>
        void LogError(string message, Exception? exception = null);

        /// <summary>
        /// Loggt ein Objekt für Debugging
        /// </summary>
        void LogObject(string name, object obj);

        /// <summary>
        /// Loggt eine Zeile aus dem Combatlog
        /// </summary>
        void LogCombatLine(int lineNumber, string line, string? parsedData = null);
    }
}
