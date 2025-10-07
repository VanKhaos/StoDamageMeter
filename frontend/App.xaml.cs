using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using frontend.Services;

namespace frontend;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public static IServiceProvider ServiceProvider { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Konfiguration laden
        var configuration = new ConfigurationBuilder()
            .SetBasePath(System.AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Services registrieren
        var services = new ServiceCollection();
        
        // Logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Konfiguration
        services.AddSingleton<IConfiguration>(configuration);

        // OSCR Backend Service
        services.AddSingleton<IOSCRBackendService, OSCRBackendService>();

        // Service Provider erstellen
        ServiceProvider = services.BuildServiceProvider();

        // MainWindow starten (nur einmal)
        var mainWindow = new MainWindow();
        mainWindow.WindowState = WindowState.Normal;
        mainWindow.Topmost = true;
        mainWindow.Show();
        mainWindow.Activate();
        mainWindow.Topmost = false;
        
        // MainWindow als Application MainWindow setzen
        Application.Current.MainWindow = mainWindow;
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

