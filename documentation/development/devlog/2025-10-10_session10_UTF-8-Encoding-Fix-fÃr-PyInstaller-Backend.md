## Session 10: UTF-8 Encoding-Fix für PyInstaller-Backend

**Datum:** 2025-10-10  
**Dauer:** ~1 Stunde  
**Fokus:** Sonderzeichen-Darstellung (ä, ö, ü) in PyInstaller-Executables

### 🎯 **Was wir erreicht haben:**

#### ✅ **Erfolgreich implementiert:**

1. **UTF-8 Encoding-Fix für PyInstaller**
   - **Problem:** PyInstaller-Executables ignorieren `PYTHONIOENCODING` Environment Variable
   - **Lösung:** Explizite UTF-8 Erzwingung direkt im Python-Code via `io.TextIOWrapper`
   - **Code:**
     ```python
     import io
     
     # Erzwinge UTF-8 für stdout/stderr (wichtig für PyInstaller .exe)
     if sys.stdout.encoding != 'utf-8':
         sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')
     if sys.stderr.encoding != 'utf-8':
         sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8', errors='replace')
     ```
   - **Resultat:** Sonderzeichen (ä, ö, ü, ß) werden korrekt angezeigt

2. **App stdin-Encoding optimiert**
   - **Problem:** `StandardInputEncoding = Encoding.UTF8` verhinderte Backend-Kommunikation
   - **Lösung:** JSON als UTF-8 Bytes direkt an `StandardInput.BaseStream` schreiben
   - **Code:**
     ```csharp
     var jsonBytes = Encoding.UTF8.GetBytes(jsonInput);
     await process.StandardInput.BaseStream.WriteAsync(jsonBytes, 0, jsonBytes.Length);
     await process.StandardInput.BaseStream.FlushAsync();
     process.StandardInput.Close();
     ```
   - **Vorteil:** Umgeht Encoding-Probleme bei stdin, sauber und zuverlässig

3. **Backend-Log ins logs/ Verzeichnis verschoben**
   - `oscr_backend.log` wird jetzt in `logs/` Unterverzeichnis geschrieben
   - Automatische Erstellung des `logs/` Ordners falls nicht vorhanden
   - Konsistenz mit App-Logs (`App_debug.log`, `backend_service_debug.log`, `backend_debug.log`)

### 🔧 **Technische Details:**

#### **PyInstaller UTF-8 Problem:**
- PyInstaller-Executables haben **kein** UTF-8 Default-Encoding
- `PYTHONIOENCODING` Environment Variable wird **ignoriert** in .exe
- Lösung: `io.TextIOWrapper` überschreibt `sys.stdout`/`sys.stderr` direkt

#### **Stdin/Stdout Encoding-Strategie:**
```
C# App → Python Backend:
  stdin:  UTF-8 Bytes direkt an BaseStream (umgeht StandardInputEncoding)
  stdout: PYTHONIOENCODING=utf-8 + io.TextIOWrapper (doppelte Absicherung)
```

### 🚨 **Gelöste Probleme:**

#### **Problem 1: Umlaute als "?" in PyInstaller .exe**
- **Symptom:** Sonderzeichen funktionierten im Script, aber nicht in der .exe
- **Ursache:** PyInstaller defaultet nicht auf UTF-8
- **Lösung:** Explizite UTF-8 Erzwingung mit `io.TextIOWrapper`
- **Resultat:** ✅ Funktioniert sowohl in Script als auch in .exe

#### **Problem 2: Backend-Kommunikation brach nach UTF-8 Änderungen ab**
- **Symptom:** "Expecting value: line 1 column 1 (char 0)" Fehler
- **Ursache:** `StandardInputEncoding = Encoding.UTF8` verursachte stdin-Probleme
- **Lösung:** Direkte Byte-Übertragung an `BaseStream`
- **Resultat:** ✅ Stabile Kommunikation + korrekte Zeichen

### 📁 **Wichtige Dateien:**

**Aktualisiert:**
- `backend/working_oscr_backend.py` - UTF-8 Erzwingung, logs/ Verzeichnis
- `app/Services/OSCRBackendService.cs` - BaseStream statt StandardInputEncoding

### 💡 **Lessons Learned:**

1. **PyInstaller Encoding:** Nie auf Default-Encoding verlassen, immer explizit setzen
2. **io.TextIOWrapper:** Zuverlässigste Methode für UTF-8 in Python Executables
3. **BaseStream vs. StandardInput:** BaseStream ist näher am OS, weniger Encoding-Probleme
4. **Environment Variables:** Nicht zuverlässig in PyInstaller-Executables
5. **UTF-8 BOM:** Immer strippen bei JSON-Parsing

### 🔄 **Build-Status:**

- ✅ Backend neu gebaut mit UTF-8 Fix
- ✅ App kompiliert erfolgreich
- ✅ Sonderzeichen funktionieren
- ✅ Backend-Kommunikation stabil
- ✅ Logs im logs/ Verzeichnis

### 🎨 **Qualität:**

**Vor dem Fix:**
- ❌ "Müller" → "M?ller"
- ❌ "Phaser-Strahl" → "Phaser-Strahl?"
- ❌ Backend-Logs verstreut

**Nach dem Fix:**
- ✅ "Müller" → "Müller"
- ✅ "Phaser-Strahl" → "Phaser-Strahl"
- ✅ Alle Logs in logs/ Verzeichnis

---
**Nächste Session:** DPS-Graph implementieren, Filter-Funktionalität aktivieren


