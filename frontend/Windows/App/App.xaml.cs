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
            builder.AddProvider(new LoggingServiceFactory());
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // OSCR Backend Service
        services.AddSingleton<IOSCRBackendService, OSCRBackendService>();

        // Combat Log Watcher Service
        services.AddSingleton<CombatLogWatcherService>();

        // Update Check Service
        services.AddSingleton<UpdateCheckService>();

        // Service Provider erstellen
        ServiceProvider = services.BuildServiceProvider();

        // LandingWindow starten (neuer Einstiegspunkt)
        var landingWindow = new LandingWindow();
        landingWindow.Show();
        landingWindow.Activate();
        
        // LandingWindow als Application MainWindow setzen
        Application.Current.MainWindow = landingWindow;
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

