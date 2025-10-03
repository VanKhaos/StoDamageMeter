# STO Damage Meter - Detaillierte Projektanalyse

## 1. Projektübersicht

**STO Damage Meter** ist eine Windows Desktop-Anwendung, die als Damage Meter für das MMORPG "Star Trek Online" (STO) entwickelt wurde. Die Anwendung analysiert Combatlog-Dateien des Spiels und stellt detaillierte Schadensstatistiken in einer modernen WPF-Benutzeroberfläche dar.

### 1.1 Technische Spezifikationen
- **Framework**: .NET 9.0 mit WPF (Windows Presentation Foundation)
- **Architektur**: MVVM (Model-View-ViewModel) Pattern
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection
- **UI Framework**: WPF-UI (moderne UI-Komponenten)
- **Logging**: Microsoft.Extensions.Logging
- **JSON-Serialisierung**: Newtonsoft.Json

## 2. Funktionalität und Zweck

### 2.1 Hauptfunktionen
1. **Combatlog-Parsing**: Analyse von STO-Combatlog-Dateien mit komplexer Regex-basierter Extraktion
2. **Live-Monitoring**: Echtzeitüberwachung von Combatlog-Dateien während des Spiels
3. **Schadensstatistiken**: Berechnung und Darstellung von DPS, kritischen Treffern, Schadensarten
4. **Spieler-Tracking**: Identifikation und Verfolgung einzelner Spieler basierend auf P-Tags
5. **Kampfumgebung-Erkennung**: Unterscheidung zwischen Boden- und Raumkampf

### 2.2 Zielgruppe
- Star Trek Online Spieler
- Raid-Gruppen und Gilden
- Performance-Analysten
- Spieler, die ihre Builds optimieren möchten

## 3. Architektur-Analyse

### 3.1 Projektstruktur

```
StoDamageMeter/
├── Components/          # Wiederverwendbare UI-Komponenten
├── Converters/          # WPF-Value-Converter
├── Models/              # Datenmodelle und Enums
├── Pages/               # Hauptseiten der Anwendung
├── Services/            # Business-Logic und Datenverarbeitung
├── Styles/              # UI-Styling und Themes
├── ViewModels/          # MVVM ViewModels
└── Views/               # WPF-Views und MainWindow
```

### 3.2 Design Patterns

#### 3.2.1 MVVM (Model-View-ViewModel)
- **Models**: `CombatLogEntry`, `PlayerInfo`, `EntityInfo`, `CombatLogStatistics`
- **ViewModels**: `MainViewModel` mit ObservableObject und RelayCommand
- **Views**: XAML-basierte UI mit Data Binding

#### 3.2.2 Dependency Injection
- Service-Registrierung in `App.xaml.cs`
- Interface-basierte Abstraktion (`ICombatLogService`, `ICombatLogParser`)
- Singleton-Pattern für Services

#### 3.2.3 Repository Pattern
- `CombatLogService` als Hauptservice für Datenverarbeitung
- `CombatLogParser` für spezifische Parsing-Logik

### 3.3 Datenfluss

```
Combatlog-Datei → CombatLogParser → CombatLogEntry → CombatLogService → CombatLogStatistics → MainViewModel → UI
```

## 4. Kernkomponenten-Analyse

### 4.1 CombatLogParser
**Zweck**: Parsing von STO-Combatlog-Zeilen in strukturierte Daten

**Kernfunktionen**:
- Regex-basierte Extraktion von Spieler-IDs (`P[ID@AccountID Name@Handle#Discriminator]`)
- Entity-Typ-Erkennung (Player, Companion, Enemy)
- Timestamp-Parsing mit mehreren Format-Unterstützungen
- Schadenswert-Extraktion und -Validierung

**Technische Details**:
```csharp
// Regex-Pattern für Player-IDs
private static readonly Regex PlayerIdPattern = 
    new(@"P\[(\d+)@(\d+)\s+([^@]+)@([^#]+)#(\d+)\]", RegexOptions.Compiled);
```

### 4.2 CombatLogService
**Zweck**: Hauptservice für Combatlog-Verarbeitung und Live-Monitoring

**Kernfunktionen**:
- Datei-basierte Verarbeitung mit `File.ReadAllLinesAsync()`
- FileSystemWatcher für Live-Updates
- Statistik-Berechnung (DPS, kritische Rate, Schadensverteilung)
- Event-basierte Kommunikation mit UI

**Performance-Optimierungen**:
- Asynchrone Dateiverarbeitung
- Lazy Loading von Statistiken
- Effiziente LINQ-Aggregationen

### 4.3 Datenmodelle

#### 4.3.1 CombatLogEntry
```csharp
public class CombatLogEntry
{
    public DateTime Timestamp { get; set; }
    public PlayerInfo PlayerInfo { get; set; }
    public EntityInfo SourceEntity { get; set; }
    public EntityInfo TargetEntity { get; set; }
    public string AttackName { get; set; }
    public string DamageType { get; set; }
    public EventType EventType { get; set; }
    public double RawDamage { get; set; }
    public double DamageWithResistance { get; set; }
}
```

#### 4.3.2 CombatLogStatistics
```csharp
public class CombatLogStatistics
{
    public int TotalEntries { get; set; }
    public double TotalDamage { get; set; }
    public double DPS { get; set; }
    public TimeSpan CombatDuration { get; set; }
    public Dictionary<string, double> DamageByType { get; set; }
    public Dictionary<string, double> DamageByAttack { get; set; }
    public double CriticalRate { get; set; }
    public int PlayerCount { get; set; }
}
```

## 5. UI/UX-Analyse

