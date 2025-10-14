## Session 14: Live Combat Tab - Visuelles Alignment mit Dashboard

**Datum:** 2025-10-11  
**Dauer:** ~3 Stunden  
**Fokus:** Live Combat Tab identisch zu Dashboard gestalten, Expander-Style-Fixes, Layout-Optimierungen

### ðŸŽ¯ **Was wir erreicht haben:**

#### âœ… **Erfolgreich implementiert:**

1. **Live Combat Tab-Aktivierung nach Log-Laden**
   - Tab war immer aktiviert â†’ FehleranfÃ¤llig
   - **LÃ¶sung:** `IsEnabled="False"` im XAML, aktiviert nach erfolgreichem Combat-Log-Laden
   - **XAML:** `<TabItem Header="Live Combat" x:Name="LiveCombatTab" IsEnabled="False">`
   - **Code:** `LiveCombatTab.IsEnabled = true;` in `LoadCombatListAsync()`
   - **Resultat:** Tab nur klickbar wenn Combat-Log geladen ist

2. **Automatische Combat-Liste-Aktualisierung**
   - **Problem:** Beendete Live Combats erschienen nicht in der Liste
   - **LÃ¶sung:** `OnLiveCombatCompleted()` lÃ¤dt automatisch `LoadCombatListAsync()` neu
   - **Event-Flow:**
     ```
     LiveCombatViewModel.CombatCompleted Event
         â†“
     MainWindow.OnLiveCombatCompleted()
         â†“
     LoadCombatListAsync(_currentLogPath)
         â†“
     Combat-Liste zeigt neuen Combat
     ```
   - **Resultat:** Combat erscheint sofort in der Liste nach Ende

3. **Expander-Collapse-Bug behoben**
   - **Problem:** PropertyChanged "CombatDuration" lÃ¶ste alle Sekunde komplettes UI-Refresh aus
   - **Symptom:** Expander klappten sofort wieder zu beim Aufklappen
   - **Ursache:** `UpdateLiveStats()` lÃ¶schte `ItemsControl.Items.Clear()` â†’ alle Expander-States verloren
   - **LÃ¶sung:**
     - **VOR** dem Clear: Expander-States in Dictionary speichern (Spielername â†’ IsExpanded)
     - **NACH** dem Rendering: States aus Dictionary wiederherstellen
   - **Code:**
     ```csharp
     // States speichern
     var expandedStates = new Dictionary<string, bool>();
     foreach (var expander in LiveStatsItemsControl.Items.OfType<Expander>()) {
         expandedStates[playerName] = expander.IsExpanded;
     }
     
     // Clear + Render...
     
     // States wiederherstellen
     foreach (var expander in LiveStatsItemsControl.Items.OfType<Expander>()) {
         if (expandedStates.TryGetValue(playerName, out bool wasExpanded))
             expander.IsExpanded = wasExpanded;
     }
     ```
   - **Resultat:** Expander bleiben offen wÃ¤hrend Live-Updates

4. **Identisches Layout: Live Combat = Dashboard**
   - **Problem:** Live Combat hatte eigene Struktur, sah anders aus als Dashboard
   - **LÃ¶sung:** Komplettes Layout-Refactoring auf Dashboard-Struktur
   - **Ã„nderungen:**
     - Eine groÃŸe `ui:Card` statt zwei separate Cards
     - Combat-Info-Header kompakt im Grid.Row="0" (nur bei aktivem Combat sichtbar)
     - `CombatStatsHeader` Component im Grid.Row="1"
     - Daten-Tabelle im Grid.Row="2"
     - Gleiche Margins: `Margin="16,8,16,16"` und `Padding="20"`
     - `VerticalAlignment="Top"` fÃ¼r oben-bÃ¼ndige Ausrichtung
   - **Resultat:** Visuell identisch zum Dashboard-Tab

5. **Expander-Style global verfÃ¼gbar**
   - **Problem:** `NoToggleIconExpanderStyle` wurde in Live Combat nicht gefunden
   - **Symptom:** WeiÃŸes Kreis-Icon links neben Spielernamen (Standard-WPF-Toggle)
   - **Ursache:** `ExpanderStyles.xaml` war nicht in App.xaml eingebunden
   - **LÃ¶sung:** 
     ```xml
     <!-- App.xaml -->
     <ResourceDictionary.MergedDictionaries>
         <ui:ThemesDictionary Theme="Dark" />
         <ui:ControlsDictionary />
         <ResourceDictionary Source="Styles/ExpanderStyles.xaml"/>
     </ResourceDictionary.MergedDictionaries>
     ```
   - **Resultat:** Style wird gefunden, kein weiÃŸes Icon mehr

6. **LiveCombatView.xaml.cs: Richtiger Style geladen**
   - **Problem:** `CombatStatsExpanderStyle` existierte nicht
   - **LÃ¶sung:** Auf `NoToggleIconExpanderStyle` geÃ¤ndert (wie in Dashboard)
   - **Code:**
     ```csharp
     var expanderStyle = Application.Current.TryFindResource("NoToggleIconExpanderStyle") as Style;
     _statsRenderer = new CombatStatsRenderer(expanderStyle!);
     ```

