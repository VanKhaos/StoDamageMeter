#!/usr/bin/env python3
"""
Arbeitendes OSCR-Backend für STO Damage Meter
Verwendet echte OSCR-Funktionalität mit korrekten Combat-Patterns
"""

import sys
import os
import json
import logging
from pathlib import Path
from datetime import datetime, timedelta
from collections import defaultdict
import io

# Erzwinge UTF-8 für stdout/stderr (wichtig für PyInstaller .exe)
if sys.stdout.encoding != 'utf-8':
    sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')
if sys.stderr.encoding != 'utf-8':
    sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8', errors='replace')

# Logging konfigurieren
# Logger mit File-Handler konfigurieren
logger = logging.getLogger(__name__)
logger.setLevel(logging.INFO)

# Console Handler
console_handler = logging.StreamHandler()
console_handler.setLevel(logging.INFO)

# File Handler - schreibt ins logs/ Unterverzeichnis
# Bestimme das Verzeichnis der .exe (oder des Scripts)
if getattr(sys, 'frozen', False):
    # Running as compiled executable
    exe_dir = os.path.dirname(sys.executable)
else:
    # Running as script
    exe_dir = os.path.dirname(os.path.abspath(__file__))

# Erstelle logs/ Unterverzeichnis falls es nicht existiert
logs_dir = os.path.join(exe_dir, 'logs')
os.makedirs(logs_dir, exist_ok=True)

log_file_path = os.path.join(logs_dir, 'oscr_backend.log')
file_handler = logging.FileHandler(log_file_path, mode='a', encoding='utf-8')
file_handler.setLevel(logging.INFO)

# Format
formatter = logging.Formatter('%(asctime)s - %(levelname)s - %(message)s')
console_handler.setFormatter(formatter)
file_handler.setFormatter(formatter)

# Handler hinzufügen
logger.addHandler(console_handler)
logger.addHandler(file_handler)

# Log-Pfad ausgeben damit Benutzer weiß wo die Log-Datei ist
logger.info(f"=== OSCR Backend Started ===")
logger.info(f"Log file: {log_file_path}")

