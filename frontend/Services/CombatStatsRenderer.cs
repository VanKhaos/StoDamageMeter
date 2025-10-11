using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using StoDamageMeter.Models;

namespace StoDamageMeter.Services
{
    /// <summary>
    /// Service für das Rendern von Combat-Statistiken in der UI
    /// </summary>
    public class CombatStatsRenderer
    {
        private readonly Style _expanderStyle;

        public CombatStatsRenderer(Style expanderStyle)
        {
            _expanderStyle = expanderStyle;
        }

        /// <summary>
        /// Rendert die Combat-Statistiken in das ItemsControl
        /// </summary>
        public void RenderCombatStats(
            ItemsControl targetControl,
            CombatData combatData,
            string sortColumn,
            bool sortAscending)
        {
            targetControl.Items.Clear();

            var sortedPlayers = SortPlayerStatistics(
                combatData.Players,
                sortColumn,
                sortAscending);

            foreach (var player in sortedPlayers)
            {
                var playerContainer = CreatePlayerRow(player);
                targetControl.Items.Add(playerContainer);
            }
        }

        /// <summary>
        /// Erstellt eine Player-Zeile mit allen Abilities und Companions
        /// </summary>
        private StackPanel CreatePlayerRow(PlayerStatistics player)
        {
            var playerContainer = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 2) // ✅ Reduziert von 4 auf 2 für kompakteres Layout
            };

            var playerExpander = new Expander
            {
                IsExpanded = false,
                Background = new SolidColorBrush(Color.FromRgb(26, 26, 26)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                BorderThickness = new Thickness(1),
                Padding = new Thickness(0),
                Style = _expanderStyle
            };

            // Player Header
            var playerHeaderGrid = CreatePlayerHeaderGrid(player, out var expandIcon);
            playerExpander.Header = playerHeaderGrid;

            // Expander Icon ändern
            playerExpander.Expanded += (s, e) => expandIcon.Text = "▼";
            playerExpander.Collapsed += (s, e) => expandIcon.Text = "▶";

            // Player Content: Abilities + Companions
            var playerContentPanel = CreatePlayerContent(player);
            playerExpander.Content = playerContentPanel;

            playerContainer.Children.Add(playerExpander);
            return playerContainer;
        }

