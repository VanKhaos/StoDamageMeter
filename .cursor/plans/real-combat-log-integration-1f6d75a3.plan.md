<!-- 1f6d75a3-8e45-4fd1-8eb7-fa56ac430f98 b6b723f9-2b56-4057-9122-de9512fc2e39 -->
# Combat Log Parsing Verbesserung

## Problem-Zusammenfassung

1. **Datum-Bug**: Combats zeigen falsches Datum (aktuelle Zeit statt Log-Zeit)
2. **Falsches Log-Format-Parsing**: Aktuell wird nur `Owner,OwnerType` geparst, aber Format ist: `Owner,OwnerType,SourceName,SourceType,Target,TargetType,Ability,...`
3. **Combat-Type falsch erkannt**: Muss auf **Target Type** basieren, nicht Source
4. **Companions nicht getrennt**: Companions (Pets, Drohnen, Außenteam) werden als Abilities angezeigt

## Lösung

### Phase 1: Log-Parsing-Logik komplett neu schreiben

**Datei: `Deploy/working_oscr_backend.py`**

#### 1.1 Neue Parse-Funktion für Log-Zeilen erstellen

```python
def parse_combat_log_line(line: str):
    """
    Parsed eine Combat-Log-Zeile vollständig
    Format: YY:MM:DD:HH:MM:SS.ms::Owner,OwnerType,SourceName,SourceType,Target,TargetType,Ability,Pn.XXX,DamageType,Flags,Damage1,Damage2
    
    Returns: dict mit allen geparsten Feldern oder None bei Fehler
    """
  - Split bei '::'
  - Parse Timestamp mit `parse_timestamp()`
  - Split Rest bei ',' (mit Limit, da Ability-Namen ',' enthalten können)
  - Extrahiere Felder:
   - owner_name (Index 0)
   - owner_type (Index 1)
   - source_name (Index 2)
   - source_type (Index 3)
   - target_name (Index 4)
   - target_type (Index 5)
   - ability_name (Index 6)
   - damage_type (Index 8)
   - flags (Index 9)
   - damage_values (Index 10+)
  - Return dict oder None
```

#### 1.2 Combat-Type-Erkennung basierend auf Target

```python
def determine_combat_type_from_line(parsed_line: dict) -> str:
    """
    Ermittelt Combat-Type (Space/Ground) basierend auf Target Type oder Source Type
    """
    target_type = parsed_line.get('target_type', '')
    source_type = parsed_line.get('source_type', '')
    
    # Prüfe Target zuerst
    if 'Space_' in target_type or 'Space_' in source_type:
        return 'Space'
    elif 'Ground_' in target_type or 'Ground_' in source_type:
        return 'Ground'
    
    return 'Space'  # Default
```

#### 1.3 Name-Bereinigung für HTML-Tags

```python
def clean_name(name: str) -> str:
    """
    Entfernt HTML-Tags aus Namen (z.B. <br>, <span>, etc.)
    """
    import re
    return re.sub(r'<[^>]+>', '', name).strip()
```

#### 1.4 Companion-Erkennung implementieren

```python
def identify_source_entity(parsed_line: dict) -> dict:
    """
    Identifiziert ob Source ein Spieler oder Companion ist
    
    WICHTIG: Ein Companion ist alles was NICHT der Spieler selbst ist!
    - Spieler direkt: owner_name == source_name
    - Companion: owner_name != source_name UND source hat Type (C[...] oder S[...])
    
    Returns:
    {
        'player_handle': '@handle',
        'player_name': 'Player Name',
        'is_companion': True/False,
        'companion_name': 'Companion Name' oder None (HTML-bereinigt)
    }
    """
    owner_name = parsed_line['owner_name']
    owner_type = parsed_line['owner_type']
    source_name = parsed_line['source_name']
    source_type = parsed_line['source_type']
    
    # Nur Player-Events verarbeiten
    if not owner_type or 'P[' not in owner_type:
        return None
    
    # Spieler-Handle extrahieren
    handle = None
    if '@' in owner_type:
        handle = owner_type.split('@')[1].split(']')[0]
    
    player_name = owner_name
    
    # Ist Source ein Companion?
    # Logik:
    # 1. Source leer oder '*' → Spieler direkt
    # 2. Source == Owner → Spieler direkt
    # 3. Source != Owner UND Source hat Type → Companion
    is_companion = False
    companion_name = None
    
    # Source leer oder nur Wildcard = Spieler direkt
    if not source_name or source_name.strip() in ('', '*'):
        is_companion = False
    # Source gefüllt und anders als Owner = Companion
    elif source_name != owner_name:
        if source_type and (source_type.startswith('C[') or source_type.startswith('S[')):
            is_companion = True
            companion_name = clean_name(source_name)  # HTML-Tags entfernen!
    
    return {
        'player_handle': handle,
        'player_name': player_name,
        'is_companion': is_companion,
        'companion_name': companion_name
    }
```

### Phase 2: Datenmodell erweitern

**Datei: `Deploy/working_oscr_backend.py`**

#### 2.1 Neue Klasse für Companions

