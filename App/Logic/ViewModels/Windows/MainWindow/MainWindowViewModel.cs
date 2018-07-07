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

        private readonly AppSettingsBase _appSettings = GlobalVariables.AppSettings;
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

        private async void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(MainWindowState):
                    if (MainWindowState.Value != WindowState.Minimized)
                        _appSettings.MainWMaximized = MainWindowState.Value == WindowState.Maximized;
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
            _appSettings.PropertyChanged -= SettingsOnPropertyChanged;
        }

        public override void Dispose()
        {
            UnsubscribeFromEvents();
            DisposeTreeViewPart();
        }
    }
}
