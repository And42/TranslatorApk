using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;
using AndroidTranslator;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.ScrollAxis;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.EventManagerLogic;
using TranslatorApk.Logic.Events;
using TranslatorApk.Logic.Interfaces;
using TranslatorApk.Logic.PluginItems;
using TranslatorApk.Logic.WebServices;
using TranslatorApk.Windows;
using TranslatorApkPluginLib;
using UsefulClasses;
using UsefulFunctionsLib;

using LocRes = TranslatorApk.Resources.Localizations.Resources;
using Settings = TranslatorApk.Properties.Settings;
using SetInc = TranslatorApk.Logic.OrganisationItems.SettingsIncapsuler;

namespace TranslatorApk.Logic.OrganisationItems
{
    public static class Functions
    {
        //todo: Исправить ошибку от Aid5 (IconHandler.IconFromExtension(item.Options.Ext, IconSize.Large))

        private static bool _canLoadIcons = true;

        /// <summary>
        /// Загружает иконку для TreeViewItem
        /// </summary>
        /// <param name="item">Целевой объект</param>
        public static void LoadIconForItem(TreeViewNodeModel item)
        {
            if (item.Options.IsImageLoaded)
                return;

            ImageSource icon;

            if (item.Options.IsFolder)
            {
                icon = GlobalResources.Icon_FolderVerticalOpen;
            }
            else
            {
                if (_canLoadIcons)
                {
                    try
                    {
                        icon = LoadIconFromFile(item.Options);
                    }
                    catch (RuntimeWrappedException)
                    {
                        icon = GlobalResources.Icon_UnknownFile;
                        _canLoadIcons = false;
                    }
                }
                else
                {
                    icon = GlobalResources.Icon_UnknownFile;
                }
            }

            if (icon != null)
            {
                item.Image = icon;
            }

            item.Options.IsImageLoaded = true;
        }

        /// <summary>
        /// Загружает изображение из файла, указанного в объекте типа <see cref="Options"/>
        /// </summary>
        /// <param name="file">Объект для обработки</param>
        private static ImageSource LoadIconFromFile(Options file)
        {
            if (SettingsIncapsuler.ImageExtensions.Contains(file.Ext))
            {
                try
                {
                    var image = LoadImageFromFile(file.FullPath);
                    file.HasPreview = true;
                    return image;
                }
                catch (NotSupportedException)
                {
                    return ShellIcon.IconToImageSource(ShellIcon.GetLargeIcon(file.FullPath));
                }
            }

            return ShellIcon.IconToImageSource(ShellIcon.GetLargeIcon(file.FullPath));
        }

        /// <summary>
        /// Загружает изображение из файла
        /// </summary>
        /// <param name="fileName">Файл</param>
        private static BitmapImage LoadImageFromFile(string fileName)
        {
            return new BitmapImage(new Uri(fileName));
        }

        /// <summary>
        /// Проверяет текущий файл на соответствие настройкам
        /// </summary>
        /// <param name="file">Файл</param>
        /// <param name="extension">Расширение файла</param>
        private static bool CheckFileWithSettings(string file, string extension)
        {
            if (extension != ".xml" && SettingsIncapsuler.OnlyXml)
                return false;

            if (extension == ".xml")
            {
                if (!SettingsIncapsuler.EmptyXml && XmlFile.Create(file).Details?.Count == 0)
                    return false;
            }
            else if (extension == ".smali")
            {
                if (!SettingsIncapsuler.EmptySmali && !SmaliFile.HasLines(file))
                    return false;
            }
            else if (SettingsIncapsuler.ImageExtensions.Contains(extension))
            {
                if (!SettingsIncapsuler.Images)
                    return false;
            }
            else if (SettingsIncapsuler.OtherExtensions.Contains(extension))
            {
                if (!SettingsIncapsuler.OtherFiles)
                    return false;
            }

            return true;
        }

