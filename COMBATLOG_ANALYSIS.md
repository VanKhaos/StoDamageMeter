# Combatlog.log Struktur-Analyse

## 📊 Datei-Übersicht
- **Format:** CSV-ähnlich mit Doppelpunkt-Trennern
- **Struktur:** Jede Zeile hat 11 Positionen (durch Kommas getrennt)

## 🔍 Zeilen-Struktur
```
Position 1: DD:MM:YY:HH:MM:SS.mmm # Timestamp auf Millisekunde genau
Position 2: :: # Separator
Position 3: Spielername,P[CharID@AccountID Spielername@Handle#Discriminator],
Position 4: EntityDetails,C[EntityID EntityName], # S Tag = Companion vom Spieler, C Tag = Entweder Hanger Schiffe oder Kitmodul, je nach dem was im EntityName Ground_ für Kitmodul oder Space_ für Hangerschiffe
Position 5: ZielEntity,C[EntityID EntityName], # Ist immer der Gegner, im EntityName steht entweder Ground_ für Bodenkamp oder Space_ für Raumkampf, das ist auch immer der Feind.
Position 6: Attack, # Das ist die Fähigkeit oder Waffe mit der Schaden verursacht wird. Ob es eine Waffe oder Fähigkeit ist spielt aber keine Rolle
Position 7: Pn.AbilityID, # Uninteressant
Position 8: Schadensart, # Korrekt das ist die Schadensart, ich kenne nicht alle Schadensarten, deswegen darf das nicht hardcoded sein sondern muss immer mit gelesen werden
Position 9: EventTyp, # hier interessiert uns nur ob es ein Crit oder Miss war, Immune wird wie Miss behandelt, wenn immune oder miss dann ist der schaden und der schaden mit resitenz uninteressant
Position 10: Schaden, # Der Schaden der am Feind verursacht wird.
Position 11: SchadenMitResistenz # Das ist der Schaden mit den Resistenzen vom Geggner, der kann höher sein als der Schaden oder niedriger
das trennzeichen ist komma
```

## 📋 Positionen-Analyse

### Zeile 1 - Lava-Boden:
```
25:10:02:16:06:04.2::Van Khaos,P[12698228@19236104 Van Khaos@vankhaos#2007],Lava-Boden,C[558 Ground_Universal_Kit_Summer_Lava_Floor],Sehlat,C[554 Beast_Sehlat_Ensign],Lava-Boden,Pn.Zeev3q,Fire,,94.5419,0
```

**Positionen-Aufschlüsselung:**
- **Pos 1:** `25:10:02:16:06:04.2` = Timestamp auf Millisekunde genau
- **Pos 2:** `::` = Separator
- **Pos 3:** `Van Khaos,P[12698228@19236104 Van Khaos@vankhaos#2007]` = Spielername + Player-ID, erkennbar am P Tag
- **Pos 4:** `Lava-Boden,C[558 Ground_Universal_Kit_Summer_Lava_Floor]` = EntityDetails + C[ID] (Ground_ = Kitmodul)
- **Pos 5:** `Sehlat,C[554 Beast_Sehlat_Ensign]` = ZielEntity + C[ID] (Ground_ = Bodenkampf)
- **Pos 6:** `Lava-Boden` = Attack (Waffe/Fähigkeit)
- **Pos 7:** `Pn.Zeev3q` = AbilityID (uninteressant)
- **Pos 8:** `Fire` = Schadensart
- **Pos 9:** `` (leer) = EventTyp (Normal)
- **Pos 10:** `94.5419` = Schaden
- **Pos 11:** `0` = SchadenMitResistenz

### Zeile 2 - Antiprotonenstrahlenbank:
```
25:10:02:16:23:35.2::Van Khaos,P[12698228@19236104 Van Khaos@vankhaos#2007],Elite-Allianz-Jäger-Staffel,C[171 Space_Alliance_Carrier_Launch_Squadron_Fighters_3],Leichter Plasmakanonen-Geschützturm,C[143 Space_Turret_Light_Plasma_Cannon_Satellite],Antiprotonenstrahlenbank,Pn.Zkj7wd,AntiProton,,319.391,0
```

