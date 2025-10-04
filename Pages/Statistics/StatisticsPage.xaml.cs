using StoDamageMeter.ViewModels;
using StoDamageMeter.Models;
using System.Windows.Controls;
using System.Windows;
using Microsoft.Extensions.Logging;

namespace StoDamageMeter.Pages.Statistics
{
    /// <summary>
    /// Interaction logic for StatisticsPage.xaml
    /// </summary>
    public partial class StatisticsPage : Page
    {
        private readonly ILogger<StatisticsPage> _logger;

        public StatisticsPage(ILogger<StatisticsPage> logger)
        {
            InitializeComponent();
            _logger = logger;
        }

        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DataGrid dataGrid && dataGrid.SelectedItem is WeaponStatistics selectedWeapon)
            {
                _logger.LogInformation("=== Waffen-Statistik ausgewählt ===");
                _logger.LogInformation("Name: {Name}", selectedWeapon.Name);
                _logger.LogInformation("Type: {Type}", selectedWeapon.Type);
                _logger.LogInformation("TotalDamage: {TotalDamage}", selectedWeapon.TotalDamage);
                _logger.LogInformation("AverageDamage: {AverageDamage}", selectedWeapon.AverageDamage);
                _logger.LogInformation("CriticalHits: {CriticalHits}", selectedWeapon.CriticalHits);
                _logger.LogInformation("TotalUses: {TotalUses}", selectedWeapon.TotalUses);
                _logger.LogInformation("CriticalRate: {CriticalRate:F2}%", selectedWeapon.CriticalRate);
                _logger.LogInformation("DamageShare: {DamageShare:F2}%", selectedWeapon.DamageShare);
                _logger.LogInformation("DPS: {DPS:F2}", selectedWeapon.DPS);
                _logger.LogInformation("=== Ende Waffen-Statistik ===");
            }
        }

        private void CompanionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button button && button.Tag is WeaponStatistics companion)
            {
                // Toggle IsExpanded
                companion.IsExpanded = !companion.IsExpanded;

                _logger.LogInformation("=== Companion erweitert/zusammengeklappt ===");
                _logger.LogInformation("Companion: {Name}", companion.Name);
                _logger.LogInformation("IsExpanded: {IsExpanded}", companion.IsExpanded);
                _logger.LogInformation("Anzahl Waffen: {WeaponCount}", companion.Weapons.Count);

                foreach (var weapon in companion.Weapons)
                {
                    _logger.LogInformation("  Waffe: {Name} - {Damage} Schaden - {DPS} DPS",
                        weapon.Name, weapon.TotalDamage, weapon.DPS);
                }
                _logger.LogInformation("=== Ende Companion-Details ===");
            }
        }
    }
}
