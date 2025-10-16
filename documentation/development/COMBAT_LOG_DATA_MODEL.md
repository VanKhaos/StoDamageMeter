# Combat Log Datenmodell - STO Damage Meter

## 📋 Übersicht

Dieses Dokument beschreibt das vollständige Datenmodell für eine geparste Combat-Log-Zeile aus Star Trek Online. Es zeigt, wie eine rohe Log-Zeile in strukturierte Daten umgewandelt wird und welche Informationen daraus extrahiert werden können.

## 🎯 Beispiel-Zeile

```
25:10:02:16:24:30.9::Van Khaos,P[12698228@19236104 Van Khaos@vankhaos#2007],Elite-Allianz-Jäger-Staffel,C[202 Space_Alliance_Carrier_Launch_Squadron_Fighters_3],Schwerer Mogai-Warbird,C[140 Space_Romulan_Escort],Antiprotonen-Impulskanone,Pn.Kydw9k,AntiProton,,106.049,964.176
```

## 🔍 Zeilen-Struktur

### **Format:**
```
YY:MM:DD:HH:MM:SS.ms::Owner,OwnerType,SourceName,SourceType,Target,TargetType,Ability,AbilityID,DamageType,Flags,Damage1,Damage2
```

### **Aufgeteilt:**
```
25:10:02:16:24:30.9::Van Khaos,P[12698228@19236104 Van Khaos@vankhaos#2007],Elite-Allianz-Jäger-Staffel,C[202 Space_Alliance_Carrier_Launch_Squadron_Fighters_3],Schwerer Mogai-Warbird,C[140 Space_Romulan_Escort],Antiprotonen-Impulskanone,Pn.Kydw9k,AntiProton,,106.049,964.176
```

| Position | Wert | Beschreibung |
|----------|------|--------------|
| **Timestamp** | `25:10:02:16:24:30.9` | Datum und Uhrzeit |
| **Owner** | `Van Khaos` | Besitzer der Aktion |
| **OwnerType** | `P[12698228@19236104 Van Khaos@vankhaos#2007]` | Player-ID mit Details |
| **SourceName** | `Elite-Allianz-Jäger-Staffel` | Ausführende Einheit |
| **SourceType** | `C[202 Space_Alliance_Carrier_Launch_Squadron_Fighters_3]` | Companion-ID |
| **Target** | `Schwerer Mogai-Warbird` | Ziel der Aktion |
| **TargetType** | `C[140 Space_Romulan_Escort]` | Ziel-Typ |
| **Ability** | `Antiprotonen-Impulskanone` | Verwendete Fähigkeit |
| **AbilityID** | `Pn.Kydw9k` | Interne Ability-ID |
| **DamageType** | `AntiProton` | Schadens-Typ |
| **Flags** | `` (leer) | Zusätzliche Flags |
| **Damage1** | `106.049` | Hauptschaden |
| **Damage2** | `964.176` | Zusätzlicher Schaden |

## 📊 Geparste Datenstruktur

### **Python Backend (working_oscr_backend.py)**

```python
parsed_line = {
    # Timestamp-Information
    'timestamp': datetime(2025, 10, 2, 16, 24, 30, 900000),  # 2025-10-02 16:24:30.9
    
    # Owner-Information (wer besitzt die Aktion)
    'owner_name': 'Van Khaos',
    'owner_type': 'P[12698228@19236104 Van Khaos@vankhaos#2007]',
    
    # Source-Information (wer führt die Aktion aus)
    'source_name': 'Elite-Allianz-Jäger-Staffel',
    'source_type': 'C[202 Space_Alliance_Carrier_Launch_Squadron_Fighters_3]',
    
    # Target-Information (wer ist das Ziel)
    'target_name': 'Schwerer Mogai-Warbird',
    'target_type': 'C[140 Space_Romulan_Escort]',
    
    # Ability-Information
    'ability_name': 'Antiprotonen-Impulskanone',
    'ability_id': 'Pn.Kydw9k',
    
    # Damage-Information
    'damage_type': 'AntiProton',
    'flags': '',  # Leer in diesem Beispiel
    'damage_values': ['106.049', '964.176']
}
```

### **C# Frontend-Modelle (OSCRModels.cs)**

```csharp
public class ParsedCombatLine
{
    public DateTime Timestamp { get; set; }              // 2025-10-02 16:24:30.9
    public string OwnerName { get; set; }                // "Van Khaos"
    public string OwnerType { get; set; }                // "P[12698228@19236104 Van Khaos@vankhaos#2007]"
    public string SourceName { get; set; }               // "Elite-Allianz-Jäger-Staffel"
    public string SourceType { get; set; }               // "C[202 Space_Alliance_Carrier_Launch_Squadron_Fighters_3]"
    public string TargetName { get; set; }               // "Schwerer Mogai-Warbird"
    public string TargetType { get; set; }               // "C[140 Space_Romulan_Escort]"
    public string AbilityName { get; set; }              // "Antiprotonen-Impulskanone"
    public string AbilityId { get; set; }                // "Pn.Kydw9k"
    public string DamageType { get; set; }               // "AntiProton"
    public string Flags { get; set; }                    // "" (leer)
    public List<double> DamageValues { get; set; }       // [106.049, 964.176]
}
```

