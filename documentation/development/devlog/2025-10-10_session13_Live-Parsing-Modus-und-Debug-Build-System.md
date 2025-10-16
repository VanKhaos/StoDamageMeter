## Session 13: Live-Parsing-Modus und Debug-Build-System

**Datum:** 2025-10-10  
**Dauer:** ~4 Stunden  
**Fokus:** Live-Parsing mit FileWatcher, IncrementalCombatUpdate, Debug-Build-System

### 🎯 **Was wir erreicht haben:**

#### ✅ **Erfolgreich implementiert:**

1. **Live-Parsing Backend-Endpoints (Python)**
   - `live_parse_log()`: Inkrementelles Lesen ab Byte-Offset
     - Liest nur neue Zeilen seit letztem Offset
     - Erkennt neue/aktive Combats basierend auf Zeit
     - Combat-Timeout: 30 Sekunden konfigurierbar
     - Gibt `new_combats`, `active_combat`, `current_byte_offset` zurück
   - `incremental_combat_update()`: Live-Stats-Berechnung
     - Analysiert Combat-Zeilen ohne komplettes Log neu zu lesen
     - Berechnet vollständige Player-Stats (DPS, Rankings, Abilities)
     - Companion- und Ability-Stats inklusive
     - Serialisiert zu CombatData-Format
   - Beide Endpoints in `main()` integriert mit Actions `live_parse` und `incremental_update`

