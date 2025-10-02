using System;

namespace StoDamageMeter.Models
{
    /// <summary>
    /// Repräsentiert einen einzelnen Combatlog-Eintrag mit vollständiger Type-Safety
    /// </summary>
    public class CombatLogEntry
    {
        /// <summary>
        /// Timestamp des Events (Position 1)
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Spielername (Position 3)
        /// </summary>
        public string PlayerName { get; set; } = string.Empty;

        /// <summary>
        /// Player-ID Information (Position 3)
        /// </summary>
        public PlayerInfo PlayerInfo { get; set; } = new();

        /// <summary>
        /// Entity-Details (Position 4)
        /// </summary>
        public EntityInfo SourceEntity { get; set; } = new();

        /// <summary>
        /// Ziel-Entity (Position 5)
        /// </summary>
        public EntityInfo TargetEntity { get; set; } = new();

        /// <summary>
        /// Attack/Fähigkeit Name (Position 6)
        /// </summary>
        public string AttackName { get; set; } = string.Empty;

        /// <summary>
        /// Ability-ID (Position 7) - uninteressant
        /// </summary>
        public string AbilityId { get; set; } = string.Empty;

        /// <summary>
        /// Schadensart (Position 8)
        /// </summary>
        public string DamageType { get; set; } = string.Empty;

        /// <summary>
        /// Event-Typ (Position 9)
        /// </summary>
        public EventType EventType { get; set; } = EventType.Normal;

        /// <summary>
        /// Roher Schaden (Position 10)
        /// </summary>
        public double RawDamage { get; set; }

        /// <summary>
        /// Schaden mit Resistenzen (Position 11)
        /// </summary>
        public double DamageWithResistance { get; set; }

        /// <summary>
        /// Ist dieser Eintrag für Damage Meter relevant?
        /// </summary>
        public bool IsRelevant => EventType == EventType.Normal || EventType == EventType.Critical;

        /// <summary>
        /// Ist es ein kritischer Treffer?
        /// </summary>
        public bool IsCritical => EventType == EventType.Critical;

        /// <summary>
        /// Ist es ein DoT (Damage over Time)?
        /// </summary>
        public bool IsDoT => EventType == EventType.DoT;
    }

    /// <summary>
    /// Player-Informationen aus Position 3
    /// </summary>
    public class PlayerInfo
    {
        public string CharId { get; set; } = string.Empty;
        public string AccountId { get; set; } = string.Empty;
        public string CharName { get; set; } = string.Empty;
        public string Handle { get; set; } = string.Empty;
        public string Discriminator { get; set; } = string.Empty;
    }

    /// <summary>
    /// Entity-Informationen (Position 4 oder 5)
    /// </summary>
    public class EntityInfo
    {
        public string Name { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public EntityType Type { get; set; } = EntityType.Unknown;
        public CombatEnvironment Environment { get; set; } = CombatEnvironment.Unknown;
    }

    /// <summary>
    /// Event-Typen (Position 9)
    /// </summary>
    public enum EventType
    {
        Normal,     // Leer
        Critical,   // "Critical"
        DoT,        // "DoT"
        Miss,       // "Miss"
        Immune      // "Immune"
    }

    /// <summary>
    /// Entity-Typen basierend auf ID-Tags
    /// </summary>
    public enum EntityType
    {
        Unknown,
        Player,     // P[ID]
        Companion,  // S[ID]
        Creature    // C[ID]
    }

    /// <summary>
    /// Kampf-Umgebung basierend auf Entity-Namen
    /// </summary>
    public enum CombatEnvironment
    {
        Unknown,
        Ground,     // Ground_ prefix
        Space       // Space_ prefix
    }
}
