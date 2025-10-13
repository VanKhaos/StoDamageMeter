# Scripts

Dieser Ordner enthält alle Build- und Utility-Scripts für das STO Damage Meter Projekt.

## 📁 Script-Übersicht

### 🔨 Build-Scripts
- **`build_debug.ps1`** - Erstellt Debug-Version im `Debug/` Ordner
- **`create_release.ps1`** - Erstellt vollständige Release-Version
- **`create_release_zip.ps1`** - Erstellt ZIP-Archiv für Distribution

### 🎨 Utility-Scripts
- **`create_icon.py`** - Konvertiert PNG zu ICO für Windows-Icons
- **`update_page_version.ps1`** - Aktualisiert Version in GitHub Pages

### 📊 Monitoring-Scripts
- **`monitor_app_logs.ps1`** - Überwacht Anwendungs-Logs
- **`monitor_combatlog.ps1`** - Überwacht Combat-Log-Datei
- **`start_monitoring.bat`** - Startet Log-Monitoring

## 🚀 Verwendung

### Debug-Build erstellen
```powershell
.\scripts\build_debug.ps1
```

### Release erstellen
```powershell
.\scripts\create_release.ps1 -Version "1.2.4"
.\scripts\create_release_zip.ps1 -Version "1.2.4"
```

### Icon erstellen
```powershell
python scripts\create_icon.py
```

### Logs überwachen
```powershell
.\scripts\start_monitoring.bat
```

## 📝 Hinweise

- Alle Scripts werden vom Root-Verzeichnis aus ausgeführt
- PowerShell-Scripts benötigen PowerShell 5.1+
- Python-Scripts benötigen Python 3.8+
- Batch-Scripts funktionieren in allen Windows-Versionen

## 🔧 Wartung

Bei Änderungen an Scripts:
1. Teste das Script im Root-Verzeichnis
2. Aktualisiere diese README.md falls nötig
3. Aktualisiere Referenzen in anderen Dokumenten
