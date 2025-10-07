# -*- mode: python ; coding: utf-8 -*-

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
    [str(oscr_dir / 'real_integrated_api.py')],
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
        'main',
        'combat',
        'datamodels',
        'detection',
        'iofunc',
        'parser',
        'utilities',
        'constants',
        'liveparser',
        'oscr_read_file_backwards',
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
