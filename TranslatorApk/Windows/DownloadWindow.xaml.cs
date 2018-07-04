using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.Utils;
using TranslatorApk.Resources.Localizations;

namespace TranslatorApk.Windows
{
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
                IOUtils.DeleteFile("NewVersion.update");
            }
        }

        private void client_DownloadFileCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (_close)
                return;

            try
            {
                IOUtils.DeleteFile("NewVersion.exe");

                File.Move("NewVersion.update", "NewVersion.exe");
                Process.Start("NewVersion.exe");
                Environment.Exit(0);
            }
            catch (Exception)
            {
                MessBox.ShowDial(StringResources.CantUpdateProgram, StringResources.ErrorLower);
                Close();
            }
        }

        private void client_DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            ProcessBar.Value = e.ProgressPercentage;
            ProgressBlock.Text = e.ProgressPercentage + "%";
        }

        private void DownloadWindow_Load(object sender, EventArgs e)
        {
            UpdateApplication();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            _close = true;
        }

    }
}
