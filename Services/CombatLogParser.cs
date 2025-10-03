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
    /// Combatlog-Parser Service
    /// </summary>
    public class CombatLogParser : ICombatLogParser
    {
        private readonly ILogger<CombatLogParser> _logger;

        // Regex-Pattern für verschiedene ID-Tags
        private static readonly Regex PlayerIdPattern = new(@"P\[(\d+)@(\d+)\s+([^@]+)@([^#]+)#(\d+)\]", RegexOptions.Compiled);
        private static readonly Regex EntityIdPattern = new(@"C\[(\d+)\s+([^\]]+)\]", RegexOptions.Compiled);
        private static readonly Regex CompanionIdPattern = new(@"S\[(\d+)\]", RegexOptions.Compiled);

        public CombatLogParser(ILogger<CombatLogParser> logger)
        {
            _logger = logger;
        }

        public async Task<List<CombatLogEntry>> ParseCombatLogFileAsync(string filePath)
        {
            var entries = new List<CombatLogEntry>();

            try
            {
                if (!File.Exists(filePath))
                {
                    return entries;
                }

                var lines = await File.ReadAllLinesAsync(filePath);

                foreach (var line in lines)
                {
                    var entry = ParseCombatLogLine(line);
                    if (entry != null)
                    {
                        entries.Add(entry);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Parsing der Combatlog-Datei");
            }

            return entries;
        }

        public CombatLogEntry? ParseCombatLogLine(string line)
        {
            try
            {
                // Teile zuerst nach :: (Timestamp + Separator)
                var timestampAndRest = line.Split(new[] { "::" }, 2, StringSplitOptions.None);
                if (timestampAndRest.Length != 2)
                {
                    return null;
                }

                var timestamp = timestampAndRest[0].Trim();
                var restOfLine = timestampAndRest[1];

                // Teile den Rest nach Komma
                var parts = restOfLine.Split(',');

                // Parse Schadenswerte
                var rawDamage = ParseDouble(parts[10]);

                // Ignoriere negative Schadenswerte (Heilung)
                if (rawDamage <= 0)
                {
                    return null;
                }

                // Erstelle CombatLogEntry
                var entry = new CombatLogEntry
                {
                    Timestamp = ParseTimestamp(timestamp),
                    PlayerInfo = ParsePlayerInfo(parts[0], parts[1]),
                    SourceEntity = ParseEntityInfo(parts[2], parts[3]),
                    TargetEntity = ParseEntityInfo(parts[4], parts[5]),
                    AttackName = parts[6].Trim(),
                    AbilityId = parts[7].Trim(),
                    DamageType = parts[8].Trim(),
                    EventType = ParseEventType(parts[9]),
                    RawDamage = rawDamage,
                    DamageWithResistance = ParseDouble(parts[11])
                };

                return entry;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Parsen der Zeile: {Line}", line);
                return null;
            }
        }

        public PlayerInfo ParsePlayerInfo(string playerName, string playerTag)
        {
            var playerInfo = new PlayerInfo
            {
                CharName = playerName.Trim(),
                PlayerTag = playerTag.Trim()
            };

            // Extrahiere Player-ID-Informationen wenn P-Tag vorhanden
            if (playerTag.StartsWith("P["))
            {
                var match = PlayerIdPattern.Match(playerTag);
                if (match.Success)
                {
                    playerInfo.CharId = match.Groups[1].Value;
                    playerInfo.AccountId = match.Groups[2].Value;
                    playerInfo.Handle = match.Groups[4].Value.Trim();
                    playerInfo.Discriminator = match.Groups[5].Value;
                }
            }

            return playerInfo;
        }

        public EntityInfo ParseEntityInfo(string entityName, string entityTag)
        {
            var entityInfo = new EntityInfo
            {
                Name = entityName.Trim(),
                EntityTag = entityTag.Trim()
            };

            // Bestimme Entity-Typ basierend auf Tag
            if (entityTag.StartsWith("P["))
            {
                entityInfo.Type = EntityType.Player;
            }
            else if (entityTag.StartsWith("S["))
            {
                entityInfo.Type = EntityType.Companion;
            }
            else if (entityTag.StartsWith("C["))
            {
                entityInfo.Type = EntityType.Enemy;

                // Extrahiere Entity-ID und Name
                var match = EntityIdPattern.Match(entityTag);
                if (match.Success)
                {
                    entityInfo.EntityId = match.Groups[1].Value;
                    entityInfo.EntityName = match.Groups[2].Value;

                    // Bestimme Kampfumgebung basierend auf Entity-Name
                    if (entityInfo.EntityName.Contains("Ground_"))
                    {
                        entityInfo.Environment = CombatEnvironment.Ground;
                    }
                    else if (entityInfo.EntityName.Contains("Space_"))
                    {
                        entityInfo.Environment = CombatEnvironment.Space;
                    }
                }
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
                // Format: DD:MM:YY:HH:MM:SS.mmm (3 Nachkommastellen)
                if (DateTime.TryParseExact(timestampData, "dd:MM:yy:HH:mm:ss.fff",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                {
                    return result;
                }

                // Format: DD:MM:YY:HH:MM:SS.m (1 Nachkommastelle)
                if (DateTime.TryParseExact(timestampData, "dd:MM:yy:HH:mm:ss.f",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                {
                    return result;
                }

                // Format: DD:MM:YY:HH:MM:SS.mm (2 Nachkommastellen)
                if (DateTime.TryParseExact(timestampData, "dd:MM:yy:HH:mm:ss.ff",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                {
                    return result;
                }

                // Fallback für andere Formate
                if (DateTime.TryParse(timestampData, out result))
                {
                    return result;
                }

                return DateTime.MinValue;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Fehler beim Parsen des Timestamps: {Timestamp}", timestampData);
                return DateTime.MinValue;
            }
        }

        private double ParseDouble(string value)
        {
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
            {
                return result;
            }
            return 0.0;
        }
    }
}
