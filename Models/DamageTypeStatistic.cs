namespace StoDamageMeter.Models
{
    /// <summary>
    /// Statistik für eine Schadensart
    /// </summary>
    public class DamageTypeStatistic
    {
        /// <summary>
        /// Name der Schadensart
        /// </summary>
        public string DamageType { get; set; } = string.Empty;

        /// <summary>
        /// Gesamtschaden dieser Art
        /// </summary>
        public double TotalDamage { get; set; }

        /// <summary>
        /// Anzahl Treffer
        /// </summary>
        public int HitCount { get; set; }

        /// <summary>
        /// Anzahl verfehlter Angriffe
        /// </summary>
        public int MissCount { get; set; }

        /// <summary>
        /// Anzahl kritischer Treffer
        /// </summary>
        public int CriticalHits { get; set; }

        /// <summary>
        /// Durchschnittlicher Schaden
        /// </summary>
        public double AverageDamage { get; set; }

        /// <summary>
        /// Schadensanteil in Prozent
        /// </summary>
        public double DamageShare { get; set; }

        /// <summary>
        /// DPS (Damage Per Second) für diese Schadensart
        /// </summary>
        public double DPS { get; set; }

        /// <summary>
        /// Genauigkeit in Prozent (Treffer / Gesamtversuche)
        /// </summary>
        public double Accuracy { get; set; }

        /// <summary>
        /// Formatierter Gesamtschaden
        /// </summary>
        public string FormattedTotalDamage => TotalDamage.ToString("N0");

        /// <summary>
        /// Formatierter durchschnittlicher Schaden
        /// </summary>
        public string FormattedAverageDamage => AverageDamage.ToString("N0");

        /// <summary>
        /// Formatierter Schadensanteil
        /// </summary>
        public string FormattedDamageShare => $"{DamageShare:F1}%";

        /// <summary>
        /// Formatierte kritische Rate
        /// </summary>
        public string FormattedCriticalRate => HitCount > 0 ? $"{(double)CriticalHits / HitCount * 100:F1}%" : "0.0%";

        /// <summary>
        /// Formatierte Anzahl verfehlter Angriffe
        /// </summary>
        public string FormattedMissCount => MissCount.ToString("N0");
    }
}
