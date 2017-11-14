using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Serialization;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.Interfaces;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.Utils;
using UsefulFunctionsLib;

using StringResources = TranslatorApk.Resources.Localizations.Resources;

namespace TranslatorApk.Windows
{
    /// <summary>
    /// Логика взаимодействия для Plugins.xaml
    /// </summary>
    public partial class Plugins : IRaisePropertyChanged
    {
        private const string PluginsLink = "http://things.pixelcurves.info/Pages/TranslatorApkPlugins.aspx?file=Plugins.xml";

        public ObservableCollection<TableItem> TableItems { get; } = new ObservableCollection<TableItem>();

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

        public Plugins()
        {
            InitializeComponent();
        }

        private async void Plugins_OnLoaded(object sender, RoutedEventArgs e)
        {
            Dictionary<string, string> existingPlugins = Directory.Exists(GlobalVariables.PathToPlugins) 
                ? Directory.EnumerateFiles(GlobalVariables.PathToPlugins, "*.dll").ToDictionary(Path.GetFileNameWithoutExtension, it => it)
                : new Dictionary<string, string>();

            string plugs;

            try
            {
                plugs = await Utils.DownloadStringAsync(PluginsLink);
            }
            catch (Exception)
            {
                MessBox.ShowDial(StringResources.CanNotRecievePluginsList, StringResources.ErrorLower);
                Close();
                return;
            }

            ServerPlugins splugins = Utils.DeserializeXml<ServerPlugins>(plugs);

            splugins.Items.ForEach(v =>
            {
                string version = existingPlugins.ContainsKey(v.DllName)
                    ? Utils.GetDllVersion(existingPlugins[v.DllName])
                    : null;

                v.Installed = version != null 
                    ? v.LatestVersion == null 
                        ? InstallOptionsEnum.ToUninstall 
                        : (Utils.CompareVersions(v.LatestVersion, version) == 1 
                            ? InstallOptionsEnum.ToUpdate
                            : InstallOptionsEnum.ToUninstall) 
                    : InstallOptionsEnum.ToInstall;
                v.Version = version ?? "";
            });

            TableItems.AddRange(splugins.Items);
        }

        private void DownloadClick(object sender, RoutedEventArgs e)
        {
            var item = sender.As<Button>().DataContext.As<TableItem>();

            switch (item.Installed)
            {
                case InstallOptionsEnum.ToUninstall:
                    UninstallPlugin(item);
                    break;
                case InstallOptionsEnum.ToInstall:
                    InstallPlugin(item);
                    break;
                case InstallOptionsEnum.ToUpdate:
                    UninstallPlugin(item);
                    InstallPlugin(item);
                    break;
            }
        }

        private static void InstallPlugin(TableItem item)
        {
            string zipPath = Path.Combine(GlobalVariables.PathToPlugins, $"{item.Title}.zip");
            string arguments = $"\"download|http://things.pixelcurves.info/Pages/TranslatorApkPlugins.aspx?file={item.Link}.zip" + $"|{zipPath}\" \"unzip|{zipPath}|{GlobalVariables.PathToPlugins}\" \"delete file|{zipPath}\"";

            if (Utils.CheckRights())
            {
                Process.Start(GlobalVariables.PathToAdminScripter, arguments)?.WaitForExit();

                string dllPath = Path.Combine(GlobalVariables.PathToPlugins, $"{item.DllName}.dll");

                item.Installed = InstallOptionsEnum.ToUninstall;
                item.Version = item.LatestVersion;

                PluginUtils.LoadPlugin(dllPath);
            }
            else if (Utils.RunAsAdmin(GlobalVariables.PathToAdminScripter, arguments, out var process))
            {
                process.WaitForExit();

                item.Installed = InstallOptionsEnum.ToUninstall;
                item.Version = item.LatestVersion;
                PluginUtils.LoadPlugin(Path.Combine(GlobalVariables.PathToPlugins, $"{item.DllName}.dll"));
            }
        }

        private static void UninstallPlugin(TableItem item)
        {
            string dllName = Path.Combine(GlobalVariables.PathToPlugins, $"{item.DllName}.dll");
            string dirName = Path.Combine(GlobalVariables.PathToPlugins, item.DllName);

            PluginUtils.UnloadPlugin(item.DllName);

            try
            {
                File.Delete(dllName);
                Directory.Delete(dirName, true);
            }
            catch (UnauthorizedAccessException)
            {
                string command = $"\"delete file|{dllName}\" \"delete folder|{dirName}\"";

                if (!Utils.RunAsAdmin(GlobalVariables.PathToAdminScripter, command, out var process))
                    return;

                process.WaitForExit();
            }

            item.Installed = InstallOptionsEnum.ToInstall;
            item.Version = "";
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
