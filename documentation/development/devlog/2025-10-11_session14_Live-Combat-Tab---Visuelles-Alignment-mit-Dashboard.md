## Session 14: Live Combat Tab - Visuelles Alignment mit Dashboard

**Datum:** 2025-10-11  
**Dauer:** ~3 Stunden  
**Fokus:** Live Combat Tab identisch zu Dashboard gestalten, Expander-Style-Fixes, Layout-Optimierungen

### 🎯 **Was wir erreicht haben:**

#### ✅ **Erfolgreich implementiert:**

1. **Live Combat Tab-Aktivierung nach Log-Laden**
   - Tab war immer aktiviert → Fehleranfällig
   - **Lösung:** `IsEnabled="False"` im XAML, aktiviert nach erfolgreichem Combat-Log-Laden
   - **XAML:** `<TabItem Header="Live Combat" x:Name="LiveCombatTab" IsEnabled="False">`
   - **Code:** `LiveCombatTab.IsEnabled = true;` in `LoadCombatListAsync()`
   - **Resultat:** Tab nur klickbar wenn Combat-Log geladen ist

2. **Automatische Combat-Liste-Aktualisierung**
   - **Problem:** Beendete Live Combats erschienen nicht in der Liste
   - **Lösung:** `OnLiveCombatCompleted()` lädt automatisch `LoadCombatListAsync()` neu
   - **Event-Flow:**
     ```
     LiveCombatViewModel.CombatCompleted Event
         ↓
     MainWindow.OnLiveCombatCompleted()
         ↓
     LoadCombatListAsync(_currentLogPath)
         ↓
     Combat-Liste zeigt neuen Combat
     ```
   - **Resultat:** Combat erscheint sofort in der Liste nach Ende

3. **Expander-Collapse-Bug behoben**
   - **Problem:** PropertyChanged "CombatDuration" löste alle Sekunde komplettes UI-Refresh aus
   - **Symptom:** Expander klappten sofort wieder zu beim Aufklappen
   - **Ursache:** `UpdateLiveStats()` löschte `ItemsControl.Items.Clear()` → alle Expander-States verloren
   - **Lösung:**
     - **VOR** dem Clear: Expander-States in Dictionary speichern (Spielername → IsExpanded)
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
   - **Resultat:** Expander bleiben offen während Live-Updates

4. **Identisches Layout: Live Combat = Dashboard**
   - **Problem:** Live Combat hatte eigene Struktur, sah anders aus als Dashboard
   - **Lösung:** Komplettes Layout-Refactoring auf Dashboard-Struktur
   - **Änderungen:**
     - Eine große `ui:Card` statt zwei separate Cards
     - Combat-Info-Header kompakt im Grid.Row="0" (nur bei aktivem Combat sichtbar)
     - `CombatStatsHeader` Component im Grid.Row="1"
     - Daten-Tabelle im Grid.Row="2"
     - Gleiche Margins: `Margin="16,8,16,16"` und `Padding="20"`
     - `VerticalAlignment="Top"` für oben-bündige Ausrichtung
   - **Resultat:** Visuell identisch zum Dashboard-Tab

5. **Expander-Style global verfügbar**
   - **Problem:** `NoToggleIconExpanderStyle` wurde in Live Combat nicht gefunden
   - **Symptom:** Weißes Kreis-Icon links neben Spielernamen (Standard-WPF-Toggle)
   - **Ursache:** `ExpanderStyles.xaml` war nicht in App.xaml eingebunden
   - **Lösung:** 
     ```xml
     <!-- App.xaml -->
     <ResourceDictionary.MergedDictionaries>
         <ui:ThemesDictionary Theme="Dark" />
         <ui:ControlsDictionary />
         <ResourceDictionary Source="Styles/ExpanderStyles.xaml"/>
     </ResourceDictionary.MergedDictionaries>
     ```
   - **Resultat:** Style wird gefunden, kein weißes Icon mehr

