# STO Damage Meter - Release Build Script
# Erstellt ein vollständiges, eigenständiges Release-Paket

param(
    [string]$Version = "1.1.0"
)

$ErrorActionPreference = "Stop"

Write-Host "=== STO Damage Meter - Release Builder ===" -ForegroundColor Cyan
Write-Host "Version: $Version" -ForegroundColor Green
Write-Host ""

# Pfade definieren
$ProjectRoot = $PSScriptRoot
$FrontendPath = Join-Path $ProjectRoot "frontend"
$BackendPath = Join-Path $ProjectRoot "backend"
$ReleasePath = Join-Path $ProjectRoot "Releases"
$ReleaseVersionPath = Join-Path $ReleasePath "StoDamageMeter_v$Version"

# 1. Alte Release-Dateien löschen
Write-Host "[1/7] Cleaning old release files..." -ForegroundColor Yellow
if (Test-Path $ReleaseVersionPath) {
    Remove-Item -Path $ReleaseVersionPath -Recurse -Force
}
New-Item -ItemType Directory -Path $ReleaseVersionPath -Force | Out-Null

# 2. Backend prüfen/bauen
Write-Host "[2/7] Checking Python Backend..." -ForegroundColor Yellow
$BackendExe = Join-Path $BackendPath "dist\OSCRBackend.exe"
if (Test-Path $BackendExe) {
    Write-Host "Backend already built: $BackendExe" -ForegroundColor Green
} else {
    Write-Host "Building Python Backend..." -ForegroundColor Yellow
    Push-Location $BackendPath
    try {
        python build_backend.py
        if ($LASTEXITCODE -ne 0) {
            throw "Backend build failed"
        }
    } finally {
        Pop-Location
    }
}

