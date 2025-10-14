## Session 13: Live-Parsing-Modus und Debug-Build-System

**Datum:** 2025-10-10  
**Dauer:** ~4 Stunden  
**Fokus:** Live-Parsing mit FileWatcher, IncrementalCombatUpdate, Debug-Build-System

### ðŸŽ¯ **Was wir erreicht haben:**

#### âœ… **Erfolgreich implementiert:**

1. **Live-Parsing Backend-Endpoints (Python)**
   - `live_parse_log()`: Inkrementelles Lesen ab Byte-Offset
     - Liest nur neue Zeilen seit letztem Offset
     - Erkennt neue/aktive Combats basierend auf Zeit
     - Combat-Timeout: 30 Sekunden konfigurierbar
     - Gibt `new_combats`, `active_combat`, `current_byte_offset` zurÃ¼ck
   - `incremental_combat_update()`: Live-Stats-Berechnung
     - Analysiert Combat-Zeilen ohne komplettes Log neu zu lesen
     - Berechnet vollstÃ¤ndige Player-Stats (DPS, Rankings, Abilities)
     - Companion- und Ability-Stats inklusive
     - Serialisiert zu CombatData-Format
   - Beide Endpoints in `main()` integriert mit Actions `live_parse` und `incremental_update`

2. **CombatLogWatcherService (C#)**
   - FileSystemWatcher fÃ¼r Combat-Log-Ãœberwachung
   - Features:
     - Debouncing: 500ms Timer sammelt neue Zeilen
     - Byte-Offset-Tracking fÃ¼r inkrementelles Lesen
     - Thread-Safe Queue fÃ¼r Log-Zeilen
     - FileShare.ReadWrite fÃ¼r gesperrte Dateien
     - UnvollstÃ¤ndige Zeilen werden gebuffert
   - Events:
     - `NewLinesDetected`: Neue Zeilen verfÃ¼gbar
     - `WatcherError`: Fehler beim Watching
   - Public Methods:
     - `StartWatching()`: Ãœberwachung starten (Offset: 0 = Anfang, -1 = Ende)
     - `StopWatching()`: Ãœberwachung beenden
     - `CurrentByteOffset`, `IsWatching`, `TotalLinesProcessed` Properties

3. **LiveCombatViewModel**
   - State-Management fÃ¼r Live-Combat-Tracking
   - Properties:
     - `IsActive`: Live-Modus aktiv
     - `StatusText`: "Warte auf Combat..." / "Combat lÃ¤uft..."
     - `CurrentCombat`: Aktuelles Combat-Data-Objekt
     - `CombatDuration`: Live-Timer (MM:SS)
     - `TotalDPS`: Summe aller Spieler-DPS
     - `LivePlayerStats`: ObservableCollection fÃ¼r UI-Binding
   - Events:
     - `CombatCompleted`: Wird ausgelÃ¶st wenn Combat endet (>30s Timeout)
   - Methods:
     - `StartLiveParsing()`: FileWatcher + Backend-Polling starten
     - `StopLiveParsing()`: Alles beenden
     - `OnNewLinesDetected()`: Verarbeitet neue Zeilen â†’ Backend-Call â†’ UI-Update
   - Duration-Timer: Aktualisiert Kampfdauer jede Sekunde

4. **LiveCombatView UI-Component**
   - XAML-Component im `Components/LiveCombat/` Ordner
   - Layout:
     - Status-Header mit Indikator (ðŸŸ¢ aktiv / âšª wartend)
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
     - `OnLiveCombatCompleted()`: Combat fertig â†’ in Liste einfÃ¼gen (TODO)
     - `MainTabControl_SelectionChanged()`: Tab-Wechsel-Logik

6. **OSCRBackendService erweitert**
   - Neue Methoden:
     - `LiveParseAsync()`: Ruft Backend-Endpoint `live_parse` auf
     - `IncrementalCombatUpdateAsync()`: Ruft Backend-Endpoint `incremental_update` auf
   - Interface `IOSCRBackendService` erweitert
   - Verwendet `ExecuteBackendCommandAsync<T>()` fÃ¼r JSON-Kommunikation

7. **Models erweitert**
   - `LiveParseResponse`: Backend-Response fÃ¼r Live-Parsing
     - `CurrentByteOffset`, `NewCombats`, `ActiveCombat`, `LinesProcessed`
   - `ActiveCombatInfo`: Info Ã¼ber aktiven Combat
     - `Lines`, `StartTime`, `Type`, `LineCount`, `IsActive`
   - `CombatData` erweitert:
     - `Date`, `Time`, `Type`, `Duration`, `Icon` Properties hinzugefÃ¼gt
     - `TotalDamage`, `TotalDPS`, `LineCount` Properties hinzugefÃ¼gt
     - `Players` von `Dictionary` zu `List` geÃ¤ndert fÃ¼r einfachere Sortierung

8. **Debug-Build-System erstellt**
   - **Neues Script:** `scripts\build_debug.ps1` im Scripts-Verzeichnis
   - **Funktionen:**
     - PrÃ¼ft Backend-Existenz (baut falls nÃ¶tig)
     - Baut Frontend im Debug-Modus
     - Kopiert alle Dateien nach `Debug/` Ordner
     - Erstellt README.txt mit Build-Zeit
   - **Vorteile:**
     - Zentrale Debug-Version im Root
     - Debug-Symbole (.pdb) fÃ¼r Visual Studio Debugging
     - Schneller Zugriff: `.\Debug\StoDamageMeter.exe`
     - Getrennt von Release-Builds
   - **Ordner-Struktur:**
     ```
     Debug\
     â”œâ”€â”€ StoDamageMeter.exe      (Frontend mit Debug-Symbolen)
     â”œâ”€â”€ StoDamageMeter.pdb      (Debug-Symbole)
     â”œâ”€â”€ OSCRBackend.exe         (Backend)
     # appsettings.json wurde entfernt
     â”œâ”€â”€ *.dll                   (Dependencies)
     â””â”€â”€ README.txt
     ```

9. **Service-Registration in App.xaml.cs**
   - `CombatLogWatcherService` als Singleton registriert
   - VerfÃ¼gbar Ã¼ber Dependency Injection
   - Wird in MainWindow automatisch injiziert

### ðŸ”§ **Technische Details:**

#### **Datenfluss Live-Parsing:**
```
FileSystemWatcher (Log-Datei-Ã„nderung)
    â†“ (500ms Debounce)
CombatLogWatcherService.NewLinesDetected Event
    â†“
LiveCombatViewModel.OnNewLinesDetected()
    â†“ (mindestens 20 Zeilen?)
Backend.IncrementalCombatUpdateAsync(combat_lines)
    â†“ (Python analysiert Zeilen)
CombatAnalysisResponse mit CombatData
    â†“
LiveCombatViewModel.CurrentCombat Update
    â†“ (PropertyChanged Event)
LiveCombatView.UpdateLiveStats()
    â†“
CombatStatsRenderer.RenderCombatStats()
    â†“
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
    
    # Parse Zeilen â†’ Erkenne Combats
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

### ðŸ“ **Wichtige Dateien:**

**Neu erstellt:**
- `backend/working_oscr_backend.py` - `live_parse_log()` und `incremental_combat_update()` hinzugefÃ¼gt
- `frontend/Services/CombatLogWatcherService.cs` - FileWatcher-Service (~230 Zeilen)
- `frontend/ViewModels/LiveCombatViewModel.cs` - Live-Combat-ViewModel (~320 Zeilen)
- `frontend/Components/LiveCombat/LiveCombatView.xaml` - UI-Component
- `frontend/Components/LiveCombat/LiveCombatView.xaml.cs` - Code-Behind
- `scripts\build_debug.ps1` - Debug-Build-Script (~80 Zeilen)
- `Debug/` - Neuer Ordner fÃ¼r Debug-Builds

**Aktualisiert:**
- `frontend/Services/OSCRBackendService.cs` - Neue Methoden hinzugefÃ¼gt
- `frontend/Models/OSCRModels.cs` - Neue Response-Klassen + CombatData erweitert
- `frontend/App.xaml.cs` - CombatLogWatcherService registriert
- `frontend/MainWindow.xaml` - TabControl mit Live Combat Tab
- `frontend/MainWindow.xaml.cs` - Tab-Wechsel-Event-Handler
- `AI_ASSISTANT_RULES.md` - Debug-Build-System dokumentiert

### ðŸš¨ **GelÃ¶ste Probleme:**

#### **Problem 1: PropertyChangedEventHandler vs PropertyChanged**
- **Symptom:** `'LiveCombatViewModel' does not implement interface member 'INotifyPropertyChanged.PropertyChanged'`
- **Ursache:** `PropertyChanged` statt `PropertyChangedEventHandler` als Event-Type
- **LÃ¶sung:** Event-Deklaration geÃ¤ndert zu `public event PropertyChangedEventHandler? PropertyChanged;`
- **Resultat:** âœ… INotifyPropertyChanged korrekt implementiert

#### **Problem 2: ExecuteBackendCommand existiert nicht**
- **Symptom:** `The name 'ExecuteBackendCommand' does not exist in the current context`
- **Ursache:** Methoden-Name war `ExecuteBackendCommandAsync` statt `ExecuteBackendCommand`
- **LÃ¶sung:** Methoden-Aufrufe korrigiert
- **Resultat:** âœ… Backend-Kommunikation funktioniert

#### **Problem 3: CombatData.Players war Dictionary statt List**
- **Symptom:** `'List<PlayerStatistics>' does not contain a definition for 'Values'`
- **Ursache:** CombatData.Players wurde von Dictionary zu List geÃ¤ndert
- **LÃ¶sung:** CombatStatsRenderer angepasst (`Players` direkt statt `Players.Values`)
- **Resultat:** âœ… Stats-Rendering funktioniert mit neuem Format

#### **Problem 4: CombatData fehlten Properties fÃ¼r Live-Modus**
- **Symptom:** `'CombatData' does not contain a definition for 'Type'/'TotalDPS'/etc.`
- **Ursache:** Backend gibt diese Properties zurÃ¼ck, aber Model hatte sie nicht
- **LÃ¶sung:** CombatData-Klasse erweitert mit `Type`, `Date`, `Time`, `Duration`, `TotalDPS`, `TotalDamage`, `LineCount`, `Icon`
- **Resultat:** âœ… VollstÃ¤ndige Daten-Serialisierung

### ðŸ’¡ **Lessons Learned:**

1. **FileSystemWatcher Debouncing:** Essentiell um zu viele Backend-Calls zu vermeiden
2. **Byte-Offset-Tracking:** Deutlich effizienter als komplettes Log neu zu lesen
3. **FileShare.ReadWrite:** Notwendig weil STO die Log-Datei wÃ¤hrend des Spiels sperrt
4. **ObservableCollection:** Perfekt fÃ¼r automatische UI-Updates in WPF
5. **Tab-basierte Navigation:** Bessere UX als Toggle-Button
6. **Inkrementelle Updates:** Backend muss nur neue Zeilen parsen, nicht ganzes Log
7. **Combat-Timeout:** 30 Sekunden ist guter Balance zwischen zu frÃ¼h/zu spÃ¤t
8. **Debug-Build-System:** Zentrale Struktur verbessert Workflow drastisch
9. **PropertyChanged Events:** Automatische UI-Updates durch INotifyPropertyChanged
10. **Dispatcher.Invoke:** Notwendig fÃ¼r UI-Updates aus Background-Threads

### ðŸ”„ **Build-Status:**

- âœ… Backend kompiliert mit neuen Endpoints
- âœ… Frontend kompiliert ohne Fehler
- âœ… Debug-Build-System funktioniert
- âœ… `Debug/` Ordner erstellt mit allen Dateien
- âœ… Alle Components registriert in DI-Container
- âœ… Tab-Navigation implementiert
- âœ… Live-Parsing-Infrastruktur vollstÃ¤ndig

### ðŸŽ¨ **UI-Status:**

**Implementiert:**
- âœ… Live Combat Tab in MainWindow
- âœ… LiveCombatView Component mit Status-Header
- âœ… Combat-Info-Panel (Type, Duration, DPS)
- âœ… Live-Stats-Tabelle (wie normale Stats)
- âœ… Tab-Wechsel-Logik (Start/Stop Live-Mode)

**Ausstehend (fÃ¼r nÃ¤chste Session):**
- â³ Live-Modus testen mit echtem Combat-Log
- â³ Combat-Ende-Erkennung verfeinern
- â³ Beendete Combats in Combat-Liste einfÃ¼gen
- â³ Performance-Optimierung bei vielen Spielern
- â³ Error-Handling verfeinern

### ðŸ“Š **Code-Umfang:**

**Neue Zeilen:**
- Backend: ~400 Zeilen (live_parse + incremental_update)
- CombatLogWatcherService: ~230 Zeilen
- LiveCombatViewModel: ~320 Zeilen
- LiveCombatView: ~160 Zeilen (XAML + Code-Behind)
- scripts\build_debug.ps1: ~80 Zeilen
- **Gesamt:** ~1190 Zeilen neuer Code

**GeÃ¤nderte Dateien:** 11
**Neue Dateien:** 6

### ðŸ“‹ **Features gemÃ¤ÃŸ Plan:**

- âœ… Live-Modus ist **immer aktiv** (kein Toggle-Button, aktiviert bei Tab-Wechsel)
- âœ… Updates alle **0.5 Sekunden** (Debouncing)
- âœ… Historische Combats werden normal geladen (Tab 1)
- âœ… Live Combat Tab zeigt aktuellen Kampf (Tab 2)
- âœ… Beendete Combats sollen in Combat-Liste erscheinen (Event vorhanden, Integration TODO)

### ðŸŽ¯ **Debug-Build-System:**

**Vorteile:**
- Zentrale Debug-Version im `Debug/` Ordner
- Ein Befehl: `.\scripts\build_debug.ps1`
- Debug-Symbole (.pdb) fÃ¼r besseres Debugging
- README.txt mit Build-Zeit
- Getrennt von Release-Builds (`Deploy/`, `Releases/`)

**Verwendung:**
```powershell
# Debug-Version erstellen
.\scripts\build_debug.ps1

# Debug-Version starten
.\Debug\StoDamageMeter.exe

# Logs prÃ¼fen
cat .\Debug\logs\*
```

---
**NÃ¤chste Session:** Live-Modus testen mit echtem STO Combat-Log, Combat-Integration in Liste, Performance-Tests


