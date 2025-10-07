"""
Vereinfachte API für WPF-Integration
Funktioniert ohne multiprocessing für bessere PyInstaller-Kompatibilität
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


class OSCRAPIError(Exception):
    """Custom Exception für API-Fehler"""
    pass


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
            'version': '2025.8.10.0',
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


def process_combat_log_api(input_data: Dict[str, Any]) -> Dict[str, Any]:
    """
    API-Endpoint für WPF-Integration (vereinfachte Version)
    
    Args:
        input_data: Dictionary mit Log-Pfad und Analyse-Parametern
        
    Returns:
        Dictionary mit Analyse-Ergebnissen oder Fehlermeldung
    """
    try:
        logger.info(f"Starting combat log analysis for: {input_data.get('logPath', 'Unknown')}")
        
        # Für jetzt nur eine Mock-Response
        # TODO: Echte OSCR-Integration implementieren
        mock_combats = [
            {
                'id': 0,
                'map': 'Test Map',
                'difficulty': 'Normal',
                'startTime': datetime.now().isoformat(),
                'endTime': datetime.now().isoformat(),
                'description': 'Test Combat',
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
        
        logger.info(f"Successfully analyzed {len(mock_combats)} combats (mock)")
        
        return {
            'success': True,
            'combats': mock_combats,
            'totalCombats': len(mock_combats),
            'bytesConsumed': 0,
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
    API-Endpoint zum Abrufen verfügbarer Combats (vereinfachte Version)
    
    Args:
        input_data: Dictionary mit Log-Pfad
        
    Returns:
        Dictionary mit Liste verfügbarer Combats
    """
    try:
        logger.info(f"Getting available combats for: {input_data.get('logPath', 'Unknown')}")
        
        # Mock-Response
        mock_combats = [
            {
                'id': 0,
                'map': 'Test Map',
                'date': '2025-10-07',
                'time': '22:00:00',
                'difficulty': 'Normal',
                'byteStart': 0,
                'byteEnd': 1000
            }
        ]
        
        logger.info(f"Found {len(mock_combats)} available combats (mock)")
        
        return {
            'success': True,
            'combats': mock_combats,
            'totalCombats': len(mock_combats),
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
            # Normale CLI-Funktionalität (vereinfacht)
            print("OSCR Simple API - Use --api flag for API mode")
            print("Example: echo '{\"action\": \"health\"}' | OSCRBackend.exe --api")
            
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
