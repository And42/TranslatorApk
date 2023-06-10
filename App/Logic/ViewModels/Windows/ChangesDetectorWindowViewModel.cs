﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AndroidHelper.Logic;
using AndroidTranslator.Classes.Files;
using AndroidTranslator.Interfaces.Files;
using AndroidTranslator.Interfaces.Strings;
using Microsoft.WindowsAPICodePack.Dialogs;
using MVVM_Tools.Code.Commands;
using MVVM_Tools.Code.Providers;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.Utils;
using TranslatorApk.Resources.Localizations;
using TranslatorApk.Windows;

namespace TranslatorApk.Logic.ViewModels.Windows
{
    internal class ChangesDetectorWindowViewModel : ViewModelBase
    {
        public FieldProperty<int> ProgressValue { get; }
        public FieldProperty<int> ProgressMax { get; }

        public FieldProperty<string> CreateFirstFolder { get; }
        public FieldProperty<string> CreateSecondFolder { get; }
        public FieldProperty<string> CreateResultFolder { get; }

        public FieldProperty<string> TranslateFolder { get; }
        public FieldProperty<string> TranslateDictionaryFolder { get; }

        public FieldProperty<string> CreateMoreFirstFolder { get; }
        public FieldProperty<string> CreateMoreSecondFolder { get; }
        public FieldProperty<string> CreateMoreResultFolder { get; }

        public FieldProperty<string> TranslateMoreFolder { get; }
        public FieldProperty<string> TranslateMoreDictionaryFolder { get; }

        public string LogText => _logText.ToString();

        private readonly StringBuilder _logText = new StringBuilder();

        public IActionCommand CreateChooseFirstCommand { get; }
        public IActionCommand CreateChooseSecondCommand { get; }
        public IActionCommand CreateChooseResultFolderCommand { get; }
        public IActionCommand CreateStartCommand { get; }

        public IActionCommand TranslateChooseFolderCommand { get; }
        public IActionCommand TranslateDictionaryFolderCommand { get; }
        public IActionCommand TranslateStartCommand { get; }

        public IActionCommand CreateMoreChooseFirstCommand { get; }
        public IActionCommand CreateMoreChooseSecondCommand { get; }
        public IActionCommand CreateMoreChooseResultFolderCommand { get; }
        public IActionCommand CreateMoreStartCommand { get; }

        public IActionCommand TranslateMoreChooseCommand { get; }
        public IActionCommand TranslateMoreDictionaryFolderCommand { get; }
        public IActionCommand TranslateMoreStartCommand { get; }

        public ChangesDetectorWindowViewModel()
        {
            ProgressValue = new FieldProperty<int>();
            ProgressMax = new FieldProperty<int>();

            CreateFirstFolder = new FieldProperty<string>();
            CreateSecondFolder = new FieldProperty<string>();
            CreateResultFolder = new FieldProperty<string>();

            TranslateFolder = new FieldProperty<string>();
            TranslateDictionaryFolder = new FieldProperty<string>();

            CreateMoreFirstFolder = new FieldProperty<string>();
            CreateMoreSecondFolder = new FieldProperty<string>();
            CreateMoreResultFolder = new FieldProperty<string>();

            TranslateMoreFolder = new FieldProperty<string>();
            TranslateMoreDictionaryFolder = new FieldProperty<string>();

            CreateChooseFirstCommand = new ActionCommand(CreateChooseFirstCommand_Execute, NotLoading);
            CreateChooseSecondCommand = new ActionCommand(CreateChooseSecondCommand_Execute, NotLoading);
            CreateChooseResultFolderCommand = new ActionCommand(CreateChooseResultFolderCommand_Execute, NotLoading);
            CreateStartCommand = new ActionCommand(CreateStartCommand_Execute, NotLoading);

            TranslateChooseFolderCommand = new ActionCommand(TranslateChooseFolderCommand_Execute, NotLoading);
            TranslateDictionaryFolderCommand = new ActionCommand(TranslateDictionaryFolderCommand_Execute, NotLoading);
            TranslateStartCommand = new ActionCommand(TranslateStartCommand_Execute, NotLoading);

            CreateMoreChooseFirstCommand = new ActionCommand(CreateMoreChooseFirstCommand_Execute, NotLoading);
            CreateMoreChooseSecondCommand = new ActionCommand(CreateMoreChooseSecondCommand_Execute, NotLoading);
            CreateMoreChooseResultFolderCommand = new ActionCommand(CreateMoreChooseResultFolderCommand_Execute, NotLoading);
            CreateMoreStartCommand = new ActionCommand(CreateMoreStartCommand_Execute, NotLoading);

            TranslateMoreChooseCommand = new ActionCommand(TranslateMoreChooseCommand_Execute, NotLoading);
            TranslateMoreDictionaryFolderCommand = new ActionCommand(TranslateMoreDictionaryFolderCommand_Execute, NotLoading);
            TranslateMoreStartCommand = new ActionCommand(TranslateMoreStartCommand_Execute, NotLoading);

            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(IsBusy):
                    RaiseCommandCanExecute();
                    break;
            }
        }

