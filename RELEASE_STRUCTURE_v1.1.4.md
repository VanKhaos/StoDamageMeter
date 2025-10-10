# 📦 STO Damage Meter v1.1.4 - Release Struktur

**Build-Datum:** 10.10.2025  
**Gesamtgröße:** ~148.66 MB (entpackt) | ~68.62 MB (ZIP)  
**Anzahl Dateien:** 414

---

## 📁 Hauptverzeichnis

### 🎯 Haupt-Anwendung
```
StoDamageMeter.exe              # Hauptanwendung (WPF)
StoDamageMeter.dll              # Anwendungs-Bibliothek
OSCRBackend.exe                 # Python Backend (7.7 MB)
```

### ⚙️ Konfiguration
```
appsettings.json                # Konfigurationsdatei
StoDamageMeter.deps.json        # .NET Abhängigkeiten
StoDamageMeter.runtimeconfig.json  # Runtime-Konfiguration
README.txt                      # Benutzer-Anleitung
```

### 🔧 .NET Runtime & Core (Self-Contained)
```
clretwrc.dll                    # CLR Event Tracing
clrgc.dll                       # CLR Garbage Collector
clrgcexp.dll                    # CLR GC Exportiert
clrjit.dll                      # CLR Just-In-Time Compiler
coreclr.dll                     # .NET Core Runtime
hostfxr.dll                     # Host Framework Resolver
hostpolicy.dll                  # Host Policy
createdump.exe                  # Crash Dump Tool
```

### 🎨 WPF & Präsentation (Windows Presentation Foundation)
```
PresentationCore.dll            # WPF Core
PresentationFramework.dll       # WPF Framework
PresentationNative_cor3.dll     # WPF Native
PresentationUI.dll              # WPF UI
WindowsBase.dll                 # Windows Base
System.Xaml.dll                 # XAML Support
Wpf.Ui.dll                      # WPF UI Library (Fluent Design)
```

#### WPF Themes
```
PresentationFramework.Aero.dll
PresentationFramework.Aero2.dll
PresentationFramework.AeroLite.dll
PresentationFramework.Classic.dll
PresentationFramework.Fluent.dll
PresentationFramework.Luna.dll
PresentationFramework.Royale.dll
```

### 🖼️ Grafik & Rendering
```
D3DCompiler_47_cor3.dll         # DirectX Shader Compiler
DirectWriteForwarder.dll        # DirectWrite Text Rendering
wpfgfx_cor3.dll                 # WPF Graphics
PenImc_cor3.dll                 # Pen Input Method Collection
vcruntime140_cor3.dll           # Visual C++ Runtime
```

### 🧩 Microsoft Extensions
```
Microsoft.Extensions.Configuration.dll
Microsoft.Extensions.Configuration.Abstractions.dll
Microsoft.Extensions.Configuration.Binder.dll
Microsoft.Extensions.Configuration.FileExtensions.dll
Microsoft.Extensions.Configuration.Json.dll
Microsoft.Extensions.DependencyInjection.dll
Microsoft.Extensions.DependencyInjection.Abstractions.dll
Microsoft.Extensions.FileProviders.Abstractions.dll
Microsoft.Extensions.FileProviders.Physical.dll
Microsoft.Extensions.FileSystemGlobbing.dll
Microsoft.Extensions.Logging.dll
Microsoft.Extensions.Logging.Abstractions.dll
Microsoft.Extensions.Logging.Configuration.dll
Microsoft.Extensions.Logging.Console.dll
Microsoft.Extensions.Options.dll
Microsoft.Extensions.Options.ConfigurationExtensions.dll
Microsoft.Extensions.Primitives.dll
```

### 📚 System Bibliotheken (Hauptkategorien)

#### Kern-Bibliotheken
```
System.dll                      # System Base
System.Core.dll                 # System Core
System.Private.CoreLib.dll      # Private Core Library
netstandard.dll                 # .NET Standard
mscorlib.dll                    # Microsoft Core Library
mscordaccore.dll                # Data Access
mscordbi.dll                    # Debugger Interface
mscorrc.dll                     # Resource Compiler
```

#### Collections & Data
```
System.Collections.dll
System.Collections.Concurrent.dll
System.Collections.Immutable.dll
System.Collections.NonGeneric.dll
System.Collections.Specialized.dll
System.Data.dll
System.Data.Common.dll
System.Data.DataSetExtensions.dll
```

#### I/O & Kompression
```
System.IO.dll
System.IO.Compression.dll
System.IO.Compression.Brotli.dll
System.IO.Compression.FileSystem.dll
System.IO.Compression.Native.dll
System.IO.Compression.ZipFile.dll
System.IO.FileSystem.dll
System.IO.FileSystem.AccessControl.dll
System.IO.FileSystem.DriveInfo.dll
System.IO.FileSystem.Primitives.dll
System.IO.FileSystem.Watcher.dll
System.IO.IsolatedStorage.dll
System.IO.MemoryMappedFiles.dll
System.IO.Packaging.dll
System.IO.Pipelines.dll
System.IO.Pipes.dll
System.IO.Pipes.AccessControl.dll
System.IO.UnmanagedMemoryStream.dll
```

