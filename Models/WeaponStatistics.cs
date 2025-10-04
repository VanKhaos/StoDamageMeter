using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StoDamageMeter.Models
{
    /// <summary>
    /// Repräsentiert aggregierte Statistiken für eine Waffe/Fähigkeit
    /// </summary>
    public class WeaponStatistics : INotifyPropertyChanged
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "weapon"; // "weapon" oder "companion"
        public string SourceType { get; set; } = "player"; // "player", "companion", "kitmodul"
        public double TotalDamage { get; set; }
        public double AverageDamage { get; set; }
        public int CriticalHits { get; set; }
        public int TotalUses { get; set; }
        public double CriticalRate => TotalUses > 0 ? (CriticalHits / (double)TotalUses) * 100 : 0;
        public double DamageShare { get; set; } // Anteil am Gesamtschaden in %
        public double DPS { get; set; }

        // Für Companion-Erweiterung
        public List<WeaponStatistics> Abilities { get; set; } = new();
        public List<WeaponStatistics> Weapons { get; set; } = new();

        // Original CombatLog-Daten für Debugging
        public List<CombatLogEntry> OriginalEntries { get; set; } = new();

        private bool _isExpanded;
        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged();
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public WeaponStatistics()
        {
        }

        public WeaponStatistics(string name, string type = "weapon")
        {
            Name = name;
            Type = type;
        }
    }
}
