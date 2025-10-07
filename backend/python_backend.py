#!/usr/bin/env python3
"""
Python Backend für STO Damage Meter
Verwendet echte OSCR-Funktionalität
"""

import sys
import os
import json
import logging
from pathlib import Path
from datetime import datetime

# Logging konfigurieren
logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

# OSCR-Module importieren
try:
    # Füge OSCR-Verzeichnis zum Python-Pfad hinzu
    current_dir = Path(__file__).parent
    oscr_dir = current_dir / 'OSCR'
    sys.path.insert(0, str(oscr_dir))
    
    from OSCR.main import OSCR
    from OSCR.datamodels import OverviewTableRow, Combat, CritterMeta
    OSCR_AVAILABLE = True
    logger.info("OSCR modules imported successfully")
except ImportError as e:
    OSCR_AVAILABLE = False
    logger.error(f"Failed to import OSCR modules: {e}")

def get_health_status():
    """Health Check"""
    if not OSCR_AVAILABLE:
        return {
            "success": False,
            "status": "unhealthy",
            "version": "unknown",
            "error": "OSCR modules not available",
            "timestamp": datetime.now().isoformat()
        }
    
    try:
        parser = OSCR()
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
    if not OSCR_AVAILABLE:
        return {
            "success": False,
            "combats": [],
            "totalCombats": 0,
            "error": "OSCR modules not available",
            "timestamp": datetime.now().isoformat()
        }
    
    try:
        if not os.path.exists(log_path):
            raise FileNotFoundError(f"Log file not found: {log_path}")
        
        parser = OSCR(log_path)
        combats = parser.isolate_combats(log_path, max_combats)
        
        combats_data = []
        for c_id, c_map, c_date, c_time, c_difficulty, c_byte_start, c_byte_end in combats:
            combats_data.append({
                "id": c_id,
                "map": c_map,
                "date": c_date,
                "time": c_time,
                "difficulty": c_difficulty,
                "byteStart": c_byte_start,
                "byteEnd": c_byte_end
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
    if not OSCR_AVAILABLE:
        return {
            "success": False,
            "combats": [],
            "totalCombats": 0,
            "bytesConsumed": 0,
            "error": "OSCR modules not available",
            "timestamp": datetime.now().isoformat()
        }
    
    try:
        if not os.path.exists(log_path):
            raise FileNotFoundError(f"Log file not found: {log_path}")
        
        parser = OSCR(log_path, settings or {})
        parser.analyze_log_file(log_path, max_combats=max_combats)
        
        combats_data = []
        for combat in parser.combats:
            if combat is None:
                continue
                
            players_data = {}
            if hasattr(combat, 'players') and combat.players:
                for player_name, stats in combat.players.items():
                    if isinstance(stats, OverviewTableRow):
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
                            'deaths': stats.deaths
                        }
            
            combat_data = {
                'id': combat.id,
                'map': combat.map if hasattr(combat, 'map') else 'Unknown',
                'difficulty': combat.difficulty if hasattr(combat, 'difficulty') else 'Unknown',
                'startTime': combat.start_time.isoformat() if hasattr(combat, 'start_time') and combat.start_time else datetime.now().isoformat(),
                'endTime': combat.end_time.isoformat() if hasattr(combat, 'end_time') and combat.end_time else datetime.now().isoformat(),
                'description': combat.description if hasattr(combat, 'description') else f"Combat {combat.id}",
                'players': players_data,
                'metadata': {
                    'logDuration': combat.meta.get('log_duration') if hasattr(combat, 'meta') and combat.meta else 0,
                    'playerDuration': combat.meta.get('player_duration') if hasattr(combat, 'meta') and combat.meta else 0,
                    'totalLines': combat.meta.get('total_lines') if hasattr(combat, 'meta') and combat.meta else 0
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
            else:
                result = {
                    "success": False,
                    "error": f"Unknown action: {action}",
                    "timestamp": datetime.now().isoformat()
                }
            
            print(json.dumps(result, ensure_ascii=False, indent=2))
        else:
            print("STO Damage Meter Python Backend")
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
