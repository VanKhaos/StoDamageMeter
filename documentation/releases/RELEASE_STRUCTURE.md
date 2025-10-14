# 📦 STO Damage Meter - Release Struktur

**Aktuelle Version:** v1.2.4+  
**Build-Datum:** 13.10.2025  
**Gesamtgröße:** ~170 MB (entpackt) | ~123 MB (ZIP)  
**Anzahl Dateien:** 441

---

## 🎯 Hauptverzeichnis (Root) - ÜBERSICHTLICH!

Das Root-Verzeichnis enthält jetzt nur noch die wichtigsten Dateien:

```
StoDamageMeter_v1.2.4/
├── 📱 StoDamageMeter.exe          (~12 MB - Launcher)
├── 📄 README.txt                  (Benutzeranleitung)
├── 📄 CHANGELOG.txt               (Versionshistorie)
├── 📁 Assets/                     (App-Icon)
│   └── app_icon.png
├── 📁 App/                        (Hauptanwendung + alle DLLs)
├── 📁 Language/                   (Sprachressourcen)
└── 📁 [Einige WPF-DLLs im Root]   (WPF-Core-Dateien)
```

### ✨ Vorteile der aktuellen Struktur

- ✅ **Übersichtliches Root-Verzeichnis** - Nur 2 Dateien + 3 Ordner + wenige DLLs
- ✅ **Schneller Start** - Benutzer sehen sofort welche EXE gestartet werden muss
- ✅ **Cleanes Design** - Die meisten DLLs sind im App/ Ordner versteckt
- ✅ **Professionell** - Ähnlich wie kommerzielle Software strukturiert

---

## 📂 App/ Verzeichnis (257 Dateien)

Enthält die gesamte .NET-Anwendung mit allen Bibliotheken:

### Hauptanwendung
```
StoDamageMeter.Core.exe         # Die eigentliche WPF-Anwendung
StoDamageMeter.dll              # Anwendungs-Bibliothek
StoDamageMeter.deps.json        # Dependency-Informationen
StoDamageMeter.runtimeconfig.json  # Runtime-Konfiguration
OSCRBackend.exe                 # Python Backend (7.7 MB)
# Konfiguration wurde entfernt - alle Einstellungen sind hardcoded
createdump.exe                  # .NET Crash Dump Tool
```

### Kategorien der enthaltenen DLLs

#### .NET Runtime (12 DLLs)
- coreclr.dll, clrjit.dll, clrgc.dll
- hostfxr.dll, hostpolicy.dll
- mscorlib.dll, netstandard.dll
- mscordaccore.dll, mscordbi.dll

#### WPF & UI (24 DLLs)
- PresentationCore.dll, PresentationFramework.dll
- WindowsBase.dll, System.Xaml.dll
- Wpf.Ui.dll (Fluent Design Library)
- UIAutomation*.dll (4 Dateien)
- 7 Theme-DLLs (Aero, Aero2, AeroLite, Classic, Fluent, Luna, Royale)

#### Microsoft Extensions (17 DLLs)
- Configuration, DependencyInjection, Logging
- FileProviders, FileSystemGlobbing
- Options, Primitives

#### System Bibliotheken (~190 DLLs)
- Collections, IO, Networking, Security
- XML, JSON, Reflection, Threading
- Diagnostics, Cryptography, etc.

#### Grafik & Rendering (6 DLLs)
- D3DCompiler_47_cor3.dll
- DirectWriteForwarder.dll
- wpfgfx_cor3.dll
- PenImc_cor3.dll
- vcruntime140_cor3.dll

---

## 🌍 Language/ Verzeichnis (13 Sprachen)

Enthält alle Sprachressourcen für die WPF-UI:

```
Language/
├── cs/          (Tschechisch)
├── de/          (Deutsch)
├── es/          (Spanisch)
├── fr/          (Französisch)
├── it/          (Italienisch)
├── ja/          (Japanisch)
├── ko/          (Koreanisch)
├── pl/          (Polnisch)
├── pt-BR/       (Brasilianisches Portugiesisch)
├── ru/          (Russisch)
├── tr/          (Türkisch)
├── zh-Hans/     (Vereinfachtes Chinesisch)
└── zh-Hant/     (Traditionelles Chinesisch)
```

Jeder Sprachordner enthält 12 Resource-DLLs:
- PresentationCore.resources.dll
- PresentationFramework.resources.dll
- PresentationUI.resources.dll
- ReachFramework.resources.dll
- System.Windows.Controls.Ribbon.resources.dll
- System.Windows.Input.Manipulations.resources.dll
- System.Xaml.resources.dll
- UIAutomationClient.resources.dll
- UIAutomationClientSideProviders.resources.dll
- UIAutomationProvider.resources.dll
- UIAutomationTypes.resources.dll
- WindowsBase.resources.dll

---

## 📊 Statistik

