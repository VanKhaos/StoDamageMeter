# Build Debug Version and copy to Debug/ folder
# Usage: .\build_debug.ps1

Write-Host "=== Building STO Damage Meter (Debug Version) ===" -ForegroundColor Cyan

# Arbeitsverzeichnis (Script ist jetzt in scripts/ Ordner)
$RootPath = Split-Path $PSScriptRoot -Parent
$FrontendPath = Join-Path $RootPath "app"
$BackendPath = Join-Path $RootPath "backend"
$DebugPath = Join-Path $RootPath "Debug"
$LogsPath = Join-Path $DebugPath "logs"

# 0. Alle laufenden Anwendungsprozesse beenden
Write-Host "`n[0/4] Stopping running application processes..." -ForegroundColor Yellow

# Prozesse finden und beenden
$processesToStop = @("StoDamageMeter", "OSCRBackend", "StoDamageMeter.Launcher")

$stoppedProcesses = @()
foreach ($processName in $processesToStop) {
    $processes = Get-Process -Name $processName -ErrorAction SilentlyContinue
    if ($processes) {
        foreach ($process in $processes) {
            Write-Host "Stopping process: $($process.ProcessName) (PID: $($process.Id))" -ForegroundColor Yellow
            try {
                $process.Kill()
                $process.WaitForExit(5000)
                $stoppedProcesses += $process.ProcessName
                Write-Host "Process stopped successfully" -ForegroundColor Green
            }
            catch {
                Write-Host "Could not stop process: $($_.Exception.Message)" -ForegroundColor Red
            }
        }
    }
}

if ($stoppedProcesses.Count -gt 0) {
    Write-Host "Stopped $($stoppedProcesses.Count) process(es): $($stoppedProcesses -join ', ')" -ForegroundColor Green
    Start-Sleep -Seconds 2
} else {
    Write-Host "No running application processes found" -ForegroundColor Green
}

# 1. Backend bauen (nur wenn geändert)
Write-Host "`n[1/4] Checking Backend..." -ForegroundColor Yellow
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

# 2. App Debug bauen
Write-Host "`n[2/4] Building App (Debug)..." -ForegroundColor Yellow
Push-Location $FrontendPath
$buildResult = dotnet build app.csproj --configuration Debug 2>&1
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: App build failed!" -ForegroundColor Red
    Write-Host $buildResult
    Pop-Location
    exit 1
}
Pop-Location
Write-Host "App build completed" -ForegroundColor Green

# 3. Debug-Ordner ist bereits korrekt befüllt
Write-Host "`n[3/4] Debug build completed!" -ForegroundColor Yellow
Write-Host "Debug files are ready in: $DebugPath" -ForegroundColor Green

# 4. Finale Zusammenfassung
Write-Host "`n[4/4] Build Summary:" -ForegroundColor Yellow
Write-Host "Application processes stopped" -ForegroundColor Green
Write-Host "Backend built and deployed" -ForegroundColor Green  
Write-Host "App built successfully" -ForegroundColor Green
Write-Host "Debug files ready" -ForegroundColor Green

Write-Host "`n=== Build abgeschlossen! ===" -ForegroundColor Green
Write-Host "Debug-Version verfügbar in: $DebugPath" -ForegroundColor Cyan
Write-Host "Starten mit: .\Debug\StoDamageMeter.exe" -ForegroundColor Cyan