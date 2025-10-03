
# STO Combatlog Format-Analyse

## Beispiel-Zeile
```
25:10:02:16:24:30.9::Van Khaos,P[12698228@19236104 Van Khaos@vankhaos#2007],Elite-Allianz-Jäger-Staffel,C[202 Space_Alliance_Carrier_Launch_Squadron_Fighters_3],Schwerer Mogai-Warbird,C[140 Space_Romulan_Escort],Antiprotonen-Impulskanone,Pn.Kydw9k,AntiProton,,106.049,964.176
```

## Datenstruktur

Das Combatlog folgt einem festen Schema mit 12 Feldern, getrennt durch Kommas:

| Position | Feld | Beschreibung | Beispiel |
|----------|------|--------------|----------|
| 0 | Spielername | Name des Spielers | `Van Khaos` |
| 1 | Player-Tag | Spieler-Identifikation | `P[12698228@19236104 Van Khaos@vankhaos#2007]` |
| 2 | Entity-Details | Quell-Entity Name | `Elite-Allianz-Jäger-Staffel` |
| 3 | Entity-Tag | Quell-Entity Identifikation | `C[202 Space_Alliance_Carrier_Launch_Squadron_Fighters_3]` |
| 4 | Gegner-Name | Ziel-Entity Name | `Schwerer Mogai-Warbird` |
| 5 | Gegner-Tag | Ziel-Entity Identifikation | `C[140 Space_Romulan_Escort]` |
| 6 | Angriff | Waffe/Fähigkeit | `Antiprotonen-Impulskanone` |
| 7 | Ability-ID | Interne Fähigkeits-ID | `Pn.Kydw9k` |
| 8 | Schadensart | Typ des Schadens | `AntiProton` |
| 9 | Event-Typ | Spezielle Ereignisse | *(leer = Normal)* |
| 10 | Roher Schaden | Schaden vor Resistenzen | `106.049` |
| 11 | Effektiver Schaden | Schaden nach Resistenzen | `964.176` |

## Entity-Klassifizierung

### Player-Tags (parts[1])
- **P[ID@AccountID Name@Handle#Discriminator]** ✅ **Echte Spieler**
  - Nur diese werden für Statistiken verwendet
  - Beispiel: `P[12698228@19236104 Van Khaos@vankhaos#2007]`

### Entity-Tags (parts[3] & parts[5])
- **C[ID EntityName]** ❌ **Gegner/Entities**
  - Normale Gegner und NPCs
  - Beispiel: `C[140 Space_Romulan_Escort]`

- **S[ID]** ❌ **Companions/Begleiter**
  - Spieler-Begleiter und Pets

## Kampfumgebung-Erkennung

### parts[3] - Schadensquelle
- **Ground_** = Kitmodul (Bodenkampf-Fähigkeiten)
- **Space_** = Hangarschiffe (Raumkampf-Fähigkeiten)

### parts[5] - Kampfumgebung
- **Ground_** = Bodenkampf
- **Space_** = Raumkampf

## Event-Typen (parts[9])

| Wert | Bedeutung |
|------|-----------|
| *(leer)* | Normaler Treffer |
| `Critical` | Kritischer Treffer |
| `DoT` | Damage over Time |
| `Miss` | Verfehlt |
| `Immune` | Immun |

## Schadensarten (parts[8])

Häufige Schadensarten in STO:
- `AntiProton` - Antiprotonen
- `Fire` - Feuer
- `Cold` - Kälte
- `Electrical` - Elektrisch
- `Kinetic` - Kinetisch
- `Plasma` - Plasma
- `Phaser` - Phaser
- `Disruptor` - Disruptor

## Wichtige Hinweise

### Leere Felder
- **parts[2]** und **parts[4]** können leer sein
- Die Anwendung nimmt die Daten so wie sie kommen
- Keine spezielle Behandlung für leere Felder erforderlich

### Datenvalidierung
- **Timestamps**: Werden als korrekt angenommen
- **Kommas**: Alle erforderlichen Kommas sind vorhanden
- **Format**: Keine zusätzliche Validierung implementiert

### Verarbeitungslogik
- Nur **positive Schadenswerte** werden verarbeitet (keine Heilung)
- **P-Tags** sind entscheidend für Spieler-Identifikation
- **parts[5]** bestimmt die Kampfumgebung (Ground_ vs. Space_)