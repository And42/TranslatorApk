using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using AndroidLibs;
using AndroidTranslator.Classes.Files;
using AndroidTranslator.Interfaces.Files;
using AndroidTranslator.Interfaces.Strings;
using Microsoft.WindowsAPICodePack.Dialogs;
using MVVM_Tools.Code.Commands;
using MVVM_Tools.Code.Disposables;
using MVVM_Tools.Code.Providers;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.Utils;
using TranslatorApk.Resources.Localizations;
using TranslatorApk.Windows;
using UsefulFunctionsLib;

namespace TranslatorApk.Logic.ViewModels.Windows
{
    internal class ChangesDetectorWindowViewModel : ViewModelBase
    {
        public PropertyProvider<int> ProgressValue { get; }
        public PropertyProvider<int> ProgressMax { get; }

        public PropertyProvider<string> CreateFirstFolder { get; }
        public PropertyProvider<string> CreateSecondFolder { get; }
        public PropertyProvider<string> CreateResultFolder { get; }

        public PropertyProvider<string> TranslateFolder { get; }
        public PropertyProvider<string> TranslateDictionaryFolder { get; }

        public PropertyProvider<string> CreateMoreFirstFolder { get; }
        public PropertyProvider<string> CreateMoreSecondFolder { get; }
        public PropertyProvider<string> CreateMoreResultFolder { get; }

        public PropertyProvider<string> TranslateMoreFolder { get; }
        public PropertyProvider<string> TranslateMoreDictionaryFolder { get; }

        public string LogText => _logText.ToString();

        public ICommand CreateChooseFirstCommand => _createChooseFirstCommand;
        public ICommand CreateChooseSecondCommand => _createChooseSecondCommand;
        public ICommand CreateChooseResultFolderCommand => _createChooseResultFolderCommand;
        public ICommand CreateStartCommand => _createStartCommand;

        public ICommand TranslateChooseFolderCommand => _translateChooseFolderCommand;
        public ICommand TranslateDictionaryFolderCommand => _translateDictionaryFolderCommand;
        public ICommand TranslateStartCommand => _translateStartCommand;

        public ICommand CreateMoreChooseFirstCommand => _createMoreChooseFirstCommand;
        public ICommand CreateMoreChooseSecondCommand => _createMoreChooseSecondCommand;
        public ICommand CreateMoreChooseResultFolderCommand => _createMoreChooseResultFolderCommand;
        public ICommand CreateMoreStartCommand => _createMoreStartCommand;

        public ICommand TranslateMoreChooseCommand => _translateMoreChooseCommand;
        public ICommand TranslateMoreDictionaryFolderCommand => _translateMoreDictionaryFolderCommand;
        public ICommand TranslateMoreStartCommand => _translateMoreStartCommand;

        private readonly StringBuilder _logText = new StringBuilder();

        private readonly ActionCommand _createChooseFirstCommand;
        private readonly ActionCommand _createChooseSecondCommand;
        private readonly ActionCommand _createChooseResultFolderCommand;
        private readonly ActionCommand _createStartCommand;

        private readonly ActionCommand _translateChooseFolderCommand;
        private readonly ActionCommand _translateDictionaryFolderCommand;
        private readonly ActionCommand _translateStartCommand;

        private readonly ActionCommand _createMoreChooseFirstCommand;
        private readonly ActionCommand _createMoreChooseSecondCommand;
        private readonly ActionCommand _createMoreChooseResultFolderCommand;
        private readonly ActionCommand _createMoreStartCommand;

        private readonly ActionCommand _translateMoreChooseCommand;
        private readonly ActionCommand _translateMoreDictionaryFolderCommand;
        private readonly ActionCommand _translateMoreStartCommand;

