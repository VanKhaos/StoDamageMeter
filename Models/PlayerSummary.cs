using System;

namespace StoDamageMeter.Models
{
    /// <summary>
    /// Zusammenfassung eines Spielers für die Dashboard-Tabelle
    /// </summary>
    public class PlayerSummary
    {
        /// <summary>
        /// Spielername
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// DPS (Damage per Second)
        /// </summary>
        public double DPS { get; set; }

        /// <summary>
        /// Durchschnittlicher Schaden pro Treffer
        /// </summary>
        public double AverageDamage { get; set; }

        /// <summary>
        /// Gesamtschaden
        /// </summary>
        public double TotalDamage { get; set; }

        /// <summary>
        /// Anzahl der Kills
        /// </summary>
        public int Kills { get; set; }

        /// <summary>
        /// Anzahl der Treffer
        /// </summary>
        public int Hits { get; set; }

        /// <summary>
        /// Kampfdauer in Sekunden
        /// </summary>
        public double Duration { get; set; }
    }
}
