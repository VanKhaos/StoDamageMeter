## Session 4: Combat-Statistiken, Companion-Parsing & Combat-Type-Erkennung

**Datum:** 2025-10-10  
**Dauer:** ~6 Stunden  
**Fokus:** Combat-Details-Anzeige, Companion-Parsing, Combat-Type-Erkennung, Datums-Fixes

### ðŸŽ¯ **Was wir erreicht haben:**

#### âœ… **Erfolgreich implementiert:**

1. **Combat-Statistiken-Anzeige (Player â†’ Companion â†’ Ability)**
   - 3-Level-Hierarchie: Player â†’ Companion â†’ Ability
   - Table-like Layout mit `Expander` statt `TreeView`
   - Spalten: Name | DPS | Total Damage | Debuff | Max Hit | Crit % | Acc %
   - Expandable Rows fÃ¼r Player und Companions
   - Sortierung nach Total Damage (gemischte Companions + Abilities)

2. **Detailliertes Combat-Log-Parsing**
   - VollstÃ¤ndiges Parsing aller Log-Zeilen-Felder:
     - Owner (Name + Type mit Handle)
     - Source (Name + Type)
     - Target (Name + Type)
     - Ability, Damage Type, Flags, Damage Values
   - HTML-Tag-Entfernung aus Namen (`<br>`, `<span>`, etc.)
   - Negative Damage-Werte werden Ã¼bersprungen (Heilung/Shields)

3. **Companion-Erkennung und -Zuordnung**
   - **Companion-Identifikation:**
     - Source Name ist gefÃ¼llt UND unterscheidet sich vom Owner
     - Source Type beginnt mit `C[` (Pets/Drohnen) oder `S[` (Away Team)
   - **Direct Player Damage:**
     - Source Name ist leer oder `*`
   - **Companion-Typen:**
     - Space Pets: `C[... Space_Fed_Shuttle...]`
     - Ground Pets: `C[... Ground_Universal_Kit...]`
     - Away Team: `S[139588389]` (S-Tag)
   - Alle Companions generisch als "Companion" bezeichnet (keine Unterscheidung Pet/Away Team/Drone)

4. **Combat-Type-Erkennung (Space vs. Ground)**
   - **PrioritÃ¤t 1 (HÃ¶chste): Target Type**
     - `C[... Space_...]` â†’ Space Combat
     - `C[... Ground_...]` â†’ Ground Combat
     - `S[...]` (Away Team) â†’ Ground Combat
   - **PrioritÃ¤t 2: Source Type** (nur wenn Target nicht definiert)
     - Gleiche Logik wie Target
   - **Strikte Combat-Trennung:**
     - Typ-Wechsel (Space â†’ Ground oder umgekehrt) startet sofort neuen Combat
     - UnabhÃ¤ngig von der 30-Sekunden-Regel
   - **Combat-Type-Icons in Liste:**
     - ðŸš€ fÃ¼r Space Combat (blau gefÃ¤rbt)
     - ðŸƒ fÃ¼r Ground Combat (blau gefÃ¤rbt)

5. **DPS-Berechnung mit Companions**
   - **Player-Stats:**
     - `DPS`: Nur direkte Player-Damage
     - `DPS With Companions`: Player + alle Companions
     - `Total Damage`: Nur direkte Player-Damage
     - `Total Damage With Companions`: Player + alle Companions
   - **Companion-Stats:**
     - Individuelle DPS fÃ¼r jeden Companion
     - Total Damage, Crit%, Accuracy% pro Companion
     - Abilities pro Companion mit eigenen Stats

6. **Chronologisches Log-Lesen (REFACTORING)**
   - **VORHER:** Log rÃ¼ckwÃ¤rts lesen â†’ falsche Byte-Positionen
   - **JETZT:** Log vorwÃ¤rts lesen (chronologisch)
   - **Vorteile:**
     - Korrekte Byte-Positionen fÃ¼r Combat-Analyse
     - Einfachere Logik (Zeit-Differenz: `current - last`)
     - `current_combat_lines[-1]` = neueste Zeile âœ…
   - **Combat-Liste:**
     - Alle Combats chronologisch sammeln
     - Letzte N Combats nehmen (`combats[-20:]`)
     - Umkehren fÃ¼r Frontend (neueste zuerst)
     - IDs neu zuweisen (0 = neuester Combat)

