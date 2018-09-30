using System.ComponentModel;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.Interfaces.SettingsPages;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Resources.Localizations;

namespace TranslatorApk.Logic.ViewModels.SettingsPages
{
    public class EditorSettingsPageViewModel : ViewModelBase, ISettingsPageViewModel
    {
        private readonly AppSettings _appSettings = GlobalVariables.AppSettings;

        public EditorSettingsPageViewModel()
        {
            RefreshData();

            _appSettings.PropertyChanged += SettingsOnPropertyChanged;
        }

        public string PageTitle { get; } = StringResources.EditorSettings_Caption;

        public string[] YesNoItems { get; private set; }

        public int AlternativeEditingKeysIndex
        {
            get => _appSettings.AlternativeEditingKeys ? 0 : 1;
            set => _appSettings.AlternativeEditingKeys = value == 0;
        }

        public int SessionAutoTranslateIndex
        {
            get => _appSettings.SessionAutoTranslate ? 0 : 1;
            set => _appSettings.SessionAutoTranslate = value == 0;
        }

        public void RefreshData()
        {
            YesNoItems = new[] { StringResources.Yes, StringResources.No };
        }

        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(AppSettings.AlternativeEditingKeys):
                    OnPropertyChanged(nameof(AlternativeEditingKeysIndex));
                    break;
                case nameof(AppSettings.SessionAutoTranslate):
                    OnPropertyChanged(nameof(SessionAutoTranslateIndex));
                    break;
            }
        }

        public override void UnsubscribeFromEvents()
        {
            _appSettings.PropertyChanged -= SettingsOnPropertyChanged;
        }
    }
}
