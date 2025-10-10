using System.Diagnostics;
using System.IO;
using System.Windows;

namespace StoDamageMeter.Launcher;

internal class Program
{
    private const string CoreAppRelativePath = @"App\StoDamageMeter.Core.exe";
    
    [STAThread]
    static int Main(string[] args)
    {
        try
        {
            // Get the directory where the launcher is located
            var launcherDir = AppDomain.CurrentDomain.BaseDirectory;
            var coreAppPath = Path.Combine(launcherDir, CoreAppRelativePath);

            // Check if core application exists
            if (!File.Exists(coreAppPath))
            {
                MessageBox.Show(
                    $"ERROR: StoDamageMeter core application not found!\n\n" +
                    $"Expected location: {coreAppPath}\n\n" +
                    $"Please ensure the application is properly installed.",
                    "STO Damage Meter - Launcher Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return 1;
            }

            // Start the core application with App/ as working directory
            var appDirectory = Path.Combine(launcherDir, "App");
            var startInfo = new ProcessStartInfo
            {
                FileName = coreAppPath,
                Arguments = string.Join(" ", args.Select(arg => 
                    arg.Contains(' ') ? $"\"{arg}\"" : arg)),
                UseShellExecute = false,
                WorkingDirectory = appDirectory  // Core-App läuft im App/ Verzeichnis
            };

            var process = Process.Start(startInfo);
            
            if (process == null)
            {
                MessageBox.Show(
                    "ERROR: Failed to start StoDamageMeter core application.",
                    "STO Damage Meter - Launcher Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return 1;
            }

            // Launcher beendet sich sofort, Core-App läuft weiter
            return 0;
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"ERROR: {ex.Message}\n\n{ex.StackTrace}",
                "STO Damage Meter - Launcher Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return 1;
        }
    }
}

