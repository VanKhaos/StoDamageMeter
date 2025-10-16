## Session 4: Combat-Statistiken, Companion-Parsing & Combat-Type-Erkennung

**Datum:** 2025-10-10  
**Dauer:** ~6 Stunden  
**Fokus:** Combat-Details-Anzeige, Companion-Parsing, Combat-Type-Erkennung, Datums-Fixes

### 🎯 **Was wir erreicht haben:**

#### ✅ **Erfolgreich implementiert:**

1. **Combat-Statistiken-Anzeige (Player → Companion → Ability)**
   - 3-Level-Hierarchie: Player → Companion → Ability
   - Table-like Layout mit `Expander` statt `TreeView`
   - Spalten: Name | DPS | Total Damage | Debuff | Max Hit | Crit % | Acc %
   - Expandable Rows für Player und Companions
   - Sortierung nach Total Damage (gemischte Companions + Abilities)

2. **Detailliertes Combat-Log-Parsing**
   - Vollständiges Parsing aller Log-Zeilen-Felder:
     - Owner (Name + Type mit Handle)
     - Source (Name + Type)
     - Target (Name + Type)
     - Ability, Damage Type, Flags, Damage Values
   - HTML-Tag-Entfernung aus Namen (`<br>`, `<span>`, etc.)
   - Negative Damage-Werte werden übersprungen (Heilung/Shields)

3. **Companion-Erkennung und -Zuordnung**
   - **Companion-Identifikation:**
     - Source Name ist gefüllt UND unterscheidet sich vom Owner
     - Source Type beginnt mit `C[` (Pets/Drohnen) oder `S[` (Away Team)
   - **Direct Player Damage:**
     - Source Name ist leer oder `*`
   - **Companion-Typen:**
     - Space Pets: `C[... Space_Fed_Shuttle...]`
     - Ground Pets: `C[... Ground_Universal_Kit...]`
     - Away Team: `S[139588389]` (S-Tag)
   - Alle Companions generisch als "Companion" bezeichnet (keine Unterscheidung Pet/Away Team/Drone)

4. **Combat-Type-Erkennung (Space vs. Ground)**
   - **Priorität 1 (Höchste): Target Type**
     - `C[... Space_...]` → Space Combat
     - `C[... Ground_...]` → Ground Combat
     - `S[...]` (Away Team) → Ground Combat
   - **Priorität 2: Source Type** (nur wenn Target nicht definiert)
     - Gleiche Logik wie Target
   - **Strikte Combat-Trennung:**
     - Typ-Wechsel (Space → Ground oder umgekehrt) startet sofort neuen Combat
     - Unabhängig von der 30-Sekunden-Regel
   - **Combat-Type-Icons in Liste:**
     - 🚀 für Space Combat (blau gefärbt)
     - 🏃 für Ground Combat (blau gefärbt)

5. **DPS-Berechnung mit Companions**
   - **Player-Stats:**
     - `DPS`: Nur direkte Player-Damage
     - `DPS With Companions`: Player + alle Companions
     - `Total Damage`: Nur direkte Player-Damage
     - `Total Damage With Companions`: Player + alle Companions
   - **Companion-Stats:**
     - Individuelle DPS für jeden Companion
     - Total Damage, Crit%, Accuracy% pro Companion
     - Abilities pro Companion mit eigenen Stats

6. **Chronologisches Log-Lesen (REFACTORING)**
   - **VORHER:** Log rückwärts lesen → falsche Byte-Positionen
   - **JETZT:** Log vorwärts lesen (chronologisch)
   - **Vorteile:**
     - Korrekte Byte-Positionen für Combat-Analyse
     - Einfachere Logik (Zeit-Differenz: `current - last`)
     - `current_combat_lines[-1]` = neueste Zeile ✅
   - **Combat-Liste:**
     - Alle Combats chronologisch sammeln
     - Letzte N Combats nehmen (`combats[-20:]`)
     - Umkehren für App (neueste zuerst)
     - IDs neu zuweisen (0 = neuester Combat)

7. **Datums-Parsing-Fixes**
   - **Problem:** Combat vom 2025-10-04 wurde unter 2025-10-10 angezeigt
   - **Ursache:** Falsche Byte-Positionen durch reversed Reading
   - **Lösung:** Chronologisches Lesen + korrekte Byte-Ranges
   - **Problem 2:** Nur älteste 20 Combats geladen statt neueste
   - **Lösung:** Erst alle Combats sammeln, dann letzte N nehmen

