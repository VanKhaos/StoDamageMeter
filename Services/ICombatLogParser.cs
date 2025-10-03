using System.Collections.Generic;
using System.Threading.Tasks;
using StoDamageMeter.Models;

namespace StoDamageMeter.Services
{
    /// <summary>
    /// Interface für Combatlog-Parser
    /// </summary>
    public interface ICombatLogParser
    {
        /// <summary>
        /// Parst eine komplette Combatlog-Datei
        /// </summary>
        /// <param name="filePath">Pfad zur Combatlog-Datei</param>
        /// <returns>Liste aller geparsten Combatlog-Einträge</returns>
        Task<List<CombatLogEntry>> ParseCombatLogFileAsync(string filePath);

        /// <summary>
        /// Parst eine einzelne Combatlog-Zeile
        /// </summary>
        /// <param name="line">Die zu parsende Zeile</param>
        /// <returns>Geparster Combatlog-Eintrag oder null wenn ungültig</returns>
        CombatLogEntry? ParseCombatLogLine(string line);

        /// <summary>
        /// Parst Player-Informationen aus parts[0-1]
        /// </summary>
        /// <param name="playerName">parts[0] - Spielername</param>
        /// <param name="playerTag">parts[1] - Player-Tag</param>
        /// <returns>PlayerInfo-Objekt</returns>
        PlayerInfo ParsePlayerInfo(string playerName, string playerTag);

        /// <summary>
        /// Parst Entity-Informationen aus parts[2-3] oder parts[4-5]
        /// </summary>
        /// <param name="entityName">parts[2] oder parts[4] - Entity-Name</param>
        /// <param name="entityTag">parts[3] oder parts[5] - Entity-Tag</param>
        /// <returns>EntityInfo-Objekt</returns>
        EntityInfo ParseEntityInfo(string entityName, string entityTag);

        /// <summary>
        /// Parst Event-Typ aus parts[9]
        /// </summary>
        /// <param name="eventTypeData">parts[9] - Event-Typ-String</param>
        /// <returns>EventType-Enum</returns>
        EventType ParseEventType(string eventTypeData);

        /// <summary>
        /// Parst Timestamp aus parts[0] (vor dem ::)
        /// </summary>
        /// <param name="timestampData">Timestamp-String</param>
        /// <returns>DateTime-Objekt</returns>
        DateTime ParseTimestamp(string timestampData);
    }
}
