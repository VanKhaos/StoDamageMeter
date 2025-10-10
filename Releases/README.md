# 📦 STO Damage Meter - Releases

Dieser Ordner enthält alle fertigen Release-ZIPs, die zur Verteilung bereit sind.

## 📋 Verfügbare Releases

### v1.1.0 (2025-10-10)
- **Datei:** `StoDamageMeter_v1.1.0.zip` (68 MB)
- **Status:** ✅ Bereit zur Verteilung
- **Features:**
  - Damage-Type-Icons mit Farbkodierung
  - Attacks-Spalte
  - Component-Refactoring
  - UTF-8 Sonderzeichen-Support
  - Self-Contained (keine Installation erforderlich)

## 🚀 Verteilung

Diese ZIP-Dateien können direkt an andere Spieler weitergegeben werden:
- Discord
- E-Mail
- GitHub Releases
- File-Sharing-Dienste (Google Drive, Dropbox, etc.)

## 📁 Ordner-Struktur

```
Releases/                              ← Release-Ordner
  ├── README.md                       ← Diese Datei
  ├── StoDamageMeter_v1.1.0/         ← Entpackte Dateien (zum Testen)
  └── StoDamageMeter_v1.1.0.zip      ← Diese Datei verteilen! ⭐
```

## 🔄 Neues Release erstellen

```powershell
# 1. Release bauen
.\create_release.ps1 -Version "1.1.1"

# 2. ZIP erstellen (landet automatisch hier)
.\create_release_zip.ps1 -Version "1.1.1"
```

Die neue ZIP-Datei wird automatisch in diesem Ordner erstellt!

## ✅ Bereit zur Verteilung

Dateien in diesem Ordner sind vollständig getestet und bereit zur Weitergabe! 🎉