8. **Debug-Logging-System**
   - Detailliertes Logging für Combat-Analyse (bei Auswahl)
   - Zeigt für jede Zeile:
     - Original Log-Zeile
     - Geparste Felder (Timestamp, Owner, Source, Target, etc.)
     - Combat-Type-Erkennung
     - Entity-Identifikation (Player vs. Companion)
     - Damage-Processing (Ability, Wert, Crit)
     - Action (wohin wurde Damage addiert)
   - Zusammenfassung am Ende (Players, Companions, Damage Events)
   - **Cleanup:** Verbose Logging entfernt, nur wichtige Infos behalten

### 🔧 **Technische Details:**

#### **Combat-Type-Erkennung-Logik:**
```python
def determine_combat_type_from_line(self, parsed_line: dict) -> str:
    target_type = parsed_line.get('target_type', '')
    source_type = parsed_line.get('source_type', '')
    
    # PRIORITÄT 1: TARGET (höchste)
    if 'Ground_' in target_type or target_type.startswith('S['):
        return 'Ground'
    elif 'Space_' in target_type:
        return 'Space'
    
    # PRIORITÄT 2: SOURCE (Fallback)
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
    
    # Source leer oder "*" → Direct Player Damage
    if not source_name or source_name.strip() in ('', '*'):
        return {'is_companion': False}
    
    # Source gefüllt + anders als Owner + Type C[]/S[] → Companion
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

# Umkehren für App (neueste zuerst)
reversed_combats = list(reversed(combats_to_return))

# IDs neu zuweisen (0 = neuester)
for new_id, combat in enumerate(reversed_combats):
    renumbered_combats.append((new_id, ...))
```

### 🚨 **Gelöste Probleme:**

#### **Problem 1: Mixed Space/Ground Data in Combat-Statistiken**
- **Symptom:** Ground Combat zeigte Space Abilities und umgekehrt
- **Ursache:** `_analyze_combat_players` filterte nicht nach Combat-Type
- **Lösung:** 
  - Combat-Type als Parameter an Analyse übergeben
  - Zeilen filtern: Nur Zeilen mit passendem Type verarbeiten
  - `determine_combat_type_from_line()` für jede Zeile aufrufen
- **Resultat:** Strikte Trennung, keine gemischten Daten mehr

#### **Problem 2: Companions wurden nicht erkannt**
- **Symptom:** Companion-Damage wurde als Player-Damage gezählt
- **Ursache:** Parsing erkannte Source-Name nicht korrekt
- **Lösung:**
  - Vollständiges Parsing mit `parse_combat_log_line()`
  - Source-Name vs. Owner-Name Vergleich
  - Source-Type-Check für `C[` und `S[` Tags
- **Resultat:** Companions werden korrekt zugeordnet

#### **Problem 3: Combat-Liste zeigte falsche Daten**
- **Symptom 1:** Combat vom 2025-10-04 unter 2025-10-10
- **Symptom 2:** Nur älteste 20 Combats statt neueste 20
- **Ursache:**
  - Reversed Reading → falsche Byte-Positionen
  - `break` nach 20 Combats → älteste statt neueste
- **Lösung:**
  - Chronologisches Lesen
  - Erst alle sammeln, dann letzte N nehmen
  - Byte-Positionen referenzieren Original-Log
- **Resultat:** Korrekte Combats mit korrekten Daten

#### **Problem 4: HTML-Tags in Namen**
- **Symptom:** `Synth-Android N7<br>(A600-Serie)` in UI
- **Lösung:** `clean_name()` entfernt alle HTML-Tags via Regex
- **Resultat:** Saubere Namen ohne `<br>`, `<span>`, etc.

#### **Problem 5: maxCombats Default = -1 (alle)**
- **Symptom:** 144 Combats geladen statt 20
- **Lösung:** Default von `-1` auf `20` geändert
- **Resultat:** Nur 20 neueste Combats werden geladen

### 📁 **Wichtige Dateien:**

**Aktualisiert:**
- `Deploy/working_oscr_backend.py` - Komplettes Refactoring
  - Chronologisches Lesen
  - Vollständiges Parsing
  - Companion-Erkennung
  - Combat-Type-Erkennung mit Prioritäten
  - HTML-Tag-Entfernung
  - Negative Damage-Filterung
  - Debug-Logging (später reduziert)
  
- `app/MainWindow.xaml` - Combat-Statistiken-UI
  - Table-like Layout mit Expander
  - 3-Level-Hierarchie (Player → Companion → Ability)
  - Combat-Type-Icons (🚀/🏃)
  
- `app/MainWindow.xaml.cs` - Combat-Statistiken-Logik
  - `PopulateCombatStatsTreeView()` mit gemischter Sortierung
  - Player- und Companion-Stats-Anzeige
  - Expander-UI-Generation
  
