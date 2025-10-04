using System;
using System.Configuration;
using System.Data;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StoDamageMeter.Services;
using StoDamageMeter.ViewModels;
using StoDamageMeter.Views;

namespace StoDamageMeter;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ServiceProvider? _serviceProvider;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Debug-Logging ist jetzt in der Console

        // Dependency Injection konfigurieren
        var services = new ServiceCollection();
        ConfigureServices(services);
        _serviceProvider = services.BuildServiceProvider();

        // MainWindow über DI erstellen
        var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        // Logging konfigurieren
        services.AddLogging(builder =>
        {
            // Console-Logging aktivieren
            builder.AddConsole();
            builder.AddDebug();
            builder.SetMinimumLevel(LogLevel.Debug);
        });

        // Services registrieren
        services.AddSingleton<ICombatLogParser, CombatLogParser>();
        services.AddSingleton<CombatPeriodService>();
        services.AddSingleton<WeaponStatisticsService>();
        services.AddSingleton<ICombatLogService, CombatLogService>();

        // ViewModels registrieren
        services.AddTransient<CombatLogViewModel>();
        services.AddTransient<StatisticsViewModel>();
        services.AddTransient<NavigationViewModel>();
        services.AddTransient<MainViewModel>();

        // Pages registrieren
        services.AddTransient<Pages.Statistics.StatisticsPage>();
        services.AddTransient<Pages.Dashboard.DashboardPage>();
        services.AddTransient<Pages.LiveTracking.LiveTrackingPage>();
        services.AddTransient<Pages.Configuration.ConfigurationPage>();
        services.AddTransient<Pages.About.AboutPage>();

        // Views registrieren
        services.AddTransient<MainWindow>();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _serviceProvider?.Dispose();
        base.OnExit(e);
    }

}

