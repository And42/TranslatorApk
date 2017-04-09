using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using TranslatorApk.Annotations;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.PluginItems;
using TranslatorApk.Properties;
using UsefulFunctionsLib;

namespace TranslatorApk.Logic.OrganisationItems
{
    public class GlobalVariables : INotifyPropertyChanged
    {
        public static GlobalVariables Instance { get; } = new GlobalVariables();

        static GlobalVariables()
        {
#if DEBUG
            PathToExe = Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) + "\\Release\\" + Path.GetFileName(Assembly.GetExecutingAssembly().Location);
#else
            PathToExe = Assembly.GetExecutingAssembly().Location;
#endif

            PathToStartFolder       = Path.GetDirectoryName(PathToExe);
            PathToFiles             = PathToStartFolder + "\\Files";
            PathToResources         = PathToFiles + "\\Resources";

            PathToApktoolVersions   = PathToResources + "\\apktools";
            PathToAdminScripter     = PathToStartFolder + "\\AdminScripter.exe";
            PathToPlugins           = PathToFiles + "\\Plugins"; 
            PathToFlags             = PathToResources + "\\Flags";
            PathToLogs              = PathToStartFolder + "\\Logs";

            EditableFileExtenstions = new[] { ".xml", ".smali" };
            ProgramVersion          = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Portable                = File.Exists(PathToStartFolder + "\\isportable");

            ThemesMap.Add("Light", Resources.Localizations.Resources.Theme_Light);
            ThemesMap.Add("Dark", Resources.Localizations.Resources.Theme_Dark);

            SourceDictionaries = Settings.Default.SourceDictionaries != null 
                ? new ObservableCollection<CheckableString>(Settings.Default.SourceDictionaries) 
                : new ObservableCollection<CheckableString>();
        }

        #region Readonly properties

        /// <summary>
        /// Словарь переводов сессии
        /// </summary>
        public static readonly Dictionary<string, string> SessionDictionary = new Dictionary<string, string>();

        /// <summary>
        /// Путь к exe файлу
        /// </summary>
        public static readonly string PathToExe;

        /// <summary>
        /// Путь к папке с exe файлом
        /// </summary>
        public static readonly string PathToStartFolder;

        /// <summary>
        /// StartFolder\Files
        /// </summary>
        public static readonly string PathToFiles;

        /// <summary>
        /// StartFolder\Files\Plugins
        /// </summary>
        public static readonly string PathToPlugins;

        /// <summary>
        /// StartFolder\Files\Resources
        /// </summary>
        public static readonly string PathToResources;

        /// <summary>
        /// StartFolder\Files\Resources\Flags
        /// </summary>
        public static readonly string PathToFlags;

        /// <summary>
        /// StartFolder\Files\Resources\apktools
        /// </summary>
        public static readonly string PathToApktoolVersions;

        /// <summary>
        /// Путь к логам
        /// </summary>
        public static readonly string PathToLogs;

        /// <summary>
        /// Поддерживаемые для редактирования расширения файлов
        /// </summary>
        public static readonly string[] EditableFileExtenstions;

        /// <summary>
        /// Возвращает путь к вспомогательной программе
        /// </summary>
        public static readonly string PathToAdminScripter;

        /// <summary>
        /// Версия программы
        /// </summary>
        public static readonly string ProgramVersion;

        /// <summary>
        /// Возвращает, является ли текущая версия портативной
        /// </summary>
        public static readonly bool Portable;

        public static readonly Map<string, string> ThemesMap = new Map<string, string>();

        /// <summary>
        /// Словарь плагинов
        /// </summary>
        public static readonly Dictionary<string, PluginHost> Plugins = new Dictionary<string, PluginHost>();

        #endregion

        /// <summary>
        /// Путь к текущему apktool.jar
        /// </summary>
        public static string CurrentApktoolPath => $"{PathToApktoolVersions}\\apktool_{SettingsIncapsuler.ApktoolVersion}.jar";

        /// <summary>
        /// Текущий сервис перевода
        /// </summary>
        public static OneTranslationService CurrentTranslationService { get; set; }

        #region Consts

        /// <summary>
        /// Агент Mozilla для WebClient
        /// </summary>
        public const string MozillaAgent = "Mozilla/4.0 (compatible; MSIE 6.0b; Windows NT 5.1)";

        public const string LogLine = "------------------------------";

        #endregion

        #region Properties with INotifyPropertyChanged

        /// <summary>
        /// Папка текущего проекта
        /// </summary>
        public string CurrentProjectFolderProp
        {
            get
            {
                return _currentProjectFolder;
            }
            set
            {
                _currentProjectFolder = value;
                OnPropertyChanged(nameof(CurrentProjectFolderProp));
            }
        }
        private string _currentProjectFolder;

        /// <summary>
        /// Файл текущего проекта (.apk)
        /// </summary>
        public string CurrentProjectFileProp
        {
            get
            {
                return _currentProjectFile;
            }
            set
            {
                _currentProjectFile = value;
                CurrentProjectFolderProp = UsefulFunctions_FileInfo.GetFullFNWithoutExt(value);
                OnPropertyChanged(nameof(CurrentProjectFileProp));
            }
        }
        private string _currentProjectFile;

        /// <summary>
        /// Папка текущего проекта
        /// </summary>
        public static string CurrentProjectFolder
        {
            get { return Instance.CurrentProjectFolderProp; }
            set { Instance.CurrentProjectFolderProp = value; }
        }

        /// <summary>
        /// Файл текущего проекта (.apk)
        /// </summary>
        public static string CurrentProjectFile
        {
            get { return Instance.CurrentProjectFileProp; }
            set { Instance.CurrentProjectFileProp = value; }
        }

        public static ObservableCollection<CheckableString> SourceDictionaries { get; }

        /// <summary>
        /// Определяет, должно ли окно всегда находиться поверх других окон
        /// </summary>
        public bool TopMost
        {
            get { return SettingsIncapsuler.TopMost; }
            set
            {
                SettingsIncapsuler.TopMost = value;
                OnPropertyChanged(nameof(TopMost));
            }
        }

#endregion

        #region Settings

        /// <summary>
        /// Settings.Default.FoldersOfLanguages as list
        /// </summary>
        public static List<string> Settings_FoldersOfLanguages { get; set; } = Properties.Resources.FoldersOfLanguages.Split('|').ToList();

        /// <summary>
        /// Settings.Default.NamesOfFolderLanguages as list
        /// </summary>
        public static List<string> Settings_NamesOfFolderLanguages { get; set; } = Resources.Localizations.Resources.NamesOfFolderLanguages.Split('|').ToList();

        #endregion

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        public virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
