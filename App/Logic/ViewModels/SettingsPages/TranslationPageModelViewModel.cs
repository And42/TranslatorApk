using System.ComponentModel;
using MVVM_Tools.Code.Providers;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.Interfaces.SettingsPages;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.WebServices;
using TranslatorApk.Resources.Localizations;

namespace TranslatorApk.Logic.ViewModels.SettingsPages
{
    public class TranslationPageViewModel : ViewModelBase, ISettingsPageViewModel
    {
        private readonly GlobalVariables _globalVariables = GlobalVariables.Instance;
        private readonly AppSettingsBase _appSettings = GlobalVariables.AppSettings;

        public string PageTitle { get; } = StringResources.TranslationSettings_Caption;

        public Property<string[]> YesNoItems { get; private set; }

        public int FixOnlineTranslationResultsIndex
        {
            get => _appSettings.FixOnlineTranslationResults ? 0 : 1;
            set
            {
                _appSettings.FixOnlineTranslationResults = value == 0;
                OnPropertyChanged();
            }
        }

        public ObservableRangeCollection<OneTranslationService> Translators { get; } = new ObservableRangeCollection<OneTranslationService>();

        public OneTranslationService OnlineTranslator
        {
            get => _globalVariables.CurrentTranslationService.Value;
            set
            {
                if (value == null)
                    return;

                _globalVariables.CurrentTranslationService.Value = value;
                _appSettings.OnlineTranslator = value.Guid;
                OnPropertyChanged(nameof(OnlineTranslator));
            }
        }

        public TranslationPageViewModel()
        {
            BindProperty(() => YesNoItems);

            RefreshData();

            _appSettings.PropertyChanged += SettingsOnPropertyChanged;
        }

        public int TranslationTimeout
        {
            get => _appSettings.TranslationTimeout;
            set
            {
                if (value < 10 || value > 100000)
                    _appSettings.TranslationTimeout = 5000;
                else
                    _appSettings.TranslationTimeout = value;
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
                case nameof(AppSettingsBase.TranslationTimeout):
                    OnPropertyChanged(nameof(TranslationTimeout));
                    break;
                case nameof(AppSettingsBase.FixOnlineTranslationResults):
                    OnPropertyChanged(nameof(FixOnlineTranslationResultsIndex));
                    break;
            }
        }

        public override void UnsubscribeFromEvents()
        {
            _appSettings.PropertyChanged -= SettingsOnPropertyChanged;
        }
    }
}
