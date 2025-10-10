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
Write-Host "[1/6] Cleaning old release files..." -ForegroundColor Yellow
if (Test-Path $ReleaseVersionPath) {
    Remove-Item -Path $ReleaseVersionPath -Recurse -Force
}
New-Item -ItemType Directory -Path $ReleaseVersionPath -Force | Out-Null

# 2. Backend bauen
Write-Host "[2/6] Building Python Backend..." -ForegroundColor Yellow
Push-Location $BackendPath
try {
    python build_backend.py
    if ($LASTEXITCODE -ne 0) {
        throw "Backend build failed"
    }
} finally {
    Pop-Location
}

# 3. Frontend als Self-Contained Release bauen
Write-Host "[3/6] Building Frontend (Self-Contained Release)..." -ForegroundColor Yellow
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

# 4. Backend in Release kopieren
Write-Host "[4/6] Copying Backend..." -ForegroundColor Yellow
$BackendSource = Join-Path $BackendPath "dist\OSCRBackend.exe"
$BackendDest = Join-Path $ReleaseVersionPath "OSCRBackend.exe"
Copy-Item -Path $BackendSource -Destination $BackendDest -Force

# 5. Zusätzliche Dateien kopieren
Write-Host "[5/6] Copying additional files..." -ForegroundColor Yellow

# appsettings.json
$AppSettingsSource = Join-Path $FrontendPath "appsettings.json"
Copy-Item -Path $AppSettingsSource -Destination $ReleaseVersionPath -Force

# 6. README für Endbenutzer erstellen
Write-Host "[6/6] Creating README..." -ForegroundColor Yellow
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

# 7. PDB-Dateien löschen (falls vorhanden)
Write-Host "[Cleanup] Removing debug files..." -ForegroundColor Yellow
Get-ChildItem -Path $ReleaseVersionPath -Filter "*.pdb" -Recurse | Remove-Item -Force

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

Write-Host "Nächste Schritte:" -ForegroundColor Yellow
Write-Host "  1. Teste das Release: Führe 'StoDamageMeter.exe' im Release-Ordner aus" -ForegroundColor White
Write-Host "  2. Erstelle ZIP: Rechtsklick auf Ordner -> 'Senden an' -> 'ZIP-komprimierter Ordner'" -ForegroundColor White
Write-Host "  3. Verteile die ZIP-Datei an andere Spieler" -ForegroundColor White
Write-Host ""
Write-Host "Fertig! 🎉" -ForegroundColor Green

