using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;
using AndroidLibs;
using AndroidTranslator;
using Microsoft.WindowsAPICodePack.Dialogs;
using TranslatorApk.Annotations;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.OrganisationItems;
using UsefulFunctionsLib;

using MessageBox = System.Windows.Forms.MessageBox;
using Strings = TranslatorApk.Resources.Localizations.Resources;

namespace TranslatorApk.Windows
{
    /// <summary>
    /// Логика взаимодействия для ChangesDetectorWindow.xaml
    /// </summary>
    public partial class ChangesDetectorWindow : INotifyPropertyChanged
    {
        public ChangesDetectorWindow()
        {
            InitializeComponent();
            TaskbarItemInfo = new TaskbarItemInfo { ProgressState = TaskbarItemProgressState.Normal };
        }

        #region All

        public int ProgressValue
        {
            get => _progressValue;
            set
            {
                _progressValue = value;
                Dispatcher.InvokeAction(() => TaskbarItemInfo.ProgressValue = value / (double)ProgressMax);
                OnPropertyChanged(nameof(ProgressValue));
            }
        }
        private int _progressValue;

        public int ProgressMax
        {
            get => _progressMax;
            set
            {
                _progressMax = value;
                OnPropertyChanged(nameof(ProgressMax));
            }
        }
        private int _progressMax;

        public bool ButtonsEnabled
        {
            get => _buttonsEnabled;
            set
            {
                _buttonsEnabled = value;
                OnPropertyChanged(nameof(ButtonsEnabled));
            }
        }
        private bool _buttonsEnabled = true;

        public string Log
        {
            get => _log.ToString();
            set
            {
                _log.Clear();
                _log.Append(value);
                OnPropertyChanged(nameof(Log));
            }
        }
        private readonly StringBuilder _log = new StringBuilder();

        #endregion

        #region Create

        public string CreateFirstFolder
        {
            get => _createFirstFolder;
            set
            {
                _createFirstFolder = value;
                OnPropertyChanged(nameof(CreateFirstFolder));
            }
        }
        private string _createFirstFolder;

        public string CreateSecondFolder
        {
            get => _createSecondFolder;
            set
            {
                _createSecondFolder = value;
                OnPropertyChanged(nameof(CreateSecondFolder));
            }
        }
        private string _createSecondFolder;

        public string CreateResultFolder
        {
            get => _createResultFolder;
            set
            {
                _createResultFolder = value;
                OnPropertyChanged(nameof(CreateResultFolder));
            }
        }
        private string _createResultFolder;

        #endregion

        #region Translate

        public string TranslateFolder
        {
            get => _translateFolder;
            set
            {
                _translateFolder = value;
                OnPropertyChanged(nameof(TranslateFolder));
            }
        }
        private string _translateFolder ;

        public string TranslateDictionaryFolder
        {
            get => _translateDictionaryFolder;
            set
            {
                _translateDictionaryFolder = value;
                OnPropertyChanged(nameof(TranslateDictionaryFolder));
            }
        }
        private string _translateDictionaryFolder;

        #endregion

        #region CreateMore

        public string CreateMoreFirstFolder
        {
            get => _createMoreFirstFolder;
            set
            {
                _createMoreFirstFolder = value;
                OnPropertyChanged(nameof(CreateMoreFirstFolder));
            }
        }
        private string _createMoreFirstFolder;

        public string CreateMoreSecondFolder
        {
            get => _createMoreSecondFolder;
            set
            {
                _createMoreSecondFolder = value;
                OnPropertyChanged(nameof(CreateMoreSecondFolder));
            }
        }
        private string _createMoreSecondFolder;

        public string CreateMoreResultFolder
        {
            get => _createMoreResultFolder;
            set
            {
                _createMoreResultFolder = value;
                OnPropertyChanged(nameof(CreateMoreResultFolder));
            }
        }
        private string _createMoreResultFolder;

        #endregion

        #region TranslateMore

        public string TranslateMoreFolder
        {
            get => _translateMoreFolder;
            set
            {
                _translateMoreFolder = value;
                OnPropertyChanged(nameof(TranslateMoreFolder));
            }
        }
        private string _translateMoreFolder;

        public string TranslateMoreDictionaryFolder
        {
            get => _translateMoreDictionaryFolder;
            set
            {
                _translateMoreDictionaryFolder = value;
                OnPropertyChanged(nameof(TranslateMoreDictionaryFolder));
            }
        }
        private string _translateMoreDictionaryFolder;

        #endregion

        #region CreateClicks

