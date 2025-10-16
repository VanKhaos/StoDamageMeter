## Session 17: Log-Rotation für alle Log-Dateien

**Datum:** 2025-10-11  
**Dauer:** ~1 Stunde  
**Fokus:** Implementierung von Log-Rotation für Backend und App, Verhinderung von unbegrenztem Log-Wachstum

### 🎯 **Was wir erreicht haben:**

#### ✅ **Erfolgreich implementiert:**

1. **Backend Log-Rotation (Python)**
   - **Problem:** User hatte 10 GB große `backend_service_debug.log` durch Dauernutzung
   - **Gefahr:** Endanwender könnten bei intensiver Nutzung (Live-Parsing über Tage/Wochen) ebenfalls mehrere GB Logs generieren
   - **Lösung:** `RotatingFileHandler` statt einfacher `FileHandler`
   - **Konfiguration:**
     ```python
     from logging.handlers import RotatingFileHandler
     file_handler = RotatingFileHandler(
         log_file_path, 
         mode='a', 
         maxBytes=10*1024*1024,  # 10 MB
         backupCount=3,  # 3 Backup-Dateien (.log.1, .log.2, .log.3)
         encoding='utf-8'
     )
     ```
   - **Resultat:** Maximale Gesamtgröße = 40 MB (10 MB + 3×10 MB Backups)
   - **Datei:** `backend/working_oscr_backend.py`

