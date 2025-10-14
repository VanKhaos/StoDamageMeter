# Build Debug Version and copy to Debug/ folder
# Usage: .\build_debug.ps1

Write-Host "=== Building STO Damage Meter (Debug Version) ===" -ForegroundColor Cyan

# Arbeitsverzeichnis (Script ist jetzt in scripts/ Ordner)
$RootPath = Split-Path $PSScriptRoot -Parent
$FrontendPath = Join-Path $RootPath "frontend"
$BackendPath = Join-Path $RootPath "backend"
$DebugPath = Join-Path $RootPath "Debug"
$LogsPath = Join-Path $DebugPath "logs"


# 1. Backend bauen (nur wenn geändert)
Write-Host "`n[1/3] Checking Backend..." -ForegroundColor Yellow
$BackendExe = Join-Path $BackendPath "dist\OSCRBackend.exe"
$BackendSource = Join-Path $BackendPath "working_oscr_backend.py"
$BackendDeploy = Join-Path $RootPath "Deploy\OSCRBackend.exe"

# Prüfen ob Backend existiert und ob es neuer ist als die Quelle
$needsBuild = $false
if (-not (Test-Path $BackendExe)) {
    Write-Host "Backend executable not found, building..." -ForegroundColor Yellow
    $needsBuild = $true
} else {
    $sourceTime = (Get-Item $BackendSource).LastWriteTime
    $exeTime = (Get-Item $BackendExe).LastWriteTime
    if ($sourceTime -gt $exeTime) {
        Write-Host "Backend source changed, rebuilding..." -ForegroundColor Yellow
        $needsBuild = $true
    } else {
        Write-Host "Backend is up to date" -ForegroundColor Green
    }
}

if ($needsBuild) {
    Write-Host "Building Backend with PyInstaller..." -ForegroundColor Yellow
    Push-Location $BackendPath
    pyinstaller --onefile --name OSCRBackend working_oscr_backend.py
    Pop-Location
    
    if (-not (Test-Path $BackendExe)) {
        Write-Host "ERROR: Backend build failed!" -ForegroundColor Red
        exit 1
    }
    Write-Host "Backend built successfully: $BackendExe" -ForegroundColor Green
}

# Backend nach Deploy kopieren (immer, falls es sich geändert hat)
Write-Host "Copying Backend to Deploy folder..." -ForegroundColor Yellow
Copy-Item $BackendExe $BackendDeploy -Force
Write-Host "Backend copied to Deploy folder" -ForegroundColor Green

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