        public static void LoadFilesToTreeView(Dispatcher dispatcher, string pathToFolder, IHaveChildren root, bool showEmptyFolders, CancellationTokenSource cts = null, Action oneFileAdded = null)
        {
            if (cts?.IsCancellationRequested == true)
                return;

            var files = Directory.EnumerateFiles(pathToFolder, "*", SearchOption.TopDirectoryOnly);
            var folders = Directory.EnumerateDirectories(pathToFolder, "*", SearchOption.TopDirectoryOnly);

            foreach (var folder in folders)
            {
                if (!CheckFilePath(folder))
                    continue;

                var item = new TreeViewNodeModel(root)
                {
                    Name = Path.GetFileName(folder),
                    Options = new Options(folder)
                };

                int index = GlobalVariables.Settings_FoldersOfLanguages.FindIndex(nm => item.Name.StartsWith(nm, StringComparison.Ordinal));

                if (File.Exists($"{GlobalVariables.PathToFlags}\\{item.Name}.png"))
                {
                    dispatcher.InvokeAction(() =>
                    {
                        item.Image = GetFlagImage(item.Name);
                    });

                    item.Options.IsImageLoaded = true;
                }

                if (index > -1)
                {
                    string found = GlobalVariables.Settings_FoldersOfLanguages[index];

                    string source = item.Name;

                    item.Name = GlobalVariables.Settings_NamesOfFolderLanguages[index];

                    if (found != source)
                        item.Name += $" ({source.Split('-').Last().TrimStart('r')})";
                }

                dispatcher.InvokeAction(() => root.Children.Add(item));
                LoadFilesToTreeView(dispatcher, folder, item, showEmptyFolders, cts, oneFileAdded);

                if (item.Children.Count == 0 && !showEmptyFolders)
                    dispatcher.InvokeAction(() => root.Children.Remove(item));
            }

            foreach (var file in files)
            {
                if (!CheckFilePath(file)) continue;

                if (cts?.IsCancellationRequested == true) return;

                oneFileAdded?.Invoke();

                if (!CheckFileWithSettings(file, Path.GetExtension(file)))
                    continue;

                var item = new TreeViewNodeModel(root)
                {
                    Name = Path.GetFileName(file),
                    Options = new Options(file)
                };

                dispatcher.InvokeAction(() => root.Children.Add(item));
            }
        }

        /// <summary>
        /// Возвращает флаг указанного языка
        /// </summary>
        /// <param name="title">Язык</param>
        public static BitmapImage GetFlagImage(string title)
        {
            string file = $"{GlobalVariables.PathToFlags}\\{title}.png";

            if (File.Exists(file))
                return new BitmapImage(new Uri(file));

            return null;
            //return GetImageFromApp($"/Resources/Flags/{title}.png");
        }

        /// <summary>
        /// Возвращает изображение из ресурсов программы
        /// </summary>
        /// <param name="pathInApp">Путь в ресурсах</param>
        public static BitmapImage GetImageFromApp(string pathInApp)
        {
            return new BitmapImage(new Uri(pathInApp, UriKind.Relative));
        }

        /// <summary>
        /// Возвращает все не повторяющиеся названия атрибутов из файла
        /// </summary>
        /// <param name="node">Начальный уровень</param>
        /// <param name="existingAtrs">Существующие атрибуты</param>
        public static List<string> GetAllAttributes(XmlNode node, IEnumerable<string> existingAtrs = null)
        {
            var result = new HashSet<string>(existingAtrs ?? Enumerable.Empty<string>());
            GetAllAttributesProcess(node, result);
            return result.ToList();
        }

        public static void GetAllAttributesProcess(XmlNode node, HashSet<string> result)
        {
            if (node.Attributes != null)
                foreach (XmlAttribute attrib in node.Attributes)
                    result.Add(attrib.Name);

            foreach (XmlNode child in node.ChildNodes)
                GetAllAttributesProcess(child, result);
        }

        /// <summary>
        /// Обновляет API ключи переводчиков в настройках
        /// </summary>
        public static void UpdateSettingsApiKeys()
        {
            SerializableStringDictionary table = Settings.Default.TranslatorServicesKeys;

            foreach (var service in TranslateService.OnlineTranslators)
            {
                string guid = service.Key.ToString();

                if (table.ContainsKey(guid))
                    table[guid] = service.Value.ApiKey;
                else
                    table.Add(guid, service.Value.ApiKey);
            }
        }

        /// <summary>
        /// Обновляет API ключи переводчиков из настроек
        /// </summary>
        public static void UpdateApiKeys()
        {
            SerializableStringDictionary table = Settings.Default.TranslatorServicesKeys;

            foreach (DictionaryEntry item in table)
            {
                string itemKey = (string) item.Key;

                OneTranslationService found;

                if (TranslateService.OnlineTranslators.TryGetValue(Guid.Parse(itemKey), out found))
                    found.ApiKey = table[itemKey];
            }
        }

