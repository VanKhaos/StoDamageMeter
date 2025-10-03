# AI Assistant Regeln & Hinweise

## 🚫 Was ich NICHT tun soll

### Build & Deployment
- **NIEMALS** Build-Tests ausführen
- **NIEMALS** automatisch committen oder pushen ohne explizite Erlaubnis
- **NIEMALS** Git-Operationen ohne vorherige Nachfrage durchführen

### Code-Qualität
- **NIEMALS** ungetestete oder unsichere Code-Änderungen implementieren
- **NIEMALS** Breaking Changes ohne vorherige Absprache
- **NIEMALS** Dependencies ohne Begründung hinzufügen

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

### Nach Änderungen
1. **Funktionalität** testen
2. **Code-Qualität** prüfen
3. **Dokumentation** vervollständigen
4. **Benutzer** informieren

## 🚨 Wichtige Erinnerungen

- **Fragen stellen** ist besser als Annahmen zu treffen
- **Sicherheit** geht vor Funktionalität
- **Performance** ist wichtig für große Dateien
- **Benutzerfreundlichkeit** hat Priorität
- **Wartbarkeit** des Codes beachten

## 📚 Referenz-Dokumente

- `COMBATLOG_ANALYSIS.md` - Combatlog-Format Spezifikation
- `PROJEKT_ANALYSE.md` - Detaillierte Projektanalyse
- `StoDamageMeter.csproj` - Projekt-Konfiguration
- `Services/` - Business Logic
- `Models/` - Datenmodelle

---

**Letzte Aktualisierung**: $(Get-Date -Format "dd.MM.yyyy HH:mm")
**Version**: 1.0
