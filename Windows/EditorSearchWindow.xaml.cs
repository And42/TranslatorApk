using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AndroidTranslator.Interfaces.Files;
using AndroidTranslator.Interfaces.Strings;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.EventManagerLogic;
using TranslatorApk.Logic.Events;
using TranslatorApk.Logic.Interfaces;
using TranslatorApk.Logic.OrganisationItems;
using UsefulFunctionsLib;

using StringResources = TranslatorApk.Resources.Localizations.Resources;

namespace TranslatorApk.Windows
{
    /// <summary>
    /// Логика взаимодействия для EditorSearchWindow.xaml
    /// </summary>
    public sealed partial class EditorSearchWindow : IRaisePropertyChanged
    {
        /// <summary>
        /// Список найденных строк
        /// </summary>
        public ObservableRangeCollection<OneFoundItem> FoundItems { get; } = new ObservableRangeCollection<OneFoundItem>();

        /// <summary>
        /// Только целые слова
        /// </summary>
        public bool OnlyFullWords
        {
            get => SettingsIncapsuler.Instance.EditorSOnlyFullWords;
            set
            {
                SettingsIncapsuler.Instance.EditorSOnlyFullWords = value;
                RaisePropertyChanged(nameof(OnlyFullWords));
            }
        }

        /// <summary>
        /// С учётом регистра
        /// </summary>
        public bool MatchCase
        {
            get => SettingsIncapsuler.Instance.EditorSMatchCase;
            set
            {
                SettingsIncapsuler.Instance.EditorSMatchCase = value;
                RaisePropertyChanged(nameof(MatchCase));
            }
        }

        /// <summary>
        /// Текст для поиска
        /// </summary>
        public string TextToSearch
        {
            get => _textToSearch;
            set => this.SetProperty(ref _textToSearch, value);
        }
        private string _textToSearch;

        public ObservableCollection<string> SearchAdds { get; } = 
            new ObservableCollection<string>(
                SettingsIncapsuler.Instance.EditorSearchAdds?.Cast<string>() 
                ?? Enumerable.Empty<string>()
            );

        public EditorSearchWindow()
        {
            InitializeComponent();
            SearchBox.SelectedIndex = -1;

            FoundItemsGrid.SelectionController = new GridSelectionControllerExt(FoundItemsGrid, FoundedItemsView_OnKeyDown);
        }

        private void FoundedItemsGrid_OnCellDoubleTapped(object sender, GridCellDoubleTappedEventArgs args)
        {
            ShowSelectedItemInEditor();
        }

        private void FindAllClick(object sender, RoutedEventArgs e)
        {
            var files = WindowManager.GetActiveWindow<EditorWindow>()?.StringFiles;

            if (files == null)
            {
                MessBox.ShowDial(StringResources.SearchEditorClosed, StringResources.ErrorLower);
                return;
            } 

            if (string.IsNullOrEmpty(TextToSearch))
                return;

            var founded = new Dictionary<IEditableFile, List<IOneString>>();

            string searchText = MatchCase ? TextToSearch : TextToSearch.ToUpper();

            LoadingWindow.ShowWindow(
                beforeStarting: () => IsEnabled = false, 
                threadActions: cts =>
                {
                    StringComparison comparison = MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

                    Predicate<IOneString> checker = OnlyFullWords
                        ? (Predicate<IOneString>) (str => str.OldText.Equals(searchText, comparison))
                        : (str => str.OldText.IndexOf(searchText, comparison) > -1);

                    foreach (IEditableFile currentFile in files)
                    {
                        var items = new List<IOneString>();

                        foreach (var str in currentFile.Details)
                        {
                            if (checker(str))
                            {
                                items.Add(str);
                            }
                        }

                        if (items.Count > 0)
                        {
                            founded.Add(currentFile, items);
                        }
                    }

                    AddToSearchAdds(TextToSearch);
                }, 
                finishActions: () =>
                {
                    Dispatcher.InvokeAction(() =>
                        {
                            Activate();
                            IsEnabled = true;

                            if (founded.Count == 0)
                            {
                                FoundItems.Clear();
                                MessBox.ShowDial(StringResources.TextNotFound);
                                return;
                            }

                            var selected =
                                founded.SelectMany(
                                    it => it.Value.Select(val => new OneFoundItem(it.Key.FileName, val.OldText, val)));

                            FoundItems.ReplaceRange(selected);
                        }
                    );
                }, 
                cancelVisibility: Visibility.Collapsed
            );
        }

        private void ShowSelectedItemInEditor()
        {
            // ReSharper disable once UsePatternMatching
            var item = FoundItemsGrid.SelectedItem as OneFoundItem;

            if (item == null)
                return;

            WindowManager.ActivateWindow<EditorWindow>(this);

            ManualEventManager.GetEvent<EditorScrollToStringAndSelectEvent>()
                .Publish(new EditorScrollToStringAndSelectEvent(f => f.FileName == item.FileName, s => s == item.EditString));
        }

        private void FindNextClick(object sender, RoutedEventArgs e)
        {
            SfDataGrid editorGrid = WindowManager.GetActiveWindow<EditorWindow>()?.EditorGrid;

            if (editorGrid == null)
            {
                MessBox.ShowDial(StringResources.SearchEditorClosed, StringResources.ErrorLower);
                return;
            }

            if (TextToSearch.NE())
                return;

            AddToSearchAdds(TextToSearch);

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

            string searchText = MatchCase ? TextToSearch : TextToSearch.ToUpper();

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

        private void AddToSearchAdds(string text)
        {
            Dispatcher.InvokeAction(() =>
            {
                SearchAdds.Remove(text);
                SearchAdds.Insert(0, text);
                SearchBox.SelectedIndex = 0;

                if (SettingsIncapsuler.Instance.EditorSearchAdds == null)
                    SettingsIncapsuler.Instance.EditorSearchAdds = new StringCollection();

                SettingsIncapsuler.Instance.EditorSearchAdds.Remove(text);
                SettingsIncapsuler.Instance.EditorSearchAdds.Insert(0, text);

                if (SearchAdds.Count > 20)
                {
                    SearchAdds.RemoveAt(20);
                    SettingsIncapsuler.Instance.EditorSearchAdds.RemoveAt(20);
                }

                SettingsIncapsuler.Save();
            });
        }

        private void EditorSearchWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            SearchBox.Focus();
        }

        private bool FoundedItemsView_OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                ShowSelectedItemInEditor();

                return true;
            }

            return false;
        }

        #region Property Changed

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