        public ChangesDetectorWindowViewModel()
        {
            ProgressValue = CreateProviderWithNotify(nameof(ProgressValue), 0);
            ProgressMax = CreateProviderWithNotify(nameof(ProgressMax), 0);

            CreateFirstFolder = CreateProviderWithNotify<string>(nameof(CreateFirstFolder));
            CreateSecondFolder = CreateProviderWithNotify<string>(nameof(CreateSecondFolder));
            CreateResultFolder = CreateProviderWithNotify<string>(nameof(CreateResultFolder));

            TranslateFolder = CreateProviderWithNotify<string>(nameof(TranslateFolder));
            TranslateDictionaryFolder = CreateProviderWithNotify<string>(nameof(TranslateDictionaryFolder));

            CreateMoreFirstFolder = CreateProviderWithNotify<string>(nameof(CreateMoreFirstFolder));
            CreateMoreSecondFolder = CreateProviderWithNotify<string>(nameof(CreateMoreSecondFolder));
            CreateMoreResultFolder = CreateProviderWithNotify<string>(nameof(CreateMoreResultFolder));

            TranslateMoreFolder = CreateProviderWithNotify<string>(nameof(TranslateMoreFolder));
            TranslateMoreDictionaryFolder = CreateProviderWithNotify<string>(nameof(TranslateMoreDictionaryFolder));

            _createChooseFirstCommand = new ActionCommand(CreateChooseFirstCommand_Execute, NotLoading);
            _createChooseSecondCommand = new ActionCommand(CreateChooseSecondCommand_Execute, NotLoading);
            _createChooseResultFolderCommand = new ActionCommand(CreateChooseResultFolderCommand_Execute, NotLoading);
            _createStartCommand = new ActionCommand(CreateStartCommand_Execute, NotLoading);

            _translateChooseFolderCommand = new ActionCommand(TranslateChooseFolderCommand_Execute, NotLoading);
            _translateDictionaryFolderCommand = new ActionCommand(TranslateDictionaryFolderCommand_Execute, NotLoading);
            _translateStartCommand = new ActionCommand(TranslateStartCommand_Execute, NotLoading);

            _createMoreChooseFirstCommand = new ActionCommand(CreateMoreChooseFirstCommand_Execute, NotLoading);
            _createMoreChooseSecondCommand = new ActionCommand(CreateMoreChooseSecondCommand_Execute, NotLoading);
            _createMoreChooseResultFolderCommand = new ActionCommand(CreateMoreChooseResultFolderCommand_Execute, NotLoading);
            _createMoreStartCommand = new ActionCommand(CreateMoreStartCommand_Execute, NotLoading);

            _translateMoreChooseCommand = new ActionCommand(TranslateMoreChooseCommand_Execute, NotLoading);
            _translateMoreDictionaryFolderCommand = new ActionCommand(TranslateMoreDictionaryFolderCommand_Execute, NotLoading);
            _translateMoreStartCommand = new ActionCommand(TranslateMoreStartCommand_Execute, NotLoading);

            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(IsLoading):
                    RaiseCommandCanExecute();
                    break;
            }
        }

        public override void UnsubscribeFromEvents() { }

        private bool NotLoading() => !IsLoading;

        #region CreateClicks

        private void CreateChooseFirstCommand_Execute()
        {
            CreateFirstFolder.Value = ChooseFolderDialogShow();
        }

        private void CreateChooseSecondCommand_Execute()
        {
            CreateSecondFolder.Value = ChooseFolderDialogShow();
        }

        private void CreateChooseResultFolderCommand_Execute()
        {
            CreateResultFolder.Value = ChooseFolderDialogShow();
        }

        private async void CreateStartCommand_Execute()
        {
            if (string.IsNullOrEmpty(CreateFirstFolder.Value) ||
                string.IsNullOrEmpty(CreateSecondFolder.Value) ||
                string.IsNullOrEmpty(CreateResultFolder.Value) ||
                !Directory.Exists(CreateFirstFolder.Value) ||
                !Directory.Exists(CreateSecondFolder.Value)
            )
            {
                return;
            }

            ClearLog();

            using (BusyDisposable())
            {
                await CreateOneFolderDictionaryAsync(
                    CreateFirstFolder.Value,
                    CreateSecondFolder.Value,
                    CreateResultFolder.Value
                );
            }
        }