## 🔍 Detaillierte Feld-Erklärungen

### **Timestamp**
- **Format:** `YY:MM:DD:HH:MM:SS.ms`
- **Beispiel:** `25:10:02:16:24:30.9`
- **Parsed:** `datetime(2025, 10, 2, 16, 24, 30, 900000)`
- **Bedeutung:** Exakter Zeitpunkt des Events

### **Owner vs Source**
- **Owner:** Der Spieler, dem die Aktion "gehört" (für DPS-Berechnung)
- **Source:** Wer die Aktion tatsächlich ausführt (kann Companion sein)
- **Beispiel:** 
  - Owner: `Van Khaos` (Spieler)
  - Source: `Elite-Allianz-Jäger-Staffel` (Companion des Spielers)

### **Typ-Identifikatoren**

#### **Player (P)**
- **Format:** `P[CharacterID@AccountID CharacterName@Handle#Discriminator]`
- **Beispiel:** `P[12698228@19236104 Van Khaos@vankhaos#2007]`
- **Aufbau:**
  - `12698228` = Character-ID
  - `19236104` = Account-ID
  - `Van Khaos` = Character-Name
  - `vankhaos#2007` = Discord-Handle

#### **Companion (C)**
- **Format:** `C[CompanionID Companion_Name]`
- **Beispiel:** `C[202 Space_Alliance_Carrier_Launch_Squadron_Fighters_3]`
- **Aufbau:**
  - `202` = Companion-ID
  - `Space_Alliance_Carrier_Launch_Squadron_Fighters_3` = Companion-Name

#### **NPC (N)**
- **Format:** `N[NPCID]`
- **Beispiel:** `N[789012]`

### **Damage Types**
- **AntiProton:** Antiprotonen-Schaden
- **Physical:** Physischer Schaden
- **Energy:** Energieschaden
- **Kinetic:** Kinetischer Schaden
- **Plasma:** Plasma-Schaden
- **Disruptor:** Disruptor-Schaden
- **Phaser:** Phaser-Schaden
- **Tetryon:** Tetryon-Schaden
- **Polaron:** Polaron-Schaden

### **Flags**
- **Kill:** Ziel wurde getötet
- **Crit:** Kritischer Treffer
- **Defeated:** Ziel wurde besiegt
- **Shield:** Schild-Treffer
- **Hull:** Rumpf-Treffer
- **Resist:** Schaden wurde reduziert
- **Immune:** Ziel war immun

### **Damage Values**
- **Erster Wert:** Hauptschaden (106.049)
- **Zweiter Wert:** Zusätzlicher Schaden (964.176)
- **Gesamtschaden:** 106.049 + 964.176 = 1.070.225

## 📈 Verarbeitung zu Statistiken

### **Player-Statistiken**
```csharp
public class PlayerStatistics
{
    public string Name { get; set; }                    // "Van Khaos"
    public double Dps { get; set; }                     // 125.450 DPS
    public double TotalDamage { get; set; }             // 15.054.000 Total
    public double MaxOneHit { get; set; }               // 485.220 Max Hit
    public double CritPercent { get; set; }             // 45.2% Crit Rate
    public double AccuracyPercent { get; set; }         // 92.5% Accuracy
    public List<AbilityStatistics> Abilities { get; set; }
    public List<CompanionStatistics> Companions { get; set; }
}
```

### **Companion-Statistiken**
```csharp
public class CompanionStatistics
{
    public string Name { get; set; }                    // "Elite-Allianz-Jäger-Staffel"
    public string Type { get; set; }                    // "AwayTeam", "KitModule", "TempControlled"
    public double Dps { get; set; }                     // 22.450 DPS
    public double TotalDamage { get; set; }             // 2.694.000
    public double MaxOneHit { get; set; }               // 156.890
    public double CritPercent { get; set; }             // 48.7%
    public double AccuracyPercent { get; set; }         // 93.4%
    public List<AbilityStatistics> Abilities { get; set; }
}
```

### **Ability-Statistiken**
```csharp
public class AbilityStatistics
{
    public string Name { get; set; }                    // "Antiprotonen-Impulskanone"
    public double TotalDamage { get; set; }             // 5.058.000
    public double Dps { get; set; }                     // 42.150 DPS
    public double MaxHit { get; set; }                  // 485.220
    public double CritPercent { get; set; }             // 52.3%
    public double AccuracyPercent { get; set; }         // 95.1%
    public int Attacks { get; set; }                    // 120 Angriffe
    public string DamageType { get; set; }              // "AntiProton"
}
```

