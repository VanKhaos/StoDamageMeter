# 🚀 WPF Setup Guide für STO Damage Meter

## 📋 Übersicht

Diese Anleitung zeigt dir, wie du eine **Windows-only Desktop App** mit **WPF (Windows Presentation Foundation)** in **Cursor AI IDE** entwickelst. WPF ist die beste Wahl für Windows-Apps, da es einfacher, performanter und kleiner ist als MAUI.

## 🛠️ Was du brauchst

### **1. .NET 8 SDK installieren:**
```bash
# Download von: https://dotnet.microsoft.com/download/dotnet/8.0
# Oder mit Winget:
winget install Microsoft.DotNet.SDK.8
```

### **2. Cursor AI Extensions installieren:**
- ✅ **C# Dev Kit** (Microsoft)
- ✅ **C#** (Microsoft)
- ✅ **IntelliCode** (Microsoft)
- ✅ **XAML** (für XAML-Unterstützung)
- ✅ **NuGet Package Manager**

### **3. Optional: Visual Studio Build Tools**
```bash
# Für bessere Build-Performance
winget install Microsoft.VisualStudio.2022.BuildTools
```

## 🚀 Projekt Setup

### **Schritt 1: WPF Projekt erstellen**
```bash
# In Cursor Terminal
dotnet new wpf -n StoDamageMeter
cd StoDamageMeter
```

### **Schritt 2: NuGet Packages hinzufügen**
```bash
# MVVM Toolkit für bessere Architektur
dotnet add package CommunityToolkit.Mvvm

# Dependency Injection
dotnet add package Microsoft.Extensions.DependencyInjection

# Logging
dotnet add package Microsoft.Extensions.Logging

# JSON Serialization
dotnet add package Newtonsoft.Json
```

### **Schritt 3: Cursor konfigurieren**
```json
// .vscode/settings.json
{
  "dotnet.defaultSolution": "StoDamageMeter.sln",
  "omnisharp.enableRoslynAnalyzers": true,
  "omnisharp.enableEditorConfigSupport": true,
  "dotnet.completion.showCompletionItemsFromUnimportedNamespaces": true,
  "files.exclude": {
    "**/bin": true,
    "**/obj": true
  },
  "xaml.format.enable": true,
  "xaml.format.indentSize": 2
}
```

## 🎨 UI-Entwicklung in Cursor

### **XAML Editor in Cursor:**

**Ja, Cursor hat einen XAML Editor!** Du kannst XAML-Dateien direkt in Cursor bearbeiten mit:
- ✅ **Syntax Highlighting**
- ✅ **IntelliSense**
- ✅ **Auto-Complete**
- ✅ **Formatierung**
- ✅ **Error Detection**

### **UI-Entwicklung Workflow:**

#### **1. XAML-Dateien bearbeiten:**
```xml
<!-- MainWindow.xaml - wird direkt in Cursor bearbeitet -->
<Window x:Class="StoDamageMeter.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        Title="STO Damage Meter" Height="800" Width="1200">
    
    <Grid>
        <!-- Dein UI-Code hier -->
    </Grid>
</Window>
```

#### **2. Code-Behind bearbeiten:**
```csharp
// MainWindow.xaml.cs - wird direkt in Cursor bearbeitet
using System.Windows;

namespace StoDamageMeter
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
    }
}
```

#### **3. Live Preview:**
```bash
# In Cursor Terminal - startet die App
dotnet run

# Mit Hot Reload (automatische Updates bei Änderungen)
dotnet watch run
```

## 🏗️ Projekt-Struktur

```
StoDamageMeter/
├── Models/                 # Datenmodelle
│   ├── CombatEvent.cs
│   ├── PlayerData.cs
│   └── Fight.cs
├── ViewModels/            # MVVM ViewModels
│   ├── MainViewModel.cs
│   └── PlayerStatsViewModel.cs
├── Views/                 # XAML Views
│   ├── MainWindow.xaml
│   ├── MainWindow.xaml.cs
│   └── PlayerStatsView.xaml
├── Services/              # Business Logic
│   ├── CombatlogWatcher.cs
│   ├── LogParserService.cs
│   └── StatisticsService.cs
├── App.xaml              # App-Konfiguration
├── App.xaml.cs           # App Startup
└── StoDamageMeter.csproj # Projekt-Datei
```

## 📱 FileWatch Service implementieren

