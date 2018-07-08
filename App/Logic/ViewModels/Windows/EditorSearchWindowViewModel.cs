using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using AndroidTranslator.Interfaces.Files;
using AndroidTranslator.Interfaces.Strings;
using MVVM_Tools.Code.Commands;
using MVVM_Tools.Code.Providers;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.EventManagerLogic;
using TranslatorApk.Logic.Events;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.Utils;
using TranslatorApk.Resources.Localizations;
using TranslatorApk.Windows;

namespace TranslatorApk.Logic.ViewModels.Windows
{
    internal class EditorSearchWindowViewModel : ViewModelBase
    {
        private const int SearchHistory = 20;

        private readonly AppSettingsBase _appSettings = GlobalVariables.AppSettings;
        private readonly Window _window;

        public ObservableRangeCollection<string> SearchAdds { get; }
        public ObservableRangeCollection<OneFoundItem> FoundItems { get; }

        public bool OnlyFullWords
        {
            get => _appSettings.EditorSOnlyFullWords;
            set
            {
                _appSettings.EditorSOnlyFullWords = value;
                OnPropertyChanged();
            }
        }

        public bool MatchCase
        {
            get => _appSettings.EditorSMatchCase;
            set
            {
                _appSettings.EditorSMatchCase = value;
                OnPropertyChanged();
            }
        }

        public Property<string> TextToSearch { get; private set; }
        public Property<int> SearchBoxIndex { get; private set; }

        public IActionCommand FindAllCommand { get; }
        public IActionCommand FindNextCommand { get; }
        public IActionCommand<OneFoundItem> ShowItemInEditorCommand { get; }

        public EditorSearchWindowViewModel(Window window)
        {
            _window = window ?? throw new ArgumentNullException(nameof(window));

            FoundItems = new ObservableRangeCollection<OneFoundItem>();

            SearchAdds = new ObservableRangeCollection<string>(
                _appSettings.EditorSearchAdds ?? Enumerable.Empty<string>()
            );

            BindProperty(() => TextToSearch);
            BindProperty(() => SearchBoxIndex, -1);

            FindAllCommand = new ActionCommand(FindAllCommand_Execute, () => !IsBusy);
            FindNextCommand = new ActionCommand(FindNextCommand_Execute, () => !IsBusy);
            ShowItemInEditorCommand = new ActionCommand<OneFoundItem>(ShowItemInEditorCommand_Execute, _ => !IsBusy);

            PropertyChanged += OnPropertyChanged;
            _appSettings.PropertyChanged += SettingsOnPropertyChanged;
        }

        private void FindAllCommand_Execute()
        {
            var files = WindowManager.GetActiveWindow<EditorWindow>()?.StringFiles;

            if (files == null)
            {
                MessBox.ShowDial(StringResources.SearchEditorClosed, StringResources.ErrorLower);
                return;
            }

            if (string.IsNullOrEmpty(TextToSearch.Value))
                return;

            var found = new Dictionary<IEditableFile, List<IOneString>>();

            string searchText = MatchCase ? TextToSearch.Value : TextToSearch.Value.ToUpper();

            LoadingWindow.ShowWindow(
                beforeStarting: () => IsBusy = true,
                threadActions: cts =>
                {
                    var comparison = MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

                    Func<IOneString, bool> checker = OnlyFullWords
                        ? (Func<IOneString, bool>)(str => str.OldText.Equals(searchText, comparison))
                        : (str => str.OldText.IndexOf(searchText, comparison) != -1);

                    foreach (IEditableFile currentFile in files)
                    {
                        List<IOneString> items = currentFile.Details.Where(checker).ToList();

                        if (items.Count > 0)
                            found.Add(currentFile, items);
                    }

                    AddToSearchAdds(TextToSearch.Value);
                },
                finishActions: () =>
                {
                    IsBusy = false;

                    if (found.Count == 0)
                    {
                        FoundItems.Clear();
                        MessBox.ShowDial(StringResources.TextNotFound);
                        return;
                    }

                    var selected =
                        found.SelectMany(
                            it => it.Value.Select(val => new OneFoundItem(it.Key.FileName, val.OldText, val)));

                    FoundItems.ReplaceRange(selected);
                },
                cancelVisibility: Visibility.Collapsed,
                ownerWindow: _window
            );
        }

