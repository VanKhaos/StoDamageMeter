#!/usr/bin/env python3
"""
Standalone OSCR Wrapper für echte Funktionalität
"""

import sys
import os
import json
from pathlib import Path

# OSCR-Module importieren
sys.path.insert(0, str(Path(__file__).parent / 'OSCR'))

try:
    from OSCR.main import OSCR
    from OSCR.datamodels import OverviewTableRow, Combat, CritterMeta
    OSCR_AVAILABLE = True
except ImportError:
    OSCR_AVAILABLE = False

def main():
    if len(sys.argv) > 1 and sys.argv[1] == "--api":
        input_json = sys.stdin.read()
        input_data = json.loads(input_json)
        action = input_data.get('action')
        
        if not OSCR_AVAILABLE:
            result = {
                "success": False,
                "error": "OSCR modules not available",
                "timestamp": "2025-10-07T22:00:00"
            }
        elif action == "health":
            parser = OSCR()
            result = {
                "success": True,
                "status": "healthy",
                "version": parser.version,
                "timestamp": "2025-10-07T22:00:00",
                "error": None
            }
        elif action == "list":
            log_path = input_data.get('logPath', '')
            max_combats = input_data.get('maxCombats', -1)
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
            
            result = {
                "success": True,
                "combats": combats_data,
                "totalCombats": len(combats_data),
                "error": None,
                "timestamp": "2025-10-07T22:00:00"
            }
        elif action == "analyze":
            log_path = input_data.get('logPath', '')
            max_combats = input_data.get('maxCombats', 1)
            parser = OSCR(log_path)
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
                    'startTime': combat.start_time.isoformat() if hasattr(combat, 'start_time') and combat.start_time else "2025-10-07T22:00:00",
                    'endTime': combat.end_time.isoformat() if hasattr(combat, 'end_time') and combat.end_time else "2025-10-07T22:00:00",
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
            
            result = {
                'success': True,
                'combats': combats_data,
                'totalCombats': len(combats_data),
                'bytesConsumed': parser.bytes_consumed,
                'error': None,
                "timestamp": "2025-10-07T22:00:00"
            }
        else:
            result = {
                "success": False,
                "error": f"Unknown action: {action}",
                "timestamp": "2025-10-07T22:00:00"
            }
        
        print(json.dumps(result, ensure_ascii=False, indent=2))
    else:
        print("OSCR Standalone Wrapper - Use with --api argument")

if __name__ == "__main__":
    main()
