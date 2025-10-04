using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace StoDamageMeter.Models
{
    public class StatisticsFilter : INotifyPropertyChanged
    {
        private DateTime? _startTime;
        private DateTime? _endTime;
        private double? _minDamage;
        private double? _maxDamage;
        private List<string> _includedWeapons = new();
        private List<string> _excludedWeapons = new();
        private List<string> _includedDamageTypes = new();
        private bool _includeCompanionDamage = true;
        private bool _includeDirectPlayerDamage = true;

        public DateTime? StartTime
        {
            get => _startTime;
            set => SetProperty(ref _startTime, value);
        }

        public DateTime? EndTime
        {
            get => _endTime;
            set => SetProperty(ref _endTime, value);
        }

        public double? MinDamage
        {
            get => _minDamage;
            set => SetProperty(ref _minDamage, value);
        }

        public double? MaxDamage
        {
            get => _maxDamage;
            set => SetProperty(ref _maxDamage, value);
        }

        public List<string> IncludedWeapons
        {
            get => _includedWeapons;
            set => SetProperty(ref _includedWeapons, value);
        }

        public List<string> ExcludedWeapons
        {
            get => _excludedWeapons;
            set => SetProperty(ref _excludedWeapons, value);
        }

        public List<string> IncludedDamageTypes
        {
            get => _includedDamageTypes;
            set => SetProperty(ref _includedDamageTypes, value);
        }

        public bool IncludeCompanionDamage
        {
            get => _includeCompanionDamage;
            set => SetProperty(ref _includeCompanionDamage, value);
        }

        public bool IncludeDirectPlayerDamage
        {
            get => _includeDirectPlayerDamage;
            set => SetProperty(ref _includeDirectPlayerDamage, value);
        }

        /// <summary>
        /// Prüft ob der Filter aktiv ist (mindestens ein Filter gesetzt)
        /// </summary>
        public bool IsActive =>
            StartTime.HasValue ||
            EndTime.HasValue ||
            MinDamage.HasValue ||
            MaxDamage.HasValue ||
            IncludedWeapons.Count > 0 ||
            ExcludedWeapons.Count > 0 ||
            IncludedDamageTypes.Count > 0 ||
            !IncludeCompanionDamage ||
            !IncludeDirectPlayerDamage;

        /// <summary>
        /// Setzt alle Filter zurück
        /// </summary>
        public void Reset()
        {
            StartTime = null;
            EndTime = null;
            MinDamage = null;
            MaxDamage = null;
            IncludedWeapons.Clear();
            ExcludedWeapons.Clear();
            IncludedDamageTypes.Clear();
            IncludeCompanionDamage = true;
            IncludeDirectPlayerDamage = true;
        }

        /// <summary>
        /// Erstellt eine Kopie des aktuellen Filters
        /// </summary>
        public StatisticsFilter Clone()
        {
            return new StatisticsFilter
            {
                StartTime = StartTime,
                EndTime = EndTime,
                MinDamage = MinDamage,
                MaxDamage = MaxDamage,
                IncludedWeapons = new List<string>(IncludedWeapons),
                ExcludedWeapons = new List<string>(ExcludedWeapons),
                IncludedDamageTypes = new List<string>(IncludedDamageTypes),
                IncludeCompanionDamage = IncludeCompanionDamage,
                IncludeDirectPlayerDamage = IncludeDirectPlayerDamage
            };
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            OnPropertyChanged(nameof(IsActive)); // IsActive ändert sich bei jeder Filter-Änderung
            return true;
        }
    }
}
