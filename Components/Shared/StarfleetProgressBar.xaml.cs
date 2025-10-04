using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StoDamageMeter.Components.Shared
{
    /// <summary>
    /// Star Trek themed Progress Bar component
    /// </summary>
    public partial class StarfleetProgressBar : UserControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(StarfleetProgressBar),
                new PropertyMetadata(0.0, OnValueChanged));

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(StarfleetProgressBar),
                new PropertyMetadata(string.Empty, OnTextChanged));

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public StarfleetProgressBar()
        {
            InitializeComponent();
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StarfleetProgressBar progressBar)
            {
                progressBar.UpdateProgress((double)e.NewValue);
            }
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StarfleetProgressBar progressBar)
            {
                progressBar.ProgressText.Text = e.NewValue?.ToString() ?? string.Empty;
            }
        }

        private void UpdateProgress(double value)
        {
            // Begrenze den Wert auf positive Zahlen
            var clampedValue = Math.Max(0, value);

            // Berechne die Breite basierend auf dem Wert
            var maxWidth = BackgroundBorder.ActualWidth - 2; // 2 für Margin

            // Wenn maxWidth noch nicht verfügbar ist, verwende eine Standardbreite
            if (maxWidth <= 0)
            {
                maxWidth = 200; // Fallback-Breite
            }

            // Berechne den relativen Fortschritt (0-1)
            var progress = Math.Min(1.0, clampedValue / 1000.0); // Normalisiere auf 1000 als Maximum
            var progressWidth = progress * maxWidth;

            // Stelle sicher, dass die Breite nicht negativ ist
            ProgressBorder.Width = Math.Max(0, progressWidth);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            // Aktualisiere die Progress Bar wenn sich die Größe ändert
            UpdateProgress(Value);
        }
    }
}
