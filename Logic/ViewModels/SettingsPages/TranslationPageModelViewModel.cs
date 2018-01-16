using System.Collections.ObjectModel;
using System.ComponentModel;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.Interfaces.SettingsPages;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.WebServices;
using UsefulFunctionsLib;

namespace TranslatorApk.Logic.ViewModels.SettingsPages
{
    public class TranslationPageViewModel : ViewModelBase, ISettingsPageViewModel
    {
        public TranslationPageViewModel()
        {
            RefreshData();

            SettingsIncapsuler.Instance.PropertyChanged += SettingsOnPropertyChanged;
        }

        public string PageTitle { get; } = "Перевод";

        public ObservableCollection<OneTranslationService> Translators { get; } = new ObservableCollection<OneTranslationService>();

        public OneTranslationService OnlineTranslator
        {
            get => GlobalVariables.CurrentTranslationService;
            set
            {
                if (value == null)
                    return;

                GlobalVariables.CurrentTranslationService = value;
                SettingsIncapsuler.Instance.OnlineTranslator = value.Guid;
                OnPropertyChanged(nameof(OnlineTranslator));
            }
        }

        public int TranslationTimeout
        {
            get => SettingsIncapsuler.Instance.TranslationTimeout;
            set
            {
                if (value < 10 || value > 100000)
                    SettingsIncapsuler.Instance.TranslationTimeout = 5000;
                else
                    SettingsIncapsuler.Instance.TranslationTimeout = value;
            }
        }

        public void RefreshData()
        {
            Translators.Clear();
            Translators.AddRange(TranslateService.OnlineTranslators.Values);

            OnPropertyChanged(nameof(OnlineTranslator));
        }

        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(SettingsIncapsuler.TranslationTimeout):
                    OnPropertyChanged(nameof(TranslationTimeout));
                    break;
            }
        }

        public override void UnsubscribeFromEvents()
        {
            SettingsIncapsuler.Instance.PropertyChanged -= SettingsOnPropertyChanged;
        }
    }
}
