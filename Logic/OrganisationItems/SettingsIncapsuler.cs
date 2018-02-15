using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.Runtime.CompilerServices;
using MVVM_Tools.Code.Classes;
using TranslatorApk.Logic.Interfaces;
using TranslatorApk.Properties;

namespace TranslatorApk.Logic.OrganisationItems
{
    /// <summary>
    /// Класс для работы с файлом настроек. Поддерживает кэширование. Крайне рекомендуется использовать его для управления настройками.
    /// </summary>
    public class SettingsIncapsuler : BindableBase, ISettingsContainer
    {
        public static SettingsIncapsuler Instance { get; } = new SettingsIncapsuler();

        public bool AutoFlush { get; set; } = true;

        private readonly Dictionary<string, object> _cachedProperties = new Dictionary<string, object>();

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

        public int TranslationTimeout
        {
            get => GetValueInternal<int>();
            set => SetValueInternal(value);
        }

        public int FontSize
        {
            get => GetValueInternal<int>();
            set => SetValueInternal(value);
        }

        public int GridFontSize
        {
            get => GetValueInternal<int>();
            set => SetValueInternal(value);
        }

        private T GetValueInternal<T>([CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            if (_cachedProperties.TryGetValue(propertyName, out object value))
                return (T) value;

            object val = Settings.Default[propertyName];

            _cachedProperties.Add(propertyName, val);
            
            return (T) val;
        }

        private void SetValueInternal<T>(T value, [CallerMemberName] string propertyName = null)
        {
            if (propertyName == null)
                throw new ArgumentNullException(nameof(propertyName));

            if (_cachedProperties.TryGetValue(propertyName, out var cachedValue))
            {
                if (EqualityComparer<T>.Default.Equals((T) cachedValue, value))
                    return;

                _cachedProperties[propertyName] = value;
            }
            else
            {
                _cachedProperties.Add(propertyName, value);
            }

            Settings.Default[propertyName] = value;

            if (AutoFlush)
                Save();

            OnPropertyChanged(propertyName);
        }

        public T GetValue<T>(string settingName)
        {
            return GetValueInternal<T>(settingName);
        }

        public void SetValue<T>(string settingName, T value)
        {
            SetValueInternal(value, settingName);
        }

        public void Save()
        {
            Settings.Default.Save();
        }
    }
}
