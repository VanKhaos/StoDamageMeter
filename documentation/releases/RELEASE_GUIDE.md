# Release Guide - STO Damage Meter

Komplette Anleitung zum Erstellen und Veröffentlichen neuer Releases.

---

## 📋 Inhaltsverzeichnis

1. [Vorbereitung](#vorbereitung)
2. [Version updaten](#version-updaten)
3. [Release bauen](#release-bauen)
4. [GitHub Release erstellen](#github-release-erstellen)
5. [GitHub Pages updaten](#github-pages-updaten)
6. [Checkliste](#checkliste)

---

## 🔧 Vorbereitung

### Voraussetzungen

- Alle Änderungen sind committed
- Tests wurden durchgeführt
- CHANGELOG.md ist aktuell
- Branch: `version/1.x` oder `main`

---

## 📝 Version updaten

### 1. CHANGELOG.md aktualisieren

Füge einen neuen Eintrag für die neue Version hinzu:

```markdown
## [1.2.3] - 2025-10-15

### Added
- Neue Feature-Beschreibung
- Weiteres Feature

### Fixed
- Bugfix-Beschreibung
- Weiterer Bugfix

### Changed
- Änderungs-Beschreibung
```

### 2. Launcher-Version aktualisieren

**Datei:** `Launcher/SplashScreen.xaml`

```xml
<TextBlock Text="Version 1.2.3"
           FontSize="12"
           Foreground="{StaticResource StarTrekTextGray}"
           HorizontalAlignment="Center"
           Opacity="0.7"/>
```

### 3. GitHub Pages Version aktualisieren

**Option A: Mit Helper-Script**

```powershell
# update_page_version.ps1 öffnen und anpassen:
$oldVersion = "1.2.2"
$newVersion = "1.2.3"
$newDate = "15. Oktober 2025"

# Dann ausführen:
.\update_page_version.ps1
```

**Option B: Manuell**

In `docs/index.html` an **5 Stellen** die Version ändern:

1. **Zeile ~47:** `Download v1.2.2` → `Download v1.2.3`
2. **Zeile ~171:** `StoDamageMeter_v1.2.2.zip` → `StoDamageMeter_v1.2.3.zip`
3. **Zeile ~244:** `Neueste Version: 1.2.2` → `Neueste Version: 1.2.3`
4. **Zeile ~248:** `STO Damage Meter v1.2.2` → `STO Damage Meter v1.2.3`
5. **Zeile ~261:** `Download v1.2.2` → `Download v1.2.3`

Auch das Datum aktualisieren:
- **Zeile ~249:** `Veröffentlicht am 11. Oktober 2025` → `Veröffentlicht am 15. Oktober 2025`

### 4. README.md Version-Badge aktualisieren (optional)

```markdown
[![Version](https://img.shields.io/badge/Version-1.2.3-blue.svg)]
```

---

## 🏗️ Release bauen

### 1. Frontend bauen

```powershell
cd frontend
dotnet build -c Debug
cd ..
```

**Überprüfe:**
- ✅ Keine Compile-Fehler
- ✅ Launcher zeigt richtige Version

### 2. Release erstellen

```powershell
# Release-Ordner erstellen
.\scripts\create_release.ps1 -Version "1.2.3"

# ZIP-Datei erstellen
.\scripts\create_release_zip.ps1 -Version "1.2.3"
```

**Output:**
- `Releases/StoDamageMeter_v1.2.3/` (Ordner)
- `Releases/StoDamageMeter_v1.2.3.zip` (ZIP)

**Hinweis:** Das Script entfernt automatisch:
- ✅ Debug-Symbole (`.pdb` Dateien)
- ✅ Log-Dateien (`App/logs/*.log`)
- ✅ Temporäre Build-Dateien

### 3. Release testen

```powershell
# Release-Version starten
cd Releases/StoDamageMeter_v1.2.3
.\StoDamageMeter.exe
```

**Teste:**
- ✅ Splashscreen zeigt richtige Version
- ✅ Anwendung startet ohne Fehler
- ✅ Combat-Log kann geladen werden
- ✅ Live Combat funktioniert
- ✅ Overlay funktioniert
- ✅ Window-Binding funktioniert (falls aktiviert)

---

## 🚀 GitHub Release erstellen

### Option 1: Via GitHub Web-Interface (Empfohlen)

1. **Navigiere zu Releases:**
   ```
   https://github.com/VanKhaos/StoDamageMeter/releases
   ```

2. **Klicke auf "Draft a new release"**

3. **Fülle das Formular aus:**
   - **Choose a tag:** `v1.2.3` (erstellt neuen Git-Tag)
   - **Target:** `main` (Standard-Branch)
   - **Release title:** `v1.2.3 - Feature Name`
   - **Description:** 
     ```markdown
     ## 🎉 Änderungen in v1.2.3
     
     [Kopiere Inhalt aus CHANGELOG.md für diese Version]
     
     ### Download
     - [StoDamageMeter_v1.2.3.zip](...)
     
     ### Installation
     1. ZIP entpacken
     2. StoDamageMeter.exe starten
     3. Combat-Log auswählen
     ```

4. **Attach binaries:**
   - Drag & Drop: `Releases/StoDamageMeter_v1.2.3.zip`

5. **Optional:** ✅ Set as the latest release

6. **Klicke auf "Publish release"**

### Option 2: Via GitHub CLI

```powershell
# GitHub CLI installieren (falls nicht vorhanden)
winget install GitHub.cli

# Authentifizieren
gh auth login

# Release erstellen
gh release create v1.2.3 `
  --title "v1.2.3 - Feature Name" `
  --notes-file CHANGELOG.md `
  Releases/StoDamageMeter_v1.2.3.zip

# Optional: Als latest markieren
gh release create v1.2.3 `
  --title "v1.2.3 - Feature Name" `
  --notes-file CHANGELOG.md `
  --latest `
  Releases/StoDamageMeter_v1.2.3.zip
```

---

## 📤 Git Workflow

### 1. Änderungen commiten

```powershell
# Status prüfen
git status

# Alle Änderungen hinzufügen
git add .

# Commit erstellen
git commit -m "Release v1.2.3: Feature Name

- Added: Feature-Beschreibung
- Fixed: Bugfix-Beschreibung
- Changed: Änderungs-Beschreibung"
```

### 2. Pushen (Development-Branch)

```powershell
# Version-Branch pushen
git push origin version/1.2
```

### 3. In Main mergen

```powershell
# Zu main wechseln
git checkout main

# version/1.2 mergen
git merge version/1.2

# Main pushen
git push origin main

# Zurück zu version/1.2
git checkout version/1.2
```

---

## 🌐 GitHub Pages updaten

**Wichtig:** Die GitHub Pages Website aktualisiert sich **NICHT automatisch**!

### Workflow

1. **Version in `docs/index.html` anpassen** (siehe oben)
2. **Commiten und pushen:**
   ```powershell
   git add docs/index.html
   git commit -m "Update GitHub Pages to v1.2.3"
   git push origin main
   ```
3. **Warten:** GitHub Pages baut automatisch neu (1-2 Minuten)
4. **Überprüfen:** https://vankhaos.github.io/StoDamageMeter/

---

## ✅ Checkliste

Vor dem Release:

- [ ] CHANGELOG.md aktualisiert
- [ ] Launcher-Version aktualisiert (`Launcher/SplashScreen.xaml`)
- [ ] GitHub Pages Version aktualisiert (`docs/index.html`)
- [ ] README.md Version-Badge aktualisiert
- [ ] Release gebaut (`scripts\create_release.ps1`)
- [ ] Release-ZIP erstellt (`scripts\create_release_zip.ps1`)
- [ ] Release getestet (Anwendung funktioniert)
- [ ] Git committed & gepusht (beide Branches)

Release erstellen:

- [ ] GitHub Release erstellt (Web-Interface oder CLI)
- [ ] ZIP-Datei hochgeladen
- [ ] Als "Latest Release" markiert
- [ ] Release-Notes aus CHANGELOG kopiert

Nach dem Release:

- [ ] GitHub Pages aktualisiert und überprüft
- [ ] Release funktioniert (Download-Link testen)
- [ ] Community informiert (Discord, Reddit, etc.)

---

## 🛠️ Troubleshooting

### Problem: ZIP-Erstellung schlägt fehl

**Ursache:** Release-Anwendung läuft noch

**Lösung:**
```powershell
# Alle Prozesse beenden
taskkill /F /IM StoDamageMeter.exe

# Erneut versuchen
.\scripts\create_release_zip.ps1 -Version "1.2.3"
```

### Problem: Git-Merge-Konflikt

**Ursache:** Unterschiedliche Änderungen in main und version/1.x

**Lösung:**
```powershell
# Merge abbrechen
git merge --abort

# Manuell prüfen welche Dateien unterschiedlich sind
git diff main version/1.2

# Dateien manuell anpassen, dann erneut mergen
```

### Problem: GitHub Pages zeigt alte Version

**Ursache:** Cache oder Build läuft noch

**Lösung:**
1. Hard-Refresh im Browser: `Ctrl + F5`
2. GitHub Pages Build-Status prüfen:
   - Repository → Actions → pages-build-deployment
3. Warten (max. 5 Minuten)

### Problem: Release-Download gibt 404

**Ursache:** ZIP-Datei wurde nicht hochgeladen oder Release nicht published

**Lösung:**
1. Gehe zu Release → Edit
2. Stelle sicher dass ZIP-Datei unter "Assets" erscheint
3. Release muss "Published" sein (nicht "Draft")

### Problem: ZIP-Datei ist riesig (mehrere GB)

**Ursache:** Log-Dateien wurden nicht entfernt

**Lösung:**
```powershell
# Logs manuell entfernen
Remove-Item -Path "Releases\StoDamageMeter_v1.2.3\App\logs\*.log" -Force

# ZIP neu erstellen
.\scripts\create_release_zip.ps1 -Version "1.2.3"
```

**Prävention:** 
- Das `scripts\create_release.ps1` Script entfernt Logs automatisch ab Version 1.2.2+
- Ab Version 1.2.3: Log-Rotation implementiert (automatisch begrenzte Log-Größen)
  - Backend: max 10 MB pro Log, 3 Backup-Dateien
  - Frontend Debug-Logs: nur in DEBUG-Builds, max 5-10 MB mit Rotation

---

## 📚 Weitere Ressourcen

- [GitHub Releases Documentation](https://docs.github.com/en/repositories/releasing-projects-on-github)
- [GitHub Pages Documentation](https://docs.github.com/en/pages)
- [Semantic Versioning](https://semver.org/)
- [Keep a Changelog](https://keepachangelog.com/)

---

## 🎯 Quick Command Reference

```powershell
# Version updaten
.\update_page_version.ps1

# Release bauen
.\scripts\create_release.ps1 -Version "1.2.3"
.\scripts\create_release_zip.ps1 -Version "1.2.3"

# Git Workflow
git add .
git commit -m "Release v1.2.3"
git push origin version/1.2
git checkout main
git merge version/1.2
git push origin main
git checkout version/1.2

# GitHub Release (CLI)
gh release create v1.2.3 --title "v1.2.3" --notes-file CHANGELOG.md Releases/StoDamageMeter_v1.2.3.zip
```

---

**Hinweis:** Diese Anleitung gilt für STO Damage Meter ab Version 1.2.x