7. **CombatStatsRenderer: Player Header Alignment**
   - **Problem:** Player-Name-Zeile hatte keine Spalten-Ausrichtung mit Headers
   - **Ursache:** Kein Padding, Werte nicht aligned
   - **Erkenntnis:** Padding wird vom `ToggleButton` im Expander-Style Ã¼bernommen (12,8)
   - **LÃ¶sung:** Kein manuelles Padding auf Header-Grid (Style macht das)
   - **Resultat:** Perfekte Ausrichtung mit Column-Headers

8. **Spacing-Optimierungen**
   - **Player-Container:** Margin von `4px` auf `2px` reduziert (kompakteres Layout)
   - **Ability-Rows:** Padding von `(12, 8)` auf `(12, 6)` reduziert
   - **Resultat:** Identisch kompakt wie Dashboard

9. **Ungenutzte Datei entfernt**
   - `CombatStatsPlayerRow.xaml` gelÃ¶scht
   - War nie verwendet (beide Tabs nutzen `CombatStatsRenderer`)
   - Projekt aufgerÃ¤umt

10. **Tab-Header umbenannt**
    - "Damage Out" â†’ "Dashboard"
    - Bessere Beschreibung der FunktionalitÃ¤t

### ðŸ”§ **Technische Details:**

#### **Live Combat Layout-Struktur:**
```xml
<Grid Background="{StaticResource StarTrekDeepBlack}">
    <!-- Nur 1 Row fÃ¼r die groÃŸe Card -->
    <RowDefinition Height="*"/>
    
    <!-- Eine groÃŸe Card (wie Dashboard) -->
    <ui:Card Grid.Row="0" Margin="16,8,16,16" Padding="20" VerticalAlignment="Top">
        <Grid>
            <RowDefinition Height="Auto"/>  <!-- Combat Info -->
            <RowDefinition Height="Auto"/>  <!-- Column Headers -->
            <RowDefinition Height="*"/>     <!-- Data Table -->
        </Grid>
        
        <!-- Combat-Info nur bei aktivem Combat -->
        <Grid x:Name="CombatInfoPanel" Grid.Row="0" Visibility="Collapsed">
            <!-- Type Icon, Duration, Total DPS -->
        </Grid>
        
        <!-- CombatStatsHeader Component -->
        <combat:CombatStatsHeader Grid.Row="1" />
        
        <!-- Data Table mit ScrollViewer -->
        <Border Grid.Row="2">
            <ScrollViewer>
                <ItemsControl x:Name="LiveStatsItemsControl" />
            </ScrollViewer>
        </Border>
    </ui:Card>
</Grid>
```

#### **Expander-State-Preservation-Algorithmus:**
```csharp
private void UpdateLiveStats()
{
    // 1. States speichern
    var expandedStates = new Dictionary<string, bool>();
    foreach (var item in LiveStatsItemsControl.Items)
    {
        if (item is Expander expander && expander.Header is Grid headerGrid)
        {
            var nameText = headerGrid.Children.OfType<TextBlock>()
                .FirstOrDefault(tb => tb.Name == "PlayerNameText");
            if (nameText != null)
                expandedStates[nameText.Text] = expander.IsExpanded;
        }
    }
    
    // 2. Clear und neu rendern
    LiveStatsItemsControl.Items.Clear();
    _statsRenderer.RenderCombatStats(...);
    
    // 3. States wiederherstellen
    foreach (var item in LiveStatsItemsControl.Items)
    {
        if (item is Expander expander && expander.Header is Grid headerGrid)
        {
            var nameText = headerGrid.Children.OfType<TextBlock>()
                .FirstOrDefault(tb => tb.Name == "PlayerNameText");
            if (nameText != null && expandedStates.TryGetValue(nameText.Text, out bool wasExpanded))
                expander.IsExpanded = wasExpanded;
        }
    }
}
```

### ðŸš¨ **GelÃ¶ste Probleme:**

#### **Problem 1: WeiÃŸes Kreis-Icon links vom Spielernamen**
- **Symptom:** Im Live Combat sichtbar, im Dashboard nicht
- **Ursache:** `ExpanderStyles.xaml` nicht global verfÃ¼gbar
- **LÃ¶sung:** In `App.xaml` als MergedDictionary eingebunden
- **Resultat:** âœ… Beide Tabs verwenden gleichen Style

#### **Problem 2: Expander klappen sofort zu**
- **Symptom:** Beim Aufklappen sofort wieder geschlossen
- **Ursache:** `CombatDuration` PropertyChanged jede Sekunde â†’ `UpdateLiveStats()` â†’ `Items.Clear()` â†’ States verloren
- **LÃ¶sung:** State-Preservation-Algorithmus implementiert
- **Resultat:** âœ… Expander bleiben offen

