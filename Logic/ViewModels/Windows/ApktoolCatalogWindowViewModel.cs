using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using HtmlAgilityPack;
using MVVM_Tools.Code.Commands;
using MVVM_Tools.Code.Disposables;
using MVVM_Tools.Code.Providers;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.Utils;
using TranslatorApk.Resources.Localizations;
using TranslatorApk.Windows;

namespace TranslatorApk.Logic.ViewModels.Windows
{
    internal class ApktoolCatalogWindowViewModel : ViewModelBase
    {
        private readonly WebClient _client;

        public ReadOnlyObservableCollection<DownloadableApktool> ServerApktools { get; }
        private readonly ObservableRangeCollection<DownloadableApktool> _serverApktools;

        public PropertyProvider<int> Progress { get; }
        public PropertyProvider<int> ProgressMax { get; }
        public PropertyProvider<bool> ProgressBarIsVisible { get; }

        public IActionCommand<DownloadableApktool> ItemClickedCommand { get; }

        public ApktoolCatalogWindowViewModel()
        {
            Progress = CreateProviderWithNotify<int>(nameof(Progress));
            ProgressMax = CreateProviderWithNotify(nameof(ProgressMax), 100);
            ProgressBarIsVisible = CreateProviderWithNotify<bool>(nameof(ProgressBarIsVisible));

            _serverApktools = new ObservableRangeCollection<DownloadableApktool>();
            ServerApktools = new ReadOnlyObservableCollection<DownloadableApktool>(_serverApktools);

            _client = new WebClient();
            _client.DownloadProgressChanged += ClientOnDownloadProgressChanged;

            ItemClickedCommand = new ActionCommand<DownloadableApktool>(ItemClickedCommand_Execute, _ => !IsBusy);

            PropertyChanged += OnPropertyChanged;
        }

        public override async Task LoadItems()
        {
            using (BusyDisposable())
            {
                List<DownloadableApktool> items = await DownloadApktoolsListAsync();

                if (items == null)
                {
                    MessBox.ShowDial(StringResources.CanNotRecieveApktoolsList, StringResources.ErrorLower);
                    return;
                }

                _serverApktools.ReplaceRange(items);
            }
        }

        public override void UnsubscribeFromEvents()
        {
            PropertyChanged -= OnPropertyChanged;
        }

        private async void ItemClickedCommand_Execute(DownloadableApktool item)
        {
            if (item == null || !ServerApktools.Contains(item))
                return;

            using (BusyDisposable())
            using (ProgressBarDisposable())
            {
                string downloadingApktoolPath =
                    Path.Combine(GlobalVariables.PathToApktoolVersions, $"apktool_{item.Version}.jar");

                if (item.Installed == InstallOptionsEnum.ToUninstall)
                {
                    try
                    {
                        IOUtils.DeleteFile(downloadingApktoolPath);
                        item.Installed = InstallOptionsEnum.ToInstall;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        if (Utils.Utils.RunAsAdmin(GlobalVariables.PathToAdminScripter,
                            $"\"delete file|{downloadingApktoolPath}\"", out Process process))
                        {
                            process.WaitForExit();
                            item.Installed = InstallOptionsEnum.ToInstall;
                        }
                    }

                    return;
                }

                if (!Utils.Utils.CheckRights())
                {
                    if (Utils.Utils.RunAsAdmin(GlobalVariables.PathToAdminScripter,
                        $"\"download|{item.Link}|{downloadingApktoolPath}\"", out Process process))
                    {
                        process.WaitForExit();
                        item.Installed = InstallOptionsEnum.ToUninstall;
                    }

                    return;
                }

                Progress.Value = 0;

                await _client.DownloadFileTaskAsync(
                    new Uri(item.Link),
                    downloadingApktoolPath
                );

                item.Installed = InstallOptionsEnum.ToUninstall;
            }
        }

        private void ClientOnDownloadProgressChanged(object sender, DownloadProgressChangedEventArgs args)
        {
            Progress.Value = args.ProgressPercentage;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(IsBusy):
                    RaiseCommandsCanExecute();
                    break;
            }
        }

        private void RaiseCommandsCanExecute()
        {
            ItemClickedCommand.RaiseCanExecuteChanged();
        }

        private CustomBoolDisposable ProgressBarDisposable()
        {
            return new CustomBoolDisposable(val => ProgressBarIsVisible.Value = val);
        }

        private static async Task<List<DownloadableApktool>> DownloadApktoolsListAsync()
        {
            string page;

            try
            {
                page = await WebUtils.DownloadStringAsync("https://bitbucket.org/iBotPeaches/apktool/downloads", WebUtils.DefaultTimeout);
            }
            catch
            {
                return null;
            }

            return await Task.Factory.StartNew(() =>
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
        }

        private static string GetVersion(HtmlNode node)
        {
            return Path.GetFileNameWithoutExtension(node.SelectSingleNode("td[@class=\"name\"]/a").InnerText.Split('_')[1]);
        }
    }
}