        /// <summary>
        /// Загружает настройки программы
        /// </summary>
        public static void LoadSettings()
        {
            if (Settings.Default.UpdateNeeded)
            {
                Settings.Default.Upgrade();
                Settings.Default.UpdateNeeded = false;
                Settings.Default.Save();
            }
                    
            if (SetInc.TargetLanguage == "")
                SetInc.TargetLanguage = "ru";

            if (SetInc.OnlineTranslator == Guid.Empty)
                SetInc.OnlineTranslator = TranslateService.OnlineTranslators.First().Key;

            if (Settings.Default.TranslatorServicesKeys == null)
                Settings.Default.TranslatorServicesKeys = new SerializableStringDictionary();
            
            if (SetInc.LanguageOfApp == "")
                SetLanguageOfApp(TranslateService.SupportedProgramLangs.FirstOrDefault(lang => lang == GetCurrentLanguage()) ?? TranslateService.SupportedProgramLangs.First());
            else
                SetLanguageOfApp(SetInc.LanguageOfApp);

            if (SetInc.XmlRules == null || SetInc.XmlRules.Length == 0)
            {
                SetInc.XmlRules = new[]
                {
                    "android:text", "android:title", "android:summary", "android:dialogTitle", 
                    "android:summaryOff", "android:summaryOn", "value"
                };
            }

            if (SetInc.AvailToEditFiles == null || SetInc.AvailToEditFiles.Length == 0)
            {
                SetInc.AvailToEditFiles = new[] {".xml", ".smali"};
            }

            if (SetInc.ImageExtensions == null || SetInc.ImageExtensions.Count == 0 ||
                SetInc.ImageExtensions.Count == 1 && SetInc.ImageExtensions.First().NE())
            {
                SetInc.ImageExtensions = new HashSet<string> {".png", ".jpg", ".jpeg"};
            }

            if (SetInc.OtherExtensions == null)
            {
                SetInc.OtherExtensions = new HashSet<string>();
            }

            XmlFile.XmlRules = SetInc.XmlRules.ToList();
            EditorWindow.Languages = TranslateService.LongTargetLanguages;

            // todo: Убрать в будущих версиях

            string source;

            if (GlobalVariables.ThemesMap.TryGetKey(SetInc.Theme, out source))
                SetInc.Theme = source;

            // ---

            if (SetInc.Theme.NE() || !GlobalVariables.ThemesMap.ContainsKey(SetInc.Theme))
                SetInc.Theme = GlobalVariables.ThemesMap.First().Key;

            ChangeTheme(SetInc.Theme);

            TranslateService.LongTargetLanguages = new ReadOnlyCollection<string>(LocRes.OnlineTranslationsLongLanguages.Split('|'));

            string apktoolVersion = SetInc.ApktoolVersion;

            if (apktoolVersion.NE() || !File.Exists($"{GlobalVariables.PathToApktoolVersions}\\apktool_{apktoolVersion}.jar"))
            {
                string vers = Directory.EnumerateFiles(GlobalVariables.PathToApktoolVersions, "*.jar").LastOrDefault();

                if (vers != null)
                    vers = Path.GetFileNameWithoutExtension(vers).SplitFR("apktool_")[0];

                SetInc.ApktoolVersion = vers;
            }

            UpdateApiKeys();

            Settings.Default.Save();
        }

        /// <summary>
        /// Меняет тему на одну из доступных
        /// </summary>
        /// <param name="name">Light / Dark</param>
        public static void ChangeTheme(string name)
        {
            var dicts = Application.Current.Resources.MergedDictionaries;

            if (!dicts.Any(d =>
            {
                string path = d.Source?.OriginalString;

                if (String.IsNullOrEmpty(path))
                    return false;

                var split = path.Split('/');

                return split.Length > 1 && split[1] == name;
            }))
            {
                string[] themesToAdd;

                switch (name)
                {
                    case "Light":
                        themesToAdd = new[] {"Themes/Light/ThemeResources.xaml"};
                        break;
                    default:
                        themesToAdd = new[] {"Themes/Dark/ThemeResources.xaml"};
                        break;
                }

                foreach (var theme in themesToAdd)
                {
                    dicts.Insert(1, new ResourceDictionary
                    {
                        Source = new Uri(theme, UriKind.Relative)
                    });
                }

                // ReSharper disable once ForCanBeConvertedToForeach
                for (int i = 0; i < themesToAdd.Length; i++)
                    dicts.RemoveAt(1 + themesToAdd.Length);

                SetInc.Theme = name;
            }
        }

