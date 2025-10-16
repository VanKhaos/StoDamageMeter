## Session 11: Clean Release Structure mit Launcher-Architektur

**Datum:** 2025-10-10  
**Dauer:** ~4 Stunden  
**Fokus:** Release-Verzeichnis aufräumen, Launcher-EXE, alle Dateien in App/ Ordner

### 🎯 **Was wir erreicht haben:**

#### ✅ **Erfolgreich implementiert:**

1. **Launcher-Projekt erstellt**
   - Neues WPF-Projekt: `Launcher/` im Root-Verzeichnis
   - Single-File-EXE (~12 MB ohne Trimming)
   - Startet `App\StoDamageMeter.Core.exe` mit korrektem WorkingDirectory
   - **OutputType: WinExe** - Kein Konsolenfenster!
   - MessageBox für Fehler statt Console-Ausgaben
   - Sofortige Beendigung nach Start der Core-App
   - Größe: ~12 MB (ohne Trimming wegen WPF-Kompatibilität)

2. **Release-Struktur komplett überarbeitet**
   - **Vorher (v1.1.4):** 414 Dateien im Root-Verzeichnis (unübersichtlich)
   - **Nachher (v1.1.7):** Nur 3 Einträge im Root (sauber!)
   - **Neue Struktur:**
     ```
     Root/
     ├── StoDamageMeter.exe    (Launcher - 12 MB)
     ├── README.txt
     ├── App/                   (257 Dateien)
     │   ├── StoDamageMeter.Core.exe
     │   ├── OSCRBackend.exe
     │   # appsettings.json wurde entfernt
     │   └── [251 DLLs + Runtime]
     └── Language/              (13 Sprachordner)
     ```

3. **Build.targets erweitert für automatische Reorganisation**
   - Neues MSBuild-Target: `ReorganizeReleaseStructure`
   - Läuft automatisch nach `dotnet publish`
   - Verschiebt:
     - Alle DLLs → `App/`
     - Alle EXEs (außer Launcher) → `App/`
     - Alle JSON-Dateien → `App/`
     - Sprachordner → `Language/`
   - Benennt `StoDamageMeter.exe` → `StoDamageMeter.Core.exe` um

4. **scripts\create_release.ps1 komplett überarbeitet**
   - 7-Schritte Build-Prozess (vorher 6):
     1. Cleaning old release files
     2. Checking Python Backend
     3. Building Launcher (neu!)
     4. Building App
     5. Copying Launcher to Root
     6. Copying Backend to App/
     7. Creating README
   - Validierung der finalen Struktur
   - Prüft ob alle kritischen Dateien vorhanden sind
   - Launcher wird als `StoDamageMeter.exe` ins Root kopiert

5. **App-Anpassungen für App-Struktur**
   - **Launcher:** Setzt WorkingDirectory auf `App/`
   - **Core-App:** Findet Backend und Config automatisch im gleichen Ordner
   - Keine Parent-Directory-Suche mehr nötig
   - Alles im gleichen Verzeichnis = einfacher

6. **Problemlösungen während der Entwicklung**
   - **Problem 1:** Backend nicht gefunden
     - **Ursache:** Lagen im Root, Core-App in App/
     - **Lösung:** Alles in App/ verschoben, Launcher setzt WorkingDirectory
   - **Problem 2:** Konsolenfenster beim Launcher-Start
     - **Ursache:** OutputType war `Exe` (Console-App)
     - **Lösung:** OutputType auf `WinExe` geändert
   - **Problem 3:** WPF Trimming-Fehler
     - **Ursache:** `PublishTrimmed=true` nicht kompatibel mit WPF
     - **Lösung:** Trimming deaktiviert (PublishTrimmed=false)
   - **Problem 4:** Launcher wartete auf Core-App
     - **Ursache:** `process.WaitForExit()` blockierte
     - **Lösung:** Sofortige Beendigung nach Start

### 🔧 **Technische Details:**

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

#### **MSBuild Target für Reorganisation:**
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
  <!-- ... für alle 13 Sprachen ... -->
</Target>
```

### 📁 **Wichtige Dateien:**

**Neu erstellt:**
- `Launcher/Launcher.csproj` - Launcher-Projekt-Datei
- `Launcher/Program.cs` - Launcher-Code mit WinExe
- `RELEASE_STRUCTURE_v1.1.5_CLEAN.md` - Dokumentation der neuen Struktur

**Aktualisiert:**
- `app/Build.targets` - ReorganizeReleaseStructure Target
- `app/App.csproj` - GenerateAssemblyInfo=false
- `scripts\create_release.ps1` - 7-Schritte Build-Prozess

### 💡 **Lessons Learned:**

1. **Launcher-Pattern:** Saubere Trennung von Start-Logik und Hauptanwendung
2. **WPF ohne Trimming:** ~12 MB Overhead akzeptabel für bessere Kompatibilität
3. **WorkingDirectory:** Kritisch für korrekte Datei-Pfade
4. **MSBuild Targets:** Mächtig für Post-Build-Automation
5. **WinExe vs Exe:** WinExe für GUI-Apps ohne Konsolenfenster
6. **Release-Hygiene:** Saubere Struktur ist wichtiger als minimale Größe

### 🔄 **Build-Status:**

- ✅ Release v1.1.7 erstellt und getestet
- ✅ ZIP erstellt (~119 MB)
- ✅ Anwendung startet korrekt
- ✅ Kein Konsolenfenster
- ✅ Alle Features funktional

---
**Nächste Session:** Splashscreen für Launcher implementieren, DPS-Graph


