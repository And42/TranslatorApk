using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
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
using TranslatorApk.Logic.WebServices;
using UsefulFunctionsLib;

using static TranslatorApk.Logic.Functions;

using Res = TranslatorApk.Resources.Localizations.Resources;
using Clipboard = System.Windows.Clipboard;
using ContextMenu = System.Windows.Controls.ContextMenu;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MenuItem = System.Windows.Controls.MenuItem;

namespace TranslatorApk.Windows
{
    /// <summary>
    /// Логика взаимодействия для EditorWindow.xaml
    /// </summary>
    public partial class EditorWindow : INotifyPropertyChanged
    {
        private static EditorWindow instance;

        public static double EditorHeaderFontSize = 20;
        public static double EditorTextFontSize = 15;

        public static ReadOnlyCollection<string> Languages;

        public static readonly RoutedUICommand TranslateAllFilesCommand = new RoutedUICommand();
        public static readonly RoutedUICommand SaveCommand = new RoutedUICommand();
        public static readonly RoutedUICommand SearchCommand = new RoutedUICommand();

        public bool SaveToDict
        {
            get
            {
                return SettingsIncapsuler.EditorWindow_SaveToDict;
            }
            set
            {
                if (SaveDictionary == null && value) return;
                SettingsIncapsuler.EditorWindow_SaveToDict = value;
                OnPropertyChanged(nameof(SaveToDict));
            }
        }

        public WindowState EditorWindowState
        {
            get
            {
                if (isMinimized)
                    return WindowState.Minimized;

                return SettingsIncapsuler.EditorWMaximized ? WindowState.Maximized : WindowState.Normal;
            }
            set
            {
                if (value != WindowState.Minimized)
                    SettingsIncapsuler.EditorWMaximized = value == WindowState.Maximized;

                isMinimized = value == WindowState.Minimized;

                OnPropertyChanged(nameof(EditorWindowState));
            }
        }
        private bool isMinimized;

        /// <summary>
        /// Список редактируемых файлов
        /// </summary>
        public ObservableRangeCollection<EditableFile> StringFiles { get; } = new ObservableRangeCollection<EditableFile>();

        public static IList<EditableFile> StringFilesStatic => instance?.StringFiles;
        public static SfDataGrid EditorGridStatic => instance?.EditorGrid;

        public DictionaryFile SaveDictionary
        {
            get
            {
                return _saveDictionary;
            }
            set
            {
                _saveDictionary = value;
                SettingsIncapsuler.TargetDictionary = value?.FileName;
                OnPropertyChanged(nameof(SaveDictionary));
            }
        }
        private DictionaryFile _saveDictionary;

        public int LangsBoxItemIndex
        {
            get
            {
                return TranslateService.ShortTargetLanguages.IndexOf(SettingsIncapsuler.TargetLanguage);
            }
            set
            {
                SettingsIncapsuler.TargetLanguage = TranslateService.GetShortTL(LangsBox.Items[value] as string);
                OnPropertyChanged(nameof(LangsBoxItemIndex));
            }
        }

        public EditorWindow()
        {
            SubscribeToEvents();

            InitializeComponent();

            InitGridEvents();

            EditorGrid.SelectionController = new GridSelectionControllerExt(EditorGrid, EditorGrid_OnKeyDown);

            instance = this;

            string settingsTargetDict = SettingsIncapsuler.TargetDictionary;

            if (!settingsTargetDict.NE() && File.Exists(settingsTargetDict))
                SaveDictionary = new DictionaryFile(settingsTargetDict);
        }

        #region События окна

        private void EditorWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            TranslateWithSessionDict();
        }

        private void EditorWindow_OnActivated(object sender, EventArgs e)
        {
            EditorGrid.Focus();
        }

        private void EditorWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (!CheckIfNeedToSave())
            {
                e.Cancel = true;
                return;
            }

            instance = null;

            UnsubscribeFromEvents();