        private static bool _pluginsLoaded;
        /// <summary>
        /// Загружает плагины из папки (вызывается только 1 раз)
        /// </summary>
        public static void LoadPlugins()
        {
            if (_pluginsLoaded)
                return;

            if (!Directory.Exists(GlobalVariables.PathToPlugins))
            {
                LoadCurrentTranslationService();

                return;
            }

            var plugins = Directory.EnumerateFiles(GlobalVariables.PathToPlugins, "*.dll", SearchOption.TopDirectoryOnly);

            plugins.ForEach(LoadPlugin);

            _pluginsLoaded = true;

            LoadCurrentTranslationService();
        }

        /// <summary>
        /// Устанавливает текущий сервис перевода, основываясь на настройках
        /// </summary>
        private static void LoadCurrentTranslationService()
        {
            if (TranslateService.OnlineTranslators.TryGetValue(SetInc.OnlineTranslator, out var found))
            {
                GlobalVariables.CurrentTranslationService = found;
            }
            else
            {
                SetInc.OnlineTranslator = TranslateService.OnlineTranslators.First().Key;
                GlobalVariables.CurrentTranslationService = TranslateService.OnlineTranslators.First().Value;
            }
        }

        /// <summary>
        /// Загружает плагин
        /// </summary>
        /// <param name="path">Путь к файлу</param>
        public static void LoadPlugin(string path)
        {
            var appDomain = AppDomain.CreateDomain(Path.GetFileNameWithoutExtension(path));

            var loader = (PluginHost)
                appDomain.CreateInstanceAndUnwrap(typeof(PluginHost).Assembly.FullName,
                    typeof(PluginHost).FullName);

            loader.Load(path);

            loader.Actions.ForEach(it => AddAction(loader, it));
            
            loader.Translators.ForEach(AddTranslator);

            GlobalVariables.Plugins.Add(loader.Name, loader);
        }

        /// <summary>
        /// Выгружает плагин
        /// </summary>
        /// <param name="name">Название плагина</param>
        public static void UnloadPlugin(string name)
        {
            PluginHost found;

            if (!GlobalVariables.Plugins.TryGetValue(name, out found))
                return;

            found.Actions.ForEach(RemoveAction);
            found.Translators.ForEach(RemoveTranslator);

            string pluginName = found.Name;

            AppDomain.Unload(found.Domain);

            GlobalVariables.Plugins.Remove(pluginName);
        }

        /// <summary>
        /// Добавляет действие в меню
        /// </summary>
        /// <param name="host"></param>
        /// <param name="action">Действие</param>
        public static void AddAction(PluginHost host, IAdditionalAction action)
        {
            MainWindow.Instance.AddActionToMenu(new PluginPart<IAdditionalAction>(host, action));
        }

        /// <summary>
        /// Удаляет действие из меню
        /// </summary>
        /// <param name="action"></param>
        public static void RemoveAction(IAdditionalAction action)
        {
            MainWindow.Instance.RemoveActionFromMenu(action.Guid);
        }

        /// <summary>
        /// Добавляет сервис перевода в список доступных сервисов
        /// </summary>
        /// <param name="translator">Сервис перевода</param>
        public static void AddTranslator(ITranslateService translator)
        {
            TranslateService.OnlineTranslators.Add(translator.Guid, new OneTranslationService(translator));
        }

        /// <summary>
        /// Удаляет сервис перевода из списка доступных сервисов
        /// </summary>
        /// <param name="translator">Сервис перевода</param>
        public static void RemoveTranslator(ITranslateService translator)
        {
            TranslateService.OnlineTranslators.Remove(translator.Guid);
        }

        /// <summary>
        /// Добавляет или заменяет перевод в словаре переводов текущей сессии, если активирована соответствующая настройка
        /// </summary>
        /// <param name="oldText">Старый текст</param>
        /// <param name="newText">Новый текст</param>
        public static void AddToSessionDict(string oldText, string newText)
        {
            if (!SettingsIncapsuler.SessionAutoTranslate)
                return;

            if (GlobalVariables.SessionDictionary.ContainsKey(oldText))
                GlobalVariables.SessionDictionary[oldText] = newText;
            else
                GlobalVariables.SessionDictionary.Add(oldText, newText);
        }

