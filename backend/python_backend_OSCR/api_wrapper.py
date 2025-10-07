"""
API-Wrapper für WPF-Integration
Ermöglicht JSON-basierte Kommunikation zwischen WPF-Frontend und Python-Backend
"""

import json
import sys
import logging
import traceback
from datetime import datetime
from typing import Dict, Any, List, Optional
try:
    from .main import OSCR
    from .datamodels import OverviewTableRow
except ImportError:
    # Für PyInstaller-Build
    from main import OSCR
    from datamodels import OverviewTableRow

# Logging-Konfiguration
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(name)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler('oscr_api.log'),
        logging.StreamHandler(sys.stderr)
    ]
)
logger = logging.getLogger(__name__)


class OSCRAPIError(Exception):
    """Custom Exception für API-Fehler"""
    pass


def format_overview_row(row: OverviewTableRow) -> Dict[str, Any]:
    """
    Konvertiert OverviewTableRow zu JSON-serialisierbarem Dictionary
    """
    return {
        'name': row.name + row.handle,
        'dps': float(row.DPS),
        'combatTime': float(row.combat_time),
        'combatTimeShare': float(row.combat_time_share * 100),
        'totalDamage': float(row.total_damage),
        'debuff': float(row.debuff * 100),
        'attacksInShare': float(row.attacks_in_share * 100),
        'takenDamageShare': float(row.taken_damage_share * 100),
        'damageShare': float(row.damage_share * 100),
        'maxOneHit': float(row.max_one_hit),
        'deaths': int(row.deaths)
    }


def process_combat_log_api(input_data: Dict[str, Any]) -> Dict[str, Any]:
    """
    API-Endpoint für WPF-Integration
    
    Args:
        input_data: Dictionary mit Log-Pfad und Analyse-Parametern
        
    Returns:
        Dictionary mit Analyse-Ergebnissen oder Fehlermeldung
    """
    try:
        logger.info(f"Starting combat log analysis for: {input_data.get('logPath', 'Unknown')}")
        
        # OSCR-Parser initialisieren
        parser = OSCR(input_data.get('logPath', ''))
        
        # Einstellungen aus Input übernehmen
        settings = input_data.get('settings', {})
        if settings:
            parser._settings.update(settings)
        
        # Log-Datei analysieren
        max_combats = input_data.get('maxCombats', 10)
        parser.analyze_log_file(max_combats=max_combats)
        
        # Combats zu JSON-serialisierbarem Format konvertieren
        combats_data = []
        for combat in parser.combats:
            if combat is None:
                continue
                
            combat_data = {
                'id': combat.id,
                'map': combat.map or 'Unknown',
                'difficulty': combat.difficulty or '',
                'startTime': combat.start_time.isoformat() if combat.start_time else '',
                'endTime': combat.end_time.isoformat() if combat.end_time else '',
                'description': getattr(combat, 'description', f"{combat.map} {combat.difficulty}"),
                'players': {},
                'metadata': {
                    'logDuration': getattr(combat.meta, 'log_duration', 0),
                    'playerDuration': getattr(combat.meta, 'player_duration', 0),
                    'totalLines': len(combat.log_data) if hasattr(combat, 'log_data') else 0
                },
                'critters': {}
            }
            
            # Spieler-Statistiken
            if hasattr(combat, 'players') and combat.players:
                for player_name, player_stats in combat.players.items():
                    if hasattr(player_stats, 'DPS'):
                        combat_data['players'][player_name] = format_overview_row(player_stats)
            
            # NPC-Informationen
            if hasattr(combat, 'critters') and combat.critters:
                for critter_name, critter_meta in combat.critters.items():
                    combat_data['critters'][critter_name] = {
                        'name': critter_meta.name,
                        'count': critter_meta.count,
                        'deaths': critter_meta.deaths,
                        'hullValues': critter_meta.hull_values
                    }
            
            combats_data.append(combat_data)
        
        logger.info(f"Successfully analyzed {len(combats_data)} combats")
        
        return {
            'success': True,
            'combats': combats_data,
            'totalCombats': len(combats_data),
            'bytesConsumed': parser.bytes_consumed,
            'error': None,
            'timestamp': datetime.now().isoformat()
        }
        
    except Exception as e:
        error_msg = f"Error during combat analysis: {str(e)}"
        logger.error(error_msg)
        logger.error(traceback.format_exc())
        
        return {
            'success': False,
            'combats': [],
            'totalCombats': 0,
            'bytesConsumed': 0,
            'error': error_msg,
            'timestamp': datetime.now().isoformat()
        }


