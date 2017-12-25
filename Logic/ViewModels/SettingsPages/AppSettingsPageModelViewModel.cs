using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.Interfaces.SettingsPages;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.WebServices;
using TranslatorApk.Windows;
using UsefulFunctionsLib;

namespace TranslatorApk.Logic.ViewModels.SettingsPages
{
    public class AppSettingsPageViewModel : BindableBase, ISettingsPageViewModel, IDisposable
    {
        public static Lazy<ISettingsPageViewModel> InstanseLazy { get; }
            = new Lazy<ISettingsPageViewModel>(() => new AppSettingsPageViewModel());

        public static AppSettingsPageViewModel Instanse => (AppSettingsPageViewModel)InstanseLazy.Value;

        private AppSettingsPageViewModel()
        {
            RefreshData();

            SettingsIncapsuler.Instance.PropertyChanged += SettingsOnPropertyChanged;
        }

        public string PageTitle { get; } = "Настройки приложения";

        public string[] YesNoItems { get; private set; }

        public int LanguageOfAppIndex
        {
            get => TranslateService.SupportedProgramLangs.IndexOf(SettingsIncapsuler.Instance.LanguageOfApp);
            set => Utils.Utils.SetLanguageOfApp(TranslateService.SupportedProgramLangs[value], true);
        }

        public int ShowPreviewsIndex
        {
            get => SettingsIncapsuler.Instance.ShowPreviews ? 0 : 1;
            set => SettingsIncapsuler.Instance.ShowPreviews = value == 0;
        }

        public int TopMostIndex
        {
            get => SettingsIncapsuler.Instance.TopMost ? 0 : 1;
            set => SettingsIncapsuler.Instance.TopMost = value == 0;
        }

        public int ShowNotificationsIndex
        {
            get => SettingsIncapsuler.Instance.ShowNotifications ? 0 : 1;
            set => SettingsIncapsuler.Instance.ShowNotifications = value == 0;
        }

        public int AlternateRowsIndex
        {
            get => SettingsIncapsuler.Instance.AlternatingRows ? 0 : 1;
            set => SettingsIncapsuler.Instance.AlternatingRows = value == 0;
        }

        public string OtherFileExts
        {
            get => SettingsIncapsuler.Instance.OtherExtensions.JoinStr("|");
            set => SettingsIncapsuler.Instance.OtherExtensions = value.SplitFR("|").Select(_ => _.Trim()).Distinct().ToArray();
        }

        public string ImageFileExts
        {
            get => SettingsIncapsuler.Instance.ImageExtensions.JoinStr("|");
            set => SettingsIncapsuler.Instance.ImageExtensions = value.SplitFR("|").Select(_ => _.Trim()).Distinct().ToArray();
        }

        public ObservableRangeCollection<string> Themes { get; } = new ObservableRangeCollection<string>();

        public ObservableRangeCollection<string> ApktoolVersions { get; } = new ObservableRangeCollection<string>();

        public string CurrentTheme
        {
            get => GlobalVariables.ThemesMap.Forward[SettingsIncapsuler.Instance.Theme];
            set => Utils.Utils.ChangeTheme(GlobalVariables.ThemesMap.Backward[value]);
        }

        public string CurrentApktoolVersion
        {
            get => SettingsIncapsuler.Instance.ApktoolVersion;
            [DebuggerStepThrough]
            set
            {
                if (value == null)
                    Utils.Utils.IgnoreComboBoxChange();

                if (value == Resources.Localizations.Resources.Catalog)
                {
                    new ApktoolCatalogWindow().ShowDialog();

                    LoadApktools();

                    Utils.Utils.IgnoreComboBoxChange();
                }

                SettingsIncapsuler.Instance.ApktoolVersion = value;
            }
        }

        public void RefreshData()
        {
            YesNoItems = new[] { Resources.Localizations.Resources.Yes, Resources.Localizations.Resources.No };

            Themes.ReplaceRange(GlobalVariables.ThemesMap.Select(it => it.Value));

            LoadApktools();
        }

        private void LoadApktools()
        {
            ApktoolVersions.ReplaceRange(
                Directory.EnumerateFiles(GlobalVariables.PathToApktoolVersions)
                    .Select(Path.GetFileNameWithoutExtension)
                    .Select(s => s.Split('_'))
                    .Where(split => split.Length == 2)
                    .Select(split => split[1])
            );

            ApktoolVersions.Add(Resources.Localizations.Resources.Catalog);

            if (!ApktoolVersions.Contains(SettingsIncapsuler.Instance.ApktoolVersion))
                SettingsIncapsuler.Instance.ApktoolVersion = ApktoolVersions[0];
            else
                RaisePropertyChanged(nameof(CurrentApktoolVersion));
        }

        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(SettingsIncapsuler.ApktoolVersion):
                    RaisePropertyChanged(nameof(CurrentApktoolVersion));
                    break;
                case nameof(SettingsIncapsuler.Theme):
                    RaisePropertyChanged(nameof(CurrentTheme));
                    break;
                case nameof(SettingsIncapsuler.ShowPreviews):
                    RaisePropertyChanged(nameof(ShowPreviewsIndex));
                    break;
                case nameof(SettingsIncapsuler.TopMost):
                    RaisePropertyChanged(nameof(TopMostIndex));
                    break;
                case nameof(SettingsIncapsuler.ShowNotifications):
                    RaisePropertyChanged(nameof(ShowNotificationsIndex));
                    break;
                case nameof(SettingsIncapsuler.AlternatingRows):
                    RaisePropertyChanged(nameof(AlternateRowsIndex));
                    break;
                case nameof(SettingsIncapsuler.OtherExtensions):
                    RaisePropertyChanged(nameof(OtherFileExts));
                    break;
                case nameof(SettingsIncapsuler.ImageExtensions):
                    RaisePropertyChanged(nameof(ImageFileExts));
                    break;
            }
        }

        public void Dispose()
        {
            SettingsIncapsuler.Instance.PropertyChanged -= SettingsOnPropertyChanged;
        }
    }
}
