using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HtmlAgilityPack;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.CustomCommandContainers;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Windows;
using UsefulFunctionsLib;

namespace TranslatorApk.Logic.ViewModels.Windows
{
    public class ApktoolCatalogWindowViewModel : BindableBase
    {
        public static ApktoolCatalogWindowViewModel Instanse => InstanseLazy.Value;

        private static readonly Lazy<ApktoolCatalogWindowViewModel> InstanseLazy 
            = new Lazy<ApktoolCatalogWindowViewModel>(() => new ApktoolCatalogWindowViewModel());

        private bool _isLoading;

        private int _progress;
        private Visibility _progressBarVisibility = Visibility.Collapsed;

        private readonly WebClient _client;
        private string _downloadingApktoolPath;

        private readonly ObservableRangeCollection<DownloadableApktool> _serverApktools;

        private readonly ActionCommand<DownloadableApktool> _itemClickedCommand;

        private ApktoolCatalogWindowViewModel()
        {
            _serverApktools = new ObservableRangeCollection<DownloadableApktool>();

            ServerApktools = new ReadOnlyObservableCollection<DownloadableApktool>(_serverApktools);

            _client = new WebClient();

            _client.DownloadProgressChanged += ClientOnDownloadProgressChanged;
            _client.DownloadFileCompleted += ClientOnDownloadFileCompleted;

            _itemClickedCommand = new ActionCommand<DownloadableApktool>(ItemClickedCommandExecute);
        }

        public bool IsLoading
        {
            get => _isLoading;
            private set => SetProperty(ref _isLoading, value);
        }

        public int Progress
        {
            get => _progress;
            private set => SetProperty(ref _progress, value);
        }

        public int ProgressMax { get; } = 100;
        
        public Visibility ProgressBarVisibility
        {
            get => _progressBarVisibility;
            private set => SetProperty(ref _progressBarVisibility, value);
        }

        public ReadOnlyObservableCollection<DownloadableApktool> ServerApktools { get; }

        public ICommand ItemClickedCommand => _itemClickedCommand;

        public async Task LoadItems()
        {
            if (IsLoading)
                return;

            IsLoading = true;

            string page;

            try
            {
                page = await Utils.Utils.DownloadStringAsync("https://bitbucket.org/iBotPeaches/apktool/downloads", Utils.Utils.DefaultTimeout);
            }
            catch
            {
                MessBox.ShowDial(Resources.Localizations.Resources.CanNotRecieveApktoolsList, Resources.Localizations.Resources.ErrorLower);
                return;
            }

            var items = await Task.Factory.StartNew(() =>
            {
                var installed =
                    new HashSet<string>(
                        Directory.EnumerateFiles(GlobalVariables.PathToApktoolVersions)
                            .Select(Path.GetFileNameWithoutExtension)
                            .Select(s => s.Split('_')[1])
                    );

                var document = new HtmlDocument();
                document.LoadHtml(page);

                HtmlNodeCollection iterableItems = document.GetElementbyId("uploaded-files")
                    .SelectNodes("tbody[1]/tr[@class=\"iterable-item\"]");

                return iterableItems
                    .Select(it =>
                        new
                        {
                            Version = GetVersion(it),
                            Size = it.SelectSingleNode("td[@class=\"size\"]").InnerText,
                            Link = "https://bitbucket.org" +
                                   it.SelectSingleNode("td[@class=\"name\"]/a").Attributes["href"].Value
                        }
                    )
                    .Select(it =>
                        new DownloadableApktool
                        {
                            Version = it.Version,
                            Size = it.Size,
                            Link = it.Link,
                            Installed = installed.Contains(it.Version)
                                ? InstallOptionsEnum.ToUninstall
                                : InstallOptionsEnum.ToInstall
                        }
                    )
                    .ToList();
            });

            _serverApktools.ReplaceRange(items);

            IsLoading = false;
        }

        private void ItemClickedCommandExecute(DownloadableApktool item)
        {
            if (item == null || !ServerApktools.Contains(item))
                return;

            IsLoading = true;

            _downloadingApktoolPath = Path.Combine(GlobalVariables.PathToApktoolVersions, $"apktool_{item.Version}.jar");

            if (item.Installed == InstallOptionsEnum.ToUninstall)
            {
                try
                {
                    File.Delete(_downloadingApktoolPath);
                    item.Installed = InstallOptionsEnum.ToInstall;
                }
                catch (UnauthorizedAccessException)
                {
                    if (Utils.Utils.RunAsAdmin(GlobalVariables.PathToAdminScripter,
                        $"\"delete file|{_downloadingApktoolPath}\"", out Process process))
                    {
                        process.WaitForExit();
                        item.Installed = InstallOptionsEnum.ToInstall;
                    }
                }

                IsLoading = false;

                return;
            }

            if (!Utils.Utils.CheckRights())
            {
                if (Utils.Utils.RunAsAdmin(GlobalVariables.PathToAdminScripter,
                    $"\"download|{item.Link}|{_downloadingApktoolPath}\"", out Process process))
                {
                    process.WaitForExit();
                    item.Installed = InstallOptionsEnum.ToUninstall;
                }

                IsLoading = false;

                return;
            }

            Progress = 0;
            ProgressBarVisibility = Visibility.Visible;

            _client.DownloadFileAsync(new Uri(item.Link), _downloadingApktoolPath, item);
        }

        private void ClientOnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs args)
        {
            Progress = args.ProgressPercentage;
        }

        private void ClientOnDownloadFileCompleted(object sender, AsyncCompletedEventArgs args)
        {
            if (args.Cancelled)
            {
                File.Delete(_downloadingApktoolPath);
            }
            else
            {
                args.UserState.As<DownloadableApktool>().Installed = InstallOptionsEnum.ToUninstall;
            }

            ProgressBarVisibility = Visibility.Collapsed;

            IsLoading = false;
        }

        private static string GetVersion(HtmlNode node)
        {
            return Path.GetFileNameWithoutExtension(node.SelectSingleNode("td[@class=\"name\"]/a").InnerText.Split('_')[1]);
        }
    }
}