# 3. Launcher bauen
Write-Host "[3/7] Building Launcher..." -ForegroundColor Yellow
$LauncherPath = Join-Path $ProjectRoot "Launcher"
$LauncherOutputPath = Join-Path $ProjectRoot "temp_launcher"
Push-Location $LauncherPath
try {
    dotnet publish -c Release -r win-x64 --self-contained true `
        -p:PublishSingleFile=true `
        -p:DebugType=none `
        -p:DebugSymbols=false `
        -p:PublishTrimmed=false `
        -o "$LauncherOutputPath"
    
    if ($LASTEXITCODE -ne 0) {
        throw "Launcher build failed"
    }
} finally {
    Pop-Location
}

# 4. Frontend als Self-Contained Release bauen
Write-Host "[4/7] Building Frontend (Self-Contained Release)..." -ForegroundColor Yellow
Push-Location $FrontendPath
try {
    # Self-contained Build für Windows x64
    dotnet publish -c Release -r win-x64 --self-contained true `
        -p:PublishSingleFile=false `
        -p:DebugType=none `
        -p:DebugSymbols=false `
        -o "$ReleaseVersionPath"
    
    if ($LASTEXITCODE -ne 0) {
        throw "Frontend build failed"
    }
} finally {
    Pop-Location
}

# 5. Launcher ins Release kopieren
Write-Host "[5/7] Copying Launcher..." -ForegroundColor Yellow
$LauncherSource = Join-Path $LauncherOutputPath "Launcher.exe"
$LauncherDest = Join-Path $ReleaseVersionPath "StoDamageMeter.exe"
if (Test-Path $LauncherSource) {
    Copy-Item -Path $LauncherSource -Destination $LauncherDest -Force
    Write-Host "Launcher copied and renamed to StoDamageMeter.exe" -ForegroundColor Green
} else {
    throw "Launcher executable not found at $LauncherSource"
}

# 6. Backend in den App/ Ordner kopieren
Write-Host "[6/7] Copying Backend to App directory..." -ForegroundColor Yellow
$AppDir = Join-Path $ReleaseVersionPath "App"
$BackendSource = Join-Path $BackendPath "dist\OSCRBackend.exe"
$BackendDest = Join-Path $AppDir "OSCRBackend.exe"

if (-not (Test-Path $AppDir)) {
    New-Item -ItemType Directory -Path $AppDir -Force | Out-Null
}

Copy-Item -Path $BackendSource -Destination $BackendDest -Force
Write-Host "Backend copied to App directory" -ForegroundColor Green

# 7. README für Endbenutzer erstellen
Write-Host "[7/7] Creating README..." -ForegroundColor Yellow
$ReadmeContent = @"
# STO Damage Meter v$Version

## 🚀 Installation

1. **Entpacke** alle Dateien in einen beliebigen Ordner
2. **Starte** `StoDamageMeter.exe`
3. **Fertig!** Keine weiteren Installationen erforderlich

## 📋 Erste Schritte

1. **Combat Log Pfad auswählen:**
   - Standard-Pfad: `C:\Program Files (x86)\Star Trek Online\Star Trek Online\Live\logs\GameClient\combatlog.log`
   - Oder: Klicke auf "Browse" um einen anderen Pfad zu wählen

2. **Combat Log laden:**
   - Klicke auf "Analyze" um die letzten Combats zu laden
   - Die neuesten 20 Combats werden in der Liste angezeigt

3. **Combat auswählen:**
   - Klicke auf einen Combat in der Liste
   - Die Statistiken werden automatisch angezeigt

## 🎮 Features

- ✅ **Combat-Statistiken:** DPS, Total Damage, Crit %, Max Hit, Attacks, Damage Types
- ✅ **Player & Companions:** Detaillierte Aufschlüsselung aller Abilities
- ✅ **Space & Ground:** Automatische Erkennung des Combat-Typs
- ✅ **Sortierbar:** Klicke auf Spalten-Header zum Sortieren
- ✅ **Farbige Icons:** Damage Types mit visuellen Hinweisen

## 📊 Spalten-Erklärung

- **Player:** Spieler-Name und Ability-Namen
- **DPS:** Damage per Second (mit und ohne Companions)
- **Total Damage:** Gesamter verursachter Schaden
- **Max Hit:** Höchster Einzelschaden
- **Crit %:** Kritische Trefferchance
- **Types:** Damage-Type-Icons (⚔ Physical, ⚡ Energy, etc.)
- **Attacks:** Anzahl der Ability-Verwendungen

## ⚙️ Konfiguration

Die Datei `appsettings.json` kann bearbeitet werden für:
- Backend-Pfad anpassen
- Max. Anzahl Combats ändern (Standard: 20)
- Sekunden zwischen Combats anpassen (Standard: 30)
- Min. Lines pro Combat anpassen (Standard: 20)

## 🔧 Systemanforderungen

- **Betriebssystem:** Windows 10/11 (64-bit)
- **Festplatte:** ~150 MB freier Speicherplatz
- **RAM:** Minimal 2 GB
- **Keine** .NET Runtime oder Python Installation erforderlich!

## 📝 Version Info

**Version:** $Version  
**Build-Datum:** $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")  
**GitHub:** https://github.com/VanKhaos/StoDamageMeter

## 🐛 Probleme?

Bei Problemen bitte ein Issue auf GitHub erstellen:
https://github.com/VanKhaos/StoDamageMeter/issues

---
**Viel Spaß beim Messen deines DPS!** 🚀
"@

$ReadmePath = Join-Path $ReleaseVersionPath "README.txt"
Set-Content -Path $ReadmePath -Value $ReadmeContent -Encoding UTF8

# Cleanup
Write-Host "[Cleanup] Removing debug files and temporary files..." -ForegroundColor Yellow
Get-ChildItem -Path $ReleaseVersionPath -Filter "*.pdb" -Recurse | Remove-Item -Force -ErrorAction SilentlyContinue

# Temporären Launcher-Ordner löschen
if (Test-Path $LauncherOutputPath) {
    Remove-Item -Path $LauncherOutputPath -Recurse -Force
}

# Struktur validieren
Write-Host "[Validation] Checking release structure..." -ForegroundColor Yellow
$AppDir = Join-Path $ReleaseVersionPath "App"
$LanguageDir = Join-Path $ReleaseVersionPath "Language"
$CoreApp = Join-Path $AppDir "StoDamageMeter.Core.exe"
$LauncherApp = Join-Path $ReleaseVersionPath "StoDamageMeter.exe"
$BackendInApp = Join-Path $AppDir "OSCRBackend.exe"
$AppSettingsInApp = Join-Path $AppDir "appsettings.json"

$ValidationErrors = @()

if (-not (Test-Path $LauncherApp)) {
    $ValidationErrors += "Launcher not found: $LauncherApp"
}
if (-not (Test-Path $AppDir)) {
    $ValidationErrors += "App directory not found: $AppDir"
}
if (-not (Test-Path $CoreApp)) {
    $ValidationErrors += "Core application not found: $CoreApp"
}
if (-not (Test-Path $LanguageDir)) {
    $ValidationErrors += "Language directory not found: $LanguageDir"
}
if (-not (Test-Path $BackendInApp)) {
    $ValidationErrors += "Backend not found in App directory: $BackendInApp"
}
if (-not (Test-Path $AppSettingsInApp)) {
    $ValidationErrors += "appsettings.json not found in App directory: $AppSettingsInApp"
}

if ($ValidationErrors.Count -gt 0) {
    Write-Host ""
    Write-Host "=== Validation Errors ===" -ForegroundColor Red
    foreach ($error in $ValidationErrors) {
        Write-Host "  - $error" -ForegroundColor Red
    }
    throw "Release structure validation failed"
}

Write-Host "Release structure validated successfully!" -ForegroundColor Green

# Zusammenfassung
Write-Host ""
Write-Host "=== Release Build Complete! ===" -ForegroundColor Green
Write-Host ""
Write-Host "Release-Ordner:" -ForegroundColor Cyan
Write-Host "  $ReleaseVersionPath" -ForegroundColor White
Write-Host ""

# Dateigröße berechnen
$TotalSize = (Get-ChildItem -Path $ReleaseVersionPath -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
Write-Host "Gesamtgröße: $([math]::Round($TotalSize, 2)) MB" -ForegroundColor Cyan
Write-Host ""

# Datei-Anzahl
$FileCount = (Get-ChildItem -Path $ReleaseVersionPath -File -Recurse).Count
Write-Host "Dateien: $FileCount" -ForegroundColor Cyan
Write-Host ""

Write-Host "Release-Struktur:" -ForegroundColor Cyan
Write-Host "  Root:" -ForegroundColor White
Write-Host "    - StoDamageMeter.exe (Launcher)" -ForegroundColor Gray
Write-Host "    - README.txt" -ForegroundColor Gray
Write-Host "    - App/ (Core-Anwendung + Backend + Config + alle DLLs)" -ForegroundColor Gray
Write-Host "    - Language/ (Sprachressourcen)" -ForegroundColor Gray
Write-Host ""

Write-Host "Nächste Schritte:" -ForegroundColor Yellow
Write-Host "  1. Teste das Release: Führe 'StoDamageMeter.exe' im Release-Ordner aus" -ForegroundColor White
Write-Host "  2. Erstelle ZIP: .\create_release_zip.ps1 -Version '$Version'" -ForegroundColor White
Write-Host "  3. Verteile die ZIP-Datei an andere Spieler" -ForegroundColor White
Write-Host ""
Write-Host "Fertig! 🎉" -ForegroundColor Green

