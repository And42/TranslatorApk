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
using System.Text;
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
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.PluginItems;
using TranslatorApk.Logic.WebServices;
using TranslatorApk.Windows;
using TranslatorApkPluginLib;
using UsefulClasses;
using UsefulFunctionsLib;

using LocRes = TranslatorApk.Resources.Localizations.Resources;
using Settings = TranslatorApk.Properties.Settings;
using SetInc = TranslatorApk.Logic.SettingsIncapsuler;

namespace TranslatorApk.Logic
{
    public static class Functions
    {
        //todo: Исправить ошибку от Aid5 (IconHandler.IconFromExtension(item.Options.Ext, IconSize.Large))

        private static bool canLoadIcons = true;

        /// <summary>
        /// Загружает иконку для TreeViewItem
        /// </summary>
        /// <param name="item">Целевой объект</param>
        public static void LoadIconForItem(TreeViewNodeModel item)
        {
            if (item.Options.IsImageLoaded) return;

            ImageSource icon;

            if (item.Options.IsFolder)
                icon = GlobalResources.Icon_FolderVerticalOpen;
            else
            {
                if (canLoadIcons)
                {
                    try
                    {
                        icon = LoadIconFromFile(item.Options);
                    }
                    catch (RuntimeWrappedException)
                    {
                        icon = GlobalResources.Icon_UnknownFile;
                        canLoadIcons = false;
                    }
                }
                else
                    icon = GlobalResources.Icon_UnknownFile;
            }
            if (icon != null) item.Image = icon;

            item.Options.IsImageLoaded = true;
        }