6. **LiveCombatView.xaml.cs: Richtiger Style geladen**
   - **Problem:** `CombatStatsExpanderStyle` existierte nicht
   - **Lösung:** Auf `NoToggleIconExpanderStyle` geändert (wie in Dashboard)
   - **Code:**
     ```csharp
     var expanderStyle = Application.Current.TryFindResource("NoToggleIconExpanderStyle") as Style;
     _statsRenderer = new CombatStatsRenderer(expanderStyle!);
     ```

7. **CombatStatsRenderer: Player Header Alignment**
   - **Problem:** Player-Name-Zeile hatte keine Spalten-Ausrichtung mit Headers
   - **Ursache:** Kein Padding, Werte nicht aligned
   - **Erkenntnis:** Padding wird vom `ToggleButton` im Expander-Style übernommen (12,8)
   - **Lösung:** Kein manuelles Padding auf Header-Grid (Style macht das)
   - **Resultat:** Perfekte Ausrichtung mit Column-Headers

8. **Spacing-Optimierungen**
   - **Player-Container:** Margin von `4px` auf `2px` reduziert (kompakteres Layout)
   - **Ability-Rows:** Padding von `(12, 8)` auf `(12, 6)` reduziert
   - **Resultat:** Identisch kompakt wie Dashboard

9. **Ungenutzte Datei entfernt**
   - `CombatStatsPlayerRow.xaml` gelöscht
   - War nie verwendet (beide Tabs nutzen `CombatStatsRenderer`)
   - Projekt aufgeräumt

10. **Tab-Header umbenannt**
    - "Damage Out" → "Dashboard"
    - Bessere Beschreibung der Funktionalität

### 🔧 **Technische Details:**

