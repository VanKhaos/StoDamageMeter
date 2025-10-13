# Build Debug Version and copy to Debug/ folder
# Usage: .\build_debug.ps1

Write-Host "=== Building STO Damage Meter (Debug Version) ===" -ForegroundColor Cyan

# Arbeitsverzeichnis (Script ist jetzt in scripts/ Ordner)
$RootPath = Split-Path $PSScriptRoot -Parent
$FrontendPath = Join-Path $RootPath "frontend"
$BackendPath = Join-Path $RootPath "backend"
$DebugPath = Join-Path $RootPath "Debug"

# 1. Backend bauen (falls noch nicht vorhanden)
Write-Host "`n[1/3] Checking Backend..." -ForegroundColor Yellow
$BackendExe = Join-Path $BackendPath "dist\OSCRBackend.exe"
if (-not (Test-Path $BackendExe)) {
    Write-Host "Backend not found, building..." -ForegroundColor Yellow
    Push-Location $BackendPath
    pyinstaller --clean working_oscr.spec
    Pop-Location
    
    if (-not (Test-Path $BackendExe)) {
        Write-Host "ERROR: Backend build failed!" -ForegroundColor Red
        exit 1
    }
}
Write-Host "Backend OK: $BackendExe" -ForegroundColor Green

# 2. Frontend Debug bauen
Write-Host "`n[2/3] Building Frontend (Debug)..." -ForegroundColor Yellow
Push-Location $FrontendPath
$buildResult = dotnet build frontend.csproj --configuration Debug 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Frontend build failed!" -ForegroundColor Red
    Write-Host $buildResult
    Pop-Location
    exit 1
}
Pop-Location
Write-Host "Frontend build completed" -ForegroundColor Green

# 3. Debug-Ordner ist bereits korrekt befüllt
Write-Host "`n[3/3] Debug build completed!" -ForegroundColor Yellow

# Debug-Ordner ist bereits durch dotnet build befüllt
# Backend wird automatisch durch Build.targets kopiert
Write-Host "Debug files are ready in: $DebugPath" -ForegroundColor Green

# README erstellen
$ReadmeContent = @"
# STO Damage Meter - Debug Build

Entwicklungsversion mit Debug-Symbolen und erweiterten Logs.

## Start
Starte die Anwendung mit: StoDamageMeter.exe

## Logs
- Frontend: logs\backend_service_debug.log
- Backend: logs\oscr_backend.log

## Debug-Builds aktualisieren
Im Projektverzeichnis ausführen:
.\build_debug.ps1

Build-Zeit: $(Get-Date -Format "yyyy-MM-dd HH:mm:ss")
"@

Set-Content -Path "$DebugPath\README.txt" -Value $ReadmeContent -Encoding UTF8

Write-Host "`n=== Build abgeschlossen! ===" -ForegroundColor Green
Write-Host "Debug-Version verfügbar in: $DebugPath" -ForegroundColor Cyan
Write-Host "Starten mit: .\Debug\StoDamageMeter.exe" -ForegroundColor Cyan