2. **CombatLogWatcherService (C#)**
   - FileSystemWatcher für Combat-Log-Überwachung
   - Features:
     - Debouncing: 500ms Timer sammelt neue Zeilen
     - Byte-Offset-Tracking für inkrementelles Lesen
     - Thread-Safe Queue für Log-Zeilen
     - FileShare.ReadWrite für gesperrte Dateien
     - Unvollständige Zeilen werden gebuffert
   - Events:
     - `NewLinesDetected`: Neue Zeilen verfügbar
     - `WatcherError`: Fehler beim Watching
   - Public Methods:
     - `StartWatching()`: Überwachung starten (Offset: 0 = Anfang, -1 = Ende)
     - `StopWatching()`: Überwachung beenden
     - `CurrentByteOffset`, `IsWatching`, `TotalLinesProcessed` Properties

3. **LiveCombatViewModel**
   - State-Management für Live-Combat-Tracking
   - Properties:
     - `IsActive`: Live-Modus aktiv
     - `StatusText`: "Warte auf Combat..." / "Combat läuft..."
     - `CurrentCombat`: Aktuelles Combat-Data-Objekt
     - `CombatDuration`: Live-Timer (MM:SS)
     - `TotalDPS`: Summe aller Spieler-DPS
     - `LivePlayerStats`: ObservableCollection für UI-Binding
   - Events:
     - `CombatCompleted`: Wird ausgelöst wenn Combat endet (>30s Timeout)
   - Methods:
     - `StartLiveParsing()`: FileWatcher + Backend-Polling starten
     - `StopLiveParsing()`: Alles beenden
     - `OnNewLinesDetected()`: Verarbeitet neue Zeilen → Backend-Call → UI-Update
   - Duration-Timer: Aktualisiert Kampfdauer jede Sekunde

4. **LiveCombatView UI-Component**
   - XAML-Component im `Components/LiveCombat/` Ordner
   - Layout:
     - Status-Header mit Indikator (🟢 aktiv / ⚪ wartend)
     - Combat-Info-Panel (Type-Icon, Duration, Total DPS)
     - Live-Stats-Tabelle (identisch zu CombatStatsHeader)
   - Code-Behind:
     - `SetViewModel()`: ViewModel-Binding
     - `UpdateStatusText()`: Status + Combat-Info aktualisieren
     - `UpdateLiveStats()`: Player-Stats mit CombatStatsRenderer rendern
     - `StartLiveMode()`, `StopLiveMode()`: Public API
   - Star Trek Theme mit Card-Layout

5. **MainWindow Tab-Integration**
   - TabControl mit 2 Tabs:
     - Tab 1: "Damage Out" (historische Combats, bisheriger Content)
     - Tab 2: "Live Combat" (neue LiveCombatView-Component)
   - Tab-Wechsel-Event:
     - Bei Wechsel zu Live Combat: `StartLiveParsing()` aufrufen
     - Bei Wechsel weg: `StopLiveParsing()` aufrufen
   - Event-Handler:
     - `OnLiveCombatCompleted()`: Combat fertig → in Liste einfügen (TODO)
     - `MainTabControl_SelectionChanged()`: Tab-Wechsel-Logik

6. **OSCRBackendService erweitert**
   - Neue Methoden:
     - `LiveParseAsync()`: Ruft Backend-Endpoint `live_parse` auf
     - `IncrementalCombatUpdateAsync()`: Ruft Backend-Endpoint `incremental_update` auf
   - Interface `IOSCRBackendService` erweitert
   - Verwendet `ExecuteBackendCommandAsync<T>()` für JSON-Kommunikation

7. **Models erweitert**
   - `LiveParseResponse`: Backend-Response für Live-Parsing
     - `CurrentByteOffset`, `NewCombats`, `ActiveCombat`, `LinesProcessed`
   - `ActiveCombatInfo`: Info über aktiven Combat
     - `Lines`, `StartTime`, `Type`, `LineCount`, `IsActive`
   - `CombatData` erweitert:
     - `Date`, `Time`, `Type`, `Duration`, `Icon` Properties hinzugefügt
     - `TotalDamage`, `TotalDPS`, `LineCount` Properties hinzugefügt
     - `Players` von `Dictionary` zu `List` geändert für einfachere Sortierung

8. **Debug-Build-System erstellt**
   - **Neues Script:** `scripts\build_debug.ps1` im Scripts-Verzeichnis
   - **Funktionen:**
     - Prüft Backend-Existenz (baut falls nötig)
     - Baut App im Debug-Modus
     - Kopiert alle Dateien nach `Debug/` Ordner
     - Erstellt README.txt mit Build-Zeit
   - **Vorteile:**
     - Zentrale Debug-Version im Root
     - Debug-Symbole (.pdb) für Visual Studio Debugging
     - Schneller Zugriff: `.\Debug\StoDamageMeter.exe`
     - Getrennt von Release-Builds
   - **Ordner-Struktur:**
     ```
     Debug\
     ├── StoDamageMeter.exe      (App mit Debug-Symbolen)
     ├── StoDamageMeter.pdb      (Debug-Symbole)
     ├── OSCRBackend.exe         (Backend)
     # appsettings.json wurde entfernt
     ├── *.dll                   (Dependencies)
     └── README.txt
     ```

9. **Service-Registration in App.xaml.cs**
   - `CombatLogWatcherService` als Singleton registriert
   - Verfügbar über Dependency Injection
   - Wird in MainWindow automatisch injiziert

### 🔧 **Technische Details:**

#### **Datenfluss Live-Parsing:**
```
FileSystemWatcher (Log-Datei-Änderung)
    ↓ (500ms Debounce)
CombatLogWatcherService.NewLinesDetected Event
    ↓
LiveCombatViewModel.OnNewLinesDetected()
    ↓ (mindestens 20 Zeilen?)
Backend.IncrementalCombatUpdateAsync(combat_lines)
    ↓ (Python analysiert Zeilen)
CombatAnalysisResponse mit CombatData
    ↓
LiveCombatViewModel.CurrentCombat Update
    ↓ (PropertyChanged Event)
LiveCombatView.UpdateLiveStats()
    ↓
CombatStatsRenderer.RenderCombatStats()
    ↓
UI aktualisiert automatisch (WPF Data Binding)
```

#### **Combat-Ende-Erkennung:**
```csharp
// LiveCombatViewModel
private DateTime? _lastUpdateTime;

private async void OnNewLinesDetected(...)
{
    _lastUpdateTime = DateTime.Now;
    
    // ... Stats aktualisieren ...
    
    // Check Combat-Ende (30s Timeout)
    if (_combatStartTime != null)
    {
        var timeSinceLastUpdate = (DateTime.Now - _lastUpdateTime.Value).TotalSeconds;
        if (timeSinceLastUpdate > 30)
        {
            await FinalizeCombat();
        }
    }
}
```

#### **Backend live_parse Implementation:**
```python
def live_parse_log(log_path: str, from_byte_offset: int = 0, combat_timeout_seconds: int = 30):
    # Lese nur neue Bytes ab Offset
    with open(log_path, 'r', encoding='utf-8', errors='replace') as f:
        f.seek(from_byte_offset)
        new_content = f.read()
        new_lines = new_content.splitlines()
    
    # Parse Zeilen → Erkenne Combats
    for line in new_lines:
        parsed = parse_combat_log_line(line)
        time_diff = (timestamp - last_combat_time).total_seconds()
        
        if time_diff > combat_timeout_seconds:
            # Neuer Combat erkannt
            combats.append(current_combat_lines)
            current_combat_lines = [line]
    
    # Return neue Combats + aktiver Combat + neuer Offset
    return {
        'new_combats': completed_combats,
        'active_combat': active_combat_info,
        'current_byte_offset': file_size
    }
```

### 📁 **Wichtige Dateien:**

**Neu erstellt:**
- `backend/working_oscr_backend.py` - `live_parse_log()` und `incremental_combat_update()` hinzugefügt
- `app/Services/CombatLogWatcherService.cs` - FileWatcher-Service (~230 Zeilen)
- `app/ViewModels/LiveCombatViewModel.cs` - Live-Combat-ViewModel (~320 Zeilen)
- `app/Components/LiveCombat/LiveCombatView.xaml` - UI-Component
- `app/Components/LiveCombat/LiveCombatView.xaml.cs` - Code-Behind
- `scripts\build_debug.ps1` - Debug-Build-Script (~80 Zeilen)
- `Debug/` - Neuer Ordner für Debug-Builds

**Aktualisiert:**
- `app/Services/OSCRBackendService.cs` - Neue Methoden hinzugefügt
- `app/Models/OSCRModels.cs` - Neue Response-Klassen + CombatData erweitert
- `app/App.xaml.cs` - CombatLogWatcherService registriert
- `app/MainWindow.xaml` - TabControl mit Live Combat Tab
- `app/MainWindow.xaml.cs` - Tab-Wechsel-Event-Handler
- `AI_ASSISTANT_RULES.md` - Debug-Build-System dokumentiert

### 🚨 **Gelöste Probleme:**

#### **Problem 1: PropertyChangedEventHandler vs PropertyChanged**
- **Symptom:** `'LiveCombatViewModel' does not implement interface member 'INotifyPropertyChanged.PropertyChanged'`
- **Ursache:** `PropertyChanged` statt `PropertyChangedEventHandler` als Event-Type
- **Lösung:** Event-Deklaration geändert zu `public event PropertyChangedEventHandler? PropertyChanged;`
- **Resultat:** ✅ INotifyPropertyChanged korrekt implementiert

#### **Problem 2: ExecuteBackendCommand existiert nicht**
- **Symptom:** `The name 'ExecuteBackendCommand' does not exist in the current context`
- **Ursache:** Methoden-Name war `ExecuteBackendCommandAsync` statt `ExecuteBackendCommand`
- **Lösung:** Methoden-Aufrufe korrigiert
- **Resultat:** ✅ Backend-Kommunikation funktioniert

#### **Problem 3: CombatData.Players war Dictionary statt List**
- **Symptom:** `'List<PlayerStatistics>' does not contain a definition for 'Values'`
- **Ursache:** CombatData.Players wurde von Dictionary zu List geändert
- **Lösung:** CombatStatsRenderer angepasst (`Players` direkt statt `Players.Values`)
- **Resultat:** ✅ Stats-Rendering funktioniert mit neuem Format

#### **Problem 4: CombatData fehlten Properties für Live-Modus**
- **Symptom:** `'CombatData' does not contain a definition for 'Type'/'TotalDPS'/etc.`
- **Ursache:** Backend gibt diese Properties zurück, aber Model hatte sie nicht
- **Lösung:** CombatData-Klasse erweitert mit `Type`, `Date`, `Time`, `Duration`, `TotalDPS`, `TotalDamage`, `LineCount`, `Icon`
- **Resultat:** ✅ Vollständige Daten-Serialisierung

### 💡 **Lessons Learned:**

1. **FileSystemWatcher Debouncing:** Essentiell um zu viele Backend-Calls zu vermeiden
2. **Byte-Offset-Tracking:** Deutlich effizienter als komplettes Log neu zu lesen
3. **FileShare.ReadWrite:** Notwendig weil STO die Log-Datei während des Spiels sperrt
4. **ObservableCollection:** Perfekt für automatische UI-Updates in WPF
5. **Tab-basierte Navigation:** Bessere UX als Toggle-Button
6. **Inkrementelle Updates:** Backend muss nur neue Zeilen parsen, nicht ganzes Log
7. **Combat-Timeout:** 30 Sekunden ist guter Balance zwischen zu früh/zu spät
8. **Debug-Build-System:** Zentrale Struktur verbessert Workflow drastisch
9. **PropertyChanged Events:** Automatische UI-Updates durch INotifyPropertyChanged
10. **Dispatcher.Invoke:** Notwendig für UI-Updates aus Background-Threads

### 🔄 **Build-Status:**

- ✅ Backend kompiliert mit neuen Endpoints
- ✅ App kompiliert ohne Fehler
- ✅ Debug-Build-System funktioniert
- ✅ `Debug/` Ordner erstellt mit allen Dateien
- ✅ Alle Components registriert in DI-Container
- ✅ Tab-Navigation implementiert
- ✅ Live-Parsing-Infrastruktur vollständig

### 🎨 **UI-Status:**

**Implementiert:**
- ✅ Live Combat Tab in MainWindow
- ✅ LiveCombatView Component mit Status-Header
- ✅ Combat-Info-Panel (Type, Duration, DPS)
- ✅ Live-Stats-Tabelle (wie normale Stats)
- ✅ Tab-Wechsel-Logik (Start/Stop Live-Mode)

**Ausstehend (für nächste Session):**
- ⏳ Live-Modus testen mit echtem Combat-Log
- ⏳ Combat-Ende-Erkennung verfeinern
- ⏳ Beendete Combats in Combat-Liste einfügen
- ⏳ Performance-Optimierung bei vielen Spielern
- ⏳ Error-Handling verfeinern

### 📊 **Code-Umfang:**

**Neue Zeilen:**
- Backend: ~400 Zeilen (live_parse + incremental_update)
- CombatLogWatcherService: ~230 Zeilen
- LiveCombatViewModel: ~320 Zeilen
- LiveCombatView: ~160 Zeilen (XAML + Code-Behind)
- scripts\build_debug.ps1: ~80 Zeilen
- **Gesamt:** ~1190 Zeilen neuer Code

**Geänderte Dateien:** 11
**Neue Dateien:** 6

### 📋 **Features gemäß Plan:**

- ✅ Live-Modus ist **immer aktiv** (kein Toggle-Button, aktiviert bei Tab-Wechsel)
- ✅ Updates alle **0.5 Sekunden** (Debouncing)
- ✅ Historische Combats werden normal geladen (Tab 1)
- ✅ Live Combat Tab zeigt aktuellen Kampf (Tab 2)
- ✅ Beendete Combats sollen in Combat-Liste erscheinen (Event vorhanden, Integration TODO)

### 🎯 **Debug-Build-System:**

**Vorteile:**
- Zentrale Debug-Version im `Debug/` Ordner
- Ein Befehl: `.\scripts\build_debug.ps1`
- Debug-Symbole (.pdb) für besseres Debugging
- README.txt mit Build-Zeit
- Getrennt von Release-Builds (`Deploy/`, `Releases/`)

**Verwendung:**
```powershell
# Debug-Version erstellen
.\scripts\build_debug.ps1

# Debug-Version starten
.\Debug\StoDamageMeter.exe

# Logs prüfen
cat .\Debug\logs\*
```

---
**Nächste Session:** Live-Modus testen mit echtem STO Combat-Log, Combat-Integration in Liste, Performance-Tests


