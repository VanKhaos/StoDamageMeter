namespace StoDamageMeter.Launcher;

/// <summary>
/// Entry point for the launcher application.
/// The actual application logic is in App.xaml.cs
/// </summary>
internal class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        var app = new App();
        app.InitializeComponent();
        app.Run();
    }
}

