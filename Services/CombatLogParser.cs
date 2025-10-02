using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using StoDamageMeter.Models;

namespace StoDamageMeter.Services
{
    /// <summary>
    /// Combatlog-Parser Service mit modularen Funktionen
    /// </summary>
    public class CombatLogParser : ICombatLogParser
    {
        private readonly IDebugLogger _debugLogger;
        private readonly ILogger<CombatLogParser> _logger;

        // Regex-Pattern für verschiedene ID-Tags
        private static readonly Regex PlayerIdPattern = new(@"P\[(\d+)@(\d+)\s+([^@]+)@([^#]+)#(\d+)\]", RegexOptions.Compiled);
        private static readonly Regex EntityIdPattern = new(@"C\[(\d+)\s+([^\]]+)\]", RegexOptions.Compiled);
        private static readonly Regex CompanionIdPattern = new(@"S\[(\d+)\]", RegexOptions.Compiled);

        public CombatLogParser(IDebugLogger debugLogger, ILogger<CombatLogParser> logger)
        {
            _debugLogger = debugLogger;
            _logger = logger;
        }

        public async Task<List<CombatLogEntry>> ParseCombatLogFileAsync(string filePath)
        {
            _debugLogger.LogInfo($"Starte Parsing der Combatlog-Datei: {filePath}");

            var entries = new List<CombatLogEntry>();

            try
            {
                if (!File.Exists(filePath))
                {
                    _debugLogger.LogError($"Combatlog-Datei nicht gefunden: {filePath}");
                    return entries;
                }

                var lines = await File.ReadAllLinesAsync(filePath);
                _debugLogger.LogInfo($"Datei gelesen: {lines.Length} Zeilen gefunden");

                for (int i = 0; i < lines.Length; i++)
                {
                    var line = lines[i].Trim();
                    if (string.IsNullOrEmpty(line) || !IsValidCombatLogLine(line))
                    {
                        continue;
                    }

                    var entry = ParseCombatLogLine(line, i + 1);
                    if (entry != null && entry.IsRelevant)
                    {
                        entries.Add(entry);
                        _debugLogger.LogCombatLine(i + 1, line, $"Parsed: {entry.AttackName} -> {entry.RawDamage} {entry.DamageType}");
                    }
                }

                _debugLogger.LogInfo($"Parsing abgeschlossen: {entries.Count} relevante Einträge gefunden");
                _debugLogger.LogObject("Parsing-Statistiken", new
                {
                    TotalLines = lines.Length,
                    RelevantEntries = entries.Count,
                    CriticalHits = entries.Count(e => e.IsCritical),
                    DoTEvents = entries.Count(e => e.IsDoT),
                    TotalDamage = entries.Sum(e => e.RawDamage)
                });
            }
            catch (Exception ex)
            {
                _debugLogger.LogError($"Fehler beim Parsing der Combatlog-Datei: {filePath}", ex);
                _logger.LogError(ex, "Fehler beim Parsing der Combatlog-Datei");
            }

            return entries;
        }

        public CombatLogEntry? ParseCombatLogLine(string line, int lineNumber)
        {
            try
            {
                var parts = line.Split(',');
                if (parts.Length != 11)
                {
                    _debugLogger.LogWarning($"Zeile {lineNumber}: Ungültige Anzahl von Teilen ({parts.Length} statt 11)");
                    return null;
                }

                var entry = new CombatLogEntry
                {
                    Timestamp = ParseTimestamp(parts[0]),
                    PlayerInfo = ParsePlayerInfo(parts[2]),
                    SourceEntity = ParseEntityInfo(parts[3]),
                    TargetEntity = ParseEntityInfo(parts[4]),
                    AttackName = parts[5].Trim(),
                    AbilityId = parts[6].Trim(),
                    DamageType = parts[7].Trim(),
                    EventType = ParseEventType(parts[8]),
                    RawDamage = ParseDouble(parts[9]),
                    DamageWithResistance = ParseDouble(parts[10])
                };

                // Player-Name aus PlayerInfo extrahieren
                entry.PlayerName = entry.PlayerInfo.CharName;

                return entry;
            }
            catch (Exception ex)
            {
                _debugLogger.LogError($"Fehler beim Parsen der Zeile {lineNumber}: {line}", ex);
                return null;
            }
        }

