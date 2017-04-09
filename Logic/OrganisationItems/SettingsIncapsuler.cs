using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using TranslatorApk.Properties;

namespace TranslatorApk.Logic.OrganisationItems
{
    /// <summary>
    /// Класс для работы с файлом настроек. Поддерживает кэширование. Крайне рекомендуется использовать его для управления настройками.
    /// </summary>
    public static class SettingsIncapsuler
    {
        public static string TargetLanguage
        {
            get
            {
                if (!_isFieldLoadedTargetLanguage)
                {
                    _targetLaguage = Settings.Default.TargetLanguage;
                    _isFieldLoadedTargetLanguage = true;
                }

                return _targetLaguage;
            }
            set
            {
                _targetLaguage = value;
                Settings.Default.TargetLanguage = value;
                Settings.Default.Save();
            }
        }
        private static string _targetLaguage;
        private static bool _isFieldLoadedTargetLanguage;

        public static string TargetDictionary
        {
            get
            {
                if (!_isFieldLoadedTargetDictionary)
                {
                    _targetDictionary = Settings.Default.TargetDictionary;
                    _isFieldLoadedTargetDictionary = true;
                }

                return _targetDictionary;
            }
            set
            {
                _targetDictionary = value;
                Settings.Default.TargetDictionary = value;
                Settings.Default.Save();
            }
        }
        private static string _targetDictionary;
        private static bool _isFieldLoadedTargetDictionary;

        public static bool ShowPreviews
        {
            get
            {
                if (!_isFieldLoadedShowPreviews)
                {
                    _showPreviews = Settings.Default.ShowPreviews;
                    _isFieldLoadedShowPreviews = true;
                }

                return _showPreviews;
            }
            set
            {
                _showPreviews = value;
                Settings.Default.ShowPreviews = value;
                Settings.Default.Save();
            }
        }
        private static bool _showPreviews;
        private static bool _isFieldLoadedShowPreviews;

        public static bool AlternativeEditingKeys
        {
            get
            {
                if (!_isFieldLoadedAlternativeEditingKeys)
                {
                    _alternativeEditingKeys = Settings.Default.AlternativeEditingKeys;
                    _isFieldLoadedAlternativeEditingKeys = true;
                }

                return _alternativeEditingKeys;
            }
            set
            {
                _alternativeEditingKeys = value;
                Settings.Default.AlternativeEditingKeys = value;
                Settings.Default.Save();
            }
        }
        private static bool _alternativeEditingKeys;
        private static bool _isFieldLoadedAlternativeEditingKeys;

        public static bool TopMost
        {
            get
            {
                if (!_isFieldLoadedTopMost)
                {
                    _topMost = Settings.Default.TopMost;
                    _isFieldLoadedTopMost = true;
                }

                return _topMost;
            }
            set
            {
                _topMost = value;
                Settings.Default.TopMost = value;
                Settings.Default.Save();
                GlobalVariables.Instance.OnPropertyChanged(nameof(TopMost));
            }
        }
        private static bool _topMost;
        private static bool _isFieldLoadedTopMost;

        public static string[] AvailToEditFiles
        {
            get
            {
                if (!_isFieldLoadedAvailToEditFiles)
                {
                    _availToEditFiles = Settings.Default.AvailToEditFiles;
                    _isFieldLoadedAvailToEditFiles = true;
                }

                return _availToEditFiles;
            }
            set
            {
                _availToEditFiles = value;
                Settings.Default.AvailToEditFiles = value;
                Settings.Default.Save();
            }
        }
        private static string[] _availToEditFiles;
        private static bool _isFieldLoadedAvailToEditFiles;

        public static string[] XmlRules
        {
            get
            {
                if (!_isFieldLoadedXmlRules)
                {
                    _xmlRules = Settings.Default.XmlRules;
                    _isFieldLoadedXmlRules = true;
                }

                return _xmlRules;
            }
            set
            {
                _xmlRules = value;
                Settings.Default.XmlRules = value;
                Settings.Default.Save();
            }
        }
        private static string[] _xmlRules;
        private static bool _isFieldLoadedXmlRules;

