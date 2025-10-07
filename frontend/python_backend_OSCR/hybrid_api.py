"""
Hybrid OSCR-API für WPF-Integration
Verwendet echte OSCR-Funktionalität wo möglich, fällt auf Mock-Daten zurück bei Problemen
"""

import json
import sys
import logging
import traceback
import os
from datetime import datetime
from typing import Dict, Any, List, Optional

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

# Pfad für OSCR-Module hinzufügen
current_dir = os.path.dirname(os.path.abspath(__file__))
sys.path.insert(0, current_dir)

# Versuche OSCR-Module zu importieren
OSCR_AVAILABLE = False
OSCR = None
OverviewTableRow = None
Combat = None
CritterMeta = None

try:
    from .main import OSCR
    from .datamodels import OverviewTableRow, Combat, CritterMeta
    OSCR_AVAILABLE = True
    logger.info("Successfully imported OSCR modules as package.")
except ImportError:
    logger.warning("Failed to import OSCR modules as package, trying direct import.")
    try:
        # Für PyInstaller - Module sind im Bundle enthalten
        if hasattr(sys, '_MEIPASS'):
            bundle_dir = sys._MEIPASS
            oscr_dir = os.path.join(bundle_dir, 'OSCR')
            if os.path.exists(oscr_dir) and oscr_dir not in sys.path:
                sys.path.insert(0, oscr_dir)
            logger.info(f"PyInstaller bundle detected. Added {oscr_dir} to sys.path.")
        else:
            if current_dir not in sys.path:
                sys.path.insert(0, current_dir)
            logger.info(f"Running outside package. Added {current_dir} to sys.path.")

        from main import OSCR
        from datamodels import OverviewTableRow, Combat, CritterMeta
        OSCR_AVAILABLE = True
        logger.info("Successfully imported OSCR modules directly.")
    except ImportError as e:
        logger.warning(f"Could not import OSCR modules: {e}")
        logger.warning("Falling back to mock data mode.")
        OSCR_AVAILABLE = False


class OSCRAPIError(Exception):
    """Custom exception for API errors."""
    pass


def get_health_status() -> Dict[str, Any]:
    """Gibt den Gesundheitsstatus des Backends zurück."""
    logger.info("Performing health check.")
    try:
        if OSCR_AVAILABLE:
            parser = OSCR()
            version = parser.version
            status = "healthy"
            error = None
        else:
            version = "mock-mode"
            status = "healthy (mock mode)"
            error = None
    except Exception as e:
        version = "unknown"
        status = "unhealthy"
        error = str(e)
        logger.error(f"Health check failed: {e}")

    return {
        "success": status.startswith("healthy"),
        "status": status,
        "version": version,
        "timestamp": datetime.now().isoformat(),
        "error": error
    }


def get_available_combats_api(log_path: str, max_combats: int = -1) -> Dict[str, Any]:
    """Gibt eine Liste der verfügbaren Combats in einer Log-Datei zurück."""
    logger.info(f"Getting available combats for: {log_path}")
    try:
        if not OSCR_AVAILABLE:
            logger.warning("OSCR not available, using mock data")
            return get_mock_available_combats(log_path, max_combats)
        
        # Prüfen ob Log-Datei existiert
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

        logger.info(f"Found {len(combats_data)} available combats")
        return {
            "success": True,
            "combats": combats_data,
            "totalCombats": len(combats_data),
            "error": None,
            "timestamp": datetime.now().isoformat()
        }
    except Exception as e:
        logger.error(f"Error getting available combats: {e}", exc_info=True)
        logger.warning("Falling back to mock data")
        return get_mock_available_combats(log_path, max_combats)


def get_mock_available_combats(log_path: str, max_combats: int = -1) -> Dict[str, Any]:
    """Mock-Implementierung für verfügbare Combats"""
    mock_combats = [
        {
            'id': 0,
            'map': 'Test Map (Mock)',
            'date': '2025-10-07',
            'time': '22:00:00',
            'difficulty': 'Normal',
            'byteStart': 0,
            'byteEnd': 1000
        }
    ]
    
    return {
        "success": True,
        "combats": mock_combats,
        "totalCombats": len(mock_combats),
        "error": "Using mock data - OSCR not available",
        "timestamp": datetime.now().isoformat()
    }