        /// <summary>
        /// Загружает файл в редактор
        /// </summary>
        /// <param name="pathToFile">Файл</param>
        public static void LoadFile(string pathToFile)
        {
            var ext = Path.GetExtension(pathToFile);

            if (!GlobalVariables.EditableFileExtenstions.Contains(ext))
            {
                Process.Start(pathToFile);
                return;
            }
            
            EditableFile file;

            switch (ext)
            {
                case ".xml":
                    file = XmlFile.Create(pathToFile);
                    break;
                default: //".smali":
                    file = new SmaliFile(pathToFile);
                    break;
            }

            WindowManager.ActivateWindow<EditorWindow>();

            ManualEventManager.GetEvent<AddEditableFilesEvent>()
                .Publish(new AddEditableFilesEvent(file));

            ManualEventManager.GetEvent<EditorScrollToFileAndSelectEvent>()
                .Publish(new EditorScrollToFileAndSelectEvent(f => f.FileName.Equals(file.FileName, StringComparison.Ordinal)));
        }

        /// <summary>
        /// Возвращает текущий язык программы
        /// </summary>
        public static string GetCurrentLanguage()
        {
            return Thread.CurrentThread.CurrentUICulture.Name.Split('-')[0];
        }

        /// <summary>
        /// Устанавливает текущий язык программы
        /// </summary>
        /// <param name="language"></param>
        /// <param name="showDialog"></param>
        public static void SetLanguageOfApp(string language, bool showDialog = false)
        {
            if (TranslateService.SupportedProgramLangs.All(lang => lang != language))
                return;

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(language);
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(language);
            SetInc.LanguageOfApp = language;
            
            if (showDialog && MessBox.ShowDial(LocRes.RestartProgramToApplyLanguage, null,
                MessBox.MessageButtons.Yes, MessBox.MessageButtons.No) ==
                MessBox.MessageButtons.Yes)
            {
                Process.Start(GlobalVariables.PathToExe);
                Environment.Exit(0);
            }

            TranslateService.LongTargetLanguages = new ReadOnlyCollection<string>(LocRes.OnlineTranslationsLongLanguages.Split('|'));
        }

        /// <summary>
        /// Показ окна "Открыть с помощью"
        /// </summary>
        /// <param name="file">Файл, для которого вызывается окно</param>
        public static void OpenAs(string file)
        {
            Process.Start("rundll32.exe", "shell32.dll,OpenAs_RunDLL " + file);
        }

        /// <summary>
        /// Проверяет наличие обновлений на сервере
        /// </summary>
        public static void CheckForUpdate()
        {
            Task.Factory.StartNew(() =>
            {
                string newVersion;

                if (!TryFunc(() => DownloadString("http://things.pixelcurves.info/Pages/Updates.aspx?cmd=trapk_version"), out newVersion))
                    return;

                if (CompareVersions(newVersion, GlobalVariables.ProgramVersion) != 1)
                    return;

                string changesLink;

                switch (Thread.CurrentThread.CurrentUICulture.TwoLetterISOLanguageName.ToLower())
                {
                    case "en":
                        changesLink = "http://things.pixelcurves.info/Pages/Updates.aspx?cmd=trapk_changes&language=en"; break;
                    default:
                        changesLink = "http://things.pixelcurves.info/Pages/Updates.aspx?cmd=trapk_changes"; break;
                }

                string changes = DownloadString(changesLink);

                Application.Current.Dispatcher.InvokeAction(() =>
                {
                    new UpdateWindow(GlobalVariables.ProgramVersion, changes).ShowDialog();
                });
            });
        }

        /// <summary>
        /// Проверяет длину пути
        /// </summary>
        /// <param name="path">Путь</param>
        public static bool CheckFilePath(string path)
        {
            if (path == null)
                return false;

            return path.Length < 260 && Path.GetDirectoryName(path)?.Length < 248;
        }

        /// <summary>
        /// Сравнивает две версии файлов в строковом формате (1.3.4.5)
        /// </summary>
        /// <param name="first">Первая версия</param>
        /// <param name="second">Вторая версия</param>
        public static int CompareVersions(string first, string second)
        {
            List<int> SplitInts(string str) => str.Split('.').Select(int.Parse).ToList();

            var firstR = SplitInts(first);
            var secondR = SplitInts(second);

            int maxLen = Math.Max(firstR.Count, secondR.Count);

            while (firstR.Count < maxLen)
                firstR.Add(0);

            while (secondR.Count < maxLen)
                secondR.Add(0);

            for (int i = 0; i < maxLen; i++)
            {
                if (firstR[i] != secondR[i])
                    return firstR[i] > secondR[i] ? 1 : -1;
            }

            return 0;
        }

