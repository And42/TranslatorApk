using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using Bugsnag;
using MVVM_Tools.Code.Providers;
using SettingsManager;
using SettingsManager.ModelProcessors;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.PluginItems;
using TranslatorApk.Logic.Utils;
using TranslatorApk.Resources.Localizations;

namespace TranslatorApk.Logic.OrganisationItems
{
    internal class GlobalVariables
    {
        public static GlobalVariables Instance { get; } = new();

        static GlobalVariables()
        {
            PathToExe = Assembly.GetExecutingAssembly().Location;
#if DEBUG
            PathToStartFolder       = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "Release", "net6.0-windows");
#else
            PathToStartFolder       = Path.GetDirectoryName(PathToExe);
#endif
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

            AppSettings =
                new SettingsBuilder<AppSettings>()
                    .WithFile(
                        Path.Combine(
                            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                            Assembly.GetExecutingAssembly().GetName().Name,
                            "appSettings.json"
                        )
                    )
                    .WithProcessor(new JsonModelProcessor())
                    .Build();

            Themes = new (string name, string localizedName)[]
            {
                (ThemeUtils.ThemeLight, StringResources.Theme_Light),
                (ThemeUtils.ThemeDark,  StringResources.Theme_Dark)
            };

            SourceDictionaries = 
                new ObservableCollection<CheckableSetting>(
                    AppSettings.SourceDictionaries ?? Enumerable.Empty<CheckableSetting>()
                );
        }

        /// <summary>
        /// Файл текущего проекта (.apk)
        /// </summary>
        public FieldProperty<string> CurrentProjectFile { get; } = new();

        /// <summary>
        /// Папка текущего проекта
        /// </summary>
        public FieldProperty<string> CurrentProjectFolder { get; } = new();

        /// <summary>
        /// Текущий сервис перевода
        /// </summary>
        public FieldProperty<OneTranslationService> CurrentTranslationService { get; } = new();

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
        public Dictionary<string, string> SessionDictionary { get; } = new();

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
        public static readonly Dictionary<string, PluginHost> Plugins = new();

        #endregion

        public static AppSettings AppSettings { get; }

        public static Client BugSnagClient { get; } = new("6cefaf3c36c7e256621bdb6d09c4d599");

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
