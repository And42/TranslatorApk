using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using AndroidTranslator.Classes.Files;
using AndroidTranslator.Interfaces.Files;
using AndroidTranslator.Interfaces.Strings;
using MVVM_Tools.Code.Commands;
using MVVM_Tools.Code.Providers;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.EventManagerLogic;
using TranslatorApk.Logic.Events;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.Utils;
using TranslatorApk.Resources.Localizations;
using TranslatorApk.Windows;

namespace TranslatorApk.Logic.ViewModels.Windows
{
    internal class SearchWindowViewModel : ViewModelBase
    {
        internal class FoundItem
        {
            public string FileName { get; }
            public string Text { get; }

            public FoundItem(string fileName, string text)
            {
                FileName = fileName;
                Text = text;
            }
        }

        private static readonly string StartFormattedString = "..." + Path.DirectorySeparatorChar;

        private readonly AppSettings _appSettings = GlobalVariables.AppSettings;
        private readonly GlobalVariables _globalVariables = GlobalVariables.Instance;
        private readonly Window _window;
        
        public ObservableRangeCollection<FoundItem> Files { get; } = new();
        public ObservableRangeCollection<string> SearchAdds { get; }

        public Setting<bool> OnlyFullWords { get; }
        public Setting<bool> MatchCase { get; }

        public FieldProperty<int> SearchBoxIndex { get; } = new(-1);
        public FieldProperty<string> TextToSearch { get; } = new();
        public FieldProperty<FoundItem> SelectedFile { get; } = new();

        public IActionCommand FindFilesCommand { get; }
        public IActionCommand LoadSelectedFileCommand { get; }

        public SearchWindowViewModel(Window window)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));

            SearchAdds = new ObservableRangeCollection<string>(
                _appSettings.FullSearchAdds ?? Enumerable.Empty<string>()
            );

            OnlyFullWords = new Setting<bool>(s => s.OnlyFullWords, StringResources.OnlyFullWords);
            MatchCase = new Setting<bool>(s => s.MatchCase, StringResources.MatchCase);

            FindFilesCommand = new ActionCommand(FindFilesCommand_Execute, () => !IsBusy);
            LoadSelectedFileCommand = new ActionCommand(LoadSelectedFileCommand_Execute, () => !IsBusy && SelectedFile.Value != null);

            SelectedFile.PropertyChanged += (sender, args) => LoadSelectedFileCommand.RaiseCanExecuteChanged();

            PropertyChanged += OnPropertyChanged;
        }

        private void FindFilesCommand_Execute()
        {
            if (_globalVariables.CurrentProjectFolder.Value.IsNullOrEmpty())
            {
                MessBox.ShowDial(StringResources.SearchWindow_FolderIsNotSelected);
                return;
            }

            string GetFormattedName(string fileName)
            {
                return StartFormattedString + fileName.Substring(_globalVariables.CurrentProjectFolder.Value.Length + 1);
            }

            AddToSearchAdds(TextToSearch.Value);

            var filesToAdd = new List<FoundItem>();

            LoadingProcessWindow.ShowWindow(
                beforeStarting: () => IsBusy = true,
                threadActions: (cts, invoker) =>
                {
                    var projectFolder = _globalVariables.CurrentProjectFolder.Value;
                    var buildFolder = Path.DirectorySeparatorChar + "build";
                    var buildFolderM = Path.DirectorySeparatorChar + "build" + Path.DirectorySeparatorChar;

                    List<string> xmlFiles =
                        Directory.EnumerateFiles(projectFolder, "*.xml", SearchOption.AllDirectories)
                            .Where(file =>
                            {
                                // ReSharper disable once PossibleNullReferenceException
                                var dir = Path.GetDirectoryName(file).Substring(projectFolder.Length);
                                return dir != buildFolder && !dir.StartsWith(buildFolderM, StringComparison.Ordinal);
                            }
                            )
                            .ToList();

                    List<string> smaliFiles = Directory.EnumerateFiles(projectFolder, "*.smali", SearchOption.AllDirectories).ToList();

                    invoker.ProcessValue = 0;
                    invoker.ProcessMax = xmlFiles.Count + smaliFiles.Count;

                    bool onlyFullWords = OnlyFullWords.Value;
                    bool matchCase = MatchCase.Value;

                    Func<string, string, bool> checkRules;

                    if (!matchCase && !onlyFullWords)
                        checkRules = (f, s) => f.IndexOf(s, StringComparison.OrdinalIgnoreCase) != -1;
                    else if (!matchCase /*&& onlyFullWords*/)
                        checkRules = (f, s) => f.Equals(s, StringComparison.OrdinalIgnoreCase);
                    else if (/*matchCase &&*/ !onlyFullWords)
                        checkRules = (f, s) => f.IndexOf(s, StringComparison.Ordinal) != -1;
                    else /*if (matchCase && onlyFullWords)*/
                        checkRules = (f, s) => f.Equals(s, StringComparison.Ordinal);

                    IEnumerable<IEditableFile> union =
                        xmlFiles.SelectSafe<string, IEditableFile>(XmlFile.Create)
                            .Concat(smaliFiles.SelectSafe(it => new SmaliFile(it)));

                    foreach (IEditableFile file in union)
                    {
                        cts.ThrowIfCancellationRequested();

                        IOneString found = file.Details?.FirstOrDefault(str => checkRules(str.OldText, TextToSearch.Value));

                        if (found != null)
                        {
                            filesToAdd.Add(new FoundItem(GetFormattedName(file.FileName), found.OldText));
                        }

                        invoker.ProcessValue++;
                    }
                },
                finishActions: () =>
                {
                    IsBusy = false;

                    if (filesToAdd.Count == 0)
                    {
                        Files.Clear();
                        MessBox.ShowDial(StringResources.TextNotFound);
                    }
                    else
                    {
                        Files.ReplaceRange(filesToAdd);
                    }
                },
                ownerWindow: _window
            );
        }

        private void AddToSearchAdds(string text)
        {
            SearchAdds.Remove(text);
            SearchAdds.Insert(0, text);
            SearchBoxIndex.Value = 0;

            var adds = new List<string>(_appSettings.FullSearchAdds ?? Enumerable.Empty<string>());

            adds.Remove(text);
            adds.Insert(0, text);

            if (SearchAdds.Count > 20)
            {
                SearchAdds.RemoveAt(20);
                adds.RemoveAt(20);
            }

            _appSettings.FullSearchAdds = adds;
        }

        private void LoadSelectedFileCommand_Execute()
        {
            var selectedFile = SelectedFile.Value;

            if (selectedFile == null)
                return;

            CommonUtils.LoadFile(Path.Combine(_globalVariables.CurrentProjectFolder.Value, selectedFile.FileName.Substring(StartFormattedString.Length)));

            ManualEventManager.GetEvent<EditorScrollToStringAndSelectEvent>()
                .Publish(new EditorScrollToStringAndSelectEvent(
                    _ => true, str => selectedFile.Text.Equals(str.OldText, StringComparison.Ordinal)));
        }

        private void RaiseCommandsCanExecute()
        {
            FindFilesCommand.RaiseCanExecuteChanged();
            LoadSelectedFileCommand.RaiseCanExecuteChanged();
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(IsBusy):
                    RaiseCommandsCanExecute();
                    break;
            }
        }

        public override void UnsubscribeFromEvents()
        {
            PropertyChanged -= OnPropertyChanged;
        }

        public override void Dispose()
        {
            UnsubscribeFromEvents();

            Files.Clear();
            SearchAdds.Clear();
        }
    }
}
