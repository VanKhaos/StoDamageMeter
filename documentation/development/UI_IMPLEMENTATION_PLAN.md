# STO Damage Meter - UI Implementierungsplan

## Übersicht
Dieses Dokument beschreibt die geplante Benutzeroberfläche für die STO Damage Meter Anwendung, basierend auf dem Design der "Open Source Combatlog Reader" Anwendung.

## Design-Prinzipien
- **Dark Theme**: Dunkles Design mit orangen Akzenten für aktive Elemente
- **Zwei-Panel-Layout**: Linke Sidebar für Navigation, rechter Bereich für Daten
- **Konsistente Farbgebung**: Weiß für Text, Orange für aktive/ausgewählte Elemente
- **Moderne Icons**: Einfache, monochrome Icons

## Hauptfenster-Struktur

### 1. Header-Bar
```
┌─────────────────────────────────────────────────────────────────┐
│ [A] STO Damage Meter                                            │
└─────────────────────────────────────────────────────────────────┘
```

**Elemente:**
- **Titel**: "STO Damage Meter" (weißer Text)
- **Icon**: Schaltkreis/Chip Icon rechts

### 2. Hauptbereich (Zwei-Panel-Layout)

#### Linke Sidebar (25-30% Breite)
```
┌─────────────────────┐
│ STO Combatlog:      │
│ [📁] combatlog.log  │
│ [Browse...] [Default] [Analyze] │
│                     │
│ Combat List:        │
│ ▶ Combat 20:45:24   │
│   2025-10-06        │
│ ▶ Combat 18:37:55   │
│   2025-09-28        │
│ ▶ Combat 18:28:37   │
│   2025-09-28        │
│                     │
│ Log Duration: 462.9s│
│ Player Duration: 462.7s │
│ [Map Detection Details] │
└─────────────────────┘
```

**Elemente:**
- **Log-Pfad**: Eingabefeld mit Ordner-Icon
- **Aktions-Buttons**: Browse, Default, Analyze
- **Combat-Liste**: Scrollbare Liste mit Combat-Einträgen
  - Format: [Typ] [Zeit] [Datum]
  - Aktiver Eintrag: Orange linke Umrandung
- **Footer-Info**: Log-Dauer, Player-Dauer, Map Detection Link

#### Rechter Datenbereich (70-75% Breite | DPS Graph 20% höhe | Combat Statistik tabelle direkt unter DPS Graphen, egal welche proportionen die Anwendung hat)
```
┌─────────────────────────────────────────────────────────────┐
│ [Damage Out] [Damage Taken] [Heals Out] [Heals In] [Selection ▼] [⚏] │
│                                                             │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │                    DPS Graph                            │ │
│ │                                                         │ │
│ │ 0 ┌─────────────────────────────────────────────────┐ 0 │ │
│ │   │                                                 │   │ │
│ │   │                                                 │   │ │
│ │   │                                                 │   │ │
│ │   └─────────────────────────────────────────────────┘   │ │
│ │ 0s                                                   462s │ 
│ └─────────────────────────────────────────────────────────┘ │
│                                                             │
│ ┌─────────────────────────────────────────────────────────┐ │
│ │ DPS │ Total Damage │ Debuff │ Max Hit │ Crit │ Acc │ ...│ │
│ ├─────────────────────────────────────────────────────────┤ │
│ │ ▶ Player                                                │ │
│ │   Van Khaos@vankhaos#2007                               │ │
│ │   1.18 │ 70.57 │ 0% │ 7.84 │ 0% │ 100% │ ... │          │ │
│ │   ▶ Ba'ul Antiproton Array - Overload III               │ │
│ │     13.10 │ 5.97 │ 38.66% │ 71.34 │ 39.51% │ ... │      │ │
│ │   ▶ Ba'ul Antiproton Array                             │ │
│ │     6.18 │ 2.82 │ 27.81% │ 14.45 │ 41.07% │ ... │       │ │
│ │ ▶ NPC                                                  │ │
│ └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

**Elemente:**
- **Filter-Bar**: Damage Out (aktiv, orange), Damage Taken, Heals Out, Heals In
- **Auswahl-Dropdown**: "Selection" mit Pfeil
- **DPS-Graph**: Zeitreihen-Diagramm für Combat-Daten
- **Daten-Tabelle**: 
  - Spalten: DPS, Total Damage, Debuff, Max Hit, Crit Chance, Accuracy, etc.
  - Kategorien: Player (erweiterbar), NPC (erweiterbar)
  - Player-Details: Name, Statistiken
  - Ability-Details: Erweiterbare Untereinträge für Fähigkeiten



## Anmerkungen

- **Responsive Design**: Layout sollte bei verschiedenen Fenstergrößen funktionieren
- **Accessibility**: Tastaturnavigation und Screen-Reader-Unterstützung
- **Performance**: Effiziente Datenanzeige bei großen Combat-Logs
- **Erweiterbarkeit**: Modulare Struktur für zukünftige Features
