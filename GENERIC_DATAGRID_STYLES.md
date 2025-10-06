# GenericDataGrid Style-Varianten

Die `GenericDataGrid`-Komponente unterstützt verschiedene Style-Varianten für maximale Flexibilität.

## 🎨 Style-Varianten

### **DataGridStyleVariant**

| Variante | Beschreibung | Verwendung |
|----------|--------------|------------|
| `Default` | Standard Starfleet-Style | Haupttabellen, Standard-Ansichten |
| `Compact` | Kompakter Style für weniger Platz | Übersichtstabellen, Dashboard-Widgets |
| `Detailed` | Detaillierter Style mit mehr Informationen | Detailansichten, große Tabellen |
| `Minimal` | Minimaler Style ohne Card-Border | Inline-Tabellen, einfache Listen |
| `Dark` | Dunkler Style | Dark Theme, bessere Sichtbarkeit |
| `Light` | Heller Style | Light Theme, helle Umgebungen |
| `Accent` | Style mit Akzentfarben | Hervorhebungen, wichtige Tabellen |

### **DataGridHeaderStyleVariant**

| Variante | Beschreibung | Verwendung |
|----------|--------------|------------|
| `Default` | Standard Header | Normale Tabellen |
| `Compact` | Kompakter Header | Weniger Platz verfügbar |
| `Detailed` | Detaillierter Header | Mehr Informationen im Header |
| `Minimal` | Minimaler Header | Sehr wenig Platz |
| `IconOnly` | Nur Icon, kein Text | Icon-basierte Navigation |
| `TextOnly` | Nur Text, kein Icon | Text-basierte Ansichten |

## 🔧 Verwendung

### **Grundlegende Verwendung:**
```xml
<shared:GenericDataGrid
    ItemsSource="{Binding MyData}"
    HeaderTitle="Meine Tabelle"
    HeaderSubtitle="Beschreibung"
    HeaderIcon="BarChart"
    StyleVariant="Default"
    HeaderStyleVariant="Default" />
```

### **Kompakte Tabelle:**
```xml
<shared:GenericDataGrid
    ItemsSource="{Binding MyData}"
    HeaderTitle="Übersicht"
    StyleVariant="Compact"
    HeaderStyleVariant="Compact"
    MaxHeight="200" />
```

### **Minimale Tabelle:**
```xml
<shared:GenericDataGrid
    ItemsSource="{Binding MyData}"
    HeaderTitle="Einfache Liste"
    StyleVariant="Minimal"
    HeaderStyleVariant="Minimal" />
```

### **Dunkle Tabelle:**
```xml
<shared:GenericDataGrid
    ItemsSource="{Binding MyData}"
    HeaderTitle="Dark Theme"
    StyleVariant="Dark"
    HeaderStyleVariant="Default" />
```

### **Akzent-Tabelle:**
```xml
<shared:GenericDataGrid
    ItemsSource="{Binding MyData}"
    HeaderTitle="Wichtige Daten"
    StyleVariant="Accent"
    HeaderStyleVariant="Detailed" />
```

### **Icon-Only Header:**
```xml
<shared:GenericDataGrid
    ItemsSource="{Binding MyData}"
    HeaderTitle="Icon Tabelle"
    HeaderIcon="BarChart"
    StyleVariant="Default"
    HeaderStyleVariant="IconOnly" />
```

## 🎯 Style-Eigenschaften

### **Card-Styles:**
- **Default**: Standard Starfleet-Card mit Gold-Border
- **Compact**: Kleinere Padding, dünnere Border
- **Detailed**: Größere Padding, Schatten-Effekt
- **Minimal**: Transparent, kein Border
- **Dark**: Dunkler Hintergrund, helle Borders
- **Light**: Heller Hintergrund, dunkle Borders
- **Accent**: Akzentfarben, Schatten-Effekt

### **DataGrid-Styles:**
- **Default**: Standard DataGrid-Style
- **Compact**: Kleinere Schrift (12px), niedrigere Zeilen (28px)
- **Detailed**: Größere Schrift (14px), höhere Zeilen (36px), alle Gridlines
- **Minimal**: Sehr kleine Schrift (11px), niedrige Zeilen (24px), keine Gridlines
- **Dark**: Dunkler Hintergrund, helle Schrift
- **Light**: Heller Hintergrund, dunkle Schrift
- **Accent**: Akzentfarben für Hintergrund

### **Header-Styles:**
- **Default**: Standard Margin (20,20,20,10)
- **Compact**: Kleinerer Margin (15,15,15,8)
- **Detailed**: Größerer Margin (25,25,25,15)
- **Minimal**: Sehr kleiner Margin (10,10,10,5)
- **IconOnly**: Versteckt alle Text-Elemente
- **TextOnly**: Versteckt alle Icon-Elemente

## 📝 Beispiele

Siehe `Components/PageSpecific/Statistics/StyleExamplesCard.xaml` für vollständige Beispiele aller Style-Varianten.

## 🚀 Vorteile

- **Flexibilität**: 7 Card-Styles × 6 Header-Styles = 42 Kombinationen
- **Konsistenz**: Alle Styles verwenden das gleiche Design-System
- **Wartbarkeit**: Styles sind zentral in `DataGridStyles.xaml` definiert
- **Erweiterbarkeit**: Neue Styles können einfach hinzugefügt werden
- **Performance**: Styles werden zur Laufzeit angewendet, keine Kompilierung nötig
