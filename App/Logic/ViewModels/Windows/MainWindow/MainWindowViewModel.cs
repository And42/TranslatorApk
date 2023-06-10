﻿using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AndroidHelper.Logic;
using MVVM_Tools.Code.Providers;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.Utils;
using TranslatorApk.Resources.Localizations;

namespace TranslatorApk.Logic.ViewModels.Windows.MainWindow
{
    internal partial class MainWindowViewModel : ViewModelBase
    {
        private const string LogLine = "------------------------------";

        private readonly Window _window;

        private readonly AppSettings _appSettings = GlobalVariables.AppSettings;
        private readonly GlobalVariables _globalVariables = GlobalVariables.Instance;

        private readonly StringBuilder _logTextBuilder = new StringBuilder();
        private StreamWriter _androidProcessLogger;

        public Setting<bool>[] MainWindowSettings { get; private set; }

        public string LogBoxText => _logTextBuilder.ToString();

        public FieldProperty<WindowState> MainWindowState { get; } = new();

        public Apktools Apk;

        public MainWindowViewModel(Window window)
        {
            _window = window;

            InitSettings();
            InitCommon();

            MainWindowState.PropertyChanged += (sender, args) =>
            {
                if (MainWindowState.Value != WindowState.Minimized)
                    _appSettings.MainWMaximized = MainWindowState.Value == WindowState.Maximized;
            };

            _appSettings.PropertyChanged += SettingsOnPropertyChanged;
        }

        private async void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            await TVSettingsOnPropertyChanged(args);
        }

        private void InitSettings()
        {
            MainWindowSettings = new[]
            {
                new Setting<bool>(s => s.EmptyXml,        StringResources.EmptyXml),
                new Setting<bool>(s => s.EmptySmali,      StringResources.EmptySmali),
                new Setting<bool>(s => s.EmptyFolders,    StringResources.EmptyFolders),
                new Setting<bool>(s => s.Images,          StringResources.Images),
                new Setting<bool>(s => s.FilesWithErrors, StringResources.FilesWithErrors),
                new Setting<bool>(s => s.OnlyXml,         StringResources.OnlyXml),
                new Setting<bool>(s => s.OtherFiles,      StringResources.OtherFiles),
                new Setting<bool>(s => s.OnlyResources,   StringResources.OnlyResources)
            };
        }

        public override Task LoadItems()
        {
            string[] arguments = Environment.GetCommandLineArgs().Skip(1).ToArray();

            if (arguments.Length == 1)
            {
                string file = arguments[0];
                string ext = Path.GetExtension(file);

                if (Directory.Exists(file))
                {
                    LoadFolder(file);
                }
                else if (File.Exists(file))
                {
                    switch (ext)
                    {
                        case ".xml":
                        case ".smali":
                            CommonUtils.LoadFile(file);
                            break;
                        case ".apk":
                            DecompileFile(file);
                            break;
                        case ".yml":
                            LoadFolder(Path.GetDirectoryName(file));
                            break;
                    }
                }
            }

            Task.Factory.StartNew(PluginUtils.LoadPlugins);

            return EmptyTask;
        }

        public override void UnsubscribeFromEvents()
        {
            _appSettings.PropertyChanged -= SettingsOnPropertyChanged;
        }

        public override void Dispose()
        {
            UnsubscribeFromEvents();
            DisposeTreeViewPart();
            DisposeLogger();
        }

        private void DisposeLogger()
        {
            if (_androidProcessLogger == null)
                return;

            _androidProcessLogger.Close();
            _androidProcessLogger = null;
        }

        ~MainWindowViewModel()
        {
            DisposeLogger();
        }
    }
}
