using System.ComponentModel;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AndroidHelper.Logic;
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
        private readonly Window _window;
        
        private readonly string[] _arguments;
        private readonly StringBuilder _logTextBuilder = new StringBuilder();

        public static Logger AndroidLogger;

        public Setting<bool>[] MainWindowSettings { get; private set; }

        public string LogBoxText => _logTextBuilder.ToString();

        public Property<WindowState> MainWindowState { get; private set; }

        public Apktools Apk;

        public MainWindowViewModel(string[] args, Window window)
        {
            _arguments = args;
            _window = window;

            BindProperty(() => MainWindowState);

            InitSettings();
            InitCommon();

            PropertyChanged += OnPropertyChanged;
            DefaultSettingsContainer.Instance.PropertyChanged += SettingsOnPropertyChanged;
        }

        private async void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            await TVSettingsOnPropertyChanged(args);
        }

        private void InitSettings()
        {
            MainWindowSettings = new[]
            {
                new Setting<bool>(nameof(DefaultSettingsContainer.Instance.EmptyXml),        StringResources.EmptyXml),
                new Setting<bool>(nameof(DefaultSettingsContainer.Instance.EmptySmali),      StringResources.EmptySmali),
                new Setting<bool>(nameof(DefaultSettingsContainer.Instance.EmptyFolders),    StringResources.EmptyFolders),
                new Setting<bool>(nameof(DefaultSettingsContainer.Instance.Images),          StringResources.Images),
                new Setting<bool>(nameof(DefaultSettingsContainer.Instance.FilesWithErrors), StringResources.FilesWithErrors),
                new Setting<bool>(nameof(DefaultSettingsContainer.Instance.OnlyXml),         StringResources.OnlyXml),
                new Setting<bool>(nameof(DefaultSettingsContainer.Instance.OtherFiles),      StringResources.OtherFiles),
                new Setting<bool>(nameof(DefaultSettingsContainer.Instance.OnlyResources),   StringResources.OnlyResources)
            };
        }

        private async void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(MainWindowState):
                    if (MainWindowState.Value != WindowState.Minimized)
                        DefaultSettingsContainer.Instance.MainWMaximized = MainWindowState.Value == WindowState.Maximized;
                    break;
            }

            await TVPropertyChanged(args);
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
            DefaultSettingsContainer.Instance.PropertyChanged -= SettingsOnPropertyChanged;
        }

        public override void Dispose()
        {
            UnsubscribeFromEvents();
            DisposeTreeViewPart();
        }
    }
}