### **1. CombatlogWatcher Service:**
```csharp
// Services/CombatlogWatcher.cs
using System.IO;
using System.Windows;

namespace StoDamageMeter.Services
{
    public class CombatlogWatcher : IDisposable
    {
        private FileSystemWatcher? _watcher;
        public event EventHandler<string>? FileChanged;
        public event EventHandler<string>? FileCreated;
        
        public void StartWatching(string filePath)
        {
            var directory = Path.GetDirectoryName(filePath);
            var fileName = Path.GetFileName(filePath);
            
            _watcher = new FileSystemWatcher(directory!, fileName)
            {
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.CreationTime
            };
            
            _watcher.Changed += OnFileChanged;
            _watcher.Created += OnFileCreated;
        }
        
        private void OnFileChanged(object sender, FileSystemEventArgs e)
        {
            // UI Thread Update
            Application.Current.Dispatcher.Invoke(() =>
            {
                FileChanged?.Invoke(this, e.FullPath);
            });
        }
        
        private void OnFileCreated(object sender, FileSystemEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                FileCreated?.Invoke(this, e.FullPath);
            });
        }
        
        public void Dispose()
        {
            _watcher?.Dispose();
        }
    }
}
```

### **2. MainViewModel mit MVVM:**
```csharp
// ViewModels/MainViewModel.cs
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using StoDamageMeter.Services;
using System.Windows;

namespace StoDamageMeter.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly CombatlogWatcher _watcher;
        
        [ObservableProperty]
        private string _selectedLogFile = string.Empty;
        
        [ObservableProperty]
        private string _lastUpdate = string.Empty;
        
        [ObservableProperty]
        private bool _isWatching = false;
        
        public MainViewModel()
        {
            _watcher = new CombatlogWatcher();
            _watcher.FileChanged += OnFileChanged;
        }
        
        [RelayCommand]
        public void SelectLogFile()
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Combatlog auswählen",
                Filter = "Log-Dateien (*.log)|*.log|Text-Dateien (*.txt)|*.txt|Alle Dateien (*.*)|*.*",
                Multiselect = false
            };
            
            if (openFileDialog.ShowDialog() == true)
            {
                SelectedLogFile = openFileDialog.FileName;
                _watcher.StartWatching(openFileDialog.FileName);
                IsWatching = true;
            }
        }
        
        private void OnFileChanged(object? sender, string filePath)
        {
            LastUpdate = DateTime.Now.ToString("HH:mm:ss");
            // Hier kannst du die neuen Combatlog-Daten verarbeiten
            // und die UI aktualisieren
        }
    }
}
```

## 🎨 UI mit XAML erstellen

### **1. MainWindow.xaml:**
```xml
<!-- Views/MainWindow.xaml -->
<Window x:Class="StoDamageMeter.MainWindow"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="clr-namespace:StoDamageMeter.ViewModels"
        x:DataType="vm:MainViewModel"
        Title="STO Damage Meter" Height="800" Width="1200"
        WindowStartupLocation="CenterScreen">
    
    <Window.Resources>
        <!-- Styling für moderne UI -->
        <Style x:Key="ModernButton" TargetType="Button">
            <Setter Property="Background" Value="#2196F3"/>
            <Setter Property="Foreground" Value="White"/>
            <Setter Property="BorderThickness" Value="0"/>
            <Setter Property="Padding" Value="15,8"/>
            <Setter Property="FontWeight" Value="SemiBold"/>
            <Setter Property="Cursor" Value="Hand"/>
            <Style.Triggers>
                <Trigger Property="IsMouseOver" Value="True">
                    <Setter Property="Background" Value="#1976D2"/>
                </Trigger>
            </Style.Triggers>
        </Style>
        
        <Style x:Key="StatusIndicator" TargetType="Ellipse">
            <Setter Property="Width" Value="12"/>
            <Setter Property="Height" Value="12"/>
            <Setter Property="Margin" Value="5"/>
        </Style>
    </Window.Resources>
    
    <Grid>
        <Grid.RowDefinitions>
            <RowDefinition Height="Auto"/>
            <RowDefinition Height="*"/>
        </Grid.RowDefinitions>
        
        <!-- Header -->
        <Border Grid.Row="0" Background="#2C3E50" Padding="20">
            <StackPanel>
                <TextBlock Text="STO Damage Meter" 
                           FontSize="28" 
                           FontWeight="Bold" 
                           Foreground="White" 
                           HorizontalAlignment="Center" 
                           Margin="0,0,0,15"/>
                
                <StackPanel Orientation="Horizontal" HorizontalAlignment="Center">
                    <Button Content="Combatlog auswählen" 
                            Command="{Binding SelectLogFileCommand}"
                            Style="{StaticResource ModernButton}"
                            Margin="0,0,10,0"/>
                    
                    <Ellipse Style="{StaticResource StatusIndicator}"
                             Fill="{Binding IsWatching, Converter={StaticResource BoolToColorConverter}}"/>
                    
                    <TextBlock Text="{Binding IsWatching, Converter={StaticResource BoolToStatusConverter}}" 
                               Foreground="White" 
                               VerticalAlignment="Center"/>
                </StackPanel>
                
                <TextBlock Text="{Binding SelectedLogFile}" 
                           FontSize="12" 
                           Foreground="#BDC3C7" 
                           HorizontalAlignment="Center" 
                           Margin="0,10,0,0"/>
                
                <TextBlock Text="{Binding LastUpdate, StringFormat='Letztes Update: {0}'}" 
                           FontSize="12" 
                           Foreground="#27AE60" 
                           HorizontalAlignment="Center"/>
            </StackPanel>
        </Border>
        
        <!-- Content Area -->
        <Grid Grid.Row="1" Background="#ECF0F1">
            <Grid.ColumnDefinitions>
                <ColumnDefinition Width="300"/>
                <ColumnDefinition Width="*"/>
            </Grid.ColumnDefinitions>
            
            <!-- Sidebar -->
            <Border Grid.Column="0" Background="White" BorderBrush="#BDC3C7" BorderThickness="0,0,1,0">
                <StackPanel Margin="20">
                    <TextBlock Text="Statistiken" 
                               FontSize="18" 
                               FontWeight="Bold" 
                               Margin="0,0,0,15"/>
                    
                    <!-- Hier kommen deine Statistiken -->
                    <TextBlock Text="Keine Daten verfügbar" 
                               Foreground="#7F8C8D" 
                               HorizontalAlignment="Center" 
                               VerticalAlignment="Center"/>
                </StackPanel>
            </Border>
            
            <!-- Main Content -->
            <ScrollViewer Grid.Column="1" Margin="20">
                <StackPanel>
                    <TextBlock Text="Damage Meter Statistiken" 
                               FontSize="20" 
                               FontWeight="Bold" 
                               Margin="0,0,0,20"/>
                    
                    <!-- Hier kommen deine Damage Meter Tabellen -->
                    <Border Background="White" 
                            BorderBrush="#BDC3C7" 
                            BorderThickness="1" 
                            CornerRadius="5" 
                            Padding="20">
                        <TextBlock Text="Statistiken werden hier angezeigt..." 
                                   HorizontalAlignment="Center" 
                                   VerticalAlignment="Center" 
                                   Foreground="#7F8C8D"/>
                    </Border>
                </StackPanel>
            </ScrollViewer>
        </Grid>
    </Grid>
</Window>
```