        public static HashSet<string> ImageExtensions
        {
            get
            {
                if (!_isFieldLoadedImageExtensions)
                {
                    var val = Settings.Default.ImageExtensions;
                    _imageExtensions = val != null ? new HashSet<string>(val.Distinct()) : null;
                    _isFieldLoadedImageExtensions = true;
                }

                return _imageExtensions;
            }
            set
            {
                _imageExtensions = value;
                Settings.Default.ImageExtensions = value.ToArray();
                Settings.Default.Save();
            }
        }
        private static HashSet<string> _imageExtensions;
        private static bool _isFieldLoadedImageExtensions;

        public static HashSet<string> OtherExtensions
        {
            get
            {
                if (!_isFieldLoadedOtherExtensions)
                {
                    var val = Settings.Default.OtherExtensions;
                    _otherExtensions = val != null ? new HashSet<string>(val) : null;
                    _isFieldLoadedOtherExtensions = true;
                }

                return _otherExtensions;
            }
            set
            {
                _otherExtensions = value;
                Settings.Default.OtherExtensions = value.ToArray();
                Settings.Default.Save();
            }
        }
        private static HashSet<string> _otherExtensions;
        private static bool _isFieldLoadedOtherExtensions;

        public static bool SessionAutoTranslate
        {
            get
            {
                if (!_isFieldLoadedSessionAutoTranslate)
                {
                    _sessionAutoTranslate = Settings.Default.SessionAutoTranslate;
                    _isFieldLoadedSessionAutoTranslate = true;
                }

                return _sessionAutoTranslate;
            }
            set
            {
                _sessionAutoTranslate = value;
                Settings.Default.SessionAutoTranslate = value;
                Settings.Default.Save();
            }
        }
        private static bool _sessionAutoTranslate;
        private static bool _isFieldLoadedSessionAutoTranslate;

        public static Guid OnlineTranslator
        {
            get
            {
                if (!_isFieldLoadedOnlineTranslator)
                {
                    _onlineTranslator = Settings.Default.OnlineTranslator;
                    _isFieldLoadedOnlineTranslator = true;
                }

                return _onlineTranslator;
            }
            set
            {
                _onlineTranslator = value;
                Settings.Default.OnlineTranslator = value;
                Settings.Default.Save();
            }
        }
        private static Guid _onlineTranslator;
        private static bool _isFieldLoadedOnlineTranslator;

        public static string LanguageOfApp
        {
            get
            {
                if (!_isFieldLoadedLanguageOfApp)
                {
                    _languageOfApp = Settings.Default.LanguageOfApp;
                    _isFieldLoadedLanguageOfApp = true;
                }

                return _languageOfApp;
            }
            set
            {
                _languageOfApp = value;
                Settings.Default.LanguageOfApp = value;
                Settings.Default.Save();
            }
        }
        private static string _languageOfApp;
        private static bool _isFieldLoadedLanguageOfApp;

        public static bool EmptyXml
        {
            get
            {
                if (!_isFieldLoadedEmptyXml)
                {
                    _emptyXml = Settings.Default.EmptyXml;
                    _isFieldLoadedEmptyXml = true;
                }

                return _emptyXml;
            }
            set
            {
                _emptyXml = value;
                Settings.Default.EmptyXml = value;
                Settings.Default.Save();
            }
        }
        private static bool _emptyXml;
        private static bool _isFieldLoadedEmptyXml;

        public static bool EmptySmali
        {
            get
            {
                if (!_isFieldLoadedEmptySmali)
                {
                    _emptySmali = Settings.Default.EmptySmali;
                    _isFieldLoadedEmptySmali = true;
                }

                return _emptySmali;
            }
            set
            {
                _emptySmali = value;
                Settings.Default.EmptySmali = value;
                Settings.Default.Save();
            }
        }
        private static bool _emptySmali;
        private static bool _isFieldLoadedEmptySmali;

