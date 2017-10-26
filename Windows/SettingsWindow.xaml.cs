using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.Interfaces;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.Utils;
using TranslatorApk.Logic.WebServices;
using UsefulFunctionsLib;

using Res = TranslatorApk.Resources.Localizations.Resources;

namespace TranslatorApk.Windows
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public sealed partial class SettingsWindow : IRaisePropertyChanged
    {   
        public string OtherFileExts
        {
            get => _otherFileExts;
            set => this.SetProperty(ref _otherFileExts, value);
        }
        private string _otherFileExts = string.Empty;

        public string ImageFileExts
        {
            get => _imageFileExts;
            set => this.SetProperty(ref _imageFileExts, value);
        }
        private string _imageFileExts = string.Empty;

        public IEnumerable<string> YesNoItems => new[] { Res.Yes, Res.No };

        public int LanguageOfAppIndex
        { 
            get => TranslateService.SupportedProgramLangs.IndexOf(SettingsIncapsuler.Instance.LanguageOfApp);
            set => Utils.SetLanguageOfApp(TranslateService.SupportedProgramLangs[value], true);
        }

        public ObservableCollection<string> ApktoolVersions { get; } = new ObservableCollection<string>(); 

        public ObservableCollection<string> Themes { get; } = new ObservableCollection<string>(); 

        public OneTranslationService OnlineTranslator
        {
            get => GlobalVariables.CurrentTranslationService;
            set
            {
                GlobalVariables.CurrentTranslationService = value;
                SettingsIncapsuler.Instance.OnlineTranslator = value.Guid;
                RaisePropertyChanged(nameof(OnlineTranslator));
            } 
        }

        public ObservableCollection<OneTranslationService> Translators { get; } = new ObservableCollection<OneTranslationService>();

        public int TopMostIndex
        {
            get => SettingsIncapsuler.Instance.TopMost ? 0 : 1;
            set => SettingsIncapsuler.Instance.TopMost = value == 0;
        }

        public int AlternativeEditingKeysIndex
        {
            get => SettingsIncapsuler.Instance.AlternativeEditingKeys ? 0 : 1;
            set => SettingsIncapsuler.Instance.AlternativeEditingKeys = value == 0;
        }

        public int SessionAutoTranslateIndex
        {
            get => SettingsIncapsuler.Instance.SessionAutoTranslate ? 0 : 1;
            set => SettingsIncapsuler.Instance.SessionAutoTranslate = value == 0;
        }

        public int ShowNotificationsIndex
        {
            get => SettingsIncapsuler.Instance.ShowNotifications ? 0 : 1;
            set => SettingsIncapsuler.Instance.ShowNotifications = value == 0;
        }

        public int ShowPreviewsIndex
        {
            get => SettingsIncapsuler.Instance.ShowPreviews ? 0 : 1;
            set => SettingsIncapsuler.Instance.ShowPreviews = value == 0;
        }

        public int AlternateRowsIndex
        {
            get => SettingsIncapsuler.Instance.AlternatingRows ? 0 : 1;
            set => SettingsIncapsuler.Instance.AlternatingRows = value == 0;
        }

        public SettingsWindow()
        {
            InitializeComponent();
            OtherFileExts = SettingsIncapsuler.Instance.OtherExtensions.JoinStr("|");
            ImageFileExts = SettingsIncapsuler.Instance.ImageExtensions.JoinStr("|");
            LoadApktools();

            Themes.AddRange(GlobalVariables.ThemesMap.Select(it => it.Value));

            ThemeBox.SelectedItem = GlobalVariables.ThemesMap.Forward[SettingsIncapsuler.Instance.Theme];

            Translators.AddRange(TranslateService.OnlineTranslators.Values);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SaveOtherExts(object sender, RoutedEventArgs e)
        {
            SettingsIncapsuler.Instance.OtherExtensions = OtherFileExts.SplitFR("|").Select(_ => _.Trim()).Distinct().ToArray();
        }

        private void SaveImageExts(object sender, RoutedEventArgs e)
        {
            SettingsIncapsuler.Instance.ImageExtensions = ImageFileExts.SplitFR("|").Select(_ => _.Trim()).Distinct().ToArray();
        }

        private void ThemeChanged(object sender, SelectionChangedEventArgs e)
        {
            string changed = e.AddedItems.Count > 0 ? e.AddedItems[0].As<string>() : null;

            if (changed != null)
                Utils.ChangeTheme(GlobalVariables.ThemesMap.Backward[changed]);
        }

        private void LoadApktools()
        {
            ApktoolVersions.Clear();
            ApktoolVersions.AddRange(
                Directory.EnumerateFiles(GlobalVariables.PathToApktoolVersions)
                    .Select(Path.GetFileNameWithoutExtension)
                    .Select(s => s.Split('_'))
                    .Where(split => split.Length == 2)
                    .Select(split => split[1])
            );
            ApktoolVersions.Add(Res.Catalog);
            ApktoolVersionBox.SelectedItem = SettingsIncapsuler.Instance.ApktoolVersion;
        }

        private void ApktoolVersionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;

            string version = e.AddedItems[0].As<string>();
            if (version == Res.Catalog)
            {
                if (!SettingsIncapsuler.Instance.ApktoolVersion.NE())
                    sender.As<ComboBox>().SelectedItem = SettingsIncapsuler.Instance.ApktoolVersion;

                new ApktoolCatalogWindow().ShowDialog();

                LoadApktools();

                return;
            }

            SettingsIncapsuler.Instance.ApktoolVersion = version;
        }
    }
}
