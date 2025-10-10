# Combat Statistics Table Structure

## Tabellenaufbau mit Demo-Daten

### Header
```
┌───────────────────────────────────────┬──────────┬──────────────┬──────────┬─────────┬─────────┐
│ Player / Ability                      │   DPS ▼  │ Total Damage │  Max Hit │  Crit % │  Acc %  │
├───────────────────────────────────────┼──────────┼──────────────┼──────────┼─────────┼─────────┤
```

### Level 1: Player (mit Expander-Icon ▼)
```
│ ▼ Captain Müller                      │  125,450 │   15,054,000 │  485,220 │   45.2% │   92.5% │
├───────────────────────────────────────┼──────────┼──────────────┼──────────┼─────────┼─────────┤
```

### Level 2a: Player Abilities (eingerückt mit Spacer)
```
│   ◦ Photonen-Torpedo-Salvø            │   42,150 │    5,058,000 │  485,220 │   52.3% │   95.1% │
│   ◦ Phaser-Strahl-Batterie III        │   28,340 │    3,400,800 │  125,450 │   41.8% │   91.2% │
│   ◦ Gravitonen-Torpedo-Streuung II    │   18,920 │    2,270,400 │   98,760 │   38.5% │   89.7% │
├───────────────────────────────────────┼──────────┼──────────────┼──────────┼─────────┼─────────┤
```

### Level 2b: Companion (eingerückt mit Expander-Icon ▶)
```
│     ▶ 🤖 Synth-Android N7             │   22,450 │    2,694,000 │  156,890 │   48.7% │   93.4% │
├───────────────────────────────────────┼──────────┼──────────────┼──────────┼─────────┼─────────┤
```

### Level 3: Companion Abilities (doppelt eingerückt mit Spacer)
```
│       ◦ Plasma-Infusionsgebläse I     │   12,340 │    1,480,800 │  156,890 │   51.2% │   94.8% │
│       ◦ Neutronen-Granatwerfer II     │    7,680 │      921,600 │   89,450 │   45.6% │   91.5% │
│       ◦ Hyperonische Strahlung        │    2,430 │      291,600 │   34,220 │   49.1% │   94.2% │
└───────────────────────────────────────┴──────────┴──────────────┴──────────┴─────────┴─────────┘
```

## Komplettes Beispiel mit mehreren Playern

```
┏━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┳━━━━━━━━━━┳━━━━━━━━━━━━━━┳━━━━━━━━━━┳━━━━━━━━━┳━━━━━━━━━┓
┃ Player / Ability                      ┃   DPS ▼  ┃ Total Damage ┃  Max Hit ┃  Crit % ┃  Acc %  ┃
┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━╋━━━━━━━━━━╋━━━━━━━━━━━━━━╋━━━━━━━━━━╋━━━━━━━━━╋━━━━━━━━━┫
┃ ▼ Captain Müller                      ┃  125,450 ┃   15,054,000 ┃  485,220 ┃   45.2% ┃   92.5% ┃
┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━╋━━━━━━━━━━╋━━━━━━━━━━━━━━╋━━━━━━━━━━╋━━━━━━━━━╋━━━━━━━━━┫
┃   ◦ Photonen-Torpedo-Salvø            ┃   42,150 ┃    5,058,000 ┃  485,220 ┃   52.3% ┃   95.1% ┃
┃   ◦ Phaser-Strahl-Batterie III        ┃   28,340 ┃    3,400,800 ┃  125,450 ┃   41.8% ┃   91.2% ┃
┃   ◦ Gravitonen-Torpedo-Streuung II    ┃   18,920 ┃    2,270,400 ┃   98,760 ┃   38.5% ┃   89.7% ┃
┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━╋━━━━━━━━━━╋━━━━━━━━━━━━━━╋━━━━━━━━━━╋━━━━━━━━━╋━━━━━━━━━┫
┃     ▶ 🤖 Synth-Android N7            ┃   22,450 ┃    2,694,000 ┃  156,890 ┃   48.7% ┃   93.4% ┃
┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━╋━━━━━━━━━━╋━━━━━━━━━━━━━━╋━━━━━━━━━━╋━━━━━━━━━╋━━━━━━━━━┫
┃       ◦ Plasma-Infusionsgebläse I     ┃   12,340 ┃    1,480,800 ┃  156,890 ┃   51.2% ┃   94.8% ┃
┃       ◦ Neutronen-Granatwerfer II     ┃    7,680 ┃      921,600 ┃   89,450 ┃   45.6% ┃   91.5% ┃
┃       ◦ Hyperonische Strahlung        ┃    2,430 ┃      291,600 ┃   34,220 ┃   49.1% ┃   94.2% ┃
┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━╋━━━━━━━━━━╋━━━━━━━━━━━━━━╋━━━━━━━━━━╋━━━━━━━━━╋━━━━━━━━━┫
┃     ▶ 🤖 Delta-Flyer-Shuttle         ┃   13,590 ┃    1,630,800 ┃   78,450 ┃   42.1% ┃   88.9% ┃
┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━╋━━━━━━━━━━╋━━━━━━━━━━━━━━╋━━━━━━━━━━╋━━━━━━━━━╋━━━━━━━━━┫
┃       ◦ Typ-IV-Phaser-Batterie        ┃    9,120 ┃    1,094,400 ┃   67,890 ┃   43.5% ┃   89.7% ┃
┃       ◦ Mikro-Photonen-Torpedo        ┃    4,470 ┃      536,400 ┃   78,450 ┃   39.8% ┃   87.3% ┃
┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━╋━━━━━━━━━━╋━━━━━━━━━━━━━━╋━━━━━━━━━━╋━━━━━━━━━╋━━━━━━━━━┫
┃ ▶ Lieutenant Schäfer                 ┃   89,230 ┃   10,707,600 ┃  398,760 ┃   43.8% ┃   90.2% ┃
┣━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━╋━━━━━━━━━━╋━━━━━━━━━━━━━━╋━━━━━━━━━━╋━━━━━━━━━╋━━━━━━━━━┫
┃   ◦ Disruptor-Kanonen-Schnellfeuer V  ┃   34,670 ┃    4,160,400 ┃  398,760 ┃   47.2% ┃   92.8% ┃
┃   ◦ Plasma-Torpedo-Hochertrag III     ┃   28,450 ┃    3,414,000 ┃  287,340 ┃   41.5% ┃   88.6% ┃
┃   ◦ Quantonen-Mine-Streuung I         ┃   26,110 ┃    3,133,200 ┃  156,780 ┃   42.7% ┃   89.5% ┃
┗━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━┻━━━━━━━━━━┻━━━━━━━━━━━━━━┻━━━━━━━━━━┻━━━━━━━━━┻━━━━━━━━━┛
```

