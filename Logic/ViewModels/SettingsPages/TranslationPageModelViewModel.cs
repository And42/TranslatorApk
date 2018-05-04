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

        public Property<string[]> YesNoItems { get; private set; }

        public int FixOnlineTranslationResultsIndex
        {
            get => DefaultSettingsContainer.Instance.FixOnlineTranslationResults ? 0 : 1;
            set
            {
                DefaultSettingsContainer.Instance.FixOnlineTranslationResults = value == 0;
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
                DefaultSettingsContainer.Instance.OnlineTranslator = value.Guid;
                OnPropertyChanged(nameof(OnlineTranslator));
            }
        }

        public TranslationPageViewModel()
        {
            BindProperty(() => YesNoItems);

            RefreshData();

            DefaultSettingsContainer.Instance.PropertyChanged += SettingsOnPropertyChanged;
        }

        public int TranslationTimeout
        {
            get => DefaultSettingsContainer.Instance.TranslationTimeout;
            set
            {
                if (value < 10 || value > 100000)
                    DefaultSettingsContainer.Instance.TranslationTimeout = 5000;
                else
                    DefaultSettingsContainer.Instance.TranslationTimeout = value;
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
                case nameof(DefaultSettingsContainer.TranslationTimeout):
                    OnPropertyChanged(nameof(TranslationTimeout));
                    break;
                case nameof(DefaultSettingsContainer.FixOnlineTranslationResults):
                    OnPropertyChanged(nameof(FixOnlineTranslationResultsIndex));
                    break;
            }
        }

        public override void UnsubscribeFromEvents()
        {
            DefaultSettingsContainer.Instance.PropertyChanged -= SettingsOnPropertyChanged;
        }
    }
}
