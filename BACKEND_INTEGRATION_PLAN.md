# Backend-Integration Entwicklungsplan

## Übersicht
Dieser Plan beschreibt die Schritte zur Integration des Python-Backends (OSCR - Open Source Combatlog Reader) mit dem WPF-Frontend, ohne dass der Endanwender Python installieren muss.

## Aktuelle Situation
- **Backend**: Python-basierte OSCR-Bibliothek für Star Trek Online Combatlog-Analyse
- **Frontend**: WPF-Anwendung (aktuell leer)
- **Ziel**: Nahtlose Integration ohne Python-Installation für Endanwender

## Lösungsansatz: Python als eingebettete Runtime

### Technologie-Stack
- **Backend**: Python mit PyInstaller für Standalone-Executable
- **Frontend**: WPF mit Process-Management für Python-Executable
- **Kommunikation**: JSON über stdin/stdout
- **Deployment**: Einzelne .exe-Datei mit eingebettetem Python-Backend

### Vorteile
- Vollständige Python-Funktionalität ohne Installation
- Keine Abhängigkeiten für Endanwender
- Einfache Deployment-Strategie
- Bewährte Technologie (PyInstaller)

### Herausforderungen
- Größere Bundle-Größe (~50-100MB)
- Process-Management zwischen WPF und Python
- JSON-Kommunikation über stdin/stdout

## Entwicklungsplan

### Phase 1: Backend-Vorbereitung (1-2 Wochen)

#### 1.1 Python-Backend als ausführbare Datei
- [ ] **PyInstaller Setup**
  - `pyinstaller --onefile --name=OSCRBackend --add-data "backend/OSCR/Data;Data" backend/OSCR/main.py`
  - Konfiguration für Windows-Bundle
  - Test der Standalone-Executable
  - Optimierung der Bundle-Größe

#### 1.2 API-Interface definieren
- [ ] **JSON-basierte Kommunikation**
  - Input: Log-Dateipfad, Analyse-Parameter
  - Output: Combat-Daten, Statistiken, Fehler
  - Schema-Definition für alle Datenstrukturen

#### 1.3 Backend-Wrapper erstellen
- [ ] **API-Wrapper für WPF-Integration**
  - JSON-Input/Output-Modus über stdin/stdout
  - Batch-Processing für mehrere Combats
  - Progress-Reporting für lange Analysen
  - Error-Handling und Logging

```python
# backend/OSCR/api_wrapper.py
import json
import sys
import logging
from .main import OSCR

def process_combat_log_api(input_data):
    """API-Endpoint für WPF-Integration"""
    try:
        parser = OSCR(input_data.get('logPath', ''))
        parser.analyze_log_file(
            max_combats=input_data.get('maxCombats', 10),
            result_handler=lambda combat: None
        )
        
        # Konvertiere Combats zu JSON-serialisierbarem Format
        combats_data = []
        for combat in parser.combats:
            combat_data = {
                'id': combat.id,
                'map': combat.map,
                'difficulty': combat.difficulty,
                'startTime': combat.start_time.isoformat(),
                'endTime': combat.end_time.isoformat(),
                'players': {}
            }
            
            for player_name, player_stats in combat.players.items():
                combat_data['players'][player_name] = {
                    'dps': player_stats.DPS,
                    'totalDamage': player_stats.total_damage,
                    'combatTime': player_stats.combat_time
                }
            
            combats_data.append(combat_data)
        
        return {
            'success': True,
            'combats': combats_data,
            'error': None
        }
    except Exception as e:
        return {
            'success': False,
            'combats': [],
            'error': str(e)
        }

def main():
    if len(sys.argv) > 1 and sys.argv[1] == "--api":
        # API-Modus für WPF
        input_data = json.loads(sys.stdin.read())
        result = process_combat_log_api(input_data)
        print(json.dumps(result))
    else:
        # Normale CLI
        from .cli import main as cli_main
        cli_main()
```

### Phase 2: Frontend-Integration (2-3 Wochen)

