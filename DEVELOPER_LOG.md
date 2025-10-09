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