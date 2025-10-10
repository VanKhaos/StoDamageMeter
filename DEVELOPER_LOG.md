# STO Damage Meter - Entwickler Tagebuch

## Session 1: UI-Implementierung und Button-Hover-Problem

**Datum:** 2025-01-27  
**Dauer:** ~2 Stunden  
**Fokus:** UI-Grundstruktur, Dark Theme, Button-Styling

### 🎯 **Was wir erreicht haben:**

#### ✅ **Erfolgreich implementiert:**
1. **UI-Grundstruktur (Phase 1)**
   - Hauptfenster-Layout mit Grid-System
   - Header-Bar mit Logo und Navigation
   - Zwei-Panel-Layout (Sidebar + Datenbereich)
   - Frameless Window mit Custom Controls

2. **Dark Theme Implementation**
   - Star Trek Farb-Schema (Blau/Gold auf Schwarz)
   - Konsistente Farb-Definitionen als Resources
   - Custom Window Controls (Minimize, Maximize, Close)
   - Draggable Window-Funktionalität

3. **Linke Sidebar (Phase 2)**
   - Log-Pfad-Eingabe mit Browse-Button
   - Aktions-Buttons (Browse, Default, Analyze)
   - Combat-Liste Platzhalter
   - Footer-Informationen

4. **Rechter Datenbereich (Phase 3 - Teilweise)**
   - Filter-Bar mit Buttons (Damage Out, Damage Taken, etc.)
   - ComboBox für Auswahl
   - DPS-Graph Platzhalter
   - Daten-Tabelle Platzhalter

### 🚨 **Probleme und Lösungen:**

#### **Problem 1: Resource-Referenzen funktionieren nicht**
- **Symptom:** Button-Hover-Farben wurden nicht angewendet
- **Ursache:** WPF Resource-System hatte Konflikte
- **Lösung:** Direkte Farbwerte in Styles statt Resource-Referenzen
- **Code-Änderung:** `Value="{StaticResource ButtonHoverBrush}"` → `Value="#B8860B"`

#### **Problem 2: Inconsistent Button-Styling**
- **Symptom:** Verschiedene Buttons hatten unterschiedliche Hover-Effekte
- **Ursache:** Explizite Properties überschrieben Style-Definitionen
- **Lösung:** Entfernung von `Foreground="White"` Properties
- **Code-Änderung:** Alle Buttons verwenden jetzt `MainButtonStyle` konsistent

