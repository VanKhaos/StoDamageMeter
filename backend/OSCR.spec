# -*- mode: python ; coding: utf-8 -*-

import os
from pathlib import Path

# Pfade definieren
backend_dir = Path(__file__).parent
oscr_dir = backend_dir / 'OSCR'

# Daten-Dateien sammeln
datas = []

# OSCR-Daten-Dateien hinzufügen
if (oscr_dir / 'Data').exists():
    datas.append((str(oscr_dir / 'Data'), 'Data'))

# Alle Python-Dateien im OSCR-Verzeichnis
a = Analysis(
    [str(oscr_dir / 'real_api.py')],
    pathex=[str(backend_dir)],
    binaries=[],
    datas=datas,
    hiddenimports=[
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
        'multiprocessing',
        'queue',
        'os',
        'sys'
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
        'IPython'
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
