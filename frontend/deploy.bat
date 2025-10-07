@echo off
echo ========================================
echo STO Damage Meter - Deployment Script
echo ========================================
echo.

REM Prüfen ob .NET verfügbar ist
echo Checking .NET Runtime...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: .NET Runtime not found!
    echo Please install .NET 9.0 Runtime from:
    echo https://dotnet.microsoft.com/download/dotnet/9.0
    pause
    exit /b 1
)
echo .NET Runtime found.

REM Prüfen ob Python verfügbar ist
echo Checking Python...
python --version >nul 2>&1
if %errorlevel% neq 0 (
    echo WARNING: Python not found. Backend build will be skipped.
    echo Please install Python for full functionality.
) else (
    echo Python found.
)

echo.
echo Building application...

REM Backend bauen (falls Python verfügbar)
python --version >nul 2>&1
if %errorlevel% equ 0 (
    echo Building Python Backend...
    pushd ..\backend
    python build_backend_improved.py
    popd
    echo Backend build completed.
) else (
    echo Skipping backend build (Python not available).
)

REM Frontend bauen
echo Building WPF Frontend...
dotnet build --configuration Release
if %errorlevel% neq 0 (
    echo ERROR: Frontend build failed!
    pause
    exit /b 1
)
echo Frontend build completed.

REM Anwendung publizieren
echo Publishing application...
dotnet publish --configuration Release --output Deploy --self-contained false
if %errorlevel% neq 0 (
    echo ERROR: Application publish failed!
    pause
    exit /b 1
)
echo Application published.

REM Backend kopieren
echo Copying backend executable...
if exist OSCRBackend.exe (
    copy OSCRBackend.exe Deploy\ >nul
    echo Backend executable copied.
) else (
    echo WARNING: Backend executable not found!
)

REM README erstellen
echo Creating README...
(
echo # STO Damage Meter
echo.
echo ## Installation
echo 1. Stellen Sie sicher, dass .NET 9.0 Runtime installiert ist
echo 2. Führen Sie frontend.exe aus
echo.
echo ## Verwendung
echo 1. Starten Sie die Anwendung
echo 2. Wählen Sie eine Combat-Log-Datei aus
echo 3. Klicken Sie auf "Analyze Combat" für die Analyse
echo.
echo ## Systemanforderungen
echo - Windows 10/11
echo - .NET 9.0 Runtime
echo - Mindestens 100 MB freier Speicherplatz
echo.
echo ## Support
echo Bei Problemen überprüfen Sie die Log-Dateien im Anwendungsverzeichnis.
) > Deploy\README.txt

REM Installer erstellen
echo Creating installer...
(
echo @echo off
echo echo Installing STO Damage Meter...
echo echo.
echo.
echo REM Zielverzeichnis erstellen
echo set "INSTALL_DIR=%%USERPROFILE%%\STO_Damage_Meter"
echo if not exist "%%INSTALL_DIR%%" mkdir "%%INSTALL_DIR%%"
echo.
echo REM Dateien kopieren
echo echo Copying files...
echo xcopy /E /I /Y "%%CD%%\*" "%%INSTALL_DIR%%\"
echo.
echo REM Desktop-Verknüpfung erstellen
echo echo Creating desktop shortcut...
echo set "DESKTOP=%%USERPROFILE%%\Desktop"
echo echo [InternetShortcut] ^> "%%DESKTOP%%\STO Damage Meter.url"
echo echo URL=file:///%%INSTALL_DIR%%/frontend.exe ^>^> "%%DESKTOP%%\STO Damage Meter.url"
echo echo IconFile=%%INSTALL_DIR%%/frontend.exe ^>^> "%%DESKTOP%%\STO Damage Meter.url"
echo echo IconIndex=0 ^>^> "%%DESKTOP%%\STO Damage Meter.url"
echo.
echo echo.
echo echo Installation completed!
echo echo Application installed to: %%INSTALL_DIR%%
echo echo Desktop shortcut created.
echo echo.
echo pause
) > Deploy\Install.bat

echo.
echo ========================================
echo Deployment completed successfully!
echo ========================================
echo.
echo Output directory: Deploy\
echo.
echo Files created:
echo - frontend.exe (Main application)
echo - OSCRBackend.exe (Backend executable)
echo - README.txt (User manual)
echo - Install.bat (Installer script)
echo.
echo Ready for distribution!
echo.
pause
