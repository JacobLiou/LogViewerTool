using LogViewer.MVVM.Models;
using LogViewer.MVVM.ViewModels;
using System.ComponentModel;
using System.Windows;

namespace LogViewer.MVVM.Views
{
    /// <summary>
    /// Interaction logic for LogImportTemplate.xaml
    /// </summary>
    public partial class LogImportTemplateDialog : Window
    {
        public LogTemplate LogTemplate { get; private set; }
        public bool NeedUpdateFile { get; private set; } = false;

        public LogImportTemplateDialog(string path)
        {
            InitializeComponent();
            DataContext = new LogImportTemplateViewModel(path);
        }

        private void LogImportTemplateDialog_OnClosing(object sender, CancelEventArgs e)
        {
            var logImportTemplateViewModel = this.DataContext as LogImportTemplateViewModel;
            if (logImportTemplateViewModel != null)
            {
                LogTemplate = logImportTemplateViewModel.LogTemplate;
                NeedUpdateFile = logImportTemplateViewModel.NeedUpdateFile;
            }
        }
    }
}