# STO Damage Meter - Release ZIP Creator
# Erstellt eine ZIP-Datei aus dem Release-Ordner

param(
    [string]$Version = "1.1.0"
)

$ErrorActionPreference = "Stop"

Write-Host "=== STO Damage Meter - ZIP Creator ===" -ForegroundColor Cyan
Write-Host "Version: $Version" -ForegroundColor Green
Write-Host ""

# Pfade definieren
$ProjectRoot = $PSScriptRoot
$ReleasePath = Join-Path $ProjectRoot "Releases"
$SourcePath = Join-Path $ReleasePath "StoDamageMeter_v$Version"
$ZipPath = Join-Path $ReleasePath "StoDamageMeter_v$Version.zip"

# Releases-Ordner erstellen falls nicht vorhanden
if (-not (Test-Path $ReleasePath)) {
    New-Item -ItemType Directory -Path $ReleasePath -Force | Out-Null
}

# Prüfen ob Release-Ordner existiert
if (-not (Test-Path $SourcePath)) {
    Write-Host "ERROR: Release-Ordner nicht gefunden!" -ForegroundColor Red
    Write-Host "Bitte erst 'create_release.ps1' ausführen!" -ForegroundColor Yellow
    exit 1
}

# Alte ZIP-Datei löschen falls vorhanden
if (Test-Path $ZipPath) {
    Write-Host "Removing old ZIP file..." -ForegroundColor Yellow
    Remove-Item -Path $ZipPath -Force
}

# ZIP-Datei erstellen
Write-Host "Creating ZIP archive..." -ForegroundColor Yellow
Write-Host "Source: $SourcePath" -ForegroundColor Gray
Write-Host "Target: $ZipPath" -ForegroundColor Gray
Write-Host ""

try {
    # PowerShell 5.0+ Compress-Archive verwenden
    Compress-Archive -Path "$SourcePath\*" -DestinationPath $ZipPath -CompressionLevel Optimal
    
    Write-Host "=== ZIP Created Successfully! ===" -ForegroundColor Green
    Write-Host ""
    
    # ZIP-Größe anzeigen
    $ZipSize = (Get-Item $ZipPath).Length / 1MB
    Write-Host "ZIP-Datei:" -ForegroundColor Cyan
    Write-Host "  $ZipPath" -ForegroundColor White
    Write-Host "  Größe: $([math]::Round($ZipSize, 2)) MB" -ForegroundColor White
    Write-Host ""
    
    Write-Host "Bereit zur Verteilung! 🎉" -ForegroundColor Green
    Write-Host ""
    Write-Host "Die ZIP-Datei kann jetzt an andere Spieler weitergegeben werden." -ForegroundColor Yellow
    Write-Host "Spieler müssen nur:" -ForegroundColor Yellow
    Write-Host "  1. ZIP entpacken" -ForegroundColor White
    Write-Host "  2. StoDamageMeter.exe starten" -ForegroundColor White
    Write-Host "  3. Fertig!" -ForegroundColor White
    
} catch {
    Write-Host "ERROR: ZIP-Erstellung fehlgeschlagen!" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    exit 1
}

