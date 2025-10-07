#!/usr/bin/env python3
"""
Test-Script für OSCR API-Wrapper
Testet die API-Funktionalität ohne PyInstaller
"""

import json
import sys
import os
from pathlib import Path

# OSCR-Module importieren
sys.path.insert(0, str(Path(__file__).parent))
from OSCR.api_wrapper import process_combat_log_api, get_available_combats_api, health_check_api

def test_health_check():
    """Testet den Health-Check"""
    print("=== Testing Health Check ===")
    result = health_check_api()
    print(f"Result: {json.dumps(result, indent=2)}")
    return result['success']

def test_available_combats():
    """Testet das Abrufen verfügbarer Combats"""
    print("\n=== Testing Available Combats ===")
    
    # Beispiel-Log-Pfad (muss durch echten Pfad ersetzt werden)
    test_input = {
        'logPath': 'test_combat.log',  # Ersetzen Sie dies durch einen echten Pfad
        'maxCombats': 5
    }
    
    result = get_available_combats_api(test_input)
    print(f"Result: {json.dumps(result, indent=2)}")
    return result['success']

def test_combat_analysis():
    """Testet die Combat-Analyse"""
    print("\n=== Testing Combat Analysis ===")
    
    # Beispiel-Log-Pfad (muss durch echten Pfad ersetzt werden)
    test_input = {
        'logPath': 'test_combat.log',  # Ersetzen Sie dies durch einen echten Pfad
        'maxCombats': 2,
        'settings': {
            'combatsToParse': 2,
            'secondsBetweenCombats': 100,
            'combatMinLines': 20
        }
    }
    
    result = process_combat_log_api(test_input)
    print(f"Result: {json.dumps(result, indent=2)}")
    return result['success']

def test_json_serialization():
    """Testet JSON-Serialisierung"""
    print("\n=== Testing JSON Serialization ===")
    
    test_data = {
        'action': 'analyze',
        'logPath': 'test.log',
        'maxCombats': 1,
        'settings': {
            'combatsToParse': 1,
            'secondsBetweenCombats': 100
        }
    }
    
    try:
        json_str = json.dumps(test_data)
        parsed_data = json.loads(json_str)
        print("PASS JSON serialization/deserialization works")
        return True
    except Exception as e:
        print(f"FAIL JSON error: {e}")
        return False

def main():
    """Hauptfunktion für Tests"""
    print("OSCR API Wrapper Test Suite")
    print("=" * 40)
    
    tests = [
        ("Health Check", test_health_check),
        ("JSON Serialization", test_json_serialization),
        ("Available Combats", test_available_combats),
        ("Combat Analysis", test_combat_analysis)
    ]
    
    results = []
    
    for test_name, test_func in tests:
        try:
            success = test_func()
            results.append((test_name, success))
            print(f"{'PASS' if success else 'FAIL'} {test_name}: {'PASSED' if success else 'FAILED'}")
        except Exception as e:
            print(f"ERROR {test_name}: ERROR - {e}")
            results.append((test_name, False))
    
    print("\n" + "=" * 40)
    print("Test Summary:")
    passed = sum(1 for _, success in results if success)
    total = len(results)
    print(f"Passed: {passed}/{total}")
    
    if passed == total:
        print("SUCCESS: All tests passed!")
        return 0
    else:
        print("FAILED: Some tests failed")
        return 1

if __name__ == "__main__":
    sys.exit(main())
