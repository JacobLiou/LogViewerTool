using LogViewer.MVVM.Models;
using LogViewer.MVVM.ViewModels;
using System;
using System.Collections.Generic;
using System.Windows;

namespace LogViewer.MVVM.Views
{
    /// <summary>
    /// Interaction logic for ImportFilesProcessDialog.xaml
    /// </summary>
    public partial class ImportLogsProcessDialog : Window
    {
        public event EventHandler<bool> ImportProcessDialogResult;

        public ImportLogsProcessDialog(List<ImportLogFile> importLogFiles)
        {
            InitializeComponent();
            this.DataContext = new ImportLogsProcessViewModel();
            ((ImportLogsProcessViewModel)DataContext).ImportFiles = new List<ImportLogFile>(importLogFiles);
        }

        private void OkButtonClick(object sender, RoutedEventArgs e)
        {
            OnImportProcessDialogResult(true);
            this.Close();
        }

        private void CancelButtonClick(object sender, RoutedEventArgs e)
        {
            OnImportProcessDialogResult(false);
            this.Close();
        }

        protected virtual void OnImportProcessDialogResult(bool e)
        {
            ImportProcessDialogResult?.Invoke(this, e);
        }
    }
}