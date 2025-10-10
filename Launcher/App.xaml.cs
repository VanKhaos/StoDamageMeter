using System.Diagnostics;
using System.IO;
using System.Windows;

namespace StoDamageMeter.Launcher;

public partial class App : Application
{
    private const string CoreAppRelativePath = @"App\StoDamageMeter.Core.exe";
    private SplashScreen? _splashScreen;
    
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        
        // Show splash screen
        _splashScreen = new SplashScreen();
        _splashScreen.Show();
        
        // Start core application on background thread
        Task.Run(() => StartCoreApplication(e.Args));
    }
    
    private async Task StartCoreApplication(string[] args)
    {
        try
        {
            // Simulate some initialization time for visual effect
            await Task.Delay(800);
            
            _splashScreen?.UpdateStatus("Initializing components...");
            await Task.Delay(400);
            
            _splashScreen?.UpdateStatus("Starting application...");
            
            // Get the directory where the launcher is located
            var launcherDir = AppDomain.CurrentDomain.BaseDirectory;
            var coreAppPath = Path.Combine(launcherDir, CoreAppRelativePath);

            // Check if core application exists
            if (!File.Exists(coreAppPath))
            {
                Dispatcher.Invoke(() =>
                {
                    _splashScreen?.Close();
                    MessageBox.Show(
                        $"ERROR: StoDamageMeter core application not found!\n\n" +
                        $"Expected location: {coreAppPath}\n\n" +
                        $"Please ensure the application is properly installed.",
                        "STO Damage Meter - Launcher Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    Shutdown(1);
                });
                return;
            }

            // Start the core application with App/ as working directory
            var appDirectory = Path.Combine(launcherDir, "App");
            var startInfo = new ProcessStartInfo
            {
                FileName = coreAppPath,
                Arguments = string.Join(" ", args.Select(arg => 
                    arg.Contains(' ') ? $"\"{arg}\"" : arg)),
                UseShellExecute = false,
                WorkingDirectory = appDirectory
            };

            var process = Process.Start(startInfo);
            
            if (process == null)
            {
                Dispatcher.Invoke(() =>
                {
                    _splashScreen?.Close();
                    MessageBox.Show(
                        "ERROR: Failed to start StoDamageMeter core application.",
                        "STO Damage Meter - Launcher Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    Shutdown(1);
                });
                return;
            }

            // Wait a bit to ensure core app window appears
            await Task.Delay(1500);
            
            // Close splash screen and shutdown launcher
            Dispatcher.Invoke(() =>
            {
                _splashScreen?.CloseSplash();
                
                // Shutdown launcher after splash fade-out
                Task.Delay(500).ContinueWith(_ => Dispatcher.Invoke(() => Shutdown(0)));
            });
        }
        catch (Exception ex)
        {
            Dispatcher.Invoke(() =>
            {
                _splashScreen?.Close();
                MessageBox.Show(
                    $"ERROR: {ex.Message}\n\n{ex.StackTrace}",
                    "STO Damage Meter - Launcher Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown(1);
            });
        }
    }
}

