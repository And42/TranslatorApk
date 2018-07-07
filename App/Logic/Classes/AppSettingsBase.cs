using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using SettingsManager;
using TranslatorApk.Logic.Interfaces;

namespace TranslatorApk.Logic.Classes
{
    public abstract class AppSettingsBase : SettingsContainerBase, ISettingsCheckable
    {
        #region Properties

        public string LanguageOfApp
        {
            get => GetValueInternal<string>();
            set => SetValueInternal(value);
        }

        public Guid OnlineTranslator
        {
            get => GetValueInternal<Guid>();
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

        public bool OnlyXml
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        public List<string> XmlRules
        {
            get => GetValueInternal<List<string>>();
            set => SetValueInternal(value);
        }

        public string TargetLanguage
        {
            get => GetValueInternal<string>();
            set => SetValueInternal(value);
        }

        public bool OnlyFullWords
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        public bool MatchCase
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        public List<string> EditorSearchAdds
        {
            get => GetValueInternal<List<string>>();
            set => SetValueInternal(value);
        }

        public List<string> FullSearchAdds
        {
            get => GetValueInternal<List<string>>();
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

        public bool MainWMaximized
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        public bool EditorWMaximized
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        public List<string> OtherExtensions
        {
            get => GetValueInternal<List<string>>();
            set => SetValueInternal(value);
        }

        public bool OtherFiles
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        public List<string> AvailToEditFiles
        {
            get => GetValueInternal<List<string>>();
            set => SetValueInternal(value);
        }

        public List<string> ImageExtensions
        {
            get => GetValueInternal<List<string>>();
            set => SetValueInternal(value);
        }

        public Size MainWindowSize
        {
            get => GetValueInternal<Size>();
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

        public bool ShowPreviews
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        public Dictionary<Guid, string> TranslatorServicesKeys
        {
            get => GetValueInternal<Dictionary<Guid, string>>();
            set => SetValueInternal(value);
        }

        public bool EmptyFolders
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        public bool TopMost
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        public string TargetDictionary
        {
            get => GetValueInternal<string>();
            set => SetValueInternal(value);
        }

        public List<CheckableString> SourceDictionaries
        {
            get => GetValueInternal<List<CheckableString>>();
            set => SetValueInternal(value);
        }

        public bool AlternativeEditingKeys
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        public bool SessionAutoTranslate
        {
            get => GetValueInternal<bool>();
            set => SetValueInternal(value);
        }

        public bool EditorWindowSaveToDict
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

        #endregion

        public virtual void CheckAll()
        {
            PropertyInfo[] properties = 
                typeof(AppSettingsBase).GetProperties(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
                GetSetting(property.Name);
        }
    }
}