        public override void UnsubscribeFromEvents() { }

        private bool NotLoading() => !IsBusy;

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
                !IOUtils.FolderExists(CreateFirstFolder.Value) ||
                !IOUtils.FolderExists(CreateSecondFolder.Value)
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
                !IOUtils.FolderExists(TranslateFolder.Value) ||
                !IOUtils.FolderExists(TranslateDictionaryFolder.Value)
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
                !IOUtils.FolderExists(CreateMoreFirstFolder.Value) ||
                !IOUtils.FolderExists(CreateMoreSecondFolder.Value)
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
                !IOUtils.FolderExists(TranslateMoreFolder.Value) ||
                !IOUtils.FolderExists(TranslateMoreDictionaryFolder.Value)
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
            CreateChooseFirstCommand.RaiseCanExecuteChanged();
            CreateChooseSecondCommand.RaiseCanExecuteChanged();
            CreateChooseResultFolderCommand.RaiseCanExecuteChanged();
            CreateStartCommand.RaiseCanExecuteChanged();

            TranslateChooseFolderCommand.RaiseCanExecuteChanged();
            TranslateDictionaryFolderCommand.RaiseCanExecuteChanged();
            TranslateStartCommand.RaiseCanExecuteChanged();

            CreateMoreChooseFirstCommand.RaiseCanExecuteChanged();
            CreateMoreChooseSecondCommand.RaiseCanExecuteChanged();
            CreateMoreChooseResultFolderCommand.RaiseCanExecuteChanged();
            CreateMoreStartCommand.RaiseCanExecuteChanged();

