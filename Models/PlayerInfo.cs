using System;

namespace StoDamageMeter.Models
{
    /// <summary>
    /// Player-Informationen aus parts[0-1]
    /// </summary>
    public class PlayerInfo
    {
        /// <summary>
        /// Spielername (parts[0])
        /// </summary>
        public string CharName { get; set; } = string.Empty;

        /// <summary>
        /// Player-Tag (parts[1]) - roh wie in der Zeile
        /// </summary>
        public string PlayerTag { get; set; } = string.Empty;

        /// <summary>
        /// Char-ID aus Player-Tag extrahiert
        /// </summary>
        public string CharId { get; set; } = string.Empty;

        /// <summary>
        /// Account-ID aus Player-Tag extrahiert
        /// </summary>
        public string AccountId { get; set; } = string.Empty;

        /// <summary>
        /// Handle aus Player-Tag extrahiert
        /// </summary>
        public string Handle { get; set; } = string.Empty;

        /// <summary>
        /// Discriminator aus Player-Tag extrahiert
        /// </summary>
        public string Discriminator { get; set; } = string.Empty;

        /// <summary>
        /// Ist dies ein echter Spieler (hat P-Tag)?
        /// </summary>
        public bool IsPlayer => PlayerTag.StartsWith("P[");
    }
}
