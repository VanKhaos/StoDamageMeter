using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace StoDamageMeter.Components.Shared
{
    public partial class LogFileSelector : UserControl
    {
        public event EventHandler<string>? LogFileSelected;

        public string LogFilePath
        {
            get => LogPathTextBox.Text;
            set => LogPathTextBox.Text = value;
        }

        public LogFileSelector()
        {
            InitializeComponent();
        }

        private void OnBrowseClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Combat Log File",
                Filter = "Log files (*.log)|*.log|All files (*.*)|*.*",
                CheckFileExists = true
            };

            if (dialog.ShowDialog() == true)
            {
                LogFilePath = dialog.FileName;
                LogFileSelected?.Invoke(this, dialog.FileName);
            }
        }
    }
}