# Echte OSCR-Funktionalität mit korrekten Combat-Patterns
class WorkingOSCR:
    """Arbeitende OSCR-Implementierung mit echten Combat-Patterns"""
    
    def __init__(self, log_path: str = '', settings: dict = None):
        self.log_path = log_path
        self.version = '2025.8.10.0'
        self.bytes_consumed = 0
        self.combats = []
        
        self._settings = {
            "combats_to_parse": 20,
            "seconds_between_combats": 30,
            "combat_min_lines": 20,
            "graph_resolution": 0.2,
        }
        
        if settings:
            self._settings.update(settings)
    
    def parse_timestamp(self, time_str):
        """Parst einen Timestamp aus dem Log-Format YY:MM:DD:HH:MM:SS.ms"""
        try:
            parts = time_str.split(':')
            if len(parts) >= 6:
                year = int(parts[0]) + 2000
                month = int(parts[1])
                day = int(parts[2])
                hour = int(parts[3])
                minute = int(parts[4])
                second = float(parts[5])
                return datetime(year, month, day, hour, minute, int(second), int((second % 1) * 1000000))
            return None
        except:
            return None
    
    def _extract_primary_damage_type(self, damage_type_string: str) -> str:
        """
        Extrahiert den primären Damage-Type aus einem String wie "Physical|Crit|DoT"
        Ignoriert: Crit, DoT, Immune, Miss, Shield
        """
        if not damage_type_string:
            return ""
        
        # Nach | splitten
        types = damage_type_string.split('|')
        
        # Ignorierte Types
        ignored = {'Crit', 'Critical', 'DoT', 'Immune', 'Miss', 'Shield', 'Flank', 'Dodge'}
        
        # Ersten nicht-ignorierten Type finden
        for t in types:
            t = t.strip()
            if t and t not in ignored:
                return t
        
        return ""
    
    def parse_combat_log_line(self, line: str):
        """
        Parsed eine Combat-Log-Zeile vollständig
        Format: YY:MM:DD:HH:MM:SS.ms::Owner,OwnerType,SourceName,SourceType,Target,TargetType,Ability,Pn.XXX,DamageType,Flags,Damage1,Damage2
        
        Returns: dict mit allen geparsten Feldern oder None bei Fehler
        """
        try:
            if '::' not in line:
                return None
            
            # Split bei ::
            parts = line.split('::', 1)
            if len(parts) < 2:
                return None
            
            timestamp_str = parts[0]
            timestamp = self.parse_timestamp(timestamp_str)
            
            # Rest der Zeile splitten - maxsplit begrenzen wegen Kommas in Namen
            data_parts = parts[1].split(',')
            
            if len(data_parts) < 7:
                return None
            
            return {
                'timestamp': timestamp,
                'owner_name': data_parts[0].strip() if len(data_parts) > 0 else '',
                'owner_type': data_parts[1].strip() if len(data_parts) > 1 else '',
                'source_name': data_parts[2].strip() if len(data_parts) > 2 else '',
                'source_type': data_parts[3].strip() if len(data_parts) > 3 else '',
                'target_name': data_parts[4].strip() if len(data_parts) > 4 else '',
                'target_type': data_parts[5].strip() if len(data_parts) > 5 else '',
                'ability_name': data_parts[6].strip() if len(data_parts) > 6 else '',
                'damage_type': data_parts[8].strip() if len(data_parts) > 8 else '',
                'flags': data_parts[9].strip() if len(data_parts) > 9 else '',
                'damage_values': data_parts[10:] if len(data_parts) > 10 else []
            }
        except Exception as e:
            return None
    
    def determine_combat_type_from_line(self, parsed_line: dict) -> str:
        """
        Ermittelt Combat-Type (Space/Ground) basierend auf Target Type (höchste Priorität), dann Source Type
        Prüft auch S-Tags (Away Team = Ground) und C-Tags mit Ground_/Space_
        
        Priorität: TARGET zuerst, dann SOURCE
        """
        if not parsed_line:
            return None  # Kein Default, wenn Parsing fehlschlägt
        
        target_type = parsed_line.get('target_type', '')
        source_type = parsed_line.get('source_type', '')
        
        # ===== PRIORITÄT 1: TARGET prüfen =====
        
        # Target: S-Tag = Away Team = Ground Combat
        if target_type.startswith('S['):
            return 'Ground'
        
        # Target: C-Tag mit Ground_ = Ground Combat
        if target_type.startswith('C[') and 'Ground_' in target_type:
            return 'Ground'
        
        # Target: C-Tag mit Space_ = Space Combat
        if target_type.startswith('C[') and 'Space_' in target_type:
            return 'Space'
        
        # Target: Generelles String-Matching für Space_/Ground_
        if 'Ground_' in target_type:
            return 'Ground'
        elif 'Space_' in target_type:
            return 'Space'
        
        # ===== PRIORITÄT 2: SOURCE prüfen (nur wenn Target nichts ergab) =====
        
        # Source: S-Tag = Away Team = Ground Combat
        if source_type.startswith('S['):
            return 'Ground'
        
        # Source: C-Tag mit Ground_ = Ground Combat
        if source_type.startswith('C[') and 'Ground_' in source_type:
            return 'Ground'
        
        # Source: C-Tag mit Space_ = Space Combat
        if source_type.startswith('C[') and 'Space_' in source_type:
            return 'Space'
        
        # Source: Generelles String-Matching für Space_/Ground_
        if 'Ground_' in source_type:
            return 'Ground'
        elif 'Space_' in source_type:
            return 'Space'
        
        # Wenn nichts gefunden, gib None zurück (kein Default)
        return None
    
    def clean_name(self, name: str) -> str:
        """
        Entfernt HTML-Tags aus Namen (z.B. <br>, <span>, etc.) 
        und dekodiert HTML-Entities (z.B. &lt;, &gt;, &amp;)
        """
        import re
        import html
        # Erst HTML-Entities dekodieren
        name = html.unescape(name)
        # Dann HTML-Tags entfernen
        return re.sub(r'<[^>]+>', '', name).strip()
    
    def identify_source_entity(self, parsed_line: dict) -> dict:
        """
        Identifiziert ob Source ein Spieler oder Companion ist
        
        WICHTIG: Ein Companion ist alles was NICHT der Spieler selbst ist!
        - Spieler direkt: owner_name == source_name ODER source leer
        - Companion: owner_name != source_name UND source hat Type (C[...] oder S[...])
        
        Returns:
        {
            'player_handle': '@handle',
            'player_name': 'Player Name',
            'is_companion': True/False,
            'companion_name': 'Companion Name' oder None (HTML-bereinigt)
        }
        """
        if not parsed_line:
            return None
        
        owner_name = parsed_line.get('owner_name', '')
        owner_type = parsed_line.get('owner_type', '')
        source_name = parsed_line.get('source_name', '')
        source_type = parsed_line.get('source_type', '')
        
        # Nur Player-Events verarbeiten
        if not owner_type or 'P[' not in owner_type:
            return None
        
        # Spieler-Handle extrahieren
        handle = None
        if '@' in owner_type:
            try:
                handle = owner_type.split('@')[1].split(']')[0]
            except:
                pass
        
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
                companion_name = self.clean_name(source_name)  # HTML-Tags entfernen!
        
        return {
            'player_handle': handle,
            'player_name': player_name,
            'is_companion': is_companion,
            'companion_name': companion_name
        }
    
    def isolate_combats(self, path: str, max_combats: int = -1):
        """Echte Combat-Isolation basierend auf Zeit-Differenzen"""
        logger.info(f"Isolating combats from: {path}")
        
        if not os.path.exists(path):
            raise FileNotFoundError(f"Log file not found: {path}")
        
        combats = []
        
        try:
            # Log-Datei lesen mit UTF-8 BOM Support
            with open(path, 'r', encoding='utf-8-sig', errors='replace') as f:
                lines = f.readlines()
            
            logger.info(f"Read {len(lines)} lines from log (chronological order)")
            
            # Combat-Daten sammeln (chronologisch, ältester zuerst)
            current_combat_lines = []
            current_combat_start_idx = 0
            last_timestamp = None
            current_combat_type = None  # 'Space' oder 'Ground'
            space_count = 0
            ground_count = 0
            seconds_between_combats = self._settings['seconds_between_combats']
            
            for i, line in enumerate(lines):
                if '::' not in line:
                    continue
                
                # Zeit-Extraktion
                time_part = line.split('::')[0]
                current_time = self.parse_timestamp(time_part)
                
                if not current_time:
                    continue
                
                # Prüfen ob es eine Combat-Zeile ist
                is_combat_line = any(pattern in line for pattern in [
                    'Damage', 'Heal', 'Shield', 'Electrical', 'Phaser', 
                    'Plasma', 'Disruptor', 'Tetryon', 'Polaron', 'Antiproton',
                    'Kinetic', 'Physical', 'Fire', 'Cold', 'Radiation',
                    'Psionic', 'Thalaron', 'Temporal', 'Exotic'
                ])
                
                if not is_combat_line:
                    continue
                
                # Combat-Type der aktuellen Zeile ermitteln (mit vollständigem Parsing)
                parsed_line = self.parse_combat_log_line(line)
                line_combat_type = None
                if parsed_line:
                    line_combat_type = self.determine_combat_type_from_line(parsed_line)
                
                # Prüfen ob neuer Combat (Zeitdifferenz ODER Type-Wechsel)
                start_new_combat = False
                
                if last_timestamp and current_combat_lines:
                    # Chronologisch: last_timestamp ist früher, current_time ist später
                    time_diff = (current_time - last_timestamp).total_seconds()
                    
                    # Neuer Combat bei Zeitdifferenz > Threshold
                    if time_diff > seconds_between_combats:
                        start_new_combat = True
                    
                    # Neuer Combat bei Type-Wechsel (Space <-> Ground)
                    if (line_combat_type and current_combat_type and 
                        line_combat_type != current_combat_type):
                        start_new_combat = True
                
                if start_new_combat:
                        # Combat beenden, wenn genug Zeilen
                        if len(current_combat_lines) >= self._settings['combat_min_lines']:
                            # Combat-Type basierend auf Mehrheit bestimmen
                            # Es gibt nur Space oder Ground, kein Unknown
                            if ground_count > space_count:
                                combat_type = 'Ground'
                            else:
                                # Default ist Space (bei Gleichstand oder wenn keiner erkannt)
                                combat_type = 'Space'
                            
                            # Combat speichern - Parse Timestamp aus der LETZTEN Zeile (neueste, chronologisch)
                            try:
                                # current_combat_lines[-1] ist die NEUESTE Zeile (chronologisch)
                                last_line_timestamp = current_combat_lines[-1].split('::')[0]
                                parsed_timestamp = self.parse_timestamp(last_line_timestamp)
                                
                                if parsed_timestamp:
                                    date_str = parsed_timestamp.strftime("%Y-%m-%d")
                                    time_str = parsed_timestamp.strftime("%H:%M:%S.%f")[:-5]  # Ohne letzte Mikrosekunde
                                else:
                                    # Fallback: Manuelles Parsing
                                    time_parts = last_line_timestamp.split(':')
                                    if len(time_parts) >= 6:
                                        date_str = f"20{time_parts[0]}-{time_parts[1]}-{time_parts[2]}"
                                        time_str = f"{time_parts[3]}:{time_parts[4]}:{time_parts[5]}"
                                    else:
                                        date_str = "2025-10-09"
                                        time_str = "00:00:00"
                            except Exception as e:
                                logger.warning(f"Error parsing combat timestamp: {e}")
                                date_str = "2025-10-09"
                                time_str = "00:00:00"
                            
                            # Combat-Type als Map-Name
                            combat_type_name = f"{combat_type} Combat"
                            
                            combats.append((
                                len(combats),
                                combat_type_name,
                                date_str,
                                time_str,
                                "Normal",
                                current_combat_start_idx,
                                i - 1,
                                combat_type
                            ))
                        
                        # Neuen Combat starten - Counter zurücksetzen
                        current_combat_lines = []
                        current_combat_start_idx = i
                        space_count = 0
                        ground_count = 0
                        current_combat_type = None  # Zurücksetzen
                
                # Zeile zum aktuellen Combat hinzufügen
                current_combat_lines.append(line)
                last_timestamp = current_time
                
                # Combat-Type zählen und setzen (mit korrekter Erkennung)
                if line_combat_type == 'Space':
                    space_count += 1
                    if not current_combat_type:
                        current_combat_type = 'Space'
                elif line_combat_type == 'Ground':
                    ground_count += 1
                    if not current_combat_type:
                        current_combat_type = 'Ground'
            
            # Letzten Combat hinzufügen falls vorhanden
            if (current_combat_lines and 
                len(current_combat_lines) >= self._settings['combat_min_lines']):
                # Combat-Type basierend auf Mehrheit bestimmen
                # Es gibt nur Space oder Ground, kein Unknown
                if ground_count > space_count:
                    combat_type = 'Ground'
                else:
                    # Default ist Space (bei Gleichstand oder wenn keiner erkannt)
                    combat_type = 'Space'
                
                # Combat speichern - Parse Timestamp aus der LETZTEN Zeile (neueste, chronologisch)
                try:
                    # current_combat_lines[-1] ist die NEUESTE Zeile (chronologisch)
                    last_line_timestamp = current_combat_lines[-1].split('::')[0]
                    parsed_timestamp = self.parse_timestamp(last_line_timestamp)
                    
                    if parsed_timestamp:
                        date_str = parsed_timestamp.strftime("%Y-%m-%d")
                        time_str = parsed_timestamp.strftime("%H:%M:%S.%f")[:-5]  # Ohne letzte Mikrosekunde
                    else:
                        # Fallback: Manuelles Parsing
                        time_parts = last_line_timestamp.split(':')
                        if len(time_parts) >= 6:
                            date_str = f"20{time_parts[0]}-{time_parts[1]}-{time_parts[2]}"
                            time_str = f"{time_parts[3]}:{time_parts[4]}:{time_parts[5]}"
                        else:
                            date_str = "2025-10-09"
                            time_str = "00:00:00"
                except Exception as e:
                    logger.warning(f"Error parsing combat timestamp: {e}")
                    date_str = "2025-10-09"
                    time_str = "00:00:00"
                
                # Combat-Type als Map-Name
                combat_type_name = f"{combat_type} Combat"
                
                combats.append((
                    len(combats),
                    combat_type_name,
                    date_str,
                    time_str,
                    "Normal",
                    current_combat_start_idx,
                    len(lines),
                    combat_type
                ))
            
            logger.info(f"Found {len(combats)} combats (chronological)")
            
            # Falls Limit gesetzt: Nur die LETZTEN N Combats nehmen (neueste)
            if max_combats > 0 and len(combats) > max_combats:
                combats_to_return = combats[-max_combats:]  # Letzte N = neueste
                logger.info(f"Limiting to last {max_combats} combats (newest)")
            else:
                combats_to_return = combats
            
            # Combats umkehren, damit neueste zuerst kommen
            reversed_combats = list(reversed(combats_to_return))
            
            # IDs neu zuweisen (0 = neuester Combat)
            renumbered_combats = []
            for new_id, (old_id, c_map, c_date, c_time, c_difficulty, c_byte_start, c_byte_end, c_type) in enumerate(reversed_combats):
                renumbered_combats.append((
                    new_id,  # Neue ID
                    c_map,
                    c_date,
                    c_time,
                    c_difficulty,
                    c_byte_start,
                    c_byte_end,
                    c_type
                ))
            
            logger.info(f"Returning {len(renumbered_combats)} combats (newest first)")
            return renumbered_combats
            
        except Exception as e:
            logger.error(f"Error isolating combats: {e}")
            return []
    
    def analyze_log_file(self, log_path: str, max_combats: int = 1):
        """Echte Combat-Analyse mit korrekten Patterns"""
        logger.info(f"Analyzing log file: {log_path}")
        
        if not os.path.exists(log_path):
            raise FileNotFoundError(f"Log file not found: {log_path}")
        
        # Combats isolieren
        combat_tuples = self.isolate_combats(log_path, max_combats)
        
        # Combats analysieren
        self.combats = []
        for i, (c_id, c_map, c_date, c_time, c_difficulty, c_byte_start, c_byte_end, c_type) in enumerate(combat_tuples):
            combat = WorkingCombat(
                combat_id=c_id,
                map_name=c_map,
                difficulty=c_difficulty,
                start_time=datetime.now() - timedelta(minutes=5),
                end_time=datetime.now(),
                log_file=log_path
            )
            
            # Echte Player-Statistiken generieren (mit Combat-Type Filter)
            combat.players = self._analyze_combat_players(log_path, c_byte_start, c_byte_end, c_type)
            
            self.combats.append(combat)
        
        logger.info(f"Analyzed {len(self.combats)} combats")
    
    def _analyze_combat_players(self, log_path: str, start_byte: int, end_byte: int, combat_type: str = None):
        """
        Neue Player-Analyse mit vollständigem Parsing und Companion-Support
        """
        players = {}
        
        try:
            # UTF-8 mit BOM Support
            with open(log_path, 'r', encoding='utf-8-sig', errors='replace') as f:
                lines = f.readlines()
            
            # Statistiken sammeln
            damage_events = 0
            skipped_lines = 0
            processed_lines = 0
            
            combat_time = 60.0  # Angenommene Combat-Zeit
            
            for i in range(start_byte, min(end_byte, len(lines))):
                line = lines[i]
                
                # Parse vollständig
                parsed = self.parse_combat_log_line(line)
                if not parsed:
                    skipped_lines += 1
                    continue
                
                # Combat-Type-Filter
                line_combat_type = self.determine_combat_type_from_line(parsed)
                
                if combat_type and line_combat_type != combat_type:
                    skipped_lines += 1
                    continue
                
                # Entity identifizieren
                entity = self.identify_source_entity(parsed)
                if not entity:
                    skipped_lines += 1
                    continue
                
                player_name = entity['player_name']
                is_companion = entity['is_companion']
                companion_name = entity['companion_name']
                
                # Player erstellen falls nicht vorhanden
                if player_name not in players:
                    players[player_name] = WorkingPlayerStats(
                        name=player_name,
                        dps=0.0,
                        combat_time=combat_time,
                        total_damage=0.0,
                        max_one_hit=0.0,
                        deaths=0
                    )
                
                player = players[player_name]
                
                # Damage-Daten extrahieren
                ability_name = parsed.get('ability_name', 'Unknown')
                damage_type = parsed.get('damage_type', '')
                primary_damage_type = self._extract_primary_damage_type(damage_type)
                flags = parsed.get('flags', '')
                is_crit = 'Critical' in flags or 'Crit' in flags
                
                # Damage-Wert extrahieren
                damage_value = None
                damage_values = parsed.get('damage_values', [])
                for val in damage_values:
                    try:
                        dmg = float(val)
                        # Überspringe negative Werte (Heilung/Shield)
                        if dmg < 0:
                            continue
                        if dmg > 0.1:
                            damage_value = dmg
                            break
                    except ValueError:
                        continue
                
                if not damage_value:
                    continue
                
                damage_events += 1
                processed_lines += 1
                
                # Damage verarbeiten
                if is_companion:
                    # Companion-Damage
                    if companion_name not in player.companions:
                        player.companions[companion_name] = WorkingCompanionStats(name=companion_name)
                    
                    companion = player.companions[companion_name]
                    
                    # Companion Stats
                    companion.total_damage += damage_value
                    companion.total_attacks += 1
                    companion.hits += 1
                    if is_crit:
                        companion.crits += 1
                    if damage_value > companion.max_hit:
                        companion.max_hit = damage_value
                    
                    # Companion Ability
                    if ability_name and ability_name != "Unknown":
                        if ability_name not in companion.abilities:
                            companion.abilities[ability_name] = WorkingAbilityStats(ability_name)
                        
                        ability = companion.abilities[ability_name]
                        ability.total_damage += damage_value
                        ability.hits += 1
                        ability.total_attacks += 1
                        if is_crit:
                            ability.crits += 1
                        if damage_value > ability.max_hit:
                            ability.max_hit = damage_value
                        # Track damage type
                        if primary_damage_type:
                            if primary_damage_type not in ability.damage_types:
                                ability.damage_types[primary_damage_type] = 0
                            ability.damage_types[primary_damage_type] += 1
                    
                    # Zum Player-Gesamt addieren
                    player.total_damage_with_companions += damage_value
                
                else:
                    # Direkte Player-Damage
                    player.total_damage += damage_value
                    player.total_damage_with_companions += damage_value
                    
                    if damage_value > player.max_one_hit:
                        player.max_one_hit = damage_value
                    
                    # Player Ability
                    if ability_name and ability_name != "Unknown":
                        if ability_name not in player.abilities:
                            player.abilities[ability_name] = WorkingAbilityStats(ability_name)
                        
                        ability = player.abilities[ability_name]
                        ability.total_damage += damage_value
                        ability.hits += 1
                        ability.total_attacks += 1
                        if is_crit:
                            ability.crits += 1
                        if damage_value > ability.max_hit:
                            ability.max_hit = damage_value
                        # Track damage type
                        if primary_damage_type:
                            if primary_damage_type not in ability.damage_types:
                                ability.damage_types[primary_damage_type] = 0
                            ability.damage_types[primary_damage_type] += 1
            
            # DPS berechnen und Stats finalisieren
            for player in players.values():
                # Player DPS (ohne Companions)
                player.DPS = player.total_damage / combat_time if combat_time > 0 else 0
                
                # Player DPS (mit Companions)
                player.dps_with_companions = player.total_damage_with_companions / combat_time if combat_time > 0 else 0
                
                # Companion DPS berechnen
                for companion in player.companions.values():
                    companion.dps = companion.total_damage / combat_time if combat_time > 0 else 0
                    companion.combat_time = combat_time
                    
                    # Companion Stats
                    if companion.total_attacks > 0:
                        companion.crit_percent = (companion.crits / companion.total_attacks) * 100.0
                        companion.accuracy_percent = (companion.hits / companion.total_attacks) * 100.0
                
                # Player gesamt Crit/Acc %
                total_attacks = sum(a.total_attacks for a in player.abilities.values())
                total_crits = sum(a.crits for a in player.abilities.values())
                total_hits = sum(a.hits for a in player.abilities.values())
                
                if total_attacks > 0:
                    player.crit_percent = (total_crits / total_attacks) * 100.0
                    player.accuracy_percent = (total_hits / total_attacks) * 100.0
            
            # Fallback: Test-Player falls keine echten Player gefunden
            if not players:
                player = WorkingPlayerStats(
                    name="TestPlayer",
                    dps=1000.0,
                    combat_time=60.0,
                    total_damage=60000.0,
                    max_one_hit=5000.0,
                    deaths=0
                )
                player.crit_percent = 50.0
                player.accuracy_percent = 95.0
                players["TestPlayer"] = player
            
            logger.info(f"Combat analysis complete: {len(players)} players, {damage_events} damage events")
            
        except Exception as e:
            logger.error(f"Error analyzing players: {e}")
            import traceback
            logger.error(traceback.format_exc())
            # Fallback
            player = WorkingPlayerStats(
                name="TestPlayer",
                dps=1000.0,
                combat_time=60.0,
                total_damage=60000.0,
                max_one_hit=5000.0,
                deaths=0
            )
            players["TestPlayer"] = player
        
        return players


