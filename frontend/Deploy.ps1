# PowerShell-Script für Deployment der STO Damage Meter Anwendung
# Erstellt ein vollständiges Deployment-Paket für Endanwender

param(
    [string]$OutputPath = ".\Deploy",
    [switch]$CreateInstaller,
    [switch]$CreateZip
)

Write-Host "=== STO Damage Meter Deployment Script ===" -ForegroundColor Green

# Funktionen
function Test-Prerequisites {
    Write-Host "Checking prerequisites..." -ForegroundColor Yellow
    
    # .NET Runtime prüfen
    try {
        $dotnetVersion = dotnet --version
        Write-Host "✓ .NET Runtime: $dotnetVersion" -ForegroundColor Green
    }
    catch {
        Write-Host "✗ .NET Runtime not found. Please install .NET 9.0 Runtime." -ForegroundColor Red
        return $false
    }
    
    # Python prüfen (für Backend-Build)
    try {
        $pythonVersion = python --version
        Write-Host "✓ Python: $pythonVersion" -ForegroundColor Green
    }
    catch {
        Write-Host "✗ Python not found. Backend build will be skipped." -ForegroundColor Yellow
    }
    
    return $true
}

function Build-Backend {
    Write-Host "Building Python Backend..." -ForegroundColor Yellow
    
    $backendScript = "..\backend\build_backend_improved.py"
    if (Test-Path $backendScript) {
        try {
            python $backendScript
            Write-Host "✓ Backend build completed" -ForegroundColor Green
            return $true
        }
        catch {
            Write-Host "✗ Backend build failed: $_" -ForegroundColor Red
            return $false
        }
    }
    else {
        Write-Host "✗ Backend build script not found" -ForegroundColor Red
        return $false
    }
}

function Build-Frontend {
    Write-Host "Building WPF Frontend..." -ForegroundColor Yellow
    
    try {
        dotnet build --configuration Release
        Write-Host "✓ Frontend build completed" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "✗ Frontend build failed: $_" -ForegroundColor Red
        return $false
    }
}

function Publish-Application {
    Write-Host "Publishing application..." -ForegroundColor Yellow
    
    try {
        dotnet publish --configuration Release --output $OutputPath --self-contained false
        Write-Host "✓ Application published to: $OutputPath" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "✗ Application publish failed: $_" -ForegroundColor Red
        return $false
    }
}

function Copy-AdditionalFiles {
    Write-Host "Copying additional files..." -ForegroundColor Yellow
    
    # Backend-Executable kopieren
    $backendSource = "OSCRBackend.exe"
    $backendTarget = "$OutputPath\OSCRBackend.exe"
    
    if (Test-Path $backendSource) {
        Copy-Item $backendSource $backendTarget -Force
        Write-Host "✓ Backend executable copied" -ForegroundColor Green
    }
    else {
        Write-Host "✗ Backend executable not found" -ForegroundColor Red
    }
    
    # README erstellen
    $readmeContent = @"
# STO Damage Meter

## Installation
1. Stellen Sie sicher, dass .NET 9.0 Runtime installiert ist
2. Führen Sie frontend.exe aus

## Verwendung
1. Starten Sie die Anwendung
2. Wählen Sie eine Combat-Log-Datei aus
3. Klicken Sie auf "Analyze Combat" für die Analyse

## Systemanforderungen
- Windows 10/11
- .NET 9.0 Runtime
- Mindestens 100 MB freier Speicherplatz

## Support
Bei Problemen überprüfen Sie die Log-Dateien im Anwendungsverzeichnis.
"@
    
    $readmeContent | Out-File -FilePath "$OutputPath\README.txt" -Encoding UTF8
    Write-Host "✓ README created" -ForegroundColor Green
}

function Create-ZipPackage {
    Write-Host "Creating ZIP package..." -ForegroundColor Yellow
    
    $zipPath = "STO_Damage_Meter_v1.0.zip"
    
    try {
        Compress-Archive -Path "$OutputPath\*" -DestinationPath $zipPath -Force
        Write-Host "✓ ZIP package created: $zipPath" -ForegroundColor Green
        return $true
    }
    catch {
        Write-Host "✗ ZIP creation failed: $_" -ForegroundColor Red
        return $false
    }
}

function Create-Installer {
    Write-Host "Creating installer..." -ForegroundColor Yellow
    
    # WiX Toolset prüfen
    try {
        $wixVersion = heat -?
        Write-Host "✓ WiX Toolset available" -ForegroundColor Green
    }
    catch {
        Write-Host "✗ WiX Toolset not found. Installer creation skipped." -ForegroundColor Yellow
        return $false
    }
    
    # Einfacher Batch-Installer erstellen
    $installerContent = @"
@echo off
echo Installing STO Damage Meter...
echo.

REM Zielverzeichnis erstellen
set "INSTALL_DIR=%USERPROFILE%\STO_Damage_Meter"
if not exist "%INSTALL_DIR%" mkdir "%INSTALL_DIR%"

REM Dateien kopieren
echo Copying files...
xcopy /E /I /Y "%CD%\*" "%INSTALL_DIR%\"

REM Desktop-Verknüpfung erstellen
echo Creating desktop shortcut...
set "DESKTOP=%USERPROFILE%\Desktop"
echo [InternetShortcut] > "%DESKTOP%\STO Damage Meter.url"
echo URL=file:///%INSTALL_DIR%/frontend.exe >> "%DESKTOP%\STO Damage Meter.url"
echo IconFile=%INSTALL_DIR%/frontend.exe >> "%DESKTOP%\STO Damage Meter.url"
echo IconIndex=0 >> "%DESKTOP%\STO Damage Meter.url"

echo.
echo Installation completed!
echo Application installed to: %INSTALL_DIR%
echo Desktop shortcut created.
echo.
pause
"@
    
    $installerContent | Out-File -FilePath "$OutputPath\Install.bat" -Encoding ASCII
    Write-Host "✓ Installer created: Install.bat" -ForegroundColor Green
    return $true
}

# Hauptlogik
Write-Host "Starting deployment process..." -ForegroundColor Cyan

# Prerequisites prüfen
if (-not (Test-Prerequisites)) {
    Write-Host "Prerequisites check failed. Aborting." -ForegroundColor Red
    exit 1
}

# Output-Verzeichnis erstellen
if (Test-Path $OutputPath) {
    Remove-Item $OutputPath -Recurse -Force
}
New-Item -ItemType Directory -Path $OutputPath -Force | Out-Null

# Backend bauen
Build-Backend

# Frontend bauen
if (-not (Build-Frontend)) {
    Write-Host "Frontend build failed. Aborting." -ForegroundColor Red
    exit 1
}

# Anwendung publizieren
if (-not (Publish-Application)) {
    Write-Host "Application publish failed. Aborting." -ForegroundColor Red
    exit 1
}

# Zusätzliche Dateien kopieren
Copy-AdditionalFiles

# ZIP-Paket erstellen
if ($CreateZip) {
    Create-ZipPackage
}

# Installer erstellen
if ($CreateInstaller) {
    Create-Installer
}

Write-Host "`n=== Deployment completed successfully! ===" -ForegroundColor Green
Write-Host "Output directory: $OutputPath" -ForegroundColor Cyan
Write-Host "Ready for distribution!" -ForegroundColor Green