def get_available_combats_api(input_data: Dict[str, Any]) -> Dict[str, Any]:
    """
    API-Endpoint zum Abrufen verfügbarer Combats ohne vollständige Analyse
    
    Args:
        input_data: Dictionary mit Log-Pfad
        
    Returns:
        Dictionary mit Liste verfügbarer Combats
    """
    try:
        logger.info(f"Getting available combats for: {input_data.get('logPath', 'Unknown')}")
        
        parser = OSCR(input_data.get('logPath', ''))
        max_combats = input_data.get('maxCombats', 50)
        
        # Nur Combats isolieren, nicht analysieren
        combats = parser.isolate_combats(parser.log_path, max_combats)
        
        combats_info = []
        for combat in combats:
            combats_info.append({
                'id': combat[0],
                'map': combat[1],
                'date': combat[2],
                'time': combat[3],
                'difficulty': combat[4],
                'byteStart': combat[5],
                'byteEnd': combat[6]
            })
        
        logger.info(f"Found {len(combats_info)} available combats")
        
        return {
            'success': True,
            'combats': combats_info,
            'totalCombats': len(combats_info),
            'error': None,
            'timestamp': datetime.now().isoformat()
        }
        
    except Exception as e:
        error_msg = f"Error getting available combats: {str(e)}"
        logger.error(error_msg)
        logger.error(traceback.format_exc())
        
        return {
            'success': False,
            'combats': [],
            'totalCombats': 0,
            'error': error_msg,
            'timestamp': datetime.now().isoformat()
        }


def health_check_api() -> Dict[str, Any]:
    """
    API-Endpoint für Health-Check
    
    Returns:
        Dictionary mit System-Status
    """
    try:
        return {
            'success': True,
            'status': 'healthy',
            'version': OSCR.version,
            'timestamp': datetime.now().isoformat(),
            'error': None
        }
    except Exception as e:
        return {
            'success': False,
            'status': 'unhealthy',
            'version': 'unknown',
            'timestamp': datetime.now().isoformat(),
            'error': str(e)
        }


def main():
    """
    Hauptfunktion für API-Modus
    """
    try:
        # Prüfen ob API-Modus aktiviert ist
        if len(sys.argv) > 1 and sys.argv[1] == "--api":
            # JSON-Input von stdin lesen
            input_json = sys.stdin.read()
            if not input_json.strip():
                raise OSCRAPIError("No input data provided")
            
            input_data = json.loads(input_json)
            action = input_data.get('action', 'analyze')
            
            # Je nach Aktion entsprechende Funktion aufrufen
            if action == 'analyze':
                result = process_combat_log_api(input_data)
            elif action == 'list':
                result = get_available_combats_api(input_data)
            elif action == 'health':
                result = health_check_api()
            else:
                raise OSCRAPIError(f"Unknown action: {action}")
            
            # JSON-Output an stdout senden
            print(json.dumps(result, ensure_ascii=False, indent=2))
            
        else:
            # Normale CLI-Funktionalität
            try:
                from .cli import main as cli_main
            except ImportError:
                from cli import main as cli_main
            cli_main()
            
    except json.JSONDecodeError as e:
        error_result = {
            'success': False,
            'error': f"Invalid JSON input: {str(e)}",
            'timestamp': datetime.now().isoformat()
        }
        print(json.dumps(error_result))
        sys.exit(1)
        
    except Exception as e:
        error_result = {
            'success': False,
            'error': f"Unexpected error: {str(e)}",
            'timestamp': datetime.now().isoformat()
        }
        print(json.dumps(error_result))
        logger.error(traceback.format_exc())
        sys.exit(1)


if __name__ == "__main__":
    main()