## Hierarchie-Ebenen

### Einrückungen und Spacer

| Ebene | Element | Margin | Spacer | Gesamt | Icon |
|-------|---------|--------|--------|--------|------|
| **Level 1** | Player | 8px | - | 8px | ▼/▶ (10px + 8px Margin) |
| **Level 2a** | Player Ability | 8px | 14px | 22px | ◦ (Bullet) |
| **Level 2b** | Companion | 32px | - | 32px | ▼/▶ (9px + 6px Margin) |
| **Level 3** | Companion Ability | 32px | 11px | 43px | ◦ (Bullet) |

### Spalten-Breiten (GridLength)

| Spalte | Width | Zweck |
|--------|-------|-------|
| Player / Ability | `3*` | Hauptspalte für Namen (breiteste) |
| DPS | `1*` | Damage per Second |
| Total Damage | `1.2*` | Gesamt-Schaden (etwas breiter) |
| Max Hit | `1*` | Maximaler Treffer |
| Crit % | `0.7*` | Critical Hit Prozent (kompakt) |
| Acc % | `0.7*` | Accuracy Prozent (kompakt) |

## Trennlinien-Alignment

```
Player:           [8px Margin]            [▼Icon(18px)]  [Text]
                  │                       │
Ability:          [8px Margin][14px Spacer][◦ Text]
                  │            │
                  └────────────┴─────► Gleiche Startposition!

Companion:        [32px Margin]           [▶Icon(15px)]  [🤖 Text]
                  │                       │
Companion Ability:[32px Margin][11px Spacer][◦ Text]
                  │            │
                  └────────────┴─────► Gleiche Startposition!
```

## Sortierung

- **Initial:** Nach DPS (mit Companions) absteigend ▼
- **Klickbar:** Alle Spalten (außer Player/Ability)
- **Toggle:** Klick auf gleiche Spalte → Richtung wechseln (▲/▼)
- **Visual:** Aktive Spalte in Star Trek Blue (#5B9BD5), fett

## Farb-Schema

| Element | Hintergrund | Text | Besonderheit |
|---------|-------------|------|--------------|
| Header | #1E1E1E | #B0B0B0 | Buttons mit Hover (#20FFFFFF) |
| Player | #1A1A1A | #FFFFFF | FontWeight: SemiBold |
| Ability | #101010 | #B0B0B0 | Opacity: 0.9 |
| Companion | #141414 | #B0B0B0 | Emoji: 🤖, Opacity: 0.95 |
| Companion Ability | #0C0C0C | #9C9C9C | Opacity: 0.85 |
| Trennlinien | - | #333333 | 1px BorderThickness |

## Icons und Symbole

- **Player Expander:** ▶ (collapsed) / ▼ (expanded) - Blau (#5B9BD5)
- **Companion Expander:** ▶ (collapsed) / ▼ (expanded) - Blau (#5B9BD5)
- **Companion Marker:** 🤖 (Roboter-Emoji)
- **Abilities:** ◦ (Bullet Point)
- **Sort Indicators:** ▲ (aufsteigend) / ▼ (absteigend)

## Features

✅ 3-Level-Hierarchie (Player → Companion/Ability → Companion Ability)  
✅ Gemischte Sortierung (Companions + Abilities nach Total Damage)  
✅ Click-to-Sort für alle Daten-Spalten  
✅ Expandable/Collapsible Rows  
✅ Durchgängige vertikale Trennlinien  
✅ Sonderzeichen-Support (ä, ö, ü, ø)  
✅ Responsive Spalten-Breiten (Star-basiert)  
✅ Star Trek Blue Theme  

