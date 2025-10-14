## Session 11: Clean Release Structure mit Launcher-Architektur

**Datum:** 2025-10-10  
**Dauer:** ~4 Stunden  
**Fokus:** Release-Verzeichnis aufrÃ¤umen, Launcher-EXE, alle Dateien in App/ Ordner

### ðŸŽ¯ **Was wir erreicht haben:**

#### âœ… **Erfolgreich implementiert:**

1. **Launcher-Projekt erstellt**
   - Neues WPF-Projekt: `Launcher/` im Root-Verzeichnis
   - Single-File-EXE (~12 MB ohne Trimming)
   - Startet `App\StoDamageMeter.Core.exe` mit korrektem WorkingDirectory
   - **OutputType: WinExe** - Kein Konsolenfenster!
   - MessageBox fÃ¼r Fehler statt Console-Ausgaben
   - Sofortige Beendigung nach Start der Core-App
   - GrÃ¶ÃŸe: ~12 MB (ohne Trimming wegen WPF-KompatibilitÃ¤t)

2. **Release-Struktur komplett Ã¼berarbeitet**
   - **Vorher (v1.1.4):** 414 Dateien im Root-Verzeichnis (unÃ¼bersichtlich)
   - **Nachher (v1.1.7):** Nur 3 EintrÃ¤ge im Root (sauber!)
   - **Neue Struktur:**
     ```
     Root/
     â”œâ”€â”€ StoDamageMeter.exe    (Launcher - 12 MB)
     â”œâ”€â”€ README.txt
     â”œâ”€â”€ App/                   (257 Dateien)
     â”‚   â”œâ”€â”€ StoDamageMeter.Core.exe
     â”‚   â”œâ”€â”€ OSCRBackend.exe
     â”‚   # appsettings.json wurde entfernt
     â”‚   â””â”€â”€ [251 DLLs + Runtime]
     â””â”€â”€ Language/              (13 Sprachordner)
     ```

3. **Build.targets erweitert fÃ¼r automatische Reorganisation**
   - Neues MSBuild-Target: `ReorganizeReleaseStructure`
   - LÃ¤uft automatisch nach `dotnet publish`
   - Verschiebt:
     - Alle DLLs â†’ `App/`
     - Alle EXEs (auÃŸer Launcher) â†’ `App/`
     - Alle JSON-Dateien â†’ `App/`
     - Sprachordner â†’ `Language/`
   - Benennt `StoDamageMeter.exe` â†’ `StoDamageMeter.Core.exe` um

4. **scripts\create_release.ps1 komplett Ã¼berarbeitet**
   - 7-Schritte Build-Prozess (vorher 6):
     1. Cleaning old release files
     2. Checking Python Backend
     3. Building Launcher (neu!)
     4. Building Frontend
     5. Copying Launcher to Root
     6. Copying Backend to App/
     7. Creating README
   - Validierung der finalen Struktur
   - PrÃ¼ft ob alle kritischen Dateien vorhanden sind
   - Launcher wird als `StoDamageMeter.exe` ins Root kopiert

5. **Frontend-Anpassungen fÃ¼r App-Struktur**
   - **Launcher:** Setzt WorkingDirectory auf `App/`
   - **Core-App:** Findet Backend und Config automatisch im gleichen Ordner
   - Keine Parent-Directory-Suche mehr nÃ¶tig
   - Alles im gleichen Verzeichnis = einfacher

6. **ProblemlÃ¶sungen wÃ¤hrend der Entwicklung**
   - **Problem 1:** Backend nicht gefunden
     - **Ursache:** Lagen im Root, Core-App in App/
     - **LÃ¶sung:** Alles in App/ verschoben, Launcher setzt WorkingDirectory
   - **Problem 2:** Konsolenfenster beim Launcher-Start
     - **Ursache:** OutputType war `Exe` (Console-App)
     - **LÃ¶sung:** OutputType auf `WinExe` geÃ¤ndert
   - **Problem 3:** WPF Trimming-Fehler
     - **Ursache:** `PublishTrimmed=true` nicht kompatibel mit WPF
     - **LÃ¶sung:** Trimming deaktiviert (PublishTrimmed=false)
   - **Problem 4:** Launcher wartete auf Core-App
     - **Ursache:** `process.WaitForExit()` blockierte
     - **LÃ¶sung:** Sofortige Beendigung nach Start

