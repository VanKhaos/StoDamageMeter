using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace StoDamageMeter.Components.Shared
{
    /// <summary>
    /// Star Trek themed Icon component with Lucide React icon mappings
    /// </summary>
    public partial class StarfleetIcon : UserControl
    {
        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(IconType), typeof(StarfleetIcon),
                new PropertyMetadata(IconType.None, OnIconChanged));

        public static readonly DependencyProperty SizeProperty =
            DependencyProperty.Register("Size", typeof(double), typeof(StarfleetIcon),
                new PropertyMetadata(16.0, OnSizeChanged));

        public static readonly DependencyProperty ColorProperty =
            DependencyProperty.Register("Color", typeof(Brush), typeof(StarfleetIcon),
                new PropertyMetadata(null, OnColorChanged));

        public IconType Icon
        {
            get => (IconType)GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        public double Size
        {
            get => (double)GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        public Brush Color
        {
            get => (Brush)GetValue(ColorProperty);
            set => SetValue(ColorProperty, value);
        }

        public StarfleetIcon()
        {
            InitializeComponent();
        }

        private static void OnIconChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StarfleetIcon icon)
            {
                icon.UpdateIcon((IconType)e.NewValue);
            }
        }

        private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StarfleetIcon icon)
            {
                icon.IconText.FontSize = (double)e.NewValue;
            }
        }

        private static void OnColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StarfleetIcon icon)
            {
                icon.IconText.Foreground = e.NewValue as Brush ?? (Brush)icon.FindResource("Foreground");
            }
        }

        private void UpdateIcon(IconType iconType)
        {
            IconText.Text = GetIconUnicode(iconType);
        }

        private string GetIconUnicode(IconType iconType)
        {
            return iconType switch
            {
                IconType.Upload => "⬆",
                IconType.FileText => "📄",
                IconType.CheckCircle => "✓",
                IconType.AlertCircle => "⚠",
                IconType.Play => "▶",
                IconType.Pause => "⏸",
                IconType.Stop => "⏹",
                IconType.Settings => "⚙",
                IconType.User => "👤",
                IconType.Users => "👥",
                IconType.Chart => "📊",
                IconType.BarChart => "📈",
                IconType.PieChart => "📊",
                IconType.Calendar => "📅",
                IconType.Clock => "🕐",
                IconType.Sword => "⚔",
                IconType.Shield => "🛡",
                IconType.Star => "⭐",
                IconType.Info => "ℹ",
                IconType.Warning => "⚠",
                IconType.Error => "❌",
                IconType.Success => "✅",
                IconType.Home => "🏠",
                IconType.Dashboard => "📊",
                IconType.Statistics => "📈",
                IconType.LiveTracking => "📡",
                IconType.Configuration => "⚙",
                IconType.About => "ℹ",
                IconType.Close => "✕",
                IconType.Minimize => "➖",
                IconType.Maximize => "⛶",
                IconType.Refresh => "🔄",
                IconType.Download => "⬇",
                IconType.Folder => "📁",
                IconType.FolderOpen => "📂",
                IconType.Search => "🔍",
                IconType.Filter => "🔽",
                IconType.Sort => "⇅",
                IconType.Edit => "✏",
                IconType.Delete => "🗑",
                IconType.Add => "➕",
                IconType.Remove => "➖",
                IconType.ArrowLeft => "←",
                IconType.ArrowRight => "→",
                IconType.ArrowUp => "↑",
                IconType.ArrowDown => "↓",
                IconType.ChevronLeft => "‹",
                IconType.ChevronRight => "›",
                IconType.ChevronUp => "⌃",
                IconType.ChevronDown => "⌄",
                _ => ""
            };
        }
    }

    public enum IconType
    {
        None,
        Upload,
        FileText,
        CheckCircle,
        AlertCircle,
        Play,
        Pause,
        Stop,
        Settings,
        User,
        Users,
        Chart,
        BarChart,
        PieChart,
        Calendar,
        Clock,
        Sword,
        Shield,
        Star,
        Info,
        Warning,
        Error,
        Success,
        Home,
        Dashboard,
        Statistics,
        LiveTracking,
        Configuration,
        About,
        Close,
        Minimize,
        Maximize,
        Refresh,
        Download,
        Folder,
        FolderOpen,
        Search,
        Filter,
        Sort,
        Edit,
        Delete,
        Add,
        Remove,
        ArrowLeft,
        ArrowRight,
        ArrowUp,
        ArrowDown,
        ChevronLeft,
        ChevronRight,
        ChevronUp,
        ChevronDown
    }
}
