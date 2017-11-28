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
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Serialization;
using AndroidTranslator.Classes.Exceptions;
using AndroidTranslator.Classes.Files;
using AndroidTranslator.Interfaces.Files;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.EventManagerLogic;
using TranslatorApk.Logic.Events;
using TranslatorApk.Logic.Interfaces;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.WebServices;
using TranslatorApk.Windows;
using UsefulFunctionsLib;

using LocRes = TranslatorApk.Resources.Localizations.Resources;
using Settings = TranslatorApk.Properties.Settings;
using SetInc = TranslatorApk.Logic.OrganisationItems.SettingsIncapsuler;

namespace TranslatorApk.Logic.Utils
{
    public static class Utils
    {
        /// <summary>
        /// Проверяет текущий файл на соответствие настройкам
        /// </summary>
        /// <param name="file">Файл</param>
        /// <param name="extension">Расширение файла</param>
        private static bool CheckFileWithSettings(string file, string extension)
        {
            if (extension != ".xml" && SetInc.Instance.OnlyXml)
                return false;

            if (extension == ".xml")
            {
                if (!SetInc.Instance.EmptyXml)
                {
                    try
                    {
                        var details = XmlFile.Create(file).Details;

                        return details != null && details.Count != 0;
                    }
                    catch (XmlParserException)
                    {
                        return false;
                    }
                }
            }
            else if (extension == ".smali")
            {
                if (!SetInc.Instance.EmptySmali && !SmaliFile.HasLines(file))
                    return false;
            }
            else if (SetInc.Instance.ImageExtensions.Contains(extension))
            {
                if (!SetInc.Instance.Images)
                    return false;
            }
            else if (SetInc.Instance.OtherExtensions.Contains(extension))
            {
                if (!SetInc.Instance.OtherFiles)
                    return false;
            }

            return true;
        }

        public static void LoadFilesToTreeView(Dispatcher dispatcher, string pathToFolder, ICommand refreshFilesListCommand, IHaveChildren root,
            bool showEmptyFolders, CancellationTokenSource cts = null, Action oneFileAdded = null)
        {
            var flagFiles =
                new HashSet<string>(
                    Directory.EnumerateFiles(GlobalVariables.PathToFlags, "*.png")
                        .Select(Path.GetFileNameWithoutExtension)
                );

            LoadFilesToTreeViewInternal(dispatcher, pathToFolder, refreshFilesListCommand, root, showEmptyFolders, cts, oneFileAdded, flagFiles);
        }

