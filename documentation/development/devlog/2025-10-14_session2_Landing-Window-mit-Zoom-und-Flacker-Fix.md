## Session 2: Landing Window mit Zoom und Flacker-Fix

**Datum:** 2025-10-14  
**Dauer:** ~3 Stunden  
**Fokus:** Neues Landing Window, Skalierbarkeit, Hover-Animationen

### 🎯 **Was wir erreicht haben:**

#### ✅ **Erfolgreich implementiert:**
1. **Neues Landing Window**
   - Transparentes, framelesses Fenster (120x120px)
   - App-Logo als zentrales Element (100x100px)
   - Hover-aktivierte Buttons um das Logo herum
   - Pin/Unpin-Funktionalität für Fenster-Position

2. **Button-Layout und Funktionalität**
   - **Rechte Buttons:** Dashboard, Graph, Overlay, Log (vertikal gestapelt)
   - **Linke Buttons:** Close, Pin (vertikal gestapelt)
   - **Farbige Hintergründe** für jeden Button-Typ
   - **2,5 Sekunden Timer** für Button-Sichtbarkeit

3. **Skalierbares Layout mit ViewBox**
   - **ViewBox** mit `Stretch="Uniform"` für proportionale Skalierung
   - **Basis-Größe:** 100x100px (Design-Referenz)
   - **Zoom-Funktionalität:** 1.0x - 3.0x (Standard: 1.2x)
   - **Keyboard Shortcuts:** Ctrl+/Ctrl-/Ctrl0 für Zoom

4. **Flacker-Problem behoben**
   - **IsHitTestVisible-Logik** für Button-Klickbarkeit
   - **Einheitliche Hover-Detection** über Grid statt individuelle Button-Events
   - **Timer-Management** für 2,5s Verzögerung

### 🚨 **Probleme und Lösungen:**

#### **Problem 1: Button-Flackern beim Hover**
- **Symptom:** Buttons flackerten beim schnellen Hover zwischen Elementen
- **Ursache:** Individuelle MouseEnter/MouseLeave Events auf Buttons
- **Lösung:** Zentrales HoverPanel mit IsHitTestVisible-Steuerung
- **Code-Änderung:** Events auf Grid-Level, Buttons werden nur bei Sichtbarkeit klickbar

#### **Problem 2: HoverPanel blockierte Button-Klicks**
- **Symptom:** Buttons waren nicht klickbar wegen überlagerndem Panel
- **Ursache:** HoverPanel hatte höchsten Z-Index
- **Lösung:** IsHitTestVisible="False" für unsichtbare Buttons
- **Code-Änderung:** Dynamisches Umschalten der Klickbarkeit

#### **Problem 3: Layout-Skalierung**
- **Symptom:** Buttons wurden bei verschiedenen Fenstergrößen falsch positioniert
- **Ursache:** Feste Pixel-Werte für Margins und Größen
- **Lösung:** ViewBox mit relativen Margins und Zoom-Funktion
- **Code-Änderung:** ViewBox + SetZoom() Methode mit Keyboard Shortcuts

### 🔧 **Technische Details:**

#### **XAML-Struktur:**
```xml
<Window Height="120" Width="120">
    <Viewbox Stretch="Uniform">
        <Grid Width="100" Height="100" MouseEnter="ShowButtons" MouseLeave="StartHideTimer">
            <Image Logo 100x100 />
            <StackPanel IconPanel (rechte Buttons) />
            <StackPanel ClosePinPanel (linke Buttons) />
        </Grid>
    </Viewbox>
</Window>
```

#### **Code-Behind Features:**
- **Timer-Management:** DispatcherTimer für 2,5s Verzögerung
- **Zoom-System:** SetZoom() mit Keyboard Shortcuts
- **Pin-Funktionalität:** Topmost-Toggle mit Icon-Wechsel
- **Overlay-Management:** Single-Instance für LiveCombatOverlay

### 📁 **Geänderte Dateien:**
- `app/Windows/LandingWindow/LandingWindow.xaml` - Komplett neu erstellt
- `app/Windows/LandingWindow/LandingWindow.xaml.cs` - Komplett neu erstellt
- `app/Windows/App/App.xaml.cs` - Startup auf LandingWindow geändert
- `app/App.csproj` - ApplicationDefinition hinzugefügt

### 🎯 **Nächste Schritte:**
- Dashboard-Button Funktionalität implementieren
- Graph-Button Funktionalität implementieren
- Log-Button Integration mit Backend
- UI-Feintuning und Performance-Optimierung

---


