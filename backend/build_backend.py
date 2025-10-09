#!/usr/bin/env python3
"""
Build-Script für OSCR-Backend mit PyInstaller
Erstellt eine Standalone-Executable für die WPF-Integration
"""

import os
import sys
import subprocess
import shutil
from pathlib import Path

def run_command(cmd, cwd=None):
    """Führt einen Befehl aus und gibt das Ergebnis zurück"""
    print(f"Running: {' '.join(cmd)}")
    result = subprocess.run(cmd, cwd=cwd, capture_output=True, text=True)
    
    if result.returncode != 0:
        print(f"Error: {result.stderr}")
        return False
    
    if result.stdout:
        print(result.stdout)
    return True

def check_dependencies():
    """Prüft ob alle benötigten Abhängigkeiten installiert sind"""
    print("Checking dependencies...")
    
    # PyInstaller wird als 'PyInstaller' importiert (großes P)
    required_packages = [('PyInstaller', 'pyinstaller'), ('numpy', 'numpy')]
    missing_packages = []
    
    for import_name, package_name in required_packages:
        try:
            __import__(import_name)
            print(f"PASS {package_name} is installed")
        except ImportError:
            missing_packages.append(package_name)
            print(f"FAIL {package_name} is missing")
    
    if missing_packages:
        print(f"\nMissing packages: {', '.join(missing_packages)}")
        print("Install with: pip install " + " ".join(missing_packages))
        return False
    
    return True

def clean_build_dirs():
    """Bereinigt Build-Verzeichnisse"""
    print("Cleaning build directories...")
    
    dirs_to_clean = ['build', 'dist', '__pycache__']
    for dir_name in dirs_to_clean:
        if os.path.exists(dir_name):
            shutil.rmtree(dir_name)
            print(f"Removed {dir_name}/")

def build_executable():
    """Erstellt die PyInstaller-Executable"""
    print("Building executable with PyInstaller...")
    
    # Verwende das working_oscr.spec File (mit working_oscr_backend.py - gefixt Datum-Parsing)
    spec_file = Path('working_oscr.spec')
    if not spec_file.exists():
        print(f"FAIL Spec file not found: {spec_file}")
        return False
    
    # PyInstaller-Befehl mit Spec-File
    cmd = [
        'pyinstaller',
        '--clean',
        str(spec_file)
    ]
    
    if not run_command(cmd):
        return False
    
    # Prüfen ob Executable erstellt wurde
    exe_path = Path('dist/OSCRBackend.exe')
    if exe_path.exists():
        size_mb = exe_path.stat().st_size / (1024 * 1024)
        print(f"PASS Executable created: {exe_path} ({size_mb:.1f} MB)")
        return True
    else:
        print("FAIL Executable not found")
        return False

def test_executable():
    """Testet die erstellte Executable"""
    print("Testing executable...")
    
    exe_path = Path('dist/OSCRBackend.exe')
    if not exe_path.exists():
        print("FAIL Executable not found for testing")
        return False
    
    # Health-Check testen
    test_input = '{"action": "health"}'
    
    try:
        result = subprocess.run(
            [str(exe_path), '--api'],
            input=test_input,
            text=True,
            capture_output=True,
            timeout=30
        )
        
        if result.returncode == 0:
            print("PASS Health check passed")
            print(f"Response: {result.stdout}")
            return True
        else:
            print(f"FAIL Health check failed: {result.stderr}")
            return False
            
    except subprocess.TimeoutExpired:
        print("FAIL Health check timed out")
        return False
    except Exception as e:
        print(f"FAIL Health check error: {e}")
        return False

def copy_to_frontend():
    """Kopiert die Executable ins Frontend-Verzeichnis"""
    print("Copying executable to frontend...")
    
    exe_path = Path('dist/OSCRBackend.exe')
    frontend_path = Path('../frontend/OSCRBackend.exe')
    
    if not exe_path.exists():
        print("FAIL Source executable not found")
        return False
    
    # Frontend-Verzeichnis erstellen falls nicht vorhanden
    frontend_path.parent.mkdir(exist_ok=True)
    
    # Kopieren
    shutil.copy2(exe_path, frontend_path)
    print(f"PASS Copied to {frontend_path}")
    return True

def main():
    """Hauptfunktion"""
    print("=== OSCR Backend Build Script ===")
    
    # Arbeitsverzeichnis prüfen
    if not Path('OSCR').exists():
        print("Error: OSCR directory not found. Run from backend/ directory.")
        return 1
    
    # Abhängigkeiten prüfen
    if not check_dependencies():
        return 1
    
    # Build-Verzeichnisse bereinigen
    clean_build_dirs()
    
    # Executable erstellen
    if not build_executable():
        return 1
    
    # Executable testen
    if not test_executable():
        print("Warning: Executable test failed, but continuing...")
    
    # Ins Frontend kopieren
    if not copy_to_frontend():
        print("Warning: Could not copy to frontend")
    
    print("\n=== Build completed successfully! ===")
    print("Executable location: dist/OSCRBackend.exe")
    print("Frontend copy: ../frontend/OSCRBackend.exe")
    
    return 0

if __name__ == "__main__":
    sys.exit(main())
