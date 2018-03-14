using System.Collections.ObjectModel;
using System.ComponentModel;
using MVVM_Tools.Code.Providers;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.Interfaces.SettingsPages;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.WebServices;
using TranslatorApk.Resources.Localizations;
using UsefulFunctionsLib;

namespace TranslatorApk.Logic.ViewModels.SettingsPages
{
    public class TranslationPageViewModel : ViewModelBase, ISettingsPageViewModel
    {
        public string PageTitle { get; } = StringResources.TranslationSettings_Caption;

        public PropertyProvider<string[]> YesNoItems { get; }

        public int FixOnlineTranslationResultsIndex
        {
            get => SettingsIncapsuler.Instance.FixOnlineTranslationResults ? 0 : 1;
            set
            {
                SettingsIncapsuler.Instance.FixOnlineTranslationResults = value == 0;
                OnPropertyChanged();
            }
        }

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

        public TranslationPageViewModel()
        {
            YesNoItems = CreateProviderWithNotify<string[]>(nameof(YesNoItems));

            RefreshData();

            SettingsIncapsuler.Instance.PropertyChanged += SettingsOnPropertyChanged;
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
            YesNoItems.Value = new[] { StringResources.Yes, StringResources.No };

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
                case nameof(SettingsIncapsuler.FixOnlineTranslationResults):
                    OnPropertyChanged(nameof(FixOnlineTranslationResultsIndex));
                    break;
            }
        }

        public override void UnsubscribeFromEvents()
        {
            SettingsIncapsuler.Instance.PropertyChanged -= SettingsOnPropertyChanged;
        }
    }
}
