# Build Debug Version and copy to Debug/ folder
# Usage: .\build_debug.ps1

Write-Host "=== Building STO Damage Meter (Debug Version) ===" -ForegroundColor Cyan

# Arbeitsverzeichnis
$RootPath = $PSScriptRoot
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

# 3. Dateien in Debug/ kopieren
Write-Host "`n[3/3] Copying files to Debug/..." -ForegroundColor Yellow

# Debug-Ordner leeren
if (Test-Path $DebugPath) {
    Remove-Item -Path "$DebugPath\*" -Recurse -Force
}

# Frontend-Dateien kopieren
$FrontendDebugPath = Join-Path $FrontendPath "bin\Debug\net9.0-windows"
if (Test-Path $FrontendDebugPath) {
    Copy-Item -Path "$FrontendDebugPath\*" -Destination $DebugPath -Recurse -Force
    Write-Host "Frontend copied" -ForegroundColor Green
} else {
    Write-Host "ERROR: Frontend Debug build not found at $FrontendDebugPath" -ForegroundColor Red
    exit 1
}

# Backend kopieren (überschreiben falls schon vom Build.targets kopiert)
Copy-Item -Path $BackendExe -Destination "$DebugPath\OSCRBackend.exe" -Force
Write-Host "Backend copied" -ForegroundColor Green

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

