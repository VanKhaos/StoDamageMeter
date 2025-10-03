using System;

namespace StoDamageMeter.Models
{
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
        Enemy       // C[ID]
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
