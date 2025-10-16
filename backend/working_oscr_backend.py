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
logger.setLevel(logging.DEBUG)

# Console Handler entfernt - nur File-Logging
# console_handler = logging.StreamHandler()
# console_handler.setLevel(logging.DEBUG)

# File Handler - schreibt ins logs/ Unterverzeichnis mit Rotation
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

# RotatingFileHandler statt FileHandler: Max 10 MB pro Datei, 3 Backups
from logging.handlers import RotatingFileHandler
file_handler = RotatingFileHandler(
    log_file_path, 
    mode='a', 
    maxBytes=10*1024*1024,  # 10 MB
    backupCount=3,  # Halte 3 Backup-Dateien (oscr_backend.log.1, .2, .3)
    encoding='utf-8'
)
file_handler.setLevel(logging.DEBUG)

# Format
formatter = logging.Formatter('%(asctime)s - %(levelname)s - %(message)s')
# console_handler.setFormatter(formatter)  # Console Handler entfernt
file_handler.setFormatter(formatter)

# Handler hinzufügen
# logger.addHandler(console_handler)  # Console Handler entfernt
logger.addHandler(file_handler)

# Log-Pfad ausgeben damit Benutzer weiß wo die Log-Datei ist

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
            "seconds_between_combats": 45,
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
    
    def _parse_time_from_line(self, line):
        """Parst den Timestamp aus einer Combat-Log-Zeile"""
        try:
            # Format: "YY:MM:DD:HH:MM:SS.ms::rest_of_line"
            if '::' in line:
                timestamp_str = line.split('::')[0]
                parsed_time = self.parse_timestamp(timestamp_str)
                return parsed_time
            else:
                return None
        except Exception as e:
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
    
    def _calculate_combat_duration(self, lines: list, start_idx: int, end_idx: int) -> float:
        """
        Berechnet Combat-Dauer aus Timestamps (erste bis letzte Zeile)
        Verwendet gleiche Logik wie get_available_combats (Zeile 1056-1080)
        
        Args:
            lines: Liste aller Log-Zeilen
            start_idx: Start-Index des Combats
            end_idx: End-Index des Combats (exklusiv)
        
        Returns:
            Duration in Sekunden, 60.0 als Fallback
        """
        try:
            if end_idx - start_idx >= 2:
                start_line = lines[start_idx].strip()
                end_line = lines[end_idx - 1].strip()
                
                start_time = self._parse_time_from_line(start_line)
                end_time = self._parse_time_from_line(end_line)
                
                if start_time and end_time:
                    duration = (end_time - start_time).total_seconds()
                    if duration > 0:
                        logger.debug(f"Calculated combat duration: {duration:.1f}s from {start_time} to {end_time}")
                        return duration
                    else:
                        logger.warning(f"Invalid duration calculated: {duration}s")
            else:
                logger.warning(f"Not enough lines for duration calculation: {end_idx - start_idx} lines")
        except Exception as e:
            logger.warning(f"Could not calculate combat duration: {e}")
        
        logger.debug("Using fallback duration: 60.0s")
        return 60.0
    
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
    
    def is_damage_taken_or_healing(self, parsed_line: dict, player_name: str) -> tuple:
        """
        Prüft ob eine Zeile Schaden erhalten oder Heilung für den Spieler ist
        
        Returns: (is_damage_taken, is_healing_received, is_healing_given, damage_value)
        """
        if not parsed_line:
            return False, False, False, 0.0
        
        target_name = parsed_line.get('target_name', '')
        source_name = parsed_line.get('source_name', '')
        owner_name = parsed_line.get('owner_name', '')
        damage_values = parsed_line.get('damage_values', [])
        
        # Schaden erhalten: Target ist der Spieler, aber Source ist nicht der Spieler
        is_damage_taken = (target_name == player_name and 
                          source_name != player_name and 
                          owner_name != player_name)
        
        # Heilung erhalten: Target ist der Spieler, Source ist der Spieler oder ein Companion
        is_healing_received = (target_name == player_name and 
                              (source_name == player_name or 
                               any(marker in parsed_line.get('source_type', '') for marker in ['P[', '@'])))
        
        # Heilung gegeben: Source ist der Spieler, Target ist jemand anderes
        is_healing_given = (source_name == player_name and 
                           target_name != player_name and 
                           target_name != '')
        
        # Damage-Wert extrahieren (erster positiver Wert)
        damage_value = 0.0
        for dmg_str in damage_values:
            try:
                dmg = float(dmg_str.strip())
                if dmg > 0:
                    damage_value = dmg
                    break
            except (ValueError, AttributeError):
                continue
        
        return is_damage_taken, is_healing_received, is_healing_given, damage_value

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
            'companion_name': 'Companion Name' oder None (HTML-bereinigt),
            'companion_type': 'AwayTeam' | 'KitModule' | 'TempControlled' | None
        }
        """
        if not parsed_line:
            return None
        
        owner_name = parsed_line.get('owner_name', '')
        owner_type = parsed_line.get('owner_type', '')
        source_name = parsed_line.get('source_name', '')
        source_type = parsed_line.get('source_type', '')
        
        # Nur Player-Events verarbeiten (erweitert um @ Marker)
        if not owner_type:
            return None
        if not any(marker in owner_type for marker in ['P[', '@']):
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
        companion_type = None
        
        # Source leer oder nur Wildcard = Spieler direkt
        if not source_name or source_name.strip() in ('', '*'):
            is_companion = False
        # Source gefüllt und anders als Owner = Companion
        elif source_name != owner_name:
            if source_type and (source_type.startswith('C[') or source_type.startswith('S[')):
                is_companion = True
                companion_name = self.clean_name(source_name)  # HTML-Tags entfernen!
                
                # Companion-Typ bestimmen
                if source_type.startswith('S['):
                    companion_type = 'AwayTeam'
                elif source_type.startswith('C[') and 'Kit' in source_type:
                    companion_type = 'KitModule'
                elif source_type.startswith('C['):
                    companion_type = 'TempControlled'
        
        return {
            'player_handle': handle,
            'player_name': player_name,
            'is_companion': is_companion,
            'companion_name': companion_name,
            'companion_type': companion_type
        }
    
    def isolate_combats(self, path: str, max_combats: int = -1):
        """Echte Combat-Isolation basierend auf Zeit-Differenzen"""
        
        if not os.path.exists(path):
            raise FileNotFoundError(f"Log file not found: {path}")
        
        combats = []
        
        try:
            # Log-Datei lesen mit UTF-8 BOM Support
            with open(path, 'r', encoding='utf-8-sig', errors='replace') as f:
                lines = f.readlines()
            
            
            # Combat-Daten sammeln (chronologisch, ältester zuerst)
            current_combat_lines = []
            current_combat_start_idx = 0
            last_timestamp = None
            current_combat_type = None  # Wird bei erster erkannter Zeile gesetzt, dann fix für Combat
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
                            # Combat-Type direkt verwenden (wurde bei erster Zeile gesetzt)
                            # Default ist Space falls nie erkannt
                            combat_type = current_combat_type if current_combat_type else 'Space'
                            
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
                                date_str = "2025-10-09"
                                time_str = "00:00:00"
                            
                            # Combat-Type als Map-Name
                            combat_type_name = f"{combat_type} Combat"
                            
                            # NUR Combats mit Player-Daten speichern (Owner muss P[ enthalten)
                            has_players = any(',P[' in line and '::' in line for line in current_combat_lines[:min(50, len(current_combat_lines))])
                            if has_players:
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
                        
                        # Neuen Combat starten - Type zurücksetzen
                        current_combat_lines = []
                        current_combat_start_idx = i
                        current_combat_type = None  # Zurücksetzen für neuen Combat
                
                # Zeile zum aktuellen Combat hinzufügen
                current_combat_lines.append(line)
                last_timestamp = current_time
                
                # Combat-Type bei erster Erkennung setzen (bleibt dann fix für Combat)
                if line_combat_type and not current_combat_type:
                    current_combat_type = line_combat_type
            
            # Letzten Combat hinzufügen falls vorhanden
            if (current_combat_lines and 
                len(current_combat_lines) >= self._settings['combat_min_lines']):
                # Combat-Type direkt verwenden (wurde bei erster Zeile gesetzt)
                # Default ist Space falls nie erkannt
                combat_type = current_combat_type if current_combat_type else 'Space'
                
                # NUR Combats mit Player-Daten speichern (Owner muss P[ enthalten)
                has_players = any(',P[' in line and '::' in line for line in current_combat_lines[:min(50, len(current_combat_lines))])
                
                if has_players:
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
            
            
            # Falls Limit gesetzt: Nur die LETZTEN N Combats nehmen (neueste)
            if max_combats > 0 and len(combats) > max_combats:
                combats_to_return = combats[-max_combats:]  # Letzte N = neueste
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
            
            # Debug-Logging für Combat-Isolation
            logger.debug(f"Combat Isolation Stats:")
            logger.debug(f"  - Total lines in log: {len(lines)}")
            logger.debug(f"  - Combats found: {len(combats)}")
            logger.debug(f"  - Combats returned: {len(renumbered_combats)}")
            logger.debug(f"  - Max combats requested: {max_combats}")
            
            for i, (c_id, c_map, c_date, c_time, c_difficulty, c_byte_start, c_byte_end, c_type) in enumerate(renumbered_combats[:3]):  # Log first 3
                logger.debug(f"  - Combat {c_id}: {c_map} ({c_type}) - Lines {c_byte_start}-{c_byte_end}")
            
            return renumbered_combats
            
        except Exception as e:
            logger.error(f"Error in isolate_combats: {e}")
            return []
    
    def analyze_log_file(self, log_path: str, max_combats: int = 1):
        """Echte Combat-Analyse mit korrekten Patterns"""
        
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
                start_time=(datetime.now() - timedelta(minutes=5)).isoformat(),
                end_time=datetime.now().isoformat(),
                log_file=log_path
            )
            
            # Echte Player-Statistiken generieren (mit Combat-Type Filter)
            combat.players = self._analyze_combat_players(log_path, c_byte_start, c_byte_end, c_type)
            
            self.combats.append(combat)
        
    
    def _analyze_combat_lines_direct(self, combat_lines: list, combat_type: str = None, duration: float = 60.0):
        """
        Analysiert Combat-Zeilen direkt (für Live-Parsing)
        
        Args:
            combat_lines: Liste von Combat-Log-Zeilen
            combat_type: Combat-Typ (Space/Ground)
            duration: Combat-Dauer in Sekunden
        
        Returns:
            Dict mit Player-Statistiken
        """
        players = {}
        
        try:
            # Berechne Duration aus combat_lines wenn möglich
            calculated_duration = None
            if combat_lines and len(combat_lines) >= 2:
                first_parsed = self.parse_combat_log_line(combat_lines[0])
                last_parsed = self.parse_combat_log_line(combat_lines[-1])
                if first_parsed and last_parsed:
                    start_time = first_parsed.get('timestamp')
                    end_time = last_parsed.get('timestamp')
                    if start_time and end_time:
                        calculated_duration = (end_time - start_time).total_seconds()
                        if calculated_duration > 0:
                            logger.debug(f"Live parsing calculated duration: {calculated_duration:.1f}s")
                        else:
                            calculated_duration = None
            
            # Verwende berechnete Duration oder Fallback
            duration = calculated_duration if calculated_duration and calculated_duration > 0 else duration
            
            # Detaillierte Statistiken sammeln
            damage_events = 0
            skipped_lines = 0
            processed_lines = 0
            small_damage_count = 0  # Damage-Werte zwischen 0 und 0.1
            
            # Neue detaillierte Statistiken
            parse_failed_count = 0
            no_entity_count = 0
            no_damage_value_count = 0
            damage_taken_count = 0
            healing_received_count = 0
            healing_given_count = 0
            
            for line in combat_lines:
                # Parse vollständig
                parsed = self.parse_combat_log_line(line)
                if not parsed:
                    parse_failed_count += 1
                    skipped_lines += 1
                    continue
                
                # Combat-Type-Filter DEAKTIVIERT für Live-Parsing
                # Problem: Wenn erste Zeile anderen Type hat, werden alle anderen ignoriert!
                # line_combat_type = self.determine_combat_type_from_line(parsed)
                # 
                # if combat_type and line_combat_type != combat_type:
                #     skipped_lines += 1
                #     continue
                
                # Entity identifizieren
                entity = self.identify_source_entity(parsed)
                if not entity:
                    skipped_lines += 1
                    continue
                
                player_name = entity['player_name']
                is_companion = entity['is_companion']
                companion_name = entity['companion_name']
                companion_type = entity['companion_type']
                
                # Player erstellen falls nicht vorhanden
                if player_name not in players:
                    players[player_name] = WorkingPlayerStats(
                        name=player_name,
                        dps=0.0,
                        combat_time=duration,
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
                        if dmg < 0:
                            continue
                        if dmg > 0:  # Alle positiven Werte akzeptieren (vorher > 0.1)
                            damage_value = dmg
                            if dmg <= 0.1:  # Zähle kleine Werte für Debugging
                                small_damage_count += 1
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
                        player.companions[companion_name] = WorkingCompanionStats(name=companion_name, companion_type=companion_type)
                    
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
                    player.total_attacks += 1  # Player total_attacks inkrementieren
                    
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
                        if primary_damage_type:
                            if primary_damage_type not in ability.damage_types:
                                ability.damage_types[primary_damage_type] = 0
                            ability.damage_types[primary_damage_type] += 1
                    
                    # Max One Hit
                    if damage_value > player.max_one_hit:
                        player.max_one_hit = damage_value
            
            # Berechne DPS für alle Player
            for player in players.values():
                if duration > 0:
                    player.DPS = player.total_damage / duration
                    player.dps_with_companions = player.total_damage_with_companions / duration
                
                # Berechne Ability DPS (mit echter Combat-Zeit)
                for ability in player.abilities.values():
                    if duration > 0:
                        ability.dps = ability.total_damage / duration
                
                # Berechne Companion DPS
                for companion in player.companions.values():
                    if duration > 0:
                        companion.dps = companion.total_damage / duration
                    
                    # Berechne Crit% und Accuracy%
                    if companion.total_attacks > 0:
                        companion.crit_percent = (companion.crits / companion.total_attacks * 100.0)
                        companion.accuracy_percent = (companion.hits / companion.total_attacks * 100.0)
                    
                    # Berechne Companion Ability DPS (mit echter Combat-Zeit)
                    for ability in companion.abilities.values():
                        if duration > 0:
                            ability.dps = ability.total_damage / duration
                
                # Player gesamt Crit/Acc % berechnen
                total_attacks = sum(a.total_attacks for a in player.abilities.values())
                total_crits = sum(a.crits for a in player.abilities.values())
                total_hits = sum(a.hits for a in player.abilities.values())
                
                # Synchronisiere player.total_attacks mit Ability-Attacks
                player.total_attacks = total_attacks
                
                if total_attacks > 0:
                    player.crit_percent = (total_crits / total_attacks) * 100.0
                    player.accuracy_percent = (total_hits / total_attacks) * 100.0
            
            # Debug-Logging für Live-Parsing
            logger.debug(f"Live Combat Analysis Stats:")
            logger.debug(f"  - Combat lines processed: {len(combat_lines)}")
            logger.debug(f"  - Successfully parsed: {processed_lines}")
            logger.debug(f"  - Skipped lines: {skipped_lines}")
            logger.debug(f"  - Parse failed: {parse_failed_count}")
            logger.debug(f"  - No entity found: {no_entity_count}")
            logger.debug(f"  - No damage value: {no_damage_value_count}")
            logger.debug(f"  - Small damage values (0-0.1): {small_damage_count}")
            logger.debug(f"  - Damage taken events: {damage_taken_count}")
            logger.debug(f"  - Healing received events: {healing_received_count}")
            logger.debug(f"  - Healing given events: {healing_given_count}")
            # logger.debug(f"  - Unknown combat type: {unknown_type_count}")  # Variable nicht definiert in dieser Funktion
            # logger.debug(f"  - Type mismatch: {type_mismatch_count}")  # Variable nicht definiert in dieser Funktion
            logger.debug(f"  - Players found: {len(players)}")
            logger.debug(f"  - Combat duration: {duration:.1f}s")
            
            for player_name, stats in players.items():
                logger.debug(f"  - {player_name}: {stats.total_damage:.0f} dmg, {stats.DPS:.0f} DPS, {stats.total_attacks} attacks")
                logger.debug(f"    Damage taken: {stats.damage_taken:.0f}, Healing received: {stats.healing_received:.0f}, Healing given: {stats.healing_given:.0f}")
            
            return players
            
        except Exception as e:
            logger.error(f"Error in _analyze_combat_lines_direct: {e}")
            return {}
    
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
            small_damage_count = 0  # Damage-Werte zwischen 0 und 0.1
            unknown_type_count = 0  # Zeilen mit unbekanntem Combat-Type
            type_mismatch_count = 0  # Zeilen mit Type-Mismatch
            
            # Berechne echte Combat-Zeit aus Timestamps
            combat_time = self._calculate_combat_duration(lines, start_byte, end_byte)
            
            for i in range(start_byte, min(end_byte, len(lines))):
                line = lines[i]
                
                # Parse vollständig
                parsed = self.parse_combat_log_line(line)
                if not parsed:
                    skipped_lines += 1
                    continue
                
                # Combat-Type-Filter (locker: None-Werte werden akzeptiert)
                line_combat_type = self.determine_combat_type_from_line(parsed)
                
                if combat_type and line_combat_type:
                    # Nur ignorieren wenn beide Types bekannt und unterschiedlich
                    if line_combat_type != combat_type:
                        type_mismatch_count += 1
                        skipped_lines += 1
                        continue
                elif combat_type and not line_combat_type:
                    # Zeile mit unbekanntem Type wird akzeptiert
                    unknown_type_count += 1
                
                # Entity identifizieren
                entity = self.identify_source_entity(parsed)
                if not entity:
                    skipped_lines += 1
                    continue
                
                player_name = entity['player_name']
                is_companion = entity['is_companion']
                companion_name = entity['companion_name']
                companion_type = entity['companion_type']
                
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
                        if dmg > 0:  # Alle positiven Werte akzeptieren (vorher > 0.1)
                            damage_value = dmg
                            if dmg <= 0.1:  # Zähle kleine Werte für Debugging
                                small_damage_count += 1
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
                        player.companions[companion_name] = WorkingCompanionStats(name=companion_name, companion_type=companion_type)
                    
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
                    player.total_attacks += 1  # Player total_attacks inkrementieren
                    
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
                
                # Ability DPS berechnen (mit echter Combat-Zeit)
                for ability in player.abilities.values():
                    ability.dps = ability.total_damage / combat_time if combat_time > 0 else 0
                
                # Companion DPS berechnen
                for companion in player.companions.values():
                    companion.dps = companion.total_damage / combat_time if combat_time > 0 else 0
                    companion.combat_time = combat_time
                    
                    # Companion Stats
                    if companion.total_attacks > 0:
                        companion.crit_percent = (companion.crits / companion.total_attacks) * 100.0
                        companion.accuracy_percent = (companion.hits / companion.total_attacks) * 100.0
                    
                    # Companion Ability DPS berechnen (mit echter Combat-Zeit)
                    for ability in companion.abilities.values():
                        ability.dps = ability.total_damage / combat_time if combat_time > 0 else 0
                
                # Player gesamt Crit/Acc %
                total_attacks = sum(a.total_attacks for a in player.abilities.values())
                total_crits = sum(a.crits for a in player.abilities.values())
                total_hits = sum(a.hits for a in player.abilities.values())
                
                # Synchronisiere player.total_attacks mit Ability-Attacks
                player.total_attacks = total_attacks
                
                if total_attacks > 0:
                    player.crit_percent = (total_crits / total_attacks) * 100.0
                    player.accuracy_percent = (total_hits / total_attacks) * 100.0
            
            # Keine Fallback-Daten mehr - leere Liste wenn keine Spieler gefunden
            
        except Exception as e:
            players = {}
            logger.error(f"Error in _analyze_combat_players: {e}")
        
        # Debug-Logging für Parsing-Statistiken
        logger.debug(f"Combat Analysis Stats:")
        logger.debug(f"  - Total lines processed: {end_byte - start_byte}")
        logger.debug(f"  - Successfully parsed: {processed_lines}")
        logger.debug(f"  - Skipped lines: {skipped_lines}")
        logger.debug(f"  - Small damage values (0-0.1): {small_damage_count}")
        # logger.debug(f"  - Unknown combat type: {unknown_type_count}")  # Variable nicht definiert in dieser Funktion
        # logger.debug(f"  - Type mismatch: {type_mismatch_count}")  # Variable nicht definiert in dieser Funktion
        logger.debug(f"  - Players found: {len(players)}")
        logger.debug(f"  - Combat duration: {combat_time:.1f}s")
        
        for player_name, stats in players.items():
            logger.debug(f"  - {player_name}: {stats.total_damage:.0f} dmg, {stats.DPS:.0f} DPS, {stats.total_attacks} attacks")
        
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
        self.dps = 0.0  # Wird mit echter Combat-Zeit berechnet
        self.hits = 0
        self.max_hit = 0.0
        self.crits = 0
        self.total_attacks = 0
        self.damage_types = {}  # Dict {damage_type: count}
    
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
    
    def __init__(self, name: str, companion_type: str = None):
        self.name = name
        self.type = companion_type  # 'AwayTeam', 'KitModule', 'TempControlled'
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
        self.total_attacks = 0  # Fehlendes Attribut hinzugefügt
        
        # Neue Felder für Schaden erhalten und Heilung (nicht für DPS-Berechnung)
        self.damage_taken = 0.0  # Schaden den der Spieler erhalten hat
        self.healing_received = 0.0  # Heilung die der Spieler erhalten hat
        self.healing_given = 0.0  # Heilung die der Spieler gegeben hat
        
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

def get_available_combats(log_path, max_combats=100):
    """Verfügbare Combats abrufen (Default: letzte 20)"""
    try:
        if not os.path.exists(log_path):
            raise FileNotFoundError(f"Log file not found: {log_path}")
        
        parser = WorkingOSCR(log_path)
        # Lade alle Combats und dann die neuesten N
        all_combats = parser.isolate_combats(log_path, -1)  # Alle Combats
        if max_combats > 0 and len(all_combats) > max_combats:
            combats = all_combats[:max_combats]  # Neueste N Combats
        else:
            combats = all_combats
        
        combats_data = []
        for c_id, c_map, c_date, c_time, c_difficulty, c_byte_start, c_byte_end, c_type in combats:
            # Calculate duration using the same logic as JSON debug log
            duration = 0
            try:
                # Use the same method as isolate_combats - read line by line
                with open(log_path, 'r', encoding='utf-8-sig', errors='replace') as f:
                    all_lines = f.readlines()
                
                # c_byte_start and c_byte_end are actually line indices in isolate_combats
                start_line_idx = c_byte_start
                end_line_idx = c_byte_end
                
                # Get the actual lines from the log
                if start_line_idx < len(all_lines) and end_line_idx <= len(all_lines):
                    # Get first and last lines of this combat
                    if end_line_idx - start_line_idx >= 2:
                        start_line = all_lines[start_line_idx].strip()
                        end_line = all_lines[end_line_idx - 1].strip()
                        
                        # Parse timestamps using the same method as JSON debug
                        start_time = parser._parse_time_from_line(start_line)
                        end_time = parser._parse_time_from_line(end_line)
                        
                        # Calculate combat time
                        if start_time and end_time:
                            duration = (end_time - start_time).total_seconds()
            except Exception as e:
                duration = 0
            
            combats_data.append({
                "id": c_id,
                "map": c_map,
                "date": c_date,
                "time": c_time,
                "difficulty": c_difficulty,
                "byteStart": c_byte_start,
                "byteEnd": c_byte_end,
                "type": c_type,
                "duration": duration
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
        players_data = []  # LISTE statt Dictionary!
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
                    'type': companion.type,  # 'AwayTeam', 'KitModule', 'TempControlled'
                    'dps': companion.dps,
                    'totalDamage': companion.total_damage,
                    'debuff': companion.debuff,
                    'maxOneHit': companion.max_hit,
                    'critPercent': companion.crit_percent,
                    'accuracyPercent': companion.accuracy_percent,
                    'abilities': companion_abilities_data
                })
            
            # An LISTE anhängen (nicht Dictionary-Key!)
            players_data.append({
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
            })
        
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
        return {
            'success': False,
            'combats': [],
            'totalCombats': 0,
            'bytesConsumed': 0,
            'error': str(e),
            "timestamp": datetime.now().isoformat()
        }

def live_parse_log(log_path: str, from_byte_offset: int = 0, combat_timeout_seconds: int = 45):
    """
    Live-Parsing: Liest nur neue Zeilen ab from_byte_offset und erkennt neue/aktive Combats
    
    Args:
        log_path: Pfad zur Combat-Log-Datei
        from_byte_offset: Byte-Position ab der gelesen werden soll
        combat_timeout_seconds: Sekunden nach denen ein Combat als beendet gilt
    
    Returns:
        Dict mit new_combats, active_combat_info, current_byte_offset
    """
    try:
        parser = WorkingOSCR()
        
        if not os.path.exists(log_path):
            return {
                'success': False,
                'error': f"Log file not found: {log_path}",
                'current_byte_offset': from_byte_offset,
                'new_combats': [],
                'active_combat': None,
                'timestamp': datetime.now().isoformat()
            }
        
        file_size = os.path.getsize(log_path)
        
        if from_byte_offset >= file_size:
            # Keine neuen Daten
            return {
                'success': True,
                'current_byte_offset': file_size,
                'new_combats': [],
                'active_combat': None,
                'timestamp': datetime.now().isoformat()
            }
        
        new_lines = []
        with open(log_path, 'r', encoding='utf-8', errors='replace') as f:
            f.seek(from_byte_offset)
            new_content = f.read()
            new_lines = new_content.splitlines()
        
        if not new_lines:
            return {
                'success': True,
                'current_byte_offset': file_size,
                'new_combats': [],
                'active_combat': None,
                'timestamp': datetime.now().isoformat()
            }
        
        # Parse neue Zeilen und erkenne Combats
        combats = []
        current_combat_lines = []
        last_combat_time = None
        current_combat_type = None
        combat_start_line = from_byte_offset
        line_count = 0
        
        for line in new_lines:
            line_count += 1
            if not line.strip():
                continue
            
            parsed = parser.parse_combat_log_line(line)
            if not parsed or not parsed.get('timestamp'):
                continue
            
            timestamp = parsed['timestamp']
            combat_type = parser.determine_combat_type_from_line(parsed)
            
            # Erste Zeile oder Combat-Timeout
            if last_combat_time is None:
                last_combat_time = timestamp
                current_combat_type = combat_type
                current_combat_lines = [line]
                continue
            
            time_diff = (timestamp - last_combat_time).total_seconds()
            type_changed = (combat_type and current_combat_type and combat_type != current_combat_type)
            
            # Combat-Ende-Kriterien
            if time_diff > combat_timeout_seconds or type_changed:
                # Speichere vorherigen Combat wenn genug Zeilen
                if len(current_combat_lines) >= 20:
                    combats.append({
                        'lines': current_combat_lines,
                        'start_time': last_combat_time,
                        'type': current_combat_type,
                        'line_count': len(current_combat_lines)
                    })
                
                # Starte neuen Combat
                current_combat_lines = [line]
                current_combat_type = combat_type
            else:
                current_combat_lines.append(line)
            
            last_combat_time = timestamp
        
        # Check ob aktueller Combat noch aktiv ist (letzter Combat im Buffer)
        active_combat = None
        if current_combat_lines and len(current_combat_lines) >= 20:
            # Combat ist aktiv wenn letzte Zeile weniger als combat_timeout_seconds alt ist
            active_combat = {
                'lines': current_combat_lines,
                'start_time': last_combat_time,
                'type': current_combat_type,
                'line_count': len(current_combat_lines),
                'is_active': True
            }
        
        # Konvertiere abgeschlossene Combats zu Combat-Infos
        completed_combats = []
        for idx, combat in enumerate(combats):
            completed_combats.append({
                'id': idx,
                'date': combat['start_time'].strftime('%Y-%m-%d') if combat['start_time'] else '',
                'time': combat['start_time'].strftime('%H:%M:%S') if combat['start_time'] else '',
                'type': combat['type'] or 'Unknown',
                'line_count': combat['line_count']
            })
        
        return {
            'success': True,
            'current_byte_offset': file_size,
            'new_combats': completed_combats,
            'active_combat': active_combat if active_combat else None,
            'lines_processed': line_count,
            'timestamp': datetime.now().isoformat()
        }
    
    except Exception as e:
        return {
            'success': False,
            'error': str(e),
            'current_byte_offset': from_byte_offset,
            'new_combats': [],
            'active_combat': None,
            'timestamp': datetime.now().isoformat()
        }

def incremental_combat_update(log_path: str, combat_lines: list, settings: dict = None):
    """
    Inkrementelle Combat-Analyse: Analysiert Combat-Zeilen und berechnet Stats
    
    Args:
        log_path: Pfad zur Combat-Log-Datei (für Context)
        combat_lines: Liste der Combat-Log-Zeilen
        settings: Optionale Analyse-Einstellungen
    
    Returns:
        Dict mit vollständiger CombatData (Player-Stats, DPS, Rankings, etc.)
    """
    try:
        parser = WorkingOSCR()
        
        if not combat_lines or len(combat_lines) < 1:  # Minimum 1 Zeile für schnelle Updates!
            return {
                'success': False,
                'error': 'No combat lines provided',
                'combats': [],
                'totalCombats': 0,
                'timestamp': datetime.now().isoformat()
            }
        
        # Parse erste Zeile für Combat-Type und Timestamp
        first_parsed = parser.parse_combat_log_line(combat_lines[0])
        last_parsed = parser.parse_combat_log_line(combat_lines[-1])
        
        if not first_parsed or not last_parsed:
            return {
                'success': False,
                'error': 'Could not parse combat lines',
                'combats': [],
                'totalCombats': 0,
                'timestamp': datetime.now().isoformat()
            }
        
        combat_type = parser.determine_combat_type_from_line(first_parsed)
        start_time = first_parsed.get('timestamp')
        end_time = last_parsed.get('timestamp')
        
        duration = 0
        if start_time and end_time:
            duration = (end_time - start_time).total_seconds()
        
        # Analysiere Combat-Zeilen direkt (nicht aus Datei)
        players_data = parser._analyze_combat_lines_direct(combat_lines, combat_type or 'Unknown', duration)
        
        # Serialisiere Player-Stats
        players_list = []
        for player_name, stats in players_data.items():
            player_dict = {
                'name': player_name,
                'dps': stats.DPS,  # Großbuchstaben!
                'dpsWithCompanions': stats.dps_with_companions,
                'totalDamage': stats.total_damage,
                'totalDamageWithCompanions': stats.total_damage_with_companions,
                'debuff': stats.debuff * 100,
                'maxHit': stats.max_one_hit,  # max_one_hit!
                'criticalHitPercentage': stats.crit_percent,  # crit_percent!
                'accuracy': stats.accuracy_percent,  # accuracy_percent!
                'attacks': sum(a.total_attacks for a in stats.abilities.values()) if stats.abilities else 0,  # Berechnet!
                'companions': [],
                'abilities': []
            }
            
            # Companions
            for comp_name, comp_stats in stats.companions.items():
                companion_dict = {
                    'name': comp_name,
                    'type': comp_stats.type,  # 'AwayTeam', 'KitModule', 'TempControlled'
                    'dps': comp_stats.dps,
                    'totalDamage': comp_stats.total_damage,
                    'maxHit': comp_stats.max_hit,
                    'criticalHitPercentage': comp_stats.crit_percent,  # crit_percent!
                    'accuracy': comp_stats.accuracy_percent,  # accuracy_percent!
                    'attacks': comp_stats.total_attacks,
                    'abilities': []
                }
                
                for ability_name, ability_stats in comp_stats.abilities.items():
                    companion_dict['abilities'].append({
                        'name': ability_name,
                        'dps': ability_stats.dps,
                        'totalDamage': ability_stats.total_damage,
                        'maxHit': ability_stats.max_hit,
                        'criticalHitPercentage': ability_stats.crit_percent,  # crit_percent!
                        'accuracy': ability_stats.accuracy_percent,  # accuracy_percent!
                        'attacks': ability_stats.total_attacks,
                        'damageType': ability_stats.get_primary_damage_type()  # Methode aufrufen!
                    })
                
                player_dict['companions'].append(companion_dict)
            
            # Player Abilities
            for ability_name, ability_stats in stats.abilities.items():
                player_dict['abilities'].append({
                    'name': ability_name,
                    'dps': ability_stats.dps,
                    'totalDamage': ability_stats.total_damage,
                    'maxHit': ability_stats.max_hit,
                    'criticalHitPercentage': ability_stats.crit_percent,  # crit_percent!
                    'accuracy': ability_stats.accuracy_percent,  # accuracy_percent!
                    'attacks': ability_stats.total_attacks,
                    'damageType': ability_stats.get_primary_damage_type()  # Methode aufrufen!
                })
            
            players_list.append(player_dict)
        
        combat_data = {
            'id': 0,
            'date': start_time.strftime('%Y-%m-%d') if start_time else '',
            'time': start_time.strftime('%H:%M:%S.%f')[:-5] if start_time else '',
            'type': combat_type or 'Unknown',
            'icon': '🚀' if combat_type == 'Space' else '🏃',
            'duration': duration,
            'players': players_list,
            'totalDamage': sum(p['totalDamageWithCompanions'] for p in players_list),
            'totalDPS': sum(p['dpsWithCompanions'] for p in players_list),
            'lineCount': len(combat_lines)
        }
        
        # Return als Array (combats) für Kompatibilität mit CombatAnalysisResponse!
        return {
            'success': True,
            'combats': [combat_data],  # ← Array statt combat_data!
            'totalCombats': 1,
            'timestamp': datetime.now().isoformat()
        }
    
    except Exception as e:
        return {
            'success': False,
            'error': str(e),
            'combats': [],  # Leeres Array im Fehlerfall
            'totalCombats': 0,
            'timestamp': datetime.now().isoformat()
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
            elif action == "live_parse":
                log_path = input_data.get('logPath', '')
                from_byte_offset = input_data.get('fromByteOffset', 0)
                combat_timeout = input_data.get('combatTimeoutSeconds', 30)
                result = live_parse_log(log_path, from_byte_offset, combat_timeout)
            elif action == "incremental_update":
                log_path = input_data.get('logPath', '')
                combat_lines = input_data.get('combatLines', [])
                settings = input_data.get('settings', {})
                result = incremental_combat_update(log_path, combat_lines, settings)
            else:
                result = {
                    "success": False,
                    "error": f"Unknown action: {action}",
                    "timestamp": datetime.now().isoformat()
                }
            
            # JSON-Antworten müssen in Console ausgegeben werden (nicht in Log-Datei)
            print(json.dumps(result, ensure_ascii=False, indent=2))
        else:
            logger.info("STO Damage Meter Working OSCR Backend")
            logger.info("Use with --api argument for JSON communication")
            
    except Exception as e:
        error_result = {
            "success": False,
            "error": f"Backend error: {str(e)}",
            "timestamp": datetime.now().isoformat()
        }
        # JSON-Fehler müssen in Console ausgegeben werden (nicht in Log-Datei)
        print(json.dumps(error_result, ensure_ascii=False, indent=2))

if __name__ == "__main__":
    main()
