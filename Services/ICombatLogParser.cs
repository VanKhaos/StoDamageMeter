using System.Collections.Generic;
using System.Threading.Tasks;
using StoDamageMeter.Models;

namespace StoDamageMeter.Services
{
    /// <summary>
    /// Interface für Combatlog-Parser Service
    /// </summary>
    public interface ICombatLogParser
    {
        /// <summary>
        /// Parst eine Combatlog-Datei und gibt alle relevanten Einträge zurück
        /// </summary>
        Task<List<CombatLogEntry>> ParseCombatLogFileAsync(string filePath);

        /// <summary>
        /// Parst eine einzelne Zeile aus dem Combatlog
        /// </summary>
        CombatLogEntry? ParseCombatLogLine(string line, int lineNumber);

        /// <summary>
        /// Validiert ob eine Zeile ein gültiger Combatlog-Eintrag ist
        /// </summary>
        bool IsValidCombatLogLine(string line);

        /// <summary>
        /// Extrahiert Player-Informationen aus Position 3
        /// </summary>
        PlayerInfo ParsePlayerInfo(string playerData);

        /// <summary>
        /// Extrahiert Entity-Informationen aus Position 4 oder 5
        /// </summary>
        EntityInfo ParseEntityInfo(string entityData);

        /// <summary>
        /// Parst Event-Typ aus Position 9
        /// </summary>
        EventType ParseEventType(string eventTypeData);

        /// <summary>
        /// Parst Timestamp aus Position 1
        /// </summary>
        DateTime ParseTimestamp(string timestampData);
    }
}
