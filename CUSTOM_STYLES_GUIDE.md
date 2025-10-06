# 🎨 Benutzerdefinierte Styles für GenericDataGrid

## ✅ **Ja, es ist möglich!**

Du kannst sehr einfach neue Styles im `Styles` Ordner erstellen und sie für die GenericDataGrid verwenden.

## 🚀 **Schritt-für-Schritt Anleitung**

### **Schritt 1: Neue Styles in DataGridStyles.xaml hinzufügen**

```xml
<!-- Dein neuer Card-Style -->
<Style x:Key="StarfleetCardMyStyle" TargetType="{x:Type Border}">
    <Setter Property="Background" Value="#1A1A2E" />
    <Setter Property="BorderBrush" Value="#16213E" />
    <Setter Property="BorderThickness" Value="2" />
    <Setter Property="CornerRadius" Value="12" />
    <Setter Property="Padding" Value="20" />
    <Setter Property="Margin" Value="0,0,0,20" />
    <Setter Property="Effect">
        <Setter.Value>
            <DropShadowEffect Color="#0F3460" Opacity="0.3" ShadowDepth="4" BlurRadius="8" />
        </Setter.Value>
    </Setter>
</Style>

<!-- Dein neuer DataGrid-Style -->
<Style x:Key="DataGridMyStyle" TargetType="{x:Type DataGrid}" BasedOn="{StaticResource DataGridStyle}">
    <Setter Property="Background" Value="#1A1A2E" />
    <Setter Property="Foreground" Value="#E94560" />
    <Setter Property="AlternatingRowBackground" Value="#16213E" />
    <Setter Property="FontFamily" Value="Segoe UI" />
    <Setter Property="FontSize" Value="13" />
    <Setter Property="RowHeight" Value="32" />
</Style>
```

### **Schritt 2: Enum erweitern**

In `Models/DataGridColumnConfig.cs`:

```csharp
public enum DataGridStyleVariant
{
    Default,
    Compact,
    Detailed,
    Minimal,
    Dark,
    Light,
    Accent,
    Gaming,
    Corporate,
    Retro,
    MyStyle,  // 👈 Dein neuer Style
    AnotherStyle  // 👈 Weitere Styles möglich
}
```

### **Schritt 3: GenericDataGrid erweitern**

In `Components/Shared/GenericDataGrid.xaml.cs`:

```csharp
private Style? GetCardStyle()
{
    return StyleVariant switch
    {
        // ... bestehende Styles ...
        DataGridStyleVariant.MyStyle => FindResource("StarfleetCardMyStyle") as Style,
        DataGridStyleVariant.AnotherStyle => FindResource("StarfleetCardAnotherStyle") as Style,
        _ => FindResource("StarfleetCardStyle") as Style
    };
}

private Style? GetDataGridStyle()
{
    return StyleVariant switch
    {
        // ... bestehende Styles ...
        DataGridStyleVariant.MyStyle => FindResource("DataGridMyStyle") as Style,
        DataGridStyleVariant.AnotherStyle => FindResource("DataGridAnotherStyle") as Style,
        _ => FindResource("DataGridStyle") as Style
    };
}
```

### **Schritt 4: Style verwenden**

```xml
<shared:GenericDataGrid
    ItemsSource="{Binding MyData}"
    HeaderTitle="Meine Tabelle"
    HeaderSubtitle="Mit meinem eigenen Style"
    HeaderIcon="Star"
    StyleVariant="MyStyle"
    HeaderStyleVariant="Default" />
```

## 🎯 **Beispiele für verschiedene Style-Typen**

### **🌙 Dark Theme Style:**
```xml
<Style x:Key="StarfleetCardDarkThemeStyle" TargetType="{x:Type Border}">
    <Setter Property="Background" Value="#0D1117" />
    <Setter Property="BorderBrush" Value="#30363D" />
    <Setter Property="BorderThickness" Value="1" />
    <Setter Property="CornerRadius" Value="8" />
    <Setter Property="Padding" Value="20" />
</Style>
```

### **🌈 Rainbow Style:**
```xml
<Style x:Key="StarfleetCardRainbowStyle" TargetType="{x:Type Border}">
    <Setter Property="Background" Value="#FF6B6B" />
    <Setter Property="BorderBrush" Value="#4ECDC4" />
    <Setter Property="BorderThickness" Value="3" />
    <Setter Property="CornerRadius" Value="15" />
    <Setter Property="Padding" Value="25" />
</Style>
```

### **🏢 Professional Style:**
```xml
<Style x:Key="StarfleetCardProfessionalStyle" TargetType="{x:Type Border}">
    <Setter Property="Background" Value="#FFFFFF" />
    <Setter Property="BorderBrush" Value="#2C3E50" />
    <Setter Property="BorderThickness" Value="1" />
    <Setter Property="CornerRadius" Value="4" />
    <Setter Property="Padding" Value="24" />
</Style>
```

## 🔧 **Style-Eigenschaften die du anpassen kannst:**

### **Card-Style (Border):**
- `Background` - Hintergrundfarbe
- `BorderBrush` - Border-Farbe
- `BorderThickness` - Border-Dicke
- `CornerRadius` - Ecken-Rundung
- `Padding` - Innenabstand
- `Margin` - Außenabstand
- `Effect` - Schatten-Effekte

### **DataGrid-Style:**
- `Background` - Hintergrundfarbe
- `Foreground` - Textfarbe
- `AlternatingRowBackground` - Abwechselnde Zeilenfarbe
- `FontFamily` - Schriftart
- `FontSize` - Schriftgröße
- `RowHeight` - Zeilenhöhe
- `GridLinesVisibility` - Gridlines sichtbar/unsichtbar

## 🎨 **Farben-Palette für Inspiration:**

```xml
<!-- Gaming-Farben -->
#00FF00 - Matrix Grün
#FF0000 - Gaming Rot
#0000FF - Gaming Blau
#FFFF00 - Gaming Gelb

<!-- Corporate-Farben -->
#2C3E50 - Dunkelblau
#34495E - Grau-Blau
#ECF0F1 - Hellgrau
#BDC3C7 - Mittelgrau

<!-- Retro-Farben -->
#8B4513 - Braun
#DAA520 - Gold
#CD853F - Perubraun
#F4A460 - Sandbraun

<!-- Modern-Farben -->
#1A1A2E - Dunkel
#16213E - Blau-Dunkel
#0F3460 - Blau
#E94560 - Rot-Akzent
```

## 🚀 **Vorteile:**

- ✅ **Einfach zu erstellen**: Nur XAML-Styles hinzufügen
- ✅ **Sofort verwendbar**: Keine Kompilierung nötig
- ✅ **Konsistent**: Verwendet das gleiche Design-System
- ✅ **Wiederverwendbar**: Für alle Tabellen im Projekt
- ✅ **Erweiterbar**: Unbegrenzt viele Styles möglich
- ✅ **Wartbar**: Zentrale Verwaltung in Styles-Ordner

## 📝 **Tipp:**

Erstelle zuerst den Style in `DataGridStyles.xaml`, teste ihn mit einem bestehenden Style-Namen, und füge dann erst das Enum und die Zuordnung hinzu. So kannst du den Style sofort testen! 🎯
