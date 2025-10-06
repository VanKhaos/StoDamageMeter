using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using StoDamageMeter.Models;

namespace StoDamageMeter.Services
{
    /// <summary>
    /// Service für die Erkennung von Kampf-Zeiträumen für spezifische Spieler
    /// </summary>
    public class CombatPeriodService
    {
        private readonly ILogger<CombatPeriodService> _logger;

        public CombatPeriodService(ILogger<CombatPeriodService> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Erkennt Kampf-Zeiträume für einen spezifischen Spieler
        /// </summary>
        /// <param name="allEntries">Alle Combat Log Einträge</param>
        /// <param name="playerName">Name des Spielers</param>
        /// <param name="combatBreakThresholdSeconds">Schwellenwert in Sekunden für Kampfende (Standard: 10)</param>
        /// <returns>Liste der erkannten Kampf-Zeiträume</returns>
        public List<CombatPeriod> GetCombatPeriodsForPlayer(
            List<CombatLogEntry> allEntries,
            string playerName,
            int combatBreakThresholdSeconds = 10)
        {
            _logger.LogInformation("=== GetCombatPeriodsForPlayer() gestartet ===");
            _logger.LogInformation("Spieler: '{PlayerName}'", playerName);
            _logger.LogInformation("Gesamtanzahl Einträge: {TotalEntries}", allEntries.Count);
            _logger.LogInformation("Kampf-Pause-Schwellenwert: {Threshold}s", combatBreakThresholdSeconds);

            if (string.IsNullOrEmpty(playerName) || !allEntries.Any())
            {
                _logger.LogInformation("Keine Daten - Spieler leer oder keine Einträge");
                return new List<CombatPeriod>();
            }

            // Filtere Einträge für den spezifischen Spieler
            var playerEntries = allEntries
                .Where(e => e.PlayerInfo.CharName == playerName && e.IsRelevant)
                .OrderBy(e => e.Timestamp)
                .ToList();

            _logger.LogInformation("Gefilterte Einträge für Spieler: {PlayerEntryCount}", playerEntries.Count);

            if (!playerEntries.Any())
            {
                _logger.LogInformation("Keine Einträge für Spieler '{PlayerName}' gefunden", playerName);
                return new List<CombatPeriod>();
            }

            _logger.LogInformation("Erster Eintrag: {FirstTime} - {FirstDamage} Schaden",
                playerEntries.First().Timestamp, playerEntries.First().RawDamage);
            _logger.LogInformation("Letzter Eintrag: {LastTime} - {LastDamage} Schaden",
                playerEntries.Last().Timestamp, playerEntries.Last().RawDamage);

            var periods = new List<CombatPeriod>();
            CombatPeriod? currentPeriod = null;
            DateTime lastEventTime = DateTime.MinValue;

            foreach (var entry in playerEntries)
            {
                // Prüfe ob ein neuer Kampf beginnen muss
                if (currentPeriod == null ||
                    (entry.Timestamp - lastEventTime).TotalSeconds > combatBreakThresholdSeconds)
                {
                    // Beende vorherige Periode falls vorhanden
                    if (currentPeriod != null)
                    {
                        periods.Add(currentPeriod);

                    }

                    // Starte neue Periode
                    currentPeriod = new CombatPeriod
                    {
                        StartTime = entry.Timestamp
                    };


                }

                // Füge Eintrag zur aktuellen Periode hinzu
                currentPeriod.AddEntry(entry);
                lastEventTime = entry.Timestamp;
            }

            // Füge die letzte Periode hinzu
            if (currentPeriod != null)
            {
                periods.Add(currentPeriod);

            }

            _logger.LogInformation("Kampf-Erkennung abgeschlossen für '{PlayerName}': {PeriodCount} Zeiträume erkannt",
                playerName, periods.Count);

            // Zeige Details der letzten 10 Zeiträume (falls vorhanden)
            var lastPeriods = periods.TakeLast(10).ToList();
            for (int i = 0; i < lastPeriods.Count; i++)
            {
                var period = lastPeriods[i];
                _logger.LogInformation("Zeitraum {Index}: {StartTime} - {EndTime} ({Duration:F1}s) - {EntryCount} Einträge - {TotalDamage:F0} Schaden",
                    i + 1, period.StartTime, period.EndTime, period.Duration.TotalSeconds, period.EntryCount, period.TotalDamage);
            }

            _logger.LogInformation("=== GetCombatPeriodsForPlayer() beendet ===");

            return periods;
        }

        /// <summary>
        /// Gibt alle verfügbaren Spieler aus den Einträgen zurück
        /// </summary>
        /// <param name="allEntries">Alle Combat Log Einträge</param>
        /// <returns>Liste der Spielernamen</returns>
        public List<string> GetAvailablePlayers(List<CombatLogEntry> allEntries)
        {
            _logger.LogInformation("=== GetAvailablePlayers aufgerufen ===");
            _logger.LogInformation("Anzahl Einträge: {EntryCount}", allEntries.Count);

            var playerEntries = allEntries
                .Where(e => e.PlayerInfo.IsPlayer && !string.IsNullOrEmpty(e.PlayerInfo.CharName))
                .ToList();

            _logger.LogInformation("Einträge mit gültigen Spielern: {PlayerEntryCount}", playerEntries.Count);

            // Debug: Zeige erste paar Einträge
            for (int i = 0; i < Math.Min(5, playerEntries.Count); i++)
            {
                var entry = playerEntries[i];
                _logger.LogInformation("Eintrag {Index}: Player='{PlayerName}', IsPlayer={IsPlayer}, CharName='{CharName}'",
                    i, entry.PlayerInfo.CharName, entry.PlayerInfo.IsPlayer, entry.PlayerInfo.CharName);
            }

            var players = playerEntries
                .Select(e => e.PlayerInfo.CharName)
                .Distinct()
                .OrderBy(name => name)
                .ToList();

            _logger.LogInformation("Eindeutige Spieler gefunden: {PlayerCount}", players.Count);
            foreach (var player in players)
            {
                _logger.LogInformation("Spieler: '{PlayerName}'", player);
            }

            _logger.LogInformation("=== GetAvailablePlayers beendet ===");
            return players;
        }
    }
}
