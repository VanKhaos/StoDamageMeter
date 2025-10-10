using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StoDamageMeter.Models
{
    /// <summary>
    /// Basis-Response für alle OSCR API-Aufrufe
    /// </summary>
    public class OSCRResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("error")]
        public string? Error { get; set; }

        [JsonPropertyName("timestamp")]
        public string? Timestamp { get; set; }
    }

    /// <summary>
    /// Response für Health-Check
    /// </summary>
    public class HealthCheckResponse : OSCRResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("version")]
        public string? Version { get; set; }
    }

    /// <summary>
    /// Response für Combat-Analyse
    /// </summary>
    public class CombatAnalysisResponse : OSCRResponse
    {
        [JsonPropertyName("combats")]
        public List<CombatData> Combats { get; set; } = new();

        [JsonPropertyName("totalCombats")]
        public int TotalCombats { get; set; }

        [JsonPropertyName("bytesConsumed")]
        public long BytesConsumed { get; set; }
    }

    /// <summary>
    /// Response für verfügbare Combats
    /// </summary>
    public class AvailableCombatsResponse : OSCRResponse
    {
        [JsonPropertyName("combats")]
        public List<CombatInfo> Combats { get; set; } = new();

        [JsonPropertyName("totalCombats")]
        public int TotalCombats { get; set; }
    }

    /// <summary>
    /// Basis-Informationen über einen Combat
    /// </summary>
    public class CombatInfo
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("map")]
        public string? Map { get; set; }

        [JsonPropertyName("date")]
        public string? Date { get; set; }

        [JsonPropertyName("time")]
        public string? Time { get; set; }

        [JsonPropertyName("difficulty")]
        public string? Difficulty { get; set; }

        [JsonPropertyName("byteStart")]
        public long ByteStart { get; set; }

        [JsonPropertyName("byteEnd")]
        public long ByteEnd { get; set; }

        [JsonPropertyName("type")]
        public string? Type { get; set; }
        
        /// <summary>
        /// Icon basierend auf Combat-Type (Space oder Ground)
        /// </summary>
        public string Icon => Type switch
        {
            "Ground" => "🏃",
            _ => "🚀" // Default ist Space
        };
    }

    /// <summary>
    /// Vollständige Combat-Daten mit Analyse-Ergebnissen
    /// </summary>
    public class CombatData
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("map")]
        public string? Map { get; set; }

        [JsonPropertyName("difficulty")]
        public string? Difficulty { get; set; }

        [JsonPropertyName("startTime")]
        public string? StartTime { get; set; }

        [JsonPropertyName("endTime")]
        public string? EndTime { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("players")]
        public Dictionary<string, PlayerStatistics> Players { get; set; } = new();

        [JsonPropertyName("metadata")]
        public CombatMetadata? Metadata { get; set; }

        [JsonPropertyName("critters")]
        public Dictionary<string, CritterData> Critters { get; set; } = new();
    }

    /// <summary>
    /// Ability-Statistiken für einen Spieler
    /// </summary>
    public class AbilityStatistics
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("totalDamage")]
        public double TotalDamage { get; set; }

        [JsonPropertyName("dps")]
        public double Dps { get; set; }

        [JsonPropertyName("maxHit")]
        public double MaxHit { get; set; }

        [JsonPropertyName("critPercent")]
        public double CritPercent { get; set; }

        [JsonPropertyName("accuracyPercent")]
        public double AccuracyPercent { get; set; }

        [JsonPropertyName("attacks")]
        public int Attacks { get; set; }

        [JsonPropertyName("damageType")]
        public string? DamageType { get; set; }
    }

    /// <summary>
    /// Companion-Statistiken (Pets, Drohnen, Außenteam)
    /// </summary>
    public class CompanionStatistics
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("dps")]
        public double Dps { get; set; }

        [JsonPropertyName("totalDamage")]
        public double TotalDamage { get; set; }

        [JsonPropertyName("debuff")]
        public double Debuff { get; set; }

        [JsonPropertyName("maxOneHit")]
        public double MaxOneHit { get; set; }

        [JsonPropertyName("critPercent")]
        public double CritPercent { get; set; }

        [JsonPropertyName("accuracyPercent")]
        public double AccuracyPercent { get; set; }

        [JsonPropertyName("abilities")]
        public List<AbilityStatistics> Abilities { get; set; } = new();
    }

    /// <summary>
    /// Spieler-Statistiken für einen Combat
    /// </summary>
    public class PlayerStatistics
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("dps")]
        public double Dps { get; set; }

        [JsonPropertyName("combatTime")]
        public double CombatTime { get; set; }

        [JsonPropertyName("combatTimeShare")]
        public double CombatTimeShare { get; set; }

        [JsonPropertyName("totalDamage")]
        public double TotalDamage { get; set; }

        [JsonPropertyName("debuff")]
        public double Debuff { get; set; }

        [JsonPropertyName("attacksInShare")]
        public double AttacksInShare { get; set; }

        [JsonPropertyName("takenDamageShare")]
        public double TakenDamageShare { get; set; }

        [JsonPropertyName("damageShare")]
        public double DamageShare { get; set; }

        [JsonPropertyName("maxOneHit")]
        public double MaxOneHit { get; set; }

        [JsonPropertyName("deaths")]
        public int Deaths { get; set; }

        [JsonPropertyName("critPercent")]
        public double CritPercent { get; set; }

        [JsonPropertyName("accuracyPercent")]
        public double AccuracyPercent { get; set; }

        [JsonPropertyName("abilities")]
        public List<AbilityStatistics> Abilities { get; set; } = new();

        [JsonPropertyName("companions")]
        public List<CompanionStatistics> Companions { get; set; } = new();

        [JsonPropertyName("dpsWithCompanions")]
        public double DpsWithCompanions { get; set; }

        [JsonPropertyName("totalDamageWithCompanions")]
        public double TotalDamageWithCompanions { get; set; }
    }

    /// <summary>
    /// Combat-Metadaten
    /// </summary>
    public class CombatMetadata
    {
        [JsonPropertyName("logDuration")]
        public double LogDuration { get; set; }

        [JsonPropertyName("playerDuration")]
        public double PlayerDuration { get; set; }

        [JsonPropertyName("totalLines")]
        public int TotalLines { get; set; }
    }

    /// <summary>
    /// NPC/Critter-Daten
    /// </summary>
    public class CritterData
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }

        [JsonPropertyName("deaths")]
        public int Deaths { get; set; }

        [JsonPropertyName("hullValues")]
        public List<double> HullValues { get; set; } = new();
    }

    /// <summary>
    /// Request-Parameter für Combat-Analyse
    /// </summary>
    public class CombatAnalysisRequest
    {
        [JsonPropertyName("action")]
        public string Action { get; set; } = "analyze";

        [JsonPropertyName("logPath")]
        public string? LogPath { get; set; }

        [JsonPropertyName("maxCombats")]
        public int MaxCombats { get; set; } = 10;

        [JsonPropertyName("settings")]
        public AnalysisSettings? Settings { get; set; }
    }

    /// <summary>
    /// Request-Parameter für einzelnen Combat
    /// </summary>
    public class SingleCombatAnalysisRequest
    {
        [JsonPropertyName("action")]
        public string Action { get; set; } = "analyze_single";

        [JsonPropertyName("logPath")]
        public string? LogPath { get; set; }

        [JsonPropertyName("combatId")]
        public int CombatId { get; set; }

        [JsonPropertyName("settings")]
        public AnalysisSettings? Settings { get; set; }
    }

    /// <summary>
    /// Request-Parameter für verfügbare Combats
    /// </summary>
    public class AvailableCombatsRequest
    {
        [JsonPropertyName("action")]
        public string Action { get; set; } = "list";

        [JsonPropertyName("logPath")]
        public string? LogPath { get; set; }

        [JsonPropertyName("maxCombats")]
        public int MaxCombats { get; set; } = 50;
    }

    /// <summary>
    /// Analyse-Einstellungen
    /// </summary>
    public class AnalysisSettings
    {
        [JsonPropertyName("combatsToParse")]
        public int CombatsToParse { get; set; } = 10;

        [JsonPropertyName("secondsBetweenCombats")]
        public int SecondsBetweenCombats { get; set; } = 100;

        [JsonPropertyName("combatMinLines")]
        public int CombatMinLines { get; set; } = 20;
    }

    /// <summary>
    /// Health-Check Request
    /// </summary>
    public class HealthCheckRequest
    {
        [JsonPropertyName("action")]
        public string Action { get; set; } = "health";
    }
}
