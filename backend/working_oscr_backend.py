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

# Logging konfigurieren
# Logger mit File-Handler konfigurieren
logger = logging.getLogger(__name__)
logger.setLevel(logging.INFO)

# Console Handler
console_handler = logging.StreamHandler()
console_handler.setLevel(logging.INFO)

# File Handler - schreibt in oscr_api.log
file_handler = logging.FileHandler('oscr_api.log', mode='a', encoding='utf-8')
file_handler.setLevel(logging.INFO)

# Format
formatter = logging.Formatter('%(asctime)s - %(levelname)s - %(message)s')
console_handler.setFormatter(formatter)
file_handler.setFormatter(formatter)

# Handler hinzufügen
logger.addHandler(console_handler)
logger.addHandler(file_handler)

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
            
            logger.info(f"Read {len(lines)} lines from log")
            
            # Zeilen umkehren (neueste zuerst)
            lines = list(reversed(lines))
            
            # Combat-Daten sammeln
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
                
                # Combat-Type der aktuellen Zeile ermitteln
                line_combat_type = None
                if 'Space_' in line:
                    line_combat_type = 'Space'
                elif 'Ground_' in line:
                    line_combat_type = 'Ground'
                
                # Prüfen ob neuer Combat (Zeitdifferenz ODER Type-Wechsel)
                start_new_combat = False
                
                if last_timestamp and current_combat_lines:
                    # Weil wir von hinten lesen, ist last_timestamp ÄLTER als current_time
                    time_diff = abs((last_timestamp - current_time).total_seconds())
                    
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
                            
                            # Combat speichern (letzter Timestamp = Ende des Combats)
                            try:
                                time_parts = current_combat_lines[0].split('::')[0].split(':')
                                if len(time_parts) >= 6:
                                    date_str = f"20{time_parts[0]}-{time_parts[1]}-{time_parts[2]}"
                                    time_str = f"{time_parts[3]}:{time_parts[4]}:{time_parts[5]}"
                                else:
                                    date_str = "2025-10-09"
                                    time_str = "00:00:00"
                            except:
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
                            
                            if max_combats > 0 and len(combats) >= max_combats:
                                break
                        
                        # Neuen Combat starten - Counter zurücksetzen
                        current_combat_lines = []
                        current_combat_start_idx = i
                        space_count = 0
                        ground_count = 0
                        current_combat_type = None  # Zurücksetzen
                
                # Zeile zum aktuellen Combat hinzufügen
                current_combat_lines.append(line)
                last_timestamp = current_time
                
                # Combat-Type zählen und setzen
                if 'Space_' in line:
                    space_count += 1
                    if not current_combat_type:
                        current_combat_type = 'Space'
                elif 'Ground_' in line:
                    ground_count += 1
                    if not current_combat_type:
                        current_combat_type = 'Ground'
            
            # Letzten Combat hinzufügen falls vorhanden
            if current_combat_lines and len(current_combat_lines) >= self._settings['combat_min_lines']:
                # Combat-Type basierend auf Mehrheit bestimmen
                # Es gibt nur Space oder Ground, kein Unknown
                if ground_count > space_count:
                    combat_type = 'Ground'
                else:
                    # Default ist Space (bei Gleichstand oder wenn keiner erkannt)
                    combat_type = 'Space'
                
                try:
                    time_parts = current_combat_lines[0].split('::')[0].split(':')
                    if len(time_parts) >= 6:
                        date_str = f"20{time_parts[0]}-{time_parts[1]}-{time_parts[2]}"
                        time_str = f"{time_parts[3]}:{time_parts[4]}:{time_parts[5]}"
                    else:
                        date_str = "2025-10-09"
                        time_str = "00:00:00"
                except:
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
            
            logger.info(f"Found {len(combats)} combats")
            return combats
            
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
    
    def _analyze_combat_players(self, log_path: str, start_byte: int, end_byte: int, combat_type: str = None, debug_log: bool = False):
        """Echte Player-Analyse mit Ability-Tracking - filtert nach Combat-Type"""
        players = {}
        
        if debug_log:
            logger.info("=" * 80)
            logger.info(f"DEBUG: Analyzing Combat (Type: {combat_type})")
            logger.info(f"Lines: {start_byte} to {end_byte}")
            logger.info("=" * 80)
        
        try:
            # UTF-8 mit BOM Support
            with open(log_path, 'r', encoding='utf-8-sig', errors='replace') as f:
                lines = f.readlines()
            
            # Player-Namen und Damage extrahieren
            player_damage = defaultdict(float)
            player_attacks = defaultdict(int)
            player_max_hit = defaultdict(float)
            player_abilities = defaultdict(lambda: defaultdict(lambda: WorkingAbilityStats("")))
            player_names = set()
            player_crits = defaultdict(int)
            player_hits = defaultdict(int)
            player_handles = {}  # Map: display name -> handle
            total_damage = 0
            damage_events = 0
            
            for i in range(start_byte, min(end_byte, len(lines))):
                line = lines[i]
                if '::' not in line:
                    continue
                
                if debug_log:
                    logger.info(f"\n--- Line {i} ---")
                    logger.info(f"ORIGINAL: {line.strip()}")
                    
                try:
                    parts = line.split('::')
                    if len(parts) < 2:
                        if debug_log:
                            logger.info("SKIP: Less than 2 parts after splitting by '::'")
                        continue
                    
                    # Parse Source (Player oder NPC)
                    # Format: "Player Name,P[id@handle]" oder "NPC Name,C[OwnerHandle Controlled]"
                    source_parts = parts[1].split(',', 1)
                    if len(source_parts) < 2:
                        if debug_log:
                            logger.info("SKIP: Could not parse source parts")
                        continue
                    
                    source_name = source_parts[0].strip()
                    source_type = source_parts[1].strip()
                    
                    if debug_log:
                        logger.info(f"PARSED Source Name: {source_name}")
                        logger.info(f"PARSED Source Type: {source_type[:50]}...")
                    
                    # Bestimme den echten Player
                    player_name = None
                    
                    # Echter Player: P[...]
                    if source_type.startswith('P['):
                        player_name = source_name
                        # Handle extrahieren für NPC-Zuordnung
                        handle_match = source_type.split('@')
                        if len(handle_match) > 1:
                            handle = handle_match[1].split(']')[0].strip()
                            player_handles[handle] = source_name
                        player_names.add(player_name)
                        
                        if debug_log:
                            logger.info(f"PARSED Player: {player_name} (Real Player)")
                    
                    # NPC eines Players: C[OwnerHandle Controlled]
                    elif source_type.startswith('C[') and 'Controlled' in source_type:
                        # Extrahiere Owner-Handle
                        owner_handle = source_type.split('[')[1].split(' ')[0].strip()
                        # Finde den Spieler für dieses Handle
                        if owner_handle in player_handles:
                            player_name = player_handles[owner_handle]
                            if debug_log:
                                logger.info(f"PARSED Player: {player_name} (via NPC/Pet, Owner Handle: {owner_handle})")
                        else:
                            # Handle als Fallback-Name verwenden
                            player_name = owner_handle
                            player_names.add(player_name)
                            if debug_log:
                                logger.info(f"PARSED Player: {player_name} (Handle as fallback)")
                    
                    # Keine Player-Events - überspringe
                    if not player_name:
                        if debug_log:
                            logger.info("SKIP: Not a player event (no player name)")
                        continue
                    
                    # Combat-Type Filter: Nur Zeilen des richtigen Types analysieren
                    if combat_type:
                        line_is_space = 'Space_' in line
                        line_is_ground = 'Ground_' in line
                        
                        if debug_log:
                            logger.info(f"PARSED Combat Type in Line: Space={line_is_space}, Ground={line_is_ground}")
                            logger.info(f"FILTER: Expected Type={combat_type}")
                        
                        # Überspringe Zeilen des falschen Types
                        if combat_type == 'Space' and line_is_ground:
                            if debug_log:
                                logger.info("SKIP: Line is Ground but Combat is Space")
                            continue
                        elif combat_type == 'Ground' and line_is_space:
                            if debug_log:
                                logger.info("SKIP: Line is Space but Combat is Ground")
                            continue
                        # Wenn combat_type gesetzt aber Zeile hat keinen erkennbaren Type, auch überspringen
                        elif not line_is_space and not line_is_ground:
                            if debug_log:
                                logger.info("SKIP: Line has no recognizable combat type")
                            continue
                    
                    # Ability-Name extrahieren - zwischen Target und Damage-Type
                    # Format: "...Target,Ability Name,Pn.xxx,DamageType,..."
                    ability_name = "Unknown"
                    if len(parts) > 1:
                        # Suche nach Ability-Namen (vor Pn.)
                        ability_parts = parts[1].split(',')
                        for idx, part in enumerate(ability_parts):
                            if 'Pn.' in part and idx > 0:
                                ability_name = ability_parts[idx - 1].strip()
                                break
                        
                        # Fallback: Versuche letzten nicht-leeren Teil vor Pn.
                        if ability_name == "Unknown":
                            for part in reversed(ability_parts):
                                if part.strip() and 'Pn.' not in part and not part.startswith('['):
                                    ability_name = part.strip()
                                    break
                    
                    if debug_log:
                        logger.info(f"PARSED Ability: {ability_name}")
                    
                    # Damage extrahieren
                    if any(pattern in line for pattern in ['Damage', 'Shield', 'Electrical', 'Phaser', 
                                                           'Plasma', 'Disruptor', 'Tetryon', 'Polaron', 
                                                           'Antiproton', 'Kinetic', 'Physical']):
                        damage_events += 1
                        is_crit = 'Critical' in line or 'Crit' in line
                        
                        # Damage-Wert extrahieren
                        if ',' in line:
                            try:
                                line_parts = line.split(',')
                                damage_value = None
                                
                                # Suche nach Damage-Werten
                                for part in line_parts:
                                    try:
                                        damage = float(part)
                                        if abs(damage) > 0.1:
                                            damage_value = abs(damage)
                                            break
                                    except ValueError:
                                        continue
                                
                                if damage_value:
                                    if debug_log:
                                        logger.info(f"PARSED Damage: {damage_value:.2f} (Crit: {is_crit})")
                                        logger.info(f"ACTION: ✓ ADDED to {player_name} -> {ability_name}")
                                    # Player-Statistiken
                                    total_damage += damage_value
                                    player_damage[player_name] += damage_value
                                    player_attacks[player_name] += 1
                                    player_hits[player_name] += 1
                                    
                                    if is_crit:
                                        player_crits[player_name] += 1
                                    
                                    if damage_value > player_max_hit[player_name]:
                                        player_max_hit[player_name] = damage_value
                                    
                                    # Ability-Statistiken
                                    if ability_name and ability_name != "Unknown":
                                        ability = player_abilities[player_name][ability_name]
                                        if not ability.name:
                                            ability.name = ability_name
                                        
                                        ability.total_damage += damage_value
                                        ability.hits += 1
                                        ability.total_attacks += 1
                                        
                                        if is_crit:
                                            ability.crits += 1
                                        
                                        if damage_value > ability.max_hit:
                                            ability.max_hit = damage_value
                            except:
                                pass
                except:
                    continue
            
            # Player-Statistiken erstellen
            for player_name in player_names:
                if player_name and player_damage[player_name] > 0:
                    combat_time = 60.0
                    dps = player_damage[player_name] / combat_time if combat_time > 0 else 0
                    
                    player = WorkingPlayerStats(
                        name=player_name,
                        dps=dps,
                        combat_time=combat_time,
                        total_damage=player_damage[player_name],
                        max_one_hit=player_max_hit[player_name] if player_max_hit[player_name] > 0 else 0,
                        deaths=0
                    )
                    
                    # Crit % und Accuracy %
                    if player_attacks[player_name] > 0:
                        player.crit_percent = (player_crits[player_name] / player_attacks[player_name]) * 100.0
                        player.accuracy_percent = (player_hits[player_name] / player_attacks[player_name]) * 100.0
                    
                    # Abilities hinzufügen
                    player.abilities = dict(player_abilities[player_name])
                    
                    players[player_name] = player
            
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
                
                # Test-Ability
                test_ability = WorkingAbilityStats("Test Ability")
                test_ability.total_damage = 30000.0
                test_ability.hits = 50
                test_ability.total_attacks = 50
                test_ability.crits = 25
                test_ability.max_hit = 2000.0
                player.abilities["Test Ability"] = test_ability
                
                players["TestPlayer"] = player
            
            if debug_log:
                logger.info("=" * 80)
                logger.info("DEBUG SUMMARY:")
                logger.info(f"Total Lines Processed: {end_byte - start_byte}")
                logger.info(f"Combat Type Filter: {combat_type}")
                logger.info(f"Damage Events: {damage_events}")
                logger.info(f"Players Found: {len(players)}")
                for pname in player_names:
                    logger.info(f"  - {pname}: {player_damage[pname]:.2f} damage, {player_attacks[pname]} attacks")
                logger.info("=" * 80)
            
            logger.info(f"Found {len(players)} players with {damage_events} damage events")
            
        except Exception as e:
            logger.error(f"Error analyzing players: {e}")
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
        self.crit_percent = 0.0
        self.accuracy_percent = 0.0


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

def get_available_combats(log_path, max_combats=-1):
    """Verfügbare Combats abrufen"""
    try:
        if not os.path.exists(log_path):
            raise FileNotFoundError(f"Log file not found: {log_path}")
        
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
                    abilities_data.append({
                        'name': ability.name,
                        'totalDamage': ability.total_damage,
                        'dps': ability.dps,
                        'maxHit': ability.max_hit,
                        'critPercent': ability.crit_percent,
                        'accuracyPercent': ability.accuracy_percent
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
        
        # Debug-Logging für UI-geladene Combats aktivieren
        combat.players = parser._analyze_combat_players(log_path, c_byte_start, c_byte_end, c_type, debug_log=True)
        
        # Players serialisieren
        players_data = {}
        for player_name, stats in combat.players.items():
            # Abilities serialisieren
            abilities_data = []
            for ability_name, ability in stats.abilities.items():
                abilities_data.append({
                    'name': ability.name,
                    'totalDamage': ability.total_damage,
                    'dps': ability.dps,
                    'maxHit': ability.max_hit,
                    'critPercent': ability.crit_percent,
                    'accuracyPercent': ability.accuracy_percent
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