        /// <summary>
        /// Показывает файл или директорию в проводнике (если директория, то список файлов внутри неё)
        /// </summary>
        /// <param name="fileOrDirectory">Файл или директория</param>
        public static void ShowInExplorer(string fileOrDirectory)
        {
            if (Directory.Exists(fileOrDirectory))
                Process.Start(fileOrDirectory);
            else if (File.Exists(fileOrDirectory))
                Process.Start("explorer.exe", "/select, " + fileOrDirectory);
        }

        /// <summary>
        /// Возвращает, есть ли у текущего пользователя права администратора
        /// </summary>
        public static bool IsAdmin()
        {
            WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            return pricipal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// Запускает программу с правами администратора
        /// </summary>
        /// <param name="fileName">Путь к программе</param>
        /// <param name="arguments">Аргументы командной строки</param>
        /// <returns>True, если запуск успешен, false, если нет</returns>
        public static bool RunAsAdmin(string fileName, string arguments)
        {
            #pragma warning disable 168

            return RunAsAdmin(fileName, arguments, out Process process);

            #pragma warning restore 168
        }

        /// <summary>
        /// Запускает программу с правами администратора
        /// </summary>
        /// <param name="fileName">Путь к программе</param>
        /// <param name="arguments">Аргументы командной строки</param>
        /// <param name="process">Запущенный процесс</param>
        /// <returns>True, если запуск успешен, false, если нет</returns>
        public static bool RunAsAdmin(string fileName, string arguments, out Process process)
        {
            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                Verb = "runas",
                FileName = fileName,
                Arguments = arguments
            };

            try
            {
                process = Process.Start(processInfo);
                return true;
            }
            catch (Win32Exception)
            {
                process = null;
                return false;
            }
        }

        /// <summary>
        /// Проверяет наличие у программы прав на запись в каталог программы
        /// </summary>
        public static bool CheckRights()
        {
            try
            {
                string file = GlobalVariables.PathToStartFolder + "\\temp";
                File.WriteAllText(file, @"test");
                File.Delete(file);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        /// <summary>
        /// Перезапускает программу
        /// </summary>
        public static void Restart()
        {
            Process.Start(GlobalVariables.PathToExe);
            Environment.Exit(0);
        }

        /// <summary>
        /// Проверяет, являются ли данные в указанном потоке словарём
        /// </summary>
        /// <param name="stream">Поток</param>
        public static bool IsDictionaryFile(Stream stream)
        {
            var xDoc = new XmlDocument();

            try
            {
                xDoc.Load(stream);
            }
            catch (XmlException)
            {
                return false;
            }

            var docElem = xDoc.DocumentElement;

            return docElem?.Name == "translations" && docElem.Attributes["name"]?.Value == "AeDict";
        }

        /// <summary>
        /// Проверяет, является ли указанный файл словарём
        /// </summary>
        /// <param name="file">Файл</param>
        public static bool IsDictionaryFile(string file)
        {
            if (Path.GetExtension(file) != ".xml")
                return false;

            using (var stream = File.OpenRead(file))
                return IsDictionaryFile(stream);
        }

        /// <summary>
        /// Возвращает версию dll на диске
        /// </summary>
        /// <param name="pathToDll">Путь к dll</param>
        public static string GetDllVersion(string pathToDll)
        {
            return FileVersionInfo.GetVersionInfo(pathToDll).FileVersion;
        }
        
        /// <summary>
        /// Возвращает изменяемый файл, соответствующий файлу на диске, или null, если подходящий файл не найден
        /// </summary>
        /// <param name="filePath">Путь к файлу на диске</param>
        public static EditableFile GetSuitableEditableFile(string filePath)
        {
            switch (Path.GetExtension(filePath))
            {
                case ".xml":
                    if (IsDictionaryFile(filePath))
                        return new DictionaryFile(filePath);
                    return XmlFile.Create(filePath);
                case ".smali":
                    return new SmaliFile(filePath);
            }

            return null;
        }

        /// <summary>
        /// Добавляет в коллекцию непустой элемент
        /// </summary>
        /// <typeparam name="T">Тип элементов в коллекции</typeparam>
        /// <param name="collection">Коллекция</param>
        /// <param name="item">Элемент</param>
        public static void AddIfNotNull<T>(this ICollection<T> collection, T item) where T : class
        {
            if (item != null)
                collection.Add(item);
        }

        /// <summary>
        /// Десериализует объект из строки, содержащий XML
        /// </summary>
        /// <typeparam name="T">Тип десериализуемого объекта</typeparam>
        /// <param name="item">Строка, содержащая XML</param>
        public static T DeserializeXml<T>(string item)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

            using (var reader = new StringReader(item))
                return (T)xmlSerializer.Deserialize(reader);
        }

        /// <summary>
        /// Сериализует объект в строку, содержащую XML представление указанного объекта
        /// </summary>
        /// <typeparam name="T">Тип объекта для сериализации</typeparam>
        /// <param name="item">Объект для сериализации</param>
        public static string SerializeXml<T>(T item)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

            using (var writer = new StringWriter())
            {
                xmlSerializer.Serialize(writer, item);

                return writer.ToString();
            }
        }