        private void FindNextCommand_Execute()
        {
            SfDataGrid editorGrid = WindowManager.GetActiveWindow<EditorWindow>()?.EditorGrid;

            if (editorGrid == null)
            {
                MessBox.ShowDial(StringResources.SearchEditorClosed, StringResources.ErrorLower);
                return;
            }

            if (string.IsNullOrEmpty(TextToSearch.Value))
                return;

            AddToSearchAdds(TextToSearch.Value);

            int fileIndex;
            int inFileIndex = 0;

            var selectedMasterGrid = editorGrid.SelectedDetailsViewGrid;

            if (selectedMasterGrid != null)
            {
                int parentRecordIndex = editorGrid.ResolveToRecordIndex(editorGrid.GetSelectedDetailsViewGridRowIndex());
                int childIndex = selectedMasterGrid.SelectedIndex;

                if (childIndex + 1 < selectedMasterGrid.View.Records.Count)
                {
                    inFileIndex = selectedMasterGrid.SelectedIndex + 1;
                    fileIndex = parentRecordIndex;
                }
                else
                {
                    inFileIndex = 0;
                    fileIndex = parentRecordIndex + 1;
                }
            }
            else
            {
                fileIndex = editorGrid.SelectedIndex > -1 ? editorGrid.SelectedIndex : 0;
            }

            string searchText = MatchCase ? TextToSearch.Value : TextToSearch.Value.ToUpper();

            for (int i = fileIndex; i < editorGrid.View.Records.Count; i++)
            {
                RecordEntry currentFileRow = editorGrid.View.Records[i];

                var currentFile = currentFileRow.Data.As<IEditableFile>();

                bool Process(IEditableFile file, IOneString instr)
                {
                    string oldText = MatchCase ? instr.OldText : instr.OldText.ToUpper();

                    if (OnlyFullWords ? oldText != searchText : !oldText.Contains(searchText))
                        return false;

                    WindowManager.ActivateWindow<EditorWindow>();

                    ManualEventManager.GetEvent<EditorScrollToStringAndSelectEvent>()
                        .Publish(new EditorScrollToStringAndSelectEvent(f => f.FileName == file.FileName, str => str == instr));

                    return true;
                }

                if (currentFileRow.IsExpanded)
                {
                    var childRecords = currentFileRow.ChildViews.First().Value.NestedRecords;

                    for (int j = inFileIndex; j < childRecords.Count; j++)
                    {
                        var currentString = childRecords[j].Data.As<IOneString>();

                        if (Process(currentFile, currentString))
                            return;
                    }
                }
                else
                {
                    foreach (IOneString currentString in currentFile.Details)
                    {
                        if (Process(currentFile, currentString))
                        {
                            SfDataGrid detailsGrid = editorGrid.GetDetailsViewGrid(i, "Details");

                            foreach (RecordEntry record in detailsGrid.View.Records)
                                if (Process(currentFile, record.Data.As<IOneString>()))
                                    return;
                        }
                    }
                }

                inFileIndex = 0;
            }

            MessBox.ShowDial(StringResources.TextNotFound);
        }

        private static void ShowItemInEditorCommand_Execute(OneFoundItem item)
        {
            if (item == null)
                return;

            WindowManager.ActivateWindow<EditorWindow>();

            ManualEventManager.GetEvent<EditorScrollToStringAndSelectEvent>()
                .Publish(new EditorScrollToStringAndSelectEvent(
                    f => f.FileName == item.FileName, s => s == item.EditString)
                );
        }

        private void AddToSearchAdds(string text)
        {
            Application.Current.Dispatcher.InvokeAction(() =>
            {
                SearchAdds.Remove(text);
                SearchAdds.Insert(0, text);
                SearchBoxIndex.Value = 0;

                var settings = _appSettings;

                if (settings.EditorSearchAdds == null)
                    settings.EditorSearchAdds = new List<string>();

                settings.EditorSearchAdds.Remove(text);
                settings.EditorSearchAdds.Insert(0, text);

                if (SearchAdds.Count > SearchHistory)
                {
                    SearchAdds.RemoveAt(SearchHistory);
                    settings.EditorSearchAdds.RemoveAt(SearchHistory);
                }

                settings.Save();
            });
        }

        private void RaiseCommandsCanExecute()
        {
            FindAllCommand.RaiseCanExecuteChanged();
            FindNextCommand.RaiseCanExecuteChanged();
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

        private void SettingsOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(AppSettingsBase.EditorSOnlyFullWords):
                    OnPropertyChanged(nameof(OnlyFullWords));
                    break;
                case nameof(AppSettingsBase.EditorSMatchCase):
                    OnPropertyChanged(nameof(MatchCase));
                    break;
            }
        }

        public override void UnsubscribeFromEvents()
        {
            PropertyChanged -= OnPropertyChanged;
            _appSettings.PropertyChanged -= SettingsOnPropertyChanged;
        }

        public override void Dispose()
        {
            UnsubscribeFromEvents();

            FoundItems.Clear();
        }
    }
}