        #endregion

        #region TranslateClicks

        private void TranslateChooseFolderCommand_Execute()
        {
            TranslateFolder.Value = ChooseFolderDialogShow();
        }

        private void TranslateDictionaryFolderCommand_Execute()
        {
            TranslateDictionaryFolder.Value = ChooseFolderDialogShow();
        }

        private async void TranslateStartCommand_Execute()
        {
            if (string.IsNullOrEmpty(TranslateFolder.Value) ||
                string.IsNullOrEmpty(TranslateDictionaryFolder.Value) ||
                !Directory.Exists(TranslateFolder.Value) ||
                !Directory.Exists(TranslateDictionaryFolder.Value)
            )
            {
                return;
            }

            ClearLog();

            using (BusyDisposable())
            {
                await TranslateOneFolderAsync(
                    TranslateFolder.Value,
                    TranslateDictionaryFolder.Value
                );
            }
        }

        #endregion

        #region CreateMoreClicks

        private void CreateMoreChooseFirstCommand_Execute()
        {
            CreateMoreFirstFolder.Value = ChooseFolderDialogShow();
        }

        private void CreateMoreChooseSecondCommand_Execute()
        {
            CreateMoreSecondFolder.Value = ChooseFolderDialogShow();
        }

        private void CreateMoreChooseResultFolderCommand_Execute()
        {
            CreateMoreResultFolder.Value = ChooseFolderDialogShow();
        }

        private async void CreateMoreStartCommand_Execute()
        {
            if (string.IsNullOrEmpty(CreateMoreFirstFolder.Value) ||
                string.IsNullOrEmpty(CreateMoreSecondFolder.Value) ||
                string.IsNullOrEmpty(CreateMoreResultFolder.Value) ||
                !Directory.Exists(CreateMoreFirstFolder.Value) ||
                !Directory.Exists(CreateMoreSecondFolder.Value)
            )
            {
                return;
            }

            ClearLog();

            using (BusyDisposable())
            {
                await CreateMoreFolderDictionariesAsync(
                    CreateMoreFirstFolder.Value,
                    CreateMoreSecondFolder.Value,
                    CreateMoreResultFolder.Value
                );
            }
        }

        #endregion

        #region TranslateMoreClicks

        private void TranslateMoreChooseCommand_Execute()
        {
            TranslateMoreFolder.Value = ChooseFolderDialogShow();
        }

        private void TranslateMoreDictionaryFolderCommand_Execute()
        {
            TranslateMoreDictionaryFolder.Value = ChooseFolderDialogShow();
        }

        private async void TranslateMoreStartCommand_Execute()
        {
            if (string.IsNullOrEmpty(TranslateMoreFolder.Value) ||
                string.IsNullOrEmpty(TranslateMoreDictionaryFolder.Value) ||
                !Directory.Exists(TranslateMoreFolder.Value) ||
                !Directory.Exists(TranslateMoreDictionaryFolder.Value)
            )
            {
                return;
            }

            ClearLog();

            using (BusyDisposable())
            {
                await TranslateMoreFoldersAsync(
                    TranslateMoreFolder.Value,
                    TranslateMoreDictionaryFolder.Value
                );
            }
        }

        #endregion

        private CustomBoolDisposable BusyDisposable()
        {
            return new CustomBoolDisposable(val => IsLoading = val);
        }

        private void Log(string contents = "", bool writeNewLine = true)
        {
            _logText.Append(contents);

            if (writeNewLine)
                _logText.AppendLine();

            OnPropertyChanged(nameof(LogText));
        }

        private void ClearLog()
        {
            _logText.Clear();
            OnPropertyChanged(nameof(LogText));
        }