| Kategorie | Anzahl | Ort |
|-----------|--------|-----|
| **Hauptdateien** | 2 | Root |
| **Ordner** | 3 | Root |
| **WPF-Core-DLLs** | 6 | Root |
| **Core-Anwendung** | 7 | App/ |
| **System DLLs** | ~250 | App/ |
| **Sprachordner** | 13 | Language/ |
| **Resource DLLs** | 156 (12 × 13) | Language/*/  |
| **Assets** | 1 | Assets/ |
| **Gesamtdateien** | **441** | |

---

## 🚀 Wie funktioniert der Launcher?

### Launcher (StoDamageMeter.exe)

Der Launcher ist eine optimierte Single-File-Anwendung:

**Funktionen:**
1. Prüft ob `App\StoDamageMeter.Core.exe` existiert
2. Startet die Core-Anwendung
3. Leitet alle Argumente weiter
4. Zeigt Fehler an wenn Core-App nicht gefunden wird
5. Wartet auf Core-App und gibt Exit-Code zurück

**Vorteile:**
- Optimierte Größe (~12 MB)
- Schneller Start
- Benutzer starten immer die richtige Datei
- Professioneller Eindruck

### Core-Anwendung (App/StoDamageMeter.Core.exe)

Die eigentliche WPF-Anwendung mit allen Features:
- Combat Log Analyse
- DPS-Berechnung
- UI mit Statistiken
- Backend-Integration

---

## 🎯 Vergleich: Alt vs. Neu

### ❌ Alte Struktur (v1.1.4)

```
Root/ (414 Dateien)
├── StoDamageMeter.exe
├── OSCRBackend.exe
# appsettings.json wurde entfernt
├── README.txt
├── 240+ DLL-Dateien 📚📚📚
├── cs/ de/ es/ fr/ it/ ja/ ko/ pl/ pt-BR/ ru/ tr/ zh-Hans/ zh-Hant/
└── logs/
```

**Problem:** Unübersichtlich, zu viele Dateien im Root

### ✅ Aktuelle Struktur (v1.2.4+)

```
Root/ (7 Einträge + wenige DLLs)
├── StoDamageMeter.exe          (Launcher)
├── README.txt
├── CHANGELOG.txt
├── Assets/                     (App-Icon)
├── App/                        (alle DLLs versteckt)
├── Language/                   (Sprachen organisiert)
└── [6 WPF-Core-DLLs]           (nur die wichtigsten)
```

**Vorteil:** Übersichtlich, professionell, cleanes Design!

---

## 💡 Technische Details

### Build-Prozess

1. **Launcher bauen:**
   - Single-File Publish
   - Optimiert für WPF-Kompatibilität
   - ~12 MB Größe

2. **Frontend bauen:**
   - Self-Contained Publish
   - Alle DLLs inkludiert
   - Build.targets verschiebt automatisch:
     - Alle DLLs → App/
     - Alle EXEs → App/ (außer Launcher)
     - Core-App → App/StoDamageMeter.Core.exe
     - Sprachordner → Language/
     - Assets → Assets/

3. **Release zusammenstellen:**
   - Launcher als StoDamageMeter.exe im Root
   - Backend im App/ Ordner
   - Konfiguration im App/ Ordner
   - Alles andere organisiert in Unterordnern

### Automatische Reorganisation

Die Datei `frontend/Build.targets` enthält ein MSBuild-Target `ReorganizeReleaseStructure`:
- Läuft nach dem Publish
- Erstellt App/, Language/ und Assets/ Ordner
- Verschiebt Dateien automatisch
- Benennt Core-App um

---

## 🔧 Systemanforderungen

- **Betriebssystem:** Windows 10/11 (64-bit)
- **Festplatte:** ~180 MB freier Speicherplatz
- **RAM:** Minimal 2 GB
- **Keine** .NET Runtime oder Python Installation erforderlich!

---

## 📝 Installation & Start

### Für Endbenutzer:

1. **ZIP entpacken** in beliebigen Ordner
2. **Doppelklick auf `StoDamageMeter.exe`**
3. **Fertig!** Die Anwendung startet automatisch

### Was passiert beim Start:

```
StoDamageMeter.exe (Launcher)
    ↓
    Prüft: Existiert App\StoDamageMeter.Core.exe?
    ↓
    Ja → Startet Core-App
    ↓
    Core-App lädt alle DLLs aus App/
    ↓
    Core-App lädt Sprachressourcen aus Language/
    ↓
    Anwendung läuft!
```

---

## ⚙️ Für Entwickler

### Neues Release erstellen:

```powershell
# 1. Release bauen (mit aktueller Struktur)
.\scripts\create_release.ps1 -Version "1.2.5"

# 2. ZIP erstellen
.\scripts\create_release_zip.ps1 -Version "1.2.5"
```

### Projektstruktur:

```
Launcher/                       # Launcher-Projekt
├── Launcher.csproj
└── Program.cs

frontend/
├── Build.targets              # ReorganizeReleaseStructure
└── frontend.csproj            # GenerateAssemblyInfo=false

scripts\create_release.ps1     # Baut Launcher + Frontend
```

---

## 🎉 Zusammenfassung

**Aktuelle Struktur bringt:**
- ✅ Sauberes, übersichtliches Root-Verzeichnis
- ✅ Professionelle Launcher-Lösung
- ✅ Automatische Reorganisation beim Build
- ✅ Gleiche Funktionalität wie vorher
- ✅ Bessere Benutzererfahrung
- ✅ Assets-Ordner für App-Icons

**Größenvergleich:**
- v1.1.4: ~148 MB (entpackt), ~68 MB (ZIP)
- v1.2.4: ~170 MB (entpackt), ~123 MB (ZIP)

**Größenzunahme:** ~22 MB (durch .NET 9.0 und erweiterte Features)  
**Vorteil:** Deutlich bessere Organisation und Benutzerfreundlichkeit!

---

**Build-System:** .NET 9.0 + PyInstaller  
**Plattform:** Windows x64  
**Release-Art:** Self-Contained mit Launcher  
**Komprimierung:** Optimal