### ðŸ”§ **Technische Details:**

#### **Launcher-Architektur:**
```csharp
// Launcher/Program.cs
[STAThread]
static int Main(string[] args)
{
    var coreAppPath = Path.Combine(launcherDir, @"App\StoDamageMeter.Core.exe");
    
    if (!File.Exists(coreAppPath))
    {
        MessageBox.Show("ERROR: Core application not found!");
        return 1;
    }
    
    var startInfo = new ProcessStartInfo
    {
        FileName = coreAppPath,
        WorkingDirectory = Path.Combine(launcherDir, "App"),
        UseShellExecute = false
    };
    
    Process.Start(startInfo);
    return 0; // Sofortige Beendigung
}
```

#### **MSBuild Target fÃ¼r Reorganisation:**
```xml
<Target Name="ReorganizeReleaseStructure" AfterTargets="Publish">
  <!-- App/ und Language/ Verzeichnisse erstellen -->
  <MakeDir Directories="$(AppDir);$(LanguageDir)" />
  
  <!-- Alle DLLs, EXEs und JSONs nach App/ verschieben -->
  <Move SourceFiles="@(AllDlls)" DestinationFolder="$(AppDir)" />
  <Move SourceFiles="@(AllExes)" DestinationFolder="$(AppDir)" />
  <Move SourceFiles="@(AllRuntimeFiles)" DestinationFolder="$(AppDir)" />
  
  <!-- StoDamageMeter.exe umbenennen -->
  <Move SourceFiles="$(AppDir)StoDamageMeter.exe" 
        DestinationFiles="$(AppDir)StoDamageMeter.Core.exe" />
  
  <!-- Sprachordner verschieben -->
  <Exec Command="move /Y &quot;$(PublishDir)de&quot; &quot;$(LanguageDir)&quot;" />
  <!-- ... fÃ¼r alle 13 Sprachen ... -->
</Target>
```

### ðŸ“ **Wichtige Dateien:**

**Neu erstellt:**
- `Launcher/Launcher.csproj` - Launcher-Projekt-Datei
- `Launcher/Program.cs` - Launcher-Code mit WinExe
- `RELEASE_STRUCTURE_v1.1.5_CLEAN.md` - Dokumentation der neuen Struktur

**Aktualisiert:**
- `frontend/Build.targets` - ReorganizeReleaseStructure Target
- `frontend/frontend.csproj` - GenerateAssemblyInfo=false
- `scripts\create_release.ps1` - 7-Schritte Build-Prozess

### ðŸ’¡ **Lessons Learned:**

1. **Launcher-Pattern:** Saubere Trennung von Start-Logik und Hauptanwendung
2. **WPF ohne Trimming:** ~12 MB Overhead akzeptabel fÃ¼r bessere KompatibilitÃ¤t
3. **WorkingDirectory:** Kritisch fÃ¼r korrekte Datei-Pfade
4. **MSBuild Targets:** MÃ¤chtig fÃ¼r Post-Build-Automation
5. **WinExe vs Exe:** WinExe fÃ¼r GUI-Apps ohne Konsolenfenster
6. **Release-Hygiene:** Saubere Struktur ist wichtiger als minimale GrÃ¶ÃŸe

### ðŸ”„ **Build-Status:**

- âœ… Release v1.1.7 erstellt und getestet
- âœ… ZIP erstellt (~119 MB)
- âœ… Anwendung startet korrekt
- âœ… Kein Konsolenfenster
- âœ… Alle Features funktional

---
**NÃ¤chste Session:** Splashscreen fÃ¼r Launcher implementieren, DPS-Graph


