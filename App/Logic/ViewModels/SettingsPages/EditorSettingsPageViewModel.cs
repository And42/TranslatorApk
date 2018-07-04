using System.ComponentModel;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.Interfaces.SettingsPages;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Resources.Localizations;

namespace TranslatorApk.Logic.ViewModels.SettingsPages
{
    public class EditorSettingsPageViewModel : ViewModelBase, ISettingsPageViewModel
    {
        public EditorSettingsPageViewModel()
        {
            RefreshData();

            DefaultSettingsContainer.Instance.PropertyChanged += SettingsOnPropertyChanged;
        }

        public string PageTitle { get; } = StringResources.EditorSettings_Caption;

        public string[] YesNoItems { get; private set; }

        public int AlternativeEditingKeysIndex
        {
            get => DefaultSettingsContainer.Instance.AlternativeEditingKeys ? 0 : 1;
            set => DefaultSettingsContainer.Instance.AlternativeEditingKeys = value == 0;
        }

        public int SessionAutoTranslateIndex
        {
            get => DefaultSettingsContainer.Instance.SessionAutoTranslate ? 0 : 1;
            set => DefaultSettingsContainer.Instance.SessionAutoTranslate = value == 0;
        }

        public void RefreshData()
        {
            YesNoItems = new[] { StringResources.Yes, StringResources.No };
        }

        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(DefaultSettingsContainer.AlternativeEditingKeys):
                    OnPropertyChanged(nameof(AlternativeEditingKeysIndex));
                    break;
                case nameof(DefaultSettingsContainer.SessionAutoTranslate):
                    OnPropertyChanged(nameof(SessionAutoTranslateIndex));
                    break;
            }
        }

        public override void UnsubscribeFromEvents()
        {
            DefaultSettingsContainer.Instance.PropertyChanged -= SettingsOnPropertyChanged;
        }
    }
}
