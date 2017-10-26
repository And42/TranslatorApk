using System;
using System.ComponentModel;
using TranslatorApk.Logic.Interfaces;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.Utils;
using TranslatorApkPluginLib;

namespace TranslatorApk.Logic.Classes
{
    public class OneTranslationService : IRaisePropertyChanged
    {
        public delegate string TranslationFunc(string text, string targetLanguage, string apiKey);

        public OneTranslationService(ITranslateService service)
        {
            Name = service.GetServiceName();
            TranslateFunc = service.Translate;
            Guid = service.Guid;
        }

        public OneTranslationService(string name, TranslationFunc translateFunc, Guid guid)
        {
            Name = name;
            TranslateFunc = translateFunc;
            Guid = guid;
        }

        public string Name { get; }

        public Guid Guid { get; }

        /// <summary>
        /// Text, target language, api key
        /// </summary>
        public TranslationFunc TranslateFunc { get; }

        public string ApiKey
        {
            get => _apiKey;
            set => this.SetProperty(ref _apiKey, value);
        }
        private string _apiKey;

        public string Translate(string text, string targetLanguage)
        {
            return TranslateFunc?.Invoke(text, targetLanguage, ApiKey);
        }

        public override string ToString()
        {
            return Name;
        }

        #region Property changed

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
