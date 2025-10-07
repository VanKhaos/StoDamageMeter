# STO Damage Meter - UI Implementierungsplan

## Übersicht
Dieses Dokument beschreibt die geplante Benutzeroberfläche für die STO Damage Meter Anwendung, basierend auf dem Design der "Open Source Combatlog Reader" Anwendung.

## Design-Prinzipien
- **Dark Theme**: Dunkles Design mit orangen Akzenten für aktive Elemente
- **Zwei-Panel-Layout**: Linke Sidebar für Navigation, rechter Bereich für Daten
- **Konsistente Farbgebung**: Weiß für Text, Orange für aktive/ausgewählte Elemente
- **Moderne Icons**: Einfache, monochrome Icons

## Hauptfenster-Struktur

### 1. Header-Bar
```
┌─────────────────────────────────────────────────────────────────┐
│ [A] STO Damage Meter                    [Overview][Analysis][Settings] [⚡] │
└─────────────────────────────────────────────────────────────────┘
```

**Elemente:**
- **Logo**: Kleines 'A' Icon links
- **Titel**: "STO Damage Meter" (weißer Text)
- **Navigation Tabs**: 
  - "Overview" (aktiv, oranger Text, orange Unterstreichung)
  - "Analysis" (weißer Text)
  - "Settings" (weißer Text)
- **Icon**: Schaltkreis/Chip Icon rechts

### 2. Hauptbereich (Zwei-Panel-Layout)

#### Linke Sidebar (25-30% Breite)
```
┌─────────────────────┐
│ STO Combatlog:      │
│ [📁] combatlog.log  │
│ [Browse...] [Default] [Analyze] │
│                     │
│ Combat List:        │
│ ▶ Combat 20:45:24   │
│   2025-10-06        │
│ ▶ Combat 18:37:55   │
│   2025-09-28        │
│ ▶ Combat 18:28:37   │
│   2025-09-28        │
│                     │
│ Log Duration: 462.9s│
│ Player Duration: 462.7s │
│ [Map Detection Details] │
└─────────────────────┘
```

**Elemente:**
- **Log-Pfad**: Eingabefeld mit Ordner-Icon
- **Aktions-Buttons**: Browse, Default, Analyze
- **Combat-Liste**: Scrollbare Liste mit Combat-Einträgen
  - Format: [Typ] [Zeit] [Datum]
  - Aktiver Eintrag: Orange linke Umrandung
- **Footer-Info**: Log-Dauer, Player-Dauer, Map Detection Link

