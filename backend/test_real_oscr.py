#!/usr/bin/env python3
"""
Test-Script für echte OSCR-Funktionalität mit der echten combatlog.log
"""

import sys
import os
from pathlib import Path

# OSCR-Module importieren
sys.path.insert(0, str(Path(__file__).parent / 'OSCR'))

try:
    from OSCR.main import OSCR
    print("PASS OSCR-Module erfolgreich importiert")
except ImportError as e:
    print(f"FAIL Fehler beim Importieren der OSCR-Module: {e}")
    sys.exit(1)

def test_isolate_combats():
    """Testet die isolate_combats-Funktionalität"""
    print("\nTeste isolate_combats...")
    
    log_path = "../combatlog.log"
    if not os.path.exists(log_path):
        print(f"FAIL Log-Datei nicht gefunden: {log_path}")
        return False
    
    try:
        parser = OSCR(log_path)
        combats = parser.isolate_combats(log_path, max_combats=5)
        
        print(f"PASS {len(combats)} Combats gefunden:")
        for i, (c_id, c_map, c_date, c_time, c_difficulty, c_byte_start, c_byte_end) in enumerate(combats):
            print(f"  Combat {i+1}: {c_map} ({c_difficulty}) - {c_date} {c_time}")
            print(f"    Bytes: {c_byte_start} - {c_byte_end}")
        
        return True
    except Exception as e:
        print(f"FAIL Fehler bei isolate_combats: {e}")
        return False

def test_analyze_combats():
    """Testet die analyze_log_file-Funktionalität"""
    print("\nTeste analyze_log_file...")
    
    log_path = "../combatlog.log"
    if not os.path.exists(log_path):
        print(f"FAIL Log-Datei nicht gefunden: {log_path}")
        return False
    
    try:
        parser = OSCR(log_path)
        parser.analyze_log_file(log_path, max_combats=1)
        
        print(f"PASS {len(parser.combats)} Combats analysiert:")
        for i, combat in enumerate(parser.combats):
            if combat is None:
                continue
            print(f"  Combat {i+1}:")
            print(f"    ID: {combat.id}")
            print(f"    Map: {getattr(combat, 'map', 'Unknown')}")
            print(f"    Difficulty: {getattr(combat, 'difficulty', 'Unknown')}")
            print(f"    Start Time: {getattr(combat, 'start_time', 'Unknown')}")
            print(f"    End Time: {getattr(combat, 'end_time', 'Unknown')}")
            
            # Player-Statistiken
            if hasattr(combat, 'players') and combat.players:
                print(f"    Players: {len(combat.players)}")
                for player_name, stats in combat.players.items():
                    if hasattr(stats, 'DPS'):
                        print(f"      {player_name}: {stats.DPS:.1f} DPS")
        
        return True
    except Exception as e:
        print(f"FAIL Fehler bei analyze_log_file: {e}")
        import traceback
        traceback.print_exc()
        return False

def main():
    """Hauptfunktion"""
    print("Teste echte OSCR-Funktionalitaet mit combatlog.log")
    print("=" * 60)
    
    # Test 1: isolate_combats
    success1 = test_isolate_combats()
    
    # Test 2: analyze_log_file
    success2 = test_analyze_combats()
    
    print("\n" + "=" * 60)
    if success1 and success2:
        print("SUCCESS Alle Tests erfolgreich! Echte OSCR-Funktionalitaet funktioniert.")
    else:
        print("FAIL Einige Tests fehlgeschlagen.")
    
    return 0 if (success1 and success2) else 1

if __name__ == "__main__":
    sys.exit(main())