7. **Datums-Parsing-Fixes**
   - **Problem:** Combat vom 2025-10-04 wurde unter 2025-10-10 angezeigt
   - **Ursache:** Falsche Byte-Positionen durch reversed Reading
   - **LÃ¶sung:** Chronologisches Lesen + korrekte Byte-Ranges
   - **Problem 2:** Nur Ã¤lteste 20 Combats geladen statt neueste
   - **LÃ¶sung:** Erst alle Combats sammeln, dann letzte N nehmen

8. **Debug-Logging-System**
   - Detailliertes Logging fÃ¼r Combat-Analyse (bei Auswahl)
   - Zeigt fÃ¼r jede Zeile:
     - Original Log-Zeile
     - Geparste Felder (Timestamp, Owner, Source, Target, etc.)
     - Combat-Type-Erkennung
     - Entity-Identifikation (Player vs. Companion)
     - Damage-Processing (Ability, Wert, Crit)
     - Action (wohin wurde Damage addiert)
   - Zusammenfassung am Ende (Players, Companions, Damage Events)
   - **Cleanup:** Verbose Logging entfernt, nur wichtige Infos behalten

### ðŸ”§ **Technische Details:**

#### **Combat-Type-Erkennung-Logik:**
```python
def determine_combat_type_from_line(self, parsed_line: dict) -> str:
    target_type = parsed_line.get('target_type', '')
    source_type = parsed_line.get('source_type', '')
    
    # PRIORITÃ„T 1: TARGET (hÃ¶chste)
    if 'Ground_' in target_type or target_type.startswith('S['):
        return 'Ground'
    elif 'Space_' in target_type:
        return 'Space'
    
    # PRIORITÃ„T 2: SOURCE (Fallback)
    if 'Ground_' in source_type or source_type.startswith('S['):
        return 'Ground'
    elif 'Space_' in source_type:
        return 'Space'
    
    return None  # Kein Default
```

#### **Companion-Identifikation:**
```python
def identify_source_entity(self, parsed_line: dict) -> dict:
    owner_name = parsed_line.get('owner_name', '')
    source_name = parsed_line.get('source_name', '')
    source_type = parsed_line.get('source_type', '')
    
    # Source leer oder "*" â†’ Direct Player Damage
    if not source_name or source_name.strip() in ('', '*'):
        return {'is_companion': False}
    
    # Source gefÃ¼llt + anders als Owner + Type C[]/S[] â†’ Companion
    if source_name != owner_name:
        if source_type.startswith('C[') or source_type.startswith('S['):
            return {
                'is_companion': True,
                'companion_name': clean_name(source_name)  # HTML-Tags entfernen
            }
    
    return {'is_companion': False}
```

#### **Chronologische Combat-Isolation:**
```python
# Log chronologisch lesen (keine Umkehrung)
lines = f.readlines()

# Alle Combats sammeln
for i, line in enumerate(lines):
    # Combat-Detection basierend auf Zeit + Type-Wechsel
    if time_diff > 30 or type_changed:
        combats.append(...)

# Letzte N Combats nehmen (neueste)
if max_combats > 0:
    combats_to_return = combats[-max_combats:]

# Umkehren fÃ¼r Frontend (neueste zuerst)
reversed_combats = list(reversed(combats_to_return))

# IDs neu zuweisen (0 = neuester)
for new_id, combat in enumerate(reversed_combats):
    renumbered_combats.append((new_id, ...))
```

### ðŸš¨ **GelÃ¶ste Probleme:**

