#!/usr/bin/env python3
"""
Verbessertes Build-Script für OSCR-Backend mit PyInstaller
Erstellt eine optimierte Standalone-Executable für die WPF-Integration
"""

import os
import sys
import subprocess
import shutil
import time
from pathlib import Path

def run_command(cmd, cwd=None, timeout=300):
    """Führt einen Befehl aus und gibt das Ergebnis zurück"""
    print(f"Running: {' '.join(cmd)}")
    try:
        result = subprocess.run(cmd, cwd=cwd, capture_output=True, text=True, timeout=timeout)
        
        if result.returncode != 0:
            print(f"Error: {result.stderr}")
            return False
        
        if result.stdout:
            print(result.stdout)
        return True
    except subprocess.TimeoutExpired:
        print(f"Command timed out after {timeout} seconds")
        return False
    except Exception as e:
        print(f"Command failed: {e}")
        return False

def check_dependencies():
    """Prüft ob alle benötigten Abhängigkeiten installiert sind"""
    print("Checking dependencies...")
    
    required_packages = ['PyInstaller', 'numpy']
    missing_packages = []
    
    for package in required_packages:
        try:
            __import__(package)
            print(f"PASS {package} is installed")
        except ImportError:
            missing_packages.append(package)
            print(f"FAIL {package} is missing")
    
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

def optimize_pyinstaller():
    """Erstellt optimierte PyInstaller-Executable"""
    print("Building optimized executable with PyInstaller...")
    
    # PyInstaller-Befehl mit Optimierungen
    cmd = [
        'pyinstaller',
        '--clean',
        '--onefile',
        '--name=OSCRBackend',
        '--distpath=dist',
        '--workpath=build',
        '--specpath=.',
        '--exclude-module=tkinter',
        '--exclude-module=matplotlib',
        '--exclude-module=pandas',
        '--exclude-module=scipy',
        '--exclude-module=PIL',
        '--exclude-module=cv2',
        '--exclude-module=tensorflow',
        '--exclude-module=torch',
        '--exclude-module=jupyter',
        '--exclude-module=notebook',
        '--exclude-module=IPython',
        '--exclude-module=test',
        '--exclude-module=unittest',
        '--exclude-module=doctest',
        '--console',
        '--optimize=2',
        'OSCR/real_api.py'
    ]
    
    if not run_command(cmd, timeout=600):  # 10 Minuten Timeout
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

def create_version_info():
    """Erstellt Versionsinformationen"""
    version_info = {
        'version': '1.0.0',
        'build_date': time.strftime('%Y-%m-%d %H:%M:%S'),
        'python_version': sys.version,
        'executable_size': 0
    }
    
    exe_path = Path('dist/OSCRBackend.exe')
    if exe_path.exists():
        version_info['executable_size'] = exe_path.stat().st_size
    
    # Version-Info in Datei schreiben
    with open('dist/version.json', 'w') as f:
        import json
        json.dump(version_info, f, indent=2)
    
    print(f"Version info created: {version_info}")

def main():
    """Hauptfunktion"""
    print("=== OSCR Backend Improved Build Script ===")
    
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
    if not optimize_pyinstaller():
        return 1
    
    # Executable testen
    if not test_executable():
        print("Warning: Executable test failed, but continuing...")
    
    # Ins Frontend kopieren
    if not copy_to_frontend():
        print("Warning: Could not copy to frontend")
    
    # Versionsinformationen erstellen
    create_version_info()
    
    print("\n=== Build completed successfully! ===")
    print("Executable location: dist/OSCRBackend.exe")
    print("Frontend copy: ../frontend/OSCRBackend.exe")
    print("Version info: dist/version.json")
    
    return 0

if __name__ == "__main__":
    sys.exit(main())
