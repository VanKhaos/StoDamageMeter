# Backend-System Wechsel: HTTP-API zurück zu stdin/stdout

## Frage
**Datum:** 2025-10-15  
**Kontext:** Nach der Implementierung des neuen HTTP-API Backends

> Könntest du denn jetzt einfach das Backend austauschen und zum Alten zurückkehren ohne viel zu ändern?

## Antwort

Ja, das ist sehr einfach! Der Wechsel zurück zum alten stdin/stdout System erfordert nur minimale Änderungen.

### Was geändert werden muss:

**1. In `frontend/Windows/App/App.xaml.cs`:**
```csharp
// HTTP Backend Manager - KOMMENTIERT
// services.AddSingleton<HttpBackendManager>();

// OSCR Backend Service - ZURÜCK ZUM ALTEN
services.AddSingleton<IOSCRBackendService, OSCRBackendService>();
```

**2. Das war's!** 

### Warum ist das so einfach?

- **Interface bleibt gleich:** `IOSCRBackendService` ist unverändert
- **Model-Klassen bleiben gleich:** Alle Response-Modelle sind identisch
- **Frontend-Code bleibt gleich:** Keine Änderungen in ViewModels oder Windows nötig
- **Dependency Injection:** Nur der Service wird ausgetauscht

### Was passiert dann:

1. **Frontend startet** → Verwendet `OSCRBackendService` (altes stdin/stdout)
2. **API-Calls** → Gehen über `OSCRBackend.exe --api` mit JSON über stdin/stdout
3. **Kein HTTP-Backend** → `HttpBackendManager` wird nicht verwendet
4. **Alte Logik** → Alles funktioniert wie vorher

### Vorteile des Wechsels:

- **Sofortiger Rollback** → Zurück zur bewährten Technologie
- **Keine neuen Abhängigkeiten** → Kein Flask, kein HTTP-Server
- **Einfacher** → Weniger bewegliche Teile
- **Bewährt** → Das alte System funktioniert bereits

### Nachteile:

- **stdin/stdout Probleme** → Können wieder auftreten
- **Weniger robust** → Keine HTTP-Status-Codes
- **Schwerer zu debuggen** → JSON über stdin/stdout

### Status
- **Implementiert:** HTTP-API Backend mit automatischem Start/Stop
- **Verfügbar:** Einfacher Wechsel zurück zum alten System
- **Entscheidung:** Noch nicht getroffen

### Nächste Schritte
- Testen des HTTP-API Systems
- Entscheidung zwischen HTTP-API und stdin/stdout
- Bei Bedarf: Wechsel zurück zum alten System