        private static void LoadFilesToTreeViewInternal(Dispatcher dispatcher, string pathToFolder, ICommand refreshFilesListCommand, IHaveChildren root, bool showEmptyFolders, CancellationTokenSource cts, Action oneFileAdded, HashSet<string> flagFiles)
        {
            if (cts?.IsCancellationRequested == true)
                return;

            IEnumerable<string> files = Directory.EnumerateFiles(pathToFolder, "*", SearchOption.TopDirectoryOnly);
            IEnumerable<string> folders = Directory.EnumerateDirectories(pathToFolder, "*", SearchOption.TopDirectoryOnly);

            List<TreeViewNodeModel> itemsToAdd = new List<TreeViewNodeModel>();

            try
            {
                foreach (string folder in folders)
                {
                    if (cts?.IsCancellationRequested == true)
                        return;

                    // folder length
                    if (!CheckFilePath(folder))
                        continue;

                    var item = new TreeViewNodeModel(refreshFilesListCommand, root)
                    {
                        Name = Path.GetFileName(folder),
                        Options = new Options(folder, true)
                    };

                    var (index, languageFolder) = GlobalVariables.SettingsFoldersOfLanguages.FindWithIndex(nm =>
                        item.Name.StartsWith(nm, StringComparison.Ordinal));

                    void SetFolder(string path)
                    {
                        dispatcher.InvokeAction(() =>
                        {
                            item.Image = ImageUtils.GetFlagImage(path);
                        });

                        item.Options.IsImageLoaded = true;
                    }

                    // folder has it's own flag
                    if (flagFiles.Contains(item.Name))
                        SetFolder(item.Name);
                    else if (index != -1 && flagFiles.Contains(languageFolder))
                        SetFolder(languageFolder);

                    // folder is a language folder
                    if (index > -1)
                    {
                        string found = GlobalVariables.SettingsFoldersOfLanguages[index];

                        string source = item.Name;

                        item.Name = GlobalVariables.SettingsNamesOfFolderLanguages[index];

                        if (found != source)
                            item.Name += $" ({source.Substring(languageFolder.Length).TrimStart('-').TrimStart('r')})";
                    }

                    LoadFilesToTreeViewInternal(dispatcher, folder, refreshFilesListCommand, item, showEmptyFolders,
                        cts, oneFileAdded, flagFiles);

                    if (item.Children.Count != 0 || showEmptyFolders)
                        itemsToAdd.Add(item);
                }

                foreach (string file in files)
                {
                    if (cts?.IsCancellationRequested == true)
                        return;

                    if (!CheckFilePath(file))
                        continue;

                    oneFileAdded?.Invoke();

                    if (!CheckFileWithSettings(file, Path.GetExtension(file)))
                        continue;

                    var item = new TreeViewNodeModel(refreshFilesListCommand, root)
                    {
                        Name = Path.GetFileName(file),
                        Options = new Options(file, false)
                    };

                    itemsToAdd.Add(item);
                }
            }
            finally
            {
                dispatcher.InvokeAction(() => root.Children.AddRange(itemsToAdd));
            }
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
                    
            if (SetInc.Instance.TargetLanguage == "")
                SetInc.Instance.TargetLanguage = "ru";

            if (SetInc.Instance.OnlineTranslator == Guid.Empty)
                SetInc.Instance.OnlineTranslator = TranslateService.OnlineTranslators.First().Key;

            if (Settings.Default.TranslatorServicesKeys == null)
                Settings.Default.TranslatorServicesKeys = new SerializableStringDictionary();
            
            if (SetInc.Instance.LanguageOfApp == "")
                SetLanguageOfApp(TranslateService.SupportedProgramLangs.FirstOrDefault(lang => lang == GetCurrentLanguage()) ?? TranslateService.SupportedProgramLangs.First());
            else
                SetLanguageOfApp(SetInc.Instance.LanguageOfApp);

            if (SetInc.Instance.XmlRules == null || SetInc.Instance.XmlRules.Length == 0)
            {
                SetInc.Instance.XmlRules = new[]
                {
                    "android:text", "android:title", "android:summary", "android:dialogTitle", 
                    "android:summaryOff", "android:summaryOn", "value"
                };
            }

            if (SetInc.Instance.AvailToEditFiles == null || SetInc.Instance.AvailToEditFiles.Length == 0)
            {
                SetInc.Instance.AvailToEditFiles = new[] {".xml", ".smali"};
            }

            if (SetInc.Instance.ImageExtensions == null || 
                SetInc.Instance.ImageExtensions.Length == 0 ||
                SetInc.Instance.ImageExtensions.Length == 1 && SetInc.Instance.ImageExtensions[0].NE())
            {
                SetInc.Instance.ImageExtensions = new[] {".png", ".jpg", ".jpeg"};
            }

            if (SetInc.Instance.OtherExtensions == null)
            {
                SetInc.Instance.OtherExtensions = new string[0];
            }

            XmlFile.XmlRules = SetInc.Instance.XmlRules.ToList();
            EditorWindow.Languages = TranslateService.LongTargetLanguages;

            // todo: Убрать в будущих версиях

            string source;

            if (GlobalVariables.ThemesMap.TryGetKey(SetInc.Instance.Theme, out source))
                SetInc.Instance.Theme = source;

            // ---

            if (SetInc.Instance.Theme.NE() || !GlobalVariables.ThemesMap.ContainsKey(SetInc.Instance.Theme))
                SetInc.Instance.Theme = GlobalVariables.ThemesMap.First().Key;

            ChangeTheme(SetInc.Instance.Theme);

            TranslateService.LongTargetLanguages = new ReadOnlyCollection<string>(LocRes.OnlineTranslationsLongLanguages.Split('|'));

            string apktoolVersion = SetInc.Instance.ApktoolVersion;

            if (apktoolVersion.NE() || !File.Exists(Path.Combine(GlobalVariables.PathToApktoolVersions, $"apktool_{apktoolVersion}.jar")))
            {
                string vers = Directory.EnumerateFiles(GlobalVariables.PathToApktoolVersions, "*.jar").LastOrDefault();

                if (vers != null)
                    vers = Path.GetFileNameWithoutExtension(vers).SplitFR("apktool_")[0];

                SetInc.Instance.ApktoolVersion = vers;
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

                SetInc.Instance.Theme = name;
            }
        }