            TranslateMoreChooseCommand.RaiseCanExecuteChanged();
            TranslateMoreDictionaryFolderCommand.RaiseCanExecuteChanged();
            TranslateMoreStartCommand.RaiseCanExecuteChanged();
        }

        private void CreateOneFolderDictionary(string sourceFolder, string modifiedFolder, string resultFolder, bool log = true)
        {
            void LogInner(string contents = "", bool writeNewLine = true)
            {
                if (log)
                    Log(contents, writeNewLine);
            }

            if (IOUtils.FolderExists(resultFolder))
                IOUtils.DeleteFolder(resultFolder);

            IOUtils.CreateFolder(resultFolder);

            var dictFileWriter = new StreamWriter(Path.Combine(resultFolder, "Paths.dict"), false, Encoding.UTF8);
            var foldersFileWriter = new StreamWriter(Path.Combine(resultFolder, "Languages.dict"), false, Encoding.UTF8);

            if (new[] { sourceFolder, modifiedFolder, resultFolder }.Any(str => str.IsNullOrEmpty() || !IOUtils.FolderExists(str)))
            {
                return;
            }

            string dictsFolder = Path.Combine(resultFolder, "Dictionaries");
            IOUtils.CreateFolder(dictsFolder);

            string languagesFolder = Path.Combine(resultFolder, "Languages");
            IOUtils.CreateFolder(languagesFolder);

            LogInner("Searching for xml files in the first folder...");

            List<XmlFile> firstXmlFiles =
                Directory.EnumerateFiles(sourceFolder, "*.xml", SearchOption.AllDirectories)
                    .SelectSafe(file => new XmlFile(file))
                    .ToList();


            LogInner("Searching for xml files in the second folder...");

            List<XmlFile> secondXmlFiles =
                Directory.EnumerateFiles(modifiedFolder, "*.xml", SearchOption.AllDirectories)
                    .SelectSafe(file => new XmlFile(file))
                    .ToList();

            secondXmlFiles.Sort((fFile, sFile) => string.Compare(fFile.FileName, sFile.FileName, StringComparison.Ordinal));

            ProgressValue.Value = 0;
            ProgressMax.Value = firstXmlFiles.Count;

            int filenameIndex = 0;
            var errors = new List<string>();

            LogInner("Comparing...");

            var comparison =
                new ComparisonWrapper<XmlFile>((fFile, sFile) =>
                    string.Compare(
                        fFile.FileName.Substring(modifiedFolder.Length + 1),
                        sFile.FileName.Substring(sourceFolder.Length + 1),
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

            LogInner("Comparing languages...");

            ProgressValue.Value = 0;
            ProgressMax.Value = modifiedFolders.Count;

            foreach (string folder in modifiedFolders)
            {
                ProgressValue.Value++;

                string part = Path.GetFileName(folder) ?? string.Empty;

                if (sourceFolders.Contains(part))
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
            void LogInner(string contents = "", bool writeNewLine = true)
            {
                if (log)
                    Log(contents, writeNewLine);
            }

            LogInner("Opening dictionaries...");

            string[] paths = File.ReadAllLines(Path.Combine(dictionaryFolder, "Paths.dict"));

            List<DictionaryFile> dicts =
                Directory.EnumerateFiles(Path.Combine(dictionaryFolder, "Dictionaries"))
                    .Select(file => new DictionaryFile(file))
                    .ToList();

            ProgressValue.Value = 0;
            ProgressMax.Value = paths.Length;

            LogInner("Translating files:");

            for (int i = 0; i < paths.Length; i++)
            {
                ProgressValue.Value++;
                string xmlFilePath = Path.Combine(fileFolder, paths[i]);

                LogInner("  -- " + xmlFilePath, false);

                if (!IOUtils.FileExists(xmlFilePath))
                {
                    LogInner(" - skipped");

                    continue;
                }

                DictionaryFile dict = dicts.Find(d => Path.GetFileNameWithoutExtension(d.FileName) == i.ToString());

                if (dict == null)
                {
                    LogInner(" - skipped");

                    continue;
                }

                try
                {
                    var xmlFile = new XmlFile(xmlFilePath);

                    xmlFile.TranslateWithDictionary(dict, true);

                    LogInner(" - translated");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Message: {ex.Message}\nStackTrace: {ex.StackTrace}");
                    LogInner(" - error");
                }
            }

            if (!IOUtils.FileExists(Path.Combine(dictionaryFolder, "Languages.dict")))
                return;

            LogInner("Adding languages:");

            string[] languages = File.ReadAllLines(Path.Combine(dictionaryFolder, "Languages.dict"));

            ProgressValue.Value = 0;
            ProgressMax.Value = languages.Length;

            foreach (var language in languages)
            {
                ProgressValue.Value++;

                LogInner("  -- " + language);

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
            if (IOUtils.FolderExists(resultFolder))
                IOUtils.DeleteFolder(resultFolder);

            IOUtils.CreateFolder(resultFolder);

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

                    if (!IOUtils.FolderExists(dictPath))
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
                if (IOUtils.FolderExists(folder))
                    IOUtils.DeleteFolder(folder);

                IOUtils.CreateFolder(folder);
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
                Process.Start(IOUtils.FolderExists(resultSignedFolder) ? resultSignedFolder : filesFolder);
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
                foreach (var sourceString in first.SpecDetails)
                {
                    IOneString item = strings.FirstOrDefault(targetString => sourceString.EqualsNavigations(targetString));

                    if (item != null && sourceString.OldText != item.OldText)
                        result.Add(sourceString.OldText, item.OldText);
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