#### **Live Combat Layout-Struktur:**
```xml
<Grid Background="{StaticResource StarTrekDeepBlack}">
    <!-- Nur 1 Row für die große Card -->
    <RowDefinition Height="*"/>
    
    <!-- Eine große Card (wie Dashboard) -->
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

### 🚨 **Gelöste Probleme:**

#### **Problem 1: Weißes Kreis-Icon links vom Spielernamen**
- **Symptom:** Im Live Combat sichtbar, im Dashboard nicht
- **Ursache:** `ExpanderStyles.xaml` nicht global verfügbar
- **Lösung:** In `App.xaml` als MergedDictionary eingebunden
- **Resultat:** ✅ Beide Tabs verwenden gleichen Style

#### **Problem 2: Expander klappen sofort zu**
- **Symptom:** Beim Aufklappen sofort wieder geschlossen
- **Ursache:** `CombatDuration` PropertyChanged jede Sekunde → `UpdateLiveStats()` → `Items.Clear()` → States verloren
- **Lösung:** State-Preservation-Algorithmus implementiert
- **Resultat:** ✅ Expander bleiben offen

#### **Problem 3: Player-Werte nicht aligned mit Headers**
- **Symptom:** DPS, Total Damage etc. nicht in richtigen Spalten
- **Ursache:** Player-Header-Grid hatte eigenes Padding (Konflikt mit Style-Padding)
- **Lösung:** Padding entfernt, Style-Padding verwenden
- **Resultat:** ✅ Perfekte Ausrichtung

#### **Problem 4: Live Combat sieht anders aus als Dashboard**
- **Symptom:** Unterschiedliche Abstände, Layout, Design
- **Ursache:** Eigene Card-Struktur statt Dashboard-Struktur
- **Lösung:** Komplettes Refactoring auf Dashboard-Layout
- **Resultat:** ✅ Visuell identisch

#### **Problem 5: Live Combat Tab immer klickbar**
- **Symptom:** Fehler wenn kein Log geladen
- **Lösung:** `IsEnabled="False"` bis Log geladen
- **Resultat:** ✅ Kein falscher Tab-Zugriff mehr

### 📁 **Wichtige Dateien:**

**Aktualisiert:**
- `app/MainWindow.xaml` - Tab-Header, LiveCombatTab.IsEnabled
- `app/MainWindow.xaml.cs` - OnLiveCombatCompleted, LiveCombatTab.IsEnabled
- `app/Components/LiveCombat/LiveCombatView.xaml` - Komplettes Layout-Refactoring
- `app/Components/LiveCombat/LiveCombatView.xaml.cs` - State-Preservation, Style-Fix
- `app/Services/CombatStatsRenderer.cs` - Player Header Padding entfernt, Margins reduziert
- `app/App.xaml` - ExpanderStyles.xaml eingebunden

**Gelöscht:**
- `app/Components/Combat/CombatStatsPlayerRow.xaml` - Ungenutzte Komponente

### 💡 **Lessons Learned:**

1. **Resource-Scope:** Custom Styles müssen in App.xaml oder mit MergedDictionaries global verfügbar sein
2. **State-Preservation:** Bei dynamischem UI-Refresh immer User-State bewahren (Expander, Selection, Scroll-Position)
3. **Layout-Konsistenz:** Gleiche Komponente = gleiches Layout = bessere UX
4. **PropertyChanged Performance:** Zu häufige Updates können UI-Probleme verursachen
5. **Padding-Konflikte:** Style-Padding und manuelles Padding können sich überschreiben
6. **Component-Struktur:** Dashboard-Pattern als Template für neue Tabs verwenden

### 🔄 **Build-Status:**

- ✅ App kompiliert erfolgreich
- ✅ Keine Linter-Fehler
- ✅ Live Combat Tab visuell identisch zu Dashboard
- ✅ Expander-States bleiben erhalten
- ✅ Combat-Liste aktualisiert automatisch
- ✅ Tab-Aktivierung funktioniert korrekt
- ✅ Debug-Version aktualisiert

### 🎨 **UI-Qualität:**

**Vor den Fixes:**
- ❌ Weißes Kreis-Icon (WPF-Standard)
- ❌ Player-Werte nicht aligned
- ❌ Expander klappen zu beim Update
- ❌ Anderes Layout als Dashboard
- ❌ Zu viel Abstand zwischen Zeilen

**Nach den Fixes:**
- ✅ Kein Toggle-Icon (wie Dashboard)
- ✅ Perfekte Spalten-Ausrichtung
- ✅ Expander bleiben offen
- ✅ Identisches Layout zum Dashboard
- ✅ Kompaktes, professionelles Design

### 📊 **Vergleich Dashboard vs. Live Combat:**

**Struktur:**
- ✅ Beide nutzen `CombatStatsHeader` Component
- ✅ Beide nutzen `CombatStatsRenderer` Service
- ✅ Beide nutzen `NoToggleIconExpanderStyle`
- ✅ Gleiche Card-Margins (16,8,16,16) und Padding (20)
- ✅ Gleiche Spalten-Definitionen
- ✅ Gleiche Spacing-Werte

**Unterschiede (gewollt):**
- Dashboard: Combat-Auswahl aus Liste
- Live Combat: Automatische Aktualisierung während Kampf
- Live Combat: Combat-Info-Header mit Duration/DPS

### 🎯 **Features komplett:**

- ✅ Live Combat Tab erst nach Log-Laden aktiviert
- ✅ Combat-Liste aktualisiert automatisch nach Live Combat
- ✅ Expander bleiben beim Update offen
- ✅ Visuell identisch zum Dashboard
- ✅ Kein weißes Toggle-Icon
- ✅ Perfekte Spalten-Ausrichtung
- ✅ Kompaktes Layout

---
**Nächste Session:** Combat-Type-Change-Erkennung verfeinern, Performance-Tests mit vielen Spielern, Live Combat DPS Popup Overlay, Tabellen spalten im Live Combat richtig darstellen