### 5.1 Design-System
**Modern Web Design** mit dunklem Theme:
- **Farbpalette**: Slate-basierte Farben (#0F172A, #1E293B, #334155)
- **Typography**: Segoe UI mit verschiedenen Gewichtungen
- **Komponenten**: Card-basierte Layouts mit abgerundeten Ecken
- **Icons**: WPF-UI Symbol-Icons für konsistente Darstellung

### 5.2 Navigation
**Sidebar-Navigation** mit 5 Hauptbereichen:
1. **Dashboard**: Übersicht und Dateiauswahl
2. **Live Tracking**: Echtzeit-Monitoring
3. **Statistics**: Detaillierte Statistiken
4. **Configuration**: Einstellungen
5. **About**: Informationen

### 5.3 Responsive Design
- **Minimale Größe**: 1000x600 Pixel
- **Standard-Größe**: 1400x800 Pixel
- **Flexible Layouts**: Grid-basierte Responsive-Designs

## 6. Combatlog-Format-Analyse

### 6.1 Datenstruktur
Basierend auf der `COMBATLOG_ANALYSIS.md`:

```
Timestamp::PlayerName,PlayerTag,SourceEntity,SourceTag,TargetEntity,TargetTag,Attack,AbilityID,DamageType,EventType,RawDamage,DamageWithResistance
```

**Beispiel**:
```
25:10:02:16:24:30.9::Van Khaos,P[12698228@19236104 Van Khaos@vankhaos#2007],Elite-Allianz-Jäger-Staffel,C[202 Space_Alliance_Carrier_Launch_Squadron_Fighters_3],Schwerer Mogai-Warbird,C[140 Space_Romulan_Escort],Antiprotonen-Impulskanone,Pn.Kydw9k,AntiProton,,106.049,964.176
```

### 6.2 Entity-Klassifizierung
- **P[ID]**: Echte Spieler (nur diese werden für Statistiken verwendet)
- **C[ID]**: Gegner/Entities
- **S[ID]**: Companions/Begleiter

### 6.3 Kampfumgebung-Erkennung
- **Ground_**: Bodenkampf (Kitmodule)
- **Space_**: Raumkampf (Hangarschiffe)

## 7. Technische Stärken

### 7.1 Code-Qualität
- **SOLID-Prinzipien**: Interface-basierte Abstraktion
- **Async/Await**: Moderne asynchrone Programmierung
- **Error Handling**: Umfassende Exception-Behandlung
- **Logging**: Strukturiertes Logging mit Microsoft.Extensions.Logging

### 7.2 Performance
- **Regex-Compilation**: Kompilierte Regex-Pattern für bessere Performance
- **LINQ-Optimierung**: Effiziente Datenaggregation
- **Memory Management**: IDisposable-Implementierung für Services

### 7.3 Wartbarkeit
- **Separation of Concerns**: Klare Trennung von UI, Business-Logic und Daten
- **Dependency Injection**: Lose Kopplung zwischen Komponenten
- **Modularer Aufbau**: Wiederverwendbare Komponenten

## 8. Identifizierte Verbesserungsmöglichkeiten

### 8.1 Performance-Optimierungen
1. **Streaming-Parser**: Für sehr große Combatlog-Dateien
2. **Caching**: Zwischenspeicherung von Statistiken
3. **Background Processing**: UI-blocking Operationen in Background-Threads

### 8.2 Funktionalitäts-Erweiterungen
1. **Export-Funktionen**: CSV/JSON-Export von Statistiken
2. **Vergleichsmodus**: Vergleich zwischen verschiedenen Kampf-Sessions
3. **Filtering**: Erweiterte Filteroptionen (Zeitraum, Spieler, Schadensarten)
4. **Visualisierungen**: Charts und Grafiken für bessere Datenvisualisierung

### 8.3 Code-Verbesserungen
1. **Unit Tests**: Fehlende Test-Abdeckung
2. **Configuration**: Externe Konfigurationsdateien
3. **Internationalization**: Mehrsprachige Unterstützung
4. **Error Recovery**: Robustere Fehlerbehandlung bei korrupten Dateien

## 9. Deployment und Distribution

### 9.1 Build-Konfiguration
- **Target Framework**: .NET 9.0-windows
- **Output Type**: WinExe (Windows-Executable)
- **Dependencies**: NuGet-Pakete mit spezifischen Versionen

### 9.2 Abhängigkeiten
```xml
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.4.0" />
<PackageReference Include="Microsoft.Extensions.DependencyInjection" Version="9.0.9" />
<PackageReference Include="Microsoft.Extensions.Logging" Version="9.0.9" />
<PackageReference Include="Newtonsoft.Json" Version="13.0.4" />
<PackageReference Include="WPF-UI" Version="4.0.3" />
```

## 10. Fazit

Das **STO Damage Meter** ist eine gut strukturierte, moderne WPF-Anwendung, die komplexe Combatlog-Analysen in einer benutzerfreundlichen Oberfläche präsentiert. Die Architektur folgt bewährten Patterns und zeigt professionelle Softwareentwicklungspraktiken.

**Stärken**:
- Saubere MVVM-Architektur
- Moderne UI mit WPF-UI
- Robuste Datenverarbeitung
- Live-Monitoring-Funktionalität
- Umfassende Statistik-Berechnungen

**Entwicklungsmöglichkeiten**:
- Erweiterte Visualisierungen
- Performance-Optimierungen für große Dateien
- Export- und Vergleichsfunktionen
- Test-Abdeckung und Dokumentation

Das Projekt zeigt hohe technische Kompetenz und ist bereit für produktive Nutzung durch Star Trek Online-Spieler.
