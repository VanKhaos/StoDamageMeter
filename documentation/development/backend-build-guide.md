# Backend Build Guide

**Datum:** 2025-10-14  
**Version:** 1.3  
**Ziel:** Anleitung zum manuellen Build des OSCR Backends mit PyInstaller

## 🎯 Übersicht

Das OSCR Backend wird als Python-Script entwickelt und mit PyInstaller zu einer standalone Executable kompiliert. Diese Anleitung zeigt, wie man das Backend manuell neu baut, wenn Änderungen am Code vorgenommen wurden.

## 📋 Voraussetzungen

### Python-Umgebung
- **Python 3.14.0** (oder kompatible Version)
- **PyInstaller** installiert: `pip install pyinstaller`
- **Abhängigkeiten** aus `requirements.txt` installiert

### Verzeichnisstruktur
```
backend/
├── working_oscr_backend.py    # Haupt-Backend-Code
├── build_backend.py           # Automatisches Build-Script
├── requirements.txt           # Python-Abhängigkeiten
├── dist/                      # Build-Output (PyInstaller)
├── build/                     # Build-Temp-Dateien
└── OSCRBackend.spec          # PyInstaller-Konfiguration
```

## 🔧 Manueller Build-Prozess

### 1. Backend-Verzeichnis wechseln
```bash
cd backend
```

### 2. PyInstaller direkt ausführen
```bash
pyinstaller --onefile --name OSCRBackend working_oscr_backend.py
```

### 3. Executable in Deployment-Ordner kopieren
```bash
# Kopiere nach Debug-Ordner
copy dist\OSCRBackend.exe ..\Debug\

# Kopiere nach Deploy-Ordner  
copy dist\OSCRBackend.exe ..\Deploy\
```

## 📊 Build-Output

### Erfolgreicher Build zeigt:
```
INFO: Building EXE from EXE-00.toc
INFO: Copying bootloader EXE to D:\Projekte\StoDamageMeter\backend\dist\OSCRBackend.exe
INFO: Copying icon to EXE
INFO: Embedding manifest in EXE
INFO: Appending PKG archive to EXE
INFO: Fixing EXE headers
INFO: Building EXE from EXE-00.toc completed successfully.
INFO: Build complete! The results are available in: D:\Projekte\StoDamageMeter\backend\dist
```

### Dateien nach Build:
- `backend/dist/OSCRBackend.exe` - Neue Executable
- `backend/build/` - Temporäre Build-Dateien
- `backend/OSCRBackend.spec` - PyInstaller-Konfiguration

## 🚀 Automatischer Build (Alternative)

### Build-Script verwenden
```bash
cd backend
python build_backend.py
```

**Hinweis:** Das Build-Script erwartet ein `OSCR/` Verzeichnis und ist für das ursprüngliche OSCR-Projekt konfiguriert. Für manuelle Builds ist PyInstaller direkt zu empfehlen.

## 🔍 Wann Backend neu bauen?

### Code-Änderungen die einen Rebuild erfordern:
- ✅ **Backend-Logik** geändert (`working_oscr_backend.py`)
- ✅ **API-Response-Format** geändert
- ✅ **Neue Features** hinzugefügt
- ✅ **Bug-Fixes** im Backend
- ✅ **Dependencies** geändert (`requirements.txt`)

### Änderungen die KEINEN Rebuild erfordern:
- ❌ **Frontend-Code** geändert (C#/XAML)
- ❌ **UI-Layout** geändert
- ❌ **Styling** geändert
- ❌ **Dokumentation** geändert

## 🐛 Troubleshooting

### Problem: "OSCR directory not found"
**Lösung:** Verwende PyInstaller direkt statt `build_backend.py`
```bash
pyinstaller --onefile --name OSCRBackend working_oscr_backend.py
```

### Problem: "Module not found" Fehler
**Lösung:** Dependencies installieren
```bash
pip install -r requirements.txt
```

### Problem: Executable startet nicht
**Lösung:** 
1. Prüfe ob alle Dependencies installiert sind
2. Teste Backend direkt: `python working_oscr_backend.py`
3. Prüfe Logs in `backend/logs/`

### Problem: Frontend kann Backend nicht finden
**Lösung:** Executable in beide Ordner kopieren
```bash
copy dist\OSCRBackend.exe ..\Debug\
copy dist\OSCRBackend.exe ..\Deploy\
```

## 📝 Build-Optimierungen

### PyInstaller-Optionen
```bash
# Minimale Größe (langsamer Start)
pyinstaller --onefile --strip --optimize=2 --name OSCRBackend working_oscr_backend.py

# Schneller Start (größere Datei)
pyinstaller --onedir --name OSCRBackend working_oscr_backend.py

# Mit Icon
pyinstaller --onefile --icon=icon.ico --name OSCRBackend working_oscr_backend.py
```

### Spez-Datei anpassen
Die generierte `OSCRBackend.spec` kann für erweiterte Konfigurationen angepasst werden:
```python
# Beispiel: Exclude bestimmte Module
excludes = ['tkinter', 'matplotlib', 'pandas']
```

## 🔄 Workflow-Integration

### Für Entwickler:
1. **Backend-Code ändern** (`working_oscr_backend.py`)
2. **Backend testen:** `python working_oscr_backend.py`
3. **Backend bauen:** `pyinstaller --onefile --name OSCRBackend working_oscr_backend.py`
4. **Executable kopieren:** `copy dist\OSCRBackend.exe ..\Debug\`
5. **Frontend testen:** `dotnet run --project frontend`

### Für CI/CD:
```bash
# Automatisierter Build-Script
cd backend
pip install -r requirements.txt
pyinstaller --onefile --name OSCRBackend working_oscr_backend.py
copy dist\OSCRBackend.exe ..\Debug\
copy dist\OSCRBackend.exe ..\Deploy\
```

## 📊 Performance-Metriken

### Build-Zeit:
- **Erster Build:** ~30-60 Sekunden
- **Inkrementeller Build:** ~10-20 Sekunden
- **Clean Build:** ~30-60 Sekunden

### Executable-Größe:
- **Typische Größe:** ~15-25 MB
- **Mit Optimierungen:** ~10-15 MB
- **Mit allen Dependencies:** ~30-50 MB

## 🎯 Best Practices

### Code-Änderungen:
1. **Immer testen** bevor Build
2. **Logging hinzufügen** für Debugging
3. **Error-Handling** implementieren
4. **API-Kompatibilität** wahren

### Build-Prozess:
1. **Clean Build** bei größeren Änderungen
2. **Beide Deployment-Ordner** aktualisieren
3. **Frontend testen** nach Backend-Update
4. **Logs prüfen** bei Problemen

### Versionierung:
1. **Backend-Version** in Code dokumentieren
2. **Changelog** führen
3. **Breaking Changes** dokumentieren
4. **API-Versionen** tracken

## 📚 Weitere Ressourcen

- **PyInstaller Docs:** https://pyinstaller.readthedocs.io/
- **Python Packaging:** https://packaging.python.org/
- **OSCR Backend Code:** `backend/working_oscr_backend.py`
- **Frontend Integration:** `frontend/Services/OSCRBackendService.cs`

---
**Letzte Aktualisierung:** 2025-10-14  
**Nächste Überprüfung:** Bei Backend-Änderungen
