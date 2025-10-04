using System;

namespace StoDamageMeter.Models
{
    /// <summary>
    /// Zusammenfassung der Kampfstatistiken für einen Spieler
    /// </summary>
    public class PlayerCombatSummary
    {
        /// <summary>
        /// Name des Spielers
        /// </summary>
        public string PlayerName { get; set; } = string.Empty;

        /// <summary>
        /// Gesamtschaden im Kampf
        /// </summary>
        public double TotalDamage { get; set; }

        /// <summary>
        /// Damage Per Second
        /// </summary>
        public double DPS { get; set; }

        /// <summary>
        /// Anzahl kritischer Treffer
        /// </summary>
        public int CriticalHits { get; set; }

        /// <summary>
        /// Gesamtanzahl Treffer
        /// </summary>
        public int TotalHits { get; set; }

        /// <summary>
        /// Kritische Trefferrate in Prozent
        /// </summary>
        public double CriticalRate { get; set; }

        /// <summary>
        /// Durchschnittlicher Schaden pro Treffer
        /// </summary>
        public double AverageDamage { get; set; }

        /// <summary>
        /// Kampfdauer
        /// </summary>
        public TimeSpan CombatDuration { get; set; }

        /// <summary>
        /// Kampfbeginn
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// Kampfende
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// Formatierte Kampfdauer
        /// </summary>
        public string FormattedDuration => CombatDuration.ToString(@"mm\:ss");

        /// <summary>
        /// Formatierter Gesamtschaden
        /// </summary>
        public string FormattedTotalDamage => TotalDamage.ToString("N0");

        /// <summary>
        /// Formatierter DPS
        /// </summary>
        public string FormattedDPS => DPS.ToString("N0");

        /// <summary>
        /// Formatierte kritische Rate
        /// </summary>
        public string FormattedCriticalRate => $"{CriticalRate:F1}%";

        /// <summary>
        /// Formatierter durchschnittlicher Schaden
        /// </summary>
        public string FormattedAverageDamage => AverageDamage.ToString("N0");
    }
}
