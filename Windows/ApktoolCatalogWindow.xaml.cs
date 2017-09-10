using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using HtmlAgilityPack;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.Interfaces;
using TranslatorApk.Logic.OrganisationItems;
using UsefulFunctionsLib;

using StringResources = TranslatorApk.Resources.Localizations.Resources;

namespace TranslatorApk.Windows
{
    /// <summary>
    /// Логика взаимодействия для ApktoolCatalogWindow.xaml
    /// </summary>
    public partial class ApktoolCatalogWindow : IRaisePropertyChanged
    {
        public class DownloadableApktool : IRaisePropertyChanged
        {
            public string Version { get; set; }
            public string Size { get; set; }
            public string Link { get; set; }

            public InstallOptionsEnum Installed
            {
                get => _installed;
                set => this.SetProperty(ref _installed, value);
            }
            private InstallOptionsEnum _installed;

            public event PropertyChangedEventHandler PropertyChanged;

            public void RaisePropertyChanged(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public int Progress
        {
            get => _progress;
            set => this.SetProperty(ref _progress, value);
        }
        private int _progress;

        public int ProgressMax
        {
            get => _progressMax;
            set => this.SetProperty(ref _progressMax, value);
        }
        private int _progressMax = 100;

        public Visibility ProgressBarVisibility
        {
            get => _progressBarVisibility;
            set => this.SetProperty(ref _progressBarVisibility, value);
        }
        private Visibility _progressBarVisibility = Visibility.Collapsed;

        private WebClient _client;
        private string _downloadingApktoolPath;

        public ObservableCollection<DownloadableApktool> ServerApktools { get; } = new ObservableCollection<DownloadableApktool>();

        public ApktoolCatalogWindow()
        {
            InitializeComponent();
        }

        private void DownloadClick(object sender, RoutedEventArgs e)
        {
            var apktool = sender.As<Button>().DataContext.As<DownloadableApktool>();

            _downloadingApktoolPath = Path.Combine(GlobalVariables.PathToApktoolVersions, $"apktool_{apktool.Version}.jar");

            if (apktool.Installed == InstallOptionsEnum.ToUninstall)
            {
                try
                {
                    File.Delete(_downloadingApktoolPath);
                    apktool.Installed = InstallOptionsEnum.ToInstall;
                }
                catch (UnauthorizedAccessException)
                {
                    if (Utils.RunAsAdmin(GlobalVariables.PathToAdminScripter,
                        $"\"delete file|{_downloadingApktoolPath}\"", out Process process))
                    {
                        process.WaitForExit();
                        apktool.Installed = InstallOptionsEnum.ToInstall;
                    }
                }
                
                return;
            }

            if (!Utils.CheckRights())
            {
                if (Utils.RunAsAdmin(GlobalVariables.PathToAdminScripter,
                    $"\"download|{apktool.Link}|{_downloadingApktoolPath}\"", out Process process))
                {
                    process.WaitForExit();
                    apktool.Installed = InstallOptionsEnum.ToUninstall;
                }

                return;
            }

            _client = new WebClient();

            Progress = 0;

            _client.DownloadProgressChanged += (o, args) =>
            {
                Progress = args.ProgressPercentage;
            };

            _client.DownloadFileCompleted += (o, args) =>
            {
                if (args.Cancelled)
                {
                    File.Delete(_downloadingApktoolPath);
                }
                else
                {
                    apktool.Installed = InstallOptionsEnum.ToUninstall;
                }

                ProgressBarVisibility = Visibility.Collapsed;
            };

            ProgressBarVisibility = Visibility.Visible;

            _client.DownloadFileAsync(new Uri(apktool.Link), _downloadingApktoolPath);
        }

        private async void ApktoolCatalogWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            string page;

            try
            {
                page = await Utils.DownloadStringAsync("https://bitbucket.org/iBotPeaches/apktool/downloads");
            }
            catch
            {
                MessBox.ShowDial(StringResources.CanNotRecieveApktoolsList, StringResources.ErrorLower);
                Close();
                return;
            }

            HashSet<string> installed = new HashSet<string>(Directory.EnumerateFiles(GlobalVariables.PathToApktoolVersions).Select(Path.GetFileNameWithoutExtension).Select(s => s.Split('_')[1]));

            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(page);

            var iterableItems = document.GetElementbyId("uploaded-files").SelectNodes("tbody[1]/tr[@class=\"iterable-item\"]");

            var items = iterableItems
                .Select(it =>
                    new
                    {
                        Version = GetVersion(it),
                        Size = it.SelectSingleNode("td[@class=\"size\"]").InnerText,
                        Link = "https://bitbucket.org" + it.SelectSingleNode("td[@class=\"name\"]/a").Attributes["href"].Value
                    })
                .Select(it =>
                    new DownloadableApktool
                    {
                        Version = it.Version,
                        Size = it.Size,
                        Link = it.Link,
                        Installed = installed.Contains(it.Version) ? InstallOptionsEnum.ToUninstall : InstallOptionsEnum.ToInstall
                    });

            ServerApktools.AddRange(items);
        }

        private static string GetVersion(HtmlNode node)
        {
            return Path.GetFileNameWithoutExtension(node.SelectSingleNode("td[@class=\"name\"]/a").InnerText.Split('_')[1]);
        }

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