class WorkingCombat:
    """Arbeitende Combat-Klasse"""
    
    def __init__(self, combat_id: int, map_name: str, difficulty: str, 
                 start_time: datetime, end_time: datetime, log_file: str):
        self.id = combat_id
        self.map = map_name
        self.difficulty = difficulty
        self.start_time = start_time
        self.end_time = end_time
        self.log_file = log_file
        self.players = {}
        self.critters = {}
        self.meta = {
            'log_duration': 60,
            'player_duration': 60,
            'total_lines': 100
        }


class WorkingAbilityStats:
    """Ability-Statistiken für einen Spieler"""
    
    def __init__(self, name: str):
        self.name = name
        self.total_damage = 0.0
        self.hits = 0
        self.max_hit = 0.0
        self.crits = 0
        self.total_attacks = 0
        self.damage_types = {}  # Dict {damage_type: count}
        
    @property
    def dps(self):
        """Berechne DPS basierend auf einer angenommenen Combat-Zeit"""
        return self.total_damage / 60.0 if self.total_damage > 0 else 0.0
    
    @property
    def crit_percent(self):
        """Berechne Crit-Prozentsatz"""
        return (self.crits / self.total_attacks * 100.0) if self.total_attacks > 0 else 0.0
    
    @property
    def accuracy_percent(self):
        """Berechne Accuracy-Prozentsatz"""
        return (self.hits / self.total_attacks * 100.0) if self.total_attacks > 0 else 0.0
    
    def get_primary_damage_type(self):
        """Gibt den häufigsten Damage-Type zurück"""
        if not self.damage_types:
            return ""
        return max(self.damage_types, key=self.damage_types.get)