        private void RaiseCommandCanExecute()
        {
            _createChooseFirstCommand.RaiseCanExecuteChanged();
            _createChooseSecondCommand.RaiseCanExecuteChanged();
            _createChooseResultFolderCommand.RaiseCanExecuteChanged();
            _createStartCommand.RaiseCanExecuteChanged();

            _translateChooseFolderCommand.RaiseCanExecuteChanged();
            _translateDictionaryFolderCommand.RaiseCanExecuteChanged();
            _translateStartCommand.RaiseCanExecuteChanged();

            _createMoreChooseFirstCommand.RaiseCanExecuteChanged();
            _createMoreChooseSecondCommand.RaiseCanExecuteChanged();
            _createMoreChooseResultFolderCommand.RaiseCanExecuteChanged();
            _createMoreStartCommand.RaiseCanExecuteChanged();

            _translateMoreChooseCommand.RaiseCanExecuteChanged();
            _translateMoreDictionaryFolderCommand.RaiseCanExecuteChanged();
            _translateMoreStartCommand.RaiseCanExecuteChanged();
        }

        private void CreateOneFolderDictionary(string sourceFolder, string modifiedFolder, string resultFolder, bool log = true)
        {
            if (Directory.Exists(resultFolder))
                Directory.Delete(resultFolder, true);
            Directory.CreateDirectory(resultFolder);

            var dictFileWriter = new StreamWriter(Path.Combine(resultFolder, "Paths.dict"), false, Encoding.UTF8);
            var foldersFileWriter = new StreamWriter(Path.Combine(resultFolder, "Languages.dict"), false, Encoding.UTF8);

            if (new[] { sourceFolder, modifiedFolder, resultFolder }.Any(str => str.NE() || !Directory.Exists(str)))
            {
                return;
            }

            string dictsFolder = Path.Combine(resultFolder, "Dictionaries");
            Directory.CreateDirectory(dictsFolder);

            string languagesFolder = Path.Combine(resultFolder, "Languages");
            Directory.CreateDirectory(languagesFolder);

            if (log)
                Log("Searching for xml files in the first folder...");

            List<XmlFile> firstXmlFiles =
                Directory.EnumerateFiles(sourceFolder, "*.xml", SearchOption.AllDirectories)
                    .Select(file => new XmlFile(file))
                    .ToList();


            if (log)
                Log("Searching for xml files in the second folder...");

            List<XmlFile> secondXmlFiles =
                Directory.EnumerateFiles(modifiedFolder, "*.xml", SearchOption.AllDirectories)
                    .Select(file => new XmlFile(file))
                    .ToList();

            secondXmlFiles.Sort((ffile, sfile) => string.Compare(ffile.FileName, sfile.FileName, StringComparison.Ordinal));

            ProgressValue.Value = 0;
            ProgressMax.Value = firstXmlFiles.Count;

            int filenameIndex = 0;
            var errors = new List<string>();

            if (log)
                Log("Comparing...");

            var comparison =
                new ComparisonWrapper<XmlFile>((ffile, sfile) =>
                    string.Compare(
                        ffile.FileName.Substring(modifiedFolder.Length + 1),
                        sfile.FileName.Substring(sourceFolder.Length + 1),
                        StringComparison.Ordinal
                    )
                );

            foreach (XmlFile file in firstXmlFiles)
            {
                try
                {
                    ProgressValue.Value++;

                    if (file.Details.Count == 0)
                        continue;

                    int index = secondXmlFiles.BinarySearch(file, comparison);

                    if (index < 0)
                        continue;

                    XmlFile item = secondXmlFiles[index];

                    var dict = CreateDictionary(file, item, Path.Combine(dictsFolder, $"{filenameIndex}.xml"));

                    if (dict.Details.Count == 0)
                        continue;

                    dict.SaveChanges();
                    filenameIndex++;

                    dictFileWriter.WriteLine(file.FileName.Substring(sourceFolder.Length + 1));
                }
                catch (Exception ex)
                {
                    errors.Add(file.FileName + " - " + ex.Message);
                }
            }

            string resFolder = Path.Combine(sourceFolder, "res");

            var sourceFolders = Directory.EnumerateDirectories(resFolder, "values-*", SearchOption.TopDirectoryOnly).Select(Path.GetFileName).ToList();
            var modifiedFolders = Directory.EnumerateDirectories(resFolder, "values-*", SearchOption.TopDirectoryOnly).ToList();

            if (log)
                Log("Comparing languages...");

            ProgressValue.Value = 0;
            ProgressMax.Value = modifiedFolders.Count;

            foreach (string folder in modifiedFolders)
            {
                ProgressValue.Value++;

                string part = Path.GetFileName(folder) ?? string.Empty;

                if (sourceFolders.Any(f => f == part))
                    continue;

                foldersFileWriter.WriteLine(part);

                IOUtils.CopyFilesRecursively(folder, Path.Combine(languagesFolder, part));
            }

            if (log)
            {
                Log();
                Log("Number of created dictionaries: " + filenameIndex);

                if (errors.Count > 0)
                {
                    Log("Number of errors: " + errors.Count);
                    foreach (var error in errors)
                        Log("  -- " + error);
                }
            }

            dictFileWriter.Close();
            foldersFileWriter.Close();
        }

