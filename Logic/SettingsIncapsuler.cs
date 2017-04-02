using System;
using System.Drawing;
using TranslatorApk.Properties;

namespace TranslatorApk.Logic
{
    public static class SettingsIncapsuler
    {
        public static string TargetLanguage
        {
            get
            {
                if (!isFieldLoadedTargetLanguage)
                {
                    targetLaguage = Settings.Default.TargetLanguage;
                    isFieldLoadedTargetLanguage = true;
                }

                return targetLaguage;
            }
            set
            {
                targetLaguage = value;
                Settings.Default.TargetLanguage = value;
                Settings.Default.Save();
            }
        }
        private static string targetLaguage;
        private static bool isFieldLoadedTargetLanguage;

        public static string TargetDictionary
        {
            get
            {
                if (!isFieldLoadedTargetDictionary)
                {
                    targetDictionary = Settings.Default.TargetDictionary;
                    isFieldLoadedTargetDictionary = true;
                }

                return targetDictionary;
            }
            set
            {
                targetDictionary = value;
                Settings.Default.TargetDictionary = value;
                Settings.Default.Save();
            }
        }
        private static string targetDictionary;
        private static bool isFieldLoadedTargetDictionary;

        public static bool ShowPreviews
        {
            get
            {
                if (!isFieldLoadedShowPreviews)
                {
                    showPreviews = Settings.Default.ShowPreviews;
                    isFieldLoadedShowPreviews = true;
                }

                return showPreviews;
            }
            set
            {
                showPreviews = value;
                Settings.Default.ShowPreviews = value;
                Settings.Default.Save();
            }
        }
        private static bool showPreviews;
        private static bool isFieldLoadedShowPreviews;

        public static bool AlternativeEditingKeys
        {
            get
            {
                if (!isFieldLoadedAlternativeEditingKeys)
                {
                    alternativeEditingKeys = Settings.Default.AlternativeEditingKeys;
                    isFieldLoadedAlternativeEditingKeys = true;
                }

                return alternativeEditingKeys;
            }
            set
            {
                alternativeEditingKeys = value;
                Settings.Default.AlternativeEditingKeys = value;
                Settings.Default.Save();
            }
        }
        private static bool alternativeEditingKeys;
        private static bool isFieldLoadedAlternativeEditingKeys;

        public static bool TopMost
        {
            get
            {
                if (!isFieldLoadedTopMost)
                {
                    topMost = Settings.Default.TopMost;
                    isFieldLoadedTopMost = true;
                }

                return topMost;
            }
            set
            {
                topMost = value;
                Settings.Default.TopMost = value;
                Settings.Default.Save();
                GlobalVariables.Instance.OnPropertyChanged(nameof(TopMost));
            }
        }
        private static bool topMost;
        private static bool isFieldLoadedTopMost;

        public static string[] AvailToEditFiles
        {
            get
            {
                if (!isFieldLoadedAvailToEditFiles)
                {
                    availToEditFiles = Settings.Default.AvailToEditFiles;
                    isFieldLoadedAvailToEditFiles = true;
                }

                return availToEditFiles;
            }
            set
            {
                availToEditFiles = value;
                Settings.Default.AvailToEditFiles = value;
                Settings.Default.Save();
            }
        }
        private static string[] availToEditFiles;
        private static bool isFieldLoadedAvailToEditFiles;

        public static string[] XmlRules
        {
            get
            {
                if (!isFieldLoadedXmlRules)
                {
                    xmlRules = Settings.Default.XmlRules;
                    isFieldLoadedXmlRules = true;
                }

                return xmlRules;
            }
            set
            {
                xmlRules = value;
                Settings.Default.XmlRules = value;
                Settings.Default.Save();
            }
        }
        private static string[] xmlRules;
        private static bool isFieldLoadedXmlRules;

        public static string[] ImageExtensions
        {
            get
            {
                if (!isFieldLoadedImageExtensions)
                {
                    imageExtensions = Settings.Default.ImageExtensions;
                    isFieldLoadedImageExtensions = true;
                }

                return imageExtensions;
            }
            set
            {
                imageExtensions = value;
                Settings.Default.ImageExtensions = value;
                Settings.Default.Save();
            }
        }
        private static string[] imageExtensions;
        private static bool isFieldLoadedImageExtensions;

        public static string[] OtherExtensions
        {
            get
            {
                if (!isFieldLoadedOtherExtensions)
                {
                    otherExtensions = Settings.Default.OtherExtensions;
                    isFieldLoadedOtherExtensions = true;
                }

                return otherExtensions;
            }
            set
            {
                otherExtensions = value;
                Settings.Default.OtherExtensions = value;
                Settings.Default.Save();
            }
        }
        private static string[] otherExtensions;
        private static bool isFieldLoadedOtherExtensions;

        public static bool SessionAutoTranslate
        {
            get
            {
                if (!isFieldLoadedSessionAutoTranslate)
                {
                    sessionAutoTranslate = Settings.Default.SessionAutoTranslate;
                    isFieldLoadedSessionAutoTranslate = true;
                }

                return sessionAutoTranslate;
            }
            set
            {
                sessionAutoTranslate = value;
                Settings.Default.SessionAutoTranslate = value;
                Settings.Default.Save();
            }
        }
        private static bool sessionAutoTranslate;
        private static bool isFieldLoadedSessionAutoTranslate;

