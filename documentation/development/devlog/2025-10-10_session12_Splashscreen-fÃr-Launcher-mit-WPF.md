## Session 12: Splashscreen für Launcher mit WPF

**Datum:** 2025-10-10  
**Dauer:** ~2 Stunden  
**Fokus:** Animierter Splashscreen während Core-App-Start, Star Trek Design

### 🎯 **Was wir erreicht haben:**

#### ✅ **Erfolgreich implementiert:**

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
   - Endlos-Loop während Ladevorgang
   - Blue Glow-Effekt (BlurEffect)
   - Dauer: 1.5 Sekunden pro Durchlauf
   - ClipToBounds für saubere Kanten

3. **Fade-In/Fade-Out Animationen**
   - Fade-In beim Start (0 → 1 in 0.5s)
   - Fade-Out beim Schließen (1 → 0 in 0.3s)
   - Smooth Transition für professionelles Erscheinungsbild

4. **Status-Text-Updates**
   - "Starting STO Damage Meter..." (Initial)
   - "Initializing components..." (nach 800ms)
   - "Starting application..." (nach 1200ms)
   - Dynamisch aktualisierbar über `UpdateStatus()` Methode

5. **App.xaml und App.xaml.cs refactored**
   - WPF Application Entry Point erstellt
   - `StartupUri="SplashScreen.xaml"` → Splashscreen als erste Anzeige
   - `ShutdownMode="OnExplicitShutdown"` → Launcher kontrolliert Shutdown
   - Asynchroner Core-App-Start in Background-Thread
   - Automatisches Splashscreen-Schließen nach Core-App-Start

6. **Program.cs vereinfacht**
   - Von 70 Zeilen auf 16 Zeilen reduziert (77% Reduktion!)
   - Nur noch WPF Application Entry Point
   - Gesamte Start-Logik in App.xaml.cs ausgelagert
   - `[STAThread]` für WPF-Kompatibilität

7. **Timing-Optimierung**
   - 800ms: Initiale Verzögerung für visuelle Wirkung
   - 400ms: Status-Update "Initializing components"
   - 200ms: Status-Update "Starting application"
   - 1500ms: Warten nach Core-App-Start (Window erscheint)
   - 300ms: Fade-Out-Animation
   - 500ms: Launcher-Shutdown nach Fade-Out

8. **Launcher.csproj angepasst**
   - `<EnableDefaultApplicationDefinition>false</EnableDefaultApplicationDefinition>`
   - Verhindert doppelte Main-Entry-Points
   - WPF SDK-Defaults für XAML-Files beibehalten

### 🔧 **Technische Details:**

