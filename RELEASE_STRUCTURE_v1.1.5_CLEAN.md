# 📦 STO Damage Meter v1.1.5 - Saubere Release Struktur

**Build-Datum:** 10.10.2025  
**Gesamtgröße:** ~160.96 MB (entpackt) | ~74.07 MB (ZIP)  
**Anzahl Dateien:** 415

---

## 🎯 Hauptverzeichnis (Root) - ÜBERSICHTLICH!

Das Root-Verzeichnis enthält jetzt nur noch die wichtigsten Dateien:

```
StoDamageMeter_v1.1.5/
├── 📱 StoDamageMeter.exe          (~156 KB - Launcher)
├── 🔧 OSCRBackend.exe             (~7.7 MB - Python Backend)
├── ⚙️ appsettings.json            (Konfiguration)
├── 📄 README.txt                  (Benutzeranleitung)
├── 📁 logs/                       (Log-Dateien)
├── 📁 App/                        (Hauptanwendung + alle DLLs)
└── 📁 Language/                   (Sprachressourcen)
```

### ✨ Vorteile der neuen Struktur

- ✅ **Übersichtliches Root-Verzeichnis** - Nur 4 Dateien + 3 Ordner
- ✅ **Schneller Start** - Benutzer sehen sofort welche EXE gestartet werden muss
- ✅ **Cleanes Design** - Keine DLL-Flut mehr im Root
- ✅ **Professionell** - Ähnlich wie kommerzielle Software strukturiert

---

## 📂 App/ Verzeichnis (255 Dateien)

Enthält die gesamte .NET-Anwendung mit allen Bibliotheken:

### Hauptanwendung
```
StoDamageMeter.Core.exe         # Die eigentliche WPF-Anwendung
StoDamageMeter.dll              # Anwendungs-Bibliothek
StoDamageMeter.deps.json        # Dependency-Informationen
StoDamageMeter.runtimeconfig.json  # Runtime-Konfiguration
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
| **Hauptdateien** | 4 | Root |
| **Ordner** | 3 | Root |
| **Core-Anwendung** | 5 | App/ |
| **System DLLs** | ~250 | App/ |
| **Sprachordner** | 13 | Language/ |
| **Resource DLLs** | 156 (12 × 13) | Language/*/  |
| **Log-Dateien** | 0-4 | logs/ |
| **Gesamtdateien** | **415** | |

---

## 🚀 Wie funktioniert der Launcher?

### Launcher (StoDamageMeter.exe)

Der Launcher ist eine kleine (~156 KB), optimierte Single-File-Anwendung:

**Funktionen:**
1. Prüft ob `App\StoDamageMeter.Core.exe` existiert
2. Startet die Core-Anwendung
3. Leitet alle Argumente weiter
4. Zeigt Fehler an wenn Core-App nicht gefunden wird
5. Wartet auf Core-App und gibt Exit-Code zurück

**Vorteile:**
- Sehr klein (trimmed + single-file)
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
├── appsettings.json
├── README.txt
├── 240+ DLL-Dateien 📚📚📚
├── cs/ de/ es/ fr/ it/ ja/ ko/ pl/ pt-BR/ ru/ tr/ zh-Hans/ zh-Hant/
└── logs/
```

**Problem:** Unübersichtlich, zu viele Dateien im Root

### ✅ Neue Struktur (v1.1.5)

```
Root/ (7 Einträge)
├── StoDamageMeter.exe          (Launcher)
├── OSCRBackend.exe
├── appsettings.json
├── README.txt
├── logs/
├── App/                        (alle DLLs versteckt)
└── Language/                   (Sprachen organisiert)
```

**Vorteil:** Übersichtlich, professionell, cleanes Design!

---

## 💡 Technische Details

### Build-Prozess

1. **Launcher bauen:**
   - Single-File Publish
   - Trimmed (nur benötigte Code)
   - ~156 KB Größe

2. **Frontend bauen:**
   - Self-Contained Publish
   - Alle DLLs inkludiert
   - Build.targets verschiebt automatisch:
     - Alle DLLs → App/
     - Alle EXEs → App/ (außer OSCRBackend.exe)
     - Core-App → App/StoDamageMeter.Core.exe
     - Sprachordner → Language/

3. **Release zusammenstellen:**
   - Launcher als StoDamageMeter.exe im Root
   - Backend im Root
   - Konfiguration im Root
   - Alles andere organisiert in Unterordnern

### Automatische Reorganisation

Die Datei `frontend/Build.targets` enthält ein MSBuild-Target `ReorganizeReleaseStructure`:
- Läuft nach dem Publish
- Erstellt App/ und Language/ Ordner
- Verschiebt Dateien automatisch
- Benennt Core-App um

---

## 🔧 Systemanforderungen

- **Betriebssystem:** Windows 10/11 (64-bit)
- **Festplatte:** ~170 MB freier Speicherplatz
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
# 1. Release bauen (mit neuer Struktur)
.\create_release.ps1 -Version "1.1.6"

# 2. ZIP erstellen
.\create_release_zip.ps1 -Version "1.1.6"
```

### Neue Dateien im Projekt:

```
Launcher/                       # Neues Launcher-Projekt
├── Launcher.csproj
└── Program.cs

frontend/
├── Build.targets              # Erweitert: ReorganizeReleaseStructure
└── frontend.csproj            # GenerateAssemblyInfo=false

create_release.ps1             # Aktualisiert: Baut Launcher
```

---

## 🎉 Zusammenfassung

**v1.1.5 bringt:**
- ✅ Sauberes, übersichtliches Root-Verzeichnis
- ✅ Professionelle Launcher-Lösung
- ✅ Automatische Reorganisation beim Build
- ✅ Gleiche Funktionalität wie vorher
- ✅ Bessere Benutzererfahrung

**Größenvergleich:**
- v1.1.4: ~148 MB (entpackt), ~68 MB (ZIP)
- v1.1.5: ~161 MB (entpackt), ~74 MB (ZIP)

**Größenzunahme:** ~13 MB (durch Launcher-Overhead)  
**Vorteil:** Deutlich bessere Organisation und Benutzerfreundlichkeit!

---

**Build-System:** .NET 9.0 + PyInstaller  
**Plattform:** Windows x64  
**Release-Art:** Self-Contained mit Launcher  
**Komprimierung:** Optimal