#### Networking
```
System.Net.dll
System.Net.Http.dll
System.Net.Http.Json.dll
System.Net.HttpListener.dll
System.Net.Mail.dll
System.Net.NameResolution.dll
System.Net.NetworkInformation.dll
System.Net.Ping.dll
System.Net.Primitives.dll
System.Net.Quic.dll
System.Net.Requests.dll
System.Net.Security.dll
System.Net.ServicePoint.dll
System.Net.Sockets.dll
System.Net.WebClient.dll
System.Net.WebHeaderCollection.dll
System.Net.WebProxy.dll
System.Net.WebSockets.dll
System.Net.WebSockets.Client.dll
msquic.dll                      # Microsoft QUIC
```

#### Security & Cryptography
```
System.Security.dll
System.Security.AccessControl.dll
System.Security.Claims.dll
System.Security.Cryptography.dll
System.Security.Cryptography.Algorithms.dll
System.Security.Cryptography.Cng.dll
System.Security.Cryptography.Csp.dll
System.Security.Cryptography.Encoding.dll
System.Security.Cryptography.OpenSsl.dll
System.Security.Cryptography.Pkcs.dll
System.Security.Cryptography.Primitives.dll
System.Security.Cryptography.ProtectedData.dll
System.Security.Cryptography.X509Certificates.dll
System.Security.Cryptography.Xml.dll
System.Security.Permissions.dll
System.Security.Principal.dll
System.Security.Principal.Windows.dll
System.Security.SecureString.dll
```

#### Threading & Tasks
```
System.Threading.dll
System.Threading.AccessControl.dll
System.Threading.Channels.dll
System.Threading.Overlapped.dll
System.Threading.Tasks.dll
System.Threading.Tasks.Dataflow.dll
System.Threading.Tasks.Extensions.dll
System.Threading.Tasks.Parallel.dll
System.Threading.Thread.dll
System.Threading.ThreadPool.dll
System.Threading.Timer.dll
```

#### Text & Encoding
```
System.Text.Encoding.dll
System.Text.Encoding.CodePages.dll
System.Text.Encoding.Extensions.dll
System.Text.Encodings.Web.dll
System.Text.Json.dll
System.Text.RegularExpressions.dll
```

#### XML & Serialization
```
System.Xml.dll
System.Xml.Linq.dll
System.Xml.ReaderWriter.dll
System.Xml.Serialization.dll
System.Xml.XDocument.dll
System.Xml.XmlDocument.dll
System.Xml.XmlSerializer.dll
System.Xml.XPath.dll
System.Xml.XPath.XDocument.dll
System.Private.Xml.dll
System.Private.Xml.Linq.dll
System.Runtime.Serialization.dll
System.Runtime.Serialization.Formatters.dll
System.Runtime.Serialization.Json.dll
System.Runtime.Serialization.Primitives.dll
System.Runtime.Serialization.Xml.dll
System.Private.DataContractSerialization.dll
```

#### Reflection & Runtime
```
System.Reflection.dll
System.Reflection.DispatchProxy.dll
System.Reflection.Emit.dll
System.Reflection.Emit.ILGeneration.dll
System.Reflection.Emit.Lightweight.dll
System.Reflection.Extensions.dll
System.Reflection.Metadata.dll
System.Reflection.Primitives.dll
System.Reflection.TypeExtensions.dll
System.Runtime.dll
System.Runtime.CompilerServices.Unsafe.dll
System.Runtime.CompilerServices.VisualC.dll
System.Runtime.Extensions.dll
System.Runtime.Handles.dll
System.Runtime.InteropServices.dll
System.Runtime.InteropServices.JavaScript.dll
System.Runtime.InteropServices.RuntimeInformation.dll
System.Runtime.Intrinsics.dll
System.Runtime.Loader.dll
System.Runtime.Numerics.dll
```

#### Diagnostics
```
System.Diagnostics.Contracts.dll
System.Diagnostics.Debug.dll
System.Diagnostics.DiagnosticSource.dll
System.Diagnostics.EventLog.dll
System.Diagnostics.EventLog.Messages.dll
System.Diagnostics.FileVersionInfo.dll
System.Diagnostics.PerformanceCounter.dll
System.Diagnostics.Process.dll
System.Diagnostics.StackTrace.dll
System.Diagnostics.TextWriterTraceListener.dll
System.Diagnostics.Tools.dll
System.Diagnostics.TraceSource.dll
System.Diagnostics.Tracing.dll
Microsoft.DiaSymReader.Native.amd64.dll
```

#### Windows Spezifisch
```
System.Windows.dll
System.Windows.Controls.Ribbon.dll
System.Windows.Extensions.dll
System.Windows.Input.Manipulations.dll
System.Windows.Presentation.dll
Microsoft.Win32.Primitives.dll
Microsoft.Win32.Registry.dll
Microsoft.Win32.Registry.AccessControl.dll
Microsoft.Win32.SystemEvents.dll
```