#### **Problem 1: Mixed Space/Ground Data in Combat-Statistiken**
- **Symptom:** Ground Combat zeigte Space Abilities und umgekehrt
- **Ursache:** `_analyze_combat_players` filterte nicht nach Combat-Type
- **LÃ¶sung:** 
  - Combat-Type als Parameter an Analyse Ã¼bergeben
  - Zeilen filtern: Nur Zeilen mit passendem Type verarbeiten
  - `determine_combat_type_from_line()` fÃ¼r jede Zeile aufrufen
- **Resultat:** Strikte Trennung, keine gemischten Daten mehr

#### **Problem 2: Companions wurden nicht erkannt**
- **Symptom:** Companion-Damage wurde als Player-Damage gezÃ¤hlt
- **Ursache:** Parsing erkannte Source-Name nicht korrekt
- **LÃ¶sung:**
  - VollstÃ¤ndiges Parsing mit `parse_combat_log_line()`
  - Source-Name vs. Owner-Name Vergleich
  - Source-Type-Check fÃ¼r `C[` und `S[` Tags
- **Resultat:** Companions werden korrekt zugeordnet

#### **Problem 3: Combat-Liste zeigte falsche Daten**
- **Symptom 1:** Combat vom 2025-10-04 unter 2025-10-10
- **Symptom 2:** Nur Ã¤lteste 20 Combats statt neueste 20
- **Ursache:**
  - Reversed Reading â†’ falsche Byte-Positionen
  - `break` nach 20 Combats â†’ Ã¤lteste statt neueste
- **LÃ¶sung:**
  - Chronologisches Lesen
  - Erst alle sammeln, dann letzte N nehmen
  - Byte-Positionen referenzieren Original-Log
- **Resultat:** Korrekte Combats mit korrekten Daten

#### **Problem 4: HTML-Tags in Namen**
- **Symptom:** `Synth-Android N7<br>(A600-Serie)` in UI
- **LÃ¶sung:** `clean_name()` entfernt alle HTML-Tags via Regex
- **Resultat:** Saubere Namen ohne `<br>`, `<span>`, etc.

#### **Problem 5: maxCombats Default = -1 (alle)**
- **Symptom:** 144 Combats geladen statt 20
- **LÃ¶sung:** Default von `-1` auf `20` geÃ¤ndert
- **Resultat:** Nur 20 neueste Combats werden geladen

### ðŸ“ **Wichtige Dateien:**

**Aktualisiert:**
- `Deploy/working_oscr_backend.py` - Komplettes Refactoring
  - Chronologisches Lesen
  - VollstÃ¤ndiges Parsing
  - Companion-Erkennung
  - Combat-Type-Erkennung mit PrioritÃ¤ten
  - HTML-Tag-Entfernung
  - Negative Damage-Filterung
  - Debug-Logging (spÃ¤ter reduziert)
  
- `frontend/MainWindow.xaml` - Combat-Statistiken-UI
  - Table-like Layout mit Expander
  - 3-Level-Hierarchie (Player â†’ Companion â†’ Ability)
  - Combat-Type-Icons (ðŸš€/ðŸƒ)
  
- `frontend/MainWindow.xaml.cs` - Combat-Statistiken-Logik
  - `PopulateCombatStatsTreeView()` mit gemischter Sortierung
  - Player- und Companion-Stats-Anzeige
  - Expander-UI-Generation
  
- `frontend/Models/OSCRModels.cs` - Erweiterte Datenmodelle
  - `CompanionStatistics` Klasse
  - `DpsWithCompanions`, `TotalDamageWithCompanions`
  - `Type` und `Icon` fÃ¼r Combat-Liste

**GelÃ¶scht (Cleanup):**
- `backend/build/`, `backend/dist/` - Build-Artefakte
- `backend/python_backend_OSCR/` - Duplikat-Ordner
- `backend/OSCR/~temp_log_files/` - Temp-Dateien
- `backend/build_backend_improved.py`, `build_integrated.py`, etc. - Alte Build-Skripte
- `backend/*.spec` (auÃŸer `working_oscr.spec`) - Alte Spec-Dateien
- `backend/OSCR/*_api.py` - Alle nicht verwendeten API-Varianten
- `Deploy/python_backend_OSCR/` - Duplikat
- `Deploy/python_backend.py`, `real_oscr_backend.py` - Alte Backends
- `frontend/deploy.bat`, `Deploy.ps1`, `simple_deploy.bat` - Alte Deploy-Skripte
- `REAL_OSCR_INTEGRATION_COMPLETE.md`, `UI_IMPLEMENTATION_PLAN.md` - Alte Docs
- `backend/IMPLEMENTATION_STATUS.md`, `SETUP_INSTRUCTIONS.md` - Alte Docs

