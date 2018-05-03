using System;
using System.Collections.Specialized;
using System.Drawing;
using SettingsManager;
using TranslatorApk.Properties;

namespace TranslatorApk.Logic.OrganisationItems
{
    internal class DefaultSettingsContainer : SettingsContainerBase
    {
        public static DefaultSettingsContainer Instance { get; } = new DefaultSettingsContainer();

        private DefaultSettingsContainer() { }

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

        public bool TVFilterBoxUseRegex
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        public bool FixOnlineTranslationResults
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        public override void Save()
        {
            Settings.Default.Save();
        }

        protected override void SetSetting(string settingName, object value)
        {
            Settings.Default[settingName] = value;
        }

        protected override object GetSetting(string settingName)
        {
            return Settings.Default[settingName];
        }
    }
}