## 🎯 Beispiel-Verarbeitung

### **Input-Zeile:**
```
25:10:02:16:24:30.9::Van Khaos,P[12698228@19236104 Van Khaos@vankhaos#2007],Elite-Allianz-Jäger-Staffel,C[202 Space_Alliance_Carrier_Launch_Squadron_Fighters_3],Schwerer Mogai-Warbird,C[140 Space_Romulan_Escort],Antiprotonen-Impulskanone,Pn.Kydw9k,AntiProton,,106.049,964.176
```

### **Parsed:**
```python
{
    'timestamp': datetime(2025, 10, 2, 16, 24, 30, 900000),
    'owner_name': 'Van Khaos',
    'owner_type': 'P[12698228@19236104 Van Khaos@vankhaos#2007]',
    'source_name': 'Elite-Allianz-Jäger-Staffel',
    'source_type': 'C[202 Space_Alliance_Carrier_Launch_Squadron_Fighters_3]',
    'target_name': 'Schwerer Mogai-Warbird',
    'target_type': 'C[140 Space_Romulan_Escort]',
    'ability_name': 'Antiprotonen-Impulskanone',
    'ability_id': 'Pn.Kydw9k',
    'damage_type': 'AntiProton',
    'flags': '',
    'damage_values': ['106.049', '964.176']
}
```

### **Aggregiert zu Statistiken:**

#### **Für Player "Van Khaos":**
- **Total Damage:** +1.070.225 (106.049 + 964.176)
- **Attack:** +1
- **Companion "Elite-Allianz-Jäger-Staffel":** +1.070.225 Damage

#### **Für Companion "Elite-Allianz-Jäger-Staffel":**
- **Total Damage:** +1.070.225
- **Attack:** +1
- **Ability "Antiprotonen-Impulskanone":** +1.070.225 Damage, +1 Attack

#### **Für Ability "Antiprotonen-Impulskanone":**
- **Total Damage:** +1.070.225
- **Attack:** +1
- **Damage Type:** AntiProton
- **Max Hit:** 1.070.225 (falls höher als bisheriger Max Hit)

## 🔧 Technische Details

### **Parsing-Logik (Python)**
```python
def parse_combat_log_line(self, line: str):
    """Parst eine Combat-Log-Zeile vollständig"""
    try:
        if '::' not in line:
            return None
        
        # Split bei ::
        parts = line.split('::', 1)
        timestamp_str = parts[0]
        timestamp = self.parse_timestamp(timestamp_str)
        
        # Rest der Zeile splitten
        data_parts = parts[1].split(',')
        
        if len(data_parts) < 7:
            return None
        
        return {
            'timestamp': timestamp,
            'owner_name': data_parts[0].strip(),
            'owner_type': data_parts[1].strip(),
            'source_name': data_parts[2].strip(),
            'source_type': data_parts[3].strip(),
            'target_name': data_parts[4].strip(),
            'target_type': data_parts[5].strip(),
            'ability_name': data_parts[6].strip(),
            'damage_type': data_parts[8].strip() if len(data_parts) > 8 else '',
            'flags': data_parts[9].strip() if len(data_parts) > 9 else '',
            'damage_values': data_parts[10:] if len(data_parts) > 10 else []
        }
    except Exception as e:
        return None
```

### **Timestamp-Parsing**
```python
def parse_timestamp(self, time_str):
    """Parst einen Timestamp aus dem Log-Format YY:MM:DD:HH:MM:SS.ms"""
    try:
        parts = time_str.split(':')
        if len(parts) >= 6:
            year = int(parts[0]) + 2000
            month = int(parts[1])
            day = int(parts[2])
            hour = int(parts[3])
            minute = int(parts[4])
            second = float(parts[5])
            return datetime(year, month, day, hour, minute, int(second), int((second % 1) * 1000000))
        return None
    except:
        return None
```

## 📚 Verwandte Dokumentation

- [TABLE_STRUCTURE.md](TABLE_STRUCTURE.md) - UI-Tabellen-Struktur
- [UI_IMPLEMENTATION_PLAN.md](UI_IMPLEMENTATION_PLAN.md) - UI-Implementierung
- [TROUBLESHOOTING.md](../troubleshooting/TROUBLESHOOTING.md) - Problembehandlung

## 🔄 Version-Historie

- **v1.0** (2025-01-14) - Erste Version mit Beispiel-Zeile
- **v1.1** (2025-01-14) - Erweiterte Feld-Erklärungen und C#-Modelle

---

**Erstellt:** 2025-01-14  
**Letzte Aktualisierung:** 2025-01-14  
**Maintainer:** STO Damage Meter Team
