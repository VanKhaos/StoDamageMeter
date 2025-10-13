# STO Damage Meter

<div align="center">
  <img src="DPS_Meter_Logo_White.png" alt="STO Damage Meter Icon" width="128" height="128">
  
  <p><strong>Ein moderner Combat Parser für Star Trek Online</strong></p>
  
  🌐 **[Website](https://vankhaos.github.io/StoDamageMeter/)** | **[Download](https://github.com/VanKhaos/StoDamageMeter/releases/latest)**
  
  [![Version](https://img.shields.io/badge/Version-1.2.2-blue.svg)](https://github.com/VanKhaos/StoDamageMeter/releases)
  [![Platform](https://img.shields.io/badge/Platform-Windows-lightgrey.svg)](https://github.com/VanKhaos/StoDamageMeter)
  [![.NET](https://img.shields.io/badge/.NET-9.0-512BD4.svg)](https://dotnet.microsoft.com/)
  [![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)
</div>

---

## 📖 Über das Projekt

**STO Damage Meter** ist ein leistungsstarker Combat-Log-Analyzer für Star Trek Online. Die Anwendung analysiert deine Combat-Logs in Echtzeit und bietet detaillierte Statistiken über Damage, DPS, Abilities und mehr.

### ✨ Hauptfeatures

- 🎯 **Live Combat Tracking** - Verfolge deine Kämpfe in Echtzeit
- 📊 **Detaillierte Statistiken** - Player, Companions, Abilities mit voller Hierarchie
- 🎨 **Live Combat Overlay** - Schwebendes Fenster mit Live-DPS-Daten
- 🌈 **Spieler-Farben** - 15 verschiedene Farben für bessere Übersicht
- ⚡ **Rank-Animationen** - Smooth Animationen bei DPS-Rang-Wechseln
- 🚀 **Space & Ground** - Automatische Erkennung von Combat-Typen
- 📈 **Sortierbare Spalten** - Sortiere nach DPS, Total Damage, Crit%, etc.
- 🎬 **Demo-Modus** - Teste das Overlay ohne ins Spiel zu gehen
- 💾 **Log-Trimming** - Automatisches Backup und Bereinigung alter Combats
- 🌙 **Dark Theme** - Modernes Star Trek-inspiriertes Design

---

## 🖼️ Screenshots

### Dashboard
*Detaillierte Combat-Statistiken mit expandable Player/Companion/Ability-Hierarchie*

### Live Combat
*Echtzeit-Tracking während des Kampfes*

### Live Combat Overlay
*Schwebendes, transparentes Overlay mit Player-Rankings*
- Anpassbare Schriftgröße (10-24px)
- Spieler-spezifische Farben
- Rank-Wechsel-Animationen
- Pin-Funktion für feste Positionierung

---

## ⚠️ Wichtige Hinweise zur Datengenauigkeit

### Deine eigenen Daten sind 100% akkurat

Die Anwendung liest deinen lokalen Combat-Log und zeigt **deine eigenen Statistiken** (DPS, Total Damage, etc.) **zu 100% korrekt** an.

### Andere Spieler-Daten können unvollständig sein

**Warum sehe ich für andere Spieler unterschiedliche Werte?**

Star Trek Online's Combat-Log ist **client-seitig**. Das bedeutet:

- **Dein Log enthält nur Events, die dein Game-Client "sieht"**
- Andere Spieler schreiben ihre eigenen Logs mit ihren sichtbaren Events

**Beispiel:**

- 🎮 Spieler A (an PC A) sieht für sich selbst: **20.000 DPS** ✅
- 🎮 Spieler B (an PC B) sieht für Spieler A: **10.000 DPS** ⚠️

**Gründe für Unterschiede:**

1. **Rendering-Distanz**
   - Events außerhalb deiner Sichtweite werden nicht geloggt
   - Spieler A's Angriffe weit weg von dir → nicht in deinem Log

2. **Pet/Companion Damage**
   - Du siehst 100% deiner eigenen Pet-Angriffe
   - Du siehst nur ~50% der Pet-Angriffe anderer Spieler

3. **DoT/HoT (Damage over Time)**
   - STO überträgt nicht alle DoT-Ticks an andere Clients
   - Plasma-Burn, Torpedo-Spread DoT werden teilweise gefiltert

4. **Network-Updates**
   - Bei Lag siehst du weniger Events von anderen Spielern
   - Schnelle Angriffe (Cannon Rapid Fire) werden "gebatched"

5. **AoE-Damage (Area of Effect)**
   - Area-Schaden wird nur für nahe Spieler vollständig geloggt

### Das ist kein Bug!

**Das ist eine Design-Entscheidung von Cryptic Studios:**
- ✅ Performance-Optimierung (nicht jedes Event an alle Clients senden)
- ✅ Network-Bandwidth sparen
- ✅ Jeder Spieler kann seine **eigenen** Daten perfekt tracken

**Alle STO Parser-Tools haben dieses Verhalten** (inkl. OSCR, SCM, etc.)

### Empfehlung

- 📊 **Vertraue deinen eigenen Statistiken** - sie sind 100% korrekt
- 👥 **Andere Spieler-Daten sind Schätzwerte** - für genaue Werte müssten sie ihren eigenen Parser nutzen
- 🤝 **Team-DPS vergleichen?** - Jeder Spieler sollte seinen eigenen Log analysieren und Ergebnisse teilen

---

## 📥 Installation

### Download

Lade die neueste Version aus dem [Releases-Bereich](https://github.com/VanKhaos/StoDamageMeter/releases/latest) herunter.

### Installation

1. **ZIP-Datei entpacken**
   ```
   StoDamageMeter_v1.2.0.zip → beliebiger Ordner
   ```

2. **Anwendung starten**
   ```
   Doppelklick auf StoDamageMeter.exe
   ```

3. **Combat-Log auswählen**
   - Browse-Button klicken
   - Navigiere zu: `Star Trek Online\Live\logs\GameClient\combatlog.log`
   - Oder: "Default"-Button für automatische Erkennung

**Fertig!** 🎉 Die Anwendung lädt automatisch deine letzten Combats.

---

## 💻 System-Anforderungen

### Minimum
- **OS:** Windows 10 (64-bit)
- **RAM:** 4 GB
- **.NET:** Nicht erforderlich (Self-Contained)
- **Speicher:** 300 MB

### Empfohlen
- **OS:** Windows 11
- **RAM:** 8 GB
- **Speicher:** 500 MB (für Logs)

---

## 🎮 Verwendung

### Combat-Log-Aktivierung in STO

1. Starte Star Trek Online
2. Tippe im Chat: `/combatlog 1`
3. Combat-Log wird aktiviert
4. Logfile: `Star Trek Online\Live\logs\GameClient\combatlog.log`

### Tabs

#### **Dashboard**
- Zeigt historische Combats aus der Combat-Liste
- Klick auf Combat → Detaillierte Statistiken
- Sortierbare Spalten (Click-to-Sort)
- Expandable Player/Companion/Ability-Hierarchie

#### **Live Combat**
- Echtzeit-Tracking während des Kampfes
- Automatische Combat-Erkennung (45 Sekunden Timeout)
- Combat-Type-Erkennung (Space/Ground)
- "Show Overlay"-Button für schwebendes Fenster

### Live Combat Overlay

**Features:**
- **Demo-Button** (🎬): Simulation ohne Spiel für Testing
- **Settings-Button** (⚙): Schriftgröße anpassen (10-24px)
- **Pin-Button** (📍): Overlay fixieren/entsperren
- **Transparent**: Sieht durch zum Spiel
- **Always-on-Top**: Bleibt über allen Fenstern

**Spieler-Farben:**
- Jeder Spieler hat eine feste Farbe
- 15 verschiedene Farben (keine Wiederholung bei Standard-Teams)
- Erste 5 maximal unterscheidbar: Blau, Grün, Orange, Lila, Rot

**Animationen:**
- Rank-Wechsel werden animiert (250ms)
- Scale-Effekt für bessere Sichtbarkeit
- Nur der Spieler mit Rank-Änderung wird animiert

---

## 🔧 Features im Detail

### Combat-Erkennung
- **Zeitbasiert**: Neue Combats nach 45 Sekunden Pause
- **Type-Change**: Space ↔ Ground Wechsel startet neuen Combat
- **Map-Detection**: Automatische Map-Erkennung
- **Min Lines**: Mindestens 20 Zeilen pro Combat

### Statistiken
- **Player Stats**: DPS, Total Damage, Max Hit, Crit%, Attacks
- **Companion Support**: Away Team, Pets, Drohnen
- **Ability Details**: Jede Ability mit eigenen Stats
- **Damage Types**: Farbige Icons für Physical, Energy, Kinetic, etc.

### Log-Management
- **Auto-Backup**: Backup bei jedem Laden
- **Auto-Trim**: Nur letzte 30 Combats behalten
- **Rolling Window**: Alte Combats werden automatisch entfernt
- **Backup-Ausschluss**: Dateien mit "backup" werden nicht getrimmt

---

## 🛠️ Entwicklung

### Projekt-Struktur
```
StoDamageMeter/
├── frontend/          # WPF Frontend (C#, .NET 9)
│   ├── Components/    # UI-Komponenten
│   ├── Services/      # Backend-Kommunikation, FileWatcher
│   ├── ViewModels/    # MVVM ViewModels
│   └── Assets/        # Icons, Bilder
├── backend/           # Python Backend (PyInstaller)
│   └── working_oscr_backend.py
├── Launcher/          # WPF Launcher (Splashscreen)
└── Releases/          # Release-Builds
```

### Build-Befehle

**Debug-Build:**
```powershell
.\scripts\build_debug.ps1
```
- Output: `Debug/`
- Mit Konsole für Logs
- Debug-Symbole (.pdb)

**Release-Build:**
```powershell
.\scripts\create_release.ps1
```
- Output: `Releases/StoDamageMeter_v1.x.x/`
- Ohne Konsole
- Self-Contained (.NET Runtime inkludiert)

**Release-ZIP:**
```powershell
.\scripts\create_release_zip.ps1 -Version "1.2.0"
```

### Technologie-Stack

**Frontend:**
- .NET 9.0 (WPF)
- WPF-UI Library (Fluent Design)
- Microsoft.Extensions (DI, Logging, Configuration)

**Backend:**
- Python 3.14
- PyInstaller (Standalone .exe)
- NumPy (optional)

**Tools:**
- PowerShell (Build-Scripts)
- Git (Version Control)

---

## 📝 Changelog

### v1.2.0 (2025-10-11)
#### Neue Features
- ✨ **Live Combat Overlay** mit transparentem Design
- 🎨 **15 spieler-spezifische Farben** (konsistent, transparent)
- ⚡ **Rank-Wechsel-Animationen** (250ms, smooth)
- 🎬 **Demo-Button** für Overlay-Testing ohne Spiel
- 📏 **Anpassbare Schriftgröße** (10-24px) im Overlay
- 📍 **Pin-Funktion** für Overlay (fixiert/draggable)
- ⚙️ **Settings-Popup** für Overlay-Konfiguration

#### Verbesserungen
- 🔧 Overlay-Höhe optimiert (200px für 5 Spieler)
- 🔧 Schriftgröße erhöht (11px → 16px Standard)
- 🐛 Kein Konsolenfenster mehr in Release-Version

### v1.1.7 (2025-10-10)
- Launcher mit Splashscreen
- Clean Release-Struktur (App/ und Language/ Ordner)

### v1.1.0 - v1.1.6
- Live Combat Tab implementiert
- Combat-Statistiken mit Hierarchie
- Sortierbare Spalten
- Companion-Parsing
- Combat-Type-Erkennung
- UTF-8 Encoding-Fixes

---

## 🤝 Credits

### Basiert auf
- **OSCR (Open STO Combat Reparser)** - Combat-Log-Parsing-Logik

### Bibliotheken
- **WPF-UI** - Fluent Design für WPF
- **Microsoft.Extensions** - Dependency Injection, Logging
- **PyInstaller** - Python → Standalone EXE

---

## 🔒 EULA-Konformität & Sicherheit

**STO Damage Meter ist vollständig EULA-konform** und sicher in der Verwendung.

### Was das Tool macht:
- ✅ Liest nur die vom Spiel erstellte `combatlog.log` Textdatei
- ✅ Parst die Daten außerhalb des Spielprozesses
- ✅ Zeigt Statistiken in separatem Fenster an

### Was das Tool NICHT macht:
- ❌ **Kein Memory-Reading** - Greift nicht auf Spielspeicher zu
- ❌ **Keine Code-Injection** - Modifiziert das Spiel nicht
- ❌ **Kein Prozess-Hooking** - Keine Interaktion mit STO-Prozess
- ❌ **Kein Gameplay-Vorteil** - Zeigt nur bereits sichtbare Kampfdaten

### Window-Binding (Optional)
Das Overlay-Window-Binding-Feature nutzt ausschließlich **Standard Windows-APIs**:
- `GetForegroundWindow()` - Ermittelt aktives Fenster
- `GetWindowRect()` - Liest Fenster-Position
- **Vergleichbar mit:** Discord Overlay, OBS, Task Manager

Dies sind öffentliche Windows-APIs die von tausenden Programmen genutzt werden.

### Community-Tools
STO Damage Meter verwendet die gleiche Parsing-Logik wie etablierte Community-Tools:
- **OSCR** (Open STO Combat Reparser)
- **CLR** (Combat Log Reader)

Diese Tools werden seit Jahren von der Community genutzt ohne Probleme.

### Zusammenfassung
- ✅ **100% EULA-konform** - Keine Manipulation des Spiels
- ✅ **Keine Bans** - Liest nur öffentliche Log-Dateien
- ✅ **Community-bewährt** - Basiert auf OSCR
- ✅ **Open Source** - Vollständig einsehbarer Code

---

## 📄 Lizenz

Dieses Projekt ist unter der **MIT License** lizenziert.

---

## 🐛 Bug Reports & Feature Requests

Bitte erstelle ein [GitHub Issue](https://github.com/VanKhaos/StoDamageMeter/issues) für:
- 🐛 Bug Reports
- 💡 Feature Requests
- 📝 Verbesserungsvorschläge

---

## ⭐ Support

Wenn dir das Projekt gefällt, gib ihm einen Stern! ⭐

---

<div align="center">
  <p>Entwickelt mit ❤️ für die Star Trek Online Community</p>
  <p>
    <a href="https://github.com/VanKhaos/StoDamageMeter/releases">Downloads</a> •
    <a href="https://github.com/VanKhaos/StoDamageMeter/issues">Issues</a> •
    <a href="DEVELOPER_LOG.md">Developer Log</a>
  </p>
</div>