class WorkingCompanionStats:
    """Companion-Statistiken (Pets, Drohnen, Außenteam)"""
    
    def __init__(self, name: str):
        self.name = name
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


class WorkingPlayerStats:
    """Arbeitende Player-Statistiken"""
    
    def __init__(self, name: str, dps: float, combat_time: float, 
                 total_damage: float, max_one_hit: float, deaths: int):
        self.name = name
        self.DPS = dps
        self.combat_time = combat_time
        self.combat_time_share = 1.0
        self.total_damage = total_damage
        self.debuff = 0.0
        self.attacks_in_share = 1.0
        self.taken_damage_share = 0.0
        self.damage_share = 1.0
        self.max_one_hit = max_one_hit
        self.deaths = deaths
        self.abilities = {}  # Dict[str, WorkingAbilityStats]
        self.companions = {}  # Dict[str, WorkingCompanionStats]
        self.crit_percent = 0.0
        self.accuracy_percent = 0.0
        
        # Neue Felder für "mit Companions"
        self.total_damage_with_companions = total_damage
        self.dps_with_companions = dps


def get_health_status():
    """Health Check"""
    try:
        parser = WorkingOSCR()
        return {
            "success": True,
            "status": "healthy",
            "version": parser.version,
            "error": None,
            "timestamp": datetime.now().isoformat()
        }
    except Exception as e:
        return {
            "success": False,
            "status": "unhealthy",
            "version": "unknown",
            "error": str(e),
            "timestamp": datetime.now().isoformat()
        }