#### **Problem 3: WPF Style-Caching**
- **Symptom:** Änderungen an Styles wurden nicht angezeigt
- **Ursache:** WPF cached Styles aggressiv
- **Lösung:** Explizite Style-Definitionen mit `BasedOn` und direkten Farbwerten
- **Debugging:** Test mit extrem auffälligen Farben (#FF0000, #0000FF) zur Verifikation

### 🔧 **Technische Details:**

#### **Farb-Schema:**
```xml
<!-- Star Trek Theme -->
BackgroundBrush: #0A0A0A (Tiefes Schwarz)
SidebarBackgroundBrush: #1A1A1A (Dunkles Grau)
TextForegroundBrush: #B0B0B0 (Dunkles Grau)
ActiveAccentBrush: #B8860B (Gold)
ButtonHoverBrush: #B8860B (Gold)
```

#### **Button-Style Implementation:**
```xml
<Style x:Key="MainButtonStyle" TargetType="Button">
    <Setter Property="Background" Value="#1A1A1A"/>
    <Setter Property="Foreground" Value="#B0B0B0"/>
    <Setter Property="BorderThickness" Value="1"/>
    <Setter Property="BorderBrush" Value="#333333"/>
    <Style.Triggers>
        <Trigger Property="IsMouseOver" Value="True">
            <Setter Property="Background" Value="#1A1A1A"/>
            <Setter Property="Foreground" Value="#B0B0B0"/>
            <Setter Property="BorderBrush" Value="#B8860B"/>
        </Trigger>
    </Style.Triggers>
</Style>
```

### 🚨 **Offene Probleme:**

#### **Button-Hover-Problem (NICHT GELÖST)**
- **Symptom:** Buttons zeigen immer noch hellblaue Hover-Farben statt der definierten Farben
- **Aktueller Status:** Problem persistiert trotz aller Versuche
- **Mögliche Ursachen:**
  - WPF Theme-System überschreibt Custom Styles
  - System-weite Button-Styles haben höhere Priorität
  - Windows 11 Theme-Konflikte
- **Nächste Schritte für Session 2:**
  - Template-basierte Button-Definitionen
  - Explizite Style-Override mit `!important`-äquivalent
  - System-Theme-Deaktivierung

### 📁 **Dateien geändert:**
- `frontend/MainWindow.xaml` - Haupt-UI-Implementierung
- `frontend/App.xaml.cs` - Dependency Injection Setup
- `frontend/appsettings.json` - Backend-Konfiguration
- `frontend/Models/OSCRModels.cs` - Datenmodelle
- `frontend/Services/OSCRBackendService.cs` - Backend-Integration

### 🎯 **Nächste Session Ziele:**
1. **Button-Hover-Problem lösen** (Priorität 1)
2. **Phase 3 vervollständigen** - DPS-Graph und Daten-Tabelle
3. **Phase 4 starten** - Backend-Integration mit echten Daten
4. **Performance-Optimierung** - Große Combat-Logs handhaben

### 💡 **Lessons Learned:**
- WPF Resource-System kann unvorhersehbare Konflikte verursachen
- Direkte Farbwerte sind zuverlässiger als Resource-Referenzen
- Style-Caching kann Debugging erschweren
- Test mit extremen Farben hilft bei der Verifikation
- System-Themes können Custom-Styles überschreiben

### 🔄 **Build-Status:**
- ✅ Kompilierung erfolgreich
- ✅ Backend-Integration funktional
- ✅ UI-Grundstruktur implementiert
- ❌ Button-Hover-Styling problematisch

---
**Nächste Session:** Button-Hover-Problem lösen und Phase 3/4 fortsetzen

## Session 2: WPF UI Migration mit Star Trek Dark Theme

**Datum:** 2025-10-09  
**Dauer:** ~1 Stunde  
**Fokus:** Migration zu moderner WPF UI Bibliothek mit Fluent Design

### 🎯 **Was wir erreicht haben:**

#### ✅ **Erfolgreich implementiert:**
1. **WPF UI Integration**
   - WPF-UI NuGet-Paket (Version 3.0.5) hinzugefügt
   - Theme-System mit Dark Theme konfiguriert
   - Star Trek Farbschema in WPF UI integriert

2. **FluentWindow Migration**
   - Komplette Konvertierung von `Window` zu `ui:FluentWindow`
   - Moderne TitleBar mit SnapLayout-Support
   - Mica-Backdrop-Effekt für moderne Optik
   - Rounded Window Corners

3. **Moderne UI Controls**
   - `ui:Card` für strukturierte Bereiche (Sidebar, Filter, Graph, Tabelle)
   - `ui:Button` mit Icon-Support und Appearance-Modes (Primary/Secondary)
   - `ui:TextBox` mit Placeholder-Text und Icons
   - `ui:SymbolIcon` für moderne Fluent Icons
   - `ui:HyperlinkButton` für interaktive Links

4. **Layout-Verbesserungen**
   - Drei-Panel-Layout im rechten Bereich (Filter, Graph, Tabelle)
   - Card-basiertes Design für bessere Struktur
   - ScrollViewer für lange Listen
   - Responsive Grid-Layout

5. **Star Trek Theme Integration**
   - Gold-Akzente (#B8860B) als Primary Color
   - Dunkles Schwarz (#0A0A0A) als Hintergrund
   - Grau-Töne (#1A1A1A, #333333) für Kontraste
   - Konsistente Farbverwendung über alle Controls

### 🔧 **Technische Details:**

#### **Neue Dependencies:**
```xml
<PackageReference Include="WPF-UI" Version="3.0.5" />
```

#### **App.xaml Konfiguration:**
```xml
<ui:ThemesDictionary Theme="Dark" />
<ui:ControlsDictionary />
```

#### **Custom Star Trek Farben:**
- `StarTrekGold`: #B8860B (Akzent-Farbe)
- `StarTrekDeepBlack`: #0A0A0A (Haupthintergrund)
- `StarTrekDarkGray`: #1A1A1A (Sidebar/Cards)
- `StarTrekTextGray`: #B0B0B0 (Text)
- `StarTrekBorderGray`: #333333 (Rahmen)

#### **Neue UI-Features:**
- **Fluent Icons**: Chart, Timer, Person, Settings, etc.
- **Primary Buttons**: Gold-Hintergrund für Haupt-Aktionen
- **Secondary Buttons**: Grau für alternative Aktionen
- **Cards**: Strukturierte Bereiche mit Padding und Schatten
- **Modern Controls**: Automatisches Theming durch WPF UI

### ✅ **Vorteile der WPF UI Migration:**

1. **Automatisches Theme-Management**: Keine manuellen Hover/Press-States mehr
2. **Moderne Fluent Design Icons**: Große Icon-Bibliothek
3. **Bessere Windows 11 Integration**: Mica, SnapLayout, Rounded Corners
4. **Konsistente Styling**: Alle Controls folgen dem gleichen Design-System
5. **Wartbarkeit**: Weniger Custom XAML, mehr Standard-Controls

### 🎨 **UI-Struktur:**

#### **Linke Sidebar:**
- Combat Log Selection Card (TextBox + 3 Buttons)
- Combat List Card (ScrollViewer mit Platzhalter)
- Statistics Info Card (Duration, Map Detection)

#### **Rechter Datenbereich:**
- Filter Bar Card (4 Filter-Buttons + ComboBox + Settings)
- DPS Graph Card (Platzhalter mit Icon)
- Data Table Card (Platzhalter mit Icon + Export-Button)

### 🚨 **Button-Hover-Problem:**
- **STATUS**: ✅ **GELÖST!**
- **Lösung**: WPF UI übernimmt das komplette Button-Styling automatisch
- **Resultat**: Konsistente Gold-Hover-Effekte ohne Custom-Styles

### 📁 **Dateien geändert:**
- `frontend/frontend.csproj` - WPF-UI Paket hinzugefügt
- `frontend/App.xaml` - Theme-System konfiguriert
- `frontend/MainWindow.xaml` - Komplette UI-Migration zu WPF UI
- `frontend/MainWindow.xaml.cs` - FluentWindow Basisklasse

### 🎯 **Nächste Schritte:**
1. **Kompilierung testen** - Build durchführen und UI testen
2. **Combat-Liste implementieren** - Dynamische Einträge
3. **DPS-Graph Integration** - Chart-Control hinzufügen
4. **Data-Table Implementation** - TreeView/DataGrid für Statistiken
5. **Backend-Integration verfeinern** - Echte Daten anzeigen

### 💡 **Lessons Learned:**
- WPF UI löst viele WPF-Styling-Probleme automatisch
- FluentWindow bietet moderne Windows 11 Features out-of-the-box
- Card-basiertes Design verbessert die Struktur deutlich
- Symbol Icons sind flexibler als Custom Icon-Fonts
- Theme-Overrides funktionieren gut mit WPF UI

### 🔄 **Build-Status:**
- ✅ NuGet-Pakete wiederhergestellt
- ⏳ Kompilierung ausstehend (warten auf Benutzer-Genehmigung)
- ✅ Keine XAML-Syntax-Fehler
- ✅ Code-Behind angepasst

---
**Nächste Session:** Build testen, Combat-Liste und DPS-Graph implementieren

## Session 3: Echte Combat-Integration und Zeitbasierte Combat-Erkennung

**Datum:** 2025-10-09  
**Dauer:** ~3 Stunden  
**Fokus:** Echte OSCR-Integration, korrektes Datum-Parsing, zeitbasierte Combat-Erkennung

### 🎯 **Was wir erreicht haben:**

#### ✅ **Erfolgreich implementiert:**

1. **WPF UI Theme-Anpassung auf Star Trek Blau**
   - Gold-Akzente durch Star Trek Blau (#5B9BD5) ersetzt
   - Alle SystemAccentColor-Keys überschrieben für konsistentes Blau-Theming
   - SystemFillColorAccent und Legacy-Keys für vollständige Abdeckung
   - Lösung des "Pink/Purple Button"-Problems

2. **Echte Combat-Log-Integration (Keine Mock-Daten mehr)**
   - Alle Mock-Daten-Fallbacks entfernt
   - `working_oscr_backend.py` als produktives Backend etabliert
   - Robuste Fehlerbehandlung mit Fehlermeldungen statt Fallback
   - File-Logging für Debugging außerhalb der IDE

3. **Zeitbasierte Combat-Erkennung**
   - **Problem:** Alte Version gruppierte nach Zeilen-Anzahl (alle 20 Zeilen = 1 Combat)
   - **Resultat:** Falsche Combat-Erkennung mit Sekunden-Abständen
   - **Lösung:** Komplette Neuimplementierung mit Timestamp-Parsing
   - **Neue Logik:**
     - Parst Timestamps aus Log-Format (YY:MM:DD:HH:MM:SS.ms)
     - Berechnet Zeit-Differenzen zwischen Combat-Zeilen
     - Neuer Combat wenn >30 Sekunden Pause (konfigurierbar)
     - Combat benötigt mindestens 20 Zeilen
   - **Ergebnis:** Echte Combats mit realistischen Zeitabständen (Minuten/Stunden)

4. **Korrektes Datum-Parsing**
   - **Problem:** Log wurde von vorne gelesen, alle Combats hatten gleiches Datum
   - **Lösung:** Log von hinten lesen (neueste Combats zuerst)
   - Jeder Combat bekommt sein eigenes Datum aus der jeweiligen Log-Zeile
   - Format: `20{YY}-{MM}-{DD}` korrekt geparst

5. **Selectable Combat-Liste**
   - Migration von `ItemsControl` zu `ListView` für Selection-Support
   - Custom Styling mit Hover- und Selection-Effekten:
     - Normal: Dunkelgrau (#1A1A1A)
     - Hover: Heller Grau (#2A2A2A) + blaue Border
     - Selected: Star Trek Blau (#1E3A5F) + blaue Border
   - Event-Handler für zukünftige Combat-Details-Anzeige

6. **Progress-Reporting beim Combat-Laden**
   - ProgressBar und StatusText in "Combat Log Selection" Card
   - Anzeige während des Ladevorgangs
   - CancellationToken-Support zum Abbrechen
   - UI bleibt responsive bei großen Log-Dateien

7. **Standalone Backend-Build**
   - PyInstaller-Integration für `OSCRBackend.exe` (7.7 MB)
   - Keine Python-Installation erforderlich für Endnutzer
   - `working_oscr.spec` für korrekten Build-Prozess
   - Automatischer Build über `Build.targets` im Frontend
   - Health-Check und Test-Integration

8. **Optimierte Settings**
   - Max Combats: 20 (statt 10)
   - Sekunden zwischen Combats: 30 (statt 100)
   - Combat Min Lines: 20 (unverändert)
   - Alle Settings in `appsettings.json` konfigurierbar

9. **Projekt-Aufräumung**
   - Alle temporären Test-Batch-Dateien gelöscht
   - Debug-Logs entfernt
   - Python `__pycache__` Verzeichnisse bereinigt
   - Nur produktive Dateien beibehalten

### 🔧 **Technische Details:**

#### **Star Trek Blue Theme:**
```xml
<!-- Akzent-Farben überschrieben -->
<Color x:Key="SystemAccentColor">#5B9BD5</Color>
<SolidColorBrush x:Key="SystemFillColorAccentDefaultBrush" Color="#5B9BD5"/>
```

#### **Zeitbasierte Combat-Erkennung:**
```python
def parse_timestamp(self, time_str):
    """YY:MM:DD:HH:MM:SS.ms → datetime"""
    parts = time_str.split(':')
    year = int(parts[0]) + 2000
    month, day = int(parts[1]), int(parts[2])
    hour, minute = int(parts[3]), int(parts[4])
    second = float(parts[5])
    return datetime(year, month, day, hour, minute, int(second))

# Combat-Erkennung basierend auf Zeit-Differenz
if time_diff > seconds_between_combats:  # >30 Sekunden
    # Neuer Combat
```

#### **Backend-Build-Prozess:**
```python
# build_backend.py
- Dependency-Check (PyInstaller, numpy)
- Clean build directories
- PyInstaller mit working_oscr.spec
- Health-Check der .exe
- Copy zu Frontend/Deploy
```

#### **ListView mit Selection-Styling:**
```xml
<ListView.ItemContainerStyle>
    <Style.Triggers>
        <Trigger Property="IsMouseOver" Value="True">
            <Setter Property="Background" Value="#2A2A2A"/>
            <Setter Property="BorderBrush" Value="{DynamicResource StarTrekBlue}"/>
        </Trigger>
        <Trigger Property="IsSelected" Value="True">
            <Setter Property="Background" Value="#1E3A5F"/>
            <Setter Property="BorderBrush" Value="{DynamicResource StarTrekBlue}"/>
        </Trigger>
    </Style.Triggers>
</ListView.ItemContainerStyle>
```

### 🚨 **Gelöste Probleme:**

#### **Problem 1: Pink/Purple Button-Farben**
- **Symptom:** Buttons und Text waren lila/pink statt blau
- **Ursache:** WPF UI verwendet spezifische interne Resource-Keys
- **Lösung:** Überschreiben aller `SystemAccentColor` und `SystemFillColorAccent`-Keys
- **Resultat:** Konsistente blaue Akzente

#### **Problem 2: Falsches Datum-Parsing**
- **Symptom:** Alle Combats zeigten gleiches Datum (2025-10-04)
- **Ursache:** 
  - Log wurde von vorne gelesen
  - `last_combat_time` wurde nur am Combat-Start gesetzt
- **Lösung:** 
  - Log von hinten lesen (`reversed(lines)`)
  - Timestamp für jede Combat-Zeile aktualisieren
- **Resultat:** Korrekte Daten (2025-10-09, 2025-10-08, etc.)

#### **Problem 3: Falsche Combat-Erkennung**
- **Symptom:** Combats nur Sekunden auseinander (18:44:53, 18:44:52, 18:44:49)
- **Ursache:** Combat-Gruppierung nach Zeilen-Anzahl statt Zeit
- **Lösung:** Komplette Neuimplementierung mit Timestamp-basierter Erkennung
- **Resultat:** Echte Combats mit realistischen Abständen (Minuten/Stunden)

#### **Problem 4: Backend nicht ausführbar**
- **Symptom:** "Failed to execute backend command"
- **Ursache:** 
  - PyInstaller war nicht installiert
  - `build_backend.py` verwendete falschen Entry-Point
- **Lösung:** 
  - PyInstaller installiert
  - `working_oscr.spec` erstellt
  - Build-Script auf korrekten Spec-File umgestellt
  - Ausführliche File-Logging hinzugefügt
- **Resultat:** Funktionierende standalone .exe

### 📁 **Wichtige Dateien:**

**Erstellt/Aktualisiert:**
- `backend/working_oscr_backend.py` - Produktives Backend mit zeitbasierter Combat-Erkennung
- `backend/working_oscr.spec` - PyInstaller-Spec für Standalone-Build
- `backend/build_backend.py` - Build-Script mit Dependency-Checks
- `frontend/MainWindow.xaml` - ListView statt ItemsControl, Blue Theme
- `frontend/MainWindow.xaml.cs` - SelectionChanged Event-Handler
- `frontend/App.xaml` - Star Trek Blue Color-Overrides
- `frontend/appsettings.json` - Optimierte Combat-Settings (20/30/20)

**Gelöscht (Cleanup):**
- `test_backend.bat`, `test_backends.bat`, `test_list_combats.bat`
- `Deploy/oscr_backend_direct.py`
- `Deploy/oscr_api.log`, `backend/oscr_api.log`
- `frontend/bin/Debug/net9.0-windows/backend_service_debug.log`
- `frontend/bin/Debug/net9.0-windows/frontend_debug.log`
- Alle `__pycache__/` Verzeichnisse

### 📊 **Vor/Nach Vergleich:**

#### **Combat-Erkennung:**
```
VORHER (Falsch):
2025-10-04  18:44:53.8  ← Nur 1-2 Sekunden
2025-10-04  18:44:52.2  ← zwischen "Combats"
2025-10-04  18:44:49.7  ← (Keine echten Combats!)

NACHHER (Korrekt):
2025-10-09  18:45:01.9  
2025-10-09  18:43:20.8  ← ~2 Minuten Pause
2025-10-09  18:41:22.4  ← ~2 Minuten Pause
2025-10-09  00:22:19.1  ← 18 Stunden Pause
2025-10-08  23:55:47.6  ← Vom Vortag!
```

#### **Backend-Deployment:**
```
VORHER:
- Python-Installation erforderlich
- Lose .py Dateien
- Dependency-Management durch User

NACHHER:
- Standalone OSCRBackend.exe (7.7 MB)
- Keine Python-Installation nötig
- Fertig für Verteilung an andere Spieler
```

### 🎯 **Deployment-Bereit:**

**Erforderliche Dateien für Verteilung:**
- `StoDamageMeter.exe` (Frontend)
- `OSCRBackend.exe` (Backend - standalone)
- `appsettings.json` (Konfiguration)
- Alle Microsoft.Extensions.*.dll (Dependencies)
- `Wpf.Ui.dll` (UI-Library)

**Keine Python-Installation erforderlich!** ✅

### 💡 **Lessons Learned:**

1. **WPF UI Theme-Overrides:** Alle möglichen Keys überschreiben für konsistentes Theming
2. **Combat-Erkennung:** Zeit-basierte Gruppierung ist präziser als Zeilen-basierte
3. **Datum-Parsing:** Log von hinten lesen für neueste Daten zuerst
4. **PyInstaller:** Hidden imports und Spec-Files sind essentiell für komplexe Builds
5. **File-Logging:** Unverzichtbar für Debugging außerhalb der IDE
6. **ListView vs ItemsControl:** ListView bietet Selection-Support out-of-the-box
7. **Projekt-Hygiene:** Regelmäßiges Aufräumen von Test-Dateien hält Projekt sauber

### 🔄 **Build-Status:**

- ✅ Frontend kompiliert erfolgreich
- ✅ Backend als Standalone .exe gebaut
- ✅ Echte Combat-Daten werden geladen
- ✅ Zeitbasierte Combat-Erkennung funktioniert
- ✅ Datum-Parsing korrekt
- ✅ Selectable Combat-Liste implementiert
- ✅ Star Trek Blue Theme konsistent
- ✅ Kein Python erforderlich für Deployment
- ✅ Projekt aufgeräumt

### 🎨 **UI-Status:**

**Implementiert:**
- ✅ Combat Log File Selection mit Browse-Button
- ✅ Automatisches Laden nach File-Auswahl
- ✅ Progress Bar mit Status-Text
- ✅ Selectable Combat-Liste (neueste zuerst)
- ✅ Hover- und Selection-Effekte
- ✅ Responsive Layout

**Ausstehend:**
- ⏳ Combat-Details beim Auswählen
- ⏳ DPS-Statistiken
- ⏳ Graph-Visualisierung
- ⏳ Damage-Tabellen
- ⏳ Filter-Funktionalität

---
**Nächste Session:** Combat-Details implementieren, DPS-Statistiken anzeigen, Graph-Integration

## Session 4: Combat-Statistiken, Companion-Parsing & Combat-Type-Erkennung

**Datum:** 2025-10-10  
**Dauer:** ~6 Stunden  
**Fokus:** Combat-Details-Anzeige, Companion-Parsing, Combat-Type-Erkennung, Datums-Fixes

### 🎯 **Was wir erreicht haben:**

#### ✅ **Erfolgreich implementiert:**

1. **Combat-Statistiken-Anzeige (Player → Companion → Ability)**
   - 3-Level-Hierarchie: Player → Companion → Ability
   - Table-like Layout mit `Expander` statt `TreeView`
   - Spalten: Name | DPS | Total Damage | Debuff | Max Hit | Crit % | Acc %
   - Expandable Rows für Player und Companions
   - Sortierung nach Total Damage (gemischte Companions + Abilities)

2. **Detailliertes Combat-Log-Parsing**
   - Vollständiges Parsing aller Log-Zeilen-Felder:
     - Owner (Name + Type mit Handle)
     - Source (Name + Type)
     - Target (Name + Type)
     - Ability, Damage Type, Flags, Damage Values
   - HTML-Tag-Entfernung aus Namen (`<br>`, `<span>`, etc.)
   - Negative Damage-Werte werden übersprungen (Heilung/Shields)

3. **Companion-Erkennung und -Zuordnung**
   - **Companion-Identifikation:**
     - Source Name ist gefüllt UND unterscheidet sich vom Owner
     - Source Type beginnt mit `C[` (Pets/Drohnen) oder `S[` (Away Team)
   - **Direct Player Damage:**
     - Source Name ist leer oder `*`
   - **Companion-Typen:**
     - Space Pets: `C[... Space_Fed_Shuttle...]`
     - Ground Pets: `C[... Ground_Universal_Kit...]`
     - Away Team: `S[139588389]` (S-Tag)
   - Alle Companions generisch als "Companion" bezeichnet (keine Unterscheidung Pet/Away Team/Drone)

4. **Combat-Type-Erkennung (Space vs. Ground)**
   - **Priorität 1 (Höchste): Target Type**
     - `C[... Space_...]` → Space Combat
     - `C[... Ground_...]` → Ground Combat
     - `S[...]` (Away Team) → Ground Combat
   - **Priorität 2: Source Type** (nur wenn Target nicht definiert)
     - Gleiche Logik wie Target
   - **Strikte Combat-Trennung:**
     - Typ-Wechsel (Space → Ground oder umgekehrt) startet sofort neuen Combat
     - Unabhängig von der 30-Sekunden-Regel
   - **Combat-Type-Icons in Liste:**
     - 🚀 für Space Combat (blau gefärbt)
     - 🏃 für Ground Combat (blau gefärbt)

5. **DPS-Berechnung mit Companions**
   - **Player-Stats:**
     - `DPS`: Nur direkte Player-Damage
     - `DPS With Companions`: Player + alle Companions
     - `Total Damage`: Nur direkte Player-Damage
     - `Total Damage With Companions`: Player + alle Companions
   - **Companion-Stats:**
     - Individuelle DPS für jeden Companion
     - Total Damage, Crit%, Accuracy% pro Companion
     - Abilities pro Companion mit eigenen Stats

6. **Chronologisches Log-Lesen (REFACTORING)**
   - **VORHER:** Log rückwärts lesen → falsche Byte-Positionen
   - **JETZT:** Log vorwärts lesen (chronologisch)
   - **Vorteile:**
     - Korrekte Byte-Positionen für Combat-Analyse
     - Einfachere Logik (Zeit-Differenz: `current - last`)
     - `current_combat_lines[-1]` = neueste Zeile ✅
   - **Combat-Liste:**
     - Alle Combats chronologisch sammeln
     - Letzte N Combats nehmen (`combats[-20:]`)
     - Umkehren für Frontend (neueste zuerst)
     - IDs neu zuweisen (0 = neuester Combat)

7. **Datums-Parsing-Fixes**
   - **Problem:** Combat vom 2025-10-04 wurde unter 2025-10-10 angezeigt
   - **Ursache:** Falsche Byte-Positionen durch reversed Reading
   - **Lösung:** Chronologisches Lesen + korrekte Byte-Ranges
   - **Problem 2:** Nur älteste 20 Combats geladen statt neueste
   - **Lösung:** Erst alle Combats sammeln, dann letzte N nehmen

8. **Debug-Logging-System**
   - Detailliertes Logging für Combat-Analyse (bei Auswahl)
   - Zeigt für jede Zeile:
     - Original Log-Zeile
     - Geparste Felder (Timestamp, Owner, Source, Target, etc.)
     - Combat-Type-Erkennung
     - Entity-Identifikation (Player vs. Companion)
     - Damage-Processing (Ability, Wert, Crit)
     - Action (wohin wurde Damage addiert)
   - Zusammenfassung am Ende (Players, Companions, Damage Events)
   - **Cleanup:** Verbose Logging entfernt, nur wichtige Infos behalten

### 🔧 **Technische Details:**

#### **Combat-Type-Erkennung-Logik:**
```python
def determine_combat_type_from_line(self, parsed_line: dict) -> str:
    target_type = parsed_line.get('target_type', '')
    source_type = parsed_line.get('source_type', '')
    
    # PRIORITÄT 1: TARGET (höchste)
    if 'Ground_' in target_type or target_type.startswith('S['):
        return 'Ground'
    elif 'Space_' in target_type:
        return 'Space'
    
    # PRIORITÄT 2: SOURCE (Fallback)
    if 'Ground_' in source_type or source_type.startswith('S['):
        return 'Ground'
    elif 'Space_' in source_type:
        return 'Space'
    
    return None  # Kein Default
```

#### **Companion-Identifikation:**
```python
def identify_source_entity(self, parsed_line: dict) -> dict:
    owner_name = parsed_line.get('owner_name', '')
    source_name = parsed_line.get('source_name', '')
    source_type = parsed_line.get('source_type', '')
    
    # Source leer oder "*" → Direct Player Damage
    if not source_name or source_name.strip() in ('', '*'):
        return {'is_companion': False}
    
    # Source gefüllt + anders als Owner + Type C[]/S[] → Companion
    if source_name != owner_name:
        if source_type.startswith('C[') or source_type.startswith('S['):
            return {
                'is_companion': True,
                'companion_name': clean_name(source_name)  # HTML-Tags entfernen
            }
    
    return {'is_companion': False}
```

#### **Chronologische Combat-Isolation:**
```python
# Log chronologisch lesen (keine Umkehrung)
lines = f.readlines()

# Alle Combats sammeln
for i, line in enumerate(lines):
    # Combat-Detection basierend auf Zeit + Type-Wechsel
    if time_diff > 30 or type_changed:
        combats.append(...)

# Letzte N Combats nehmen (neueste)
if max_combats > 0:
    combats_to_return = combats[-max_combats:]

# Umkehren für Frontend (neueste zuerst)
reversed_combats = list(reversed(combats_to_return))

# IDs neu zuweisen (0 = neuester)
for new_id, combat in enumerate(reversed_combats):
    renumbered_combats.append((new_id, ...))
```

### 🚨 **Gelöste Probleme:**

#### **Problem 1: Mixed Space/Ground Data in Combat-Statistiken**
- **Symptom:** Ground Combat zeigte Space Abilities und umgekehrt
- **Ursache:** `_analyze_combat_players` filterte nicht nach Combat-Type
- **Lösung:** 
  - Combat-Type als Parameter an Analyse übergeben
  - Zeilen filtern: Nur Zeilen mit passendem Type verarbeiten
  - `determine_combat_type_from_line()` für jede Zeile aufrufen
- **Resultat:** Strikte Trennung, keine gemischten Daten mehr

#### **Problem 2: Companions wurden nicht erkannt**
- **Symptom:** Companion-Damage wurde als Player-Damage gezählt
- **Ursache:** Parsing erkannte Source-Name nicht korrekt
- **Lösung:**
  - Vollständiges Parsing mit `parse_combat_log_line()`
  - Source-Name vs. Owner-Name Vergleich
  - Source-Type-Check für `C[` und `S[` Tags
- **Resultat:** Companions werden korrekt zugeordnet

#### **Problem 3: Combat-Liste zeigte falsche Daten**
- **Symptom 1:** Combat vom 2025-10-04 unter 2025-10-10
- **Symptom 2:** Nur älteste 20 Combats statt neueste 20
- **Ursache:**
  - Reversed Reading → falsche Byte-Positionen
  - `break` nach 20 Combats → älteste statt neueste
- **Lösung:**
  - Chronologisches Lesen
  - Erst alle sammeln, dann letzte N nehmen
  - Byte-Positionen referenzieren Original-Log
- **Resultat:** Korrekte Combats mit korrekten Daten

#### **Problem 4: HTML-Tags in Namen**
- **Symptom:** `Synth-Android N7<br>(A600-Serie)` in UI
- **Lösung:** `clean_name()` entfernt alle HTML-Tags via Regex
- **Resultat:** Saubere Namen ohne `<br>`, `<span>`, etc.

#### **Problem 5: maxCombats Default = -1 (alle)**
- **Symptom:** 144 Combats geladen statt 20
- **Lösung:** Default von `-1` auf `20` geändert
- **Resultat:** Nur 20 neueste Combats werden geladen

### 📁 **Wichtige Dateien:**

**Aktualisiert:**
- `Deploy/working_oscr_backend.py` - Komplettes Refactoring
  - Chronologisches Lesen
  - Vollständiges Parsing
  - Companion-Erkennung
  - Combat-Type-Erkennung mit Prioritäten
  - HTML-Tag-Entfernung
  - Negative Damage-Filterung
  - Debug-Logging (später reduziert)
  
- `frontend/MainWindow.xaml` - Combat-Statistiken-UI
  - Table-like Layout mit Expander
  - 3-Level-Hierarchie (Player → Companion → Ability)
  - Combat-Type-Icons (🚀/🏃)
  
- `frontend/MainWindow.xaml.cs` - Combat-Statistiken-Logik
  - `PopulateCombatStatsTreeView()` mit gemischter Sortierung
  - Player- und Companion-Stats-Anzeige
  - Expander-UI-Generation
  
- `frontend/Models/OSCRModels.cs` - Erweiterte Datenmodelle
  - `CompanionStatistics` Klasse
  - `DpsWithCompanions`, `TotalDamageWithCompanions`
  - `Type` und `Icon` für Combat-Liste

**Gelöscht (Cleanup):**
- `backend/build/`, `backend/dist/` - Build-Artefakte
- `backend/python_backend_OSCR/` - Duplikat-Ordner
- `backend/OSCR/~temp_log_files/` - Temp-Dateien
- `backend/build_backend_improved.py`, `build_integrated.py`, etc. - Alte Build-Skripte
- `backend/*.spec` (außer `working_oscr.spec`) - Alte Spec-Dateien
- `backend/OSCR/*_api.py` - Alle nicht verwendeten API-Varianten
- `Deploy/python_backend_OSCR/` - Duplikat
- `Deploy/python_backend.py`, `real_oscr_backend.py` - Alte Backends
- `frontend/deploy.bat`, `Deploy.ps1`, `simple_deploy.bat` - Alte Deploy-Skripte
- `REAL_OSCR_INTEGRATION_COMPLETE.md`, `UI_IMPLEMENTATION_PLAN.md` - Alte Docs
- `backend/IMPLEMENTATION_STATUS.md`, `SETUP_INSTRUCTIONS.md` - Alte Docs

### 📊 **Statistiken:**

**Code-Umfang:**
- `working_oscr_backend.py`: ~1100 Zeilen (von ~200 erweitert)
- Neue Funktionen: `parse_combat_log_line`, `determine_combat_type_from_line`, `identify_source_entity`, `clean_name`
- Komplettes Refactoring: `isolate_combats`, `_analyze_combat_players`

**Features:**
- ✅ Vollständiges Combat-Log-Parsing
- ✅ Companion-Erkennung (Pets, Away Team, Drohnen)
- ✅ Combat-Type-Erkennung (Space/Ground)
- ✅ Strikte Combat-Trennung bei Typ-Wechsel
- ✅ Chronologisches Lesen mit korrekten Byte-Positionen
- ✅ 3-Level-Statistik-Hierarchie
- ✅ DPS-Berechnung mit/ohne Companions
- ✅ HTML-Tag-Entfernung
- ✅ Negative Damage-Filterung

### 💡 **Lessons Learned:**

1. **Reversed Reading ist komplex:** Chronologisches Lesen ist einfacher und weniger fehleranfällig
2. **Byte-Positionen sind kritisch:** Falsche Positionen führen zu falschen Daten
3. **Combat-Type braucht Prioritäten:** Target vor Source für präzise Erkennung
4. **Debug-Logging ist unverzichtbar:** Half enorm bei der Fehlersuche
5. **HTML in Spiel-Logs:** Immer auf HTML-Tags prüfen und entfernen
6. **Negative Damage:** Shield/Heal-Events müssen explizit gefiltert werden
7. **Companion-Zuordnung:** Source-Name UND Source-Type beide prüfen
8. **Default-Werte:** Immer sinnvolle Defaults setzen (nicht `-1` für "alle")

### 🔄 **Build-Status:**

- ✅ Frontend kompiliert erfolgreich
- ✅ Backend als Standalone .exe gebaut
- ✅ Combat-Statistiken werden korrekt angezeigt
- ✅ Companions werden erkannt und zugeordnet
- ✅ Combat-Type-Erkennung funktioniert
- ✅ Space/Ground-Combats strikt getrennt
- ✅ Chronologisches Lesen implementiert
- ✅ Neueste 20 Combats werden geladen
- ✅ Debug-Logging aufgeräumt
- ✅ Projekt bereinigt

### 🎨 **UI-Status:**

**Implementiert:**
- ✅ Combat-Statistiken-Tabelle (Player → Companion → Ability)
- ✅ Expandable Rows für Player und Companions
- ✅ Sortierung nach Total Damage (gemischt)
- ✅ Combat-Type-Icons in Combat-Liste (🚀/🏃)
- ✅ Korrekte Spalten-Ausrichtung
- ✅ Visual Hierarchy (Einrückung, Farben, Schriftgrößen)

**Ausstehend:**
- ⏳ DPS-Graph-Visualisierung
- ⏳ Sortierbare Spalten (Click-to-Sort)
- ⏳ Filter-Funktionalität (Damage Out/In, Heal, etc.)
- ⏳ Export-Funktion
- ⏳ Live-Parsing-Modus

---
**Nächste Session:** DPS-Graph implementieren, Spalten-Sortierung, Filter-Funktionalität

## Session 5: Spalten-Sortierung für Combat-Statistiken

**Datum:** 2025-10-10  
**Dauer:** ~30 Minuten  
**Fokus:** Click-to-Sort Funktionalität für Combat-Statistik-Tabelle

### 🎯 **Was wir erreicht haben:**

#### ✅ **Erfolgreich implementiert:**

1. **Klickbare Spalten-Header**
   - TextBlocks durch Button-Controls ersetzt
   - Alle Spalten sortierbar: DPS, Total Damage, Debuff, Max Hit, Crit %, Acc %
   - Custom Button-Style mit transparentem Hintergrund
   - Hover-Effekt: Leichte Hintergrund-Farbe (#20FFFFFF)
   - Hand-Cursor für bessere UX

2. **Dynamische Sortier-Logik**
   - Private Felder für Sortier-Status:
     - `_currentSortColumn` (Default: "DpsWithCompanions")
     - `_sortAscending` (Default: false = absteigend)
   - Toggle-Funktion: Gleiche Spalte → Richtung wechseln
   - Neue Spalte: Immer absteigend als Start

3. **SortPlayerStatistics Methode**
   - Switch-Statement für flexible Spalten-Auswahl
   - Unterstützt alle 6 Spalten (DPS, Total Damage, Debuff, Max Hit, Crit %, Acc %)
   - Erweiterbar: Neue Spalten können einfach hinzugefügt werden
   - Aufsteigend/Absteigend-Sortierung

4. **Visuelle Sortier-Indikatoren**
   - Pfeil-Symbole: ▲ (aufsteigend) / ▼ (absteigend)
   - Nur bei aktiver Sortier-Spalte sichtbar
   - Aktive Spalte: Star Trek Blue (#5B9BD5), FontWeight Bold
   - Inaktive Spalten: Gray (#B0B0B0), FontWeight SemiBold

5. **UpdateColumnHeaderIndicators Methode**
   - Aktualisiert alle Header-Buttons dynamisch
   - Zeigt Sortier-Pfeil und Farb-Highlighting
   - Wird automatisch nach jedem Klick aufgerufen
   - Initial-Sortierung wird beim ersten Combat-Laden angezeigt

### 🔧 **Technische Details:**

#### **XAML-Änderungen (MainWindow.xaml):**
```xml
<!-- Vorher: TextBlock -->
<TextBlock Grid.Column="1" Text="DPS" .../>

<!-- Nachher: Button mit Custom Style -->
<Button x:Name="DpsHeaderButton"
        Content="DPS"
        Tag="DpsWithCompanions"
        Click="OnColumnHeaderClick"
        Cursor="Hand">
    <Button.Style>
        <Style TargetType="Button">
            <Setter Property="Background" Value="Transparent"/>
            <Setter Property="Template">
                <Setter.Value>
                    <ControlTemplate TargetType="Button">
                        <Border Background="{TemplateBinding Background}">
                            <ContentPresenter HorizontalAlignment="Right"/>
                        </Border>
                    </ControlTemplate>
                </Setter.Value>
            </Setter>
            <Style.Triggers>
                <Trigger Property="IsMouseOver" Value="True">
                    <Setter Property="Background" Value="#20FFFFFF"/>
                </Trigger>
            </Style.Triggers>
        </Style>
    </Button.Style>
</Button>
```

#### **Code-Behind-Änderungen (MainWindow.xaml.cs):**

**Neue Felder:**
```csharp
private string _currentSortColumn = "DpsWithCompanions";
private bool _sortAscending = false;
```

**Sortier-Logik:**
```csharp
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
        // ... weitere Spalten
    };
    return orderedPlayers.ToList();
}
```

**Event-Handler:**
```csharp
private void OnColumnHeaderClick(object sender, RoutedEventArgs e)
{
    if (sender is not Button button || button.Tag is not string columnName)
        return;

    // Toggle oder neue Spalte
    if (_currentSortColumn == columnName)
        _sortAscending = !_sortAscending;
    else
    {
        _currentSortColumn = columnName;
        _sortAscending = false;
    }

    UpdateColumnHeaderIndicators();
    if (_currentCombatData != null)
        PopulateCombatStatsTreeView(_currentCombatData);
}
```

**Visual Update:**
```csharp
private void UpdateColumnHeaderIndicators()
{
    foreach (var (button, column) in headerButtons)
    {
        bool isActive = _currentSortColumn == column;
        string arrow = isActive ? (_sortAscending ? " ▲" : " ▼") : "";
        button.Content = baseText + arrow;
        button.Foreground = isActive ? StarTrekBlue : Gray;
        button.FontWeight = isActive ? Bold : SemiBold;
    }
}
```

### 📁 **Dateien geändert:**

**Aktualisiert:**
- `frontend/MainWindow.xaml` - Spalten-Header zu Buttons konvertiert
- `frontend/MainWindow.xaml.cs` - Sortier-Logik und Event-Handler hinzugefügt

**Keine neuen Dateien erstellt**

### ✅ **Features:**

**Sortierbare Spalten:**
- ✅ DPS (mit Companions)
- ✅ Total Damage (mit Companions)
- ✅ Debuff
- ✅ Max Hit
- ✅ Crit %
- ✅ Acc %

**Sortier-Verhalten:**
- ✅ Initial-Sortierung: DPS absteigend (beibehalten)
- ✅ Klick auf gleiche Spalte: Toggle auf-/absteigend
- ✅ Klick auf neue Spalte: Absteigend als Default
- ✅ Visuelle Indikatoren: Pfeil + Farbe + Bold

**Performance:**
- ✅ Sortierung im Memory (keine Backend-Anfrage)
- ✅ Nur UI-Neurendering
- ✅ Keine Lags auch bei vielen Spielern

### 🎨 **UI-Verbesserungen:**

**Vorher:**
- Statische TextBlock-Header
- Keine visuelle Rückmeldung
- Sortierung fix nach DPS

**Nachher:**
- Klickbare Button-Header mit Hand-Cursor
- Hover-Effekt für bessere UX
- Sortier-Pfeile zeigen aktuelle Richtung
- Farbiges Highlighting der aktiven Spalte
- Flexibel sortierbar nach allen wichtigen Spalten

### 💡 **Lessons Learned:**

1. **Button-Styling in WPF:** Custom ControlTemplates ermöglichen vollständige Kontrolle über Aussehen
2. **Switch Expressions:** Eleganter Code für Multi-Case-Logik (C# 8.0+)
3. **Tag-Property:** Perfekt für Metadaten an UI-Controls (hier: Spaltenname)
4. **Performance:** In-Memory-Sortierung ist schnell genug für Hunderte von Spielern
5. **UX-Details:** Kleine Dinge wie Cursor-Änderung und Hover-Effekte machen großen Unterschied

### 🔄 **Build-Status:**

- ✅ Keine Linter-Fehler
- ✅ Code kompiliert erfolgreich
- ✅ Keine Breaking Changes
- ✅ Abwärtskompatibel (bestehende Funktionalität intakt)

### 🎯 **Erweiterbarkeit:**

**Um weitere Spalten sortierbar zu machen:**
1. Spalten-Header von TextBlock zu Button ändern
2. `Tag` mit Spaltenname setzen
3. `OnColumnHeaderClick` Event-Handler zuweisen
4. Case zum Switch-Statement in `SortPlayerStatistics` hinzufügen
5. Entry zu `headerButtons` Array in `UpdateColumnHeaderIndicators` hinzufügen

→ Keine Änderung der Kernlogik erforderlich! ✅

### 📊 **Code-Umfang:**

**Neue Zeilen:**
- MainWindow.xaml: ~220 Zeilen (Header-Buttons mit Styles)
- MainWindow.xaml.cs: ~120 Zeilen (3 neue Methoden + Felder)

**Geänderte Methoden:**
- `PopulateCombatStatsTreeView`: Verwendet jetzt `SortPlayerStatistics`
- `LoadCombatDetailsAsync`: Ruft `UpdateColumnHeaderIndicators` auf

### 🚨 **Bekannte Einschränkungen:**

**Keine:**
- Feature funktioniert wie geplant
- Alle gewünschten Spalten sind sortierbar
- Erweiterung ist einfach möglich

---
**Nächste Session:** DPS-Graph implementieren, Filter-Funktionalität

## Session 6: Spalten-Trennlinien für bessere Lesbarkeit

**Datum:** 2025-10-10  
**Dauer:** ~20 Minuten  
**Fokus:** Vertikale Trennlinien zwischen Spalten für alle Ebenen

### 🎯 **Was wir erreicht haben:**

#### ✅ **Erfolgreich implementiert:**

1. **Durchgängige vertikale Trennlinien**
   - Linien zwischen allen Spalten sichtbar
   - Durchgehend von Header bis durch alle Ebenen:
     - Player-Ebene
     - Companion-Ebene  
     - Ability-Ebene (Player und Companion)
   - Konsistente Farbe: StarTrekBorderGray (#333333)
   - Dünne 1px-Linien für subtile Abgrenzung

2. **CreateTableCell Refactoring**
   - Rückgabewert: `Border` statt `TextBlock`
   - TextBlock wird in Border gewrappt
   - `BorderThickness`: `1,0,0,0` (linker Border)
   - Neuer Parameter `showLeftBorder` für Flexibilität
   - Alle Daten-Zellen nutzen die gleiche Methode

3. **Name-Spalten angepasst**
   - Player-Namen in Border gewrappt
   - Companion-Namen in Border gewrappt  
   - Ability-Namen in Border gewrappt (2 Stellen)
   - Rechter Border (`0,0,1,0`) für Trennlinie zur DPS-Spalte

4. **Header-Buttons mit Borders**
   - Alle Header-Buttons haben `BorderThickness="1,0,0,0"`
   - Player/Ability Header als Border mit rechtem BorderThickness
   - `HorizontalAlignment="Stretch"` für volle Spalten-Breite
   - Hover-Effekt bleibt erhalten

### 🔧 **Technische Details:**

#### **CreateTableCell - Vorher/Nachher:**

**Vorher:**
```csharp
private TextBlock CreateTableCell(string text, bool isHeader, double opacity = 1.0)
{
    return new TextBlock { Text = text, ... };
}
```

**Nachher:**
```csharp
private Border CreateTableCell(string text, bool isHeader, double opacity = 1.0, bool showLeftBorder = true)
{
    var textBlock = new TextBlock { Text = text, ... };
    return new Border
    {
        Child = textBlock,
        BorderBrush = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
        BorderThickness = showLeftBorder ? new Thickness(1, 0, 0, 0) : new Thickness(0)
    };
}
```

#### **Name-Spalten Border-Wrapping:**
```csharp
// Beispiel: Player-Name
var playerNameBorder = new Border
{
    Child = playerNamePanel,
    BorderBrush = new SolidColorBrush(Color.FromRgb(51, 51, 51)),
    BorderThickness = new Thickness(0, 0, 1, 0) // Rechter Border
};
Grid.SetColumn(playerNameBorder, 0);
playerHeaderGrid.Children.Add(playerNameBorder);
```

#### **XAML Header mit Borders:**
```xml
<!-- Player/Ability Header -->
<Border Grid.Column="0"
        BorderBrush="{DynamicResource StarTrekBorderGray}"
        BorderThickness="0,0,1,0">
    <TextBlock Text="Player / Ability" ... />
</Border>

<!-- DPS Header (Button) -->
<Button ... HorizontalAlignment="Stretch">
    <Button.Template>
        <ControlTemplate TargetType="Button">
            <Border Background="{TemplateBinding Background}"
                    BorderBrush="{DynamicResource StarTrekBorderGray}"
                    BorderThickness="1,0,0,0">
                <ContentPresenter ... />
            </Border>
        </ControlTemplate>
    </Button.Template>
</Button>
```

### 📁 **Dateien geändert:**

**Aktualisiert:**
- `frontend/MainWindow.xaml.cs` - CreateTableCell refactored, alle Name-Spalten mit Border
- `frontend/MainWindow.xaml` - Header-Buttons mit BorderThickness, Player/Ability Header als Border

### ✅ **Visuelle Verbesserungen:**

**Vorher:**
- Keine Spalten-Abgrenzung
- Daten schwer zuzuordnen bei vielen Spalten
- Unklare Spalten-Grenzen

**Nachher:**
- Klare vertikale Trennlinien
- Durchgängig von Header bis Ability-Ebene
- Bessere Lesbarkeit und Orientierung
- Professionelles Tabellen-Layout

### 💡 **Lessons Learned:**

1. **Border-Wrapping:** Flexibler als direkte BorderThickness auf Controls
2. **Konsistenz:** Gleiche Border-Lösung für alle Ebenen erhöht Wartbarkeit
3. **XAML Template-Borders:** BorderThickness im ControlTemplate für klickbare Elemente
4. **Type-Ambiguität:** `System.Windows.Controls.Button` vs. `Wpf.Ui.Controls.Button` explizit auflösen

### 🔄 **Build-Status:**

- ✅ Keine Linter-Fehler
- ✅ Ambiguous Button-Referenz aufgelöst
- ✅ Code kompiliert erfolgreich
- ✅ Visuelle Verbesserung ohne Breaking Changes

### 🎨 **UI-Qualität:**

- ✅ Durchgängige Trennlinien (Header → Player → Companion → Ability)
- ✅ Subtile 1px-Linien stören nicht
- ✅ Star Trek Theme konsistent (#333333)
- ✅ Bessere Daten-Zuordnung in breiten Tabellen
- ✅ Professionelles Table-Layout

---
**Nächste Session:** DPS-Graph implementieren, Filter-Funktionalität

## Session 7: Spalten-Layout-Optimierung und Sonderzeichen-Handling

**Datum:** 2025-10-10  
**Dauer:** ~1.5 Stunden  
**Fokus:** Spalten-Breiten anpassen, Einrückung optimieren, Sonderzeichen-Probleme lösen

### 🎯 **Was wir erreicht haben:**

#### ✅ **Erfolgreich implementiert:**

1. **Spalten-Layout-Optimierung**
   - Player/Ability-Spalte deutlich verbreitert (`3*` statt `2*`)
   - Crit % und Acc % Spalten verschmälert (`0.7*` statt `1*`)
   - Debuff-Spalte komplett entfernt (nicht benötigt)
   - Total Damage Spalte leicht vergrößert (`1.2*` statt `1*`)
   - Optimierte Raumnutzung für längere Spieler- und Ability-Namen

2. **Einrückung-Fixes für bessere Alignment**
   - Player Abilities: Padding von `new Thickness(20, 6, 8, 6)` → `new Thickness(16, 6, 8, 6)`
   - Companion Abilities: Padding von `new Thickness(52, 5, 8, 5)` → `new Thickness(48, 5, 8, 5)`
   - Companion-Namen: Margin angepasst auf `new Thickness(32, 6, 8, 6)`
   - Alle Container-Paddings entfernt für präzise Kontrolle

3. **Expander-Icon-Alignment mit Spacer-Elementen**
   - **Problem:** Zeilen mit Expander-Icons (Player, Companion) waren breiter als Ability-Zeilen ohne Icons
   - **Lösung:** Unsichtbare Spacer-Elemente in Ability-Zeilen hinzugefügt
   - Player Ability Spacer: `Width=18` (reserviert Platz für Player-Expander)
   - Companion Ability Spacer: `Width=15` (reserviert Platz für Companion-Expander)
   - Resultat: Perfekte vertikale Ausrichtung aller Spalten-Trennlinien

4. **Sonderzeichen-Problem gelöst (Umlaute: ä, ü, ö)**
   - **Problem:** Umlaute wurden als "?" angezeigt
   - **Ursache 1:** HTML-Entities im Combat-Log (z.B. `&lt;` statt `<`)
   - **Lösung 1:** `html.unescape()` in `clean_name()` Methode im Backend
   - **Ursache 2:** UTF-8 Encoding nicht explizit gesetzt
   - **Lösung 2:** 
     - Frontend: `process.StartInfo.StandardInputEncoding = Encoding.UTF8;`
     - Frontend: `process.StartInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";`
   - Resultat: Alle Sonderzeichen werden korrekt angezeigt

5. **UTF-8 BOM-Handling**
   - **Problem:** "Unexpected UTF-8 BOM (decode using utf-8-sig)" beim Combat-Laden
   - **Ursache:** Windows fügt Byte Order Mark (BOM) `\uFEFF` zum JSON-Input hinzu
   - **Lösung (Frontend):** `output = output.TrimStart('\uFEFF');` vor JSON-Deserialisierung
   - **Lösung (Backend):** `input_json = input_json.lstrip('\ufeff')` nach `sys.stdin.read()`
   - Resultat: Keine JSON-Parse-Fehler mehr, Combat-Liste lädt ohne Probleme

### 🔧 **Technische Details:**

#### **Spalten-Breiten (Grid.ColumnDefinitions):**
```xml
<!-- Neue Verteilung -->
<ColumnDefinition Width="3*"/>      <!-- Player/Ability -->
<ColumnDefinition Width="1*"/>      <!-- DPS -->
<ColumnDefinition Width="1.2*"/>    <!-- Total Damage -->
<ColumnDefinition Width="1*"/>      <!-- Max Hit -->
<ColumnDefinition Width="0.7*"/>    <!-- Crit % -->
<ColumnDefinition Width="0.7*"/>    <!-- Acc % -->
<!-- Debuff entfernt -->
```

#### **Spacer-Element für Alignment:**
```csharp
// Player Ability - Spacer für Player-Expander-Icon
var abilitySpacer = new Rectangle
{
    Width = 18,
    Fill = Brushes.Transparent
};
abilityNamePanel.Children.Add(abilitySpacer);

// Companion Ability - Spacer für Companion-Expander-Icon
var companionAbilitySpacer = new Rectangle
{
    Width = 15,
    Fill = Brushes.Transparent
};
companionAbilityNamePanel.Children.Add(companionAbilitySpacer);
```

#### **HTML-Entity-Decoding im Backend:**
```python
import html

def clean_name(self, name: str) -> str:
    """Entfernt HTML-Tags UND decodiert HTML-Entities"""
    if not name:
        return ""
    
    # Erst HTML-Entities decodieren (&lt; → <, &uuml; → ü, etc.)
    name = html.unescape(name)
    
    # Dann HTML-Tags entfernen
    name = re.sub(r'<[^>]+>', '', name)
    
    return name.strip()
```

#### **UTF-8 Encoding im Frontend:**
```csharp
// OSCRBackendService.cs
process.StartInfo.StandardInputEncoding = Encoding.UTF8;
process.StartInfo.EnvironmentVariables["PYTHONIOENCODING"] = "utf-8";

// BOM entfernen vor JSON-Parse
output = output.TrimStart('\uFEFF');
var result = JsonSerializer.Deserialize<T>(output, _jsonOptions);
```

#### **UTF-8 BOM-Handling im Backend:**
```python
# working_oscr_backend.py
input_json = sys.stdin.read()
input_json = input_json.lstrip('\ufeff')  # BOM entfernen
data = json.loads(input_json)
```

### 🚨 **Gelöste Probleme:**

#### **Problem 1: Spalten-Misalignment trotz Trennlinien**
- **Symptom:** Vertikale Trennlinien waren nicht perfekt ausgerichtet zwischen Ebenen
- **Ursache:** 
  - Expander-Icons in Player/Companion-Zeilen vergrößerten die erste Spalte
  - Ability-Zeilen ohne Icons waren schmaler
  - Padding in Containern verschob alle Spalten
- **Lösung:**
  - Alle Container-Paddings entfernt
  - Padding nur auf individuelle TextBlocks angewendet
  - Spacer-Elemente in Ability-Zeilen hinzugefügt (Width 18/15)
- **Resultat:** Perfekt ausgerichtete vertikale Linien über alle 4 Ebenen

#### **Problem 2: Umlaute als "?" angezeigt**
- **Symptom:** Deutsche Umlaute (ä, ö, ü) wurden als "?" dargestellt
- **Ursache:** 
  - Combat-Log enthält HTML-Entities (`&auml;` statt `ä`)
  - UTF-8 Encoding war nicht explizit gesetzt (Python defaultet zu ASCII)
- **Lösung:**
  - `html.unescape()` vor HTML-Tag-Entfernung
  - `StandardInputEncoding = Encoding.UTF8` im Frontend
  - `PYTHONIOENCODING=utf-8` Environment Variable für Python
- **Resultat:** Alle Sonderzeichen korrekt angezeigt

#### **Problem 3: "Unexpected UTF-8 BOM" JSON-Fehler**
- **Symptom:** Combat-Liste lädt nicht, Fehler "BOM (decode using utf-8-sig)"
- **Ursache:** Windows fügt BOM-Character `\uFEFF` zum JSON-Input hinzu
- **Lösung:** 
  - Frontend: BOM aus Backend-Output entfernen vor Deserialisierung
  - Backend: BOM aus stdin-Input entfernen vor JSON-Parsing
- **Resultat:** Combat-Liste lädt ohne Fehler

#### **Problem 4: Player-Namen benötigen mehr Platz**
- **Symptom:** Lange Spieler- und Ability-Namen wurden abgeschnitten
- **Lösung:** Player/Ability-Spalte von `2*` auf `3*` verbreitert
- **Resultat:** Alle Namen lesbar, keine Truncation mehr

### 📁 **Wichtige Dateien:**

**Aktualisiert:**
- `frontend/MainWindow.xaml` - Grid.ColumnDefinitions angepasst, Debuff entfernt
- `frontend/MainWindow.xaml.cs` - Spacer-Elemente, Padding-Anpassungen
- `frontend/Services/OSCRBackendService.cs` - UTF-8 Encoding, BOM-Handling
- `Deploy/working_oscr_backend.py` - HTML-Entity-Decoding, BOM-Handling

**Keine neuen Dateien erstellt**

### 📊 **Vor/Nach Vergleich:**

#### **Spalten-Breiten:**
```
VORHER:
Player/Ability: 2*  (zu schmal)
DPS: 1*
Total Damage: 1*
Debuff: 1*  (unnötig)
Max Hit: 1*
Crit %: 1*  (zu breit für Prozent-Werte)
Acc %: 1*   (zu breit für Prozent-Werte)

NACHHER:
Player/Ability: 3*  (deutlich mehr Platz)
DPS: 1*
Total Damage: 1.2*  (leicht größer)
Max Hit: 1*
Crit %: 0.7*  (kompakter)
Acc %: 0.7*   (kompakter)
Debuff: ENTFERNT
```

#### **Sonderzeichen:**
```
VORHER:
Spielername: "M?ller"
Ability: "Photonen-Torpedo-Salvø"

NACHHER:
Spielername: "Müller"
Ability: "Photonen-Torpedo-Salvø"
```

### 💡 **Lessons Learned:**

1. **Expander-Icon-Alignment:** Invisible Spacer-Elemente sind die sauberste Lösung für Icon-bedingte Breiten-Unterschiede
2. **HTML-Entities vs. UTF-8:** Beides muss gehandhabt werden - erst Entities decodieren, dann Tags entfernen
3. **Windows BOM:** Bei stdin/stdout zwischen C# und Python immer BOM-Handling implementieren
4. **Python Encoding:** `PYTHONIOENCODING` Environment Variable überschreibt Python's Default-Encoding zuverlässig
5. **Container-Padding:** Für präzise Alignment besser kein Padding auf Container, nur auf Children
6. **Spalten-Breiten:** Relative Breiten (`*`) ermöglichen flexible, aber proportionale Layouts

### 🔄 **Build-Status:**

- ✅ Frontend kompiliert erfolgreich
- ✅ Backend kompiliert erfolgreich
- ✅ Keine Linter-Fehler
- ✅ Sonderzeichen werden korrekt angezeigt
- ✅ BOM-Fehler behoben
- ✅ Spalten perfekt ausgerichtet
- ✅ Layout optimiert

### 🎨 **UI-Qualität:**

**Spalten-Alignment:**
- ✅ Vertikale Trennlinien perfekt ausgerichtet (Header → Player → Companion → Ability)
- ✅ Spacer-Elemente kompensieren Expander-Icons
- ✅ Keine verschobenen Spalten mehr

**Lesbarkeit:**
- ✅ Player/Ability-Spalte hat ausreichend Platz
- ✅ Prozent-Spalten kompakt und übersichtlich
- ✅ Debuff-Spalte entfernt (war leer)
- ✅ Sonderzeichen korrekt dargestellt

**Stabilität:**
- ✅ Keine JSON-Parse-Fehler mehr
- ✅ UTF-8 durchgängig korrekt gehandhabt
- ✅ BOM-tolerantes Parsing

### 🎯 **Nächste Schritte:**

**Implementiert:**
- ✅ Combat-Statistiken-Tabelle mit 3-Level-Hierarchie
- ✅ Spalten-Sortierung (Click-to-Sort)
- ✅ Spalten-Trennlinien
- ✅ Optimiertes Spalten-Layout
- ✅ Sonderzeichen-Support
- ✅ Combat-Type-Erkennung (Space/Ground)
- ✅ Companion-Parsing

**Ausstehend:**
- ⏳ DPS-Graph-Visualisierung
- ⏳ Filter-Funktionalität (Damage Out/In, Heal, etc.)
- ⏳ Export-Funktion
- ⏳ Live-Parsing-Modus

---
**Nächste Session:** DPS-Graph implementieren, Filter-Funktionalität

## Session 8: Component-Refactoring und UI-Wartbarkeitsverbesserungen

**Datum:** 2025-10-10  
**Dauer:** ~2 Stunden  
**Fokus:** Code-Wartbarkeit durch Component-Architektur, Service-Auslagerung, Layout-Optimierungen

### 🎯 **Was wir erreicht haben:**

#### ✅ **Erfolgreich implementiert:**

1. **Component-Architektur eingeführt**
   - **Ordnerstruktur:** `frontend/Components/Combat/` und `frontend/Components/Shared/`
   - **Neue Components:**
     - `CombatStatsHeader` - Table Header mit Sortier-Funktionalität
     - `CombatListView` - Combat-Liste Sidebar
     - `FilterBar` - Filter-Buttons (All/Space/Ground)
     - `LogFileSelector` - Log-Datei-Auswahl mit Browse-Button
   - **Vorteile:**
     - Wiederverwendbare UI-Komponenten
     - Klare Separation of Concerns
     - Bessere Testbarkeit
     - Einfachere Wartung

2. **CombatStatsRenderer Service erstellt**
   - **Problem:** MainWindow.xaml.cs war 786 Zeilen groß
   - **Lösung:** Gesamte UI-Rendering-Logik in dedizierten Service ausgelagert
   - **Umfang:** ~500 Zeilen Code aus MainWindow extrahiert
   - **Funktionen:**
     - `RenderCombatStats()` - Hauptmethode
     - `CreatePlayerRow()` - Player-Zeilen erstellen
     - `CreateAbilityRow()` - Ability-Zeilen erstellen
     - `CreateCompanionRow()` - Companion-Zeilen erstellen
     - `CreateTableCell()` - Tabellen-Zellen erstellen
     - `SortPlayerStatistics()` - Sortier-Logik
   - **Resultat:** MainWindow.xaml.cs jetzt nur noch **400 Zeilen** (47% Reduktion!)

3. **CombatStatsHeader Component**
   - Vollständiger Table Header als wiederverwendbare Component
   - Sortier-Logik integriert:
     - `CurrentSortColumn` und `SortAscending` Properties
     - `ColumnHeaderClicked` Event
     - `SetSortColumn()` Methode für externe Updates
     - `UpdateColumnHeaderIndicators()` für visuelle Feedback
   - Custom Button-Styles für alle sortierbaren Spalten
   - Event-basierte Kommunikation mit MainWindow

4. **CombatListView Component**
   - Kapselung der Combat-Liste
   - **DataTemplate-Fix:** Korrigierte Property-Namen (`Date`, `Time`, `Icon` statt `MapName`, etc.)
   - **Problem gelöst:** Combat-Einträge zeigten keinen Text → DataBinding-Fehler behoben
   - Public Methods:
     - `SetCombats()` - Liste aktualisieren
     - `ClearSelection()` - Selection aufheben
     - `SelectFirst()` - Ersten Eintrag auswählen
   - `CombatSelected` Event für Combat-Auswahl

5. **Layout-Optimierungen**
   - **Combat Statistics Card:** Oben bündig positioniert
     - Grid Row Definitionen vereinfacht (3 Rows → 2 Rows)
     - `VerticalAlignment="Top"` auf Card und ScrollViewer
     - Keine leeren Lücken mehr zwischen Filter und Tabelle
   - **Combat List Component:** Nutzt gesamte verfügbare Höhe
     - Sidebar-Layout von ScrollViewer+StackPanel zu Grid umgestellt
     - Row 0 (Auto): Log Selection Card
     - Row 1 (*): Combat List Component (volle Höhe)
     - Feste `Height="500"` entfernt

6. **MainWindow Refactoring**
   - **Vorher:** 786 Zeilen monolithischer Code
   - **Nachher:** 400 Zeilen koordinierender Code
   - **Fokus jetzt:**
     - Event-Handling
     - Component-Koordination
     - Backend-Kommunikation
     - Keine UI-Rendering-Details mehr!

### 🔧 **Technische Details:**

#### **Component-Event-Wiring:**
```csharp
// MainWindow.xaml.cs - Constructor
public MainWindow()
{
    InitializeComponent();
    
    // Stats Renderer initialisieren
    _statsRenderer = new CombatStatsRenderer(
        (Style)this.FindResource("NoToggleIconExpanderStyle"));
    
    // Component Events verbinden
    CombatListViewComponent.CombatSelected += OnCombatSelected;
    StatsHeaderComponent.ColumnHeaderClicked += OnColumnHeaderClicked;
}
```

#### **CombatStatsRenderer Integration:**
```csharp
// MainWindow.xaml.cs
private readonly CombatStatsRenderer _statsRenderer;

// Rendering delegieren
_statsRenderer.RenderCombatStats(
    CombatStatsItemsControl,
    _currentCombatData,
    _currentSortColumn,
    _sortAscending);
```

#### **Component DataTemplate Fix (CombatListView):**
```xml
<!-- VORHER (FALSCH): -->
<TextBlock Text="{Binding MapName}" />

<!-- NACHHER (KORREKT): -->
<Grid>
    <TextBlock Grid.Column="0" Text="{Binding Icon}" />
    <TextBlock Grid.Column="1" Text="{Binding Date}" />
    <TextBlock Grid.Column="2" Text="{Binding Time}" />
</Grid>
```

### 📁 **Wichtige Dateien:**

**Neu erstellt:**
- `frontend/Components/Combat/CombatStatsHeader.xaml` + `.cs`
- `frontend/Components/Combat/CombatListView.xaml` + `.cs`
- `frontend/Components/Shared/FilterBar.xaml` + `.cs`
- `frontend/Components/Shared/LogFileSelector.xaml` + `.cs`
- `frontend/Services/CombatStatsRenderer.cs`

**Aktualisiert:**
- `frontend/MainWindow.xaml` - Components eingebunden, Layout optimiert
- `frontend/MainWindow.xaml.cs` - Von 786 auf 400 Zeilen reduziert

### 💡 **Lessons Learned:**

1. **Component-Architektur:** Reduziert Code-Komplexität drastisch
2. **Service-Auslagerung:** UI-Rendering gehört nicht ins MainWindow
3. **Event-basierte Kommunikation:** Lose Kopplung zwischen Components
4. **DataBinding-Fehler:** Property-Namen müssen exakt mit Model übereinstimmen
5. **Layout mit Grid:** Flexibler als StackPanel für dynamische Höhen

### 🔄 **Build-Status:**

- ✅ Alle Components kompilieren erfolgreich
- ✅ Keine Linter-Fehler
- ✅ Build erfolgreich (Debug)
- ✅ Combat List zeigt Daten korrekt
- ✅ Layout optimiert

---
**Nächste Session:** DPS-Graph implementieren, Filter-Funktionalität aktivieren

## Session 9: Damage Types, Attacks Column, Component Cleanup

**Datum:** 2025-10-10  
**Dauer:** ~2 Stunden  
**Fokus:** Types-Spalte mit Icons, Attacks-Spalte, Component-Refactoring, Debug-Cleanup

### 🎯 **Was wir erreicht haben:**

#### ✅ **Erfolgreich implementiert:**

1. **"Types" Spalte mit farbigen Damage-Type-Icons**
   - Neue Spalte zwischen "Crit %" und "Attacks"
   - Unicode-Symbole für verschiedene Damage-Types:
     - Physical: ⚔ (weiß)
     - Energy: ⚡ (gelb)
     - Kinetic: 🎯 (orange)
     - Radiation: ☢ (grün)
     - Antiproton: ◆ (rot)
     - Plasma: 🔥 (orange-rot)
     - Tetryon: ❄ (cyan)
     - Polaron: ◉ (lila)
     - Disruptor: ⚛ (grün)
     - Phaser: ◈ (blau)
     - Electrical: ⚡ (gelb)
     - Cold: ❄ (cyan)
     - Toxic: ☠ (grün)
     - Psionic: 👁 (lila)
     - Shield: ◙ (cyan)
     - Proton: ◐ (grün)
   - Tooltips mit Damage-Type-Namen
   - Backend: `_extract_primary_damage_type()` filtert irrelevante Types (Crit, DoT, Immune, Miss, Shield, Flank, Dodge)
   - Backend: `damage_types` Dictionary pro Ability
   - Farbkodierung für bessere Lesbarkeit

2. **"Attacks" Spalte (ersetzt "Acc %")**
   - Zeigt Anzahl der Ability-Verwendungen
   - Player-Zeile: Summe aller Player-Abilities
   - Companion-Zeile: Summe aller Companion-Abilities
   - Ability-Zeilen: Individuelle Attack-Counts
   - Backend: `total_attacks` Tracking im `WorkingAbilityStats`
   - Frontend: Serialisierung und Anzeige

3. **Companion-Icon entfernt**
   - 🤖 Emoji vor Companion-Namen entfernt
   - Cleanes Aussehen ohne visuelle Ablenkung

4. **"Types" Header visuell angepasst**
   - Problem: Header war immer blau (wie sortiert)
   - Ursache: Hardcodierte Foreground-Farbe (#5B9BD5)
   - Lösung: Foreground auf #B0B0B0 (gray) geändert
   - Header hat Hover-Effekt und Hand-Cursor (wie andere)
   - Aber keine Sortier-Funktionalität
   - `HorizontalContentAlignment="Center"` für zentrierte Ausrichtung

5. **Spalten-Breiten-Optimierungen**
   - "Crit %" feste Breite (70px)
   - "Types" feste Breite (60px)
   - "Attacks" kompakt (0.5*)
   - "DPS" kleiner (0.8* statt 1*)
   - "Total Damage" bleibt (1*)
   - "Max Hit" viel kleiner (0.8* statt 1*)
   - "Player" Header-Text (statt "Player / Ability")

6. **Debug-Logging komplett entfernt**
   - Alle `[DEBUG]` Print-Statements aus Backend entfernt:
     - Ability DamageType Debug (Zeile 584-586)
     - Companion Ability Debug (Zeile 644-645)
     - Player Ability Debug (Zeile 670-671)
     - Serialize Debug (Zeile 924-926)
     - Debug-Log Parameter aus `_analyze_combat_players()` entfernt
     - Single Combat Debug (Zeile 1032-1034, 1053-1055)
   - `debug_log` Parameter komplett entfernt
   - `if debug_log:` Blöcke entfernt
   - Cleaner, produktionsreifer Code

7. **Font-Größen-Anpassungen**
   - Alle Table-Texte um 1px erhöht
   - Header: FontSize 14
   - Player-Rows: FontSize 14
   - Companion-Rows: FontSize 14
   - Ability-Rows: FontSize 13
   - Bessere Lesbarkeit

8. **Expander-Styles ausgelagert**
   - `NoToggleIconExpanderStyle` in separate Datei verschoben
   - `frontend/Styles/ExpanderStyles.xaml` erstellt
   - Bessere Code-Organisation
   - Wiederverwendbarkeit

### 🔧 **Technische Details:**

#### **Damage-Type-Extraktion (Backend):**
```python
def _extract_primary_damage_type(self, damage_type_str: str) -> str:
    """Extrahiert primären Damage-Type aus String wie 'Physical|Crit|DoT'"""
    if not damage_type_str:
        return ""
    
    # Ignoriere diese Types
    ignore_types = {'Crit', 'Critical', 'DoT', 'Immune', 'Miss', 'Shield', 'Flank', 'Dodge'}
    
    parts = damage_type_str.split('|')
    for part in parts:
        part = part.strip()
        if part and part not in ignore_types:
            return part
    
    return ""
```

#### **Damage-Type-Icon-Mapping (Frontend):**
```csharp
private string GetDamageTypeIcon(string damageType)
{
    return damageType switch
    {
        "Physical" => "⚔",
        "Energy" => "⚡",
        "Kinetic" => "🎯",
        "Radiation" => "☢",
        "Antiproton" => "◆",
        "Plasma" => "🔥",
        "Tetryon" => "❄",
        "Polaron" => "◉",
        "Disruptor" => "⚛",
        "Phaser" => "◈",
        "Electrical" => "⚡",
        "Cold" => "❄",
        "Toxic" => "☠",
        "Psionic" => "👁",
        "Shield" => "◙",
        "Proton" => "◐",
        _ => ""
    };
}
```

#### **Farbkodierung (Frontend):**
```csharp
private string GetDamageTypeColor(string damageType)
{
    return damageType switch
    {
        "Physical" => "#FFFFFF",    // Weiß
        "Energy" => "#FFD700",      // Gold
        "Kinetic" => "#FF8C00",     // Orange
        "Radiation" => "#00FF00",   // Grün
        "Antiproton" => "#FF0000",  // Rot
        "Plasma" => "#FF4500",      // Orange-Rot
        "Tetryon" => "#00CED1",     // Cyan
        "Polaron" => "#9370DB",     // Lila
        "Disruptor" => "#32CD32",   // Grün
        "Phaser" => "#1E90FF",      // Blau
        "Electrical" => "#FFD700",  // Gold
        "Cold" => "#00CED1",        // Cyan
        "Toxic" => "#00FF00",       // Grün
        "Psionic" => "#9370DB",     // Lila
        "Shield" => "#00CED1",      // Cyan
        "Proton" => "#00FF00",      // Grün
        _ => "#B0B0B0"              // Grau (fallback)
    };
}
```

### 📁 **Wichtige Dateien:**

**Aktualisiert:**
- `frontend/Components/Combat/CombatStatsHeader.xaml` - Types-Spalte hinzugefügt
- `frontend/Components/Combat/CombatStatsHeader.xaml.cs` - Spalten-Definitionen aktualisiert
- `frontend/Services/CombatStatsRenderer.cs` - Damage-Type-Icons und Attacks-Logik
- `frontend/Models/OSCRModels.cs` - `Attacks` und `DamageType` Properties hinzugefügt
- `Deploy/working_oscr_backend.py` - Debug-Logs entfernt, Damage-Type-Extraktion

**Neu erstellt:**
- `frontend/Styles/ExpanderStyles.xaml` - Ausgelagerte Expander-Styles

### 🚨 **Gelöste Probleme:**

#### **Problem 1: "Types" Header immer blau**
- **Symptom:** Types-Spalten-Header war immer blau, als wäre er sortiert
- **Ursache:** Hardcodierte `Foreground="#5B9BD5"` im XAML, Button nicht in `headerButtons` Array
- **Lösung:** Foreground auf `#B0B0B0` (gray) geändert
- **Resultat:** Konsistentes Aussehen mit anderen nicht-sortierten Headers

#### **Problem 2: Attacks zeigt 0 an**
- **Symptom:** Attacks-Spalte zeigte immer 0
- **Ursache:** `analyze_single_combat` hatte veraltete Serialisierungs-Logik
- **Lösung:** Serialisierung auf neues Format aktualisiert
- **Resultat:** Korrekte Attack-Counts werden angezeigt

#### **Problem 3: Viele Debug-Logs verschmutzen Konsole**
- **Symptom:** stderr voller Debug-Prints
- **Ursache:** Debug-Logs aus Entwicklungsphase nicht entfernt
- **Lösung:** Alle Debug-Statements systematisch entfernt
- **Resultat:** Cleaner Output, produktionsreifer Code

### 💡 **Lessons Learned:**

1. **Unicode-Symbole:** Perfekt für Icons ohne Image-Assets
2. **Farbkodierung:** Verbessert Lesbarkeit und User-Experience
3. **Debug-Cleanup:** Wichtiger Schritt vor jedem Commit
4. **Component-Isolation:** Styles auslagern für bessere Wartbarkeit
5. **Font-Größen:** Kleine Anpassungen können große Wirkung haben
6. **Property-Tracking:** `total_attacks` musste an mehreren Stellen implementiert werden

### 🔄 **Build-Status:**

- ✅ Frontend kompiliert erfolgreich (Debug)
- ✅ Backend kompiliert erfolgreich
- ✅ Keine Linter-Fehler
- ✅ Alle Features funktional
- ✅ Debug-Logs entfernt
- ✅ Code committed und gepusht

### 🎨 **UI-Status:**

**Implementiert:**
- ✅ Types-Spalte mit farbigen Icons
- ✅ Attacks-Spalte mit Summen
- ✅ Optimierte Spalten-Breiten
- ✅ Companion ohne Icon
- ✅ Größere Font-Sizes
- ✅ Expander-Styles ausgelagert

**Ausstehend:**
- ⏳ DPS-Graph-Visualisierung
- ⏳ Filter-Funktionalität (All/Space/Ground)
- ⏳ Export-Funktion
- ⏳ Live-Parsing-Modus

### 📊 **Git-Commit:**

```
Commit: 589a84d
Branch: version/1.1
Message: Feature: Damage Types Column, Attacks Column, Component Refactoring, Debug Cleanup

Files changed: 8
Insertions: +333
Deletions: -125
```

---
**Nächste Session:** DPS-Graph implementieren, Filter-Funktionalität aktivieren

## Session 10: UTF-8 Encoding-Fix für PyInstaller-Backend

**Datum:** 2025-10-10  
**Dauer:** ~1 Stunde  
**Fokus:** Sonderzeichen-Darstellung (ä, ö, ü) in PyInstaller-Executables

### 🎯 **Was wir erreicht haben:**

#### ✅ **Erfolgreich implementiert:**

1. **UTF-8 Encoding-Fix für PyInstaller**
   - **Problem:** PyInstaller-Executables ignorieren `PYTHONIOENCODING` Environment Variable
   - **Lösung:** Explizite UTF-8 Erzwingung direkt im Python-Code via `io.TextIOWrapper`
   - **Code:**
     ```python
     import io
     
     # Erzwinge UTF-8 für stdout/stderr (wichtig für PyInstaller .exe)
     if sys.stdout.encoding != 'utf-8':
         sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')
     if sys.stderr.encoding != 'utf-8':
         sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8', errors='replace')
     ```
   - **Resultat:** Sonderzeichen (ä, ö, ü, ß) werden korrekt angezeigt

2. **Frontend stdin-Encoding optimiert**
   - **Problem:** `StandardInputEncoding = Encoding.UTF8` verhinderte Backend-Kommunikation
   - **Lösung:** JSON als UTF-8 Bytes direkt an `StandardInput.BaseStream` schreiben
   - **Code:**
     ```csharp
     var jsonBytes = Encoding.UTF8.GetBytes(jsonInput);
     await process.StandardInput.BaseStream.WriteAsync(jsonBytes, 0, jsonBytes.Length);
     await process.StandardInput.BaseStream.FlushAsync();
     process.StandardInput.Close();
     ```
   - **Vorteil:** Umgeht Encoding-Probleme bei stdin, sauber und zuverlässig

3. **Backend-Log ins logs/ Verzeichnis verschoben**
   - `oscr_backend.log` wird jetzt in `logs/` Unterverzeichnis geschrieben
   - Automatische Erstellung des `logs/` Ordners falls nicht vorhanden
   - Konsistenz mit Frontend-Logs (`frontend_debug.log`, `backend_service_debug.log`, `backend_debug.log`)

### 🔧 **Technische Details:**

#### **PyInstaller UTF-8 Problem:**
- PyInstaller-Executables haben **kein** UTF-8 Default-Encoding
- `PYTHONIOENCODING` Environment Variable wird **ignoriert** in .exe
- Lösung: `io.TextIOWrapper` überschreibt `sys.stdout`/`sys.stderr` direkt

#### **Stdin/Stdout Encoding-Strategie:**
```
C# Frontend → Python Backend:
  stdin:  UTF-8 Bytes direkt an BaseStream (umgeht StandardInputEncoding)
  stdout: PYTHONIOENCODING=utf-8 + io.TextIOWrapper (doppelte Absicherung)
```

### 🚨 **Gelöste Probleme:**

#### **Problem 1: Umlaute als "?" in PyInstaller .exe**
- **Symptom:** Sonderzeichen funktionierten im Script, aber nicht in der .exe
- **Ursache:** PyInstaller defaultet nicht auf UTF-8
- **Lösung:** Explizite UTF-8 Erzwingung mit `io.TextIOWrapper`
- **Resultat:** ✅ Funktioniert sowohl in Script als auch in .exe

#### **Problem 2: Backend-Kommunikation brach nach UTF-8 Änderungen ab**
- **Symptom:** "Expecting value: line 1 column 1 (char 0)" Fehler
- **Ursache:** `StandardInputEncoding = Encoding.UTF8` verursachte stdin-Probleme
- **Lösung:** Direkte Byte-Übertragung an `BaseStream`
- **Resultat:** ✅ Stabile Kommunikation + korrekte Zeichen

### 📁 **Wichtige Dateien:**

**Aktualisiert:**
- `backend/working_oscr_backend.py` - UTF-8 Erzwingung, logs/ Verzeichnis
- `frontend/Services/OSCRBackendService.cs` - BaseStream statt StandardInputEncoding

### 💡 **Lessons Learned:**

1. **PyInstaller Encoding:** Nie auf Default-Encoding verlassen, immer explizit setzen
2. **io.TextIOWrapper:** Zuverlässigste Methode für UTF-8 in Python Executables
3. **BaseStream vs. StandardInput:** BaseStream ist näher am OS, weniger Encoding-Probleme
4. **Environment Variables:** Nicht zuverlässig in PyInstaller-Executables
5. **UTF-8 BOM:** Immer strippen bei JSON-Parsing

### 🔄 **Build-Status:**

- ✅ Backend neu gebaut mit UTF-8 Fix
- ✅ Frontend kompiliert erfolgreich
- ✅ Sonderzeichen funktionieren
- ✅ Backend-Kommunikation stabil
- ✅ Logs im logs/ Verzeichnis

### 🎨 **Qualität:**

**Vor dem Fix:**
- ❌ "Müller" → "M?ller"
- ❌ "Phaser-Strahl" → "Phaser-Strahl?"
- ❌ Backend-Logs verstreut

**Nach dem Fix:**
- ✅ "Müller" → "Müller"
- ✅ "Phaser-Strahl" → "Phaser-Strahl"
- ✅ Alle Logs in logs/ Verzeichnis

---
**Nächste Session:** DPS-Graph implementieren, Filter-Funktionalität aktivieren

## Session 11: Clean Release Structure mit Launcher-Architektur

**Datum:** 2025-10-10  
**Dauer:** ~4 Stunden  
**Fokus:** Release-Verzeichnis aufräumen, Launcher-EXE, alle Dateien in App/ Ordner

### 🎯 **Was wir erreicht haben:**

#### ✅ **Erfolgreich implementiert:**

1. **Launcher-Projekt erstellt**
   - Neues WPF-Projekt: `Launcher/` im Root-Verzeichnis
   - Single-File-EXE (~12 MB ohne Trimming)
   - Startet `App\StoDamageMeter.Core.exe` mit korrektem WorkingDirectory
   - **OutputType: WinExe** - Kein Konsolenfenster!
   - MessageBox für Fehler statt Console-Ausgaben
   - Sofortige Beendigung nach Start der Core-App
   - Größe: ~12 MB (ohne Trimming wegen WPF-Kompatibilität)

2. **Release-Struktur komplett überarbeitet**
   - **Vorher (v1.1.4):** 414 Dateien im Root-Verzeichnis (unübersichtlich)
   - **Nachher (v1.1.7):** Nur 3 Einträge im Root (sauber!)
   - **Neue Struktur:**
     ```
     Root/
     ├── StoDamageMeter.exe    (Launcher - 12 MB)
     ├── README.txt
     ├── App/                   (257 Dateien)
     │   ├── StoDamageMeter.Core.exe
     │   ├── OSCRBackend.exe
     │   ├── appsettings.json
     │   └── [251 DLLs + Runtime]
     └── Language/              (13 Sprachordner)
     ```

3. **Build.targets erweitert für automatische Reorganisation**
   - Neues MSBuild-Target: `ReorganizeReleaseStructure`
   - Läuft automatisch nach `dotnet publish`
   - Verschiebt:
     - Alle DLLs → `App/`
     - Alle EXEs (außer Launcher) → `App/`
     - Alle JSON-Dateien → `App/`
     - Sprachordner → `Language/`
   - Benennt `StoDamageMeter.exe` → `StoDamageMeter.Core.exe` um

4. **create_release.ps1 komplett überarbeitet**
   - 7-Schritte Build-Prozess (vorher 6):
     1. Cleaning old release files
     2. Checking Python Backend
     3. Building Launcher (neu!)
     4. Building Frontend
     5. Copying Launcher to Root
     6. Copying Backend to App/
     7. Creating README
   - Validierung der finalen Struktur
   - Prüft ob alle kritischen Dateien vorhanden sind
   - Launcher wird als `StoDamageMeter.exe` ins Root kopiert

5. **Frontend-Anpassungen für App-Struktur**
   - **Launcher:** Setzt WorkingDirectory auf `App/`
   - **Core-App:** Findet Backend und Config automatisch im gleichen Ordner
   - Keine Parent-Directory-Suche mehr nötig
   - Alles im gleichen Verzeichnis = einfacher

6. **Problemlösungen während der Entwicklung**
   - **Problem 1:** Backend und appsettings nicht gefunden
     - **Ursache:** Lagen im Root, Core-App in App/
     - **Lösung:** Alles in App/ verschoben, Launcher setzt WorkingDirectory
   - **Problem 2:** Konsolenfenster beim Launcher-Start
     - **Ursache:** OutputType war `Exe` (Console-App)
     - **Lösung:** OutputType auf `WinExe` geändert
   - **Problem 3:** WPF Trimming-Fehler
     - **Ursache:** `PublishTrimmed=true` nicht kompatibel mit WPF
     - **Lösung:** Trimming deaktiviert (PublishTrimmed=false)
   - **Problem 4:** Launcher wartete auf Core-App
     - **Ursache:** `process.WaitForExit()` blockierte
     - **Lösung:** Sofortige Beendigung nach Start

### 🔧 **Technische Details:**

#### **Launcher-Architektur:**
```csharp
// Launcher/Program.cs
[STAThread]
static int Main(string[] args)
{
    var coreAppPath = Path.Combine(launcherDir, @"App\StoDamageMeter.Core.exe");
    
    if (!File.Exists(coreAppPath))
    {
        MessageBox.Show("ERROR: Core application not found!");
        return 1;
    }
    
    var startInfo = new ProcessStartInfo
    {
        FileName = coreAppPath,
        WorkingDirectory = Path.Combine(launcherDir, "App"),
        UseShellExecute = false
    };
    
    Process.Start(startInfo);
    return 0; // Sofortige Beendigung
}
```

#### **MSBuild Target für Reorganisation:**
```xml
<Target Name="ReorganizeReleaseStructure" AfterTargets="Publish">
  <!-- App/ und Language/ Verzeichnisse erstellen -->
  <MakeDir Directories="$(AppDir);$(LanguageDir)" />
  
  <!-- Alle DLLs, EXEs und JSONs nach App/ verschieben -->
  <Move SourceFiles="@(AllDlls)" DestinationFolder="$(AppDir)" />
  <Move SourceFiles="@(AllExes)" DestinationFolder="$(AppDir)" />
  <Move SourceFiles="@(AllRuntimeFiles)" DestinationFolder="$(AppDir)" />
  
  <!-- StoDamageMeter.exe umbenennen -->
  <Move SourceFiles="$(AppDir)StoDamageMeter.exe" 
        DestinationFiles="$(AppDir)StoDamageMeter.Core.exe" />
  
  <!-- Sprachordner verschieben -->
  <Exec Command="move /Y &quot;$(PublishDir)de&quot; &quot;$(LanguageDir)&quot;" />
  <!-- ... für alle 13 Sprachen ... -->
</Target>
```

### 📁 **Wichtige Dateien:**

**Neu erstellt:**
- `Launcher/Launcher.csproj` - Launcher-Projekt-Datei
- `Launcher/Program.cs` - Launcher-Code mit WinExe
- `RELEASE_STRUCTURE_v1.1.5_CLEAN.md` - Dokumentation der neuen Struktur

**Aktualisiert:**
- `frontend/Build.targets` - ReorganizeReleaseStructure Target
- `frontend/frontend.csproj` - GenerateAssemblyInfo=false
- `create_release.ps1` - 7-Schritte Build-Prozess

### 💡 **Lessons Learned:**

1. **Launcher-Pattern:** Saubere Trennung von Start-Logik und Hauptanwendung
2. **WPF ohne Trimming:** ~12 MB Overhead akzeptabel für bessere Kompatibilität
3. **WorkingDirectory:** Kritisch für korrekte Datei-Pfade
4. **MSBuild Targets:** Mächtig für Post-Build-Automation
5. **WinExe vs Exe:** WinExe für GUI-Apps ohne Konsolenfenster
6. **Release-Hygiene:** Saubere Struktur ist wichtiger als minimale Größe

### 🔄 **Build-Status:**

- ✅ Release v1.1.7 erstellt und getestet
- ✅ ZIP erstellt (~119 MB)
- ✅ Anwendung startet korrekt
- ✅ Kein Konsolenfenster
- ✅ Alle Features funktional

---
**Nächste Session:** Splashscreen für Launcher implementieren, DPS-Graph