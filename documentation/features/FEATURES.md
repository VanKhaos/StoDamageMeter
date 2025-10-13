# STO Damage Meter - Feature Roadmap & Proposals

Dieses Dokument sammelt geplante Features, Verbesserungen und bekannte Probleme die in zukünftigen Versionen implementiert werden könnten.

**Legende:**
- 🟢 **Implemented** - Feature ist fertig
- 🟡 **In Progress** - Wird gerade entwickelt
- 🔵 **Planned** - Geplant für nächste Version
- ⚪ **Proposal** - Vorschlag, noch nicht geplant
- 🔴 **On Hold** - Zurückgestellt

---

## Inhaltsverzeichnis

1. [Combat Continuity bei Player-Death](#1-combat-continuity-bei-player-death)
2. [Weitere Features](#weitere-features)

---

## 1. Combat Continuity bei Player-Death

**Status:** ⚪ Proposal  
**Priorität:** Medium  
**Kategorie:** Combat Detection / Parser Logic  
**Betroffene Dateien:** `backend/working_oscr_backend.py`

### Problem-Beschreibung

Wenn ein Spieler während eines Combats stirbt und respawnt, wird der Combat fälschlicherweise in zwei separate Combats aufgesplittet, obwohl:
- ✅ Keine 45 Sekunden Pause vergangen sind
- ✅ Der Combat-Type (Space/Ground) sich nicht geändert hat
- ✅ Der Combat durchgehend weiterläuft (andere Spieler/Team)

### Warum passiert das?

Die Combat-Detection verwendet `last_timestamp` um zu prüfen ob ein neuer Combat gestartet werden soll:

```python
# Zeitdifferenz zwischen letztem und aktuellem Player-Damage-Event
time_diff = (current_time - last_timestamp).total_seconds()

if time_diff > 45:  # seconds_between_combats
    start_new_combat = True
```

**Das Problem:**
1. Spieler macht Damage → `last_timestamp = 20:44:20` ✅
2. **Spieler stirbt** (NPC macht Damage) → Log-Zeile wird **ignoriert** (Owner = NPC, nicht Player) ❌
3. Spieler respawnt + fliegt zurück (dauert ~50 Sekunden)
4. Spieler macht wieder Damage → `current_time = 20:45:15` ✅
5. **Zeitdifferenz:** `20:45:15 - 20:44:20 = 55 Sekunden > 45 Sekunden` ❌
6. **➡️ Neuer Combat wird gestartet!** ❌

**Root Cause:**  
Death-Events (NPC → Player) werden komplett ignoriert und aktualisieren `last_timestamp` nicht, obwohl sie Teil des Combats sind.

### Gewünschtes Verhalten

Der Combat sollte als **ein zusammenhängender Combat** getrackt werden, auch wenn einzelne Spieler sterben und respawnen.

**Beispiel-Szenario:**

```
Timeline:
20:44:00 - Combat startet (Space Combat)
20:44:20 - Player "Visa" macht Damage         [Combat läuft]
20:44:28 - Borg tötet "Visa" (Kill|DoT)       [Combat läuft weiter!]
20:44:35 - Andere Spieler machen Damage       [Combat läuft]
20:44:50 - "Visa" respawnt
20:45:15 - "Visa" macht wieder Damage          [GLEICHER Combat!]
20:46:00 - Combat endet (keine Aktivität > 45s)

Ergebnis: 1 Combat mit 2:00 Minuten Dauer ✅
```

**Aktuell:** Wird in 2 Combats gesplittet ❌

### Lösungsvorschläge

#### Option 1: Death-Events als Combat-Activity tracken ⭐ Empfohlen

**Konzept:**  
Death-Events (NPC → Player) zählen als Combat-Activity und aktualisieren `last_timestamp`, auch wenn sie nicht für DPS gezählt werden.

**Implementierung:**

```python
# In isolate_combats() oder live_parse_log()

# Parse die Zeile
parsed = self.parse_combat_log_line(line)

# Prüfe ob es ein Death-Event ist (Player als Target)
is_player_death = False
if parsed and 'P[' in parsed.get('target_type', ''):
    flags = parsed.get('flags', '')
    if 'Kill' in flags or 'Defeated' in flags:
        is_player_death = True

# Aktualisiere last_timestamp auch für Death-Events
if is_combat_line or is_player_death:
    last_timestamp = current_time  # Combat bleibt aktiv!
```

**Vorteile:**
- ✅ Minimaler Code-Change
- ✅ Logisch korrekt (Tod ist Teil des Combats)
- ✅ Löst das Problem direkt an der Ursache

**Nachteile:**
- ⚠️ Muss in mehreren Funktionen implementiert werden

#### Option 2: Längeres Combat-Timeout

**Konzept:**  
Combat-Timeout von 45s auf 90-120s erhöhen.

**Vorteile:**
- ✅ Sehr einfach (nur Config-Änderung)

**Nachteile:**
- ❌ Löst nicht die Grundursache
- ❌ Kann zu falschen Combat-Merges führen

#### Option 3: Multi-Player Combat-Continuity

**Konzept:**  
Combat endet erst wenn **alle** Spieler > 45s keine Aktivität haben.

**Vorteile:**
- ✅ Sehr robust für Team-Combats

**Nachteile:**
- ❌ Komplexer zu implementieren
- ❌ Mehr State-Tracking erforderlich

### Implementierungs-Details

**Betroffene Dateien:**
- `backend/working_oscr_backend.py`:
  - `isolate_combats()` - Zeile ~315-430
  - `live_parse_log()` - Zeile ~1283-1400

**Geschätzter Aufwand:**
- 🕐 2-3 Stunden Code
- 🧪 1-2 Stunden Testing

### Test-Szenarien

**Szenario 1: Einzelner Player stirbt**
```
Input:
- 20:44:20 - Player macht Damage
- 20:44:28 - Player stirbt
- 20:45:15 - Player macht wieder Damage (55s später)

Erwartet: 1 Combat
```

**Szenario 2: Team-Combat mit mehreren Toden**
```
Input:
- 20:44:00 - Player A macht Damage
- 20:44:20 - Player B macht Damage
- 20:44:28 - Player A stirbt
- 20:44:35 - Player B macht Damage
- 20:45:15 - Player A macht wieder Damage

Erwartet: 1 Combat für beide Spieler
```

**Szenario 3: Echter Combat-Ende**
```
Input:
- 20:44:00 - Player macht Damage
- 20:44:28 - Player stirbt
- 20:45:30 - Keine weiteren Events (> 45s nach letztem Event)
- 20:46:00 - Neuer Combat startet

Erwartet: 2 separate Combats
```

---

## Weitere Features

### Placeholder für zukünftige Features

Weitere Feature-Vorschläge werden hier dokumentiert wenn sie identifiziert werden.

Mögliche Bereiche:
- **UI/UX Verbesserungen**
- **Performance Optimierungen**
- **Export-Funktionen** (CSV, JSON, etc.)
- **Erweiterte Statistiken** (Heal-Tracking, Shield-Analysis, etc.)
- **Multiplayer-Features** (Team-Vergleiche, Rankings, etc.)

---

**Erstellt:** 2025-10-11  
**Letzte Aktualisierung:** 2025-10-11  
**Maintainer:** STO Damage Meter Team