        private void CreateChooseFirstClick(object sender, RoutedEventArgs e)
        {
            CreateFirstFolder = ChooseFolderDialogShow();
        }

        private void CreateChooseSecondClick(object sender, RoutedEventArgs e)
        {
            CreateSecondFolder = ChooseFolderDialogShow();
        }

        private void CreateChooseResultFolderClick(object sender, RoutedEventArgs e)
        {
            CreateResultFolder = ChooseFolderDialogShow();
        }

        private void CreateStartClick(object sender, RoutedEventArgs e)
        {
            if (CreateFirstFolder.NE() || CreateSecondFolder.NE() || CreateResultFolder.NE() ||
                !Directory.Exists(CreateFirstFolder) || !Directory.Exists(CreateSecondFolder))
                return;

            Log = "";
            ButtonsEnabled = false;
            var proc = new Task(() => CreateOneFolderDictionary(CreateFirstFolder, CreateSecondFolder, CreateResultFolder));
            proc.ContinueWith(task => ButtonsEnabled = true);
            proc.Start();
        }

        #endregion

        #region TranslateClicks

        private void TranslateChooseFolder(object sender, RoutedEventArgs e)
        {
            TranslateFolder = ChooseFolderDialogShow();
        }

        private void TranslateDictionaryFolderClick(object sender, RoutedEventArgs e)
        {
            TranslateDictionaryFolder = ChooseFolderDialogShow();
        }

        private void TranslateStartClick(object sender, RoutedEventArgs e)
        {
            if (TranslateFolder.NE() || TranslateDictionaryFolder.NE() ||
                !Directory.Exists(TranslateFolder) || !Directory.Exists(TranslateDictionaryFolder))
                return;

            Log = "";
            ButtonsEnabled = false;
            var proc = new Task(() => TranslateOneFolder(TranslateFolder, TranslateDictionaryFolder));
            proc.ContinueWith(task => ButtonsEnabled = true);
            proc.Start();
        }

        #endregion

        #region CreateMoreClicks

        private void CreateMoreChooseFirstClick(object sender, RoutedEventArgs e)
        {
            CreateMoreFirstFolder = ChooseFolderDialogShow();
        }

        private void CreateMoreChooseSecondClick(object sender, RoutedEventArgs e)
        {
            CreateMoreSecondFolder = ChooseFolderDialogShow();
        }

        private void CreateMoreChooseResultFolderClick(object sender, RoutedEventArgs e)
        {
            CreateMoreResultFolder = ChooseFolderDialogShow();
        }

        private void CreateMoreStartClick(object sender, RoutedEventArgs e)
        {
            if (CreateMoreFirstFolder.NE() || CreateMoreSecondFolder.NE() || CreateMoreResultFolder.NE() ||
                !Directory.Exists(CreateMoreFirstFolder) || !Directory.Exists(CreateMoreSecondFolder))
                return;

            Log = "";
            ButtonsEnabled = false;
            var proc = new Task(() => CreateMoreFolderDictionaries(CreateMoreFirstFolder, CreateMoreSecondFolder, CreateMoreResultFolder));
            proc.ContinueWith(task => ButtonsEnabled = true);
            proc.Start();
        }

        #endregion

        #region TranslateMoreClicks

        private void TranslateMoreChooseFolder(object sender, RoutedEventArgs e)
        {
            TranslateMoreFolder = ChooseFolderDialogShow();
        }

        private void TranslateMoreDictionaryFolderClick(object sender, RoutedEventArgs e)
        {
            TranslateMoreDictionaryFolder = ChooseFolderDialogShow();
        }

        private void TranslateMoreStartClick(object sender, RoutedEventArgs e)
        {
            if (TranslateMoreFolder.NE() || TranslateMoreDictionaryFolder.NE() ||
                !Directory.Exists(TranslateMoreFolder) || !Directory.Exists(TranslateMoreDictionaryFolder))
                return;

            Log = "";
            ButtonsEnabled = false;
            var proc = new Task(() => TranslateMoreFolders(TranslateMoreFolder, TranslateMoreDictionaryFolder));
            proc.ContinueWith(task => ButtonsEnabled = true);
            proc.Start();
        }

        #endregion

        #region Functions