        private Task CreateOneFolderDictionaryAsync(string sourceFolder, string modifiedFolder, string resultFolder, bool log = true)
        {
            return Task.Factory.StartNew(() => CreateOneFolderDictionary(sourceFolder, modifiedFolder, resultFolder, log));
        }

        private void TranslateOneFolder(string fileFolder, string dictionaryFolder, bool log = true)
        {
            if (log) Log("Opening dictionaries...");

            string[] paths = File.ReadAllLines(Path.Combine(dictionaryFolder, "Paths.dict"));

            List<DictionaryFile> dicts =
                Directory.EnumerateFiles(Path.Combine(dictionaryFolder, "Dictionaries"))
                    .Select(file => new DictionaryFile(file))
                    .ToList();

            ProgressValue.Value = 0;
            ProgressMax.Value = paths.Length;

            if (log)
                Log("Translating files:");

            for (int i = 0; i < paths.Length; i++)
            {
                ProgressValue.Value++;
                string xmlfilePath = Path.Combine(fileFolder, paths[i]);

                if (log)
                    Log("  -- " + xmlfilePath, false);

                if (!File.Exists(xmlfilePath))
                {
                    if (log)
                        Log(" - skipped");

                    continue;
                }

                DictionaryFile dict = dicts.Find(d => Path.GetFileNameWithoutExtension(d.FileName) == i.ToString());

                if (dict == null)
                {
                    if (log)
                        Log(" - skipped");

                    continue;
                }

                var xmlfile = new XmlFile(xmlfilePath);

                xmlfile.TranslateWithDictionary(dict, true);

                if (log)
                    Log(" - translated");
            }

            if (!File.Exists(Path.Combine(dictionaryFolder, "Languages.dict")))
                return;

            if (log)
                Log("Adding languages:");

            string[] languages = File.ReadAllLines(Path.Combine(dictionaryFolder, "Languages.dict"));

            ProgressValue.Value = 0;
            ProgressMax.Value = languages.Length;

            foreach (var language in languages)
            {
                ProgressValue.Value++;

                if (log)
                    Log("  -- " + language);

                IOUtils.CopyFilesRecursively(
                    Path.Combine(dictionaryFolder, "Languages", language),
                    Path.Combine(fileFolder, "res", language)
                );
            }
        }

        private Task TranslateOneFolderAsync(string fileFolder, string dictionaryFolder, bool log = true)
        {
            return Task.Factory.StartNew(() => TranslateOneFolder(fileFolder, dictionaryFolder, log));
        }