        public static bool OtherFiles
        {
            get
            {
                if (!_isFieldLoadedOtherFiles)
                {
                    _otherFiles = Settings.Default.OtherFiles;
                    _isFieldLoadedOtherFiles = true;
                }

                return _otherFiles;
            }
            set
            {
                _otherFiles = value;
                Settings.Default.OtherFiles = value;
                Settings.Default.Save();
            }
        }
        private static bool _otherFiles;
        private static bool _isFieldLoadedOtherFiles;

        public static bool OnlyXml
        {
            get
            {
                if (!_isFieldLoadedOnlyXml)
                {
                    _onlyXml = Settings.Default.OnlyXml;
                    _isFieldLoadedOnlyXml = true;
                }

                return _onlyXml;
            }
            set
            {
                _onlyXml = value;
                Settings.Default.OnlyXml = value;
                Settings.Default.Save();
            }
        }
        private static bool _onlyXml;
        private static bool _isFieldLoadedOnlyXml;

        public static bool EmptyFolders
        {
            get
            {
                if (!_isFieldLoadedEmptyFolders)
                {
                    _emptyFolders = Settings.Default.EmptyFolders;
                    _isFieldLoadedEmptyFolders = true;
                }

                return _emptyFolders;
            }
            set
            {
                _emptyFolders = value;
                Settings.Default.EmptyFolders = value;
                Settings.Default.Save();
            }
        }
        private static bool _emptyFolders;
        private static bool _isFieldLoadedEmptyFolders;

        public static bool Images
        {
            get
            {
                if (!_isFieldLoadedImages)
                {
                    _images = Settings.Default.Images;
                    _isFieldLoadedImages = true;
                }

                return _images;
            }
            set
            {
                _images = value;
                Settings.Default.Images = value;
                Settings.Default.Save();
            }
        }
        private static bool _images;
        private static bool _isFieldLoadedImages;

        public static bool FilesWithErrors
        {
            get
            {
                if (!_isFieldLoadedFilesWithErrors)
                {
                    _filesWithErrors = Settings.Default.FilesWithErrors;
                    _isFieldLoadedFilesWithErrors = true;
                }

                return _filesWithErrors;
            }
            set
            {
                _filesWithErrors = value;
                Settings.Default.FilesWithErrors = value;
                Settings.Default.Save();
            }
        }
        private static bool _filesWithErrors;
        private static bool _isFieldLoadedFilesWithErrors;

        public static bool OnlyResources
        {
            get
            {
                if (!_isFieldLoadedOnlyResources)
                {
                    _onlyResources = Settings.Default.OnlyResources;
                    _isFieldLoadedOnlyResources = true;
                }

                return _onlyResources;
            }
            set
            {
                _onlyResources = value;
                Settings.Default.OnlyResources = value;
                Settings.Default.Save();
            }
        }
        private static bool _onlyResources;
        private static bool _isFieldLoadedOnlyResources;

        public static string Theme
        {
            get
            {
                if (!_isFieldLoadedTheme)
                {
                    _theme = Settings.Default.Theme;
                    _isFieldLoadedTheme = true;
                }

                return _theme;
            }
            set
            {
                _theme = value;
                Settings.Default.Theme = value;
                Settings.Default.Save();
            }
        }
        private static string _theme;
        private static bool _isFieldLoadedTheme;

        public static string ApktoolVersion
        {
            get
            {
                if (!_isFieldLoadedApktoolVersion)
                {
                    _apktoolVersion = Settings.Default.ApktoolVersion;
                    _isFieldLoadedApktoolVersion = true;
                }

                return _apktoolVersion;
            }
            set
            {
                _apktoolVersion = value;
                Settings.Default.ApktoolVersion = value;
                Settings.Default.Save();
            }
        }
        private static string _apktoolVersion;
        private static bool _isFieldLoadedApktoolVersion;

        public static bool EditorWindowSaveToDict
        {
            get
            {
                if (!_isFieldLoadedEditorWindowSaveToDict)
                {
                    _editorWindowSaveToDict = Settings.Default.EditorWindow_SaveToDict;
                    _isFieldLoadedEditorWindowSaveToDict = true;
                }

                return _editorWindowSaveToDict;
            }
            set
            {
                _editorWindowSaveToDict = value;
                Settings.Default.EditorWindow_SaveToDict = value;
                Settings.Default.Save();
            }
        }
        private static bool _editorWindowSaveToDict;
        private static bool _isFieldLoadedEditorWindowSaveToDict;

