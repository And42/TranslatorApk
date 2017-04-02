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
using TranslatorApk.Annotations;
using TranslatorApk.Logic;
using TranslatorApk.Logic.Classes;
using UsefulFunctionsLib;

using StringResources = TranslatorApk.Resources.Localizations.Resources;

namespace TranslatorApk.Windows
{
    /// <summary>
    /// Логика взаимодействия для Plugins.xaml
    /// </summary>
    public partial class Plugins : INotifyPropertyChanged
    {
        private const string PluginsLink = "http://things.pixelcurves.info/Pages/TranslatorApkPlugins.aspx?file=Plugins.xml";

        public ObservableCollection<TableItem> TableItems { get; } = new ObservableCollection<TableItem>();

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

        public class TableItem : INotifyPropertyChanged
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
                get { return _version; }
                set
                {
                    if (_version == value) return;
                    _version = value;
                    OnPropertyChanged1(nameof(Version));
                }
            }
            private string _version;

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
                plugs = await Functions.DownloadStringAsync(PluginsLink);
            }
            catch (Exception)
            {
                MessBox.ShowDial(StringResources.CanNotRecievePluginsList, StringResources.ErrorLower);
                Close();
                return;
            }

            ServerPlugins splugins = Functions.DeserializeXml<ServerPlugins>(plugs);

            splugins.Items.ForEach(v =>
            {
                string version = existingPlugins.ContainsKey(v.DllName)
                    ? Functions.GetDllVersion(existingPlugins[v.DllName])
                    : null;

                v.Installed = version != null 
                    ? v.LatestVersion == null 
                        ? InstallOptionsEnum.ToUninstall 
                        : (Functions.CompareVersions(v.LatestVersion, version) == 1 
                            ? InstallOptionsEnum.ToUpdate
                            : InstallOptionsEnum.ToUninstall) 
                    : InstallOptionsEnum.ToInstall;
                v.Version = version ?? "";
            });

            TableItems.AddRange(splugins.Items);
        }

        private static void InstallPlugin(TableItem item)
        {
            string zipPath = $"{GlobalVariables.PathToPlugins}\\{item.Title}.zip";
            string arguments = $"\"download|http://things.pixelcurves.info/Pages/TranslatorApkPlugins.aspx?file={item.Link}.zip" + $"|{zipPath}\" \"unzip|{zipPath}|{GlobalVariables.PathToPlugins}\" \"delete file|{zipPath}\"";

            if (Functions.CheckRights())
            {
                Process.Start(GlobalVariables.PathToAdminScripter, arguments)?.WaitForExit();

                string dllPath = $"{GlobalVariables.PathToPlugins}\\{item.DllName}.dll";

                item.Installed = InstallOptionsEnum.ToUninstall;
                item.Version = item.LatestVersion;

                Functions.LoadPlugin(dllPath);
            }
            else if (Functions.RunAsAdmin(GlobalVariables.PathToAdminScripter, arguments, out var process))
            {
                process.WaitForExit();

                item.Installed = InstallOptionsEnum.ToUninstall;
                item.Version = item.LatestVersion;
                Functions.LoadPlugin($"{GlobalVariables.PathToPlugins}\\{item.DllName}.dll");
            }
        }

        private static void UninstallPlugin(TableItem item)
        {
            string dllName = $"{GlobalVariables.PathToPlugins}\\{item.DllName}.dll";
            string dirName = $"{GlobalVariables.PathToPlugins}\\{item.DllName}";

            Functions.UnloadPlugin(item.DllName);

            try
            {
                File.Delete(dllName);
                Directory.Delete(dirName, true);
            }
            catch (UnauthorizedAccessException)
            {
                string command = $"\"delete file|{dllName}\" \"delete folder|{dirName}\"";

                if (!Functions.RunAsAdmin(GlobalVariables.PathToAdminScripter, command, out var process))
                    return;

                process.WaitForExit();
            }

            item.Installed = InstallOptionsEnum.ToInstall;
            item.Version = "";
        }

        private void DownloadClick(object sender, RoutedEventArgs e)
        {
            TableItem item = sender.As<Button>().DataContext.As<TableItem>();

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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
