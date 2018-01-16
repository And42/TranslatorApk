using System.ComponentModel;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.Interfaces.SettingsPages;
using TranslatorApk.Logic.OrganisationItems;

namespace TranslatorApk.Logic.ViewModels.SettingsPages
{
    public class EditorSettingsPageViewModel : ViewModelBase, ISettingsPageViewModel
    {
        public EditorSettingsPageViewModel()
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
                    OnPropertyChanged(nameof(AlternativeEditingKeysIndex));
                    break;
                case nameof(SettingsIncapsuler.SessionAutoTranslate):
                    OnPropertyChanged(nameof(SessionAutoTranslateIndex));
                    break;
            }
        }

        public override void UnsubscribeFromEvents()
        {
            SettingsIncapsuler.Instance.PropertyChanged -= SettingsOnPropertyChanged;
        }
    }
}
