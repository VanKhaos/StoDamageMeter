## Session 12: Splashscreen fÃ¼r Launcher mit WPF

**Datum:** 2025-10-10  
**Dauer:** ~2 Stunden  
**Fokus:** Animierter Splashscreen wÃ¤hrend Core-App-Start, Star Trek Design

### ðŸŽ¯ **Was wir erreicht haben:**

#### âœ… **Erfolgreich implementiert:**

1. **SplashScreen.xaml erstellt**
   - Frameless WPF Window (WindowStyle="None", AllowsTransparency="True")
   - Star Trek Theme Design (Blue #5B9BD5, Deep Black #0A0A0A)
   - 600x400px zentriert, Topmost, nicht in Taskbar
   - Logo-Bereich mit Haupttitel "STO DAMAGE METER"
   - Untertitel "STAR TREK ONLINE COMBAT PARSER"
   - Version-Anzeige (1.2.0)
   - Border mit Glow-Effekt (DropShadowEffect)

2. **Animierter Ladebalken**
   - Smooth Animation von links nach rechts
   - Endlos-Loop wÃ¤hrend Ladevorgang
   - Blue Glow-Effekt (BlurEffect)
   - Dauer: 1.5 Sekunden pro Durchlauf
   - ClipToBounds fÃ¼r saubere Kanten

3. **Fade-In/Fade-Out Animationen**
   - Fade-In beim Start (0 â†’ 1 in 0.5s)
   - Fade-Out beim SchlieÃŸen (1 â†’ 0 in 0.3s)
   - Smooth Transition fÃ¼r professionelles Erscheinungsbild

4. **Status-Text-Updates**
   - "Starting STO Damage Meter..." (Initial)
   - "Initializing components..." (nach 800ms)
   - "Starting application..." (nach 1200ms)
   - Dynamisch aktualisierbar Ã¼ber `UpdateStatus()` Methode

5. **App.xaml und App.xaml.cs refactored**
   - WPF Application Entry Point erstellt
   - `StartupUri="SplashScreen.xaml"` â†’ Splashscreen als erste Anzeige
   - `ShutdownMode="OnExplicitShutdown"` â†’ Launcher kontrolliert Shutdown
   - Asynchroner Core-App-Start in Background-Thread
   - Automatisches Splashscreen-SchlieÃŸen nach Core-App-Start

6. **Program.cs vereinfacht**
   - Von 70 Zeilen auf 16 Zeilen reduziert (77% Reduktion!)
   - Nur noch WPF Application Entry Point
   - Gesamte Start-Logik in App.xaml.cs ausgelagert
   - `[STAThread]` fÃ¼r WPF-KompatibilitÃ¤t

7. **Timing-Optimierung**
   - 800ms: Initiale VerzÃ¶gerung fÃ¼r visuelle Wirkung
   - 400ms: Status-Update "Initializing components"
   - 200ms: Status-Update "Starting application"
   - 1500ms: Warten nach Core-App-Start (Window erscheint)
   - 300ms: Fade-Out-Animation
   - 500ms: Launcher-Shutdown nach Fade-Out

8. **Launcher.csproj angepasst**
   - `<EnableDefaultApplicationDefinition>false</EnableDefaultApplicationDefinition>`
   - Verhindert doppelte Main-Entry-Points
   - WPF SDK-Defaults fÃ¼r XAML-Files beibehalten

### ðŸ”§ **Technische Details:**

#### **Splashscreen-Architektur:**
```
Program.Main() â†’ App.OnStartup() â†’ Show SplashScreen
                                   â†“
                              Task.Run(StartCoreApp)
                                   â†“
                              Wait for Core App
                                   â†“
                              Fade-Out SplashScreen
                                   â†“
                              Shutdown Launcher
```

#### **XAML Fade-In Animation:**
```xml
<Storyboard x:Key="FadeInAnimation">
    <DoubleAnimation Storyboard.TargetProperty="Opacity"
                   From="0" To="1"
                   Duration="0:0:0.5"/>
</Storyboard>

<Window.Triggers>
    <EventTrigger RoutedEvent="Window.Loaded">
        <BeginStoryboard Storyboard="{StaticResource FadeInAnimation}"/>
    </EventTrigger>
</Window.Triggers>
```

#### **Loading Bar Animation:**
```xml
<Storyboard x:Key="LoadingAnimation" RepeatBehavior="Forever">
    <DoubleAnimation Storyboard.TargetName="LoadingBar"
                   Storyboard.TargetProperty="(Rectangle.RenderTransform).(TranslateTransform.X)"
                   From="-100" To="600"
                   Duration="0:0:1.5"/>
</Storyboard>
```

#### **Status-Update-Methode:**
```csharp
public void UpdateStatus(string message)
{
    Dispatcher.Invoke(() =>
    {
        StatusText.Text = message;
    });
}
```

#### **Asynchroner Start-Prozess:**
```csharp
private async Task StartCoreApplication(string[] args)
{
    await Task.Delay(800);  // Initial delay
    _splashScreen?.UpdateStatus("Initializing components...");
    await Task.Delay(400);
    
    _splashScreen?.UpdateStatus("Starting application...");
    // ... Core-App starten ...
    
    await Task.Delay(1500);  // Wait for Core window
    _splashScreen?.CloseSplash();
    
    // Shutdown launcher after fade-out
    Task.Delay(500).ContinueWith(_ => 
        Dispatcher.Invoke(() => Shutdown(0)));
}
```

### ðŸ“ **Wichtige Dateien:**

**Neu erstellt:**
- `Launcher/SplashScreen.xaml` - Splashscreen UI (146 Zeilen)
- `Launcher/SplashScreen.xaml.cs` - Splashscreen Code-Behind
- `Launcher/App.xaml` - WPF Application Entry Point
- `Launcher/App.xaml.cs` - Start-Logik (106 Zeilen)

**Aktualisiert:**
- `Launcher/Program.cs` - Von 70 auf 16 Zeilen vereinfacht
- `Launcher/Launcher.csproj` - EnableDefaultApplicationDefinition=false

**UnverÃ¤ndert:**
- `scripts\create_release.ps1` - Funktioniert mit neuem Launcher
- `scripts\create_release_zip.ps1` - Funktioniert automatisch

### ðŸš¨ **GelÃ¶ste Probleme:**

#### **Problem 1: LetterSpacing Property nicht verfÃ¼gbar**
- **Symptom:** `error MC3072: The property 'LetterSpacing' does not exist`
- **Ursache:** LetterSpacing ist WinUI-Property, nicht WPF
- **LÃ¶sung:** Property entfernt aus XAML
- **Resultat:** âœ… Build erfolgreich

#### **Problem 2: Doppelter Entry Point**
- **Symptom:** `error CS0017: Program has more than one entry point defined`
- **Ursache:** WPF generiert automatisch Main() aus App.xaml
- **LÃ¶sung:** `<EnableDefaultApplicationDefinition>false</EnableDefaultApplicationDefinition>`
- **Resultat:** âœ… Manuelle Kontrolle Ã¼ber Entry Point

#### **Problem 3: Duplicate Page Items**
- **Symptom:** `error NETSDK1022: Duplicate 'Page' items`
- **Ursache:** SDK fÃ¼gt XAML automatisch hinzu, manueller ItemGroup-Eintrag
- **LÃ¶sung:** Manuelle ItemGroup entfernt, SDK-Defaults verwenden
- **Resultat:** âœ… Sauberer Build ohne Duplikate

### ðŸ’¡ **Lessons Learned:**

1. **WPF Application-Modell:** App.xaml ist der Standard-Entry-Point fÃ¼r WPF
2. **EnableDefaultApplicationDefinition:** Muss false sein fÃ¼r manuellen Main()
3. **SDK Implicit Items:** WPF SDK fÃ¼gt XAML-Dateien automatisch hinzu
4. **Splashscreen-Timing:** 2-3 Sekunden ideal fÃ¼r guten UX
5. **Fade-Animationen:** Smooth Transitions wirken professionell
6. **Async Task.Run:** Verhindert UI-Freeze wÃ¤hrend Core-App-Start
7. **Dispatcher.Invoke:** Notwendig fÃ¼r UI-Updates aus Background-Threads

### ðŸ”„ **Build-Status:**

- âœ… Launcher kompiliert erfolgreich
- âœ… Splashscreen zeigt korrekt an
- âœ… Animationen laufen smooth
- âœ… Core-App startet nach Splash
- âœ… Launcher schlieÃŸt sich automatisch
- âœ… Release v1.2.0 erstellt (268.84 MB, 415 Dateien)
- âœ… ZIP erstellt (119.22 MB)

### ðŸŽ¨ **UI-Features:**

**Splashscreen-Design:**
- âœ… Frameless Window mit Border-Glow
- âœ… Star Trek Blue Theme
- âœ… GroÃŸer Haupttitel mit Glow-Effekt
- âœ… Untertitel und Version-Info
- âœ… Animierter Ladebalken (endlos)
- âœ… Status-Text (3 Phasen)
- âœ… Fade-In beim Start
- âœ… Fade-Out beim SchlieÃŸen

**Benutzer-Erfahrung:**
- âœ… Kein Konsolenfenster
- âœ… Smooth Transitions
- âœ… Professionelles Erscheinungsbild
- âœ… Klare visuelle RÃ¼ckmeldung
- âœ… Automatische AblÃ¤ufe (keine Interaktion nÃ¶tig)

### ðŸ“Š **Code-Statistiken:**

**Launcher-Projekt:**
- SplashScreen.xaml: 146 Zeilen
- SplashScreen.xaml.cs: 23 Zeilen
- App.xaml: 10 Zeilen
- App.xaml.cs: 106 Zeilen
- Program.cs: 16 Zeilen (von 70)
- **Gesamt:** ~301 Zeilen (vs. 70 vorher)
- **FunktionalitÃ¤t:** +Splashscreen +Animationen +bessere UX

**Release v1.2.0:**
- Entpackt: 268.84 MB (415 Dateien)
- ZIP: 119.22 MB
- Launcher: ~12 MB (Single-File, Self-Contained)

### ðŸš¨ **GelÃ¶ste Probleme (Post-Release):**

#### **Problem 1: DllNotFoundException beim Launcher-Start**
- **Symptom:** `System.DllNotFoundException: Dll was not found`
- **Ursache:** WPF funktioniert nicht mit `PublishSingleFile=true`
- **LÃ¶sung:** `PublishSingleFile=false` â†’ Multi-File-Deployment mit WPF-DLLs
- **Resultat:** âœ… Launcher startet korrekt, 6 WPF-DLLs im Root

#### **Problem 2: App.xaml StartupUri Konflikt**
- **Symptom:** Launcher zeigte nichts an
- **Ursache:** `StartupUri="SplashScreen.xaml"` und manuelle `Show()` im Code
- **LÃ¶sung:** `StartupUri` entfernt, `Startup="Application_Startup"` Event verwendet
- **Resultat:** âœ… Splashscreen erscheint korrekt

#### **Problem 3: Splashscreen verschwindet zu frÃ¼h**
- **Symptom:** Splashscreen schlieÃŸt bevor Hauptfenster sichtbar ist
- **Ursache:** Feste Wartezeit (1500ms) reicht nicht
- **LÃ¶sung:** Intelligente Fenster-Erkennung mit `MainWindowHandle` und `MainWindowTitle` Check
- **Polling:** Alle 500ms prÃ¼fen ob Hauptfenster vorhanden, max. 10 Sekunden Timeout
- **Resultat:** âœ… Splashscreen bleibt bis App vollstÃ¤ndig geladen ist

### ðŸ“Š **Finale Release-Struktur v1.2.0:**

```
Root/
â”œâ”€â”€ StoDamageMeter.exe         (Launcher - 11 MB)
â”œâ”€â”€ D3DCompiler_47_cor3.dll    (WPF)
â”œâ”€â”€ PenImc_cor3.dll            (WPF)
â”œâ”€â”€ PresentationNative_cor3.dll (WPF)
â”œâ”€â”€ vcruntime140_cor3.dll      (WPF)
â”œâ”€â”€ wpfgfx_cor3.dll            (WPF)
â”œâ”€â”€ README.txt
â”œâ”€â”€ App/                       (257 Dateien, 252 MB)
â”‚   â”œâ”€â”€ StoDamageMeter.Core.exe
â”‚   â”œâ”€â”€ OSCRBackend.exe
â”‚   # appsettings.json wurde entfernt
â”‚   â””â”€â”€ [Runtime + DLLs]
â””â”€â”€ Language/                  (13 Sprachordner)
```

**GrÃ¶ÃŸen:**
- Entpackt: 276.84 MB (420 Dateien)
- ZIP: ~120 MB
- Root: 7 Dateien (Launcher + 6 WPF-DLLs)

### âš¡ **Performance-Hinweis:**

**Warum die App langsam startet (3-5 Sekunden):**
1. **Self-Contained .NET Runtime** (~150 MB muss geladen werden)
2. **WPF Framework-Initialisierung**
3. **WPF-UI Bibliothek** (Fluent Design Components)
4. **Windows 11 Mica/Backdrop-Effekte**

**Alternative (nicht implementiert):**
- Framework-Dependent Deployment â†’ < 1 Sekunde Start
- Nachteil: Benutzer muss .NET 9 Runtime installieren
- Entscheidung: Self-Contained fÃ¼r bessere Benutzerfreundlichkeit

---
**NÃ¤chste Session:** Live-Parsing-Modus (FileWatcher fÃ¼r Combat-Log), DPS-Graph


