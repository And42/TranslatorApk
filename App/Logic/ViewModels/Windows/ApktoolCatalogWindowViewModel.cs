using System;
using System.Collections.Generic;
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
        private readonly WebClient _webClient = new WebClient();

        public ObservableRangeCollection<DownloadableApktool> ServerApktools { get; } = new ObservableRangeCollection<DownloadableApktool>();

        public Property<int> Progress { get; } = new Property<int>();
        public Property<int> ProgressMax { get; } = new Property<int>(100);
        public Property<bool> ProgressBarIsVisible { get; } = new Property<bool>();

        public IActionCommand<DownloadableApktool> ItemClickedCommand { get; }

        public ApktoolCatalogWindowViewModel()
        {
            _webClient.DownloadProgressChanged += (sender, args) => Progress.Value = args.ProgressPercentage;

            ItemClickedCommand = new ActionCommand<DownloadableApktool>(ItemClickedCommand_Execute, _ => !IsBusy);

            PropertyChanged += OnPropertyChanged;
        }

        public override async Task LoadItems()
        {
            using (BusyDisposable())
            {
                try
                {
                    List<DownloadableApktool> items = await DownloadApktoolsListAsync();
                    ServerApktools.ReplaceRange(items);
                }
                catch (Exception)
                {
                    MessBox.ShowDial(StringResources.CanNotRecieveApktoolsList, StringResources.ErrorLower);
                }
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
                        if (CommonUtils.RunAsAdmin(GlobalVariables.PathToAdminScripter,
                            $"\"delete file|{downloadingApktoolPath}\"", out Process process))
                        {
                            process.WaitForExit();
                            item.Installed = InstallOptionsEnum.ToInstall;
                        }
                    }

                    return;
                }

                if (!CommonUtils.CheckRights())
                {
                    if (CommonUtils.RunAsAdmin(GlobalVariables.PathToAdminScripter,
                        $"\"download|{item.Link}|{downloadingApktoolPath}\"", out Process process))
                    {
                        process.WaitForExit();
                        item.Installed = InstallOptionsEnum.ToUninstall;
                    }

                    return;
                }

                Progress.Value = 0;

                await _webClient.DownloadFileTaskAsync(
                    new Uri(item.Link),
                    downloadingApktoolPath
                );

                item.Installed = InstallOptionsEnum.ToUninstall;
            }
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
            string page = await WebUtils.DownloadStringAsync("https://bitbucket.org/iBotPeaches/apktool/downloads", WebUtils.DefaultTimeout);

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
