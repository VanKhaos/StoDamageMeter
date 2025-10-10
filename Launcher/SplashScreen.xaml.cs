using System.Windows;

namespace StoDamageMeter.Launcher;

public partial class SplashScreen : Window
{
    public SplashScreen()
    {
        InitializeComponent();
        Opacity = 0; // Start invisible for fade-in animation
    }
    
    public void UpdateStatus(string message)
    {
        Dispatcher.Invoke(() =>
        {
            StatusText.Text = message;
        });
    }
    
    public void CloseSplash()
    {
        Dispatcher.Invoke(() =>
        {
            // Fade out before closing
            var fadeOut = new System.Windows.Media.Animation.DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
            fadeOut.Completed += (s, e) => Close();
            BeginAnimation(OpacityProperty, fadeOut);
        });
    }
}