```python
class WorkingCompanionStats:
    def __init__(self, name: str):
        self.name = name
        # Companion-Type wird intern erkannt, aber nicht gespeichert/angezeigt
        self.total_damage = 0.0
        self.dps = 0.0
        self.max_hit = 0.0
        self.combat_time = 0.0
        self.abilities = {}  # Dict[str, WorkingAbilityStats]
        self.hits = 0
        self.crits = 0
        self.total_attacks = 0
        self.crit_percent = 0.0
        self.accuracy_percent = 0.0
        self.debuff = 0.0
```

#### 2.2 WorkingPlayerStats erweitern

```python
class WorkingPlayerStats:
    def __init__(self, ...):
        # Existing fields...
        self.companions = {}  # Dict[str, WorkingCompanionStats]
        
        # Neue Felder für "mit Companions"
        self.total_damage_with_companions = 0.0
        self.dps_with_companions = 0.0
```

### Phase 3: _analyze_combat_players neu implementieren

**Datei: `Deploy/working_oscr_backend.py`**

Komplette Neuimplementierung mit:

1. Alle Zeilen mit `parse_combat_log_line()` parsen
2. Combat-Type-Filter basierend auf `determine_combat_type_from_line()`
3. Für jede Zeile: `identify_source_entity()` aufrufen
4. Unterscheidung:

                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                - Spieler-Damage direkt → `player.abilities[ability_name]`
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                - Companion-Damage → `player.companions[companion_name].abilities[ability_name]`

5. Companion-DPS zum Player-DPS addieren
6. Debug-Logging erweitern mit vollständigen Parse-Details
```python
def _analyze_combat_players(self, log_path: str, start_byte: int, end_byte: int, combat_type: str = None, debug_log: bool = False):
    players = {}
    
    for i in range(start_byte, min(end_byte, len(lines))):
        line = lines[i]
        
        # Parse vollständig
        parsed = parse_combat_log_line(line)
        if not parsed:
            continue
        
        # Combat-Type-Filter
        line_combat_type = determine_combat_type_from_line(parsed)
        if combat_type and line_combat_type != combat_type:
            if debug_log:
                logger.info(f"SKIP: Line is {line_combat_type} but Combat is {combat_type}")
            continue
        
        # Entity identifizieren
        entity = identify_source_entity(parsed)
        if not entity:
            continue
        
        player_name = entity['player_name']
        
        # Player erstellen falls nicht vorhanden
        if player_name not in players:
            players[player_name] = WorkingPlayerStats(name=player_name, ...)
        
        player = players[player_name]
        
        # Damage verarbeiten
        if entity['is_companion']:
            # Companion-Damage
            comp_name = entity['companion_name']
            if comp_name not in player.companions:
                player.companions[comp_name] = WorkingCompanionStats(
                    name=comp_name,
                    companion_type=entity['companion_type']
                )
            
            companion = player.companions[comp_name]
            # Stats zu Companion.Ability hinzufügen
            # ...
            
            # Zum Player-Gesamt addieren
            player.total_damage_with_companions += damage_value
        else:
            # Direkte Player-Damage
            # Stats zu Player.Ability hinzufügen
            # ...
    
    # DPS berechnen (mit Companions)
    for player in players.values():
        player.dps_with_companions = player.total_damage_with_companions / combat_time
        
        for companion in player.companions.values():
            companion.dps = companion.total_damage / combat_time
```


### Phase 4: Frontend-Datenmodelle erweitern

**Datei: `frontend/Models/OSCRModels.cs`**

```csharp
public class CompanionStatistics
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }
    
    [JsonPropertyName("companionType")]
    public string? CompanionType { get; set; }
    
    [JsonPropertyName("dps")]
    public double Dps { get; set; }
    
    [JsonPropertyName("totalDamage")]
    public double TotalDamage { get; set; }
    
    [JsonPropertyName("debuff")]
    public double Debuff { get; set; }
    
    [JsonPropertyName("maxOneHit")]
    public double MaxOneHit { get; set; }
    
    [JsonPropertyName("critPercent")]
    public double CritPercent { get; set; }
    
    [JsonPropertyName("accuracyPercent")]
    public double AccuracyPercent { get; set; }
    
    [JsonPropertyName("abilities")]
    public List<AbilityStatistics> Abilities { get; set; } = new();
}

public class PlayerStatistics
{
    // Existing fields...
    
    [JsonPropertyName("companions")]
    public List<CompanionStatistics> Companions { get; set; } = new();
    
    [JsonPropertyName("dpsWithCompanions")]
    public double DpsWithCompanions { get; set; }
    
    [JsonPropertyName("totalDamageWithCompanions")]
    public double TotalDamageWithCompanions { get; set; }
}
```

### Phase 5: UI für 3-stufige Hierarchie anpassen

**Datei: `frontend/MainWindow.xaml.cs`**

#### 5.1 PopulateCombatStatsTreeView erweitern

Neue Struktur:

1. Player-Zeile (mit DPS inkl. Companions)
2. → Player-eigene Abilities (eingerückt)
3. → Companions (eingerückt, mit eigenem DPS)
4. → → Companion-Abilities (doppelt eingerückt)
```csharp
private void PopulateCombatStatsTreeView(CombatData combatData)
{
    CombatStatsItemsControl.Items.Clear();
    
    var sortedPlayers = combatData.Players.Values
        .OrderByDescending(p => p.DpsWithCompanions)  // Mit Companions sortieren!
        .ToList();
    
    foreach (var player in sortedPlayers)
    {
        // Player-Expander (zeigt DPS inkl. Companions)
        var playerExpander = CreatePlayerExpander(player);
        
        // Player-Content: Abilities + Companions
        var playerContent = new StackPanel();
        
        // 1. Player-eigene Abilities
        foreach (var ability in player.Abilities.OrderByDescending(a => a.Dps))
        {
            var abilityRow = CreateAbilityRow(ability, indent: 32);
            playerContent.Children.Add(abilityRow);
        }
        
        // 2. Companions
        foreach (var companion in player.Companions.OrderByDescending(c => c.Dps))
        {
            // Companion-Expander (eingerückt)
            var companionExpander = CreateCompanionExpander(companion, indent: 32);
            
            // Companion-Content: Abilities
            var companionContent = new StackPanel();
            foreach (var ability in companion.Abilities.OrderByDescending(a => a.Dps))
            {
                var abilityRow = CreateAbilityRow(ability, indent: 64);  // Doppelt eingerückt
                companionContent.Children.Add(abilityRow);
            }
            
            companionExpander.Content = companionContent;
            playerContent.Children.Add(companionExpander);
        }
        
        playerExpander.Content = playerContent;
        CombatStatsItemsControl.Items.Add(playerExpander);
    }
}
```


#### 5.2 Helper-Methoden für Companion-Zeilen

```csharp
private Expander CreateCompanionExpander(CompanionStatistics companion, int indent)
{
    // Ähnlich wie Player, aber:
    // - Kleinere Schrift
    // - Andere Farbe (z.B. leicht transparenter)
    // - Icon für Companion-Type (🤖 für Drone, 🚁 für Pet, 👤 für Away Team)
    // - Indent für Einrückung
}
```

### Phase 6: Datum-Parsing-Bug beheben

**Datei: `Deploy/working_oscr_backend.py`**

In `isolate_combats()` bei Combat-Erstellung:

```python
# WICHTIG: Korrekte Zeile für Datum verwenden!
# Wir lesen von hinten (reversed), also ist lines[i] die AKTUELLE Zeile
# Für Combat-Ende (neueste Zeile des Combats) nehmen wir die ERSTE Zeile der Gruppe

# Bei Combat-Erstellung:
first_line_of_combat = current_combat_lines[0]  # Neueste Zeile des Combats
time_parts = first_line_of_combat.split('::')[0].split(':')

# Oder besser: Verwende last_timestamp (aktuellsten Timestamp)
date_str = last_timestamp.strftime("%Y-%m-%d")
time_str = last_timestamp.strftime("%H:%M:%S")
```

## Implementierungs-Reihenfolge

1. Neue Parse-Funktionen im Backend erstellen
2. Combat-Type-Erkennung auf Target umstellen
3. Companion-Erkennung implementieren
4. Datenmodell-Klassen erweitern (Backend)
5. `_analyze_combat_players` komplett neu schreiben
6. Datum-Parsing-Bug beheben
7. Backend testen (mit Debug-Logs)
8. Frontend-Modelle erweitern
9. UI-Hierarchie für 3 Ebenen implementieren
10. Gesamttest

## Wichtige Test-Zeilen

**Spieler-Direktschaden:**

```
25:10:04:14:30:36.9::Van Khaos,P[12698228@19236104 Van Khaos@vankhaos#2007],Van Khaos,P[...],Target,C[...],Ability,...
```

**Companion (Pet):**

```
25:10:08:11:41:37.2::Van Khaos,P[...],Elite-Typ-7-Shuttle,C[774 Space_Fed_Shuttle_Type7_Cheyenne_Pet_3],Target,...
```

**Companion (Drohne):**

```
25:10:04:14:29:54.0::Van Khaos,P[...],KIDD-Meister-Drohne,C[5 Ground_Universal_Kit_Cb33_Kidd_Drone_Combined],Target,...
```

**Companion (Außenteam):**

```
25:10:04:14:29:55.2::Van Khaos,P[...],Sillar,S[139588389],Target,C[...],Blitzschlag,...
```

### To-dos

- [ ] Add ability tracking to _analyze_combat_players in backend
- [ ] Create WorkingAbilityStats class
- [ ] Add analyze_single action handler to backend
- [ ] Add AbilityStatistics class to OSCRModels.cs
- [ ] Extend PlayerStatistics with Abilities, CritPercent, AccuracyPercent
- [ ] Add AnalyzeSingleCombatAsync method to OSCRBackendService.cs
- [ ] Replace data table placeholder with TreeView in MainWindow.xaml
- [ ] Style TreeView with Star Trek Blue theme and expandable rows
- [ ] Implement combat selection handler in MainWindow.xaml.cs
- [ ] Bind combat data to TreeView
- [ ] Test combat selection and data display