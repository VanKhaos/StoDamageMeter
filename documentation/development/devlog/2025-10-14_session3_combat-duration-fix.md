# DevLog 2025-10-14 Session 3: Combat Duration Fix & Debug Cleanup

## Problem
Die Combat Duration wurde in der Combat List immer als "0:00" angezeigt, obwohl die Kämpfe tatsächlich länger dauerten (z.B. 0:57 oder 1:40).

## Root Cause Analysis
Das Problem lag in der `get_available_combats` Funktion im Backend:
- Die Funktion verwendete eine andere Parsing-Logik als die JSON-Debug-Erstellung
- Byte-Offset vs. Line-Index Verwirrung führte zu fehlerhaften Zeilen-Zugriffen
- Die `end_time` konnte nicht korrekt geparst werden, was zu `duration = 0` führte

## Lösung

### 1. Parsing-Logik vereinheitlicht
- `get_available_combats` verwendet jetzt die gleiche Logik wie die JSON-Debug-Erstellung
- Korrekte Behandlung von `c_byte_start` und `c_byte_end` als Line-Indices
- Verwendung von `utf-8-sig` Encoding für korrekte Zeilen-Lesung

### 2. Debug-Logging entfernt
Nach erfolgreicher Fix wurden alle Debug-Logs entfernt:
- Alle `logger.debug()` Aufrufe aus `get_available_combats`
- Alle `logger.info()` Aufrufe für Combat-Loading
- Komplette JSON-Debug-Datei-Erstellung (80+ Zeilen Code)
- Build-Script Logs-Ordner-Löschung entfernt

### 3. Code-Cleanup
- `json` Import wieder hinzugefügt (wird in `main` Funktion benötigt)
- Build-Script Schritt-Nummern von 4 auf 3 reduziert
- Alle unnötigen Warning- und Error-Logs entfernt

## Technische Details

### Vorher (fehlerhaft):
```python
# Byte-basierte Zeilen-Lesung
f.seek(c_byte_start)
chunk = f.read(min(1000, c_byte_end - c_byte_start))
# ... komplexe Byte-Offset-Berechnung
```

### Nachher (korrekt):
```python
# Line-basierte Zeilen-Lesung (wie JSON-Debug)
with open(log_path, 'r', encoding='utf-8-sig', errors='replace') as f:
    all_lines = f.readlines()

start_line_idx = c_byte_start
end_line_idx = c_byte_end
start_line = all_lines[start_line_idx].strip()
end_line = all_lines[end_line_idx - 1].strip()
```

## Ergebnis
- ✅ Combat Duration wird korrekt berechnet und angezeigt
- ✅ Keine unnötigen Debug-Logs mehr in der Konsole
- ✅ Sauberer, performanter Code
- ✅ JSON-Debug-Dateien werden nicht mehr erstellt

## Dateien geändert
- `backend/working_oscr_backend.py` - Parsing-Logik vereinheitlicht, Debug-Logs entfernt
- `scripts/build_debug.ps1` - Logs-Ordner-Löschung entfernt, Schritt-Nummern angepasst

## Testing
- Combat Duration wird jetzt korrekt als "1:40", "0:57", etc. angezeigt
- Backend läuft ohne Debug-Spam in der Konsole
- Build-Prozess ist effizienter