        /// <summary>
        /// Добавляет или заменяет перевод в словаре переводов текущей сессии, если активирована соответствующая настройка
        /// </summary>
        /// <param name="oldText">Старый текст</param>
        /// <param name="newText">Новый текст</param>
        public static void AddToSessionDict(string oldText, string newText)
        {
            if (!SetInc.Instance.SessionAutoTranslate)
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
            
            IEditableFile file;

            try
            {
                switch (ext)
                {
                    case ".xml":
                        file = XmlFile.Create(pathToFile);
                        break;
                    default: //".smali":
                        file = new SmaliFile(pathToFile);
                        break;
                }
            }
            catch (IOException ex)
            {
                // todo: add to string resources
                MessBox.ShowDial($"Не удалось загрузить файл из-за ошибки системы.\nСообщение: {ex.Message}", Resources.Localizations.Resources.ErrorLower);
                return;
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
            SetInc.Instance.LanguageOfApp = language;
            
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
            var pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());

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
            return RunAsAdmin(fileName, arguments, out Process _);
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
                string file = Path.Combine(GlobalVariables.PathToStartFolder, "temp");
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

            XmlElement docElem = xDoc.DocumentElement;

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

            using (FileStream stream = File.OpenRead(file))
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
        public static IEditableFile GetSuitableEditableFile(string filePath)
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
            var xmlSerializer = new XmlSerializer(typeof(T));

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
            var xmlSerializer = new XmlSerializer(typeof(T));

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
                result = default;
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
            return GlobalVariables.CurrentTranslationService.Translate(text, SetInc.Instance.TargetLanguage);
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
            // ReSharper disable once PossibleNullReferenceException
            return type.GetMethod(name).Invoke(obj, parameters).As<T>();
        }

        public static bool Contains<T>(this T[] array, T value)
        {
            return Array.IndexOf(array, value) != -1;
        }

        public static bool SetProperty<TClass, TPropType>(this TClass sender, ref TPropType storage, TPropType value, [CallerMemberName] string propertyName = null) where TClass : IRaisePropertyChanged
        {
            if (EqualityComparer<TPropType>.Default.Equals(storage, value))
                return false;

            storage = value;
            sender.RaisePropertyChanged(propertyName);

            return true;
        }

        public static bool SetRefProperty<TClass, TPropType>(this TClass sender, ref TPropType storage, TPropType value, [CallerMemberName] string propertyName = null) where TClass : IRaisePropertyChanged where TPropType : class
        {
            if (ReferenceEquals(storage, value))
                return false;

            storage = value;
            sender.RaisePropertyChanged(propertyName);

            return true;
        }

        public static IEnumerable<TRecord> SortWithDescriptions<TRecord>(IEnumerable<TRecord> collection, IEnumerable<SortDescription> sortDescriptions)
        {
            var recType = typeof(TRecord);

            foreach (SortDescription sortDesc in sortDescriptions)
            {
                var prop = recType.GetProperty(sortDesc.PropertyName);

                if (prop == null)
                    throw new NullReferenceException($"Property `{sortDesc.PropertyName}` was not found in `{recType.FullName}`");

                collection = 
                    sortDesc.Direction == ListSortDirection.Ascending 
                        ? collection.OrderBy(rec => prop.GetValue(rec, null)) 
                        : collection.OrderByDescending(rec => prop.GetValue(rec, null));
            }

            return collection;
        }

        /// <summary>
        /// Select method with error handling
        /// </summary>
        /// <typeparam name="TSource">Source collection type</typeparam>
        /// <typeparam name="TResult">Target collection type</typeparam>
        /// <param name="source">Source collection</param>
        /// <param name="selector">Value converter</param>
        /// <param name="onFail">Called when converting an item causes an exception</param>
        public static IEnumerable<TResult> SelectSafe<TSource, TResult>(this IEnumerable<TSource> source,
            Func<TSource, TResult> selector, Action<TSource, Exception> onFail = null)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            IEnumerable<TResult> _()
            {
                foreach (TSource it in source)
                {
                    TResult res;

                    try
                    {
                        res = selector(it);
                    }
                    catch (Exception ex)
                    {
                        onFail?.Invoke(it, ex);

                        continue;
                    }

                    yield return res;
                }
            }

            return _();
        }

        /// <summary>
        /// Returns files from drag event argsuments
        /// </summary>
        /// <param name="args">Arguments</param>
        public static string[] GetFilesDrop(this DragEventArgs args)
        {
            return args.Data.GetData(DataFormats.FileDrop) as string[];
        }

        /// <summary>
        /// Freezes object if can
        /// </summary>
        /// <typeparam name="T">Freezable object type</typeparam>
        /// <param name="obj">Object to freeze</param>
        public static T FreezeIfCan<T>(this T obj) where T : Freezable
        {
            if (obj.CanFreeze)
                obj.Freeze();

            return obj;
        }

        public static (int index, T value) FindWithIndex<T>(this IEnumerable<T> collection, Predicate<T> predicate)
        {
            if (collection == null)
                throw new ArgumentNullException(nameof(collection));
            if (predicate == null)
                throw new ArgumentNullException(nameof(predicate));

            int index = 0;

            foreach (T item in collection)
            {
                if (predicate(item))
                    return (index, item);

                index++;
            }

            return (-1, default);
        }
    }
}