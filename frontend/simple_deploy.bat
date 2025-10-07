@echo off
echo ========================================
echo STO Damage Meter - Simple Deployment
echo ========================================
echo.

echo Checking .NET Runtime...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ERROR: .NET Runtime not found!
    echo Please install .NET 9.0 Runtime
    pause
    exit /b 1
)
echo .NET Runtime found.

echo.
echo Building WPF Frontend...
dotnet build --configuration Release
if %errorlevel% neq 0 (
    echo ERROR: Frontend build failed!
    pause
    exit /b 1
)
echo Frontend build completed.

echo.
echo Publishing application...
dotnet publish --configuration Release --output Deploy --self-contained false
if %errorlevel% neq 0 (
    echo ERROR: Application publish failed!
    pause
    exit /b 1
)
echo Application published.

echo.
echo Copying backend executable...
if exist OSCRBackend.exe (
    copy OSCRBackend.exe Deploy\ >nul
    echo Backend executable copied.
) else (
    echo WARNING: Backend executable not found!
)

echo.
echo Creating README...
echo # STO Damage Meter > Deploy\README.txt
echo. >> Deploy\README.txt
echo ## Installation >> Deploy\README.txt
echo 1. Install .NET 9.0 Runtime >> Deploy\README.txt
echo 2. Run frontend.exe >> Deploy\README.txt
echo. >> Deploy\README.txt
echo ## Usage >> Deploy\README.txt
echo 1. Start the application >> Deploy\README.txt
echo 2. Select a combat log file >> Deploy\README.txt
echo 3. Click "Analyze Combat" >> Deploy\README.txt
echo README created.

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
echo.
echo Ready for distribution!
echo.
pause