**Positionen-Aufschlüsselung:**
- **Pos 1:** `25:10:02:16:23:35.2` = Timestamp auf Millisekunde genau
- **Pos 2:** `::` = Separator
- **Pos 3:** `Van Khaos,P[12698228@19236104 Van Khaos@vankhaos#2007]` = Spielername + Player-ID
- **Pos 4:** `Elite-Allianz-Jäger-Staffel,C[171 Space_Alliance_Carrier_Launch_Squadron_Fighters_3]` = EntityDetails + C[ID] (Space_ = Hangerschiffe)
- **Pos 5:** `Leichter Plasmakanonen-Geschützturm,C[143 Space_Turret_Light_Plasma_Cannon_Satellite]` = ZielEntity + C[ID] (Space_ = Raumkampf)
- **Pos 6:** `Antiprotonenstrahlenbank` = Attack (Waffe/Fähigkeit)
- **Pos 7:** `Pn.Zkj7wd` = AbilityID (uninteressant)
- **Pos 8:** `AntiProton` = Schadensart
- **Pos 9:** `` (leer) = EventTyp (Normal)
- **Pos 10:** `319.391` = Schaden
- **Pos 11:** `0` = SchadenMitResistenz

### Zeile 3 - DoT mit S[ID]:
```
25:10:02:17:09:39.0::Van Khaos,P[12698228@19236104 Van Khaos@vankhaos#2007],Sillar,S[139588389],Gil,C[26 Ground_Cardassian_Ens_Range],Blitz der Kalten Fusion I,Pn.3hhwrm,Cold,DoT,49.7942,0
```

**Positionen-Aufschlüsselung:**
- **Pos 1:** `25:10:02:17:09:39.0` = Timestamp auf Millisekunde genau
- **Pos 2:** `::` = Separator
- **Pos 3:** `Van Khaos,P[12698228@19236104 Van Khaos@vankhaos#2007]` = Spielername + Player-ID
- **Pos 4:** `Sillar,S[139588389]` = EntityDetails + S[ID] (Companion vom Spieler)
- **Pos 5:** `Gil,C[26 Ground_Cardassian_Ens_Range]` = ZielEntity + C[ID] (Ground_ = Bodenkampf)
- **Pos 6:** `Blitz der Kalten Fusion I` = Attack (Waffe/Fähigkeit)
- **Pos 7:** `Pn.3hhwrm` = AbilityID (uninteressant)
- **Pos 8:** `Cold` = Schadensart
- **Pos 9:** `DoT` = EventTyp
- **Pos 10:** `49.7942` = Schaden
- **Pos 11:** `0` = SchadenMitResistenz

### Zeile 4 - Schildgenerator:
```
25:10:02:17:09:38.7::Van Khaos,P[12698228@19236104 Van Khaos@vankhaos#2007],Schildgenerator I,C[41 Ground_Engineering_Kit_Shield_Generator_1_Klg_R9],Marschall Janeway,S[140096978],Schildgenerator-Energiematrix I,Pn.Q9ejka,Shield,,-11.6073,0
```

**Positionen-Aufschlüsselung:**
- **Pos 1:** `25:10:02:17:09:38.7` = Timestamp auf Millisekunde genau
- **Pos 2:** `::` = Separator
- **Pos 3:** `Van Khaos,P[12698228@19236104 Van Khaos@vankhaos#2007]` = Spielername + Player-ID
- **Pos 4:** `Schildgenerator I,C[41 Ground_Engineering_Kit_Shield_Generator_1_Klg_R9]` = EntityDetails + C[ID] (Ground_ = Kitmodul)
- **Pos 5:** `Marschall Janeway,S[140096978]` = ZielEntity + S[ID] (Companion vom Spieler)
- **Pos 6:** `Schildgenerator-Energiematrix I` = Attack (Waffe/Fähigkeit)
- **Pos 7:** `Pn.Q9ejka` = AbilityID (uninteressant)
- **Pos 8:** `Shield` = Schadensart
- **Pos 9:** `` (leer) = EventTyp (Normal)
- **Pos 10:** `-11.6073` = Schaden (negativer Wert = Heilung?)
- **Pos 11:** `0` = SchadenMitResistenz

### Zeile 5 - Phasergewehr:
```
25:10:02:17:09:41.1::Gil,C[25 Ground_Cardassian_Ens_Range],,*,Sillar,S[139588389],Phasergewehr,Pn.Kkaio1,Phaser,,7.11893,99.6068
```

