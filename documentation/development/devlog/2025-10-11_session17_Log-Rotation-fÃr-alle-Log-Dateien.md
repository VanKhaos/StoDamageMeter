## Session 17: Log-Rotation fÃ¼r alle Log-Dateien

**Datum:** 2025-10-11  
**Dauer:** ~1 Stunde  
**Fokus:** Implementierung von Log-Rotation fÃ¼r Backend und Frontend, Verhinderung von unbegrenztem Log-Wachstum

### ðŸŽ¯ **Was wir erreicht haben:**

#### âœ… **Erfolgreich implementiert:**

1. **Backend Log-Rotation (Python)**
   - **Problem:** User hatte 10 GB groÃŸe `backend_service_debug.log` durch Dauernutzung
   - **Gefahr:** Endanwender kÃ¶nnten bei intensiver Nutzung (Live-Parsing Ã¼ber Tage/Wochen) ebenfalls mehrere GB Logs generieren
   - **LÃ¶sung:** `RotatingFileHandler` statt einfacher `FileHandler`
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
   - **Resultat:** Maximale GesamtgrÃ¶ÃŸe = 40 MB (10 MB + 3Ã—10 MB Backups)
   - **Datei:** `backend/working_oscr_backend.py`

2. **Frontend Debug-Log-Rotation (C#)**
   - **Betroffen:** 
     - `frontend_debug.log` (MainWindow.xaml.cs)
     - `backend_debug.log` (OSCRBackendService.cs)
   - **LÃ¶sung:** 
     - Nur in `DEBUG`-Builds aktiv (via `#if DEBUG`)
     - Manuelle GrÃ¶ÃŸen-PrÃ¼fung vor jedem Schreibvorgang
     - Rotation bei > 5 MB (frontend) bzw. > 10 MB (backend)
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
   - **LÃ¶sung:** `scripts\create_release.ps1` entfernt alle `*.log` Dateien vor ZIP-Erstellung
   - **Code:**
     ```powershell
     # Log-Dateien entfernen
     $LogsDir = Join-Path $AppDir "logs"
     if (Test-Path $LogsDir) {
         Get-ChildItem -Path $LogsDir -Filter "*.log" -Recurse | Remove-Item -Force
     }
     ```
   - **Resultat:** Release-ZIP enthÃ¤lt keine Log-Dateien mehr

4. **.gitignore erweitert**
   - Neue EintrÃ¤ge:
     ```
     # Logs
     *.log
     **/logs/
     **/logs/*.log
     frontend/bin/Debug/net9.0-windows/logs/
     frontend/bin/Release/net9.0-windows/logs/
     ```
   - **Resultat:** Logs werden nicht mehr committed

5. **RELEASE_GUIDE.md aktualisiert**
   - Neue Sektion "Troubleshooting: ZIP zu groÃŸ"
   - Dokumentiert Log-Rotation-Feature (ab v1.2.3)
   - ErklÃ¤rt maximale Log-GrÃ¶ÃŸen fÃ¼r Endanwender

### ðŸ”§ **Technische Details:**

#### **Maximale Log-GrÃ¶ÃŸen pro Build-Type:**

| Build-Type | Log-Dateien | Max. GrÃ¶ÃŸe |
|------------|-------------|------------|
| **Release** | `oscr_backend.log` | 40 MB (10+3Ã—10) |
| **Debug** | `oscr_backend.log` | 40 MB (10+3Ã—10) |
| **Debug** | `frontend_debug.log` | 10 MB (5+5) |
| **Debug** | `backend_debug.log` | 20 MB (10+10) |
| **Release** | Frontend Debug-Logs | **0 MB** (nicht erstellt) |

**Gesamte Max-GrÃ¶ÃŸe:**
- Release-Build: **~40 MB** (nur Backend)
- Debug-Build: **~70 MB** (Backend + Frontend-Logs)

### ðŸ“ **Wichtige Dateien:**

**Aktualisiert:**
- `backend/working_oscr_backend.py` - RotatingFileHandler implementiert
- `frontend/MainWindow.xaml.cs` - Debug-Log-Rotation + #if DEBUG
- `frontend/Services/OSCRBackendService.cs` - Debug-Log-Rotation + #if DEBUG
- `scripts\create_release.ps1` - Log-Cleanup vor ZIP-Erstellung
- `.gitignore` - Log-Pattern hinzugefÃ¼gt
- `RELEASE_GUIDE.md` - Log-Rotation dokumentiert

### ðŸš¨ **Verhinderte Probleme:**

#### **Problem: 10 GB Log-Datei beim Entwickler**
- **Ursache:** Live-Parsing Ã¼ber mehrere Tage ohne Log-Rotation
- **Datei:** `backend_service_debug.log` (10 GB!)
- **Symptom:** Release-ZIP-Erstellung fehlgeschlagen ("Datenstrom war zu lang")
- **LÃ¶sung:** 
  1. Log-Rotation implementiert
  2. Logs aus Release-Package entfernt
  3. `.gitignore` aktualisiert
- **Resultat:** âœ… Problem kann nicht mehr auftreten

#### **Potenzielle Endanwender-Szenarien:**
- **Szenario 1:** Spieler lÃ¤sst App 24/7 mit Live-Parsing laufen
  - **Ohne Rotation:** Mehrere GB innerhalb von Wochen
  - **Mit Rotation:** Maximal 40 MB
- **Szenario 2:** Intensiver Raider mit vielen Combat-Log-Analysen
  - **Ohne Rotation:** Hunderte MB Backend-Logs
  - **Mit Rotation:** Maximal 40 MB
- **Szenario 3:** Debugging durch Entwickler
  - **Ohne Rotation:** Debug-Logs ohne Limit
  - **Mit Rotation:** Max. 70 MB (Ã¼berschaubar)

### ðŸ’¡ **Lessons Learned:**

1. **RotatingFileHandler ist Standard:** Sollte immer verwendet werden fÃ¼r produktive Anwendungen
2. **Debug vs Release:** Debug-Logs gehÃ¶ren nicht ins Release (via `#if DEBUG`)
3. **Release-Hygiene:** Logs mÃ¼ssen vor ZIP-Erstellung entfernt werden
4. **Gitignore:** Logs sollten nie committed werden
5. **FrÃ¼herkennung:** User-Feedback Ã¼ber groÃŸe ZIP-Dateien war wichtiger Hinweis
6. **Backup-Count:** 3 Backups sind guter Balance zwischen Historie und Speicherplatz
7. **Encoding:** UTF-8 fÃ¼r Log-Dateien ist wichtig (Sonderzeichen)

### ðŸ”„ **Build-Status:**

- âœ… Backend mit Log-Rotation neu gebaut
- âœ… Frontend kompiliert mit Log-Rotation
- âœ… Debug-Build getestet
- âœ… `.gitignore` aktualisiert
- âœ… `RELEASE_GUIDE.md` dokumentiert
- âœ… Keine Breaking Changes

### ðŸŽ¨ **Code-QualitÃ¤t:**

**Vorher:**
- âŒ Unbegrenzt wachsende Log-Dateien
- âŒ 10 GB Log-Datei mÃ¶glich
- âŒ Release-ZIP mit Logs verschmutzt
- âŒ Logs im Git-Repository

**Nachher:**
- âœ… Automatische Log-Rotation
- âœ… Maximal 40-70 MB (je nach Build-Type)
- âœ… Saubere Release-ZIPs ohne Logs
- âœ… Logs in `.gitignore`

### ðŸ“Š **Code-Umfang:**

**Ã„nderungen:**
- Backend: ~20 Zeilen (Import + Handler-Konfiguration)
- Frontend MainWindow: ~25 Zeilen (Rotation-Logik + #if DEBUG)
- Frontend OSCRBackendService: ~30 Zeilen (Rotation-Logik + #if DEBUG)
- scripts\create_release.ps1: ~10 Zeilen (Log-Cleanup)
- .gitignore: 6 Zeilen
- RELEASE_GUIDE.md: ~15 Zeilen

**Gesamt:** ~100 Zeilen fÃ¼r komplettes Log-Management

### ðŸŽ¯ **Schutz fÃ¼r Endanwender:**

**Worst-Case-Szenario verhindert:**
- âœ… Live-Parsing 24/7 Ã¼ber Monate: Max. 40 MB (statt unbegrenzt)
- âœ… Intensive Combat-Analyse: Max. 40 MB (statt GB)
- âœ… Debug-Builds: Max. 70 MB (Ã¼berschaubar)
- âœ… Release-Builds: Nur notwendige Logs (~40 MB)

**Endanwender muss sich nicht kÃ¼mmern:**
- Keine manuellen Log-LÃ¶schungen erforderlich
- Automatische Rotation im Hintergrund
- Kein Festplatz-Problem durch Logs
- Logs bleiben fÃ¼r Debugging verfÃ¼gbar (3 Backups)

---
**NÃ¤chste Session:** Release 1.2.3 erstellen, GitHub Pages aktualisieren, Merge in main