### **2. MainWindow.xaml.cs:**
```csharp
// Views/MainWindow.xaml.cs
using StoDamageMeter.ViewModels;
using System.Windows;

namespace StoDamageMeter
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
```

## 🔧 Development Commands

### **Development starten:**
```bash
# In Cursor Terminal
dotnet run

# Mit Hot Reload (automatische Updates)
dotnet watch run

# Debug Mode
dotnet run --configuration Debug
```

### **Build Commands:**
```bash
# Debug Build
dotnet build

# Release Build
dotnet build --configuration Release

# Publish für Distribution
dotnet publish -c Release -r win-x64 --self-contained true
```

## 🎯 UI-Entwicklung Tipps

### **1. XAML IntelliSense nutzen:**
- **Ctrl+Space** für Auto-Complete
- **F12** für Go to Definition
- **Ctrl+Shift+P** → "XAML: Format Document"

### **2. Live Preview:**
```bash
# Starte die App und lasse sie laufen
dotnet watch run

# Ändere XAML → App aktualisiert sich automatisch
```

### **3. Debugging:**
- **F5** für Debug-Modus
- **Breakpoints** in C# Code setzen
- **XAML Binding Errors** werden in Output angezeigt

### **4. Styling:**
```xml
<!-- Moderne Buttons -->
<Button Style="{StaticResource ModernButton}">
    <Button.Content>
        <StackPanel Orientation="Horizontal">
            <Path Data="M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2Z" 
                  Fill="White" 
                  Width="16" 
                  Height="16" 
                  Margin="0,0,5,0"/>
            <TextBlock Text="Button Text"/>
        </StackPanel>
    </Button.Content>
</Button>
```

## 📦 Distribution

### **1. Einfache .exe erstellen:**
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

### **2. Installer erstellen (optional):**
```bash
# Mit WiX Toolset
dotnet add package WixSharp
```

## 🎉 Vorteile von WPF

- ✅ **Einfacher** als MAUI
- ✅ **Bessere Performance** (native Windows)
- ✅ **Kleinere App-Größe** (20-40MB)
- ✅ **Reifere Technologie** (seit 2006)
- ✅ **Perfekte FileWatch-Unterstützung**
- ✅ **Ausgezeichnete Cursor-Unterstützung**
- ✅ **XAML Editor** mit IntelliSense
- ✅ **Hot Reload** für schnelle Entwicklung

## 🚀 Nächste Schritte

1. **Cursor installieren** und Extensions hinzufügen
2. **.NET 8 SDK** installieren
3. **WPF Projekt** erstellen
4. **FileWatch Service** implementieren
5. **UI entwickeln** mit XAML
6. **MVVM Pattern** verwenden
7. **App testen** und distributieren

**Mit diesem Setup hast du eine moderne, performante Windows Desktop App mit ausgezeichneter Entwicklungserfahrung in Cursor!**