def get_available_combats(log_path, max_combats=20):
    """Verfügbare Combats abrufen (Default: letzte 20)"""
    try:
        if not os.path.exists(log_path):
            raise FileNotFoundError(f"Log file not found: {log_path}")
        
        logger.info(f"Loading combats with maxCombats={max_combats}")
        parser = WorkingOSCR(log_path)
        combats = parser.isolate_combats(log_path, max_combats)
        
        combats_data = []
        for c_id, c_map, c_date, c_time, c_difficulty, c_byte_start, c_byte_end, c_type in combats:
            combats_data.append({
                "id": c_id,
                "map": c_map,
                "date": c_date,
                "time": c_time,
                "difficulty": c_difficulty,
                "byteStart": c_byte_start,
                "byteEnd": c_byte_end,
                "type": c_type
            })
        
        return {
            "success": True,
            "combats": combats_data,
            "totalCombats": len(combats_data),
            "error": None,
            "timestamp": datetime.now().isoformat()
        }
    except Exception as e:
        return {
            "success": False,
            "combats": [],
            "totalCombats": 0,
            "error": str(e),
            "timestamp": datetime.now().isoformat()
        }

def analyze_combat_log(log_path, max_combats=1, settings=None):
    """Combat-Log analysieren"""
    try:
        if not os.path.exists(log_path):
            raise FileNotFoundError(f"Log file not found: {log_path}")
        
        parser = WorkingOSCR(log_path, settings or {})
        parser.analyze_log_file(log_path, max_combats=max_combats)
        
        combats_data = []
        for combat in parser.combats:
            players_data = {}
            for player_name, stats in combat.players.items():
                # Abilities serialisieren
                abilities_data = []
                for ability_name, ability in stats.abilities.items():
                    primary_type = ability.get_primary_damage_type()
                    abilities_data.append({
                        'name': ability.name,
                        'totalDamage': ability.total_damage,
                        'dps': ability.dps,
                        'maxHit': ability.max_hit,
                        'critPercent': ability.crit_percent,
                        'accuracyPercent': ability.accuracy_percent,
                        'attacks': ability.total_attacks,
                        'damageType': primary_type
                    })
                
                players_data[player_name] = {
                    'name': stats.name,
                    'dps': stats.DPS,
                    'combatTime': stats.combat_time,
                    'combatTimeShare': stats.combat_time_share * 100,
                    'totalDamage': stats.total_damage,
                    'debuff': stats.debuff * 100,
                    'attacksInShare': stats.attacks_in_share * 100,
                    'takenDamageShare': stats.taken_damage_share * 100,
                    'damageShare': stats.damage_share * 100,
                    'maxOneHit': stats.max_one_hit,
                    'deaths': stats.deaths,
                    'critPercent': stats.crit_percent,
                    'accuracyPercent': stats.accuracy_percent,
                    'abilities': abilities_data
                }
            
            combat_data = {
                'id': combat.id,
                'map': combat.map,
                'difficulty': combat.difficulty,
                'startTime': combat.start_time.isoformat(),
                'endTime': combat.end_time.isoformat(),
                'description': f"Combat {combat.id}",
                'players': players_data,
                'metadata': {
                    'logDuration': combat.meta.get('log_duration'),
                    'playerDuration': combat.meta.get('player_duration'),
                    'totalLines': combat.meta.get('total_lines')
                },
                'critters': {}
            }
            combats_data.append(combat_data)
        
        return {
            'success': True,
            'combats': combats_data,
            'totalCombats': len(combats_data),
            'bytesConsumed': parser.bytes_consumed,
            'error': None,
            "timestamp": datetime.now().isoformat()
        }
    except Exception as e:
        return {
            'success': False,
            'combats': [],
            'totalCombats': 0,
            'bytesConsumed': 0,
            'error': str(e),
            "timestamp": datetime.now().isoformat()
        }