        private static ImageSource LoadIconFromFile(Options file)
        {
            if (SettingsIncapsuler.ImageExtensions.Any(e => e == file.Ext))
            {
                try
                {
                    var image = LoadPreviewFromFile(file.FullPath);
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

        private static BitmapImage LoadPreviewFromFile(string fileName)
        {
            return new BitmapImage(new Uri(fileName));
        }

        private static bool CheckFileWithSettings(string file, string extension)
        {
            if (SettingsIncapsuler.OnlyXml && extension != ".xml")
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
            else if (SettingsIncapsuler.ImageExtensions.Any(ext => ext == extension))
            {
                if (!SettingsIncapsuler.Images)
                    return false;
            }
            else if (SettingsIncapsuler.OtherExtensions.Any(ext => ext == extension))
            {
                if (!SettingsIncapsuler.OtherFiles)
                    return false;
            }

            return true;
        }

        public static void LoadFilesToTreeView(Dispatcher dispatcher, string pathToFolder, IHaveChildren root, bool showEmptyFolders, CancellationTokenSource cts = null,
            Action oneFileAdded = null)
        {
            if (cts?.IsCancellationRequested == true)
                return;

            var files = Directory.EnumerateFiles(pathToFolder, "*", SearchOption.TopDirectoryOnly);
            var folders = Directory.EnumerateDirectories(pathToFolder, "*", SearchOption.TopDirectoryOnly);

            foreach (var folder in folders)
            {
                if (!CheckFilePath(folder)) continue;

                var item = new TreeViewNodeModel(root)
                {
                    Name = Path.GetFileName(folder),
                    Options = new Options(folder)
                };

                int index = GlobalVariables.Settings_FoldersOfLanguages.FindIndex(nm => item.Name.StartsWith(nm, StringComparison.Ordinal));

                if (index > -1)
                {
                    string found = GlobalVariables.Settings_FoldersOfLanguages[index];

                    dispatcher.InvokeAction(() =>
                    {
                        item.Image = GetFlagImage(found);
                    });

                    string source = item.Name;

                    item.Name = GlobalVariables.Settings_NamesOfFolderLanguages[index];

                    if (found != source)
                        item.Name += $" ({source.Split('-').Last().TrimStart('r')})";

                    item.Options.IsImageLoaded = true;
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

        public static BitmapImage GetFlagImage(string title)
        {
            return GetImageFromApp($"/Resources/Flags/{title}.png");
        }

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

            if (SetInc.ImageExtensions == null || SetInc.ImageExtensions.Length == 0 ||
                SetInc.ImageExtensions.Length == 1 && SetInc.ImageExtensions[0].NE())
            {
                SetInc.ImageExtensions = new[] {".png", ".jpg", ".jpeg"};
            }

            if (SetInc.OtherExtensions == null)
            {
                SetInc.OtherExtensions = new string[0];
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

            TranslateService.LongTargetLanguages = new ReadOnlyCollection<string>
            (LocRes.OnlineTranslationsLongLanguages.Split('|'));

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

        public static void ChangeTheme(string name)
        {
            var dicts = Application.Current.Resources.MergedDictionaries;

            if (!dicts.Any(d =>
            {
                string path = d.Source?.OriginalString;

                if (path.NE())
                    return false;

                // ReSharper disable once PossibleNullReferenceException
                var split = path.Split('/');

                return split.Length > 1 && split[1] == name;
            }))
            {
                string[] themesToAdd;

                // ReSharper disable once ConvertIfStatementToConditionalTernaryExpression

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
                    dicts.Insert(1, new ResourceDictionary
                    {
                        Source = new Uri(theme, UriKind.Relative)
                    });
                
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
                return;

            string[] plugins = Directory.GetFiles(GlobalVariables.PathToPlugins, "*.dll", SearchOption.TopDirectoryOnly);

            plugins.ForEach(LoadPlugin);

            _pluginsLoaded = true;

            OneTranslationService found;

            if (!TranslateService.OnlineTranslators.TryGetValue(SetInc.OnlineTranslator, out found))
            {
                SetInc.OnlineTranslator = TranslateService.OnlineTranslators.First().Key;
                GlobalVariables.CurrentTranslationService = TranslateService.OnlineTranslators.First().Value;
            }
            else
            {
                GlobalVariables.CurrentTranslationService = found;
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

        public static void AddAction(PluginHost host, IAdditionalAction action)
        {
            MainWindow.Instance.AddActionToMenu(new PluginPart<IAdditionalAction>(host, action));
        }

        public static void RemoveAction(IAdditionalAction action)
        {
            MainWindow.Instance.RemoveActionFromMenu(action.Guid);
        }

        public static void AddTranslator(ITranslateService translator)
        {
            TranslateService.OnlineTranslators.Add(translator.Guid, new OneTranslationService(translator));
        }

        public static void RemoveTranslator(ITranslateService translator)
        {
            TranslateService.OnlineTranslators.Remove(translator.Guid);
        }

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

            ManualEventManager.GetEvent<EditFilesEvent>()
                .Publish(new EditFilesEvent(file));

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

            TranslateService.LongTargetLanguages = new ReadOnlyCollection<string>
            (LocRes.OnlineTranslationsLongLanguages.Split('|'));
        }

        /// <summary>
        /// Показ окна "Открыть с помощью"
        /// </summary>
        /// <param name="file"></param>
        public static void OpenAs(string file)
        {
            Process.Start("rundll32.exe", "shell32.dll,OpenAs_RunDLL " + file);
        }

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
            Func<string, List<int>> splitInts = str => str.Split('.').Select(int.Parse).ToList();

            var firstR = splitInts(first);
            var secondR = splitInts(second);

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

        public static bool IsAdmin()
        {
            WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            return pricipal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        public static bool RunAsAdmin(string fileName, string arguments)
        {
            return RunAsAdmin(fileName, arguments, out Process process);
        }

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

        public static void Restart()
        {
            Process.Start(GlobalVariables.PathToExe);
            Environment.Exit(0);
        }

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

        public static bool IsDictionaryFile(string file)
        {
            if (Path.GetExtension(file) != ".xml")
                return false;

            using (var stream = File.OpenRead(file))
                return IsDictionaryFile(stream);
        }

        public static string GetDllVersion(string pathToDll)
        {
            return FileVersionInfo.GetVersionInfo(pathToDll).FileVersion;
        }
        
        public static EditableFile GetSuitableEditableFile(string filePath)
        {
            switch (Path.GetExtension(filePath))
            {
                case ".xml":
                    {
                        if (IsDictionaryFile(filePath))
                            return new DictionaryFile(filePath);
                        return XmlFile.Create(filePath);
                    }
                case ".smali":
                    return new SmaliFile(filePath);
            }

            return null;
        }

        public static void AddIfNotNull<T>(this ICollection<T> collection, T item) where T : class
        {
            if (item != null)
                collection.Add(item);
        }

        public static T DeserializeXml<T>(string item)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

            using (var reader = new StringReader(item))
                return (T)xmlSerializer.Deserialize(reader);
        }

        public static string SerializeXml<T>(T item)
        {
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));

            using (var writer = new StringWriter())
            {
                xmlSerializer.Serialize(writer, item);

                return writer.ToString();
            }
        }

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

        public static void InvokeAction(this Dispatcher obj, Action action)
        {
            obj.Invoke(action);
        }

        public static string DownloadString(string link)
        {
            using (WebClient client = new WebClient {Encoding = Encoding.UTF8})
                return client.DownloadString(link);
        }

        public static Task<string> DownloadStringAsync(string link)
        {
            return Task<string>.Factory.StartNew(() => DownloadString(link));
        }

        public static string TranslateTextWithSettings(string text)
        {
            return GlobalVariables.CurrentTranslationService.Translate(text, SettingsIncapsuler.TargetLanguage);
        }

        public static void ExitActions()
        {
            Settings.Default.SourceDictionaries = GlobalVariables.SourceDictionaries.ToArray();

            UpdateSettingsApiKeys();

            Settings.Default.Save();
        }

        public static DetailsViewManager GetViewManager(this SfDataGrid grid)
        {
            var propertyInfo = grid.GetType().GetField("DetailsViewManager", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            return propertyInfo?.GetValue(grid) as DetailsViewManager;
        }

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

        public static void ScrollToFileAndSelect(this SfDataGrid grid, Predicate<EditableFile> filePredicate, bool expandRecord = true)
        {
            int parentIndex = grid.View.Records.FindIndex(it => filePredicate(it.Data.As<EditableFile>()));

            if (expandRecord)
                grid.ExpandDetailsViewAt(parentIndex);

            grid.SelectedIndex = parentIndex;

            var container = grid.GetVisualContainer();

            container.ScrollRows.ScrollInView(grid.ResolveToRowIndex(parentIndex));
        }

        public static T CreateDelegate<T>(object obj, string method) where T : class 
        {
            return Delegate.CreateDelegate(typeof(T), obj, method) as T;
        }

        public static T ExecRefl<T>(Type type, object obj, string name, params object[] parameters)
        {
            return type.GetMethod(name).Invoke(obj, parameters).As<T>();
        }

        /*public static void ShowNewOrActivate<T>(ref T instance, bool isDialog = false) where T : Window, new()
        {
            if (instance == null)
            {
                instance = new T();

                if (isDialog)
                    instance.ShowDialog();
                else
                    instance.Show();

                return;
            }

            instance.Activate();
            instance.Focus();

            if (instance.WindowState == WindowState.Minimized)
                instance.WindowState = WindowState.Normal;
        }*/
    }
}