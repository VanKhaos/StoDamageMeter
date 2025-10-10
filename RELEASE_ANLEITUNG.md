# 📦 STO Damage Meter - Release Anleitung

## ✅ Was wurde erstellt?

### 1. **Release-Dateien im Releases-Ordner**
- **Pfad (entpackt):** `D:\Projekte\StoDamageMeter\Releases\StoDamageMeter_v1.1.0\`
- **Größe:** ~148 MB (entpackt)
- **Dateien:** 414 Dateien
- **Inhalt:**
  - `StoDamageMeter.exe` (Hauptanwendung)
  - `OSCRBackend.exe` (Backend, 7.7 MB)
  - `README.txt` (Benutzer-Anleitung)
  - `appsettings.json` (Konfiguration)
  - Alle notwendigen .NET Runtime-Dateien (self-contained)
  - Alle Dependencies (WPF-UI, Microsoft.Extensions, etc.)

### 2. **ZIP-Datei (Bereit zur Verteilung)** ⭐
- **Pfad:** `D:\Projekte\StoDamageMeter\Releases\StoDamageMeter_v1.1.0.zip`
- **Größe:** ~68 MB (komprimiert)
- **Status:** ✅ Bereit zur Weitergabe

---

## 🚀 Wie verteile ich das Release?

### Option 1: Direkt weitergeben (Empfohlen)
```
1. Gehe zu: D:\Projekte\StoDamageMeter\Releases\
2. Rechtsklick auf: StoDamageMeter_v1.1.0.zip
3. "Senden an" → Deine bevorzugte Methode (Discord, E-Mail, USB-Stick, etc.)
```

### Option 2: Auf GitHub hochladen
```
1. Gehe zu deinem GitHub-Repository
2. Klicke auf "Releases" → "Create a new release"
3. Tag: v1.1.0
4. Titel: "STO Damage Meter v1.1.0"
5. Upload: StoDamageMeter_v1.1.0.zip
6. Publish release
```

### Option 3: Auf File-Sharing-Dienst hochladen
- Google Drive
- Dropbox
- OneDrive
- WeTransfer
- etc.

---

## 👥 Was müssen andere Spieler tun?

### Schritt 1: Entpacken
```
1. ZIP-Datei herunterladen
2. Rechtsklick → "Alle extrahieren"
3. Zielordner auswählen (z.B. "C:\Games\StoDamageMeter")
4. Entpacken
```

### Schritt 2: Starten
```
1. In den entpackten Ordner gehen
2. Doppelklick auf "StoDamageMeter.exe"
3. Fertig!
```

### ⚠️ Wichtig für andere Spieler:
- ✅ **KEINE** .NET Installation erforderlich (Self-Contained Build)
- ✅ **KEINE** Python Installation erforderlich (Backend ist gebündelt)
- ✅ **NUR** Windows 10/11 64-bit erforderlich
- ⚠️ Windows Defender könnte eine Warnung anzeigen (normal für neue .exe Dateien)
  - Lösung: "Weitere Informationen" → "Trotzdem ausführen"

---

## 🔄 Neues Release erstellen

### Wenn du Änderungen gemacht hast:

```powershell
# 1. Release bauen
.\create_release.ps1 -Version "1.1.1"

