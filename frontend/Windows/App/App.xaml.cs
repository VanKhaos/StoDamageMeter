using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StoDamageMeter.Services;

namespace StoDamageMeter;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Services registrieren
        var services = new ServiceCollection();
        
        // Logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // OSCR Backend Service
        services.AddSingleton<IOSCRBackendService, OSCRBackendService>();

        // Combat Log Watcher Service
        services.AddSingleton<CombatLogWatcherService>();

        // Update Check Service
        services.AddSingleton<UpdateCheckService>();

        // Application Window Binding Service
        services.AddSingleton<ApplicationWindowBindingService>();

        // Service Provider erstellen
        ServiceProvider = services.BuildServiceProvider();

        // LandingWindow starten (neuer Einstiegspunkt)
        var landingWindow = new LandingWindow();
        
        // Prüfen ob STO läuft und Fenster positionieren
        var windowBinding = ServiceProvider.GetRequiredService<ApplicationWindowBindingService>();
        PositionWindowInSto(landingWindow);
        
        landingWindow.Show();
        landingWindow.Activate();
        
        // LandingWindow als Application MainWindow setzen
        Application.Current.MainWindow = landingWindow;
    }

    private void PositionWindowInSto(Window window)
    {
        try
        {
            var windowBindingService = ServiceProvider.GetRequiredService<ApplicationWindowBindingService>();
            var windowBinding = new WindowBindingService();
            
               if (windowBinding.TryGetStoBounds(out var stoBounds))
               {
                   // Positioniere das Fenster innerhalb des STO-Fensters
                   window.Left = stoBounds.Left + 50; // 50px vom linken Rand
                   window.Top = stoBounds.Top + 50;   // 50px vom oberen Rand
                   
                   // Stelle sicher, dass das Fenster nicht außerhalb des STO-Fensters ist
                   if (window.Left + window.Width > stoBounds.Right)
                       window.Left = stoBounds.Right - window.Width - 10;
                   if (window.Top + window.Height > stoBounds.Bottom)
                       window.Top = stoBounds.Bottom - window.Height - 10;
               }
        }
        catch (Exception ex)
        {
            // Falls STO nicht läuft oder Fehler auftreten, verwende Standard-Position
            System.Diagnostics.Debug.WriteLine($"Could not position window in STO: {ex.Message}");
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
        base.OnExit(e);
    }
}