        public bool IsValidCombatLogLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;

            // Mindestanforderungen: Doppelpunkt-Separator und Komma-Trennung
            return line.Contains("::") && line.Split(',').Length == 11;
        }

        public PlayerInfo ParsePlayerInfo(string playerData)
        {
            var playerInfo = new PlayerInfo();

            try
            {
                var match = PlayerIdPattern.Match(playerData);
                if (match.Success)
                {
                    playerInfo.CharId = match.Groups[1].Value;
                    playerInfo.AccountId = match.Groups[2].Value;
                    playerInfo.CharName = match.Groups[3].Value.Trim();
                    playerInfo.Handle = match.Groups[4].Value.Trim();
                    playerInfo.Discriminator = match.Groups[5].Value;
                }
                else
                {
                    // Fallback: Nur Spielername ohne ID-Tag
                    playerInfo.CharName = playerData.Split(',')[0].Trim();
                }
            }
            catch (Exception ex)
            {
                _debugLogger.LogError($"Fehler beim Parsen der Player-Info: {playerData}", ex);
            }

            return playerInfo;
        }

        public EntityInfo ParseEntityInfo(string entityData)
        {
            var entityInfo = new EntityInfo();

            try
            {
                if (string.IsNullOrWhiteSpace(entityData))
                {
                    return entityInfo;
                }

                // Companion (S[ID])
                var companionMatch = CompanionIdPattern.Match(entityData);
                if (companionMatch.Success)
                {
                    entityInfo.Type = EntityType.Companion;
                    entityInfo.EntityId = companionMatch.Groups[1].Value;
                    entityInfo.Name = entityData.Split(',')[0].Trim();
                    return entityInfo;
                }

                // Creature/Entity (C[ID])
                var entityMatch = EntityIdPattern.Match(entityData);
                if (entityMatch.Success)
                {
                    entityInfo.Type = EntityType.Creature;
                    entityInfo.EntityId = entityMatch.Groups[1].Value;
                    entityInfo.Name = entityMatch.Groups[2].Value.Trim();

                    // Environment bestimmen
                    if (entityInfo.Name.StartsWith("Ground_"))
                    {
                        entityInfo.Environment = CombatEnvironment.Ground;
                    }
                    else if (entityInfo.Name.StartsWith("Space_"))
                    {
                        entityInfo.Environment = CombatEnvironment.Space;
                    }
                }
                else
                {
                    // Fallback: Nur Name ohne ID-Tag
                    entityInfo.Name = entityData.Split(',')[0].Trim();
                }
            }
            catch (Exception ex)
            {
                _debugLogger.LogError($"Fehler beim Parsen der Entity-Info: {entityData}", ex);
            }

            return entityInfo;
        }

        public EventType ParseEventType(string eventTypeData)
        {
            if (string.IsNullOrWhiteSpace(eventTypeData))
                return EventType.Normal;

            return eventTypeData.Trim().ToLower() switch
            {
                "critical" => EventType.Critical,
                "dot" => EventType.DoT,
                "miss" => EventType.Miss,
                "immune" => EventType.Immune,
                _ => EventType.Normal
            };
        }

        public DateTime ParseTimestamp(string timestampData)
        {
            try
            {
                // Format: DD:MM:YY:HH:MM:SS.mmm
                if (DateTime.TryParseExact(timestampData, "dd:MM:yy:HH:mm:ss.fff",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                {
                    return result;
                }

                // Fallback für andere Formate
                if (DateTime.TryParse(timestampData, out result))
                {
                    return result;
                }

                _debugLogger.LogWarning($"Ungültiges Timestamp-Format: {timestampData}");
                return DateTime.MinValue;
            }
            catch (Exception ex)
            {
                _debugLogger.LogError($"Fehler beim Parsen des Timestamps: {timestampData}", ex);
                return DateTime.MinValue;
            }
        }

        private static double ParseDouble(string value)
        {
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }
            return 0.0;
        }
    }
}