#### Rechter Datenbereich (70-75% Breite)
```
┌─────────────────────────────────────────────────────────────┐
│ [Damage Out] [Damage Taken] [Heals Out] [Heals In] [Selection ▼] [⚏] │
│                                                             │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │                    DPS Graph                            │ │
│ │                                                         │ │
│ │ 0 ┌─────────────────────────────────────────────────┐ 0 │ │
│ │   │                                                 │   │ │
│ │   │                                                 │   │ │
│ │   │                                                 │   │ │
│ │   └─────────────────────────────────────────────────┘   │ │
│ │ 0s                                                   462s │ │
│ └─────────────────────────────────────────────────────────┘ │
│                                                             │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ DPS │ Total Damage │ Debuff │ Max Hit │ Crit │ Acc │ ... │ │
│ ├─────────────────────────────────────────────────────────┤ │
│ │ ▶ Player                                               │ │
│ │   Van Khaos@vankhaos#2007                              │ │
│ │   1.18 │ 70.57 │ 0% │ 7.84 │ 0% │ 100% │ ... │        │ │
│ │   ▶ Ba'ul Antiproton Array - Overload III              │ │
│ │     13.10 │ 5.97 │ 38.66% │ 71.34 │ 39.51% │ ... │    │ │
│ │   ▶ Ba'ul Antiproton Array                             │ │
│ │     6.18 │ 2.82 │ 27.81% │ 14.45 │ 41.07% │ ... │    │ │
│ │ ▶ NPC                                                  │ │
│ └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

**Elemente:**
- **Filter-Bar**: Damage Out (aktiv, orange), Damage Taken, Heals Out, Heals In
- **Auswahl-Dropdown**: "Selection" mit Pfeil
- **DPS-Graph**: Zeitreihen-Diagramm für Combat-Daten
- **Daten-Tabelle**: 
  - Spalten: DPS, Total Damage, Debuff, Max Hit, Crit Chance, Accuracy, etc.
  - Kategorien: Player (erweiterbar), NPC (erweiterbar)
  - Player-Details: Name, Statistiken
  - Ability-Details: Erweiterbare Untereinträge für Fähigkeiten

## Technische Implementierung

### 1. WPF-Struktur
```xml
<Window>
  <Grid>
    <!-- Header -->
    <Grid.RowDefinitions>
      <RowDefinition Height="Auto"/>
      <RowDefinition Height="*"/>
    </Grid.RowDefinitions>
    
    <!-- Header Bar -->
    <Border Grid.Row="0" Background="#2D2D30">
      <Grid>
        <Grid.ColumnDefinitions>
          <ColumnDefinition Width="Auto"/>
          <ColumnDefinition Width="*"/>
          <ColumnDefinition Width="Auto"/>
        </Grid.ColumnDefinitions>
        
        <!-- Logo & Title -->
        <StackPanel Grid.Column="0" Orientation="Horizontal">
          <Image Source="logo.png" Width="24" Height="24"/>
          <TextBlock Text="STO Damage Meter" Foreground="White"/>
        </StackPanel>
        
        <!-- Navigation Tabs -->
        <TabControl Grid.Column="1">
          <TabItem Header="Overview" Style="{StaticResource ActiveTabStyle}"/>
          <TabItem Header="Analysis"/>
          <TabItem Header="Settings"/>
        </TabControl>
        
        <!-- Right Icon -->
        <Image Grid.Column="2" Source="chip.png" Width="24" Height="24"/>
      </Grid>
    </Border>
    
    <!-- Main Content -->
    <Grid Grid.Row="1">
      <Grid.ColumnDefinitions>
        <ColumnDefinition Width="300"/>
        <ColumnDefinition Width="*"/>
      </Grid.ColumnDefinitions>
      
      <!-- Left Sidebar -->
      <Border Grid.Column="0" Background="#3C3C3C">
        <!-- Sidebar Content -->
      </Border>
      
      <!-- Right Data Area -->
      <Border Grid.Column="1" Background="#1E1E1E">
        <!-- Data Content -->
      </Border>
    </Grid>
  </Grid>
</Window>
```

### 2. Farb-Schema
```csharp
public static class AppColors
{
    // Haupt-Hintergründe
    public static readonly Brush Background = new SolidColorBrush(Color.FromRgb(30, 30, 30));      // #1E1E1E
    public static readonly Brush SidebarBackground = new SolidColorBrush(Color.FromRgb(60, 60, 60)); // #3C3C3C
    public static readonly Brush HeaderBackground = new SolidColorBrush(Color.FromRgb(45, 45, 48));  // #2D2D30
    
    // Text-Farben
    public static readonly Brush TextForeground = new SolidColorBrush(Colors.White);                // #FFFFFF
    public static readonly Brush ActiveAccent = new SolidColorBrush(Color.FromRgb(255, 107, 53));   // #FF6B35 (Orange)
    
    // Button-Farben
    public static readonly Brush ButtonBackground = new SolidColorBrush(Color.FromRgb(60, 60, 60));  // #3C3C3C
    public static readonly Brush ButtonHover = new SolidColorBrush(Color.FromRgb(80, 80, 80));       // #505050
    public static readonly Brush ActiveButton = new SolidColorBrush(Color.FromRgb(255, 107, 53));    // #FF6B35 (Orange)
    
    // Border-Farben
    public static readonly Brush BorderColor = new SolidColorBrush(Color.FromRgb(80, 80, 80));       // #505050
    public static readonly Brush ActiveBorder = new SolidColorBrush(Color.FromRgb(255, 107, 53));    // #FF6B35 (Orange)
}
```

### 3. Styling
```xml
<Style x:Key="ActiveTabStyle" TargetType="TabItem">
  <Setter Property="Foreground" Value="#FF6B35"/>
  <Setter Property="BorderBrush" Value="#FF6B35"/>
  <Setter Property="BorderThickness" Value="0,0,0,2"/>
</Style>

<Style x:Key="CombatListItemStyle" TargetType="ListBoxItem">
  <Setter Property="Foreground" Value="White"/>
  <Setter Property="Background" Value="Transparent"/>
  <Style.Triggers>
    <Trigger Property="IsSelected" Value="True">
      <Setter Property="BorderBrush" Value="#FF6B35"/>
      <Setter Property="BorderThickness" Value="2,0,0,0"/>
    </Trigger>
  </Style.Triggers>
