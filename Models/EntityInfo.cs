using System;

namespace StoDamageMeter.Models
{
    /// <summary>
    /// Entity-Informationen (parts[2-3] oder parts[4-5])
    /// </summary>
    public class EntityInfo
    {
        /// <summary>
        /// Entity-Name (parts[2] oder parts[4]) - roh wie in der Zeile
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Entity-Tag (parts[3] oder parts[5]) - roh wie in der Zeile
        /// </summary>
        public string EntityTag { get; set; } = string.Empty;

        /// <summary>
        /// Entity-ID aus Entity-Tag extrahiert
        /// </summary>
        public string EntityId { get; set; } = string.Empty;

        /// <summary>
        /// Entity-Name aus Entity-Tag extrahiert
        /// </summary>
        public string EntityName { get; set; } = string.Empty;

        /// <summary>
        /// Entity-Typ basierend auf Tag
        /// </summary>
        public EntityType Type { get; set; } = EntityType.Unknown;

        /// <summary>
        /// Kampf-Umgebung basierend auf Entity-Name
        /// </summary>
        public CombatEnvironment Environment { get; set; } = CombatEnvironment.Unknown;
    }
}