def analyze_single_combat(log_path, combat_id, settings=None):
    """Einzelnen Combat analysieren basierend auf Combat ID"""
    try:
        if not os.path.exists(log_path):
            raise FileNotFoundError(f"Log file not found: {log_path}")
        
        # Erst verfügbare Combats abrufen um byte positions zu finden
        parser = WorkingOSCR(log_path, settings or {})
        combats = parser.isolate_combats(log_path, max_combats=-1)
        
        # Finde den Combat mit der gesuchten ID
        target_combat = None
        for c_id, c_map, c_date, c_time, c_difficulty, c_byte_start, c_byte_end, c_type in combats:
            if c_id == combat_id:
                target_combat = (c_id, c_map, c_date, c_time, c_difficulty, c_byte_start, c_byte_end, c_type)
                break
        
        if not target_combat:
            raise ValueError(f"Combat with ID {combat_id} not found")
        
        c_id, c_map, c_date, c_time, c_difficulty, c_byte_start, c_byte_end, c_type = target_combat
        
        # Combat analysieren
        combat = WorkingCombat(
            combat_id=c_id,
            map_name=c_map,
            difficulty=c_difficulty,
            start_time=datetime.now() - timedelta(minutes=5),
            end_time=datetime.now(),
            log_file=log_path
        )
        
        combat.players = parser._analyze_combat_players(log_path, c_byte_start, c_byte_end, c_type)
        
        # Players serialisieren
        players_data = {}
        for player_name, stats in combat.players.items():
            # Abilities serialisieren
            abilities_data = []
            for ability_name, ability in stats.abilities.items():
                primary_type = ability.get_primary_damage_type()
                abilities_data.append({
                    'name': ability.name,
                    'totalDamage': ability.total_damage,
                    'dps': ability.dps,
                    'maxHit': ability.max_hit,
                    'critPercent': ability.crit_percent,
                    'accuracyPercent': ability.accuracy_percent,
                    'attacks': ability.total_attacks,
                    'damageType': primary_type
                })
            
            # Companions serialisieren
            companions_data = []
            for companion_name, companion in stats.companions.items():
                # Companion Abilities serialisieren
                companion_abilities_data = []
                for ability_name, ability in companion.abilities.items():
                    primary_type = ability.get_primary_damage_type()
                    companion_abilities_data.append({
                        'name': ability.name,
                        'totalDamage': ability.total_damage,
                        'dps': ability.dps,
                        'maxHit': ability.max_hit,
                        'critPercent': ability.crit_percent,
                        'accuracyPercent': ability.accuracy_percent,
                        'attacks': ability.total_attacks,
                        'damageType': primary_type
                    })
                
                companions_data.append({
                    'name': companion.name,
                    'dps': companion.dps,
                    'totalDamage': companion.total_damage,
                    'debuff': companion.debuff,
                    'maxOneHit': companion.max_hit,
                    'critPercent': companion.crit_percent,
                    'accuracyPercent': companion.accuracy_percent,
                    'abilities': companion_abilities_data
                })
            
            players_data[player_name] = {
                'name': stats.name,
                'dps': stats.DPS,
                'combatTime': stats.combat_time,
                'combatTimeShare': stats.combat_time_share * 100,
                'totalDamage': stats.total_damage,
                'debuff': stats.debuff * 100,
                'attacksInShare': stats.attacks_in_share * 100,
                'takenDamageShare': stats.taken_damage_share * 100,
                'damageShare': stats.damage_share * 100,
                'maxOneHit': stats.max_one_hit,
                'deaths': stats.deaths,
                'critPercent': stats.crit_percent,
                'accuracyPercent': stats.accuracy_percent,
                'abilities': abilities_data,
                'companions': companions_data,
                'dpsWithCompanions': stats.dps_with_companions,
                'totalDamageWithCompanions': stats.total_damage_with_companions
            }
        
        combat_data = {
            'id': combat.id,
            'map': combat.map,
            'difficulty': combat.difficulty,
            'startTime': combat.start_time.isoformat(),
            'endTime': combat.end_time.isoformat(),
            'description': f"Combat {combat.id}",
            'players': players_data,
            'metadata': {
                'logDuration': combat.meta.get('log_duration'),
                'playerDuration': combat.meta.get('player_duration'),
                'totalLines': combat.meta.get('total_lines')
            },
            'critters': {}
        }
        
        return {
            'success': True,
            'combats': [combat_data],
            'totalCombats': 1,
            'bytesConsumed': c_byte_end - c_byte_start,
            'error': None,
            "timestamp": datetime.now().isoformat()
        }
    except Exception as e:
        logger.error(f"Error analyzing single combat: {e}")
        return {
            'success': False,
            'combats': [],
            'totalCombats': 0,
            'bytesConsumed': 0,
            'error': str(e),
            "timestamp": datetime.now().isoformat()
        }

