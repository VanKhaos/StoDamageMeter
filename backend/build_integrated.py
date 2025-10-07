#!/usr/bin/env python3
"""
Build-Script für vollständig integrierte STO Damage Meter Anwendung
Erstellt eine einzige .exe für Endanwender
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

def build_integrated_backend():
    """Erstellt die integrierte Backend-Executable"""
    print("Building integrated backend executable...")
    
    cmd = [
        'python', '-m', 'PyInstaller',
        '--clean',
        'integrated.spec'
    ]
    
    if not run_command(cmd, timeout=600):  # 10 Minuten Timeout
        return False
    
    # Prüfen ob Executable erstellt wurde
    exe_path = Path('dist/OSCRBackend.exe')
    if exe_path.exists():
        size_mb = exe_path.stat().st_size / (1024 * 1024)
        print(f"PASS Integrated executable created: {exe_path} ({size_mb:.1f} MB)")
        return True
    else:
        print("FAIL Integrated executable not found")
        return False

def test_integrated_backend():
    """Testet die integrierte Backend-Executable"""
    print("Testing integrated backend...")
    
    exe_path = Path('dist/OSCRBackend.exe')
    if not exe_path.exists():
        print("FAIL Integrated executable not found for testing")
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
    """Kopiert die integrierte Executable ins Frontend-Verzeichnis"""
    print("Copying integrated executable to frontend...")
    
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

def build_frontend():
    """Baut das WPF-Frontend"""
    print("Building WPF frontend...")
    
    frontend_dir = Path('../frontend')
    if not frontend_dir.exists():
        print("FAIL Frontend directory not found")
        return False
    
    # Frontend bauen
    cmd = ['dotnet', 'build', '--configuration', 'Release']
    if not run_command(cmd, cwd=frontend_dir, timeout=300):
        return False
    
    # Frontend publizieren
    cmd = ['dotnet', 'publish', '--configuration', 'Release', '--output', 'Deploy', '--self-contained', 'false']
    if not run_command(cmd, cwd=frontend_dir, timeout=300):
        return False
    
    print("PASS Frontend built and published")
    return True

def create_final_package():
    """Erstellt das finale Paket für Endanwender"""
    print("Creating final package for end users...")
    
    # Quellverzeichnisse
    frontend_deploy = Path('../frontend/Deploy')
    backend_exe = Path('dist/OSCRBackend.exe')
    final_package = Path('../STO_Damage_Meter_Final')
    
    if not frontend_deploy.exists():
        print("FAIL Frontend deploy directory not found")
        return False
    
    if not backend_exe.exists():
        print("FAIL Backend executable not found")
        return False
    
    # Finales Paket-Verzeichnis erstellen
    if final_package.exists():
        shutil.rmtree(final_package)
    final_package.mkdir()
    
    # Frontend-Dateien kopieren
    for item in frontend_deploy.iterdir():
        if item.is_file():
            shutil.copy2(item, final_package / item.name)
        elif item.is_dir():
            shutil.copytree(item, final_package / item.name)
    
    # Backend-Executable kopieren
    shutil.copy2(backend_exe, final_package / 'OSCRBackend.exe')
    
    # README erstellen
    readme_content = """# STO Damage Meter

## Installation
1. Stellen Sie sicher, dass .NET 9.0 Runtime installiert ist
2. Führen Sie frontend.exe aus

## Verwendung
1. Starten Sie die Anwendung durch Doppelklick auf frontend.exe
2. Das Backend startet automatisch im Hintergrund
3. Wählen Sie eine Combat-Log-Datei aus
4. Klicken Sie auf "Analyze Combat" für die Analyse

## Systemanforderungen
- Windows 10/11
- .NET 9.0 Runtime
- Mindestens 100 MB freier Speicherplatz

## Support
Bei Problemen überprüfen Sie die Log-Dateien im Anwendungsverzeichnis.
"""
    
    with open(final_package / 'README.txt', 'w', encoding='utf-8') as f:
        f.write(readme_content)
    
    print(f"PASS Final package created at: {final_package}")
    return True

def main():
    """Hauptfunktion"""
    print("=== STO Damage Meter - Integrated Build Script ===")
    
    # Arbeitsverzeichnis prüfen
    if not Path('OSCR').exists():
        print("Error: OSCR directory not found. Run from backend/ directory.")
        return 1
    
    # Abhängigkeiten prüfen
    if not check_dependencies():
        return 1
    
    # Build-Verzeichnisse bereinigen
    clean_build_dirs()
    
    # Integriertes Backend erstellen
    if not build_integrated_backend():
        return 1
    
    # Backend testen
    if not test_integrated_backend():
        print("Warning: Backend test failed, but continuing...")
    
    # Ins Frontend kopieren
    if not copy_to_frontend():
        print("Warning: Could not copy to frontend")
    
    # Frontend bauen
    if not build_frontend():
        return 1
    
    # Finales Paket erstellen
    if not create_final_package():
        return 1
    
    print("\n=== Build completed successfully! ===")
    print("Final package location: ../STO_Damage_Meter_Final/")
    print("End user can run: frontend.exe")
    print("Backend runs automatically in background")
    
    return 0

if __name__ == "__main__":
    sys.exit(main())
