# Devlog: Update-Benachrichtigungssystem
**Datum:** 14. Oktober 2025  
**Session:** 3  
**Feature:** Update-Benachrichtigungssystem mit visueller Indikation

## 🎯 **Ziel**
Implementierung eines automatischen Update-Benachrichtigungssystems, das:
- Beim App-Start automatisch nach Updates sucht
- Visuelle Benachrichtigung über Logo-Wechsel (grün = Update verfügbar)
- Context-Menü Badge für Update-Information
- Direkter Link zu GitHub Releases

## 🚀 **Implementierte Features**

### **1. UpdateCheckService**
- **GitHub Releases API Integration**
  - Abfrage der neuesten Version von `https://api.github.com/repos/VanKhaos/StoDamageMeter/releases/latest`
  - Semantische Versionsvergleich (SemVer)
  - Timeout von 10 Sekunden für Netzwerk-Requests

- **Datenmodelle**
  ```csharp
  public class GitHubRelease
  {
      public string TagName { get; set; }
      public string Name { get; set; }
      public string HtmlUrl { get; set; }
      public DateTime PublishedAt { get; set; }
      public string Body { get; set; }
  }

  public class UpdateInfo
  {
      public bool UpdateAvailable { get; set; }
      public string LatestVersion { get; set; }
      public string ReleaseUrl { get; set; }
      public string ReleaseNotes { get; set; }
      public DateTime PublishedAt { get; set; }
  }
  ```

### **2. Visuelle Update-Benachrichtigung**
- **Logo-Wechsel System**
  - Standard: Weißes Logo (`app_icon.png`)
  - Update verfügbar: Grünes Logo (`DPS_Meter_Logo_Green.png`)
  - Dynamische Änderung über `BitmapImage` und Resource-URIs

- **Context-Menü Integration**
  - Update-Badge: "🔔 Update Available - v2.1.0"
  - Grüne Farbe (`#4CAF50`) für Update-Benachrichtigung
  - Automatisches Ein-/Ausblenden basierend auf Update-Status

### **3. Automatische Update-Prüfung**
- **Startup-Check**
  - Fire-and-forget beim `Loaded` Event
  - Non-blocking, keine UI-Blockierung
  - Silent fail bei Netzwerk-Problemen

- **Dependency Injection**
  - `UpdateCheckService` als Singleton registriert
  - Verfügbar über `App.ServiceProvider`

## 🔧 **Technische Details**

### **Resource-Management**
```xml
<!-- App.csproj -->
<Resource Include="Assets\DPS_Meter_Logo_Green.png" />
```

```csharp
// Dynamischer Logo-Wechsel
LogoImage.Source = new BitmapImage(new Uri("pack://application:,,,/Assets/DPS_Meter_Logo_Green.png"));
```

### **Versionsvergleich**
```csharp
private bool IsNewerVersion(Version latest, Version current)
{
    return latest > current;
}

public Version GetCurrentVersion()
{
    // Hardcoded für Stabilität
    return new Version(2, 0, 0, 0);
}
```

### **Error Handling**
- Silent fail bei Netzwerk-Problemen
- Timeout-Schutz (10 Sekunden)
- Graceful degradation bei API-Fehlern

## 🎨 **UI/UX Verbesserungen**

### **Entfernte Features**
- ❌ Manueller "Check for Updates" Button (zu aufdringlich)
- ✅ Nur noch automatische Benachrichtigung bei verfügbaren Updates

### **Visuelle Indikatoren**
- 🟢 **Grünes Logo** = Update verfügbar
- ⚪ **Weißes Logo** = Aktuelle Version
- 🔔 **Context-Menü Badge** = Zusätzliche Information

## 🧪 **Test-Modus**
- **Simulation verfügbarer Updates**
  - Hardcoded Test-Daten für Entwicklung
  - 1 Sekunde Netzwerk-Delay Simulation
  - Einfache Aktivierung/Deaktivierung über `TEST_MODE` Flag

## 📁 **Datei-Änderungen**

### **Neue Dateien**
- `app/Services/UpdateCheckService.cs` - Update-Check Service
- `app/Assets/DPS_Meter_Logo_Green.png` - Grünes Logo für Updates

### **Geänderte Dateien**
- `app/Windows/App/App.xaml.cs` - Service Registration
- `app/Windows/LandingWindow/LandingWindow.xaml` - Context-Menü Update
- `app/Windows/LandingWindow/LandingWindow.xaml.cs` - Update-Logic
- `app/App.csproj` - Resource-Definition

## 🔄 **Workflow**
1. **App-Start** → Automatischer Update-Check
2. **Update verfügbar** → Grünes Logo + Context-Menü Badge
3. **User klickt Badge** → GitHub Releases öffnet sich im Browser
4. **Kein Update** → Normales Logo, kein Badge

## 🎯 **Ergebnis**
- ✅ Automatische Update-Benachrichtigung
- ✅ Visuelle Indikation über Logo-Wechsel
- ✅ Non-intrusive UX (kein manueller Check-Button)
- ✅ Direkter Link zu GitHub Releases
- ✅ Robuste Error-Behandlung
- ✅ Test-Modus für Entwicklung

## 🚀 **Nächste Schritte**
- Test-Modus für Produktion deaktivieren
- Integration in Release-Pipeline
- Monitoring der Update-Check Erfolgsrate