        private void CreateMoreFolderDictionaries(string sourceFolder, string modifiedFolder, string resultFolder)
        {
            if (Directory.Exists(resultFolder))
                Directory.Delete(resultFolder, true);

            Directory.CreateDirectory(resultFolder);

            List<Apktools> sourceApktools =
                Directory.EnumerateFiles(sourceFolder, "*.apk")
                    .Select(file => new Apktools(file, GlobalVariables.PathToResources, GlobalVariables.CurrentApktoolPath))
                    .ToList();

            List<Apktools> modifiedApktools =
                Directory.EnumerateFiles(modifiedFolder, "*.apk")
                    .Select(file => new Apktools(file, GlobalVariables.PathToResources, GlobalVariables.CurrentApktoolPath))
                    .ToList();

            Log("Decompiling files:");
            ProgressValue.Value = 0;
            ProgressMax.Value = sourceApktools.Count + modifiedApktools.Count;
            for (int i = 0; i < sourceApktools.Count; i++)
            {
                ProgressValue.Value++;
                Log("  -- " + sourceApktools[i].FileName);
                if (!sourceApktools[i].Decompile(true, false))
                {
                    Log("Error while decompiling!");
                    sourceApktools.RemoveAt(i);
                    i--;
                }
            }
            for (int i = 0; i < modifiedApktools.Count; i++)
            {
                ProgressValue.Value++;
                Log("  -- " + modifiedApktools[i].FileName);
                if (!modifiedApktools[i].Decompile(true, false))
                {
                    Log("Error while decompiling!");
                    modifiedApktools.RemoveAt(i);
                    i--;
                }
            }
            Log();
            Log("Creating dictionaries:");
            int progressState = 1;
            int progressMax = sourceApktools.Count;
            foreach (var apktool in sourceApktools)
            {
                try
                {
                    Log($"  -- ({progressState++} из {progressMax}) {apktool.FileName}", false);
                    Apktools modifiedApktool = modifiedApktools.Find(a => a.Manifest.Package == apktool.Manifest.Package);
                    if (modifiedApktool == null)
                    {
                        Log(" - skipped");
                        continue;
                    }

                    CreateOneFolderDictionary(apktool.FolderOfProject, modifiedApktool.FolderOfProject, Path.Combine(resultFolder, apktool.Manifest.Package), false);

                    Log(" - created");
                }
                catch (Exception ex)
                {
                    // ReSharper disable once LocalizableElement
                    MessageBox.Show($"Message: {ex.Message}\nStackTrace: {ex.StackTrace}");
                    Log(" - error");
                }
            }
        }

        private Task CreateMoreFolderDictionariesAsync(string sourceFolder, string modifiedFolder, string resultFolder)
        {
            return Task.Factory.StartNew(() => CreateMoreFolderDictionaries(sourceFolder, modifiedFolder, resultFolder));
        }

