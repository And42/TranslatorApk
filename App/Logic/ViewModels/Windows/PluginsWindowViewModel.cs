using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using MVVM_Tools.Code.Classes;
using MVVM_Tools.Code.Commands;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.Utils;
using TranslatorApk.Resources.Localizations;
using TranslatorApk.Windows;

namespace TranslatorApk.Logic.ViewModels.Windows
{
    public class PluginsWindowViewModel : ViewModelBase
    {
        public class TableItem : BindableBase
        {
            [XmlElement("title")]
            public string Title { get; set; }

            [XmlElement("description")]
            public string Description { get; set; }

            [XmlElement("link")]
            public string Link { get; set; }

            [XmlElement("dllName")]
            public string DllName { get; set; }

            [XmlElement("latestVersion")]
            public string LatestVersion { get; set; }

            public string Version
            {
                get => _version;
                set => SetProperty(ref _version, value);
            }
            private string _version;

            public InstallOptionsEnum Installed
            {
                get => _installed;
                set => SetProperty(ref _installed, value);
            }
            private InstallOptionsEnum _installed;
        }

        [XmlRoot("items")]
        public class ServerPlugins
        {
            [XmlElement("item")]
            public List<TableItem> Items { get; set; }
        }

        private const string PluginsLink = WebUtils.ServerPluginsDirLink + "/Plugins.xml";

        public ObservableRangeCollection<TableItem> TableItems { get; } = new ObservableRangeCollection<TableItem>();

        public IActionCommand<TableItem> ItemClickedCommand { get; }

        public PluginsWindowViewModel()
        {
            ItemClickedCommand = new ActionCommand<TableItem>(ItemClickedCommand_Execute, _ => !IsBusy);

            PropertyChanged += OnPropertyChanged;
        }

        public override async Task LoadItems()
        {
            using (BusyDisposable())
            {
                string plugs;

                try
                {
                    plugs = await WebUtils.DownloadStringAsync(PluginsLink, WebUtils.DefaultTimeout);
                }
                catch (Exception)
                {
                    MessBox.ShowDial(StringResources.CanNotRecievePluginsList, StringResources.ErrorLower);
                    return;
                }

                ServerPlugins plugins = await Task.Factory.StartNew(() =>
                {
                    Dictionary<string, string> existingPlugins = Directory.Exists(GlobalVariables.PathToPlugins)
                        ? Directory.EnumerateFiles(GlobalVariables.PathToPlugins, "*.dll")
                            .ToDictionary(Path.GetFileNameWithoutExtension, it => it)
                        : new Dictionary<string, string>();

                    var splugins = CommonUtils.DeserializeXml<ServerPlugins>(plugs);

                    splugins.Items.ForEach(v =>
                    {
                        string version = existingPlugins.ContainsKey(v.DllName)
                            ? CommonUtils.GetDllVersion(existingPlugins[v.DllName])
                            : null;

                        v.Installed = version != null
                            ? v.LatestVersion == null
                                ? InstallOptionsEnum.ToUninstall
                                : (CommonUtils.CompareVersions(v.LatestVersion, version) == 1
                                    ? InstallOptionsEnum.ToUpdate
                                    : InstallOptionsEnum.ToUninstall)
                            : InstallOptionsEnum.ToInstall;
                        v.Version = version ?? "";
                    });

                    return splugins;
                });

                TableItems.ReplaceRange(plugins.Items);
            }
        }

        private async void ItemClickedCommand_Execute(TableItem item)
        {
            switch (item.Installed)
            {
                case InstallOptionsEnum.ToUninstall:
                    await UninstallPlugin(item);
                    break;
                case InstallOptionsEnum.ToInstall:
                    await InstallPlugin(item);
                    break;
                case InstallOptionsEnum.ToUpdate:
                    await UninstallPlugin(item);
                    await InstallPlugin(item);
                    break;
            }
        }

        private async Task InstallPlugin(TableItem item)
        {
            IsBusy = true;

            await Task.Factory.StartNew(() =>
            {
                string zipPath = Path.Combine(GlobalVariables.PathToPlugins, $"{item.Title}.zip");
                string arguments = $"\"download|{WebUtils.ServerPluginsDirLink}/{item.Link}.zip" + $"|{zipPath}\" \"unzip|{zipPath}|{GlobalVariables.PathToPlugins}\" \"delete file|{zipPath}\"";

                if (CommonUtils.CheckRights())
                {
                    Process.Start(GlobalVariables.PathToAdminScripter, arguments)?.WaitForExit();

                    string dllPath = Path.Combine(GlobalVariables.PathToPlugins, $"{item.DllName}.dll");

                    item.Installed = InstallOptionsEnum.ToUninstall;
                    item.Version = item.LatestVersion;

                    PluginUtils.LoadPlugin(dllPath);
                }
                else if (CommonUtils.RunAsAdmin(GlobalVariables.PathToAdminScripter, arguments, out var process))
                {
                    process.WaitForExit();

                    item.Installed = InstallOptionsEnum.ToUninstall;
                    item.Version = item.LatestVersion;
                    PluginUtils.LoadPlugin(Path.Combine(GlobalVariables.PathToPlugins, $"{item.DllName}.dll"));
                }
            });

            IsBusy = false;
        }

        private async Task UninstallPlugin(TableItem item)
        {
            IsBusy = true;

            await Task.Factory.StartNew(() =>
            {
                string dllName = Path.Combine(GlobalVariables.PathToPlugins, $"{item.DllName}.dll");
                string dirName = Path.Combine(GlobalVariables.PathToPlugins, item.DllName);

                PluginUtils.UnloadPlugin(item.DllName);

                try
                {
                    IOUtils.DeleteFile(dllName);
                    IOUtils.DeleteFolder(dirName);
                }
                catch (UnauthorizedAccessException)
                {
                    string command = $"\"delete file|{dllName}\" \"delete folder|{dirName}\"";

                    if (!CommonUtils.RunAsAdmin(GlobalVariables.PathToAdminScripter, command, out var process))
                        return;

                    process.WaitForExit();
                }

                item.Installed = InstallOptionsEnum.ToInstall;
                item.Version = "";
            });

            IsBusy = false;
        }

        private void RaiseCommandsCanExecute()
        {
            ItemClickedCommand.RaiseCanExecuteChanged();
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

        public override void UnsubscribeFromEvents()
        {
            PropertyChanged -= OnPropertyChanged;
        }
    }
}