#### **Problem 3: Player-Werte nicht aligned mit Headers**
- **Symptom:** DPS, Total Damage etc. nicht in richtigen Spalten
- **Ursache:** Player-Header-Grid hatte eigenes Padding (Konflikt mit Style-Padding)
- **LÃ¶sung:** Padding entfernt, Style-Padding verwenden
- **Resultat:** âœ… Perfekte Ausrichtung

#### **Problem 4: Live Combat sieht anders aus als Dashboard**
- **Symptom:** Unterschiedliche AbstÃ¤nde, Layout, Design
- **Ursache:** Eigene Card-Struktur statt Dashboard-Struktur
- **LÃ¶sung:** Komplettes Refactoring auf Dashboard-Layout
- **Resultat:** âœ… Visuell identisch

#### **Problem 5: Live Combat Tab immer klickbar**
- **Symptom:** Fehler wenn kein Log geladen
- **LÃ¶sung:** `IsEnabled="False"` bis Log geladen
- **Resultat:** âœ… Kein falscher Tab-Zugriff mehr

### ðŸ“ **Wichtige Dateien:**

**Aktualisiert:**
- `frontend/MainWindow.xaml` - Tab-Header, LiveCombatTab.IsEnabled
- `frontend/MainWindow.xaml.cs` - OnLiveCombatCompleted, LiveCombatTab.IsEnabled
- `frontend/Components/LiveCombat/LiveCombatView.xaml` - Komplettes Layout-Refactoring
- `frontend/Components/LiveCombat/LiveCombatView.xaml.cs` - State-Preservation, Style-Fix
- `frontend/Services/CombatStatsRenderer.cs` - Player Header Padding entfernt, Margins reduziert
- `frontend/App.xaml` - ExpanderStyles.xaml eingebunden

**GelÃ¶scht:**
- `frontend/Components/Combat/CombatStatsPlayerRow.xaml` - Ungenutzte Komponente

### ðŸ’¡ **Lessons Learned:**

1. **Resource-Scope:** Custom Styles mÃ¼ssen in App.xaml oder mit MergedDictionaries global verfÃ¼gbar sein
2. **State-Preservation:** Bei dynamischem UI-Refresh immer User-State bewahren (Expander, Selection, Scroll-Position)
3. **Layout-Konsistenz:** Gleiche Komponente = gleiches Layout = bessere UX
4. **PropertyChanged Performance:** Zu hÃ¤ufige Updates kÃ¶nnen UI-Probleme verursachen
5. **Padding-Konflikte:** Style-Padding und manuelles Padding kÃ¶nnen sich Ã¼berschreiben
6. **Component-Struktur:** Dashboard-Pattern als Template fÃ¼r neue Tabs verwenden

### ðŸ”„ **Build-Status:**

- âœ… Frontend kompiliert erfolgreich
- âœ… Keine Linter-Fehler
- âœ… Live Combat Tab visuell identisch zu Dashboard
- âœ… Expander-States bleiben erhalten
- âœ… Combat-Liste aktualisiert automatisch
- âœ… Tab-Aktivierung funktioniert korrekt
- âœ… Debug-Version aktualisiert

### ðŸŽ¨ **UI-QualitÃ¤t:**

**Vor den Fixes:**
- âŒ WeiÃŸes Kreis-Icon (WPF-Standard)
- âŒ Player-Werte nicht aligned
- âŒ Expander klappen zu beim Update
- âŒ Anderes Layout als Dashboard
- âŒ Zu viel Abstand zwischen Zeilen

**Nach den Fixes:**
- âœ… Kein Toggle-Icon (wie Dashboard)
- âœ… Perfekte Spalten-Ausrichtung
- âœ… Expander bleiben offen
- âœ… Identisches Layout zum Dashboard
- âœ… Kompaktes, professionelles Design

### ðŸ“Š **Vergleich Dashboard vs. Live Combat:**

**Struktur:**
- âœ… Beide nutzen `CombatStatsHeader` Component
- âœ… Beide nutzen `CombatStatsRenderer` Service
- âœ… Beide nutzen `NoToggleIconExpanderStyle`
- âœ… Gleiche Card-Margins (16,8,16,16) und Padding (20)
- âœ… Gleiche Spalten-Definitionen
- âœ… Gleiche Spacing-Werte

**Unterschiede (gewollt):**
- Dashboard: Combat-Auswahl aus Liste
- Live Combat: Automatische Aktualisierung wÃ¤hrend Kampf
- Live Combat: Combat-Info-Header mit Duration/DPS

### ðŸŽ¯ **Features komplett:**

- âœ… Live Combat Tab erst nach Log-Laden aktiviert
- âœ… Combat-Liste aktualisiert automatisch nach Live Combat
- âœ… Expander bleiben beim Update offen
- âœ… Visuell identisch zum Dashboard
- âœ… Kein weiÃŸes Toggle-Icon
- âœ… Perfekte Spalten-Ausrichtung
- âœ… Kompaktes Layout

---
**NÃ¤chste Session:** Combat-Type-Change-Erkennung verfeinern, Performance-Tests mit vielen Spielern, Live Combat DPS Popup Overlay, Tabellen spalten im Live Combat richtig darstellen


