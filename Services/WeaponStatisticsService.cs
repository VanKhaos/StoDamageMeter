using Microsoft.Extensions.Logging;
using StoDamageMeter.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StoDamageMeter.Services
{
    /// <summary>
    /// Service für die Aggregation von Waffen-Statistiken
    /// </summary>
    public class WeaponStatisticsService
    {
        private readonly ILogger<WeaponStatisticsService> _logger;

        public WeaponStatisticsService(ILogger<WeaponStatisticsService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Aggregiert Combat Log Einträge zu Waffen-Statistiken
        /// </summary>
        /// <param name="entries">Combat Log Einträge</param>
        /// <param name="duration">Kampfdauer in Sekunden</param>
        /// <returns>Liste von aggregierten Waffen-Statistiken</returns>
        public List<WeaponStatistics> GetWeaponStatistics(List<CombatLogEntry> entries, double duration)
        {
            if (entries == null || !entries.Any())
            {
                _logger.LogInformation("Keine Einträge für Waffen-Statistiken verfügbar");
                return new List<WeaponStatistics>();
            }

            _logger.LogInformation("=== ROHDATEN für Waffen-Statistiken ===");
            _logger.LogInformation("Anzahl Einträge: {EntryCount}", entries.Count);
            _logger.LogInformation("Kampfdauer: {Duration:F2} Sekunden", duration);

            // Logge die ersten 3 Einträge mit vollständigen Rohdaten
            for (int i = 0; i < Math.Min(3, entries.Count); i++)
            {
                var entry = entries[i];
                _logger.LogInformation("--- Eintrag {Index} ---", i + 1);
                _logger.LogInformation("Timestamp: {Timestamp}", entry.Timestamp);
                _logger.LogInformation("Player: {PlayerName} (Tag: {PlayerTag})", entry.PlayerInfo?.CharName ?? "N/A", entry.PlayerInfo?.PlayerTag ?? "N/A");
                _logger.LogInformation("Source: {SourceName} (ID: {SourceId})", entry.SourceEntity?.Name ?? "N/A", entry.SourceEntity?.EntityId ?? "N/A");
                _logger.LogInformation("Target: {TargetName} (ID: {TargetId})", entry.TargetEntity?.Name ?? "N/A", entry.TargetEntity?.EntityId ?? "N/A");
                _logger.LogInformation("Attack: {AttackName}", entry.AttackName);
                _logger.LogInformation("AbilityId: {AbilityId}", entry.AbilityId);
                _logger.LogInformation("DamageType: {DamageType}", entry.DamageType);
                _logger.LogInformation("EventType: {EventType}", entry.EventType);
                _logger.LogInformation("RawDamage: {RawDamage}", entry.RawDamage);
                _logger.LogInformation("DamageWithResistance: {DamageWithResistance}", entry.DamageWithResistance);
                _logger.LogInformation("IsCritical: {IsCritical}", entry.IsCritical);
            }
            _logger.LogInformation("=== Ende ROHDATEN ===");

            _logger.LogInformation("Aggregiere Waffen-Statistiken aus {EntryCount} Einträgen", entries.Count);

            var weaponStats = new List<WeaponStatistics>();
            var totalDamage = entries.Sum(e => e.RawDamage);

            // Trenne Companion-Schäden von Spieler-Schäden
            var playerEntries = entries.Where(e => !e.IsCompanionDamage).ToList();
            var companionEntries = entries.Where(e => e.IsCompanionDamage).ToList();

            // Verarbeite Spieler-Waffen
            var playerWeaponGroups = playerEntries
                .Where(e => !string.IsNullOrEmpty(e.AttackName))
                .GroupBy(e => e.AttackName)
                .ToList();

            foreach (var group in playerWeaponGroups)
            {
                var weaponName = group.Key;
                var weaponEntries = group.ToList();

                var weaponStat = new WeaponStatistics(weaponName, "weapon")
                {
                    TotalDamage = weaponEntries.Sum(e => e.RawDamage),
                    TotalUses = weaponEntries.Count,
                    CriticalHits = weaponEntries.Count(e => e.IsCritical),
                    DPS = duration > 0 ? weaponEntries.Sum(e => e.RawDamage) / duration : 0
                };

                // Berechne Durchschnittsschaden
                weaponStat.AverageDamage = weaponStat.TotalUses > 0
                    ? weaponStat.TotalDamage / weaponStat.TotalUses
                    : 0;

                // Berechne Schadensanteil
                weaponStat.DamageShare = totalDamage > 0
                    ? (weaponStat.TotalDamage / totalDamage) * 100
                    : 0;

                weaponStats.Add(weaponStat);
            }

            // Verarbeite Companions
            var companionGroups = companionEntries
                .Where(e => !string.IsNullOrEmpty(e.SourceEntity?.Name))
                .GroupBy(e => e.SourceEntity.Name)
                .ToList();

            foreach (var group in companionGroups)
            {
                var companionName = group.Key;
                var companionGroupEntries = group.ToList();

                var companionStat = new WeaponStatistics(companionName, "companion")
                {
                    TotalDamage = companionGroupEntries.Sum(e => e.RawDamage),
                    TotalUses = companionGroupEntries.Count,
                    CriticalHits = companionGroupEntries.Count(e => e.IsCritical),
                    DPS = duration > 0 ? companionGroupEntries.Sum(e => e.RawDamage) / duration : 0
                };

                // Berechne Durchschnittsschaden
                companionStat.AverageDamage = companionStat.TotalUses > 0
                    ? companionStat.TotalDamage / companionStat.TotalUses
                    : 0;

                // Berechne Schadensanteil
                companionStat.DamageShare = totalDamage > 0
                    ? (companionStat.TotalDamage / totalDamage) * 100
                    : 0;

                // Erstelle Unterstatistiken für Companion-Waffen
                var companionWeaponGroups = companionGroupEntries
                    .Where(e => !string.IsNullOrEmpty(e.AttackName))
                    .GroupBy(e => e.AttackName)
                    .ToList();

                foreach (var weaponGroup in companionWeaponGroups)
                {
                    var weaponName = weaponGroup.Key;
                    var weaponEntries = weaponGroup.ToList();

                    var weaponStat = new WeaponStatistics(weaponName, "weapon")
                    {
                        TotalDamage = weaponEntries.Sum(e => e.RawDamage),
                        TotalUses = weaponEntries.Count,
                        CriticalHits = weaponEntries.Count(e => e.IsCritical),
                        DPS = duration > 0 ? weaponEntries.Sum(e => e.RawDamage) / duration : 0
                    };

                    // Berechne Durchschnittsschaden
                    weaponStat.AverageDamage = weaponStat.TotalUses > 0
                        ? weaponStat.TotalDamage / weaponStat.TotalUses
                        : 0;

                    // Berechne Schadensanteil
                    weaponStat.DamageShare = companionStat.TotalDamage > 0
                        ? (weaponStat.TotalDamage / companionStat.TotalDamage) * 100
                        : 0;

                    companionStat.Weapons.Add(weaponStat);
                }

                // Sortiere Companion-Waffen nach Schaden
                companionStat.Weapons = companionStat.Weapons
                    .OrderByDescending(w => w.TotalDamage)
                    .ToList();

                weaponStats.Add(companionStat);
            }

            // Sortiere nach Schaden (absteigend)
            weaponStats = weaponStats
                .OrderByDescending(w => w.TotalDamage)
                .ToList();

            _logger.LogInformation("=== AGGREGIERTE Waffen-Statistiken ===");
            _logger.LogInformation("Anzahl Waffen: {WeaponCount}", weaponStats.Count);

            // Logge die ersten 3 aggregierten Waffen
            for (int i = 0; i < Math.Min(3, weaponStats.Count); i++)
            {
                var weapon = weaponStats[i];
                _logger.LogInformation("Waffe {Index}: {Name} | Schaden: {TotalDamage} | DPS: {DPS:F2} | Verwendungen: {TotalUses} | Kritisch: {CriticalHits}",
                    i + 1, weapon.Name, weapon.TotalDamage, weapon.DPS, weapon.TotalUses, weapon.CriticalHits);
            }
            _logger.LogInformation("=== Ende AGGREGIERTE Waffen-Statistiken ===");

            return weaponStats;
        }

        /// <summary>
        /// Aggregiert Waffen-Statistiken für einen bestimmten Spieler
        /// </summary>
        /// <param name="entries">Alle Combat Log Einträge</param>
        /// <param name="playerName">Name des Spielers</param>
        /// <param name="duration">Kampfdauer in Sekunden</param>
        /// <returns>Liste von aggregierten Waffen-Statistiken für den Spieler</returns>
        public List<WeaponStatistics> GetPlayerWeaponStatistics(List<CombatLogEntry> entries, string playerName, double duration)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                _logger.LogWarning("Spielername ist leer - kann keine Waffen-Statistiken erstellen");
                return new List<WeaponStatistics>();
            }

            // Filtere Einträge für den spezifischen Spieler UND nur relevante Einträge
            var playerEntries = entries
                .Where(e => e.PlayerInfo?.CharName == playerName && e.IsRelevant)
                .ToList();

            _logger.LogInformation("Erstelle Waffen-Statistiken für Spieler '{PlayerName}' aus {EntryCount} relevanten Einträgen",
                playerName, playerEntries.Count);

            // Debug: Zeige die gefilterten Einträge
            _logger.LogInformation("=== GEFILTERTE Einträge für Spieler '{PlayerName}' ===", playerName);
            foreach (var entry in playerEntries.Take(5))
            {
                _logger.LogInformation("Eintrag: {AttackName} | {DamageType} | {RawDamage} Schaden | Player: {PlayerName} | Source: {SourceName}",
                    entry.AttackName, entry.DamageType, entry.RawDamage, entry.PlayerInfo?.CharName, entry.SourceEntity?.Name);
            }
            _logger.LogInformation("=== Ende GEFILTERTE Einträge ===");

            return GetWeaponStatistics(playerEntries, duration);
        }
    }
}