        public static bool EditorWMaximized
        {
            get
            {
                if (!_isFieldLoadedEditorWMaximized)
                {
                    _editorWMaximized = Settings.Default.EditorWMaximized;
                    _isFieldLoadedEditorWMaximized = true;
                }

                return _editorWMaximized;
            }
            set
            {
                _editorWMaximized = value;
                Settings.Default.EditorWMaximized = value;
                Settings.Default.Save();
            }
        }
        private static bool _editorWMaximized;
        private static bool _isFieldLoadedEditorWMaximized;

        public static bool MainWMaximized
        {
            get
            {
                if (!_isFieldLoadedMainWMaximized)
                {
                    _mainWMaximized = Settings.Default.MainWMaximized;
                    _isFieldLoadedMainWMaximized = true;
                }

                return _mainWMaximized;
            }
            set
            {
                _mainWMaximized = value;
                Settings.Default.MainWMaximized = value;
                Settings.Default.Save();
            }
        }
        private static bool _mainWMaximized;
        private static bool _isFieldLoadedMainWMaximized;

        public static Point MainWindowSize
        {
            get
            {
                if (!_isFieldLoadedMainWindowSize)
                {
                    _mainWindowSize = Settings.Default.MainWindowSize;
                    _isFieldLoadedMainWindowSize = true;
                }

                return _mainWindowSize;
            }
            set
            {
                _mainWindowSize = value;
                Settings.Default.MainWindowSize = value;
                Settings.Default.Save();
            }
        }
        private static Point _mainWindowSize;
        private static bool _isFieldLoadedMainWindowSize;

        public static bool EditorSOnlyFullWords
        {
            get
            {
                if (!_isFieldLoadedEditorSOnlyFullWords)
                {
                    _editorSOnlyFullWords = Settings.Default.EditorSOnlyFullWords;
                    _isFieldLoadedEditorSOnlyFullWords = true;
                }

                return _editorSOnlyFullWords;
            }
            set
            {
                _editorSOnlyFullWords = value;
                Settings.Default.EditorSOnlyFullWords = value;
                Settings.Default.Save();
            }
        }
        private static bool _editorSOnlyFullWords;
        private static bool _isFieldLoadedEditorSOnlyFullWords;

        public static bool EditorSMatchCase
        {
            get
            {
                if (!_isFieldLoadedEditorSMatchCase)
                {
                    _editorSMatchCase = Settings.Default.EditorSMatchCase;
                    _isFieldLoadedEditorSMatchCase = true;
                }

                return _editorSMatchCase;
            }
            set
            {
                _editorSMatchCase = value;
                Settings.Default.EditorSMatchCase = value;
                Settings.Default.Save();
            }
        }
        private static bool _editorSMatchCase;
        private static bool _isFieldLoadedEditorSMatchCase;

        public static bool ShowNotifications
        {
            get
            {
                if (!_isFieldLoadedShowNotifications)
                {
                    _showNotifications = Settings.Default.ShowNotifications;
                    _isFieldLoadedShowNotifications = true;
                }

                return _showNotifications;
            }
            set
            {
                _showNotifications = value;
                Settings.Default.ShowNotifications = value;
                Settings.Default.Save();
            }
        }
        private static bool _showNotifications;
        private static bool _isFieldLoadedShowNotifications;

        public static bool AlternatingRows
        {
            get
            {
                if (!_isFieldLoadedAlternatingRows)
                {
                    _alternatingRows = Settings.Default.AlternatingRows;
                    _isFieldLoadedAlternatingRows = true;
                }

                return _alternatingRows;
            }
            set
            {
                _alternatingRows = value;
                Settings.Default.AlternatingRows = value;
                Settings.Default.Save();
            }
        }
        private static bool _alternatingRows;
        private static bool _isFieldLoadedAlternatingRows;
    }
}
