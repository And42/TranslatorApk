using System;
using System.ComponentModel;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.Interfaces.SettingsPages;
using TranslatorApk.Logic.OrganisationItems;

namespace TranslatorApk.Logic.ViewModels.SettingsPages
{
    public class EditorSettingsPageViewModel : BindableBase, ISettingsPageViewModel, IDisposable
    {
        public static Lazy<ISettingsPageViewModel> InstanseLazy { get; }
            = new Lazy<ISettingsPageViewModel>(() => new EditorSettingsPageViewModel());

        public static EditorSettingsPageViewModel Instanse => (EditorSettingsPageViewModel)InstanseLazy.Value;

        private EditorSettingsPageViewModel()
        {
            RefreshData();

            SettingsIncapsuler.Instance.PropertyChanged += SettingsOnPropertyChanged;
        }

        public string PageTitle { get; } = "Редактор";

        public string[] YesNoItems { get; private set; }

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

        public void RefreshData()
        {
            YesNoItems = new[] { Resources.Localizations.Resources.Yes, Resources.Localizations.Resources.No };
        }

        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(SettingsIncapsuler.AlternativeEditingKeys):
                    RaisePropertyChanged(nameof(AlternativeEditingKeysIndex));
                    break;
                case nameof(SettingsIncapsuler.SessionAutoTranslate):
                    RaisePropertyChanged(nameof(SessionAutoTranslateIndex));
                    break;
            }
        }

        public void Dispose()
        {
            SettingsIncapsuler.Instance.PropertyChanged -= SettingsOnPropertyChanged;
        }
    }
}