        private void CreateOneFolderDictionary(string sourceFolder, string modifiedFolder, string resultFolder, bool log = true)
        {
            if (Directory.Exists(resultFolder)) Directory.Delete(resultFolder, true);
            Directory.CreateDirectory(resultFolder);

            StreamWriter dictFileWriter = new StreamWriter(resultFolder + "\\Paths.dict", false, Encoding.UTF8);
            StreamWriter foldersFileWriter = new StreamWriter(resultFolder + "\\Languages.dict", false, Encoding.UTF8);

            if (
                new[] { sourceFolder, modifiedFolder, resultFolder }.Any(
                    str => str.NE() || !Directory.Exists(str))) return;

            string dictsFolder = resultFolder + "\\Dictionaries";
            Directory.CreateDirectory(dictsFolder);

            string languagesFolder = resultFolder + "\\Languages";
            Directory.CreateDirectory(languagesFolder);

            if (log) LogWrite("Searching for xml files in the first folder...");
            List<XmlFile> firstXmlFiles =
                new List<XmlFile>(
                    Directory.GetFiles(sourceFolder, "*.xml", SearchOption.AllDirectories)
                        .Select(file => new XmlFile(file)));


            if (log) LogWrite("Searching for xml files in the second folder...");
            List<XmlFile> secondXmlFiles =
                new List<XmlFile>(
                    Directory.GetFiles(modifiedFolder, "*.xml", SearchOption.AllDirectories)
                        .Select(file => new XmlFile(file)));

            secondXmlFiles.Sort((ffile, sfile) => string.Compare(ffile.FileName, sfile.FileName, StringComparison.Ordinal));

            ProgressValue = 0;
            ProgressMax = firstXmlFiles.Count;
            int filenameIndex = 0;
            List<string> errors = new List<string>();

            if (log) LogWrite("Comparing...");
            foreach (XmlFile file in firstXmlFiles)
            {
                try
                {
                    ProgressValue++;
                    if (file.Details.Count == 0) continue;
                    int index = secondXmlFiles.BinarySearch(file,
                        new ComparisonWrapper<XmlFile>((ffile, sfile) => string.Compare(
                            ffile.FileName.Substring(modifiedFolder.Length + 1),
                            sfile.FileName.Substring(sourceFolder.Length + 1),
                            StringComparison.Ordinal)));
                    if (index < 0) continue;
                    XmlFile item = secondXmlFiles[index];
                    var dict = CreateDictionary(file, item, $"{dictsFolder}\\{filenameIndex}.xml");
                    if (dict.Details.Count == 0) continue;
                    dict.SaveChanges();
                    filenameIndex++;
                    dictFileWriter.WriteLine(file.FileName.Substring(sourceFolder.Length + 1));
                }
                catch (Exception ex)
                {
                    errors.Add(file.FileName + " - " + ex.Message);
                }
            }

            var sourceFolders = Directory.GetDirectories(sourceFolder + "\\res", "values-*", SearchOption.TopDirectoryOnly).Select(Path.GetFileName).ToArray();
            var modifiedFolders = Directory.GetDirectories(modifiedFolder + "\\res", "values-*", SearchOption.TopDirectoryOnly);

            if (log) LogWrite("Comparing languages...");

            ProgressValue = 0;
            ProgressMax = modifiedFolders.Length;

            foreach (string folder in modifiedFolders)
            {
                ProgressValue++;

                string part = Path.GetFileName(folder);

                if (sourceFolders.Any(f => f == part)) continue;
                
                foldersFileWriter.WriteLine(part);

                Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory(folder, languagesFolder + "\\" + part, true);
            }

            if (log)
            {
                LogWrite();
                LogWrite("Number of created dictionaries: " + filenameIndex);

                if (errors.Count > 0)
                {
                    LogWrite("Number of errors: " + errors.Count);
                    foreach (var error in errors) LogWrite("  -- " + error);
                }
            }

            dictFileWriter.Close();
            foldersFileWriter.Close();
        }

        private void TranslateOneFolder(string fileFolder, string dictionaryFolder, bool log = true)
        {
            if (log) LogWrite("Opening dictionaries...");

            string[] paths = File.ReadAllLines(dictionaryFolder + "\\Paths.dict");
            List<DictionaryFile> dicts =
                new List<DictionaryFile>(
                    Directory.GetFiles(dictionaryFolder + "\\Dictionaries")
                        .Select(file => new DictionaryFile(file)));

            ProgressValue = 0;
            ProgressMax = paths.Length;
            if (log) LogWrite("Translating files:");
            for (int i = 0; i < paths.Length; i++)
            {
                ProgressValue++;
                string xmlfilePath = fileFolder + "\\" + paths[i];
                if (log) LogWrite("  -- " + xmlfilePath, false);
                if (!File.Exists(xmlfilePath))
                {
                    if (log) LogWrite(" - skipped");
                    continue;
                }
                DictionaryFile dict = dicts.Find(d => Path.GetFileNameWithoutExtension(d.FileName) == i.ToString());
                if (dict == null)
                {
                    if (log) LogWrite(" - skipped");
                    continue;
                }
                XmlFile xmlfile = new XmlFile(xmlfilePath);
                xmlfile.TranslateWithDictionary(dict, true);
                if (log) LogWrite(" - translated");
            }

            if (!File.Exists(dictionaryFolder + "\\Languages.dict")) return;

            if (log) LogWrite("Adding languages:");

            string[] languages = File.ReadAllLines(dictionaryFolder + "\\Languages.dict");

            ProgressValue = 0;
            ProgressMax = languages.Length;

            foreach (var language in languages)
            {
                ProgressValue++;

                if (log) LogWrite("  -- " + language);

                Microsoft.VisualBasic.FileIO.FileSystem.CopyDirectory($@"{dictionaryFolder}\Languages\{language}", $@"{fileFolder}\res\{language}", true);
            }
        }