        private void TranslateMoreFolders(string filesFolder, string dictionariesFolder)
        {
            var fileApktools =
                Directory.EnumerateFiles(filesFolder, "*.apk")
                    .Select(file => new Apktools(file, GlobalVariables.PathToResources))
                    .ToList();

            Log("Decompiling files:");
            ProgressValue.Value = 0;
            ProgressMax.Value = fileApktools.Count;

            for (int i = 0; i < fileApktools.Count; i++)
            {
                ProgressValue.Value++;
                Log("  -- " + fileApktools[i].FileName);

                if (!fileApktools[i].Decompile(true, false))
                {
                    Log("Error while decompiling!");
                    fileApktools.RemoveAt(i--);
                }
            }

            Log();
            Log("Translating files...");

            int progressState = 1;
            int progressMax = fileApktools.Count;

            foreach (var apktool in fileApktools)
            {
                try
                {
                    Log($"  -- ({progressState++} из {progressMax}) {apktool.FileName}", false);

                    string dictPath = Path.Combine(dictionariesFolder, apktool.Manifest.Package);

                    if (!Directory.Exists(dictPath))
                    {
                        Log(" - skipped");
                        continue;
                    }

                    TranslateOneFolder(apktool.FolderOfProject, dictPath, false);

                    Log(" - translated");
                }
                catch (Exception ex)
                {
                    // ReSharper disable once LocalizableElement
                    MessageBox.Show($"Message: {ex.Message}\nStackTrace: {ex.StackTrace}");
                    Log(" - error");
                }
            }

            Log();
            Log("Compiling:");

            ProgressValue.Value = 0;
            ProgressMax.Value = fileApktools.Count;

            string resultFolder = Path.Combine(filesFolder, "Result");
            string resultSignedFolder = Path.Combine(filesFolder, "ResultSigned");

            foreach (var folder in new[] { resultFolder, resultSignedFolder })
            {
                if (Directory.Exists(folder))
                    Directory.Delete(folder, true);

                Directory.CreateDirectory(folder);
            }

            for (int i = 0; i < fileApktools.Count; i++)
            {
                try
                {
                    ProgressValue.Value++;
                    Log("  -- " + fileApktools[i].FileName, false);

                    List<Error> errors;
                    if (fileApktools[i].Compile(out errors))
                    {
                        Log(" - compiled", false);
                        File.Copy(fileApktools[i].NewApk, Path.Combine(resultFolder, Path.GetFileName(fileApktools[i].NewApk) ?? string.Empty));
                        Log(" - copied");
                    }
                    else
                    {
                        foreach (var error in errors)
                            Log($" - error: \n    File: {error.File}\n    Line: {error.Line}\n    Message: {error.Message}");

                        fileApktools.RemoveAt(i--);
                    }
                }
                catch (Exception ex)
                {
                    // ReSharper disable once LocalizableElement
                    MessageBox.Show($"Message: {ex.Message}\nStackTrace: {ex.StackTrace}");
                    Log(" - error");
                }
            }

            Log();
            Log("Signing:");

            ProgressValue.Value = 0;
            ProgressMax.Value = fileApktools.Count;

            for (int i = 0; i < fileApktools.Count; i++)
            {
                try
                {
                    ProgressValue.Value++;
                    Log("  -- " + fileApktools[i].FileName, false);

                    if (fileApktools[i].Sign())
                    {
                        Log(" - signed", false);
                        File.Copy(fileApktools[i].SignedApk, Path.Combine(resultSignedFolder, Path.GetFileName(fileApktools[i].SignedApk) ?? string.Empty));
                        Log(" - copied");
                    }
                    else
                    {
                        Log(" - error");
                        fileApktools.RemoveAt(i--);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($@"Message: {ex.Message}\nStackTrace: {ex.StackTrace}");
                    Log(" - error");
                }
            }

            Log();
            Log("Finished");

            if (
                MessBox.ShowDial(
                    "Открыть папку с готовыми файлами?", null,
                    MessBox.MessageButtons.Yes, MessBox.MessageButtons.No
                ) == MessBox.MessageButtons.Yes
            )
            {
                Process.Start(Directory.Exists(resultSignedFolder) ? resultSignedFolder : filesFolder);
            }
        }

        private Task TranslateMoreFoldersAsync(string filesFolder, string dictionariesFolder)
        {
            return Task.Factory.StartNew(() => TranslateMoreFolders(filesFolder, dictionariesFolder));
        }

        private static IDictionaryFile CreateDictionary(IXmlFile first, IXmlFile second, string resultFilename)
        {
            var result = new DictionaryFile(resultFilename);
            var strings = second.SpecDetails.ToList();

            if (Path.GetFileName(first.FileName) == "strings.xml")
            {
                foreach (var str in first.Details)
                {
                    IOneString item = second.Details.FirstOrDefault(it => it.Name == str.Name);

                    if (item != null && item.OldText != str.OldText)
                        result.Add(str.OldText, item.OldText);
                }
            }
            else
            {
                foreach (var str in first.SpecDetails)
                {
                    IOneString item = strings.FirstOrDefault(sstr => str.EqualsNavigations(sstr));

                    if (item != null && str.OldText != item.OldText)
                        result.Add(str.OldText, item.OldText);
                }
            }

            return result;
        }

        private static string ChooseFolderDialogShow()
        {
            var fd = new CommonOpenFileDialog
            {
                Title = StringResources.ChooseFolder,
                Multiselect = false,
                IsFolderPicker = true
            };

            return fd.ShowDialog() != CommonFileDialogResult.Ok ? null : fd.FileName;
        }
    }
}
