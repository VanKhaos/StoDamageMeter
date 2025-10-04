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
        private int _lineCounter = 0;
        private readonly Random _random = new Random();
        private readonly HashSet<int> _selectedLines = new HashSet<int>();

        // Regex-Pattern für verschiedene ID-Tags
        private static readonly Regex PlayerIdPattern = new(@"P\[(\d+)@(\d+)\s+([^@]+)@([^#]+)#(\d+)\]", RegexOptions.Compiled);
        private static readonly Regex EntityIdPattern = new(@"C\[(\d+)\s+([^\]]+)\]", RegexOptions.Compiled);
        private static readonly Regex CompanionIdPattern = new(@"S\[(\d+)\]", RegexOptions.Compiled);

        public CombatLogParser(ILogger<CombatLogParser> logger)
        {
            _logger = logger;
        }


        /// <summary>
        /// Bestimmt ob ein Entity ein Companion oder Hangar-Pet ist
        /// </summary>
        private bool IsCompanionOrHangarPet(EntityInfo sourceEntity, PlayerInfo playerInfo, string originalLine)
        {
            // Wenn Source leer ist, dann ist es direkter Spieler-Schaden (kein Companion)
            if (string.IsNullOrEmpty(sourceEntity.Name))
            {
                return false; // Direkter Spieler-Schaden
            }

            // Wenn Source der Spieler selbst ist, dann ist es kein Companion
            if (sourceEntity.Name == playerInfo.CharName)
            {
                return false; // Spieler selbst
            }

            // Prüfe ob ein P[ Tag in der ursprünglichen Zeile vorhanden ist
            // Das würde bedeuten, dass es ein Companion ist
            bool hasPlayerTag = originalLine.Contains("P[");

            // Wenn ein P[ Tag vorhanden ist, dann ist es ein Companion
            // Wenn kein P[ Tag vorhanden ist, dann ist es ein Gegner
            return hasPlayerTag;
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
                _logger.LogInformation("=== COMBAT LOG PARSING GESTARTET ===");
                _logger.LogInformation("Datei: {FilePath}", filePath);
                _logger.LogInformation("Anzahl Zeilen: {LineCount}", lines.Length);

                // Reset line counter für neue Datei
                _lineCounter = 0;
                _selectedLines.Clear();

                // Wähle 10 zufällige Zeilen für detailliertes Logging aus
                if (lines.Length > 0)
                {
                    int linesToSelect = Math.Min(10, lines.Length);
                    for (int i = 0; i < linesToSelect; i++)
                    {
                        int randomLine = _random.Next(0, lines.Length);
                        _selectedLines.Add(randomLine);
                    }
                    _logger.LogInformation("Ausgewählte {Count} zufällige Zeilen für detailliertes Logging", linesToSelect);
                }

                int parsedCount = 0;
                int skippedCount = 0;

                foreach (var line in lines)
                {
                    var entry = ParseCombatLogLine(line);
                    if (entry != null)
                    {
                        entries.Add(entry);
                        parsedCount++;
                    }
                    else
                    {
                        skippedCount++;
                    }
                }

                _logger.LogInformation("Parsing abgeschlossen: {ParsedCount} Einträge geparst, {SkippedCount} übersprungen", parsedCount, skippedCount);
                _logger.LogInformation("=== COMBAT LOG PARSING BEENDET ===");
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
                // Logge zufällig ausgewählte Zeilen mit Rohdaten-Parsing-Details
                _lineCounter++;
                bool shouldLogDetails = _selectedLines.Contains(_lineCounter - 1);

                // Parsing-Debug-Logging entfernt für saubere Console

                // Teile zuerst nach :: (Timestamp + Separator)
                var timestampAndRest = line.Split(new[] { "::" }, 2, StringSplitOptions.None);
                if (timestampAndRest.Length != 2)
                {
                    // Fehler-Logging entfernt für saubere Console
                    return null;
                }

                var timestamp = timestampAndRest[0].Trim();
                var restOfLine = timestampAndRest[1];

                // Timestamp-Debug-Logging entfernt

                // Teile den Rest nach Komma
                var parts = restOfLine.Split(',');

                // Parts-Debug-Logging für ausgewählte Zeilen
                if (shouldLogDetails)
                {
                    _logger.LogInformation("=== PARTS DEBUG ===");
                    _logger.LogInformation("Anzahl Parts: {PartsCount}", parts.Length);
                    for (int i = 0; i < parts.Length; i++)
                    {
                        _logger.LogInformation("  Part[{Index}]: '{Value}'", i, parts[i]);
                    }
                    _logger.LogInformation("=== ENDE PARTS DEBUG ===");
                }

                // Parse Schadenswerte
                var rawDamage = ParseDouble(parts[10]);

                // Raw Damage-Debug-Logging entfernt

                // Ignoriere negative Schadenswerte (Heilung)
                if (rawDamage <= 0)
                {
                    // Heilung-Logging entfernt
                    return null;
                }

                // Parse Player und Source
                var playerInfo = ParsePlayerInfo(parts[0], parts[1]);
                var sourceEntity = ParseEntityInfo(parts[2], parts[3]);
                var targetEntity = ParseEntityInfo(parts[4], parts[5]);

                // Erstelle CombatLogEntry
                var entry = new CombatLogEntry
                {
                    Timestamp = ParseTimestamp(timestamp),
                    PlayerInfo = playerInfo,
                    SourceEntity = sourceEntity,
                    TargetEntity = targetEntity,
                    AttackName = parts[6].Trim(),
                    AbilityId = parts[7].Trim(),
                    DamageType = parts[8].Trim(),
                    EventType = ParseEventType(parts[9]),
                    RawDamage = Math.Round(rawDamage),
                    DamageWithResistance = Math.Round(ParseDouble(parts[11])),
                    OriginalLine = line // Speichere Original-Zeile für Debugging
                };

                // Bestimme ob das ein Companion/Hangar-Pet ist
                entry.IsCompanionDamage = IsCompanionOrHangarPet(sourceEntity, playerInfo, line);

                // Debug-Logging entfernt - wird jetzt in StatisticsViewModel für ausgewählten Kampf gemacht

                // Bestimme ob kritisch und setze EventType entsprechend
                if (parts.Length > 12 && parts[12].Contains("Critical"))
                {
                    entry.EventType = EventType.Critical;
                }

                // Detailliertes Parsing-Logging entfernt

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
                // Format: YY:MM:DD:HH:mm:ss.m (Jahr:Monat:Tag:Stunde:Minute:Sekunde.Millisekunde)
                // Beispiel: 25:10:02:16:38:01.9 = 2025-10-02 16:38:01.900
                if (DateTime.TryParseExact(timestampData, "yy:MM:dd:HH:mm:ss.f",
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
                {
                    // Korrigiere 2-stellige Jahre zu 4-stelligen
                    if (result.Year < 2000)
                    {
                        return result.AddYears(2000);
                    }
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