        private void CreateMoreFolderDictionaries(string sourceFolder, string modifiedFolder, string resultFolder)
        {
            if (Directory.Exists(resultFolder)) Directory.Delete(resultFolder, true);
            Directory.CreateDirectory(resultFolder);

            List<Apktools> sourceApktools = new List<Apktools>(Directory.GetFiles(sourceFolder, "*.apk").Select(file => new Apktools(file, GlobalVariables.PathToResources, GlobalVariables.CurrentApktoolPath)));
            List<Apktools> modifiedApktools = new List<Apktools>(Directory.GetFiles(modifiedFolder, "*.apk").Select(file => new Apktools(file, GlobalVariables.PathToResources, GlobalVariables.CurrentApktoolPath)));
            LogWrite("Decompiling files:");
            ProgressValue = 0;
            ProgressMax = sourceApktools.Count + modifiedApktools.Count;
            for (int i = 0; i < sourceApktools.Count; i++)
            {
                ProgressValue++;
                LogWrite("  -- " + sourceApktools[i].FileName);
                if (!sourceApktools[i].Decompile(true, false))
                {
                    LogWrite("Error while decompiling!");
                    sourceApktools.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < modifiedApktools.Count; i++)
            {
                ProgressValue++;
                LogWrite("  -- " + modifiedApktools[i].FileName);
                if (!modifiedApktools[i].Decompile(true, false))
                {
                    LogWrite("Error while decompiling!");
                    modifiedApktools.RemoveAt(i);
                    i--;
                }
            }
            LogWrite();
            LogWrite("Creating dictionaries:");
            int progressState = 1;
            int progressMax = sourceApktools.Count;
            foreach (var apktool in sourceApktools)
            {
                try
                {
                    LogWrite($"  -- ({progressState++} из {progressMax}) {apktool.FileName}", false);
                    Apktools modifiedApktool = modifiedApktools.Find(a => a.Manifest.Package == apktool.Manifest.Package);
                    if (modifiedApktool == null)
                    {
                        LogWrite(" - skipped");
                        continue;
                    }

                    CreateOneFolderDictionary(apktool.FolderOfProject, modifiedApktool.FolderOfProject, resultFolder + "\\" + apktool.Manifest.Package, false);

                    LogWrite(" - created");
                }
                catch (Exception ex)
                {
                    // ReSharper disable once LocalizableElement
                    MessageBox.Show($"Message: {ex.Message}\nStackTrace: {ex.StackTrace}");
                    LogWrite(" - error");
                }
            }
        }

        private void TranslateMoreFolders(string filesFolder, string dictionariesFolder)
        {
            List<Apktools> fileApktools = new List<Apktools>(Directory.GetFiles(filesFolder, "*.apk").Select(file => new Apktools(file, GlobalVariables.PathToResources)));
            LogWrite("Decompiling files:");
            ProgressValue = 0;
            ProgressMax = fileApktools.Count;
            for (int i = 0; i < fileApktools.Count; i++)
            {
                ProgressValue++;
                LogWrite("  -- " + fileApktools[i].FileName);
                if (!fileApktools[i].Decompile(true, false))
                {
                    LogWrite("Error while decompiling!");
                    fileApktools.RemoveAt(i--);
                }
            }

            LogWrite();
            LogWrite("Translating files...");

            int progressState = 1;
            int progressMax = fileApktools.Count;

            foreach (var apktool in fileApktools)
            {
                try
                {
                    LogWrite($"  -- ({progressState++} из {progressMax}) {apktool.FileName}", false);
                    string dictPath = dictionariesFolder + "\\" + apktool.Manifest.Package;
                    if (!Directory.Exists(dictPath))
                    {
                        LogWrite(" - skipped");
                        continue;
                    }

                    TranslateOneFolder(apktool.FolderOfProject, dictPath, false);

                    LogWrite(" - translated");
                }
                catch (Exception ex)
                {
                    // ReSharper disable once LocalizableElement
                    MessageBox.Show($"Message: {ex.Message}\nStackTrace: {ex.StackTrace}");
                    LogWrite(" - error");
                }
            }

            LogWrite();
            LogWrite("Compiling:");

            ProgressValue = 0;
            ProgressMax = fileApktools.Count;

            string resultFolder = filesFolder + "\\Result";
            string resultSignedFolder = filesFolder + "\\ResultSigned";

            foreach (var folder in new[] {resultFolder, resultSignedFolder})
            {
                if (Directory.Exists(folder)) Directory.Delete(folder, true);
                Directory.CreateDirectory(folder);
            }

            for (int i = 0; i < fileApktools.Count; i++)
            {
                try
                {
                    ProgressValue++;
                    LogWrite("  -- " + fileApktools[i].FileName, false);
                    List<Error> errors;
                    if (fileApktools[i].Compile(out errors))
                    {
                        LogWrite(" - compiled", false);
                        File.Copy(fileApktools[i].NewApk, resultFolder + "\\" + Path.GetFileName(fileApktools[i].NewApk));
                        LogWrite(" - copied");
                    }
                    else
                    {
                        foreach (var error in errors)
                            LogWrite($" - error: \n    File: {error.File}\n    Line: {error.Line}\n    Message: {error.Message}");
                        fileApktools.RemoveAt(i--);
                    }
                }
                catch (Exception ex)
                {
                    // ReSharper disable once LocalizableElement
                    MessageBox.Show($"Message: {ex.Message}\nStackTrace: {ex.StackTrace}");
                    LogWrite(" - error");
                }
            }

            LogWrite();
            LogWrite("Signing:");

            ProgressValue = 0;
            ProgressMax = fileApktools.Count;

            for (int i = 0; i < fileApktools.Count; i++)
            {
                try
                {
                    ProgressValue++;
                    LogWrite("  -- " + fileApktools[i].FileName, false);
                    if (fileApktools[i].Sign())
                    {
                        LogWrite(" - signed", false);
                        File.Copy(fileApktools[i].SignedApk, resultSignedFolder + "\\" + Path.GetFileName(fileApktools[i].SignedApk));
                        LogWrite(" - copied");
                    }
                    else
                    {
                        LogWrite(" - error");
                        fileApktools.RemoveAt(i--);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(@"Message: " + ex.Message + @"\nStackTrace: " + ex.StackTrace);
                    LogWrite(" - error");
                }
            }

            LogWrite();
            LogWrite("Finished");
            if (
                MessBox.ShowDial("Открыть папку с готовыми файлами?", null, MessBox.MessageButtons.Yes,
                    MessBox.MessageButtons.No) == MessBox.MessageButtons.Yes)
                Process.Start(Directory.Exists(resultSignedFolder) ? resultSignedFolder : filesFolder);
        }

        private DictionaryFile CreateDictionary(XmlFile first, XmlFile second, string resultFilename)
        {
            DictionaryFile result = new DictionaryFile(resultFilename);
            List<OneString> strings = new List<OneString>(second.Details);

            if (Path.GetFileName(first.FileName) == "strings.xml")
            {
                foreach (OneString str in first.Details)
                {
                    OneString item = second.Details.FirstOrDefault(it => it.Name == str.Name);
                    if (item != null && item.OldText != str.OldText) result.Add(str.OldText, item.OldText);
                }
            }
            else
            {
                foreach (OneString str in first.Details)
                {
                    OneString item = strings.FirstOrDefault(sstr => str.As<IXmlString>().EqualsNavigations(sstr));
                    if (item != null && str.OldText != item.OldText) result.Add(str.OldText, item.OldText);
                }
            }
            return result;
        }

        private string ChooseFolderDialogShow()
        {
            CommonOpenFileDialog fd = new CommonOpenFileDialog
            {
                Title = Strings.ChooseFolder,
                Multiselect = false,
                IsFolderPicker = true
            };

            string result = fd.ShowDialog() != CommonFileDialogResult.Ok ? string.Empty : fd.FileName;

            Activate();

            return result;
        }

        private void LogWrite(string text = "", bool writeNewLine = true)
        {
            _log.Append(text);
            if (writeNewLine) _log.Append(Environment.NewLine);
            OnPropertyChanged(nameof(Log));
            Dispatcher.InvokeAction(() => LogBox.ScrollToEnd());
        }

        #endregion

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