</Style>

<Style x:Key="FilterButtonStyle" TargetType="Button">
  <Setter Property="Background" Value="{StaticResource ButtonBackground}"/>
  <Setter Property="Foreground" Value="White"/>
  <Setter Property="Padding" Value="12,6"/>
  <Style.Triggers>
    <Trigger Property="IsPressed" Value="True">
      <Setter Property="Background" Value="#FF6B35"/>
    </Trigger>
  </Style.Triggers>
</Style>
```

## Implementierungsphasen

### Phase 1: Grundstruktur
1. **Hauptfenster-Layout**: Grid mit Header und Hauptbereich
2. **Header-Bar**: Logo, Titel, Navigation Tabs
3. **Zwei-Panel-Layout**: Sidebar und Datenbereich
4. **Grundlegendes Styling**: Dark Theme, Farben

### Phase 2: Linke Sidebar
1. **Log-Pfad-Eingabe**: TextBox mit Browse-Button
2. **Aktions-Buttons**: Browse, Default, Analyze
3. **Combat-Liste**: ListBox mit Combat-Einträgen
4. **Footer-Informationen**: Log-Dauer, Player-Dauer

### Phase 3: Rechter Datenbereich
1. **Filter-Bar**: Buttons für Damage Out, Damage Taken, etc.
2. **Auswahl-Dropdown**: Selection-Dropdown
3. **DPS-Graph**: OxyPlot-Integration für Zeitreihen-Diagramm
4. **Daten-Tabelle**: DataGrid mit Player- und NPC-Daten

### Phase 4: Daten-Integration
1. **Backend-Anbindung**: Verbindung zu OSCRBackendService
2. **Daten-Binding**: MVVM-Pattern für Datenanzeige
3. **Echtzeit-Updates**: Progress-Events für Analyse
4. **Fehlerbehandlung**: User-freundliche Fehlermeldungen

### Phase 5: Erweiterte Features
1. **Erweiterbare Listen**: TreeView für Player-Abilities
2. **Tooltips**: Hilfetexte für Statistiken
3. **Kontext-Menüs**: Rechtsklick-Aktionen
4. **Export-Funktionen**: CSV, JSON Export

## Datenmodelle

### CombatListItem
```csharp
public class CombatListItem
{
    public string Type { get; set; }        // "Combat", "Operation Wolf"
    public string Time { get; set; }        // "20:45:24"
    public string Date { get; set; }        // "2025-10-06"
    public string Status { get; set; }      // "Normal"
    public bool IsSelected { get; set; }
}
```

### PlayerStatistics
```csharp
public class PlayerStatistics
{
    public string Name { get; set; }           // "Van Khaos@vankhaos#2007"
    public double DPS { get; set; }            // 1.18
    public double TotalDamage { get; set; }    // 70.57
    public double Debuff { get; set; }         // 0%
    public double MaxHit { get; set; }         // 7.84
    public double CritChance { get; set; }     // 0%
    public double Accuracy { get; set; }       // 100%
    public List<AbilityStatistics> Abilities { get; set; }
}
```

### AbilityStatistics
```csharp
public class AbilityStatistics
{
    public string Name { get; set; }           // "Ba'ul Antiproton Array - Overload III"
    public double DPS { get; set; }            // 13.10
    public double TotalDamage { get; set; }    // 5.97
    public double Debuff { get; set; }         // 38.66%
    public double MaxHit { get; set; }         // 71.34
    public double CritChance { get; set; }     // 39.51%
    public double Accuracy { get; set; }       // 97.63%
}
```

## Nächste Schritte

1. **Review des Plans**: Überprüfung der UI-Struktur und -Designs
2. **Priorisierung**: Welche Phasen zuerst implementieren
3. **Technische Details**: Spezifische WPF-Controls und -Patterns
4. **Daten-Integration**: Anpassung der bestehenden Backend-Services
5. **Testing**: UI-Tests und Benutzerfreundlichkeit

## Anmerkungen

- **Responsive Design**: Layout sollte bei verschiedenen Fenstergrößen funktionieren
- **Accessibility**: Tastaturnavigation und Screen-Reader-Unterstützung
- **Performance**: Effiziente Datenanzeige bei großen Combat-Logs
- **Erweiterbarkeit**: Modulare Struktur für zukünftige Features