        public static Guid OnlineTranslator
        {
            get
            {
                if (!isFieldLoadedOnlineTranslator)
                {
                    onlineTranslator = Settings.Default.OnlineTranslator;
                    isFieldLoadedOnlineTranslator = true;
                }

                return onlineTranslator;
            }
            set
            {
                onlineTranslator = value;
                Settings.Default.OnlineTranslator = value;
                Settings.Default.Save();
            }
        }
        private static Guid onlineTranslator;
        private static bool isFieldLoadedOnlineTranslator;

        public static string LanguageOfApp
        {
            get
            {
                if (!isFieldLoadedLanguageOfApp)
                {
                    languageOfApp = Settings.Default.LanguageOfApp;
                    isFieldLoadedLanguageOfApp = true;
                }

                return languageOfApp;
            }
            set
            {
                languageOfApp = value;
                Settings.Default.LanguageOfApp = value;
                Settings.Default.Save();
            }
        }
        private static string languageOfApp;
        private static bool isFieldLoadedLanguageOfApp;

        public static bool EmptyXml
        {
            get
            {
                if (!isFieldLoadedEmptyXml)
                {
                    emptyXml = Settings.Default.EmptyXml;
                    isFieldLoadedEmptyXml = true;
                }

                return emptyXml;
            }
            set
            {
                emptyXml = value;
                Settings.Default.EmptyXml = value;
                Settings.Default.Save();
            }
        }
        private static bool emptyXml;
        private static bool isFieldLoadedEmptyXml;

        public static bool EmptySmali
        {
            get
            {
                if (!isFieldLoadedEmptySmali)
                {
                    emptySmali = Settings.Default.EmptySmali;
                    isFieldLoadedEmptySmali = true;
                }

                return emptySmali;
            }
            set
            {
                emptySmali = value;
                Settings.Default.EmptySmali = value;
                Settings.Default.Save();
            }
        }
        private static bool emptySmali;
        private static bool isFieldLoadedEmptySmali;

        public static bool OtherFiles
        {
            get
            {
                if (!isFieldLoadedOtherFiles)
                {
                    otherFiles = Settings.Default.OtherFiles;
                    isFieldLoadedOtherFiles = true;
                }

                return otherFiles;
            }
            set
            {
                otherFiles = value;
                Settings.Default.OtherFiles = value;
                Settings.Default.Save();
            }
        }
        private static bool otherFiles;
        private static bool isFieldLoadedOtherFiles;

        public static bool OnlyXml
        {
            get
            {
                if (!isFieldLoadedOnlyXml)
                {
                    onlyXml = Settings.Default.OnlyXml;
                    isFieldLoadedOnlyXml = true;
                }

                return onlyXml;
            }
            set
            {
                onlyXml = value;
                Settings.Default.OnlyXml = value;
                Settings.Default.Save();
            }
        }
        private static bool onlyXml;
        private static bool isFieldLoadedOnlyXml;

        public static bool EmptyFolders
        {
            get
            {
                if (!isFieldLoadedEmptyFolders)
                {
                    emptyFolders = Settings.Default.EmptyFolders;
                    isFieldLoadedEmptyFolders = true;
                }

                return emptyFolders;
            }
            set
            {
                emptyFolders = value;
                Settings.Default.EmptyFolders = value;
                Settings.Default.Save();
            }
        }
        private static bool emptyFolders;
        private static bool isFieldLoadedEmptyFolders;

        public static bool Images
        {
            get
            {
                if (!isFieldLoadedImages)
                {
                    images = Settings.Default.Images;
                    isFieldLoadedImages = true;
                }

                return images;
            }
            set
            {
                images = value;
                Settings.Default.Images = value;
                Settings.Default.Save();
            }
        }
        private static bool images;
        private static bool isFieldLoadedImages;

        public static bool FilesWithErrors
        {
            get
            {
                if (!isFieldLoadedFilesWithErrors)
                {
                    filesWithErrors = Settings.Default.FilesWithErrors;
                    isFieldLoadedFilesWithErrors = true;
                }

                return filesWithErrors;
            }
            set
            {
                filesWithErrors = value;
                Settings.Default.FilesWithErrors = value;
                Settings.Default.Save();
            }
        }
        private static bool filesWithErrors;
        private static bool isFieldLoadedFilesWithErrors;

        public static bool OnlyResources
        {
            get
            {
                if (!isFieldLoadedOnlyResources)
                {
                    onlyResources = Settings.Default.OnlyResources;
                    isFieldLoadedOnlyResources = true;
                }

                return onlyResources;
            }
            set
            {
                onlyResources = value;
                Settings.Default.OnlyResources = value;
                Settings.Default.Save();
            }
        }
        private static bool onlyResources;
        private static bool isFieldLoadedOnlyResources;

