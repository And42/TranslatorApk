using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TranslatorApk.Annotations;
using TranslatorApk.Logic;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.WebServices;
using TranslatorApk.Properties;
using UsefulFunctionsLib;

using Res = TranslatorApk.Resources.Localizations.Resources;

namespace TranslatorApk.Windows
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public sealed partial class SettingsWindow : INotifyPropertyChanged
    {   
        public string OtherFileExts
        {
            get
            {
                return _otherFileExts;
            }
            set
            {
                _otherFileExts = value;
                OnPropertyChanged(nameof(OtherFileExts));
            }
        }
        private string _otherFileExts = string.Empty;

        public string ImageFileExts
        {
            get
            {
                return _imageFileExts;
            }
            set
            {
                _imageFileExts = value;
                OnPropertyChanged(nameof(ImageFileExts));
            }
        }
        private string _imageFileExts = string.Empty;

        public IEnumerable<string> YesNoItems => new[] { Res.Yes, Res.No };

        public int LanguageOfAppIndex { 
            get
            {
                return TranslateService.SupportedProgramLangs.IndexOf(SettingsIncapsuler.LanguageOfApp);
            }
            set
            {
                Functions.SetLanguageOfApp(TranslateService.SupportedProgramLangs[value], true);
            } 
        }

        public ObservableCollection<string> ApktoolVersions { get; } = new ObservableCollection<string>(); 

        public ObservableCollection<string> Themes { get; } = new ObservableCollection<string>(); 

        public OneTranslationService OnlineTranslator
        {
            get { return GlobalVariables.CurrentTranslationService; }
            set
            {
                GlobalVariables.CurrentTranslationService = value;
                SettingsIncapsuler.OnlineTranslator = value.Guid;
                OnPropertyChanged(nameof(OnlineTranslator));
            } 
        }

        public ObservableCollection<OneTranslationService> Translators { get; } = new ObservableCollection<OneTranslationService>();

        public int TopMostIndex
        {
            get { return SettingsIncapsuler.TopMost ? 0 : 1; }
            set { SettingsIncapsuler.TopMost = value == 0; }
        }

        public int AlternativeEditingKeysIndex
        {
            get { return SettingsIncapsuler.AlternativeEditingKeys ? 0 : 1; }
            set { SettingsIncapsuler.AlternativeEditingKeys = value == 0; }
        }

        public int SessionAutoTranslateIndex
        {
            get { return SettingsIncapsuler.SessionAutoTranslate ? 0 : 1; }
            set { SettingsIncapsuler.SessionAutoTranslate = value == 0; }
        }

        public int ShowNotificationsIndex
        {
            get { return SettingsIncapsuler.ShowNotifications ? 0 : 1; }
            set { SettingsIncapsuler.ShowNotifications = value == 0; }
        }

        public int ShowPreviewsIndex
        {
            get { return SettingsIncapsuler.ShowPreviews ? 0 : 1; }
            set { SettingsIncapsuler.ShowPreviews = value == 0; }
        }

        public int AlternateRowsIndex
        {
            get { return SettingsIncapsuler.AlternatingRows ? 0 : 1; }
            set { SettingsIncapsuler.AlternatingRows = value == 0; }
        }

        public SettingsWindow()
        {
            InitializeComponent();
            OtherFileExts = SettingsIncapsuler.OtherExtensions.JoinStr("|");
            ImageFileExts = SettingsIncapsuler.ImageExtensions.JoinStr("|");
            LoadApktools();

            Themes.AddRange(GlobalVariables.ThemesMap.Select(it => it.Value));

            ThemeBox.SelectedItem = GlobalVariables.ThemesMap.Forward[Settings.Default.Theme];

            Translators.AddRange(TranslateService.OnlineTranslators.Values);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SaveOtherExts(object sender, RoutedEventArgs e)
        {
            SettingsIncapsuler.OtherExtensions = OtherFileExts.SplitFR("|");
        }

        private void SaveImageExts(object sender, RoutedEventArgs e)
        {
            SettingsIncapsuler.ImageExtensions = ImageFileExts.SplitFR("|");
        }

        private void ThemeChanged(object sender, SelectionChangedEventArgs e)
        {
            string changed = e.AddedItems.Count > 0 ? e.AddedItems[0].As<string>() : null;

            if (changed != null)
                Functions.ChangeTheme(GlobalVariables.ThemesMap.Reverse[changed]);
        }

        private void LoadApktools()
        {
            ApktoolVersions.Clear();
            ApktoolVersions.AddRange(Directory.EnumerateFiles(GlobalVariables.PathToApktoolVersions).Select(Path.GetFileNameWithoutExtension).Select(s => s.Split('_')).Where(split => split.Length == 2).Select(split => split[1]));
            ApktoolVersions.Add(Res.Catalog);
            ApktoolVersionBox.SelectedItem = Settings.Default.ApktoolVersion;
        }

        private void ApktoolVersionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;

            string version = e.AddedItems[0].As<string>();
            if (version == Res.Catalog)
            {
                if (!Settings.Default.ApktoolVersion.NE())
                    sender.As<ComboBox>().SelectedItem = Settings.Default.ApktoolVersion;

                new ApktoolCatalogWindow().ShowDialog();

                LoadApktools();

                return;
            }

            Settings.Default.ApktoolVersion = version;
            Settings.Default.Save();
        }
    }
}