2. **App Debug-Log-Rotation (C#)**
   - **Betroffen:** 
     - `App_debug.log` (MainWindow.xaml.cs)
     - `backend_debug.log` (OSCRBackendService.cs)
   - **Lösung:** 
     - Nur in `DEBUG`-Builds aktiv (via `#if DEBUG`)
     - Manuelle Größen-Prüfung vor jedem Schreibvorgang
     - Rotation bei > 5 MB (App) bzw. > 10 MB (backend)
   - **Code:**
     ```csharp
     #if DEBUG
     var fileInfo = new FileInfo(logFile);
     if (fileInfo.Exists && fileInfo.Length > 5 * 1024 * 1024)
     {
         var oldFile = logFile + ".1";
         if (File.Exists(oldFile))
             File.Delete(oldFile);
         File.Move(logFile, oldFile);
     }
     File.AppendAllText(logFile, logMessage + Environment.NewLine);
     #endif
     ```
   - **Resultat:** Debug-Logs in Release-Builds **gar nicht** erstellt, in Debug-Builds max. 10-15 MB

3. **Release-Script Log-Cleanup (PowerShell)**
   - **Problem:** Log-Dateien wurden im Release-Package mitgeliefert
   - **Lösung:** `scripts\create_release.ps1` entfernt alle `*.log` Dateien vor ZIP-Erstellung
   - **Code:**
     ```powershell
     # Log-Dateien entfernen
     $LogsDir = Join-Path $AppDir "logs"
     if (Test-Path $LogsDir) {
         Get-ChildItem -Path $LogsDir -Filter "*.log" -Recurse | Remove-Item -Force
     }
     ```
   - **Resultat:** Release-ZIP enthält keine Log-Dateien mehr

4. **.gitignore erweitert**
   - Neue Einträge:
     ```
     # Logs
     *.log
     **/logs/
     **/logs/*.log
     app/bin/Debug/net9.0-windows/logs/
     app/bin/Release/net9.0-windows/logs/
     ```
   - **Resultat:** Logs werden nicht mehr committed

5. **RELEASE_GUIDE.md aktualisiert**
   - Neue Sektion "Troubleshooting: ZIP zu groß"
   - Dokumentiert Log-Rotation-Feature (ab v1.2.3)
   - Erklärt maximale Log-Größen für Endanwender

### 🔧 **Technische Details:**

#### **Maximale Log-Größen pro Build-Type:**

| Build-Type | Log-Dateien | Max. Größe |
|------------|-------------|------------|
| **Release** | `oscr_backend.log` | 40 MB (10+3×10) |
| **Debug** | `oscr_backend.log` | 40 MB (10+3×10) |
| **Debug** | `App_debug.log` | 10 MB (5+5) |
| **Debug** | `backend_debug.log` | 20 MB (10+10) |
| **Release** | App Debug-Logs | **0 MB** (nicht erstellt) |

**Gesamte Max-Größe:**
- Release-Build: **~40 MB** (nur Backend)
- Debug-Build: **~70 MB** (Backend + App-Logs)

### 📁 **Wichtige Dateien:**

**Aktualisiert:**
- `backend/working_oscr_backend.py` - RotatingFileHandler implementiert
- `app/MainWindow.xaml.cs` - Debug-Log-Rotation + #if DEBUG
- `app/Services/OSCRBackendService.cs` - Debug-Log-Rotation + #if DEBUG
- `scripts\create_release.ps1` - Log-Cleanup vor ZIP-Erstellung
- `.gitignore` - Log-Pattern hinzugefügt
- `RELEASE_GUIDE.md` - Log-Rotation dokumentiert

### 🚨 **Verhinderte Probleme:**

#### **Problem: 10 GB Log-Datei beim Entwickler**
- **Ursache:** Live-Parsing über mehrere Tage ohne Log-Rotation
- **Datei:** `backend_service_debug.log` (10 GB!)
- **Symptom:** Release-ZIP-Erstellung fehlgeschlagen ("Datenstrom war zu lang")
- **Lösung:** 
  1. Log-Rotation implementiert
  2. Logs aus Release-Package entfernt
  3. `.gitignore` aktualisiert
- **Resultat:** ✅ Problem kann nicht mehr auftreten

#### **Potenzielle Endanwender-Szenarien:**
- **Szenario 1:** Spieler lässt App 24/7 mit Live-Parsing laufen
  - **Ohne Rotation:** Mehrere GB innerhalb von Wochen
  - **Mit Rotation:** Maximal 40 MB
- **Szenario 2:** Intensiver Raider mit vielen Combat-Log-Analysen
  - **Ohne Rotation:** Hunderte MB Backend-Logs
  - **Mit Rotation:** Maximal 40 MB
- **Szenario 3:** Debugging durch Entwickler
  - **Ohne Rotation:** Debug-Logs ohne Limit
  - **Mit Rotation:** Max. 70 MB (überschaubar)

### 💡 **Lessons Learned:**

1. **RotatingFileHandler ist Standard:** Sollte immer verwendet werden für produktive Anwendungen
2. **Debug vs Release:** Debug-Logs gehören nicht ins Release (via `#if DEBUG`)
3. **Release-Hygiene:** Logs müssen vor ZIP-Erstellung entfernt werden
4. **Gitignore:** Logs sollten nie committed werden
5. **Früherkennung:** User-Feedback über große ZIP-Dateien war wichtiger Hinweis
6. **Backup-Count:** 3 Backups sind guter Balance zwischen Historie und Speicherplatz
7. **Encoding:** UTF-8 für Log-Dateien ist wichtig (Sonderzeichen)

### 🔄 **Build-Status:**

- ✅ Backend mit Log-Rotation neu gebaut
- ✅ App kompiliert mit Log-Rotation
- ✅ Debug-Build getestet
- ✅ `.gitignore` aktualisiert
- ✅ `RELEASE_GUIDE.md` dokumentiert
- ✅ Keine Breaking Changes

### 🎨 **Code-Qualität:**

**Vorher:**
- ❌ Unbegrenzt wachsende Log-Dateien
- ❌ 10 GB Log-Datei möglich
- ❌ Release-ZIP mit Logs verschmutzt
- ❌ Logs im Git-Repository

**Nachher:**
- ✅ Automatische Log-Rotation
- ✅ Maximal 40-70 MB (je nach Build-Type)
- ✅ Saubere Release-ZIPs ohne Logs
- ✅ Logs in `.gitignore`

### 📊 **Code-Umfang:**

**Änderungen:**
- Backend: ~20 Zeilen (Import + Handler-Konfiguration)
- App MainWindow: ~25 Zeilen (Rotation-Logik + #if DEBUG)
- App OSCRBackendService: ~30 Zeilen (Rotation-Logik + #if DEBUG)
- scripts\create_release.ps1: ~10 Zeilen (Log-Cleanup)
- .gitignore: 6 Zeilen
- RELEASE_GUIDE.md: ~15 Zeilen

**Gesamt:** ~100 Zeilen für komplettes Log-Management

### 🎯 **Schutz für Endanwender:**

**Worst-Case-Szenario verhindert:**
- ✅ Live-Parsing 24/7 über Monate: Max. 40 MB (statt unbegrenzt)
- ✅ Intensive Combat-Analyse: Max. 40 MB (statt GB)
- ✅ Debug-Builds: Max. 70 MB (überschaubar)
- ✅ Release-Builds: Nur notwendige Logs (~40 MB)

**Endanwender muss sich nicht kümmern:**
- Keine manuellen Log-Löschungen erforderlich
- Automatische Rotation im Hintergrund
- Kein Festplatz-Problem durch Logs
- Logs bleiben für Debugging verfügbar (3 Backups)

---
**Nächste Session:** Release 1.2.3 erstellen, GitHub Pages aktualisieren, Merge in main


