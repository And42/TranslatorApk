using System;
using System.Collections.ObjectModel;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.Interfaces.SettingsPages;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.WebServices;
using UsefulFunctionsLib;

namespace TranslatorApk.Logic.ViewModels.SettingsPages
{
    public class TranslationPageViewModel : BindableBase, ISettingsPageViewModel
    {
        public static Lazy<ISettingsPageViewModel> InstanseLazy { get; }
            = new Lazy<ISettingsPageViewModel>(() => new TranslationPageViewModel());

        public static TranslationPageViewModel Instanse => (TranslationPageViewModel)InstanseLazy.Value;

        private TranslationPageViewModel()
        {
            RefreshData();
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
                RaisePropertyChanged(nameof(OnlineTranslator));
            }
        }

        public void RefreshData()
        {
            Translators.Clear();
            Translators.AddRange(TranslateService.OnlineTranslators.Values);

            RaisePropertyChanged(nameof(OnlineTranslator));
        }

        public void Dispose()
        {
            
        }
    }
}
