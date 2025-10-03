using System;

namespace StoDamageMeter.Models
{
    /// <summary>
    /// Repräsentiert einen einzelnen Combatlog-Eintrag
    /// </summary>
    public class CombatLogEntry
    {
        /// <summary>
        /// Timestamp des Events (Position 1)
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Player-Informationen (parts[0-1])
        /// </summary>
        public PlayerInfo PlayerInfo { get; set; } = new();

        /// <summary>
        /// Source-Entity-Informationen (parts[2-3])
        /// </summary>
        public EntityInfo SourceEntity { get; set; } = new();

        /// <summary>
        /// Target-Entity-Informationen (parts[4-5])
        /// </summary>
        public EntityInfo TargetEntity { get; set; } = new();

        /// <summary>
        /// Attack/Fähigkeit Name (parts[6])
        /// </summary>
        public string AttackName { get; set; } = string.Empty;

        /// <summary>
        /// Ability-ID (parts[7]) - roh wie in der Zeile
        /// </summary>
        public string AbilityId { get; set; } = string.Empty;

        /// <summary>
        /// Schadensart (parts[8])
        /// </summary>
        public string DamageType { get; set; } = string.Empty;

        /// <summary>
        /// Event-Typ (parts[9])
        /// </summary>
        public EventType EventType { get; set; } = EventType.Normal;

        /// <summary>
        /// Roher Schaden (parts[10])
        /// </summary>
        public double RawDamage { get; set; }

        /// <summary>
        /// Schaden mit Resistenzen (parts[11])
        /// </summary>
        public double DamageWithResistance { get; set; }

        /// <summary>
        /// Ist dieser Eintrag für Damage Meter relevant?
        /// Nur positive Schadenswerte sind relevant (keine Heilung)
        /// </summary>
        public bool IsRelevant => RawDamage > 0;

        /// <summary>
        /// Ist es ein kritischer Treffer?
        /// </summary>
        public bool IsCritical => EventType == EventType.Critical;

        /// <summary>
        /// Ist es ein DoT (Damage over Time)?
        /// </summary>
        public bool IsDoT => EventType == EventType.DoT;
    }
}
