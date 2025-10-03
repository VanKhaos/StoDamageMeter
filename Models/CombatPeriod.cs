using System;
using System.Collections.Generic;
using System.Linq;

namespace StoDamageMeter.Models
{
    /// <summary>
    /// Repräsentiert einen Kampf-Zeitraum für einen spezifischen Spieler
    /// </summary>
    public class CombatPeriod
    {
        /// <summary>
        /// Startzeit des Kampfes
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Endzeit des Kampfes
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Alle Combat Log Einträge dieses Zeitraums für den Spieler
        /// </summary>
        public List<CombatLogEntry> Entries { get; set; } = new();

        /// <summary>
        /// Dauer des Kampfes
        /// </summary>
        public TimeSpan Duration => EndTime - StartTime;

        /// <summary>
        /// Gesamtschaden in diesem Zeitraum
        /// </summary>
        public double TotalDamage => Entries.Sum(e => e.RawDamage);

        /// <summary>
        /// DPS (Damage Per Second) dieses Zeitraums
        /// </summary>
        public double DPS => Duration.TotalSeconds > 0 ? TotalDamage / Duration.TotalSeconds : 0;

        /// <summary>
        /// Anzahl der relevanten Einträge (mit Schaden)
        /// </summary>
        public int EntryCount => Entries.Count;

        /// <summary>
        /// Anzahl der kritischen Treffer
        /// </summary>
        public int CriticalHits => Entries.Count(e => e.IsCritical);

        /// <summary>
        /// Kritische Treffer Rate in Prozent
        /// </summary>
        public double CriticalRate => EntryCount > 0 ? (double)CriticalHits / EntryCount * 100 : 0;

        /// <summary>
        /// Durchschnittlicher Schaden pro Treffer
        /// </summary>
        public double AverageDamage => EntryCount > 0 ? TotalDamage / EntryCount : 0;

        /// <summary>
        /// Fügt einen neuen Eintrag zum Zeitraum hinzu
        /// </summary>
        /// <param name="entry">Der hinzuzufügende Combat Log Eintrag</param>
        public void AddEntry(CombatLogEntry entry)
        {
            Entries.Add(entry);

            // Aktualisiere Start- und Endzeit
            if (StartTime == default || entry.Timestamp < StartTime)
            {
                StartTime = entry.Timestamp;
            }

            if (EndTime == default || entry.Timestamp > EndTime)
            {
                EndTime = entry.Timestamp;
            }
        }

        /// <summary>
        /// String-Repräsentation des Zeitraums
        /// </summary>
        public override string ToString()
        {
            return $"{StartTime:dd.MM.yyyy | HH:mm} - {EndTime:dd.MM.yyyy | HH:mm} ({Duration.TotalSeconds:F1}s) - {TotalDamage:F0} Schaden";
        }
    }
}