**Positionen-Aufschlüsselung:**
- **Pos 1:** `25:10:02:17:09:41.1` = Timestamp auf Millisekunde genau
- **Pos 2:** `::` = Separator
- **Pos 3:** `Gil,C[25 Ground_Cardassian_Ens_Range]` = Spielername + C[ID] (Ground_ = Bodenkampf)
- **Pos 4:** `` (leer) = EntityDetails (leer)
- **Pos 5:** `Sillar,S[139588389]` = ZielEntity + S[ID] (Companion vom Spieler)
- **Pos 6:** `Phasergewehr` = Attack (Waffe/Fähigkeit)
- **Pos 7:** `Pn.Kkaio1` = AbilityID (uninteressant)
- **Pos 8:** `Phaser` = Schadensart
- **Pos 9:** `` (leer) = EventTyp (Normal)
- **Pos 10:** `7.11893` = Schaden
- **Pos 11:** `99.6068` = SchadenMitResistenz (höher als roher Schaden!)

## 🏷️ ID-Tags Struktur

### P[ID] - Player/Spieler (Position 3)
- **Format:** `P[CharID@AccountID CharName@Handle#Discriminator]`
- **Beispiel:** `P[12698228@19236104 Van Khaos@vankhaos#2007]`

### C[ID] - Creature/Entity (Position 4 oder 5)
- **Format:** `C[EntityID EntityName]`
- **Beispiel:** `C[558 Ground_Universal_Kit_Summer_Lava_Floor]`
- **Ground_** = Kitmodul oder Bodenkampf
- **Space_** = Hangerschiffe oder Raumkampf

### S[ID] - Companion vom Spieler (Position 4 oder 5)
- **Format:** `S[SystemID]` (nur numerische ID)
- **Beispiel:** `S[139588389]`, `S[140096978]`

### AbilityID (Position 7)
- **Format:** `Pn.xxxxx` (immer "Pn." gefolgt von 6 Zeichen)
- **Beispiele:** `Pn.Zeev3q`, `Pn.Zkj7wd`, `Pn.3hhwrm`, `Pn.Q9ejka`, `Pn.Kkaio1`
- **Status:** Uninteressant für Damage Meter

## 🎯 Event-Typen (Position 9)

### Standard Event-Typen
- **Leer** = Normal Hit
- **"DoT"** = Damage over Time
- **"Critical"** = Kritischer Treffer
- **"Miss"** = Verfehlt
- **"Immune"** = Immun (wird wie Miss behandelt)

### Wichtige Hinweise
- **Miss/Immune:** Schaden und SchadenMitResistenz sind uninteressant
- **Nur Critical und Normal Hit:** Sind für Damage Meter relevant

## 📊 Schadensarten (Position 8)

### Wichtige Hinweise
- **Nicht hardcoded!** Alle Schadensarten müssen dynamisch gelesen werden
- **Beispiele:** Fire, AntiProton, Cold, Shield, Phaser
- **Unbekannte Schadensarten** können jederzeit auftreten

## 🔧 Besondere Strukturen

### Leere Positionen
- **Position 4:** Kann leer sein (Zeile 5)
- **Position 9:** Kann leer sein (Normal Hit)

### Spezielle Fälle
- **Position 4:** Kann Entity-Details enthalten (Lava-Boden, Elite-Allianz-Jäger-Staffel, Sillar, Schildgenerator I)
- **Position 5:** Kann S[ID] für Companion enthalten
- **Position 10:** Kann negative Werte haben (Heilung?)
- **Position 11:** Kann höher oder niedriger sein als Position 10 (Schaden mit Resistenzen vom Gegner)

## 💡 Parser-Hinweise

### Wichtige Positionen für Damage Meter
- **Position 1:** Timestamp (für DPS-Berechnung)
- **Position 3:** Spielername + Player-ID (für Spieler-Identifikation)
- **Position 5:** ZielEntity + ID (für Ziel-Identifikation)
- **Position 6:** Attack (für Waffen-Ranking)
- **Position 8:** Schadensart (dynamisch lesen!)
- **Position 9:** EventTyp (für Critical-Rate - nur Critical und Normal Hit relevant)
- **Position 10:** Schaden (für Gesamtschaden - nur bei Hit/Critical relevant)
- **Position 11:** SchadenMitResistenz (kann höher/niedriger als Position 10 sein)

### Datenqualität
- ✅ **Konsistente Struktur** (11 Positionen)
- ✅ **Vollständige Zeitstempel**
- ✅ **Detaillierte Schadenswerte**
- ✅ **Event-Typ-Information**
- ✅ **ID-Tags für Entity-Identifikation**

---

*Struktur-Analyse basierend auf 5 Beispiel-Zeilen*