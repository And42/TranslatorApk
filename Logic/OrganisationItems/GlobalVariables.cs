using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.Interfaces;
using TranslatorApk.Logic.PluginItems;
using TranslatorApk.Properties;
using UsefulFunctionsLib;

namespace TranslatorApk.Logic.OrganisationItems
{
    public class GlobalVariables : IRaisePropertyChanged
    {
        public static GlobalVariables Instance { get; } = new GlobalVariables();

        static GlobalVariables()
        {
#if DEBUG
            PathToExe = 
                Path.Combine(
                    Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) ?? string.Empty,
                    "Release",
                    Path.GetFileName(Assembly.GetExecutingAssembly().Location ?? string.Empty)
                );
#else
            PathToExe = Assembly.GetExecutingAssembly().Location;
#endif

            PathToStartFolder       = Path.GetDirectoryName(PathToExe) ?? string.Empty;
            PathToFiles             = Path.Combine(PathToStartFolder, "Files");
            PathToResources         = Path.Combine(PathToFiles, "Resources");

            PathToApktoolVersions   = Path.Combine(PathToResources, "apktools");
            PathToAdminScripter     = Path.Combine(PathToStartFolder, "AdminScripter.exe");
            PathToPlugins           = Path.Combine(PathToFiles, "Plugins"); 
            PathToFlags             = Path.Combine(PathToResources, "Flags");
            PathToLogs              = Path.Combine(PathToStartFolder, "Logs");

            EditableFileExtenstions = new[] { ".xml", ".smali" };
            ProgramVersion          = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Portable                = File.Exists(Path.Combine(PathToStartFolder, "isportable"));

            ThemesMap.Add("Light", Resources.Localizations.Resources.Theme_Light);
            ThemesMap.Add("Dark", Resources.Localizations.Resources.Theme_Dark);

            SourceDictionaries = 
                Settings.Default.SourceDictionaries != null 
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
        public static string CurrentApktoolPath => 
            Path.Combine(PathToApktoolVersions, $"apktool_{SettingsIncapsuler.Instance.ApktoolVersion}.jar");

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
            get => _currentProjectFolder;
            set => this.SetProperty(ref _currentProjectFolder, value);
        }
        private string _currentProjectFolder;

        /// <summary>
        /// Файл текущего проекта (.apk)
        /// </summary>
        public string CurrentProjectFileProp
        {
            get => _currentProjectFile;
            set
            {
                if (this.SetProperty(ref _currentProjectFile, value))
                    CurrentProjectFolderProp = UsefulFunctions_FileInfo.GetFullFNWithoutExt(value);
            }
        }
        private string _currentProjectFile;

        /// <summary>
        /// Папка текущего проекта
        /// </summary>
        public static string CurrentProjectFolder
        {
            get => Instance.CurrentProjectFolderProp;
            set => Instance.CurrentProjectFolderProp = value;
        }

        /// <summary>
        /// Файл текущего проекта (.apk)
        /// </summary>
        public static string CurrentProjectFile
        {
            get => Instance.CurrentProjectFileProp;
            set => Instance.CurrentProjectFileProp = value;
        }

        public static ObservableCollection<CheckableString> SourceDictionaries { get; }

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

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
