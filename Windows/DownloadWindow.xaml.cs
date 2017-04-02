using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using TranslatorApk.Logic;

namespace TranslatorApk.Windows
{
    /// <summary>
    /// Логика взаимодействия для DownloadCS.xaml
    /// </summary>
    public partial class DownloadWindow
    {
        public DownloadWindow()
        {
            InitializeComponent();
        }

        private bool _close;

        private void UpdateApplication()
        {
            try
            {
                var webClient = new WebClient();
                webClient.DownloadProgressChanged += client_DownloadProgressChanged;
                webClient.DownloadFileCompleted += client_DownloadFileCompleted;
                webClient.Headers.Add("user-agent", GlobalVariables.MozillaAgent);
                var address = new Uri("http://things.pixelcurves.info/Pages/Updates.aspx?cmd=trapk_download");
                webClient.DownloadFileAsync(address, "NewVersion.update");
            }
            catch (Exception)
            {
                File.Delete("NewVersion.update");
            }
        }

        private void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (_close) return;
            try
            {
                if (File.Exists("NewVersion.exe")) File.Delete("NewVersion.exe");
                File.Move("NewVersion.update", "NewVersion.exe");
                Process.Start("NewVersion.exe");
                Environment.Exit(0);
            }
            catch (Exception)
            {
                MessBox.ShowDial(TranslatorApk.Resources.Localizations.Resources.CantUpdateProgram, TranslatorApk.Resources.Localizations.Resources.ErrorLower);
                Close();
            }
        }

        private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ProcessBar.Value = e.ProgressPercentage;
        }

        private void DownloadCS_Load(object sender, EventArgs e)
        {
            UpdateApplication();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _close = true;
        }

    }
}
