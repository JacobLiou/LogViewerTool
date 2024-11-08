using System;
using System.Deployment.Application;
using System.Windows;

namespace LogViewer.MVVM.Views
{
    /// <summary>
    /// Interaction logic for NewUpdateAvailableDialog.xaml
    /// </summary>
    public partial class NewUpdateAvailableDialog : Window
    {
        public NewUpdateAvailableDialog(UpdateCheckInfo updateInfo)
        {
            InitializeComponent();
            SizeTextBlock.Text = $"{Math.Round((double)updateInfo.UpdateSizeBytes / 1024 / 1024, 2)} MB";
            VersionTextBlock.Text = updateInfo.AvailableVersion.ToString();
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
    }
}