#### 2.1 Backend-Service im WPF
- [ ] **OSCRBackendService.cs erstellen**
  - Process-Management für Python-Executable
  - JSON-Kommunikation über stdin/stdout
  - Async/Await-Pattern für UI-Responsivität
  - Error-Handling und Timeout-Management
  - Progress-Reporting für lange Analysen

```csharp
public class OSCRBackendService : IOSCRBackendService
{
    private readonly string _backendPath;
    private readonly ILogger<OSCRBackendService> _logger;
    
    public OSCRBackendService(IConfiguration configuration, ILogger<OSCRBackendService> logger)
    {
        _backendPath = configuration["OSCRBackendPath"] ?? "OSCRBackend.exe";
        _logger = logger;
    }
    
    public async Task<CombatAnalysisResult> AnalyzeCombatLogAsync(string logPath, AnalysisOptions options)
    {
        var request = new
        {
            logPath = logPath,
            maxCombats = options.MaxCombats,
            settings = new
            {
                combatsToParse = options.CombatsToParse,
                secondsBetweenCombats = options.SecondsBetweenCombats,
                combatMinLines = options.CombatMinLines
            }
        };
        
        var jsonInput = JsonSerializer.Serialize(request);
        
        using var process = new Process();
        process.StartInfo.FileName = _backendPath;
        process.StartInfo.Arguments = "--api";
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.RedirectStandardInput = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.CreateNoWindow = true;
        
        process.Start();
        
        // JSON-Input senden
        await process.StandardInput.WriteAsync(jsonInput);
        process.StandardInput.Close();
        
        // JSON-Output lesen
        var jsonOutput = await process.StandardOutput.ReadToEndAsync();
        var errorOutput = await process.StandardError.ReadToEndAsync();
        
        await process.WaitForExitAsync();
        
        if (process.ExitCode != 0)
        {
            throw new BackendException($"Backend process failed: {errorOutput}");
        }
        
        var result = JsonSerializer.Deserialize<CombatAnalysisResult>(jsonOutput);
        return result;
    }
    
    public async Task<bool> IsBackendAvailableAsync()
    {
        try
        {
            return File.Exists(_backendPath);
        }
        catch
        {
            return false;
        }
    }
}
```

#### 2.2 Datenmodelle synchronisieren
- [ ] **C# Modelle für OSCR-Daten**
  - Combat-Klasse
  - PlayerStatistics-Klasse
  - DamageEntry-Klasse
  - JSON-Deserialisierung

#### 2.3 UI-Integration
- [ ] **Progress-Indikatoren**
  - Backend-Processing-Status
  - Combat-Analyse-Fortschritt
  - Error-Display

- [ ] **File-Watcher Integration**
  - Automatische Log-Datei-Erkennung
  - Real-time Updates

### Phase 3: Deployment & Distribution (1 Woche)

#### 3.1 Build-Integration
- [ ] **MSBuild-Targets für PyInstaller-Integration**
  - Automatisches PyInstaller-Build vor WPF-Build
  - Python-Backend-Executable in WPF-Ausgabe kopieren
  - Executable-Pfade in appsettings.json konfigurieren
  - Debug/Release-Varianten mit entsprechenden Backend-Builds

#### 3.2 Installer-Paket
- [ ] **WiX/Advanced Installer für Standalone-Deployment**
  - Python-Executable (OSCRBackend.exe) einbetten
  - WPF-Anwendung mit Backend-Executable bündeln
  - Keine Python-Installation erforderlich
  - Desktop-Verknüpfung und Startmenü-Einträge
  - Automatische Updates für beide Komponenten

#### 3.3 Testing & Validation
- [ ] **Integrationstests**
  - Verschiedene Log-Dateien testen
  - Performance-Benchmarks
  - Memory-Usage-Monitoring

### Phase 4: Optimierung & Erweiterung (1-2 Wochen)

#### 4.1 Performance-Optimierung
- [ ] **Caching-Strategien**
  - Combat-Ergebnisse zwischenspeichern
  - Incremental-Updates
  - Memory-Management

#### 4.2 Erweiterte Features
- [ ] **Real-time Monitoring**
  - Live-Log-Parsing
  - Streaming-Updates
  - Background-Processing

