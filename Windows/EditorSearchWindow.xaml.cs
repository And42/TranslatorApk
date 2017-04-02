using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AndroidTranslator;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.ScrollAxis;
using TranslatorApk.Annotations;
using TranslatorApk.Logic;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.EventManagerLogic;
using TranslatorApk.Logic.Events;
using TranslatorApk.Logic.OrganisationItems;
using UsefulFunctionsLib;

using StringResources = TranslatorApk.Resources.Localizations.Resources;

namespace TranslatorApk.Windows
{
    /// <summary>
    /// Логика взаимодействия для EditorSearchWindow.xaml
    /// </summary>
    public sealed partial class EditorSearchWindow : INotifyPropertyChanged
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
            get
            {
                return SettingsIncapsuler.EditorSOnlyFullWords;
            }
            set
            {
                SettingsIncapsuler.EditorSOnlyFullWords = value;
                OnPropertyChanged(nameof(OnlyFullWords));
            }
        }

        /// <summary>
        /// С учётом регистра
        /// </summary>
        public bool MatchCase
        {
            get
            {
                return SettingsIncapsuler.EditorSMatchCase;
            }
            set
            {
                SettingsIncapsuler.EditorSMatchCase = value;
                OnPropertyChanged(nameof(MatchCase));
            }
        }

        /// <summary>
        /// Текст для поиска
        /// </summary>
        public string TextToSearch
        {
            get
            {
                return _textToSearch;
            }
            set
            {
                _textToSearch = value;
                OnPropertyChanged(nameof(TextToSearch));
            }
        }
        private string _textToSearch;

        public ObservableCollection<string> SearchAdds { get; } = new ObservableCollection<string>(
            Properties.Settings.Default.EditorSearchAdds != null ?
                Properties.Settings.Default.EditorSearchAdds.Cast<string>() :
                new List<string>());

        public EditorSearchWindow()
        {
            InitializeComponent();
            SearchBox.SelectedIndex = -1;

            FoundedItemsGrid.SelectionController = new GridSelectionControllerExt(FoundedItemsGrid, FoundedItemsView_OnKeyDown, PointerOperation);
        }

        private bool _showSelected;

        private bool PointerOperation(GridPointerEventArgs gridPointerEventArgs, RowColumnIndex rowColumnIndex)
        {
            if (gridPointerEventArgs.Operation == Syncfusion.UI.Xaml.Grid.PointerOperation.DoubleTapped)
            {
                _showSelected = true;
                return false;
            }

            if (gridPointerEventArgs.Operation == Syncfusion.UI.Xaml.Grid.PointerOperation.Tapped && _showSelected)
            {
                _showSelected = false;
                ShowSelectedItemInEditor();
                return true;
            }

            return false;
        }

        private void FindAllClick(object sender, RoutedEventArgs e)
        {
            var files = EditorWindow.StringFilesStatic;

            if (files == null)
            {
                MessBox.ShowDial(StringResources.SearchEditorClosed, StringResources.ErrorLower);
                return;
            } 

            if (string.IsNullOrEmpty(TextToSearch))
                return;

            var founded = new Dictionary<EditableFile, List<OneString>>();

            string searchText = MatchCase ? TextToSearch : TextToSearch.ToUpper();

            LoadingWindow.ShowWindow(() => IsEnabled = false, cts =>
            {
                StringComparison comparison = MatchCase ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

                Predicate<OneString> checker = OnlyFullWords
                    ? (Predicate<OneString>) (str => str.OldText.Equals(searchText, comparison))
                    : (str => str.OldText.IndexOf(searchText, comparison) > -1);

                foreach (EditableFile currentFile in files)
                {
                    List<OneString> items = new List<OneString>();

                    foreach (OneString str in currentFile.Details)
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
            }, () => Dispatcher.InvokeAction(() =>
            {
                Activate();
                IsEnabled = true;

                if (founded.Count == 0)
                {
                    FoundItems.Clear();
                    MessBox.ShowDial(StringResources.TextNotFound);
                    return;
                }

                var selected = founded.SelectMany(it => it.Value.Select(val => new {fileName = it.Key.FileName, str = val}))
                    .Select(it => new OneFoundItem(it.fileName, it.str.OldText, it.str)).ToArray();

                FoundItems.ReplaceRange(selected);
            }), Visibility.Collapsed);
        }

        private void ShowSelectedItemInEditor()
        {
            var item = FoundedItemsGrid.SelectedItem as OneFoundItem;

            if (item == null)
            {
                return;
            }

            WindowManager.ActivateWindow<EditorWindow>();

            ManualEventManager.GetEvent<EditorScrollToStringAndSelectEvent>()
                .Publish(new EditorScrollToStringAndSelectEvent(f => f.FileName == item.FileName, s => s == item.EditString));
        }

        private void FindNextClick(object sender, RoutedEventArgs e)
        {
            SfDataGrid editorGrid = EditorWindow.EditorGridStatic;

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

                EditableFile currentFile = currentFileRow.Data.As<EditableFile>();

                Func<EditableFile, OneString, bool> process = (file, instr) =>
                {
                    string oldText = MatchCase ? instr.OldText : instr.OldText.ToUpper();

                    if (OnlyFullWords ? oldText != searchText : !oldText.Contains(searchText))
                        return false;

                    WindowManager.ActivateWindow<EditorWindow>();

                    ManualEventManager.GetEvent<EditorScrollToStringAndSelectEvent>()
                        .Publish(new EditorScrollToStringAndSelectEvent(
                            f => f.FileName == file.FileName, str => str == instr));

                    return true;
                };

                if (currentFileRow.IsExpanded)
                {
                    var childRecords = currentFileRow.ChildViews.First().Value.NestedRecords;

                    for (int j = inFileIndex; j < childRecords.Count; j++)
                    {
                        OneString currentString = childRecords[j].Data.As<OneString>();

                        if (process(currentFile, currentString))
                            return;
                    }
                }
                else
                {
                    foreach (OneString currentString in currentFile.Details)
                    {
                        if (process(currentFile, currentString))
                        {
                            SfDataGrid detailsGrid = editorGrid.GetDetailsViewGrid(i, "Details");
                            
                            foreach (RecordEntry record in detailsGrid.View.Records)
                                if (process(currentFile, record.Data.As<OneString>()))
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

                if (Properties.Settings.Default.EditorSearchAdds == null) Properties.Settings.Default.EditorSearchAdds = new StringCollection();
                Properties.Settings.Default.EditorSearchAdds.Remove(text);
                Properties.Settings.Default.EditorSearchAdds.Insert(0, text);

                if (SearchAdds.Count > 20)
                {
                    SearchAdds.RemoveAt(20);
                    Properties.Settings.Default.EditorSearchAdds.RemoveAt(20);
                }

                Properties.Settings.Default.Save();
            });
        }

        #pragma warning disable CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]

        private void OnPropertyChanged(string propertyName)
        #pragma warning restore CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
    }
}