        public static string Theme
        {
            get
            {
                if (!isFieldLoadedTheme)
                {
                    theme = Settings.Default.Theme;
                    isFieldLoadedTheme = true;
                }

                return theme;
            }
            set
            {
                theme = value;
                Settings.Default.Theme = value;
                Settings.Default.Save();
            }
        }
        private static string theme;
        private static bool isFieldLoadedTheme;

        public static string ApktoolVersion
        {
            get
            {
                if (!isFieldLoadedApktoolVersion)
                {
                    apktoolVersion = Settings.Default.ApktoolVersion;
                    isFieldLoadedApktoolVersion = true;
                }

                return apktoolVersion;
            }
            set
            {
                apktoolVersion = value;
                Settings.Default.ApktoolVersion = value;
                Settings.Default.Save();
            }
        }
        private static string apktoolVersion;
        private static bool isFieldLoadedApktoolVersion;

        public static bool EditorWindow_SaveToDict
        {
            get
            {
                if (!isFieldLoadedEditorWindow_SaveToDict)
                {
                    editorWindow_SaveToDict = Settings.Default.EditorWindow_SaveToDict;
                    isFieldLoadedEditorWindow_SaveToDict = true;
                }

                return editorWindow_SaveToDict;
            }
            set
            {
                editorWindow_SaveToDict = value;
                Settings.Default.EditorWindow_SaveToDict = value;
                Settings.Default.Save();
            }
        }
        private static bool editorWindow_SaveToDict;
        private static bool isFieldLoadedEditorWindow_SaveToDict;

        public static bool EditorWMaximized
        {
            get
            {
                if (!isFieldLoadedEditorWMaximized)
                {
                    editorWMaximized = Settings.Default.EditorWMaximized;
                    isFieldLoadedEditorWMaximized = true;
                }

                return editorWMaximized;
            }
            set
            {
                editorWMaximized = value;
                Settings.Default.EditorWMaximized = value;
                Settings.Default.Save();
            }
        }
        private static bool editorWMaximized;
        private static bool isFieldLoadedEditorWMaximized;

        public static bool MainWMaximized
        {
            get
            {
                if (!isFieldLoadedMainWMaximized)
                {
                    mainWMaximized = Settings.Default.MainWMaximized;
                    isFieldLoadedMainWMaximized = true;
                }

                return mainWMaximized;
            }
            set
            {
                mainWMaximized = value;
                Settings.Default.MainWMaximized = value;
                Settings.Default.Save();
            }
        }
        private static bool mainWMaximized;
        private static bool isFieldLoadedMainWMaximized;

        public static Point MainWindowSize
        {
            get
            {
                if (!isFieldLoadedMainWindowSize)
                {
                    mainWindowSize = Settings.Default.MainWindowSize;
                    isFieldLoadedMainWindowSize = true;
                }

                return mainWindowSize;
            }
            set
            {
                mainWindowSize = value;
                Settings.Default.MainWindowSize = value;
                Settings.Default.Save();
            }
        }
        private static Point mainWindowSize;
        private static bool isFieldLoadedMainWindowSize;

        public static bool EditorSOnlyFullWords
        {
            get
            {
                if (!isFieldLoadedEditorSOnlyFullWords)
                {
                    editorSOnlyFullWords = Settings.Default.EditorSOnlyFullWords;
                    isFieldLoadedEditorSOnlyFullWords = true;
                }

                return editorSOnlyFullWords;
            }
            set
            {
                editorSOnlyFullWords = value;
                Settings.Default.EditorSOnlyFullWords = value;
                Settings.Default.Save();
            }
        }
        private static bool editorSOnlyFullWords;
        private static bool isFieldLoadedEditorSOnlyFullWords;

        public static bool EditorSMatchCase
        {
            get
            {
                if (!isFieldLoadedEditorSMatchCase)
                {
                    editorSMatchCase = Settings.Default.EditorSMatchCase;
                    isFieldLoadedEditorSMatchCase = true;
                }

                return editorSMatchCase;
            }
            set
            {
                editorSMatchCase = value;
                Settings.Default.EditorSMatchCase = value;
                Settings.Default.Save();
            }
        }
        private static bool editorSMatchCase;
        private static bool isFieldLoadedEditorSMatchCase;

        public static bool ShowNotifications
        {
            get
            {
                if (!isFieldLoadedShowNotifications)
                {
                    showNotifications = Settings.Default.ShowNotifications;
                    isFieldLoadedShowNotifications = true;
                }

                return showNotifications;
            }
            set
            {
                showNotifications = value;
                Settings.Default.ShowNotifications = value;
                Settings.Default.Save();
            }
        }
        private static bool showNotifications;
        private static bool isFieldLoadedShowNotifications;

        public static bool AlternatingRows
        {
            get
            {
                if (!isFieldLoadedAlternatingRows)
                {
                    alternatingRows = Settings.Default.AlternatingRows;
                    isFieldLoadedAlternatingRows = true;
                }

                return alternatingRows;
            }
            set
            {
                alternatingRows = value;
                Settings.Default.AlternatingRows = value;
                Settings.Default.Save();
            }
        }
        private static bool alternatingRows;
        private static bool isFieldLoadedAlternatingRows;
    }
}
