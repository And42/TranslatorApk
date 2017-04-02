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
using TranslatorApk.Annotations;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.OrganisationItems;
using UsefulFunctionsLib;

using StringResources = TranslatorApk.Resources.Localizations.Resources;

namespace TranslatorApk.Windows
{
    /// <summary>
    /// Логика взаимодействия для ApktoolCatalogWindow.xaml
    /// </summary>
    public partial class ApktoolCatalogWindow : INotifyPropertyChanged
    {
        public class DownloadableApktool : INotifyPropertyChanged
        {
            public string Version { get; set; }
            public string Size { get; set; }
            public string Link { get; set; }

            public InstallOptionsEnum Installed
            {
                get { return _installed; }
                set
                {
                    if (value == _installed)
                        return;

                    _installed = value;
                    OnPropertyChanged1(nameof(Installed));
                }
            }
            private InstallOptionsEnum _installed;

            public event PropertyChangedEventHandler PropertyChanged;

            [NotifyPropertyChangedInvocator]
            protected virtual void OnPropertyChanged1(string propertyName)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public int Progress
        {
            get { return _progress; }
            set
            {
                if (_progress == value) return;
                _progress = value;
                OnPropertyChanged(nameof(Progress));
            }
        }
        private int _progress;

        public int ProgressMax
        {
            get { return _progressMax; }
            set
            {
                if (_progressMax == value) return;
                _progressMax = value;
                OnPropertyChanged(nameof(ProgressMax));
            }
        }
        private int _progressMax = 100;

        public Visibility ProgressBarVisibility
        {
            get { return _progressBarVisibility; }
            set
            {
                if (_progressBarVisibility == value) return;
                _progressBarVisibility = value;
                OnPropertyChanged(nameof(ProgressBarVisibility));
            }
        }
        private Visibility _progressBarVisibility = Visibility.Collapsed;

        private WebClient client;
        private string downloadingApktoolPath;

        public ObservableCollection<DownloadableApktool> ServerApktools { get; } = new ObservableCollection<DownloadableApktool>();

        public ApktoolCatalogWindow()
        {
            InitializeComponent();
        }

        private void DownloadClick(object sender, RoutedEventArgs e)
        {
            var apktool = sender.As<Button>().DataContext.As<DownloadableApktool>();

            downloadingApktoolPath = $"{GlobalVariables.PathToApktoolVersions}\\apktool_{apktool.Version}.jar";

            if (apktool.Installed == InstallOptionsEnum.ToUninstall)
            {
                try
                {
                    File.Delete(downloadingApktoolPath);
                    apktool.Installed = InstallOptionsEnum.ToInstall;
                }
                catch (UnauthorizedAccessException)
                {
                    if (Functions.RunAsAdmin(GlobalVariables.PathToAdminScripter,
                        $"\"delete file|{downloadingApktoolPath}\"", out Process process))
                    {
                        process.WaitForExit();
                        apktool.Installed = InstallOptionsEnum.ToInstall;
                    }
                }
                
                return;
            }

            if (!Functions.CheckRights())
            {
                if (Functions.RunAsAdmin(GlobalVariables.PathToAdminScripter,
                    $"\"download|{apktool.Link}|{downloadingApktoolPath}\"", out Process process))
                {
                    process.WaitForExit();
                    apktool.Installed = InstallOptionsEnum.ToUninstall;
                }

                return;
            }

            client = new WebClient();

            Progress = 0;

            client.DownloadProgressChanged += (o, args) =>
            {
                Progress = args.ProgressPercentage;
            };

            client.DownloadFileCompleted += (o, args) =>
            {
                if (args.Cancelled)
                {
                    File.Delete(downloadingApktoolPath);
                }
                else
                {
                    apktool.Installed = InstallOptionsEnum.ToUninstall;
                }

                ProgressBarVisibility = Visibility.Collapsed;
            };

            ProgressBarVisibility = Visibility.Visible;

            client.DownloadFileAsync(new Uri(apktool.Link), downloadingApktoolPath);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private async void ApktoolCatalogWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            string page;

            try
            {
                page = await Functions.DownloadStringAsync("https://bitbucket.org/iBotPeaches/apktool/downloads");
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
    }
}
