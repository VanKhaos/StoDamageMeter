@echo off
title STO Live Combat Monitor
echo === Starting Monitors ===
echo.
echo Terminal 1: App Logs Monitor
echo Terminal 2: Combat Log Monitor
echo.

:: Starte beide Monitoring-Scripts in separaten Fenstern
start "App Logs Monitor" powershell -ExecutionPolicy Bypass -NoExit -Command "& { cd '%~dp0'; .\monitor_app_logs.ps1 }"
timeout /t 2 /nobreak >nul
start "Combat Log Monitor" powershell -ExecutionPolicy Bypass -NoExit -Command "& { cd '%~dp0'; .\monitor_combatlog.ps1 }"

echo Beide Monitore gestartet!
echo Schließe dieses Fenster, wenn du fertig bist.
pause


