# -*- mode: python ; coding: utf-8 -*-


a = Analysis(
    ['OSCR\\real_api.py'],
    pathex=[],
    binaries=[],
    datas=[],
    hiddenimports=[
        'OSCR',
        'OSCR.main',
        'OSCR.datamodels',
        'OSCR.combat',
        'OSCR.parser',
        'OSCR.detection',
        'OSCR.iofunc',
        'OSCR.utilities',
        'OSCR.constants',
        'OSCR.oscr_read_file_backwards',
    ],
    hookspath=[],
    hooksconfig={},
    runtime_hooks=[],
    excludes=['tkinter', 'matplotlib', 'pandas', 'scipy', 'PIL', 'cv2', 'tensorflow', 'torch', 'jupyter', 'notebook', 'IPython', 'test', 'unittest', 'doctest'],
    noarchive=False,
    optimize=2,
)
pyz = PYZ(a.pure)

exe = EXE(
    pyz,
    a.scripts,
    a.binaries,
    a.datas,
    [('O', None, 'OPTION'), ('O', None, 'OPTION')],
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
)
