# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [2.0.0] - 2025-10-14

### Added
- **Material Design in XAML Toolkit Integration**
  - Modern UI components with Material Design styling
  - Material Design icons and buttons throughout the application
  - Elevated cards with shadow effects for better visual hierarchy
  - Circular progress bars for loading indicators
- **Enhanced UI Components**
  - Material Design IconButtons for Pin/Close functionality
  - PackIcon components for consistent iconography
  - Material Design Cards for elevated content areas
  - Custom ListBox styling with proper hover/selection effects

### Changed
- **Complete UI Modernization**
  - Replaced WPF UI components with Material Design equivalents
  - Updated color scheme to maintain Star Trek theme with Material Design
  - Improved visual consistency across all windows and components
  - Enhanced user experience with modern Material Design patterns
- **Color System Overhaul**
  - Fixed #454545 Material Design default color issue
  - Implemented custom Material Design color overrides for dark theme
  - Maintained Star Trek blue (#5B9BD5) and orange (#FFA500) accent colors
  - Consistent dark color palette throughout the application
- **Combat List Improvements**
  - Custom ListBoxItem styling for proper hover/selection effects
  - Optimized padding and spacing for better readability
  - Enhanced visual feedback for user interactions

### Fixed
- **Material Design Color Issues**
  - Resolved #454545 default Material Design card background color
  - Implemented comprehensive color overrides for dark theme consistency
  - Fixed ListBox background transparency issues
- **UI Consistency**
  - Unified color scheme across all components
  - Consistent hover and selection effects
  - Proper Material Design elevation and shadow effects

### Technical
- **Dependencies**
  - Added MaterialDesignInXAML.Toolkit NuGet package
  - Updated App.xaml with Material Design theme configuration
  - Integrated Material Design 3 standards
- **Architecture**
  - Maintained existing functionality while modernizing UI
  - Preserved Star Trek theme identity
  - Enhanced maintainability with Material Design patterns

## [1.2.4] - 2025-10-11

### Added
- **Hybrid-Debouncing für Live-Parsing**
  - Erste Zeilen sofort verarbeitet (0ms) für schnelle Reaktion
  - Normale Zeilen gebatched (100ms) für Performance
  - Max-Delay (500ms) verhindert verpasste Zeilen
  - Finaler Flush bei Combat-Ende für 100% Datengenauigkeit

### Fixed
- **Live Combat vs Dashboard Unterschiede behoben**
  - FileWatcher verpasst keine Zeilen mehr durch optimiertes Debouncing
  - Identische DPS/Total Damage Werte zwischen Live Combat und Dashboard
  - Finale Flush-Mechanik erfasst alle ausstehenden Zeilen vor Combat-Abschluss

### Changed
- **Multi-Player-Datengenauigkeit dokumentiert**
  - README.md erklärt warum eigene Stats 100% akkurat sind
  - README.md erklärt warum andere Spieler-Daten abweichen können (Client-seitige Logs)
  - 5 Hauptgründe dokumentiert: Rendering-Distanz, Pets, DoT, Network, AoE

## [1.2.3] - 2025-10-11

### Added
- **Automatische Log-Rotation für alle Log-Dateien**
  - Backend: `RotatingFileHandler` mit max. 10 MB pro Datei, 3 Backups
  - Frontend Debug-Logs: Nur in DEBUG-Builds, max. 5-10 MB mit Rotation
  - Gesamt-Maximum: ~40 MB (Release) bzw. ~70 MB (Debug)
  - Verhindert unbegrenztes Log-Wachstum bei Dauernutzung

### Fixed
- **Log-Größen-Problem:** Verhindert mehrere GB große Log-Dateien
  - Entwickler hatte 10 GB `backend_service_debug.log` durch Dauernutzung
  - Endanwender sind jetzt geschützt vor Festplatz-Problemen
  - Logs rotieren automatisch bei Erreichen der Grenze
- **Release-ZIP-Größe:** Log-Dateien werden nicht mehr ins Release-Package inkludiert
  - `scripts\create_release.ps1` entfernt alle Logs vor ZIP-Erstellung
  - Kleinere und sauberere Release-Pakete

### Changed
- Backend-Logging nutzt `RotatingFileHandler` statt einfachen `FileHandler`
- Frontend Debug-Logs nur in DEBUG-Builds (via `#if DEBUG`)
- `.gitignore` erweitert um alle Log-Pattern
- `RELEASE_GUIDE.md` dokumentiert Log-Rotation-Feature

## [1.2.2] - 2025-10-11

### Added
- **STO Window Binding** (Optional)
  - Overlay nur sichtbar wenn Star Trek Online im Vordergrund ist
  - Overlay bleibt innerhalb der STO-Fenster-Grenzen
  - Automatisches Ein-/Ausblenden bei Tab-Wechsel (Alt+Tab)
  - 2 Sekunden Verzögerung beim Öffnen (Zeit um zu STO zu wechseln)
  - Nutzt Standard Windows-APIs (GetForegroundWindow, GetWindowRect)
  - EULA-konform: Keine Prozess-Manipulation, nur Fenster-Status
  - Window-Binding ist immer aktiviert (hardcoded)

### Fixed
- **Ability DPS Berechnung:** Korrigierte DPS-Werte für Abilities
  - Abilities nutzen jetzt echte Combat-Zeit statt fixe 60 Sekunden
  - Konsistente Berechnung mit Player und Companion DPS
  - Beispiel: 500k Damage in 255s = 1.961 DPS (vorher: 8.333 DPS ❌)
- **Live-Parsing immer aktiv:** Live-Parsing startet automatisch wenn Log geladen wird
  - Kein Tab-Wechsel zu Live Combat mehr nötig
  - Overlay zeigt immer aktuelle Daten, unabhängig vom Tab
  - Zuverlässiger und benutzerfreundlicher
- **Overlay Window Binding:** Overlay bleibt 2 Sekunden sichtbar beim Öffnen
  - Gibt User Zeit zu STO zu wechseln
  - Verhindert sofortiges Verschwinden bei Testing/Development
- **Overlay User-Interaktion-Schutz:** Overlay wird nicht versteckt während Benutzung
  - 3 Sekunden Grace-Period nach letzter Interaktion
  - Kein Auto-Hide beim Positionieren, Settings ändern, oder Klicken
  - Kein Auto-Hide wenn Settings-Popup offen ist

### Changed
- Live-Parsing läuft kontinuierlich nach Log-Laden (unabhängig von Tab-Auswahl)
- Overlay zeigt Live-Daten auch wenn Dashboard-Tab aktiv ist
- Verbessertes Logging für Window-Binding und Live-Parsing-Status
- FEATURES.md hinzugefügt für zukünftige Feature-Planung
- Launcher zeigt korrekte Version (1.2.2) im Splash-Screen

## [1.2.1] - 2025-10-11

### Fixed
- **Combat-Type Detection:** Korrigierte Combat-Type Klassifizierung (Space vs Ground)
  - Majority-Counting-Logik entfernt die False-Positives verursachte
  - Combat-Type wird jetzt durch ersten erkannten Gegnertyp bestimmt
  - Einfacherer und zuverlässigerer Erkennungs-Algorithmus
- **Live Combat Overlay:** Overlay löscht jetzt ordnungsgemäß nach Combat-Ende
  - PropertyChanged-Event wird jetzt korrekt gefeuert wenn Combat finalisiert
  - Spieler-Liste wird beim Empty-State gelöscht
  - Keine alten Combat-Daten mehr im Overlay angezeigt

### Changed
- Combat-Type Detection vereinfacht von ~25 auf 3 Zeilen Logik
- Robusterer und wartbarerer Code

## [1.2.0] - 2025-10-11

### Added
- **Live Combat Overlay Window**
  - Always-on-top Floating-Overlay zeigt Live-Combat-Daten
  - Zeigt Spielername, DPS und Total Damage in Echtzeit
  - Verschiebbar und größenveränderbar
  - Pin-Funktion zum Fixieren des Overlays
  - Anpassbare Schriftgröße (10-24px) mit Live-Vorschau
  - 15 spielerspezifische Farben zur einfachen Identifikation
  - Sanfte Rank-Wechsel-Animationen (250ms)
  - Demo-Modus für Testing und Development
- **Live Combat Tab**
  - Echtzeit-Combat-Parsing während des Spielens
  - Updates alle 0,5 Sekunden
  - Automatische Combat-Erkennung (45-Sekunden-Timeout)
  - Combat-Type-Wechsel-Erkennung (Space ↔ Ground)
  - Abgeschlossene Combats werden automatisch zur Combat-Liste hinzugefügt
- **GitHub Landing Page (README.md)**
  - Umfassende Dokumentation
  - Installations-Anleitung
  - Feature-Übersicht
  - Development-Setup-Guide

### Fixed
- Combat-Log-Backup und Trimming behält letzte 30 Combats
- File-Watcher startet korrekt nach Log-Trimming neu
- Overlay schließt korrekt wenn Hauptfenster geschlossen wird
- Konsolen-Fenster erscheint nicht mehr in Release-Builds

### Changed
- Standard Combat-Timeout erhöht von 30s auf 45s
- Schriftgrößen erhöht für bessere Lesbarkeit
- Overlay Initial-Höhe optimiert für 5 Spieler (200px)

## [1.1.7] - 2025-10-10

### Added
- Saubere Release-Struktur mit Launcher-Architektur
- App-Ordner enthält alle DLLs und Executables
- Language-Ordner für Lokalisierungs-Dateien
- Launcher mit animiertem Splashscreen

### Changed
- Vereinfachte Release-Struktur (nur 3 Items im Root)
- Professionelle Startup-Erfahrung mit Fade-Animationen

## [1.1.4] - 2025-10-09

### Added
- Combat-Statistiken mit 3-Level-Hierarchie (Player → Companion → Ability)
- Sortierbare Spalten (DPS, Total Damage, Max Hit, Crit %, Attacks)
- Combat-Type-Erkennung (Space vs Ground)
- Companion-Parsing (Pets, Away Team, Drones)
- Damage-Type-Visualisierung mit farbigen Icons
- Spalten-Trenner für bessere Lesbarkeit

### Fixed
- UTF-8-Encoding für Sonderzeichen (ä, ö, ü)
- Korrekte Datums-Parsing für Combat-Timestamps
- Zeit-basierte Combat-Erkennung (30-Sekunden-Lücken)

### Changed
- WPF-UI-Integration mit Fluent Design
- Star Trek Blue Theme (#5B9BD5)
- Backend als Standalone-Executable (kein Python erforderlich)

## [1.0.0] - 2025-01-27

### Added
- Erstes Release
- Basis-Combat-Log-Parsing
- Spieler-Statistik-Anzeige
- Dark Theme UI
- Backend-Integration mit OSCR-Parser


