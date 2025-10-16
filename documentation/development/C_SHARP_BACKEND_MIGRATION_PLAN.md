# C# Backend Migration - Python zu C# Integration

## Überblick

Migration des Python OSCR-Backends zu einer vollständig in C# integrierten Combat-Log-Parsing-Engine mit 1:1 Feature-Parität, asynchroner Verarbeitung und FileSystemWatcher-basiertem Live-Parsing.

## Phase 1: Core Parsing-Logik (C# Klassen)

### 1.1 Datenmodelle erstellen

**Datei:** `frontend/Services/CombatLogParser/Models/ParsedCombatLine.cs`

Erstelle Klasse für geparste Combat-Log-Zeilen:

- `DateTime Timestamp`
- `string OwnerName, OwnerType`
- `string SourceName, SourceType`
- `string TargetName, TargetType`
- `string AbilityName, DamageType, Flags`
- `List<double> DamageValues`

**Datei:** `frontend/Services/CombatLogParser/Models/CombatLineEntity.cs`

Erstelle Klasse für identifizierte Entities:

- `string PlayerHandle, PlayerName`
- `bool IsCompanion`
- `string? CompanionName, CompanionType`

**Datei:** `frontend/Services/CombatLogParser/Models/PlayerStats.cs`

Port von `WorkingPlayerStats` aus Python:

- `string Name`
- `double Dps, TotalDamage, MaxOneHit`
- `int TotalAttacks, Hits, Crits, Deaths`
- `Dictionary<string, CompanionStats> Companions`
- `Dictionary<string, AbilityStats> Abilities`

**Datei:** `frontend/Services/CombatLogParser/Models/CompanionStats.cs`

Port von `WorkingCompanionStats`:

- `string Name, CompanionType`
- `double TotalDamage, MaxHit`
- `int TotalAttacks, Hits, Crits`
- `Dictionary<string, AbilityStats> Abilities`

**Datei:** `frontend/Services/CombatLogParser/Models/AbilityStats.cs`

Port von `WorkingAbilityStats`:

- `string Name`
- `double TotalDamage, MaxHit`
- `int TotalAttacks, Hits, Crits`
- `Dictionary<string, int> DamageTypes`

### 1.2 Combat-Log-Parser Basisklasse

**Datei:** `frontend/Services/CombatLogParser/CombatLogLineParser.cs`

Port von Python `parse_combat_log_line()`:

```csharp
public class CombatLogLineParser
{
    public ParsedCombatLine? ParseLine(string line)
    {
        // Logik aus working_oscr_backend.py Zeile 175-214
        // Split bei ::
        // Timestamp parsen (YY:MM:DD:HH:MM:SS.ms)
        // Data-Parts splitten
        // Return ParsedCombatLine object
    }

    public DateTime? ParseTimestamp(string timeStr)
    {
        // Logik aus working_oscr_backend.py Zeile 88-102
        // Format: 25:10:02:16:24:30.9
        // Split bei ':', parse Year+2000, Month, Day, Hour, Minute, Second
    }

    public string DetermineCombatType(ParsedCombatLine parsed)
    {
        // Logik aus working_oscr_backend.py Zeile 258-312
        // Priorität: Target Type > Source Type
        // S[ = Ground, C[ mit Ground_ = Ground, C[ mit Space_ = Space
    }

    public CombatLineEntity? IdentifySourceEntity(ParsedCombatLine parsed)
    {
        // Logik aus working_oscr_backend.py Zeile 326-399
        // Player vs Companion Unterscheidung
        // Companion-Type: AwayTeam, KitModule, TempControlled
    }

    public string CleanName(string name)
    {
        // Logik aus working_oscr_backend.py Zeile 314-324
        // HTML-Entities dekodieren, HTML-Tags entfernen
        // System.Net.WebUtility.HtmlDecode()
        // Regex.Replace("<[^>]+>", "")
    }
}
```

## Phase 2: Combat-Isolation & Analyse

### 2.1 Combat-Isolator

**Datei:** `frontend/Services/CombatLogParser/CombatIsolator.cs`

Port von Python `isolate_combats()` (Zeile 401-530):

```csharp
public class CombatIsolator
{
    private readonly CombatLogLineParser _parser;
    private readonly int _secondsBetweenCombats = 45;
    private readonly int _combatMinLines = 20;

    public async Task<List<CombatSegment>> IsolateCombatsAsync(
        string logPath, 
        int maxCombats = -1,
        CancellationToken cancellationToken = default)
    {
        // Logik aus working_oscr_backend.py Zeile 401-530
        // 1. Datei lesen (UTF-8-sig, errors='replace')
        // 2. Zeile für Zeile durchgehen
        // 3. Timestamp-Differenz prüfen (> 45s = neuer Combat)
        // 4. Combat-Type-Wechsel prüfen (Space <-> Ground)
        // 5. Combats sammeln (min. 20 Zeilen)
        // 6. Chronologisch älteste zuerst zurückgeben
    }
}

public class CombatSegment
{
    public List<string> Lines { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string CombatType { get; set; }
    public int LineCount { get; set; }
}
```