#### 4.3 Error-Handling & Logging
- [ ] **Robuste Fehlerbehandlung**
  - Backend-Crash-Recovery
  - Detailed Error-Logging
  - User-Friendly Error-Messages

## Technische Details

### Backend-API Schema

```json
{
  "request": {
    "logPath": "string",
    "maxCombats": "number",
    "settings": {
      "combatsToParse": "number",
      "secondsBetweenCombats": "number",
      "combatMinLines": "number"
    }
  },
  "response": {
    "success": "boolean",
    "combats": [
      {
        "id": "number",
        "map": "string",
        "difficulty": "string",
        "startTime": "string",
        "endTime": "string",
        "players": {
          "playerName": {
            "dps": "number",
            "totalDamage": "number",
            "combatTime": "number"
          }
        }
      }
    ],
    "error": "string"
  }
}
```

### WPF-Service-Interface

```csharp
public interface IOSCRBackendService
{
    Task<CombatAnalysisResult> AnalyzeCombatLogAsync(string logPath, AnalysisOptions options);
    Task<List<CombatInfo>> GetAvailableCombatsAsync(string logPath);
    Task<bool> IsBackendAvailableAsync();
    event EventHandler<CombatAnalysisProgressEventArgs> AnalysisProgress;
}
```

## Risiken & Mitigation

### Risiko 1: Python-Executable-Größe
- **Problem**: PyInstaller erstellt große Executables (~50-100MB)
- **Mitigation**: 
  - PyInstaller-Optimierung mit `--exclude-module` für ungenutzte Module
  - UPX-Kompression für weitere Größenreduktion
  - Nur benötigte Python-Standardbibliotheken einbinden

### Risiko 2: Process-Management-Komplexität
- **Problem**: WPF muss Python-Prozesse zuverlässig starten und verwalten
- **Mitigation**: 
  - Robuste Process-Klasse mit Timeout-Handling
  - Async/Await-Pattern für UI-Responsivität
  - Umfassende Error-Handling und Logging

### Risiko 3: JSON-Kommunikation über stdin/stdout
- **Problem**: Potentielle Probleme bei großen Datenmengen oder speziellen Zeichen
- **Mitigation**: 
  - Chunked-Processing für große Log-Dateien
  - UTF-8-Encoding explizit setzen
  - Fallback-Mechanismen für Kommunikationsfehler

## Zeitplan

| Phase | Dauer | Meilensteine |
|-------|-------|--------------|
| Phase 1 | 1-2 Wochen | Backend als Executable, API-Interface |
| Phase 2 | 2-3 Wochen | WPF-Integration, Datenmodelle |
| Phase 3 | 1 Woche | Deployment, Installer |
| Phase 4 | 1-2 Wochen | Optimierung, erweiterte Features |
| **Gesamt** | **5-8 Wochen** | **Vollständige Integration** |

## Nächste Schritte

1. **Sofort**: Python-API-Wrapper implementieren (`backend/OSCR/api_wrapper.py`)
2. **Woche 1**: PyInstaller-Setup und erste Standalone-Executable-Tests
3. **Woche 2**: WPF-Backend-Service mit Process-Management
4. **Woche 3**: Datenmodelle und JSON-Serialisierung
5. **Woche 4**: UI-Integration und erste End-to-End-Tests
6. **Woche 5**: Build-Integration und Deployment-Setup

## Erfolgskriterien

- [ ] **Standalone-Backend**: OSCRBackend.exe läuft ohne Python-Installation
- [ ] **WPF-Integration**: Frontend kommuniziert erfolgreich über stdin/stdout
- [ ] **Funktionalität**: Combat-Analyse identisch zur ursprünglichen CLI
- [ ] **Performance**: Akzeptable Verarbeitungszeiten (< 30s für typische Log-Dateien)
- [ ] **Deployment**: Einzelner Installer erstellt vollständig funktionsfähige Anwendung
- [ ] **Robustheit**: Error-Handling und Process-Management funktionieren zuverlässig
- [ ] **Bundle-Größe**: Finale Anwendung < 150MB (inkl. Python-Runtime)