#### **Splashscreen-Architektur:**
```
Program.Main() → App.OnStartup() → Show SplashScreen
                                   ↓
                              Task.Run(StartCoreApp)
                                   ↓
                              Wait for Core App
                                   ↓
                              Fade-Out SplashScreen
                                   ↓
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

### 📁 **Wichtige Dateien:**

**Neu erstellt:**
- `Launcher/SplashScreen.xaml` - Splashscreen UI (146 Zeilen)
- `Launcher/SplashScreen.xaml.cs` - Splashscreen Code-Behind
- `Launcher/App.xaml` - WPF Application Entry Point
- `Launcher/App.xaml.cs` - Start-Logik (106 Zeilen)

**Aktualisiert:**
- `Launcher/Program.cs` - Von 70 auf 16 Zeilen vereinfacht
- `Launcher/Launcher.csproj` - EnableDefaultApplicationDefinition=false

**Unverändert:**
- `scripts\create_release.ps1` - Funktioniert mit neuem Launcher
- `scripts\create_release_zip.ps1` - Funktioniert automatisch

### 🚨 **Gelöste Probleme:**

#### **Problem 1: LetterSpacing Property nicht verfügbar**
- **Symptom:** `error MC3072: The property 'LetterSpacing' does not exist`
- **Ursache:** LetterSpacing ist WinUI-Property, nicht WPF
- **Lösung:** Property entfernt aus XAML
- **Resultat:** ✅ Build erfolgreich

#### **Problem 2: Doppelter Entry Point**
- **Symptom:** `error CS0017: Program has more than one entry point defined`
- **Ursache:** WPF generiert automatisch Main() aus App.xaml
- **Lösung:** `<EnableDefaultApplicationDefinition>false</EnableDefaultApplicationDefinition>`
- **Resultat:** ✅ Manuelle Kontrolle über Entry Point

#### **Problem 3: Duplicate Page Items**
- **Symptom:** `error NETSDK1022: Duplicate 'Page' items`
- **Ursache:** SDK fügt XAML automatisch hinzu, manueller ItemGroup-Eintrag
- **Lösung:** Manuelle ItemGroup entfernt, SDK-Defaults verwenden
- **Resultat:** ✅ Sauberer Build ohne Duplikate

### 💡 **Lessons Learned:**

1. **WPF Application-Modell:** App.xaml ist der Standard-Entry-Point für WPF
2. **EnableDefaultApplicationDefinition:** Muss false sein für manuellen Main()
3. **SDK Implicit Items:** WPF SDK fügt XAML-Dateien automatisch hinzu
4. **Splashscreen-Timing:** 2-3 Sekunden ideal für guten UX
5. **Fade-Animationen:** Smooth Transitions wirken professionell
6. **Async Task.Run:** Verhindert UI-Freeze während Core-App-Start
7. **Dispatcher.Invoke:** Notwendig für UI-Updates aus Background-Threads

### 🔄 **Build-Status:**

- ✅ Launcher kompiliert erfolgreich
- ✅ Splashscreen zeigt korrekt an
- ✅ Animationen laufen smooth
- ✅ Core-App startet nach Splash
- ✅ Launcher schließt sich automatisch
- ✅ Release v1.2.0 erstellt (268.84 MB, 415 Dateien)
- ✅ ZIP erstellt (119.22 MB)

### 🎨 **UI-Features:**

**Splashscreen-Design:**
- ✅ Frameless Window mit Border-Glow
- ✅ Star Trek Blue Theme
- ✅ Großer Haupttitel mit Glow-Effekt
- ✅ Untertitel und Version-Info
- ✅ Animierter Ladebalken (endlos)
- ✅ Status-Text (3 Phasen)
- ✅ Fade-In beim Start
- ✅ Fade-Out beim Schließen

**Benutzer-Erfahrung:**
- ✅ Kein Konsolenfenster
- ✅ Smooth Transitions
- ✅ Professionelles Erscheinungsbild
- ✅ Klare visuelle Rückmeldung
- ✅ Automatische Abläufe (keine Interaktion nötig)

### 📊 **Code-Statistiken:**

**Launcher-Projekt:**
- SplashScreen.xaml: 146 Zeilen
- SplashScreen.xaml.cs: 23 Zeilen
- App.xaml: 10 Zeilen
- App.xaml.cs: 106 Zeilen
- Program.cs: 16 Zeilen (von 70)
- **Gesamt:** ~301 Zeilen (vs. 70 vorher)
- **Funktionalität:** +Splashscreen +Animationen +bessere UX

**Release v1.2.0:**
- Entpackt: 268.84 MB (415 Dateien)
- ZIP: 119.22 MB
- Launcher: ~12 MB (Single-File, Self-Contained)

### 🚨 **Gelöste Probleme (Post-Release):**

#### **Problem 1: DllNotFoundException beim Launcher-Start**
- **Symptom:** `System.DllNotFoundException: Dll was not found`
- **Ursache:** WPF funktioniert nicht mit `PublishSingleFile=true`
- **Lösung:** `PublishSingleFile=false` → Multi-File-Deployment mit WPF-DLLs
- **Resultat:** ✅ Launcher startet korrekt, 6 WPF-DLLs im Root

#### **Problem 2: App.xaml StartupUri Konflikt**
- **Symptom:** Launcher zeigte nichts an
- **Ursache:** `StartupUri="SplashScreen.xaml"` und manuelle `Show()` im Code
- **Lösung:** `StartupUri` entfernt, `Startup="Application_Startup"` Event verwendet
- **Resultat:** ✅ Splashscreen erscheint korrekt

#### **Problem 3: Splashscreen verschwindet zu früh**
- **Symptom:** Splashscreen schließt bevor Hauptfenster sichtbar ist
- **Ursache:** Feste Wartezeit (1500ms) reicht nicht
- **Lösung:** Intelligente Fenster-Erkennung mit `MainWindowHandle` und `MainWindowTitle` Check
- **Polling:** Alle 500ms prüfen ob Hauptfenster vorhanden, max. 10 Sekunden Timeout
- **Resultat:** ✅ Splashscreen bleibt bis App vollständig geladen ist

### 📊 **Finale Release-Struktur v1.2.0:**

```
Root/
├── StoDamageMeter.exe         (Launcher - 11 MB)
├── D3DCompiler_47_cor3.dll    (WPF)
├── PenImc_cor3.dll            (WPF)
├── PresentationNative_cor3.dll (WPF)
├── vcruntime140_cor3.dll      (WPF)
├── wpfgfx_cor3.dll            (WPF)
├── README.txt
├── App/                       (257 Dateien, 252 MB)
│   ├── StoDamageMeter.Core.exe
│   ├── OSCRBackend.exe
│   # appsettings.json wurde entfernt
│   └── [Runtime + DLLs]
└── Language/                  (13 Sprachordner)
```

**Größen:**
- Entpackt: 276.84 MB (420 Dateien)
- ZIP: ~120 MB
- Root: 7 Dateien (Launcher + 6 WPF-DLLs)

### ⚡ **Performance-Hinweis:**

**Warum die App langsam startet (3-5 Sekunden):**
1. **Self-Contained .NET Runtime** (~150 MB muss geladen werden)
2. **WPF Framework-Initialisierung**
3. **WPF-UI Bibliothek** (Fluent Design Components)
4. **Windows 11 Mica/Backdrop-Effekte**

**Alternative (nicht implementiert):**
- Framework-Dependent Deployment → < 1 Sekunde Start
- Nachteil: Benutzer muss .NET 9 Runtime installieren
- Entscheidung: Self-Contained für bessere Benutzerfreundlichkeit

---
**Nächste Session:** Live-Parsing-Modus (FileWatcher für Combat-Log), DPS-Graph


