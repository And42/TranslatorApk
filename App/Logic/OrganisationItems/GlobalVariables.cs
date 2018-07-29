using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Bugsnag;
using MVVM_Tools.Code.Providers;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.PluginItems;
using TranslatorApk.Resources.Localizations;

namespace TranslatorApk.Logic.OrganisationItems
{
    internal class GlobalVariables
    {
        public static GlobalVariables Instance { get; } = new GlobalVariables();

        static GlobalVariables()
        {
#if DEBUG
            PathToExe = 
                Path.Combine(
                    Path.GetDirectoryName(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)) ?? string.Empty,
                    "Release",
                    Path.GetFileName(Assembly.GetExecutingAssembly().Location)
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

            Themes = new (string name, string localizedName)[]
            {
                ("Light", StringResources.Theme_Light),
                ("Dark",  StringResources.Theme_Dark)
            };

            SourceDictionaries = 
                new ObservableCollection<CheckableSetting>(
                    AppSettings.SourceDictionaries ?? Enumerable.Empty<CheckableSetting>()
                );
        }

        /// <summary>
        /// Файл текущего проекта (.apk)
        /// </summary>
        public Property<string> CurrentProjectFile { get; } = new Property<string>();

        /// <summary>
        /// Папка текущего проекта
        /// </summary>
        public Property<string> CurrentProjectFolder { get; } = new Property<string>();

        /// <summary>
        /// Текущий сервис перевода
        /// </summary>
        public Property<OneTranslationService> CurrentTranslationService { get; } = new Property<OneTranslationService>();

        private GlobalVariables()
        {
            CurrentProjectFile.PropertyChanged += (sender, args) =>
            {
                string file = CurrentProjectFile.Value;
                CurrentProjectFolder.Value = file.Remove(file.Length - Path.GetExtension(file).Length);
            };
        }

        #region Readonly properties

        /// <summary>
        /// Словарь переводов сессии
        /// </summary>
        public Dictionary<string, string> SessionDictionary { get; } = new Dictionary<string, string>();

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

        public static readonly (string name, string localizedName)[] Themes;

        /// <summary>
        /// Словарь плагинов
        /// </summary>
        public static readonly Dictionary<string, PluginHost> Plugins = new Dictionary<string, PluginHost>();

        #endregion

        public static AppSettingsBase AppSettings { get; } = new JsonSettingsContainer();

        public static Client BugSnagClient { get; } = new Client("6cefaf3c36c7e256621bdb6d09c4d599");

        /// <summary>
        /// Путь к текущему apktool.jar
        /// </summary>
        public static string CurrentApktoolPath => 
            Path.Combine(PathToApktoolVersions, $"apktool_{AppSettings.ApktoolVersion}.jar");

        #region Consts

        /// <summary>
        /// Агент Mozilla для WebClient
        /// </summary>
        public const string MozillaAgent = "Mozilla/4.0 (compatible; MSIE 6.0b; Windows NT 5.1)";

        #endregion

        public static ObservableCollection<CheckableSetting> SourceDictionaries { get; }
    }
}