### ðŸ“Š **Statistiken:**

**Code-Umfang:**
- `working_oscr_backend.py`: ~1100 Zeilen (von ~200 erweitert)
- Neue Funktionen: `parse_combat_log_line`, `determine_combat_type_from_line`, `identify_source_entity`, `clean_name`
- Komplettes Refactoring: `isolate_combats`, `_analyze_combat_players`

**Features:**
- âœ… VollstÃ¤ndiges Combat-Log-Parsing
- âœ… Companion-Erkennung (Pets, Away Team, Drohnen)
- âœ… Combat-Type-Erkennung (Space/Ground)
- âœ… Strikte Combat-Trennung bei Typ-Wechsel
- âœ… Chronologisches Lesen mit korrekten Byte-Positionen
- âœ… 3-Level-Statistik-Hierarchie
- âœ… DPS-Berechnung mit/ohne Companions
- âœ… HTML-Tag-Entfernung
- âœ… Negative Damage-Filterung

### ðŸ’¡ **Lessons Learned:**

1. **Reversed Reading ist komplex:** Chronologisches Lesen ist einfacher und weniger fehleranfÃ¤llig
2. **Byte-Positionen sind kritisch:** Falsche Positionen fÃ¼hren zu falschen Daten
3. **Combat-Type braucht PrioritÃ¤ten:** Target vor Source fÃ¼r prÃ¤zise Erkennung
4. **Debug-Logging ist unverzichtbar:** Half enorm bei der Fehlersuche
5. **HTML in Spiel-Logs:** Immer auf HTML-Tags prÃ¼fen und entfernen
6. **Negative Damage:** Shield/Heal-Events mÃ¼ssen explizit gefiltert werden
7. **Companion-Zuordnung:** Source-Name UND Source-Type beide prÃ¼fen
8. **Default-Werte:** Immer sinnvolle Defaults setzen (nicht `-1` fÃ¼r "alle")

### ðŸ”„ **Build-Status:**

- âœ… Frontend kompiliert erfolgreich
- âœ… Backend als Standalone .exe gebaut
- âœ… Combat-Statistiken werden korrekt angezeigt
- âœ… Companions werden erkannt und zugeordnet
- âœ… Combat-Type-Erkennung funktioniert
- âœ… Space/Ground-Combats strikt getrennt
- âœ… Chronologisches Lesen implementiert
- âœ… Neueste 20 Combats werden geladen
- âœ… Debug-Logging aufgerÃ¤umt
- âœ… Projekt bereinigt

### ðŸŽ¨ **UI-Status:**

**Implementiert:**
- âœ… Combat-Statistiken-Tabelle (Player â†’ Companion â†’ Ability)
- âœ… Expandable Rows fÃ¼r Player und Companions
- âœ… Sortierung nach Total Damage (gemischt)
- âœ… Combat-Type-Icons in Combat-Liste (ðŸš€/ðŸƒ)
- âœ… Korrekte Spalten-Ausrichtung
- âœ… Visual Hierarchy (EinrÃ¼ckung, Farben, SchriftgrÃ¶ÃŸen)

**Ausstehend:**
- â³ DPS-Graph-Visualisierung
- â³ Sortierbare Spalten (Click-to-Sort)
- â³ Filter-FunktionalitÃ¤t (Damage Out/In, Heal, etc.)
- â³ Export-Funktion
- â³ Live-Parsing-Modus

---
**NÃ¤chste Session:** DPS-Graph implementieren, Spalten-Sortierung, Filter-FunktionalitÃ¤t


