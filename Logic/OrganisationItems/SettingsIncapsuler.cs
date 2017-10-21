using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using TranslatorApk.Annotations;
using TranslatorApk.Logic.Interfaces;
using TranslatorApk.Properties;

namespace TranslatorApk.Logic.OrganisationItems
{
    /// <summary>
    /// Класс для работы с файлом настроек. Поддерживает кэширование. Крайне рекомендуется использовать его для управления настройками.
    /// </summary>
    public class SettingsIncapsuler : ISettingsContainer
    {
        private static readonly Dictionary<string, PropertyInfo> CurrentTypeProperties;

        public static SettingsIncapsuler Instance =>
            _current ?? (_current = new SettingsIncapsuler());
        private static SettingsIncapsuler _current;

        private static readonly Dictionary<string, object> CachedProperties = new Dictionary<string, object>();

        static SettingsIncapsuler()
        {
            CurrentTypeProperties = typeof(SettingsIncapsuler).GetProperties().ToDictionary(_ => _.Name, _ => _);
        }

        private SettingsIncapsuler() { }

        public string TargetLanguage
        {
            get => GetValueInternal<string>();
            set => SetValueInternal(value);
        }

        public string TargetDictionary
        {
            get => GetValueInternal<string>();
            set => SetValueInternal(value);
        }

        public bool ShowPreviews
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        public bool AlternativeEditingKeys
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        public bool TopMost
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        public string[] AvailToEditFiles
        {
            get => GetValueInternal<string[]>();
            set => SetValueInternal(value);
        }

        public string[] XmlRules
        {
            get => GetValueInternal<string[]>();
            set => SetValueInternal(value);
        }

        public string[] ImageExtensions
        {
            get => GetValueInternal<string[]>();
            set => SetValueInternal(value);
        }

        public string[] OtherExtensions
        {
            get => GetValueInternal<string[]>();
            set => SetValueInternal(value);
        }

        public bool SessionAutoTranslate
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        public Guid OnlineTranslator
        {
            get => GetValueInternal<Guid>();
            set => SetValueInternal(value);
        }

        public string LanguageOfApp
        {
            get => GetValueInternal<string>();
            set => SetValueInternal(value);
        }

        public bool EmptyXml
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        public bool EmptySmali
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        public bool OtherFiles
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        public bool OnlyXml
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        public bool EmptyFolders
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        public bool Images
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        public bool FilesWithErrors
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        public bool OnlyResources
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        public string Theme
        {
            get => GetValueInternal<string>();
            set => SetValueInternal(value);
        }

        public string ApktoolVersion
        {
            get => GetValueInternal<string>();
            set => SetValueInternal(value);
        }

        public bool EditorWindowSaveToDict
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        public bool EditorWMaximized
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        public bool MainWMaximized
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        public Point MainWindowSize
        {
            get => GetValueInternal<Point>();
            set => SetValueInternal(value);
        }

        public bool EditorSOnlyFullWords
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        public bool EditorSMatchCase
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        public bool ShowNotifications
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        public bool AlternatingRows
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        public StringCollection FullSearchAdds
        {
            get => GetValueInternal<StringCollection>();
            set => SetValueInternal(value);
        }

        public StringCollection EditorSearchAdds
        {
            get => GetValueInternal<StringCollection>();
            set => SetValueInternal(value);
        }

        public bool MatchCase
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        public bool OnlyFullWords
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        private T GetValueInternal<T>([CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            if (CachedProperties.TryGetValue(propertyName, out object value))
                return (T) value;

            object val = Settings.Default[propertyName];

            CachedProperties.Add(propertyName, val);

            return (T) val;
        }

        private void SetValueInternal<T>(T value, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            if (CachedProperties.TryGetValue(propertyName, out var cachedValue))
            {
                if (EqualityComparer<T>.Default.Equals((T) cachedValue, value))
                    return;

                CachedProperties[propertyName] = value;
            }
            else
            {
                CachedProperties.Add(propertyName, value);
            }

            Settings.Default[propertyName] = value;

            Save();

            OnPropertyChanged(propertyName);
        }

        public T GetValue<T>(string settingName)
        {
            if (CurrentTypeProperties.TryGetValue(settingName, out var property))
                return (T) property.GetValue(null, null);

            throw new ArgumentOutOfRangeException(nameof(settingName));
        }

        public void SetValue<T>(string settingName, T value)
        {
            if (CurrentTypeProperties.TryGetValue(settingName, out var property))
                property.SetValue(this, value, null);
            else
                throw new ArgumentOutOfRangeException(nameof(settingName));
        }

        void ISettingsContainer.Save()
        {
            Save();
        }

        public static void Save()
        {
            Settings.Default.Save();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