### 2.2 Combat-Analyzer

**Datei:** `frontend/Services/CombatLogParser/CombatAnalyzer.cs`

Port von Python `_analyze_combat_lines_direct()` (Zeile 650-850):

```csharp
public class CombatAnalyzer
{
    private readonly CombatLogLineParser _parser;

    public async Task<Dictionary<string, PlayerStats>> AnalyzeCombatAsync(
        List<string> combatLines,
        string combatType,
        double duration,
        CancellationToken cancellationToken = default)
    {
        // Logik aus working_oscr_backend.py Zeile 650-850
        // 1. Duration aus ersten/letzten Zeilen berechnen
        // 2. Für jede Zeile:
        //    - Parse Line
        //    - Identify Entity (Player/Companion)
        //    - Extract Damage Value
        //    - Update Player/Companion/Ability Stats
        // 3. DPS berechnen (TotalDamage / Duration)
        // 4. Crit % und Accuracy % berechnen
        // 5. Return Dictionary<PlayerName, PlayerStats>
    }
}
```

## Phase 3: Service-Integration

### 3.1 Native C# Backend Service

**Datei:** `frontend/Services/NativeCombatLogService.cs`

Erstelle neuen Service der `IOSCRBackendService` implementiert:

```csharp
public class NativeCombatLogService : IOSCRBackendService
{
    private readonly CombatLogLineParser _parser;
    private readonly CombatIsolator _isolator;
    private readonly CombatAnalyzer _analyzer;
    private readonly ILogger<NativeCombatLogService> _logger;

    // Implement IOSCRBackendService Interface:
    public async Task<HealthCheckResponse> HealthCheckAsync()
    {
        // Immer erfolgreich (kein externes Backend)
        return new HealthCheckResponse 
        { 
            Success = true, 
            Status = "healthy", 
            Version = "2.0.0-native" 
        };
    }

    public async Task<AvailableCombatsResponse> GetAvailableCombatsAsync(...)
    {
        // 1. IsolateCombatsAsync() aufrufen
        // 2. Combats zu CombatInfo konvertieren
        // 3. AvailableCombatsResponse zurückgeben
    }

    public async Task<CombatAnalysisResponse> AnalyzeSingleCombatAsync(...)
    {
        // 1. IsolateCombatsAsync() -> Combat finden
        // 2. AnalyzeCombatAsync() -> Stats berechnen
        // 3. PlayerStats zu CombatData konvertieren
        // 4. CombatAnalysisResponse zurückgeben
    }

    public async Task<LiveParseResponse> LiveParseAsync(...)
    {
        // 1. Datei ab fromByteOffset lesen
        // 2. Neue Zeilen parsen
        // 3. Combat-Timeout prüfen (45s)
        // 4. Combats isolieren
        // 5. LiveParseResponse mit ActiveCombat zurückgeben
    }

    public async Task<CombatAnalysisResponse> IncrementalCombatUpdateAsync(...)
    {
        // 1. combatLines direkt analysieren
        // 2. AnalyzeCombatAsync() aufrufen
        // 3. CombatAnalysisResponse zurückgeben
    }
}
```

### 3.2 Dependency Injection Update

**Datei:** `frontend/Windows/App/App.xaml.cs`

Service-Registration ändern:

```csharp
// ALT (auskommentieren):
// services.AddSingleton<IOSCRBackendService, OSCRBackendService>();
// services.AddSingleton<HttpBackendManager>();

// NEU:
services.AddSingleton<CombatLogLineParser>();
services.AddSingleton<CombatIsolator>();
services.AddSingleton<CombatAnalyzer>();
services.AddSingleton<IOSCRBackendService, NativeCombatLogService>();
```

## Phase 4: File-Reading Optimierung

### 4.1 Asynchrones File-Reading

**Datei:** `frontend/Services/CombatLogParser/AsyncFileReader.cs`

Erstelle Helper für effizientes asynchrones Lesen:

```csharp
public class AsyncFileReader
{
    public async Task<string[]> ReadAllLinesAsync(
        string path, 
        CancellationToken cancellationToken = default)
    {
        // UTF-8-sig Encoding (BOM-Support)
        // StreamReader mit FileStream
        // Async line-by-line reading
        // Errors='replace' Equivalent: DecoderFallback
    }

    public async Task<(string[] NewLines, long NewOffset)> ReadFromOffsetAsync(
        string path,
        long fromByteOffset,
        CancellationToken cancellationToken = default)
    {
        // FileStream mit Seek(fromByteOffset)
        // Lese bis Ende
        // Return neue Zeilen + neuer Offset
    }
}
```

## Phase 5: Testing & Cleanup

### 5.1 Backend-Dateien entfernen

- `backend/working_oscr_backend.py` → Löschen
- `backend/http_oscr_backend.py` → Löschen
- `backend/OSCRBackend.exe` → Löschen (aus Deploy/Debug)
- `frontend/Services/OSCRBackendService.cs` → Löschen
- `frontend/Services/HttpOSCRBackendService.cs` → Löschen

### 5.2 Build-Scripts aktualisieren

**Datei:** `scripts/create_release.ps1`

Entferne Backend-Build-Schritte:

- Keine Python-Backend-Kopie mehr
- Keine OSCRBackend.exe mehr

**Datei:** `frontend/Build.targets`

Entferne Backend-Copy-Tasks:

- Kein Post-Build Backend-Copy mehr

### 5.3 README.md Updates

**Datei:** `README.md`

Update Technologie-Stack:

- "Native C# Combat-Log-Parsing" statt "Python Backend"
- Keine Python-Dependency mehr
- Schnellere Performance erwähnen

## Technische Details

### Async/Await Pattern

Alle File-I/O und Analyse-Methoden als `async Task<T>`:

- `await File.ReadAllLinesAsync()`
- `await Task.Run()` für CPU-intensive Parsing
- `CancellationToken` Support überall

### Encoding-Handling

```csharp
// UTF-8 mit BOM-Support (wie Python utf-8-sig)
var encoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
using var reader = new StreamReader(path, encoding, detectEncodingFromByteOrderMarks: true);
```

### Error-Handling

```csharp
// Equivalent zu Python errors='replace'
var encoding = Encoding.GetEncoding(
    "utf-8",
    new EncoderReplacementFallback("?"),
    new DecoderReplacementFallback("?"));
```

### Performance-Optimierungen

- `StringBuilder` für String-Manipulationen
- `Dictionary<TKey, TValue>` statt Lookup-Schleifen
- `Parallel.ForEachAsync()` für große Combat-Analysen (optional)
- Memory-Mapped Files für große Log-Dateien (Phase 2, optional)

## Migration-Vorteile

1. **Keine externe Prozesse**: Kein Python-Backend-Start mehr
2. **Schnellere Performance**: Native C# ist schneller als Python
3. **Einfacheres Debugging**: Alles in einem Prozess
4. **Kleinere Distribution**: Keine Python-Runtime oder .exe
5. **Bessere Integration**: Direkter Zugriff auf WPF-Threading
6. **Wartbarkeit**: Ein Language-Stack (C# only)

## Risiken & Mitigation

**Risiko:** Port-Fehler bei komplexer Python-Logik

**Mitigation:** Unit-Tests mit gleichen Test-Daten wie Python

**Risiko:** Performance bei großen Logs

**Mitigation:** Async/await, StreamReader statt ReadAllLines

**Risiko:** Encoding-Probleme (UTF-8, BOM)

**Mitigation:** UTF8Encoding mit BOM-Detection

## Geschätzte Implementierungszeit

- Phase 1: Core Parsing (4-6 Stunden)
- Phase 2: Combat-Isolation & Analyse (6-8 Stunden)
- Phase 3: Service-Integration (2-3 Stunden)
- Phase 4: File-Reading (2-3 Stunden)
- Phase 5: Testing & Cleanup (3-4 Stunden)

**Total: 17-24 Stunden**

## To-dos

- [ ] Erstelle C# Datenmodelle (ParsedCombatLine, Entity, Stats)
- [ ] Implementiere CombatLogLineParser mit Core-Methoden
- [ ] Implementiere CombatIsolator für Combat-Erkennung
- [ ] Implementiere CombatAnalyzer für Stats-Berechnung
- [ ] Erstelle NativeCombatLogService mit IOSCRBackendService Interface
- [ ] Update Dependency Injection in App.xaml.cs
- [ ] Implementiere AsyncFileReader für optimiertes File-Reading
- [ ] Entferne alte Backend-Dateien und Services
- [ ] Update Build-Scripts (kein Backend-Build mehr)
- [ ] Update README.md und Dokumentation

---

**Erstellt:** 2025-01-14  
**Status:** Geplant für spätere Implementierung  
**Maintainer:** STO Damage Meter Team
