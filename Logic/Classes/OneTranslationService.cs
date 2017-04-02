using System;
using System.ComponentModel;
using TranslatorApk.Annotations;
using TranslatorApkPluginLib;

namespace TranslatorApk.Logic.Classes
{
    public class OneTranslationService : INotifyPropertyChanged
    {
        public OneTranslationService(ITranslateService service)
        {
            Name = service.GetServiceName();
            TranslateFunc = service.Translate;
            Guid = service.Guid;
        }

        public OneTranslationService(string name, Func<string, string, string, string> translateFunc, Guid guid)
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
        public Func<string, string, string, string> TranslateFunc { get; }

        public string ApiKey
        {
            get { return _apiKey; }
            set
            {
                if (_apiKey == value) return;
                _apiKey = value;
                OnPropertyChanged(nameof(ApiKey));
            }
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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