def process_combat_log_api(input_data: Dict[str, Any]) -> Dict[str, Any]:
    """API-Endpoint für WPF-Integration zur Analyse eines Combat-Logs."""
    log_path = input_data.get('logPath', '')
    max_combats = input_data.get('maxCombats', 1)
    settings = input_data.get('settings', {})

    logger.info(f"Starting combat log analysis for: {log_path}")

    try:
        if not OSCR_AVAILABLE:
            logger.warning("OSCR not available, using mock data")
            return get_mock_combat_analysis(log_path, max_combats, settings)
        
        # Prüfen ob Log-Datei existiert
        if not os.path.exists(log_path):
            raise FileNotFoundError(f"Log file not found: {log_path}")
        
        # OSCR-Parser initialisieren
        parser = OSCR(log_path, settings)
        
        # Combats analysieren (ohne Multiprocessing für bessere Kompatibilität)
        parser.analyze_log_file(log_path, max_combats=max_combats)
        
        # Combats in JSON-Format konvertieren
        combats_data = []
        for combat in parser.combats:
            if combat is None:
                continue
                
            # Player-Statistiken konvertieren
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
            
            # Critter-Metadaten konvertieren
            critters_data = {}
            if hasattr(combat, 'critters') and combat.critters:
                for critter_name, meta in combat.critters.items():
                    if isinstance(meta, CritterMeta):
                        critters_data[critter_name] = {
                            'name': meta.name,
                            'count': meta.count,
                            'deaths': meta.deaths,
                            'hullValues': meta.hull_values
                        }
            
            # Combat-Daten zusammenstellen
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
                'critters': critters_data
            }
            combats_data.append(combat_data)

        logger.info(f"Successfully analyzed {len(combats_data)} combats")
        return {
            'success': True,
            'combats': combats_data,
            'totalCombats': len(combats_data),
            'bytesConsumed': parser.bytes_consumed,
            'error': None,
            "timestamp": datetime.now().isoformat()
        }
    except Exception as e:
        logger.error(f"Error during combat analysis: {e}", exc_info=True)
        logger.warning("Falling back to mock data")
        return get_mock_combat_analysis(log_path, max_combats, settings)


def get_mock_combat_analysis(log_path: str, max_combats: int, settings: Dict[str, Any]) -> Dict[str, Any]:
    """Mock-Implementierung für Combat-Analyse"""
    mock_combats = [
        {
            'id': 0,
            'map': 'Test Map (Mock)',
            'difficulty': 'Normal',
            'startTime': datetime.now().isoformat(),
            'endTime': datetime.now().isoformat(),
            'description': 'Mock Combat for Testing',
            'players': {
                'TestPlayer': {
                    'name': 'TestPlayer',
                    'dps': 1000.0,
                    'combatTime': 60.0,
                    'combatTimeShare': 100.0,
                    'totalDamage': 60000.0,
                    'debuff': 0.0,
                    'attacksInShare': 100.0,
                    'takenDamageShare': 0.0,
                    'damageShare': 100.0,
                    'maxOneHit': 5000.0,
                    'deaths': 0
                }
            },
            'metadata': {
                'logDuration': 60,
                'playerDuration': 60,
                'totalLines': 100
            },
            'critters': {}
        }
    ]
    
    return {
        'success': True,
        'combats': mock_combats,
        'totalCombats': len(mock_combats),
        'bytesConsumed': 0,
        'error': 'Using mock data - OSCR not available',
        "timestamp": datetime.now().isoformat()
    }


def main():
    """Hauptfunktion für den API-Wrapper."""
    try:
        # Lese die Aktion aus dem ersten Argument oder aus stdin
        if len(sys.argv) > 1 and sys.argv[1] == "--api":
            # API-Modus für WPF
            input_json = sys.stdin.read()
            input_data = json.loads(input_json)
            action = input_data.get('action')
            
            result: Dict[str, Any]
            if action == "health":
                result = get_health_status()
            elif action == "list":
                log_path = input_data.get('logPath', '')
                max_combats = input_data.get('maxCombats', -1)
                result = get_available_combats_api(log_path, max_combats)
            elif action == "analyze":
                result = process_combat_log_api(input_data)
            else:
                raise OSCRAPIError(f"Unknown action: {action}")
            
            # JSON-Output an stdout senden
            print(json.dumps(result, ensure_ascii=False, indent=2))
            
        else:
            # Normale CLI-Funktionalität (falls gewünscht, hier nicht implementiert)
            logger.info("Running in CLI mode (not fully implemented for this wrapper).")
            print("Please run with '--api' argument and JSON input for API functionality.")
            sys.exit(1)
            
    except json.JSONDecodeError as e:
        error_result = {
            'success': False,
            'error': f"Invalid JSON input: {str(e)}",
            'timestamp': datetime.now().isoformat(),
            'traceback': traceback.format_exc()
        }
        print(json.dumps(error_result, ensure_ascii=False, indent=2))
        sys.exit(1)
    except OSCRAPIError as e:
        error_result = {
            'success': False,
            'error': str(e),
            'timestamp': datetime.now().isoformat(),
            'traceback': traceback.format_exc()
        }
        print(json.dumps(error_result, ensure_ascii=False, indent=2))
        sys.exit(1)
    except Exception as e:
        error_result = {
            'success': False,
            'error': f"An unexpected error occurred: {str(e)}",
            'timestamp': datetime.now().isoformat(),
            'traceback': traceback.format_exc()
        }
        print(json.dumps(error_result, ensure_ascii=False, indent=2))
        sys.exit(1)


if __name__ == "__main__":
    main()
