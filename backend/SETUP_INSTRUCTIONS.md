# Backend Setup Anweisungen

## Python Installation

Da Python nicht auf dem System installiert ist, müssen Sie es zuerst installieren:

### Option 1: Python von python.org
1. Gehen Sie zu https://www.python.org/downloads/
2. Laden Sie Python 3.9 oder höher herunter
3. Installieren Sie Python mit "Add Python to PATH" aktiviert

### Option 2: Microsoft Store
1. Öffnen Sie den Microsoft Store
2. Suchen Sie nach "Python"
3. Installieren Sie Python 3.11 oder höher

## Backend Setup

Nach der Python-Installation:

```bash
# Ins Backend-Verzeichnis wechseln
cd backend

# Abhängigkeiten installieren
pip install -r requirements.txt

# Backend bauen
python build_backend.py
```

## Manuelle PyInstaller-Befehle

Falls das Build-Script nicht funktioniert:

```bash
# PyInstaller installieren
pip install pyinstaller

# Executable erstellen
pyinstaller --onefile --name=OSCRBackend --add-data="OSCR/Data;Data" OSCR/api_wrapper.py

# Testen
dist/OSCRBackend.exe --api
```

## API-Test

Testen Sie die API mit:

```bash
# Health-Check
echo '{"action": "health"}' | dist/OSCRBackend.exe --api

# Combat-Analyse (mit echten Log-Dateien)
echo '{"action": "analyze", "logPath": "path/to/combat.log", "maxCombats": 5}' | dist/OSCRBackend.exe --api
```

## Troubleshooting

### Python nicht gefunden
- Stellen Sie sicher, dass Python im PATH ist
- Verwenden Sie `python3` statt `python`
- Verwenden Sie den vollständigen Pfad zu python.exe

### PyInstaller-Fehler
- Aktualisieren Sie pip: `python -m pip install --upgrade pip`
- Installieren Sie PyInstaller neu: `pip uninstall pyinstaller && pip install pyinstaller`

### Import-Fehler
- Stellen Sie sicher, dass alle OSCR-Module im richtigen Verzeichnis sind
- Prüfen Sie die Python-Pfade in der .spec-Datei
