# AI Assistant Regeln & Hinweise

## 🚫 Was ich NICHT tun soll

### Build & Deployment
- **NIEMALS** automatisch die app starten, immer bescheid geben
- **NIEMALS** automatisch committen oder pushen ohne explizite Erlaubnis
- **NIEMALS** Git-Operationen ohne vorherige Nachfrage durchführen
- **NIEMALS** dotnet build bei einfachen Text-Änderungen ausführen

### Code-Qualität
- **NIEMALS** ungetestete oder unsichere Code-Änderungen implementieren
- **NIEMALS** Breaking Changes ohne vorherige Absprache
- **NIEMALS** Dependencies ohne Begründung hinzufügen
- **NIEMALS** Debug-Logs nach erfolgreichem Fixing stehen lassen

## ✅ Was ich tun soll

### Kommunikation
- **IMMER** auf Deutsch antworten
- **IMMER** Commit-Messages auf Englisch schreiben
- **IMMER** nachfragen bei unklaren Anforderungen
- **IMMER** erklären was ich mache und warum

### Code-Entwicklung
- **IMMER** bestehende Architektur respektieren
- **IMMER** MVVM-Pattern befolgen
- **IMMER** Dependency Injection verwenden
- **IMMER** asynchrone Programmierung bevorzugen
- **IMMER** Error Handling implementieren
- **IMMER** Debug-Logs nach erfolgreichem Feature-Fix entfernen
- **IMMER** dotnet build nur bei wichtigen Code-Änderungen ausführen

### Dokumentation
- **IMMER** detaillierte Kommentare schreiben
- **IMMER** Änderungen dokumentieren
- **IMMER** README/Anleitungen aktualisieren

## 🎯 Projekt-spezifische Regeln

### STO Damage Meter
- **Combatlog-Format** genau befolgen (siehe COMBATLOG_ANALYSIS.md)
- **WPF-UI Framework** verwenden für moderne UI
- **Performance** bei großen Dateien berücksichtigen
- **Live-Monitoring** Funktionalität erhalten
- **Deutsche UI-Texte** verwenden

### Code-Standards
- **C# 9.0+** Features nutzen
- **Nullable Reference Types** aktiviert
- **CommunityToolkit.Mvvm** für ViewModels
- **Microsoft.Extensions.Logging** für Logging
- **Newtonsoft.Json** für Serialisierung

## 🔧 Technische Präferenzen

### Architektur
- **SOLID-Prinzipien** befolgen
- **Interface-basierte** Abstraktion
- **Separation of Concerns**
- **Loose Coupling**

### Performance
- **Async/Await** für I/O-Operationen
- **LINQ** für Datenverarbeitung
- **Regex-Compilation** für bessere Performance
- **Memory Management** beachten

### UI/UX
- **Modern Web Design** (dunkles Theme)
- **Card-basierte** Layouts
- **Responsive Design**
- **Accessibility** berücksichtigen

## 📝 Workflow

### Vor Änderungen
1. **Aktuelle Implementierung** verstehen
2. **Auswirkungen** abschätzen
3. **Alternative Lösungen** überlegen
4. **Benutzer** informieren

### Während der Entwicklung
1. **Kleine, testbare** Schritte
2. **Zwischenergebnisse** zeigen
3. **Feedback** einholen
4. **Dokumentation** aktualisieren
5. **Debug-Logs** temporär hinzufügen für Troubleshooting

### Nach Änderungen
1. **Funktionalität** testen
2. **Code-Qualität** prüfen
3. **Debug-Logs** entfernen (nach erfolgreichem Fix)
4. **Build nur bei wichtigen Änderungen** (nicht bei Text-Änderungen)
5. **Dokumentation** vervollständigen
6. **Benutzer** informieren

## 🚨 Wichtige Erinnerungen

- **Fragen stellen** ist besser als Annahmen zu treffen
- **Sicherheit** geht vor Funktionalität
- **Performance** ist wichtig für große Dateien
- **Benutzerfreundlichkeit** hat Priorität
- **Wartbarkeit** des Codes beachten
- **Debug-Logs** sind temporär - nach Fixing immer entfernen
- **Build-Effizienz** - nur bei wichtigen Änderungen kompilieren

## 🔚 Session-Beendigung

**Wenn der User sagt "Wir beenden die Session" soll ich folgendes machen:**

1. **Projekt aufräumen** - nicht mehr benötigte Dateien löschen
2. **DEVELOPER_LOG.md aktualisieren** - was wir gemacht haben dokumentieren
3. **Alles committen** - mit aussagekräftiger Commit-Message
4. **In aktuellen Branch pushen** - NIEMALS direkt in main branch!
5. **Session beenden** - nach erfolgreichem Push

## 🛠️ Build & Deployment

### Debug-Version erstellen
- **Script:** `.\build_debug.ps1` im Root-Verzeichnis ausführen
- **Ziel:** Zentrale Debug-Version im `Debug\` Ordner
- **Vorteile:**
  - Alle Debug-Dateien an einem Ort
  - Debug-Symbole (.pdb) für besseres Debugging
  - Schneller Zugriff aus Root
  - Getrennt von Release-Builds
- **Struktur:**
  ```
  Debug\
  ├── StoDamageMeter.exe      (Frontend mit Debug-Symbolen)
  ├── StoDamageMeter.pdb      (Debug-Symbole)
  ├── OSCRBackend.exe         (Backend)
  ├── appsettings.json
  ├── *.dll                   (Dependencies)
  └── README.txt
  ```

### Release-Version erstellen
- **Script:** `.\create_release.ps1` für vollständigen Build
- **Script:** `.\create_release_zip.ps1` für ZIP-Archiv
- **Ziel:** `Releases\StoDamageMeter_vX.X.X\`
- **Struktur:** Launcher + App/ + Language/

## 📚 Referenz-Dokumente

- `COMBATLOG_ANALYSIS.md` - Combatlog-Format Spezifikation
- `PROJEKT_ANALYSE.md` - Detaillierte Projektanalyse
- `DEVELOPER_LOG.md` - Entwicklungs-Tagebuch
- `build_debug.ps1` - Debug-Build-Script
- `create_release.ps1` - Release-Build-Script
- `frontend/Build.targets` - MSBuild Custom Targets
- `Services/` - Business Logic
- `Models/` - Datenmodelle

---

**Letzte Aktualisierung**: 2025-01-10
**Version**: 1.1
