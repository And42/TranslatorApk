using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using AndroidLibs;
using MVVM_Tools.Code.Providers;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.Utils;
using TranslatorApk.Resources.Localizations;
using UsefulClasses;

namespace TranslatorApk.Logic.ViewModels.Windows.MainWindow
{
    internal partial class MainWindowViewModel : ViewModelBase
    {
        private readonly Dispatcher _windowDispatcher;
        private readonly Control _windowControl;
        
        private readonly string[] _arguments;
        private readonly StringBuilder _logTextBuilder = new StringBuilder();

        public static Logger AndroidLogger;

        public Setting<bool>[] MainWindowSettings { get; private set; }

        public string LogBoxText => _logTextBuilder.ToString();

        public PropertyProvider<WindowState> MainWindowState { get; }

        public Apktools Apk;

        public MainWindowViewModel(string[] args, Control window)
        {
            _arguments = args;
            _windowControl = window;
            _windowDispatcher = window.Dispatcher;

            MainWindowState = CreateProviderWithNotify<WindowState>(nameof(MainWindowState));

            InitSettings();
            InitCommon();

            PropertyChanged += OnPropertyChanged;
        }

        private void InitSettings()
        {
            MainWindowSettings = new[]
            {
                new Setting<bool>(nameof(SettingsIncapsuler.Instance.EmptyXml),        StringResources.EmptyXml),
                new Setting<bool>(nameof(SettingsIncapsuler.Instance.EmptySmali),      StringResources.EmptySmali),
                new Setting<bool>(nameof(SettingsIncapsuler.Instance.EmptyFolders),    StringResources.EmptyFolders),
                new Setting<bool>(nameof(SettingsIncapsuler.Instance.Images),          StringResources.Images),
                new Setting<bool>(nameof(SettingsIncapsuler.Instance.FilesWithErrors), StringResources.FilesWithErrors),
                new Setting<bool>(nameof(SettingsIncapsuler.Instance.OnlyXml),         StringResources.OnlyXml),
                new Setting<bool>(nameof(SettingsIncapsuler.Instance.OtherFiles),      StringResources.OtherFiles),
                new Setting<bool>(nameof(SettingsIncapsuler.Instance.OnlyResources),   StringResources.OnlyResources)
            };
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(MainWindowState):
                    if (MainWindowState.Value != WindowState.Minimized)
                        SettingsIncapsuler.Instance.MainWMaximized = MainWindowState.Value == WindowState.Maximized;
                    break;
            }
        }

        public override Task LoadItems()
        {
            if (_arguments.Length == 1)
            {
                string file = _arguments[0];
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
                            Utils.Utils.LoadFile(file);
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
            PropertyChanged -= OnPropertyChanged;
        }
    }
}