# 2. ZIP erstellen
.\create_release_zip.ps1 -Version "1.1.1"
```

### Oder alles auf einmal:
```powershell
# Release bauen UND ZIP erstellen
.\create_release.ps1 -Version "1.1.1"
.\create_release_zip.ps1 -Version "1.1.1"
```

---

## 📋 Release-Checkliste

Vor der Verteilung prüfen:

- [ ] Release-Build erfolgreich (`create_release.ps1` lief ohne Fehler)
- [ ] ZIP-Datei erstellt (`create_release_zip.ps1` lief ohne Fehler)
- [ ] **Test auf einem anderen PC** (wichtig!)
  - [ ] ZIP entpacken
  - [ ] StoDamageMeter.exe starten
  - [ ] Combat Log laden funktioniert
  - [ ] Statistiken werden korrekt angezeigt
- [ ] README.txt im ZIP vorhanden
- [ ] Version-Nummer korrekt (in README.txt und Dateinamen)
- [ ] Keine unnötigen Debug-Dateien (*.pdb sind automatisch entfernt)

---

## 🔧 Fehlerbehebung

### "Windows hat diesen PC geschützt" Warnung
**Ursache:** Windows SmartScreen warnt bei unbekannten Anwendungen  
**Lösung für Endnutzer:**
1. "Weitere Informationen" klicken
2. "Trotzdem ausführen" wählen

**Langfristige Lösung (optional):**
- Code-Signing-Zertifikat kaufen und Anwendung signieren
- Kostet ca. 300-500€/Jahr

### Anwendung startet nicht
**Mögliche Ursachen:**
1. Windows 7/8 (nicht unterstützt - nur Win 10/11)
2. 32-bit Windows (nicht unterstützt - nur 64-bit)
3. Antivirus blockiert die Anwendung

**Lösung:**
- Systemanforderungen prüfen
- Antivirus-Ausnahme hinzufügen

### Backend-Fehler
**Symptom:** "Backend not available" oder "Failed to execute backend"  
**Ursache:** OSCRBackend.exe fehlt oder ist blockiert  
**Lösung:**
1. Prüfen ob `OSCRBackend.exe` im gleichen Ordner wie `StoDamageMeter.exe` ist
2. Antivirus-Ausnahme für OSCRBackend.exe hinzufügen

---

## 📊 Release-Statistiken

**Aktuelles Release v1.1.0:**
- Entpackte Größe: 148.32 MB
- ZIP-Größe: 68.26 MB
- Dateien: 414
- Plattform: Windows x64
- .NET: Self-Contained (9.0)
- Python Backend: PyInstaller Bundle

**Enthaltene Features:**
- ✅ Combat-Statistiken mit DPS-Berechnung
- ✅ Player & Companion Abilities
- ✅ Space & Ground Combat-Erkennung
- ✅ Sortierbare Spalten
- ✅ Damage-Type-Icons (farbig)
- ✅ Attack-Counting
- ✅ UTF-8 Sonderzeichen-Support

---

## 💡 Tipps für die Verteilung

1. **Beschreibung schreiben:**
   ```
   STO Damage Meter v1.1.0
   
   Analysiere deine Star Trek Online Combat Logs!
   - DPS-Tracking
   - Companion-Statistiken
   - Space & Ground Support
   - Sortierbare Tabellen
   - Farbige Damage-Type-Icons
   
   Installation: ZIP entpacken → StoDamageMeter.exe starten → Fertig!
   Keine Installation erforderlich!
   
   Systemanforderungen: Windows 10/11 64-bit
   ```

2. **Screenshots hinzufügen:**
   - Mache ein paar Screenshots der Anwendung
   - Zeige die Combat-Liste und Statistiken-Tabelle
   - Füge sie zur Release-Beschreibung hinzu

3. **Version-History pflegen:**
   - Dokumentiere was sich geändert hat
   - Schreibe ein Changelog (siehe unten)

---

## 📝 Changelog-Beispiel

```markdown
## v1.1.0 (2025-10-10)

### Neue Features
- 🎨 Damage-Type-Icons mit Farbkodierung
- 📊 Attacks-Spalte (zeigt Ability-Verwendungen)
- 🏗️ Component-Refactoring für bessere Wartbarkeit
- 📏 Optimierte Spalten-Breiten

### Verbesserungen
- Größere Schriftgrößen für bessere Lesbarkeit
- Automatische Auswahl des ersten Combats
- Entfernung des Companion-Icons für cleanes Design
- Performance-Optimierungen

### Bugfixes
- Spalten-Alignment korrigiert
- UTF-8 Sonderzeichen werden korrekt angezeigt
- BOM-Handling für JSON-Parsing
```

---

## 🎉 Fertig!

Dein Release ist bereit zur Verteilung! 🚀

**Nächste Schritte:**
1. Teste das Release selbst noch einmal
2. Verteile die ZIP-Datei
3. Sammle Feedback von anderen Spielern
4. Behebe Bugs und erstelle v1.1.1 wenn nötig

**Support:**
- GitHub Issues: https://github.com/VanKhaos/StoDamageMeter/issues
- Fragen können dort gestellt werden

