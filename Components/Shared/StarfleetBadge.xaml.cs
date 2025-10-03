using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StoDamageMeter.Components.Shared
{
    /// <summary>
    /// Star Trek themed Badge component with different variants
    /// </summary>
    public partial class StarfleetBadge : UserControl
    {
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(StarfleetBadge),
                new PropertyMetadata(string.Empty, OnTextChanged));

        public static readonly DependencyProperty VariantProperty =
            DependencyProperty.Register("Variant", typeof(BadgeVariant), typeof(StarfleetBadge),
                new PropertyMetadata(BadgeVariant.Default, OnVariantChanged));

        public string Text
        {
            get => (string)GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public BadgeVariant Variant
        {
            get => (BadgeVariant)GetValue(VariantProperty);
            set => SetValue(VariantProperty, value);
        }

        public StarfleetBadge()
        {
            InitializeComponent();
        }

        private static void OnTextChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StarfleetBadge badge)
            {
                badge.BadgeText.Text = e.NewValue?.ToString() ?? string.Empty;
            }
        }

        private static void OnVariantChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StarfleetBadge badge)
            {
                badge.UpdateVariant((BadgeVariant)e.NewValue);
            }
        }

        private void UpdateVariant(BadgeVariant variant)
        {
            switch (variant)
            {
                case BadgeVariant.Default:
                    BadgeBorder.Background = (SolidColorBrush)FindResource("Primary");
                    BadgeBorder.BorderBrush = (SolidColorBrush)FindResource("Primary");
                    BadgeText.Foreground = (SolidColorBrush)FindResource("PrimaryForeground");
                    break;
                case BadgeVariant.Secondary:
                    BadgeBorder.Background = (SolidColorBrush)FindResource("Secondary");
                    BadgeBorder.BorderBrush = (SolidColorBrush)FindResource("Secondary");
                    BadgeText.Foreground = (SolidColorBrush)FindResource("SecondaryForeground");
                    break;
                case BadgeVariant.Destructive:
                    BadgeBorder.Background = (SolidColorBrush)FindResource("Destructive");
                    BadgeBorder.BorderBrush = (SolidColorBrush)FindResource("Destructive");
                    BadgeText.Foreground = (SolidColorBrush)FindResource("DestructiveForeground");
                    break;
                case BadgeVariant.Outline:
                    BadgeBorder.Background = Brushes.Transparent;
                    BadgeBorder.BorderBrush = (SolidColorBrush)FindResource("Border");
                    BadgeText.Foreground = (SolidColorBrush)FindResource("Foreground");
                    break;
                case BadgeVariant.Success:
                    BadgeBorder.Background = (SolidColorBrush)FindResource("SuccessGreen");
                    BadgeBorder.BorderBrush = (SolidColorBrush)FindResource("SuccessGreen");
                    BadgeText.Foreground = (SolidColorBrush)FindResource("PrimaryForeground");
                    break;
                case BadgeVariant.Warning:
                    BadgeBorder.Background = (SolidColorBrush)FindResource("WarningOrange");
                    BadgeBorder.BorderBrush = (SolidColorBrush)FindResource("WarningOrange");
                    BadgeText.Foreground = (SolidColorBrush)FindResource("PrimaryForeground");
                    break;
            }
        }
    }

    public enum BadgeVariant
    {
        Default,
        Secondary,
        Destructive,
        Outline,
        Success,
        Warning
    }
}
