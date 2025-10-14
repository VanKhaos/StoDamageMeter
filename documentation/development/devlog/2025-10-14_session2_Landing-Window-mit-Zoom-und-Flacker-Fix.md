## Session 2: Landing Window mit Zoom und Flacker-Fix

**Datum:** 2025-10-14  
**Dauer:** ~3 Stunden  
**Fokus:** Neues Landing Window, Skalierbarkeit, Hover-Animationen

### ðŸŽ¯ **Was wir erreicht haben:**

#### âœ… **Erfolgreich implementiert:**
1. **Neues Landing Window**
   - Transparentes, framelesses Fenster (120x120px)
   - App-Logo als zentrales Element (100x100px)
   - Hover-aktivierte Buttons um das Logo herum
   - Pin/Unpin-FunktionalitÃ¤t fÃ¼r Fenster-Position

2. **Button-Layout und FunktionalitÃ¤t**
   - **Rechte Buttons:** Dashboard, Graph, Overlay, Log (vertikal gestapelt)
   - **Linke Buttons:** Close, Pin (vertikal gestapelt)
   - **Farbige HintergrÃ¼nde** fÃ¼r jeden Button-Typ
   - **2,5 Sekunden Timer** fÃ¼r Button-Sichtbarkeit

3. **Skalierbares Layout mit ViewBox**
   - **ViewBox** mit `Stretch="Uniform"` fÃ¼r proportionale Skalierung
   - **Basis-GrÃ¶ÃŸe:** 100x100px (Design-Referenz)
   - **Zoom-FunktionalitÃ¤t:** 1.0x - 3.0x (Standard: 1.2x)
   - **Keyboard Shortcuts:** Ctrl+/Ctrl-/Ctrl0 fÃ¼r Zoom

4. **Flacker-Problem behoben**
   - **IsHitTestVisible-Logik** fÃ¼r Button-Klickbarkeit
   - **Einheitliche Hover-Detection** Ã¼ber Grid statt individuelle Button-Events
   - **Timer-Management** fÃ¼r 2,5s VerzÃ¶gerung

### ðŸš¨ **Probleme und LÃ¶sungen:**

#### **Problem 1: Button-Flackern beim Hover**
- **Symptom:** Buttons flackerten beim schnellen Hover zwischen Elementen
- **Ursache:** Individuelle MouseEnter/MouseLeave Events auf Buttons
- **LÃ¶sung:** Zentrales HoverPanel mit IsHitTestVisible-Steuerung
- **Code-Ã„nderung:** Events auf Grid-Level, Buttons werden nur bei Sichtbarkeit klickbar

#### **Problem 2: HoverPanel blockierte Button-Klicks**
- **Symptom:** Buttons waren nicht klickbar wegen Ã¼berlagerndem Panel
- **Ursache:** HoverPanel hatte hÃ¶chsten Z-Index
- **LÃ¶sung:** IsHitTestVisible="False" fÃ¼r unsichtbare Buttons
- **Code-Ã„nderung:** Dynamisches Umschalten der Klickbarkeit

#### **Problem 3: Layout-Skalierung**
- **Symptom:** Buttons wurden bei verschiedenen FenstergrÃ¶ÃŸen falsch positioniert
- **Ursache:** Feste Pixel-Werte fÃ¼r Margins und GrÃ¶ÃŸen
- **LÃ¶sung:** ViewBox mit relativen Margins und Zoom-Funktion
- **Code-Ã„nderung:** ViewBox + SetZoom() Methode mit Keyboard Shortcuts

### ðŸ”§ **Technische Details:**

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
- **Timer-Management:** DispatcherTimer fÃ¼r 2,5s VerzÃ¶gerung
- **Zoom-System:** SetZoom() mit Keyboard Shortcuts
- **Pin-FunktionalitÃ¤t:** Topmost-Toggle mit Icon-Wechsel
- **Overlay-Management:** Single-Instance fÃ¼r LiveCombatOverlay

### ðŸ“ **GeÃ¤nderte Dateien:**
- `frontend/Windows/LandingWindow/LandingWindow.xaml` - Komplett neu erstellt
- `frontend/Windows/LandingWindow/LandingWindow.xaml.cs` - Komplett neu erstellt
- `frontend/Windows/App/App.xaml.cs` - Startup auf LandingWindow geÃ¤ndert
- `frontend/frontend.csproj` - ApplicationDefinition hinzugefÃ¼gt

### ðŸŽ¯ **NÃ¤chste Schritte:**
- Dashboard-Button FunktionalitÃ¤t implementieren
- Graph-Button FunktionalitÃ¤t implementieren
- Log-Button Integration mit Backend
- UI-Feintuning und Performance-Optimierung

---


