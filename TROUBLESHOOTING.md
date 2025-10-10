# 🔧 STO Damage Meter - Troubleshooting Guide

## 📋 Inhalt

1. [Log-Dateien finden](#log-dateien-finden)
2. [Häufige Fehler](#häufige-fehler)
3. [Backend-Fehler](#backend-fehler)
4. [Combat Log Probleme](#combat-log-probleme)
5. [Windows Defender Warnung](#windows-defender-warnung)

---

## 📁 Log-Dateien finden

Die Anwendung erstellt automatisch Log-Dateien für die Fehlersuche:

### **Speicherort:**
```
Im gleichen Ordner wie StoDamageMeter.exe:
├── StoDamageMeter.exe
├── oscr_backend.log      ← Backend-Fehler (Python)
└── frontend_debug.log    ← Frontend-Fehler (C#)
```

### **Log-Dateien öffnen:**
1. Navigiere zum Ordner wo `StoDamageMeter.exe` liegt
2. Öffne `oscr_backend.log` mit Notepad
3. Öffne `frontend_debug.log` mit Notepad
4. Scrolle nach unten (neueste Einträge sind am Ende)

---

## 🐛 Häufige Fehler

### **Problem: "Die Combat-Log-Datei konnte nicht gelesen werden"**

**Mögliche Ursachen:**
1. **Falscher Pfad**
   - Lösung: Überprüfe den Pfad zur `combatlog.log`
   - Standard-Pfad: `C:\Program Files (x86)\Star Trek Online\Star Trek Online\Live\logs\GameClient\combatlog.log`

2. **Datei ist leer oder nicht vorhanden**
   - Lösung: Starte STO und aktiviere Combat-Logging im Spiel (`/combatlog 1`)

3. **Datei wird von STO gesperrt**
   - Lösung: Schließe Star Trek Online bevor du die Log-Datei lädst

4. **Keine Leserechte**
   - Lösung: Führe die Anwendung als Administrator aus (Rechtsklick → "Als Administrator ausführen")

**Debug-Schritte:**
1. Öffne `oscr_backend.log`
2. Suche nach der letzten Fehlermeldung (am Ende der Datei)
3. Häufige Fehler:
   - `FileNotFoundError` → Datei nicht gefunden
   - `PermissionError` → Keine Leserechte
   - `UnicodeDecodeError` → Encoding-Problem

---

### **Problem: "Backend not available"**

**Ursache:** `OSCRBackend.exe` kann nicht gestartet werden

**Lösungen:**
1. **Prüfe ob Backend vorhanden ist:**
   - `OSCRBackend.exe` muss im gleichen Ordner wie `StoDamageMeter.exe` sein
   - Falls fehlend: Release neu entpacken

2. **Antivirus blockiert die .exe:**
   - Füge eine Ausnahme für `OSCRBackend.exe` hinzu
   - Temporär Antivirus deaktivieren zum Testen

3. **Windows Defender blockiert:**
   - Siehe [Windows Defender Warnung](#windows-defender-warnung)

---

### **Problem: Anwendung startet nicht**

**Mögliche Ursachen:**
1. **Windows-Version zu alt**
   - Erforderlich: Windows 10 (64-bit) oder neuer
   - Lösung: Upgrade auf Windows 10/11

2. **32-bit Windows**
   - Die Anwendung ist nur für 64-bit Windows
   - Lösung: Upgrade auf 64-bit Windows (neu installieren)

3. **Fehlende Dateien**
   - Lösung: ZIP-Datei komplett neu entpacken
   - Alle DLL-Dateien müssen vorhanden sein

---

## 🐍 Backend-Fehler

### **Backend-Log analysieren (`oscr_backend.log`):**

**Wichtige Log-Einträge:**
```
=== OSCR Backend Started ===
Log file: C:\...\oscr_backend.log
```
→ Backend hat erfolgreich gestartet

```
Error isolating combats: ...
```
→ Problem beim Erkennen von Combats

```
Error analyzing players: ...
```
→ Problem beim Parsen der Log-Zeilen

### **Häufige Backend-Fehler:**

**1. UTF-8 Encoding-Probleme**
```
UnicodeDecodeError: 'utf-8' codec can't decode...
```
**Lösung:**
- Combat-Log ist vermutlich beschädigt
- Lösche die alte `combatlog.log` in STO
- Starte STO neu, damit eine neue Log-Datei erstellt wird

**2. JSON-Parse-Fehler**
```
JSONDecodeError: ...
```
**Ursache:** Kommunikations-Problem zwischen Frontend und Backend  
**Lösung:**
- Schließe die Anwendung komplett
- Lösche `oscr_backend.log`
- Starte die Anwendung neu

**3. Memory-Fehler**
```
MemoryError: ...
```
**Ursache:** Combat-Log zu groß (>1 GB)  
**Lösung:**
1. Lösche die alte `combatlog.log` in STO
2. Oder: Kopiere nur die letzten 100 MB der Datei

---

## 📝 Combat Log Probleme

### **Keine Combats werden erkannt**

**Debug-Schritte:**
1. Öffne `combatlog.log` mit Notepad
2. Prüfe ob Zeilen vorhanden sind
3. Suche nach Zeilen mit Damage-Events (z.B. "Photon Torpedo")

**Mögliche Ursachen:**
1. **Combat-Logging nicht aktiviert**
   - Lösung: In STO `/combatlog 1` eingeben

2. **Zu wenig Zeilen**
   - Minimum: 20 Zeilen pro Combat
   - Lösung: Mehr Kämpfe im Spiel machen

3. **Nur Heilung, kein Damage**
   - Die Anwendung erkennt nur Damage-Events
   - Lösung: Mache Damage im Spiel

---

### **Falsche Combat-Erkennung**

**Symptom:** Combats sind zu kurz oder zu lang

**Ursache:** Zeit zwischen Combats falsch konfiguriert

**Lösung:**
1. Öffne `appsettings.json` im Anwendungsordner
2. Ändere `"SecondsBetweenCombats": 30` auf einen höheren Wert (z.B. 60)
3. Speichern und Anwendung neu starten

---

## 🛡️ Windows Defender Warnung

### **"Windows hat diesen PC geschützt"**

**Das ist normal!** Windows SmartScreen warnt bei allen unbekannten Anwendungen.

**Lösung:**
1. Klicke auf "Weitere Informationen"
2. Klicke auf "Trotzdem ausführen"

**Warum passiert das?**
- Die Anwendung ist neu und Windows kennt sie noch nicht
- Nach mehreren Downloads lernt Windows die Anwendung kennen
- Alternativ: Code-Signing-Zertifikat (~300€/Jahr) kaufen

**Ist die Anwendung sicher?**
- Ja! Der komplette Quellcode ist auf GitHub öffentlich
- Keine Malware, keine Spyware
- Nur Daten aus der lokalen Combat-Log-Datei werden gelesen

---

## 📧 Weitere Hilfe

### **Problem nicht gelöst?**

1. **Log-Dateien sammeln:**
   - `oscr_backend.log`
   - `frontend_debug.log`

2. **GitHub Issue erstellen:**
   - https://github.com/VanKhaos/StoDamageMeter/issues
   - Log-Dateien anhängen (als .txt oder .zip)
   - Beschreibe genau was du gemacht hast
   - Füge Screenshots hinzu

3. **Informationen angeben:**
   - Windows-Version (z.B. Windows 11 64-bit)
   - STO-Version (Steam oder Arc)
   - Fehlermeldung (genauer Text)

---

## 🔍 Debug-Modus

### **Erweiterte Fehlersuche:**

**Frontend-Log aktivieren:**
1. Die Datei `frontend_debug.log` wird automatisch erstellt
2. Enthält alle Frontend-Aktivitäten
3. Zeigt Backend-Kommunikation

**Backend-Log lesen:**
1. `oscr_backend.log` wird automatisch erstellt
2. Enthält alle Backend-Aktivitäten
3. Zeigt Combat-Parsing-Details

### **Log-Dateien teilen:**
```
Wichtig: Log-Dateien können Spieler-Namen enthalten!
Falls du das nicht möchtest, anonymisiere die Namen vor dem Teilen.
```

---

## ✅ Checkliste bei Problemen

- [ ] Beide Log-Dateien vorhanden? (`oscr_backend.log`, `frontend_debug.log`)
- [ ] Combat-Logging in STO aktiviert? (`/combatlog 1`)
- [ ] Richtiger Pfad zur `combatlog.log`?
- [ ] Alle Dateien aus der ZIP entpackt?
- [ ] `OSCRBackend.exe` im gleichen Ordner?
- [ ] Windows 10/11 64-bit?
- [ ] Antivirus deaktiviert zum Testen?
- [ ] Als Administrator gestartet?
- [ ] Combat-Log-Datei nicht leer?
- [ ] STO geschlossen beim Laden der Log-Datei?

---

**Bei weiteren Fragen:** https://github.com/VanKhaos/StoDamageMeter/issues 🚀

