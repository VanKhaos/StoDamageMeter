#!/usr/bin/env python3
"""
Erstellt eine Standalone OSCR-Executable mit echter Funktionalität
"""

import os
import sys
import subprocess
import shutil
from pathlib import Path

def create_standalone_oscr():
    """Erstellt eine Standalone OSCR-Executable"""
    print("Creating standalone OSCR executable...")
    
    # Erstelle eine einfache OSCR-Wrapper-Datei
    oscr_wrapper = '''#!/usr/bin/env python3
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
'''
    
    # Schreibe die Wrapper-Datei
    with open('standalone_oscr.py', 'w', encoding='utf-8') as f:
        f.write(oscr_wrapper)
    
    # Erstelle PyInstaller-Spec für Standalone
    spec_content = '''# -*- mode: python ; coding: utf-8 -*-

import os
from pathlib import Path

# Pfade definieren
backend_dir = Path('.')
oscr_dir = backend_dir / 'OSCR'

# Alle OSCR-Python-Dateien als Daten hinzufügen
datas = []
for py_file in oscr_dir.glob('*.py'):
    datas.append((str(py_file), f'OSCR/{py_file.name}'))

# OSCR-Daten-Dateien hinzufügen
if (oscr_dir / 'Data').exists():
    datas.append((str(oscr_dir / 'Data'), 'OSCR/Data'))

a = Analysis(
    ['standalone_oscr.py'],
    pathex=[str(backend_dir), str(oscr_dir)],
    binaries=[],
    datas=datas,
    hiddenimports=[
        'OSCR',
        'OSCR.main',
        'OSCR.combat',
        'OSCR.datamodels',
        'OSCR.detection',
        'OSCR.iofunc',
        'OSCR.parser',
        'OSCR.utilities',
        'OSCR.constants',
        'OSCR.liveparser',
        'OSCR.oscr_read_file_backwards',
        'numpy',
        'json',
        'logging',
        'traceback',
        'datetime',
        'collections',
        'collections.abc',
        'multiprocessing',
        'queue',
        'os',
        'sys',
        'pathlib'
    ],
    hookspath=[],
    hooksconfig={},
    runtime_hooks=[],
    excludes=[
        'tkinter',
        'matplotlib',
        'pandas',
        'scipy',
        'PIL',
        'cv2',
        'tensorflow',
        'torch',
        'jupyter',
        'notebook',
        'IPython',
        'test',
        'unittest',
        'doctest'
    ],
    win_no_prefer_redirects=False,
    win_private_assemblies=False,
    cipher=None,
    noarchive=False,
)

# Entferne unnötige Module
a.binaries = [x for x in a.binaries if not any(exclude in x[0].lower() for exclude in [
    'tkinter', 'matplotlib', 'pandas', 'scipy', 'PIL', 'cv2', 'tensorflow', 'torch'
])]

pyz = PYZ(a.pure, a.zipped_data, cipher=None)

exe = EXE(
    pyz,
    a.scripts,
    a.binaries,
    a.zipfiles,
    a.datas,
    [],
    name='OSCRBackend',
    debug=False,
    bootloader_ignore_signals=False,
    strip=False,
    upx=True,
    upx_exclude=[],
    runtime_tmpdir=None,
    console=True,
    disable_windowed_traceback=False,
    argv_emulation=False,
    target_arch=None,
    codesign_identity=None,
    entitlements_file=None,
    icon=None
)
'''
    
    with open('standalone_oscr.spec', 'w', encoding='utf-8') as f:
        f.write(spec_content)
    
    # Baue die Standalone-Executable
    cmd = ['python', '-m', 'PyInstaller', '--clean', 'standalone_oscr.spec']
    result = subprocess.run(cmd, capture_output=True, text=True)
    
    if result.returncode == 0:
        print("PASS Standalone OSCR executable created")
        return True
    else:
        print(f"FAIL Error creating standalone executable: {result.stderr}")
        return False

if __name__ == "__main__":
    create_standalone_oscr()