#### UI Automation
```
UIAutomationClient.dll
UIAutomationClientSideProviders.dll
UIAutomationProvider.dll
UIAutomationTypes.dll
Accessibility.dll
```

#### Weitere System Bibliotheken
```
System.AppContext.dll
System.Buffers.dll
System.CodeDom.dll
System.ComponentModel.dll
System.ComponentModel.Annotations.dll
System.ComponentModel.DataAnnotations.dll
System.ComponentModel.EventBasedAsync.dll
System.ComponentModel.Primitives.dll
System.ComponentModel.TypeConverter.dll
System.Configuration.dll
System.Configuration.ConfigurationManager.dll
System.Console.dll
System.DirectoryServices.dll
System.Drawing.dll
System.Drawing.Primitives.dll
System.Dynamic.Runtime.dll
System.Formats.Asn1.dll
System.Formats.Nrbf.dll
System.Formats.Tar.dll
System.Globalization.dll
System.Globalization.Calendars.dll
System.Globalization.Extensions.dll
System.Linq.dll
System.Linq.Expressions.dll
System.Linq.Parallel.dll
System.Linq.Queryable.dll
System.Memory.dll
System.Numerics.dll
System.Numerics.Vectors.dll
System.ObjectModel.dll
System.Printing.dll
System.Private.Uri.dll
System.Resources.Extensions.dll
System.Resources.Reader.dll
System.Resources.ResourceManager.dll
System.Resources.Writer.dll
System.ServiceModel.Web.dll
System.ServiceProcess.dll
System.Transactions.dll
System.Transactions.Local.dll
System.ValueTuple.dll
System.Web.dll
System.Web.HttpUtility.dll
Microsoft.CSharp.dll
Microsoft.VisualBasic.dll
Microsoft.VisualBasic.Core.dll
ReachFramework.dll
```

---

## 📁 Unterverzeichnisse

### 📂 logs/
```
backend_debug.log               # Backend Debug Log
backend_service_debug.log       # Backend Service Debug Log
frontend_debug.log              # Frontend Debug Log
oscr_backend.log                # OSCR Backend Log
```

### 🌍 Sprachressourcen (Localization)

Jeder Sprachordner enthält 12 Resource-DLLs für WPF UI-Elemente:

```
cs/          (Tschechisch)
de/          (Deutsch)
es/          (Spanisch)
fr/          (Französisch)
it/          (Italienisch)
ja/          (Japanisch)
ko/          (Koreanisch)
pl/          (Polnisch)
pt-BR/       (Brasilianisches Portugiesisch)
ru/          (Russisch)
tr/          (Türkisch)
zh-Hans/     (Vereinfachtes Chinesisch)
zh-Hant/     (Traditionelles Chinesisch)
```

#### Inhalt jedes Sprachordners (12 Dateien):
```
PresentationCore.resources.dll
PresentationFramework.resources.dll
PresentationUI.resources.dll
ReachFramework.resources.dll
System.Windows.Controls.Ribbon.resources.dll
System.Windows.Input.Manipulations.resources.dll
System.Xaml.resources.dll
UIAutomationClient.resources.dll
UIAutomationClientSideProviders.resources.dll
UIAutomationProvider.resources.dll
UIAutomationTypes.resources.dll
WindowsBase.resources.dll
```

---

## 📊 Statistik

| Kategorie | Anzahl |
|-----------|--------|
| **Hauptdateien** | 4 |
| **Konfigurationsdateien** | 4 |
| **System DLLs** | ~240 |
| **WPF/UI Bibliotheken** | ~20 |
| **Microsoft Extensions** | 17 |
| **Sprachordner** | 13 |
| **Resource DLLs** | 156 (12 × 13 Sprachen) |
| **Log-Dateien** | 4 |
| **Gesamtdateien** | **414** |

---

## 🎯 Wichtigste Dateien für Benutzer

```
StoDamageMeter.exe              ← Hauptanwendung starten
OSCRBackend.exe                 ← Wird automatisch von StoDamageMeter.exe gestartet
appsettings.json                ← Konfiguration anpassen
README.txt                      ← Anleitung lesen
logs/                           ← Bei Problemen Logs prüfen
```

---

## ⚙️ Self-Contained Deployment

✅ **Keine Installation erforderlich!**

Das Release ist als Self-Contained Build erstellt. Das bedeutet:
- ✅ **Gesamte .NET 9.0 Runtime** ist enthalten
- ✅ **Alle WPF-Bibliotheken** sind enthalten
- ✅ **Python Backend** ist als Standalone EXE enthalten
- ✅ Läuft auf **jedem Windows 10/11 64-bit** System
- ❌ **Keine** .NET Installation erforderlich
- ❌ **Keine** Python Installation erforderlich

---

## 🚀 Installation & Start

1. **Entpacken:** ZIP-Datei in beliebigen Ordner entpacken
2. **Starten:** Doppelklick auf `StoDamageMeter.exe`
3. **Fertig!** Die Anwendung ist sofort einsatzbereit

---

**Build-System:** .NET 9.0 + PyInstaller  
**Plattform:** Windows x64  
**Release-Art:** Self-Contained  
**Komprimierung:** Optimal