- `app/Models/OSCRModels.cs` - Erweiterte Datenmodelle
  - `CompanionStatistics` Klasse
  - `DpsWithCompanions`, `TotalDamageWithCompanions`
  - `Type` und `Icon` für Combat-Liste

**Gelöscht (Cleanup):**
- `backend/build/`, `backend/dist/` - Build-Artefakte
- `backend/python_backend_OSCR/` - Duplikat-Ordner
- `backend/OSCR/~temp_log_files/` - Temp-Dateien
- `backend/build_backend_improved.py`, `build_integrated.py`, etc. - Alte Build-Skripte
- `backend/*.spec` (außer `working_oscr.spec`) - Alte Spec-Dateien
- `backend/OSCR/*_api.py` - Alle nicht verwendeten API-Varianten
- `Deploy/python_backend_OSCR/` - Duplikat
- `Deploy/python_backend.py`, `real_oscr_backend.py` - Alte Backends
- `app/deploy.bat`, `Deploy.ps1`, `simple_deploy.bat` - Alte Deploy-Skripte
- `REAL_OSCR_INTEGRATION_COMPLETE.md`, `UI_IMPLEMENTATION_PLAN.md` - Alte Docs
- `backend/IMPLEMENTATION_STATUS.md`, `SETUP_INSTRUCTIONS.md` - Alte Docs

### 📊 **Statistiken:**

**Code-Umfang:**
- `working_oscr_backend.py`: ~1100 Zeilen (von ~200 erweitert)
- Neue Funktionen: `parse_combat_log_line`, `determine_combat_type_from_line`, `identify_source_entity`, `clean_name`
- Komplettes Refactoring: `isolate_combats`, `_analyze_combat_players`

**Features:**
- ✅ Vollständiges Combat-Log-Parsing
- ✅ Companion-Erkennung (Pets, Away Team, Drohnen)
- ✅ Combat-Type-Erkennung (Space/Ground)
- ✅ Strikte Combat-Trennung bei Typ-Wechsel
- ✅ Chronologisches Lesen mit korrekten Byte-Positionen
- ✅ 3-Level-Statistik-Hierarchie
- ✅ DPS-Berechnung mit/ohne Companions
- ✅ HTML-Tag-Entfernung
- ✅ Negative Damage-Filterung

### 💡 **Lessons Learned:**

1. **Reversed Reading ist komplex:** Chronologisches Lesen ist einfacher und weniger fehleranfällig
2. **Byte-Positionen sind kritisch:** Falsche Positionen führen zu falschen Daten
3. **Combat-Type braucht Prioritäten:** Target vor Source für präzise Erkennung
4. **Debug-Logging ist unverzichtbar:** Half enorm bei der Fehlersuche
5. **HTML in Spiel-Logs:** Immer auf HTML-Tags prüfen und entfernen
6. **Negative Damage:** Shield/Heal-Events müssen explizit gefiltert werden
7. **Companion-Zuordnung:** Source-Name UND Source-Type beide prüfen
8. **Default-Werte:** Immer sinnvolle Defaults setzen (nicht `-1` für "alle")

### 🔄 **Build-Status:**

- ✅ App kompiliert erfolgreich
- ✅ Backend als Standalone .exe gebaut
- ✅ Combat-Statistiken werden korrekt angezeigt
- ✅ Companions werden erkannt und zugeordnet
- ✅ Combat-Type-Erkennung funktioniert
- ✅ Space/Ground-Combats strikt getrennt
- ✅ Chronologisches Lesen implementiert
- ✅ Neueste 20 Combats werden geladen
- ✅ Debug-Logging aufgeräumt
- ✅ Projekt bereinigt

### 🎨 **UI-Status:**

**Implementiert:**
- ✅ Combat-Statistiken-Tabelle (Player → Companion → Ability)
- ✅ Expandable Rows für Player und Companions
- ✅ Sortierung nach Total Damage (gemischt)
- ✅ Combat-Type-Icons in Combat-Liste (🚀/🏃)
- ✅ Korrekte Spalten-Ausrichtung
- ✅ Visual Hierarchy (Einrückung, Farben, Schriftgrößen)

**Ausstehend:**
- ⏳ DPS-Graph-Visualisierung
- ⏳ Sortierbare Spalten (Click-to-Sort)
- ⏳ Filter-Funktionalität (Damage Out/In, Heal, etc.)
- ⏳ Export-Funktion
- ⏳ Live-Parsing-Modus

---
**Nächste Session:** DPS-Graph implementieren, Spalten-Sortierung, Filter-Funktionalität


