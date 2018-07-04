using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.Interfaces.SettingsPages;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.Utils;
using TranslatorApk.Logic.WebServices;
using TranslatorApk.Resources.Localizations;
using TranslatorApk.Windows;

namespace TranslatorApk.Logic.ViewModels.SettingsPages
{
    public class AppSettingsPageViewModel : ViewModelBase, ISettingsPageViewModel
    {
        private readonly ObservableRangeCollection<string> _themes;
        private readonly ObservableRangeCollection<string> _apktoolVersions;

        public AppSettingsPageViewModel()
        {
            _themes = new ObservableRangeCollection<string>();
            _apktoolVersions = new ObservableRangeCollection<string>();

            Themes = new ReadOnlyObservableCollection<string>(_themes);
            ApktoolVersions = new ReadOnlyObservableCollection<string>(_apktoolVersions);

            RefreshData();

            DefaultSettingsContainer.Instance.PropertyChanged += SettingsOnPropertyChanged;
        }

        public string PageTitle { get; } = StringResources.AppSettings_Caption;

        public string[] YesNoItems { get; private set; }

        public int LanguageOfAppIndex
        {
            get => TranslateService.SupportedProgramLangs.IndexOf(DefaultSettingsContainer.Instance.LanguageOfApp);
            set => Utils.Utils.SetLanguageOfApp(TranslateService.SupportedProgramLangs[value], true);
        }

        public int ShowPreviewsIndex
        {
            get => DefaultSettingsContainer.Instance.ShowPreviews ? 0 : 1;
            set => DefaultSettingsContainer.Instance.ShowPreviews = value == 0;
        }

        public int TopMostIndex
        {
            get => DefaultSettingsContainer.Instance.TopMost ? 0 : 1;
            set => DefaultSettingsContainer.Instance.TopMost = value == 0;
        }

        public int ShowNotificationsIndex
        {
            get => DefaultSettingsContainer.Instance.ShowNotifications ? 0 : 1;
            set => DefaultSettingsContainer.Instance.ShowNotifications = value == 0;
        }

        public int AlternateRowsIndex
        {
            get => DefaultSettingsContainer.Instance.AlternatingRows ? 0 : 1;
            set => DefaultSettingsContainer.Instance.AlternatingRows = value == 0;
        }

        public string OtherFileExts
        {
            get => DefaultSettingsContainer.Instance.OtherExtensions.JoinStr("|");
            set => DefaultSettingsContainer.Instance.OtherExtensions = value.SplitRemove('|').Select(_ => _.Trim()).Distinct().ToArray();
        }

        public string ImageFileExts
        {
            get => DefaultSettingsContainer.Instance.ImageExtensions.JoinStr("|");
            set => DefaultSettingsContainer.Instance.ImageExtensions = value.SplitRemove('|').Select(_ => _.Trim()).Distinct().ToArray();
        }

        public int FontSize
        {
            get => DefaultSettingsContainer.Instance.FontSize;
            set => DefaultSettingsContainer.Instance.FontSize = value;
        }

        public int GridFontSize
        {
            get => DefaultSettingsContainer.Instance.GridFontSize;
            set => DefaultSettingsContainer.Instance.GridFontSize = value;
        }

        public ReadOnlyObservableCollection<string> Themes { get; }

        public ReadOnlyObservableCollection<string> ApktoolVersions { get; }

        public string CurrentTheme
        {
            get => GlobalVariables.ThemesMap.Forward[DefaultSettingsContainer.Instance.Theme];
            set => ThemeUtils.ChangeTheme(GlobalVariables.ThemesMap.Backward[value]);
        }

        public string CurrentApktoolVersion
        {
            get => DefaultSettingsContainer.Instance.ApktoolVersion;
            set
            {
                if (value == null)
                    Utils.Utils.IgnoreComboBoxChange();

                if (value == StringResources.Catalog)
                {
                    new ApktoolCatalogWindow().ShowDialog();

                    LoadApktools();

                    Utils.Utils.IgnoreComboBoxChange();
                }

                DefaultSettingsContainer.Instance.ApktoolVersion = value;
            }
        }

        public void RefreshData()
        {
            YesNoItems = new[] { StringResources.Yes, StringResources.No };

            _themes.ReplaceRange(GlobalVariables.ThemesMap.Select(it => it.Value));

            LoadApktools();
        }

        private void LoadApktools()
        {
            _apktoolVersions.ReplaceRange(
                Directory.EnumerateFiles(GlobalVariables.PathToApktoolVersions)
                    .Select(Path.GetFileNameWithoutExtension)
                    .Select(s => s.Split('_'))
                    .Where(split => split.Length == 2)
                    .Select(split => split[1])
            );

            _apktoolVersions.Add(StringResources.Catalog);

            if (!ApktoolVersions.Contains(DefaultSettingsContainer.Instance.ApktoolVersion))
                DefaultSettingsContainer.Instance.ApktoolVersion = ApktoolVersions[0];
            else
                OnPropertyChanged(nameof(CurrentApktoolVersion));
        }

        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(DefaultSettingsContainer.ApktoolVersion):
                    OnPropertyChanged(nameof(CurrentApktoolVersion));
                    break;
                case nameof(DefaultSettingsContainer.Theme):
                    OnPropertyChanged(nameof(CurrentTheme));
                    break;
                case nameof(DefaultSettingsContainer.ShowPreviews):
                    OnPropertyChanged(nameof(ShowPreviewsIndex));
                    break;
                case nameof(DefaultSettingsContainer.TopMost):
                    OnPropertyChanged(nameof(TopMostIndex));
                    break;
                case nameof(DefaultSettingsContainer.ShowNotifications):
                    OnPropertyChanged(nameof(ShowNotificationsIndex));
                    break;
                case nameof(DefaultSettingsContainer.AlternatingRows):
                    OnPropertyChanged(nameof(AlternateRowsIndex));
                    break;
                case nameof(DefaultSettingsContainer.OtherExtensions):
                    OnPropertyChanged(nameof(OtherFileExts));
                    break;
                case nameof(DefaultSettingsContainer.ImageExtensions):
                    OnPropertyChanged(nameof(ImageFileExts));
                    break;
            }
        }

        public override void UnsubscribeFromEvents()
        {
            DefaultSettingsContainer.Instance.PropertyChanged -= SettingsOnPropertyChanged;
        }
    }
}