            WindowManager.CloseWindow<EditorSearchWindow>();
        }

        #endregion

        #region Глобальные события

        /// <summary>
        /// Подписывает текущий экземпляр на глобальные события редактора
        /// </summary>
        private void SubscribeToEvents()
        {
            ManualEventManager.GetEvent<EditorScrollToStringAndSelectEvent>()
                .Subscribe(OnScrollToFileAndSelectString);

            ManualEventManager.GetEvent<EditorScrollToFileAndSelectEvent>()
                .Subscribe(OnScrollToFileAndSelect);

            ManualEventManager.GetEvent<EditorWindowTranslateTextEvent>()
                .Subscribe(OnTranslateText);

            ManualEventManager.GetEvent<AddEditableFilesEvent>()
                .Subscribe(OnAddEditableFiles);
        }

        private void OnScrollToFileAndSelectString(EditorScrollToStringAndSelectEvent parameter)
        {
            EditorGrid.ScrollToFileAndSelectString(parameter.FilePredicate, parameter.StringPredicate);
        }

        private void OnScrollToFileAndSelect(EditorScrollToFileAndSelectEvent editorScrollToFileAndSelectEvent)
        {
            EditorGrid.ScrollToFileAndSelect(editorScrollToFileAndSelectEvent.FilePredicate, editorScrollToFileAndSelectEvent.ExpandRecord);
        }

        private void OnTranslateText(EditorWindowTranslateTextEvent editorWindowTranslateTextEvent)
        {
            if (editorWindowTranslateTextEvent.OldText.NE() ||
                editorWindowTranslateTextEvent.NewText.NE())
                return;

            foreach (var file in StringFiles.Where(editorWindowTranslateTextEvent.Filter))
            {
                foreach (var str in file.Details)
                {
                    if (str.OldText.Equals(editorWindowTranslateTextEvent.OldText, StringComparison.Ordinal) && str.NewText.NE())
                        str.NewText = editorWindowTranslateTextEvent.NewText;
                }
            }
        }

        private void OnAddEditableFiles(AddEditableFilesEvent addEditableFilesEvent)
        {
            if (addEditableFilesEvent.Files == null)
                return;

            var union = StringFiles.Join(addEditableFilesEvent.Files, f => f.FileName, f => f.FileName, (f, s) => f, StringComparer.Ordinal).ToList();

            if (addEditableFilesEvent.ClearExisting)
                StringFiles.Clear();

            IEnumerable<EditableFile> toAdd;

            if (union.Count > 0)
            {
                toAdd =
                    addEditableFilesEvent.Files.Except(union,
                        new ComparisonWrapper<EditableFile>(
                            (f, s) => string.Compare(f.FileName, s.FileName, StringComparison.Ordinal),
                            f => f.FileName.GetHashCode()));
            }
            else
            {
                toAdd = addEditableFilesEvent.Files;
            }

            if (addEditableFilesEvent.ClearExisting && union.Count > 0)
                toAdd = union.UnionWOEqCheck(toAdd);

            if (addEditableFilesEvent.Files.Count > 0)
                StringFiles.AddRange(toAdd);

            TranslateWithSessionDict();
        }

        /// <summary>
        /// Отписывает текущий экземпляр от глобальных событий редактора
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            ManualEventManager.GetEvent<EditorScrollToStringAndSelectEvent>()
                .Unsubscribe(OnScrollToFileAndSelectString);

            ManualEventManager.GetEvent<EditorScrollToFileAndSelectEvent>()
                .Unsubscribe(OnScrollToFileAndSelect);

            ManualEventManager.GetEvent<EditorWindowTranslateTextEvent>()
                .Unsubscribe(OnTranslateText);

            ManualEventManager.GetEvent<AddEditableFilesEvent>()
                .Unsubscribe(OnAddEditableFiles);
        }

        #endregion

        #region События кнопок

        public void TranslateAllFilesClicked(object sender, RoutedEventArgs e)
        {
            TranslateAllFiles();
        }

        public void SaveClicked(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void SaveAndCloseClicked(object sender, RoutedEventArgs e)
        {
            Save(Close);
        }

        #endregion

        #region События элементов меню

        private void ShowSearchClick(object sender, RoutedEventArgs e)
        {
            WindowManager.ActivateWindow<EditorSearchWindow>();
        }

        private void ChooseDictionariesClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = true,
                AutoUpgradeEnabled = true,
                CheckPathExists = true,
                DefaultExt = ".xml",
                Filter = Res.DictionaryFiles + @" (*.xml)|*.xml",
                Multiselect = true,
                ShowHelp = false
            };

            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            foreach (var file in dialog.FileNames)
            {
                DictionaryFile dictFile;

                if (TryFunc(() => new DictionaryFile(file), out dictFile))
                    GlobalVariables.SourceDictionaries.Add(new CheckableString(file, true));
                else
                    MessBox.ShowDial(file + "\n" + Res.TheFileIsCorrupted, Res.ErrorLower);
            }
        }

        private void UseDictionariesClick(object sender, RoutedEventArgs e)
        {
            var dictsToUse = GlobalVariables.SourceDictionaries.Where(dict => dict.Item2 && File.Exists(dict.Item1)).ToArray();

            if (dictsToUse.Length == 0)
            {
                MessBox.ShowDial(Res.YouNeedToChooseDictionary, Res.ErrorLower);
                return;
            }

            int translated = 0;

            LoadingWindow.ShowWindow(() =>
            {
                IsEnabled = false;
                MainWindow.Disable();
            }, cts =>
            {
                var dictWords = 
                    UsefulFunctions.UnionWOEqCheck(dictsToUse.Select(d => new DictionaryFile(d.Item1).Details))
                        .DistinctByGrouping(f => f.OldText)
                        .ToDictionary(it => it.OldText, it => it.NewText, StringComparer.Ordinal);

                Parallel.ForEach(StringFiles, file =>
                {
                    if (file is DictionaryFile)
                        return;

                    int trans = 0;

                    foreach (OneString str in file.Details)
                    {
                        if (str.IsChanged)
                            continue;

                        string found;

                        if (dictWords.TryGetValue(str.OldText, out found))
                        {
                            str.NewText = found;
                            trans++;
                            break;
                        }
                    }

                    Interlocked.Add(ref translated, trans);
                });
            }, () =>
            {
                IsEnabled = true;
                MainWindow.Enable();
                MessBox.ShowDial(Res.StringsTranslated + " " + translated, Res.Finished);
            }, Visibility.Collapsed);
        }

        private void CurrentDictionariesDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.All : DragDropEffects.None;
            e.Handled = true;
        }

        private void CurrentDictionariesDragDrop(object sender, DragEventArgs e)
        {
            e.Handled = true;

            var files = e.Data.GetData(DataFormats.FileDrop) as string[];

            if (files == null)
                return;

            foreach (var file in files)
            {
                DictionaryFile dictFile;

                if (TryFunc(() => new DictionaryFile(file), out dictFile))
                    GlobalVariables.SourceDictionaries.Add(new CheckableString(file, true));
                else
                    MessBox.ShowDial(file + "\n" + Res.TheFileIsCorrupted, Res.ErrorLower);
            }
        }

        private void RemoveSourceDict_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CheckableString item = (sender as Grid)?.DataContext as CheckableString;

            if (item != null)
                GlobalVariables.SourceDictionaries.Remove(item);

            e.Handled = true;
        }

        private void RemoveSourceDict_MouseUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void ChooseSaveDictionaryClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = false,
                AutoUpgradeEnabled = true,
                CheckPathExists = true,
                DefaultExt = ".xml",
                Filter = Res.DictionaryFiles + @" (*.xml)|*.xml",
                Multiselect = false,
                ShowHelp = false,
                AddExtension = true
            };

            if (dialog.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                return;

            if (!File.Exists(dialog.FileName))
                File.Create(dialog.FileName).Close();

#if DEBUG
            SaveDictionary = new DictionaryFile(dialog.FileName);
#else
            try
            {
                SaveDictionary = new DictionaryFile(dialog.FileName);
            }
            catch (Exception)
            {
                MessBox.ShowDial(Res.TheFileIsCorrupted, Res.ErrorLower);
            }
#endif
        }

        private void RemoveCurrentSaveDictClick(object sender, RoutedEventArgs e)
        {
            SaveDictionary = null;
            SaveToDict = false;
        }

        private void SaveDictDragOver(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.All : DragDropEffects.None;
            e.Handled = true;
        }

        private void SaveDictDragDrop(object sender, DragEventArgs e)
        {
            e.Handled = true;

            var files = e.Data.GetData(DataFormats.FileDrop) as string[];

            if (files == null || files.Length == 0)
                return;

            try
            {
                SaveDictionary = new DictionaryFile(files[0]);
            }
            catch (Exception)
            {
                MessBox.ShowDial(Res.TheFileIsCorrupted, Res.ErrorLower);
            }
        }

        #endregion

        #region Контекстное меню

        private void GridContextMenuOpened(object sender, RoutedEventArgs e)
        {
            MenuItem createItem(string header, RoutedEventHandler clicked)
            {
                var item = new MenuItem { Header = header };
                item.Click += clicked;
                return item;
            }

            var menu = sender.As<ContextMenu>();

            // ReSharper disable once PossibleNullReferenceException
            menu.Items.Clear();

            menu.Items.Add(createItem(Res.Expand, Expand_Click));
            menu.Items.Add(createItem(Res.Collapse, Collapse_Click));

            EditableFile selectedFile = GetSelectedFile();

            if (selectedFile != null)
            {
                var items = new Dictionary<string, RoutedEventHandler>
                {
                    { Res.ShowInExplorer, (o, args) => ShowInExplorer(selectedFile.FileName)},
                    { Res.FullFilePathToClipboard, (o, args) => Clipboard.SetText(selectedFile.FileName) },
                    { Res.FileNameToClipboard, (o, args) => Clipboard.SetText(Path.GetFileName(selectedFile.FileName)) },
                    { Res.DirectoryPathToClipboard, (o, args) => Clipboard.SetText(Path.GetDirectoryName(selectedFile.FileName)) },
                    { Res.Delete, (o, args) => StringFiles.Remove(selectedFile) }
            };

                items.ForEach(it => menu.Items.Add(createItem(it.Key, it.Value)));
            }

            RowColumnIndex rowColumn;
            OneString selectedString = GetSelectedString(out rowColumn);

            if (selectedString != null)
            {
                menu.Items.Add(createItem(Res.Copy, (o, args) =>
                {
                    switch (rowColumn.ColumnIndex)
                    {
                        case 1:
                            Clipboard.SetText(selectedString.Name ?? "");
                            break;
                        case 2:
                            Clipboard.SetText(selectedString.OldText ?? "");
                            break;
                        case 3:
                            Clipboard.SetText(selectedString.NewText ?? "");
                            break;
                    }
                }));

                if (rowColumn.ColumnIndex == 2 && !selectedString.IsOldTextReadOnly)
                {
                    menu.Items.Add(createItem(Res.Paste, (o, args) =>
                    {
                        string text = Clipboard.GetText();

                        if (!text.NE())
                            selectedString.OldText = text;
                    }));
                }

                if (rowColumn.ColumnIndex == 3 && !selectedString.IsNewTextReadOnly)
                {
                    menu.Items.Add(createItem(Res.Paste, (o, args) =>
                    {
                        string text = Clipboard.GetText();

                        if (!text.NE())
                        {
                            selectedString.NewText = text;

                            if (!(selectedString is OneDictionaryString))
                            {
                                AddToSessionDict(selectedString.OldText, selectedString.NewText);

                                TranslateWithSessionDictIfNeeded(selectedString.OldText, selectedString.NewText);
                            }
                        }
                    }));
                }
            }
        }

        private void Expand_Click(object sender, RoutedEventArgs e)
        {
            EditorGrid.ExpandAllDetailsView();
        }

        private void Collapse_Click(object sender, RoutedEventArgs e)
        {
            EditorGrid.CollapseAllDetailsView();
        }

        #endregion

        #region События EditorGrid и связанные с этим вещи

        private void InitGridEvents()
        {
            void collapseAll() => EditorGrid.CollapseAllDetailsView();

            void expandAll() => EditorGrid.ExpandAllDetailsView();

            void enterAction()
            {
                var str = GetSelectedString();

                if (str != null)
                {
                    new StringEditorWindow(str).ShowDialog();
                }
                else
                {
                    int row;

                    EditableFile file = GetSelectedFile(out row);

                    if (file != null)
                    {
                        var expanded = GetFileSelectedEntry().IsExpanded;

                        if (expanded)
                            EditorGrid.CollapseDetailsViewAt(row);
                        else
                            EditorGrid.ExpandDetailsViewAt(row);
                    }
                }
            };

            void copyToNew()
            {
                var str = GetSelectedString();

                if (str != null)
                    str.NewText = str.OldText;
            };

            void copyToClipboard()
            {
                RowColumnIndex position;

                var str = GetSelectedString(out position);

                if (str == null)
                {
                    var file = GetSelectedFile();

                    if (file != null)
                        Dispatcher.InvokeAction(() => Clipboard.SetText(file.FileName));

                    return;
                }

                Dispatcher.InvokeAction(() =>
                {
                    switch (position.ColumnIndex)
                    {
                        case 1:
                            Clipboard.SetText(str.Name ?? "");
                            break;
                        case 2:
                            Clipboard.SetText(str.OldText ?? "");
                            break;
                        case 3:
                            Clipboard.SetText(str.NewText ?? "");
                            break;
                    }
                });
            };

            void pasteFromClipboard()
            {
                RowColumnIndex position;

                var str = GetSelectedString(out position);

                if (str == null)
                    return;

                Dispatcher.InvokeAction(() =>
                {
                    switch (position.ColumnIndex)
                    {
                        case 2:
                            if (!str.IsOldTextReadOnly)
                            {
                                string text = Clipboard.GetText();

                                if (!text.NE())
                                    str.OldText = text;
                            }
                            break;
                        case 3:
                            if (!str.IsNewTextReadOnly)
                            {
                                string text = Clipboard.GetText();

                                if (!text.NE())
                                {
                                    str.NewText = text;

                                    if (!(str is OneDictionaryString))
                                    {
                                        AddToSessionDict(str.OldText, str.NewText);

                                        TranslateWithSessionDictIfNeeded(str.OldText, str.NewText);
                                    }
                                }
                            }
                            break;
                    }
                });
            };

            void moveToHead()
            {
                int? row = null;

                if (GetSelectedString() != null)
                    row = EditorGrid.ResolveToRowIndex(EditorGrid.SelectedIndex);
                if (GetSelectedFile() != null && EditorGrid.SelectedIndex > 0)
                    row = EditorGrid.ResolveToRowIndex(EditorGrid.SelectedIndex - 1);

                if (row.HasValue && row.Value >= 0)
                {
                    EditorGrid.ScrollInView(new RowColumnIndex(row.Value, 0));
                    EditorGrid.SelectCell(EditorGrid.GetRecordAtRowIndex(row.Value), EditorGrid.Columns[0]);
                }
            };

            void moveToNext()
            {
                int? row = null;

                if (EditorGrid.View.Records.Count > EditorGrid.SelectedIndex + 1 &&
                    (GetSelectedString() != null || GetSelectedFile() != null))
                {
                    row = EditorGrid.ResolveToRowIndex(EditorGrid.SelectedIndex + 1);
                }

                if (row.HasValue)
                {
                    EditorGrid.ScrollInView(new RowColumnIndex(row.Value, 0));
                    EditorGrid.SelectedIndex++;
                }

            };

            void addEmpty()
            {
                if (StringFiles.Count == 0)
                    return;

                DictionaryFile dict;

                if (StringFiles.Count > 1)
                    dict = EditorGrid.CurrentItem as DictionaryFile;
                else
                    dict = StringFiles[0] as DictionaryFile;

                dict?.Add("", "");
            };

            void removeRow()
            {
                var str = GetSelectedString() as OneDictionaryString;

                if (str == null)
                {
                    var file = GetSelectedFile(out var rowColumnIndex);

                    if (file != null)
                    {
                        StringFiles.Remove(file);
                        EditorGrid.SelectCell(EditorGrid.GetRecordAtRowIndex(EditorGrid.ResolveToRowIndex(rowColumnIndex)), EditorGrid.Columns[0]);
                    }

                    return;
                }

                var dict = EditorGrid.CurrentItem as DictionaryFile;

                dict?.Remove(str);
            };

            // Свернуть всё
            AddGridKeyEvent(Key.C, collapseAll, Ctrls, Shifts);

            // Развернуть всё
            AddGridKeyEvent(Key.E, expandAll, Ctrls, Shifts);

            // Скопировать текущую ячейку в буфер
            AddGridKeyEvent(Key.C, copyToClipboard, Ctrls);

            // Вставить текст из буфера
            AddGridKeyEvent(Key.V, pasteFromClipboard, Ctrls);

            // Обработать нажатие Enter (развернуть/свернуть строки файла / открыть окно редактора текущей строки)
            AddGridKeyEvent(Key.Enter, enterAction);

            // Скопировать старый текст в новый
            AddGridKeyEvent(Key.Right, copyToNew, Ctrls, Shifts);

            // Перевести выбранную строку/файл
            AddGridKeyEvent(Key.Right, TranslateSelected, Ctrls);

            // Выделить файл, содержащий выделенную строку
            AddGridKeyEvent(Key.Up, moveToHead, Ctrls, Shifts);

            // Перейти к следующему файлу
            AddGridKeyEvent(Key.Down, moveToNext, Ctrls, Shifts);

            // Добавить пустую строку в словарь
            AddGridKeyEvent(Key.A, addEmpty, Ctrls);

            // Удалить строку из словаря
            AddGridKeyEvent(Key.Delete, removeRow);
        }

        private readonly List<Tuple<Key, Key[][], Action>> EditorGridKeyEvents = new List<Tuple<Key, Key[][], Action>>();

        private void AddGridKeyEvent(Key key, Action action, params Key[][] modifers)
        {
            EditorGridKeyEvents.Add(new Tuple<Key, Key[][], Action>(key, modifers, action));
        }

        private static readonly Key[] Ctrls = { Key.LeftCtrl, Key.RightCtrl };
        private static readonly Key[] Shifts = { Key.LeftShift, Key.RightShift };

        private void EditorGrid_OnDragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
                e.Effects = DragDropEffects.Copy;

            e.Handled = true;
        }

        private void EditorGrid_OnDrop(object sender, DragEventArgs e)
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];

            if (files == null)
                return;

            string firstAdded = null;

            var alreadyAddedFiles = new List<string>();

            foreach (string file in files)
            {
                if (StringFiles.Any(f => f.FileName == file) || GlobalVariables.SourceDictionaries.Any(d => d.Item1 == file) || SaveDictionary?.FileName == file)
                {
                    if (firstAdded == null)
                        firstAdded = file;

                    alreadyAddedFiles.Add(file);
                    continue;
                }

                EditableFile current = GetSuitableEditableFile(file);

                if (firstAdded == null && current != null)
                    firstAdded = current.FileName;

                StringFiles.AddIfNotNull(current);
            }

            if (alreadyAddedFiles.Count > 0)
            {
                MessBox.ShowDial($"{Res.TheFollowingFilesHaveBeenAlreadyAdded}: {Environment.NewLine}{alreadyAddedFiles.Select(it => "  - " + it).JoinStr(Environment.NewLine)}", Res.ErrorLower);
            }

            if (firstAdded != null)
            {
                ManualEventManager.GetEvent<EditorScrollToFileAndSelectEvent>()
                    .Publish(new EditorScrollToFileAndSelectEvent(f => f.FileName.Equals(firstAdded, StringComparison.Ordinal),
                        files.Length == 1));
            }
        }

        private bool EditorGrid_OnKeyDown(KeyEventArgs e)
        {
            Func<Key[][], bool> keysDown = keys => keys.All(ks => ks.Any(e.KeyboardDevice.IsKeyDown));

            foreach (var ev in EditorGridKeyEvents)
            {
                if (e.Key == ev.Item1 && keysDown(ev.Item2))
                {
                    ev.Item3();
                    e.Handled = true;
                    return true;
                }
            }

            return false;
        }

        private void EditorGrid_OnTextInput(object sender, TextCompositionEventArgs e)
        {
            if (SettingsIncapsuler.AlternativeEditingKeys)
            {
                OneString current = GetSelectedString();

                if (current == null)
                    return;

                new StringEditorWindow(current, true, e.Text).ShowDialog();
            }
        }

        private void SfDataGrid_OnCellDoubleTapped(object sender, GridCellDoubleTappedEventArgs args)
        {
            switch (args.Record)
            {
                case OneString str:
                    new StringEditorWindow(str).ShowDialog();
                    break;
                case EditableFile file:
                    GetSelectedFile(out var rowIndex);

                    if (GetFileSelectedEntry().IsExpanded)
                        EditorGrid.CollapseDetailsViewAt(rowIndex);
                    else
                        EditorGrid.ExpandDetailsViewAt(rowIndex);
                    break;
            }
        }

        #endregion

        #region GetSelections

        private OneString GetSelectedString()
        {
            var result = EditorGrid.SelectedDetailsViewGrid?.SelectedItem as OneString;

            return result;
        }

        private OneString GetSelectedString(out RowColumnIndex rowColumn)
        {
            rowColumn = RowColumnIndex.Empty;

            var result = EditorGrid.SelectedDetailsViewGrid?.SelectedItem as OneString;

            if (result == null)
                return null;

            rowColumn = EditorGrid.SelectedDetailsViewGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex;

            return result;
        }

        private EditableFile GetSelectedFile()
        {
            return EditorGrid.SelectedItem as EditableFile;
        }

        private EditableFile GetSelectedFile(out int recordRowIndex)
        {
            recordRowIndex = -1;

            var result = EditorGrid.SelectedItem as EditableFile;

            if (result == null)
                return null;

            recordRowIndex = EditorGrid.SelectedIndex;

            return result;
        }

        private RecordEntry GetFileSelectedEntry()
        {
            return EditorGrid.SelectionController.SelectedCells.FirstOrDefault()?.NodeEntry.As<RecordEntry>();
        }

        #endregion

        /// <summary>
        /// Переводит все файлы в редакторе с помоью онлайн переводчика
        /// </summary>
        private void TranslateAllFiles()
        {
            if (!TryAction(() => DownloadString("http://www.ya.ru")))
            {
                MessBox.ShowDial(Res.NoInternetCantTranslate, Res.ErrorLower);
                return;
            }

            string errorMessage = null;
            var toTranslate = new ConcurrentQueue<KeyValuePair<OneString, string>>();

            void StartActions()
            {
                Disable();
                MainWindow.Disable();
            }

            void ThreadActions(CancellationTokenSource token)
            {
                int max = StringFiles.Sum(file => file.Details.Count);
                LoadingProcessWindow.Instance.ProcessMax = max;
                var cantTranslate = 0;
                var list = StringFiles.SelectMany(file => file.Details);

                max = Math.Min(max, 4);

                Parallel.ForEach(list, str =>
                {
                    if (token.IsCancellationRequested) return;
                    if (!string.IsNullOrEmpty(str.NewText))
                    {
                        LoadingProcessWindow.Instance.ProcessValue++;
                        return;
                    }
                    try
                    {
                        LoadingProcessWindow.Instance.ProcessValue++;

                        string translated = GlobalVariables.CurrentTranslationService.Translate(str.OldText,
                            SettingsIncapsuler.TargetLanguage);

                        toTranslate.Enqueue(new KeyValuePair<OneString, string>(str, translated));

                        cantTranslate = 0;
                    }
                    catch (Exception ex)
                    {
                        Interlocked.Add(ref cantTranslate, 1);
                        if (!token.IsCancellationRequested && cantTranslate >= max)
                        {
                            token.Cancel();
                            errorMessage = ex.Message;
                        }
                    }
                });
            }

            void FinishActions()
            {
                foreach (var vals in toTranslate)
                    vals.Key.NewText = vals.Value;

                if (errorMessage != null)
                    MessBox.ShowDial(Res.CantTranslate + "\n" + errorMessage, Res.ErrorLower);

                Enable();
                MainWindow.Enable();
                EditorGrid.Focus();
            }

            LoadingProcessWindow.ShowWindow(StartActions, ThreadActions, FinishActions);
        }

        /// <summary>
        /// Применяет изменения в файлах
        /// </summary>
        /// <param name="onFinished">Действие после обработки</param>
        private void Save(Action onFinished = null)
        {
            void SavingProcess(CancellationTokenSource cts)
            {
                bool saveToDict = SaveToDict && SaveDictionary != null;
                DictionaryFile dict = SaveDictionary;

                foreach (var file in StringFiles)
                {
                    LoadingProcessWindow.Instance.ProcessValue++;

                    if (file.IsChanged)
                    {
                        if (saveToDict)
                        {
                            dict.AddChangedStringsFromFile(file);
                        }

                        file.SaveChanges();
                    }
                }

                if (saveToDict)
                {
                    dict.SaveChanges();
                }
            }

            LoadingProcessWindow.ShowWindow(Disable, SavingProcess, () =>
            {
                Enable();
                onFinished?.Invoke();
            },
            Visibility.Collapsed, StringFiles.Count);
        }

        /// <summary>
        /// Переводит все строки с помощью словаря сессии, если соответствующая настройка активна
        /// </summary>
        private void TranslateWithSessionDict()
        {
            void TranslateWithSessionDict(EditableFile file)
            {
                if (file == null || file is DictionaryFile)
                    return;

                foreach (var str in file.Details)
                {
                    string found;
                    if (str.NewText.NE() && GlobalVariables.SessionDictionary.TryGetValue(str.OldText, out found))
                        str.NewText = found;
                }
            }

            if (!SettingsIncapsuler.SessionAutoTranslate)
                return;

            StringFiles.ForEach(TranslateWithSessionDict);
        }

        /// <summary>
        /// Переводит выбранные строки с помощью онлайн переводчика
        /// </summary>
        private void TranslateSelected()
        {
            List<OneString> rows = null;

            var it = GetSelectedString();

            if (it == null)
            {
                var file = GetSelectedFile();

                if (file != null)
                {
                    rows = new List<OneString>(file.Details);
                }
            }
            else
            {
                rows = new List<OneString> { it };
            }

            if (rows == null)
                return;

            string errorMessage = null;
            var translated = new ConcurrentQueue<KeyValuePair<OneString, string>>();

            void ThreadActions(CancellationTokenSource token)
            {
                int cantTranslate = 0;

                int max = Math.Min(4, rows.Count);

                Parallel.ForEach(rows, str =>
                {
                    if (token.IsCancellationRequested)
                        return;

                    try
                    {
                        LoadingProcessWindow.Instance.ProcessValue++;
                        translated.Enqueue(new KeyValuePair<OneString, string>(str, TranslateTextWithSettings(str.OldText)));

                        cantTranslate = 0;
                    }
                    catch (Exception ex)
                    {
                        if (++cantTranslate >= max)
                        {
                            token.Cancel();
                            errorMessage = ex.Message;
                        }
                    }
                });
            }

            void FinishActions()
            {
                foreach (var str in translated)
                    str.Key.NewText = str.Value;

                if (errorMessage != null)
                    MessBox.ShowDial(Res.CantTranslate + "\n" + errorMessage, Res.ErrorLower);

                Enable();
                EditorGrid.Focus();
            }

            LoadingProcessWindow.ShowWindow(Disable, ThreadActions, FinishActions, progressMax: rows.Count);
        }

        private static void Enable()
        {
            instance?.Dispatcher.InvokeAction(() => instance.IsEnabled = true);
        }

        private static void Disable()
        {
            instance?.Dispatcher.InvokeAction(() => instance.IsEnabled = false);
        }

        private bool CheckIfNeedToSave()
        {
            if (StringFiles?.Any(f => f.IsChanged) == true)
            {
                string result = MessBox.ShowDial(Res.ApplyTheChanges, "", MessBox.MessageButtons.Yes,
                    MessBox.MessageButtons.No, MessBox.MessageButtons.Cancel);

                if (result == MessBox.MessageButtons.Yes)
                {
                    Save();
                }
                else if (result == MessBox.MessageButtons.Cancel)
                {
                    return false;
                }
            }

            return true;
        }

        private void TranslateWithSessionDictIfNeeded(string oldText, string newText)
        {
            if (SettingsIncapsuler.SessionAutoTranslate)
            {
                ManualEventManager.GetEvent<EditorWindowTranslateTextEvent>()
                    .Publish(new EditorWindowTranslateTextEvent(oldText, newText,
                        EditorWindowTranslateTextEvent.NotDictionaryFileFilter));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}