## Session 10: UTF-8 Encoding-Fix fÃ¼r PyInstaller-Backend

**Datum:** 2025-10-10  
**Dauer:** ~1 Stunde  
**Fokus:** Sonderzeichen-Darstellung (Ã¤, Ã¶, Ã¼) in PyInstaller-Executables

### ðŸŽ¯ **Was wir erreicht haben:**

#### âœ… **Erfolgreich implementiert:**

1. **UTF-8 Encoding-Fix fÃ¼r PyInstaller**
   - **Problem:** PyInstaller-Executables ignorieren `PYTHONIOENCODING` Environment Variable
   - **LÃ¶sung:** Explizite UTF-8 Erzwingung direkt im Python-Code via `io.TextIOWrapper`
   - **Code:**
     ```python
     import io
     
     # Erzwinge UTF-8 fÃ¼r stdout/stderr (wichtig fÃ¼r PyInstaller .exe)
     if sys.stdout.encoding != 'utf-8':
         sys.stdout = io.TextIOWrapper(sys.stdout.buffer, encoding='utf-8', errors='replace')
     if sys.stderr.encoding != 'utf-8':
         sys.stderr = io.TextIOWrapper(sys.stderr.buffer, encoding='utf-8', errors='replace')
     ```
   - **Resultat:** Sonderzeichen (Ã¤, Ã¶, Ã¼, ÃŸ) werden korrekt angezeigt

2. **Frontend stdin-Encoding optimiert**
   - **Problem:** `StandardInputEncoding = Encoding.UTF8` verhinderte Backend-Kommunikation
   - **LÃ¶sung:** JSON als UTF-8 Bytes direkt an `StandardInput.BaseStream` schreiben
   - **Code:**
     ```csharp
     var jsonBytes = Encoding.UTF8.GetBytes(jsonInput);
     await process.StandardInput.BaseStream.WriteAsync(jsonBytes, 0, jsonBytes.Length);
     await process.StandardInput.BaseStream.FlushAsync();
     process.StandardInput.Close();
     ```
   - **Vorteil:** Umgeht Encoding-Probleme bei stdin, sauber und zuverlÃ¤ssig

3. **Backend-Log ins logs/ Verzeichnis verschoben**
   - `oscr_backend.log` wird jetzt in `logs/` Unterverzeichnis geschrieben
   - Automatische Erstellung des `logs/` Ordners falls nicht vorhanden
   - Konsistenz mit Frontend-Logs (`frontend_debug.log`, `backend_service_debug.log`, `backend_debug.log`)

### ðŸ”§ **Technische Details:**

#### **PyInstaller UTF-8 Problem:**
- PyInstaller-Executables haben **kein** UTF-8 Default-Encoding
- `PYTHONIOENCODING` Environment Variable wird **ignoriert** in .exe
- LÃ¶sung: `io.TextIOWrapper` Ã¼berschreibt `sys.stdout`/`sys.stderr` direkt

#### **Stdin/Stdout Encoding-Strategie:**
```
C# Frontend â†’ Python Backend:
  stdin:  UTF-8 Bytes direkt an BaseStream (umgeht StandardInputEncoding)
  stdout: PYTHONIOENCODING=utf-8 + io.TextIOWrapper (doppelte Absicherung)
```

### ðŸš¨ **GelÃ¶ste Probleme:**

#### **Problem 1: Umlaute als "?" in PyInstaller .exe**
- **Symptom:** Sonderzeichen funktionierten im Script, aber nicht in der .exe
- **Ursache:** PyInstaller defaultet nicht auf UTF-8
- **LÃ¶sung:** Explizite UTF-8 Erzwingung mit `io.TextIOWrapper`
- **Resultat:** âœ… Funktioniert sowohl in Script als auch in .exe

#### **Problem 2: Backend-Kommunikation brach nach UTF-8 Ã„nderungen ab**
- **Symptom:** "Expecting value: line 1 column 1 (char 0)" Fehler
- **Ursache:** `StandardInputEncoding = Encoding.UTF8` verursachte stdin-Probleme
- **LÃ¶sung:** Direkte Byte-Ãœbertragung an `BaseStream`
- **Resultat:** âœ… Stabile Kommunikation + korrekte Zeichen

### ðŸ“ **Wichtige Dateien:**

**Aktualisiert:**
- `backend/working_oscr_backend.py` - UTF-8 Erzwingung, logs/ Verzeichnis
- `frontend/Services/OSCRBackendService.cs` - BaseStream statt StandardInputEncoding

### ðŸ’¡ **Lessons Learned:**

1. **PyInstaller Encoding:** Nie auf Default-Encoding verlassen, immer explizit setzen
2. **io.TextIOWrapper:** ZuverlÃ¤ssigste Methode fÃ¼r UTF-8 in Python Executables
3. **BaseStream vs. StandardInput:** BaseStream ist nÃ¤her am OS, weniger Encoding-Probleme
4. **Environment Variables:** Nicht zuverlÃ¤ssig in PyInstaller-Executables
5. **UTF-8 BOM:** Immer strippen bei JSON-Parsing

### ðŸ”„ **Build-Status:**

- âœ… Backend neu gebaut mit UTF-8 Fix
- âœ… Frontend kompiliert erfolgreich
- âœ… Sonderzeichen funktionieren
- âœ… Backend-Kommunikation stabil
- âœ… Logs im logs/ Verzeichnis

### ðŸŽ¨ **QualitÃ¤t:**

**Vor dem Fix:**
- âŒ "MÃ¼ller" â†’ "M?ller"
- âŒ "Phaser-Strahl" â†’ "Phaser-Strahl?"
- âŒ Backend-Logs verstreut

**Nach dem Fix:**
- âœ… "MÃ¼ller" â†’ "MÃ¼ller"
- âœ… "Phaser-Strahl" â†’ "Phaser-Strahl"
- âœ… Alle Logs in logs/ Verzeichnis

---
**NÃ¤chste Session:** DPS-Graph implementieren, Filter-FunktionalitÃ¤t aktivieren