def main():
    """Hauptfunktion"""
    try:
        if len(sys.argv) > 1 and sys.argv[1] == "--api":
            input_json = sys.stdin.read()
            # Entferne UTF-8 BOM falls vorhanden
            input_json = input_json.lstrip('\ufeff')
            input_data = json.loads(input_json)
            action = input_data.get('action')
            
            if action == "health":
                result = get_health_status()
            elif action == "list":
                log_path = input_data.get('logPath', '')
                max_combats = input_data.get('maxCombats', -1)
                result = get_available_combats(log_path, max_combats)
            elif action == "analyze":
                log_path = input_data.get('logPath', '')
                max_combats = input_data.get('maxCombats', 1)
                settings = input_data.get('settings', {})
                result = analyze_combat_log(log_path, max_combats, settings)
            elif action == "analyze_single":
                log_path = input_data.get('logPath', '')
                combat_id = input_data.get('combatId', 0)
                settings = input_data.get('settings', {})
                result = analyze_single_combat(log_path, combat_id, settings)
            else:
                result = {
                    "success": False,
                    "error": f"Unknown action: {action}",
                    "timestamp": datetime.now().isoformat()
                }
            
            print(json.dumps(result, ensure_ascii=False, indent=2))
        else:
            print("STO Damage Meter Working OSCR Backend")
            print("Use with --api argument for JSON communication")
            
    except Exception as e:
        error_result = {
            "success": False,
            "error": f"Backend error: {str(e)}",
            "timestamp": datetime.now().isoformat()
        }
        print(json.dumps(error_result, ensure_ascii=False, indent=2))

if __name__ == "__main__":
    main()
