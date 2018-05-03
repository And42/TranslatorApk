using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
using UsefulFunctionsLib;

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

        private readonly Window _window;
        private static readonly string StartFormattedString = "..." + Path.DirectorySeparatorChar;

        public ReadOnlyObservableCollection<FoundItem> Files { get; }
        private readonly ObservableRangeCollection<FoundItem> _files;

        public ReadOnlyObservableCollection<string> SearchAdds { get; }
        private readonly ObservableRangeCollection<string> _searchAdds;

        public Setting<bool> OnlyFullWords { get; }
        public Setting<bool> MatchCase { get; }

        public PropertyProvider<string> TextToSearch { get; }
        public PropertyProvider<int> SearchBoxIndex { get; } 
        public PropertyProvider<FoundItem> SelectedFile { get; }

        public IActionCommand FindFilesCommand { get; }
        public IActionCommand LoadSelectedFileCommand { get; }

        public SearchWindowViewModel(Window window)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));

            _files = new ObservableRangeCollection<FoundItem>();
            Files = new ReadOnlyObservableCollection<FoundItem>(_files);

            _searchAdds = new ObservableRangeCollection<string>(
                DefaultSettingsContainer.Instance.FullSearchAdds?.Cast<string>() ?? Enumerable.Empty<string>()
            );
            SearchAdds = new ReadOnlyObservableCollection<string>(_searchAdds);

            OnlyFullWords = new Setting<bool>(nameof(DefaultSettingsContainer.OnlyFullWords), StringResources.OnlyFullWords);
            MatchCase = new Setting<bool>(nameof(DefaultSettingsContainer.MatchCase), StringResources.MatchCase);

            TextToSearch = CreateProviderWithNotify<string>(nameof(TextToSearch));
            SearchBoxIndex = CreateProviderWithNotify(nameof(SearchBoxIndex), -1);
            SelectedFile = CreateProviderWithNotify<FoundItem>(nameof(SelectedFile));

            FindFilesCommand = new ActionCommand(FindFilesCommand_Execute, () => !IsBusy);
            LoadSelectedFileCommand = new ActionCommand(LoadSelectedFileCommand_Execute, () => !IsBusy && SelectedFile.Value != null);

            PropertyChanged += OnPropertyChanged;
        }

        private void FindFilesCommand_Execute()
        {
            if (GlobalVariables.CurrentProjectFolder == null)
            {
                MessBox.ShowDial(StringResources.SearchWindow_FolderIsNotSelected);
                return;
            }

            string GetFormattedName(string fileName) => StartFormattedString + fileName.Substring(GlobalVariables.CurrentProjectFolder.Length + 1);

            AddToSearchAdds(TextToSearch.Value);

            var filesToAdd = new List<FoundItem>();

            LoadingProcessWindow.ShowWindow(
                beforeStarting: () => IsBusy = true,
                threadActions: (cts, invoker) =>
                {
                    var projectFolder = GlobalVariables.CurrentProjectFolder;
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
                            .UnionWOEqCheck(smaliFiles.SelectSafe(it => new SmaliFile(it)));

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
                        _files.Clear();
                        MessBox.ShowDial(StringResources.TextNotFound);
                    }
                    else
                    {
                        _files.ReplaceRange(filesToAdd);
                    }
                },
                ownerWindow: _window
            );
        }

        private void AddToSearchAdds(string text)
        {
            _searchAdds.Remove(text);
            _searchAdds.Insert(0, text);
            SearchBoxIndex.Value = 0;

            if (DefaultSettingsContainer.Instance.FullSearchAdds == null)
                DefaultSettingsContainer.Instance.FullSearchAdds = new StringCollection();

            StringCollection adds = DefaultSettingsContainer.Instance.FullSearchAdds;

            adds.Remove(text);
            adds.Insert(0, text);

            if (SearchAdds.Count > 20)
            {
                _searchAdds.RemoveAt(20);
                adds.RemoveAt(20);
            }

            DefaultSettingsContainer.Instance.Save();
        }

        private void LoadSelectedFileCommand_Execute()
        {
            var selectedFile = SelectedFile.Value;

            if (selectedFile == null)
                return;

            Utils.Utils.LoadFile(Path.Combine(GlobalVariables.CurrentProjectFolder, selectedFile.FileName.Substring(StartFormattedString.Length)));

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
                case nameof(SelectedFile):
                    LoadSelectedFileCommand.RaiseCanExecuteChanged();
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

            _files.Clear();
            _searchAdds.Clear();
        }
    }
}