        /// <summary>
        /// Erstellt den Player Header Grid
        /// </summary>
        private Grid CreatePlayerHeaderGrid(PlayerStatistics player, out TextBlock expandIcon)
        {
            var headerGrid = new Grid
            {
                Background = new SolidColorBrush(Color.FromRgb(26, 26, 26))
                // ✅ Kein Padding/Margin nötig - wird vom ToggleButton-Padding im Style übernommen
            };

            AddColumnDefinitions(headerGrid);

            // Player Name mit Icon
            var playerNamePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0)
            };

            expandIcon = new TextBlock
            {
                Text = "▶",
                FontSize = 13,
                Foreground = new SolidColorBrush(Color.FromRgb(91, 155, 213)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 8, 0)
            };

            var playerNameText = new TextBlock
            {
                Text = player.Name ?? "Unknown",
                FontSize = 15,
                FontWeight = FontWeights.SemiBold,
                Foreground = new SolidColorBrush(Colors.White),
                VerticalAlignment = VerticalAlignment.Center
            };

            playerNamePanel.Children.Add(expandIcon);
            playerNamePanel.Children.Add(playerNameText);
            Grid.SetColumn(playerNamePanel, 0);
            headerGrid.Children.Add(playerNamePanel);

            // Player Stats
            var playerStats = new[]
            {
                CreateTableCell($"{player.DpsWithCompanions:N0}"),
                CreateTableCell($"{player.TotalDamageWithCompanions:N0}"),
                CreateTableCell($"{player.MaxOneHit:N0}"),
                CreateTableCell($"{player.CritPercent:F1}%"),
                CreateDamageTypeCell("-", "Mixed"), // Player hat mixed types
                CreateTableCell($"{player.Abilities.Sum(a => a.Attacks)}") // Summe aller Player-Abilities
            };

            for (int i = 0; i < playerStats.Length; i++)
            {
                Grid.SetColumn(playerStats[i], i + 1);
                headerGrid.Children.Add(playerStats[i]);
            }

            return headerGrid;
        }

        /// <summary>
        /// Erstellt den Player Content mit Abilities und Companions
        /// </summary>
        private StackPanel CreatePlayerContent(PlayerStatistics player)
        {
            var contentPanel = new StackPanel
            {
                Background = new SolidColorBrush(Color.FromRgb(16, 16, 16)),
                Margin = new Thickness(0)
            };

            // Alle Items (Abilities + Companions) nach Total Damage sortieren
            var allItems = new List<(double totalDamage, bool isCompanion, object item)>();

            foreach (var ability in player.Abilities)
            {
                allItems.Add((ability.TotalDamage, false, ability));
            }

            foreach (var companion in player.Companions)
            {
                allItems.Add((companion.TotalDamage, true, companion));
            }

            var sortedItems = allItems.OrderByDescending(x => x.totalDamage).ToList();

            foreach (var (_, isCompanion, item) in sortedItems)
            {
                if (!isCompanion)
                {
                    var ability = (AbilityStatistics)item;
                    contentPanel.Children.Add(CreateAbilityRow(ability));
                }
                else
                {
                    var companion = (CompanionStatistics)item;
                    contentPanel.Children.Add(CreateCompanionRow(companion));
                }
            }

            return contentPanel;
        }

        /// <summary>
        /// Erstellt eine Ability-Zeile
        /// </summary>
        private Border CreateAbilityRow(AbilityStatistics ability)
        {
            var abilityContainer = new Border
            {
                Background = new SolidColorBrush(Color.FromRgb(16, 16, 16)),
                Margin = new Thickness(0, 1, 0, 0),
                Padding = new Thickness(12, 6, 12, 6) // ✅ Reduziert von 8 auf 6 für kompakteres Layout
            };

            var abilityGrid = new Grid();
            AddColumnDefinitions(abilityGrid);

            // Ability Name mit Spacer
            var abilityNamePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(0)
            };

            var spacer = new TextBlock
            {
                Width = 14,
                VerticalAlignment = VerticalAlignment.Center
            };

            var abilityNameText = new TextBlock
            {
                Text = ability.Name,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(176, 176, 176)),
                VerticalAlignment = VerticalAlignment.Center,
                Opacity = 0.9
            };

            abilityNamePanel.Children.Add(spacer);
            abilityNamePanel.Children.Add(abilityNameText);
            Grid.SetColumn(abilityNamePanel, 0);
            abilityGrid.Children.Add(abilityNamePanel);

            // Ability Stats
            var abilityStats = new[]
            {
                CreateTableCell($"{ability.Dps:N0}", 0.85),
                CreateTableCell($"{ability.TotalDamage:N0}", 0.85),
                CreateTableCell($"{ability.MaxHit:N0}", 0.85),
                CreateTableCell($"{ability.CritPercent:F1}%", 0.85),
                CreateDamageTypeCell(GetDamageTypeIcon(ability.DamageType), ability.DamageType ?? "Unknown", 0.85),
                CreateTableCell($"{ability.Attacks}", 0.85)
            };

            for (int i = 0; i < abilityStats.Length; i++)
            {
                Grid.SetColumn(abilityStats[i], i + 1);
                abilityGrid.Children.Add(abilityStats[i]);
            }

            abilityContainer.Child = abilityGrid;
            return abilityContainer;
        }

        /// <summary>
        /// Erstellt eine Companion-Zeile
        /// </summary>
        private Expander CreateCompanionRow(CompanionStatistics companion)
        {
            var companionExpander = new Expander
            {
                IsExpanded = false,
                Background = new SolidColorBrush(Color.FromRgb(20, 20, 20)),
                BorderBrush = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                BorderThickness = new Thickness(0, 1, 0, 0),
                Padding = new Thickness(0),
                Margin = new Thickness(0, 1, 0, 0),
                Style = _expanderStyle
            };

            // Companion Header
            var companionHeaderGrid = CreateCompanionHeaderGrid(companion, out var expandIcon);
            companionExpander.Header = companionHeaderGrid;

            // Expander Icon ändern
            companionExpander.Expanded += (s, e) => expandIcon.Text = "▼";
            companionExpander.Collapsed += (s, e) => expandIcon.Text = "▶";

            // Companion Abilities
            if (companion.Abilities.Count > 0)
            {
                var companionAbilitiesPanel = CreateCompanionAbilities(companion);
                companionExpander.Content = companionAbilitiesPanel;
            }

            return companionExpander;
        }

        /// <summary>
        /// Erstellt den Companion Header Grid
        /// </summary>
        private Grid CreateCompanionHeaderGrid(CompanionStatistics companion, out TextBlock expandIcon)
        {
            var headerGrid = new Grid
            {
                Background = new SolidColorBrush(Color.FromRgb(20, 20, 20))
            };

            AddColumnDefinitions(headerGrid);

            // Companion Name mit Icon
            var companionNamePanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Margin = new Thickness(32, 0, 0, 0)
            };

            expandIcon = new TextBlock
            {
                Text = "▶",
                FontSize = 12,
                Foreground = new SolidColorBrush(Color.FromRgb(91, 155, 213)),
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 6, 0)
            };

            var companionNameText = new TextBlock
            {
                Text = companion.Name ?? "Unknown",
                FontSize = 14,
                FontWeight = FontWeights.Normal,
                Foreground = new SolidColorBrush(Color.FromRgb(176, 176, 176)),
                VerticalAlignment = VerticalAlignment.Center,
                Opacity = 0.95
            };

            companionNamePanel.Children.Add(expandIcon);
            companionNamePanel.Children.Add(companionNameText);
            
            // Type Badge hinzufügen (falls vorhanden)
            var typeBadge = CreateCompanionTypeBadge(companion.Type);
            if (typeBadge != null)
            {
                companionNamePanel.Children.Add(typeBadge);
            }
            
            Grid.SetColumn(companionNamePanel, 0);
            headerGrid.Children.Add(companionNamePanel);

            // Companion Stats
            var companionStats = new[]
            {
                CreateTableCell($"{companion.Dps:N0}", 0.8),
                CreateTableCell($"{companion.TotalDamage:N0}", 0.8),
                CreateTableCell($"{companion.MaxOneHit:N0}", 0.8),
                CreateTableCell($"{companion.CritPercent:F1}%", 0.8),
                CreateDamageTypeCell("-", "Mixed", 0.8), // Companion hat mixed types
                CreateTableCell($"{companion.Abilities.Sum(a => a.Attacks)}", 0.8) // Summe aller Companion-Abilities
            };

            for (int i = 0; i < companionStats.Length; i++)
            {
                Grid.SetColumn(companionStats[i], i + 1);
                headerGrid.Children.Add(companionStats[i]);
            }

            return headerGrid;
        }

        /// <summary>
        /// Erstellt ein Type-Badge für Companions
        /// </summary>
        private Border? CreateCompanionTypeBadge(string? companionType)
        {
            if (string.IsNullOrEmpty(companionType))
                return null;

            string icon;
            string tooltip;
            Color backgroundColor;

            switch (companionType)
            {
                case "AwayTeam":
                    icon = "👥";
                    tooltip = "Away Team Member";
                    backgroundColor = Color.FromRgb(74, 158, 255); // Blau
                    break;
                case "KitModule":
                    icon = "🔧";
                    tooltip = "Kit Module";
                    backgroundColor = Color.FromRgb(255, 165, 0); // Orange
                    break;
                case "TempControlled":
                    icon = "⚡";
                    tooltip = "Temporarily Controlled";
                    backgroundColor = Color.FromRgb(155, 89, 182); // Lila
                    break;
                default:
                    return null;
            }

            var badge = new Border
            {
                Background = new SolidColorBrush(backgroundColor),
                CornerRadius = new CornerRadius(4),
                Padding = new Thickness(4, 2, 4, 2),
                Margin = new Thickness(6, 0, 0, 0),
                VerticalAlignment = VerticalAlignment.Center,
                ToolTip = tooltip
            };

            var iconText = new TextBlock
            {
                Text = icon,
                FontSize = 12,
                Foreground = Brushes.White,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = System.Windows.HorizontalAlignment.Center
            };

            badge.Child = iconText;
            return badge;
        }

        /// <summary>
        /// Erstellt die Companion Abilities
        /// </summary>
        private StackPanel CreateCompanionAbilities(CompanionStatistics companion)
        {
            var abilitiesPanel = new StackPanel
            {
                Background = new SolidColorBrush(Color.FromRgb(12, 12, 12)),
                Margin = new Thickness(0)
            };

            var sortedAbilities = companion.Abilities
                .OrderByDescending(a => a.Dps)
                .ToList();

            foreach (var ability in sortedAbilities)
            {
                var abilityContainer = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(12, 12, 12)),
                    Margin = new Thickness(0, 1, 0, 0),
                    Padding = new Thickness(12, 8, 12, 8)
                };

                var abilityGrid = new Grid();
                AddColumnDefinitions(abilityGrid);

                // Ability Name mit Spacer
                var abilityNamePanel = new StackPanel
                {
                    Orientation = Orientation.Horizontal,
                    Margin = new Thickness(32, 0, 0, 0)
                };

                var spacer = new TextBlock
                {
                    Width = 11,
                    VerticalAlignment = VerticalAlignment.Center
                };

                var abilityNameText = new TextBlock
                {
                    Text = ability.Name,
                    FontSize = 13,
                    Foreground = new SolidColorBrush(Color.FromRgb(156, 156, 156)),
                    VerticalAlignment = VerticalAlignment.Center,
                    Opacity = 0.85
                };

                abilityNamePanel.Children.Add(spacer);
                abilityNamePanel.Children.Add(abilityNameText);
                Grid.SetColumn(abilityNamePanel, 0);
                abilityGrid.Children.Add(abilityNamePanel);

                // Ability Stats
                var abilityStats = new[]
                {
                    CreateTableCell($"{ability.Dps:N0}", 0.75),
                    CreateTableCell($"{ability.TotalDamage:N0}", 0.75),
                    CreateTableCell($"{ability.MaxHit:N0}", 0.75),
                    CreateTableCell($"{ability.CritPercent:F1}%", 0.75),
                    CreateDamageTypeCell(GetDamageTypeIcon(ability.DamageType), ability.DamageType ?? "Unknown", 0.75),
                    CreateTableCell($"{ability.Attacks}", 0.75)
                };

                for (int i = 0; i < abilityStats.Length; i++)
                {
                    Grid.SetColumn(abilityStats[i], i + 1);
                    abilityGrid.Children.Add(abilityStats[i]);
                }

                abilityContainer.Child = abilityGrid;
                abilitiesPanel.Children.Add(abilityContainer);
            }

            return abilitiesPanel;
        }

        /// <summary>
        /// Fügt Standard-Spalten-Definitionen hinzu
        /// </summary>
        private void AddColumnDefinitions(Grid grid)
        {
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.8, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.8, GridUnitType.Star) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(70, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60, GridUnitType.Pixel) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(0.5, GridUnitType.Star) });
        }
        
        /// <summary>
        /// Gibt das Icon für einen Damage-Type zurück
        /// </summary>
        private string GetDamageTypeIcon(string? damageType)
        {
            if (string.IsNullOrEmpty(damageType))
                return "";
            
            return damageType switch
            {
                "Physical" => "⚔",
                "Energy" => "⚡",
                "Kinetic" => "●",
                "Exotic" => "✦",
                "Plasma" => "▲",
                "Disruptor" => "◆",
                "Phaser" => "◉",
                "Tetryon" => "◈",
                "Polaron" => "◘",
                "Proton" => "◐",
                "Antiproton" => "◆",
                "AntiProton" => "◆",
                "Radiation" => "☢",
                "Electrical" => "⚡",
                "Toxic" => "☠",
                "Psionic" => "◉",
                "Shield" => "◙",
                _ => ""  // Leerer String für unbekannte Types
            };
        }
        
        /// <summary>
        /// Gibt die Farbe für einen Damage-Type zurück
        /// </summary>
        private Color GetDamageTypeColor(string? damageType)
        {
            if (string.IsNullOrEmpty(damageType))
                return Color.FromRgb(176, 176, 176);
            
            return damageType switch
            {
                "Physical" => Color.FromRgb(192, 192, 192),      // Silber
                "Energy" => Color.FromRgb(255, 215, 0),          // Gold
                "Kinetic" => Color.FromRgb(139, 69, 19),         // Braun
                "Exotic" => Color.FromRgb(138, 43, 226),         // Violett
                "Plasma" => Color.FromRgb(255, 69, 0),           // Orange-Rot
                "Disruptor" => Color.FromRgb(0, 255, 127),       // Grün
                "Phaser" => Color.FromRgb(255, 140, 0),          // Orange
                "Tetryon" => Color.FromRgb(0, 191, 255),         // Hellblau
                "Polaron" => Color.FromRgb(147, 112, 219),       // Lila
                "Proton" => Color.FromRgb(100, 149, 237),        // Kornblumenblau
                "Antiproton" => Color.FromRgb(220, 20, 60),      // Rot
                "AntiProton" => Color.FromRgb(220, 20, 60),      // Rot
                "Radiation" => Color.FromRgb(173, 255, 47),      // Gelbgrün
                "Electrical" => Color.FromRgb(255, 255, 0),      // Gelb
                "Toxic" => Color.FromRgb(50, 205, 50),           // Giftgrün
                "Psionic" => Color.FromRgb(255, 0, 255),         // Magenta
                "Shield" => Color.FromRgb(135, 206, 250),        // Hellblau
                _ => Color.FromRgb(176, 176, 176)                // Grau für Unbekannt
            };
        }

        /// <summary>
        /// Erstellt eine Tabellen-Zelle
        /// </summary>
        private Border CreateTableCell(string text, double opacity = 1.0)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                FontSize = 14,
                Foreground = new SolidColorBrush(Color.FromRgb(176, 176, 176)),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(8, 6, 8, 6),
                Opacity = opacity,
                FontWeight = FontWeights.Normal
            };

            return new Border
            {
                Child = textBlock,
                BorderBrush = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                BorderThickness = new Thickness(1, 0, 0, 0)
            };
        }
        
        /// <summary>
        /// Erstellt eine Damage-Type-Zelle mit Icon und Tooltip
        /// </summary>
        private Border CreateDamageTypeCell(string icon, string? tooltipText, double opacity = 1.0)
        {
            var color = GetDamageTypeColor(tooltipText);
            
            var textBlock = new TextBlock
            {
                Text = icon,
                FontSize = 16,
                FontWeight = FontWeights.Bold,
                Foreground = new SolidColorBrush(color),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Padding = new Thickness(4, 6, 4, 6),
                Opacity = opacity,
                ToolTip = string.IsNullOrEmpty(tooltipText) ? null : tooltipText
            };
            
            return new Border
            {
                Child = textBlock,
                BorderBrush = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
                BorderThickness = new Thickness(1, 0, 0, 0)
            };
        }

        /// <summary>
        /// Sortiert Player-Statistiken
        /// </summary>
        private List<PlayerStatistics> SortPlayerStatistics(
            IEnumerable<PlayerStatistics> players,
            string sortColumn,
            bool ascending)
        {
            IOrderedEnumerable<PlayerStatistics> orderedPlayers = sortColumn switch
            {
                "DpsWithCompanions" => ascending
                    ? players.OrderBy(p => p.DpsWithCompanions)
                    : players.OrderByDescending(p => p.DpsWithCompanions),

                "TotalDamageWithCompanions" => ascending
                    ? players.OrderBy(p => p.TotalDamageWithCompanions)
                    : players.OrderByDescending(p => p.TotalDamageWithCompanions),

                "MaxOneHit" => ascending
                    ? players.OrderBy(p => p.MaxOneHit)
                    : players.OrderByDescending(p => p.MaxOneHit),

                "CritPercent" => ascending
                    ? players.OrderBy(p => p.CritPercent)
                    : players.OrderByDescending(p => p.CritPercent),

                "AccuracyPercent" => ascending
                    ? players.OrderBy(p => p.AccuracyPercent)
                    : players.OrderByDescending(p => p.AccuracyPercent),

                "Attacks" => ascending
                    ? players.OrderBy(p => p.Abilities.Sum(a => a.Attacks))
                    : players.OrderByDescending(p => p.Abilities.Sum(a => a.Attacks)),

                _ => ascending
                    ? players.OrderBy(p => p.DpsWithCompanions)
                    : players.OrderByDescending(p => p.DpsWithCompanions)
            };

            return orderedPlayers.ToList();
        }
    }
}