        /// <summary>
        /// Выполняет действие и возвращает его результат
        /// </summary>
        /// <param name="func">Действие</param>
        /// <param name="result">Результат действия</param>
        /// <returns>True, если всё прошло успешно, false, если во время выполнения возникло исключение</returns>
        public static bool TryFunc<T>(Func<T> func, out T result)
        {
            try
            {
                result = func();
                return true;
            }
            catch (Exception)
            {
                result = default(T);
                return false;
            }
        }

        /// <summary>
        /// Выполняет действие
        /// </summary>
        /// <param name="action">Действие</param>
        /// <returns>True, если всё прошло успешно, false, если во время выполнения возникло исключение</returns>
        public static bool TryAction(Action action)
        {
            try
            {
                action();
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        /// <summary>
        /// Выполняет действие в Dispatcher'e (чуть сокращает код вызова благодаря неявному приведению типов)
        /// </summary>
        /// <param name="dispatcher">Dispatcher</param>
        /// <param name="action">Действие</param>
        public static void InvokeAction(this Dispatcher dispatcher, Action action)
        {
            dispatcher.Invoke(action);
        }

        /// <summary>
        /// Время ожидания ответа от сервера при загрузке данных по умолчанию
        /// </summary>
        private const int DefaultTimeout = 50000;

        /// <summary>
        /// Загружает страницу в виде текста по ссылке
        /// </summary>
        /// <param name="link">Ссылка</param>
        /// <param name="timeout">Время ожидания ответа от сервера</param>
        public static string DownloadString(string link, int timeout = DefaultTimeout)
        {
            var client = (HttpWebRequest)WebRequest.Create(link);

            client.UserAgent = GlobalVariables.MozillaAgent;
            client.Timeout = client.ReadWriteTimeout = timeout;

            Stream stream = client.GetResponse().GetResponseStream();

            if (stream == null)
                return string.Empty;

            using (var strread = new StreamReader(stream))
                return strread.ReadToEnd();
        }

        /// <summary>
        /// Асинхронно загружает страницу в виде текста по ссылке
        /// </summary>
        /// <param name="link">Ссылка</param>
        /// <param name="timeout">Время ожидания ответа от сервера</param>
        public static Task<string> DownloadStringAsync(string link, int timeout = DefaultTimeout)
        {
            return Task<string>.Factory.StartNew(() => DownloadString(link));
        }

        /// <summary>
        /// Переводит указанный текст с помощью онлайн переводчика, основываясь на текущих настройках
        /// </summary>
        /// <param name="text">Текст для перевода</param>
        public static string TranslateTextWithSettings(string text)
        {
            return GlobalVariables.CurrentTranslationService.Translate(text, SettingsIncapsuler.TargetLanguage);
        }

        /// <summary>
        /// Выполняет финальные действия при завершении программы
        /// </summary>
        public static void ExitActions()
        {
            Settings.Default.SourceDictionaries = GlobalVariables.SourceDictionaries.ToArray();

            UpdateSettingsApiKeys();

            Settings.Default.Save();
        }

        #region Методы расширения для SfDataGrid

        /// <summary>
        /// Возвращает <see cref="DetailsViewManager"/> таблицы
        /// </summary>
        /// <param name="grid">Таблица, объект которой нужно получить</param>
        public static DetailsViewManager GetViewManager(this SfDataGrid grid)
        {
            var propertyInfo = grid.GetType().GetField("DetailsViewManager", BindingFlags.Instance | BindingFlags.NonPublic);
            return propertyInfo?.GetValue(grid) as DetailsViewManager;
        }

        /// <summary>
        /// Возвращает таблицу вложенных элементов для таблицы, при этом раскрывая её и проматывая до её начала
        /// </summary>
        /// <param name="grid">Таблица, таблицу вложенных элементов которой нужно получить</param>
        /// <param name="relationalColumn">Название столбца с подробностями</param>
        /// <param name="recordIndex">Позиция записи в таблице</param>
        /// <param name="detailsColumn">Номер столбца в таблице вложенных элементов</param>
        public static SfDataGrid GetDetailsViewGridWUpd(this SfDataGrid grid, string relationalColumn, int recordIndex, int detailsColumn = 1)
        {
            int rowIndex = grid.ResolveToRowIndex(recordIndex);
            RecordEntry record = grid.View.Records[recordIndex];

            if (!record.IsExpanded)
                grid.ExpandDetailsViewAt(recordIndex);

            int vIndex = grid.DetailsViewDefinition.FindIndex(it => it.RelationalColumn == relationalColumn) + rowIndex + 1;

            grid.ScrollInView(new RowColumnIndex(vIndex, detailsColumn));

            var view = grid.GetDetailsViewGrid(recordIndex, relationalColumn);

            if (view != null)
                return view;

            grid.GetViewManager().BringIntoView(vIndex);

            grid.ScrollInView(new RowColumnIndex(rowIndex, detailsColumn));

            return grid.GetDetailsViewGrid(recordIndex, relationalColumn);
        }

        /// <summary>
        /// Перемещает позицию в таблице на позицию необходимой строки и выделяет найденную строку
        /// </summary>
        /// <param name="grid">Таблица для обработки</param>
        /// <param name="filePredicate">Проверка на корректность файла</param>
        /// <param name="stringPredicate">Проверка на корректность строки</param>
        public static void ScrollToFileAndSelectString(this SfDataGrid grid, Predicate<EditableFile> filePredicate,
            Predicate<OneString> stringPredicate)
        {
            int parentIndex = grid.View.Records.FindIndex(it => filePredicate(it.Data.As<EditableFile>()));

            var detailsGrid = grid.GetDetailsViewGridWUpd("Details", parentIndex);

            int childIndex = detailsGrid.View.Records.FindIndex(it => stringPredicate(it.Data as OneString));

            detailsGrid.SelectedIndex = childIndex;

            var container = grid.GetVisualContainer();

            container.ScrollRows.ScrollInView(grid.ResolveToRowIndex(parentIndex));

            for (int i = 0; i < childIndex; i++)
                container.ScrollRows.ScrollToNextLine();
        }

        /// <summary>
        /// Перемещает позицию в таблице на позицию необходимого файла и выделяет найденный файл
        /// </summary>
        /// <param name="grid">Таблица для обработки</param>
        /// <param name="filePredicate">Проверка на корректность файла</param>
        /// <param name="expandRecord">Нужно ли разворачивать таблицу вложенных элементов у файла</param>
        public static void ScrollToFileAndSelect(this SfDataGrid grid, Predicate<EditableFile> filePredicate, bool expandRecord = true)
        {
            int parentIndex = grid.View.Records.FindIndex(it => filePredicate(it.Data.As<EditableFile>()));

            if (expandRecord)
                grid.ExpandDetailsViewAt(parentIndex);

            grid.SelectedIndex = parentIndex;

            var container = grid.GetVisualContainer();

            container.ScrollRows.ScrollInView(grid.ResolveToRowIndex(parentIndex));
        }

        #endregion

        /// <summary>
        /// Создаёт делегат указанного метода
        /// </summary>
        /// <typeparam name="T">Тип делегата</typeparam>
        /// <param name="obj">Объект, содержащий метод</param>
        /// <param name="method">Метод</param>
        public static T CreateDelegate<T>(object obj, string method) where T : class 
        {
            return Delegate.CreateDelegate(typeof(T), obj, method) as T;
        }

        /// <summary>
        /// Выполняет метод в объекте, используя рефлексию
        /// </summary>
        /// <typeparam name="T">Тип возвращаемого значения</typeparam>
        /// <param name="type">Тип, в котором выполняется поиск метода</param>
        /// <param name="obj">Объект, у которого вызывается метод</param>
        /// <param name="name">Название метода</param>
        /// <param name="parameters">Параметры метода</param>
        public static T ExecRefl<T>(Type type, object obj, string name, params object[] parameters)
        {
            return type.GetMethod(name).Invoke(obj, parameters).As<T>();
        }
    }
}