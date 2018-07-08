using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using AndroidTranslator;
using AndroidTranslator.Classes.Files;
using AndroidTranslator.Interfaces.Files;
using AndroidTranslator.Interfaces.Strings;
using Microsoft.WindowsAPICodePack.Dialogs;
using MVVM_Tools.Code.Commands;
using Syncfusion.Data;
using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.Grid.Helpers;
using Syncfusion.UI.Xaml.ScrollAxis;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.EventManagerLogic;
using TranslatorApk.Logic.Events;
using TranslatorApk.Logic.Interfaces;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.Utils;
using TranslatorApk.Logic.WebServices;
using TranslatorApk.Resources.Localizations;

using Clipboard = System.Windows.Clipboard;
using ContextMenu = System.Windows.Controls.ContextMenu;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MenuItem = System.Windows.Controls.MenuItem;

namespace TranslatorApk.Windows
{
    public partial class EditorWindow : IRaisePropertyChanged
    {
        public ICommand TranslateAllFilesCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand SaveAndCloseCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ClearSourceDictionariesListCommand { get; }

        public bool SaveToDict
        {
            get => GlobalVariables.AppSettings.EditorWindowSaveToDict;
            set
            {
                if (SaveDictionary == null && value)
                    return;

                GlobalVariables.AppSettings.EditorWindowSaveToDict = value;
                RaisePropertyChanged(nameof(SaveToDict));
            }
        }

        public WindowState EditorWindowState
        {
            get
            {
                if (_isMinimized)
                    return WindowState.Minimized;

                return GlobalVariables.AppSettings.EditorWMaximized ? WindowState.Maximized : WindowState.Normal;
            }
            set
            {
                if (value != WindowState.Minimized)
                    GlobalVariables.AppSettings.EditorWMaximized = value == WindowState.Maximized;

                _isMinimized = value == WindowState.Minimized;

                RaisePropertyChanged(nameof(EditorWindowState));
            }
        }
        private bool _isMinimized;

        /// <summary>
        /// Список редактируемых файлов
        /// </summary>
        public ObservableRangeCollection<IEditableFile> StringFiles { get; } = new ObservableRangeCollection<IEditableFile>();

        public IDictionaryFile SaveDictionary
        {
            get => _saveDictionary;
            set
            {
                if (this.SetProperty(ref _saveDictionary, value))
                    GlobalVariables.AppSettings.TargetDictionary = value?.FileName;
            }
        }
        private IDictionaryFile _saveDictionary;

        public int LangsBoxItemIndex
        {
            get => TranslateService.ShortTargetLanguages.IndexOf(GlobalVariables.AppSettings.TargetLanguage);
            set
            {
                GlobalVariables.AppSettings.TargetLanguage = TranslateService.GetShortTL(LangsBox.Items[value] as string);
                RaisePropertyChanged(nameof(LangsBoxItemIndex));
            }
        }

        private readonly QueueEventManager _queueEventManager = new QueueEventManager();

        private readonly object _lock = new object();

        public EditorWindow()
        {
            TranslateAllFilesCommand = new ActionCommand(TranslateAllFiles);
            SaveCommand = new ActionCommand(() => Save());
            SaveAndCloseCommand = new ActionCommand(() => Save(Close));
            SearchCommand = new ActionCommand(() => WindowManager.ActivateWindow<EditorSearchWindow>());
            ClearSourceDictionariesListCommand = new ActionCommand(GlobalVariables.SourceDictionaries.Clear);

            SubscribeToEvents();

            InitializeComponent();

            InitGridEvents();

            EditorGrid.SelectionController = new GridSelectionControllerExt(EditorGrid, EditorGrid_OnKeyDown);

            string settingsTargetDict = GlobalVariables.AppSettings.TargetDictionary;

            if (!settingsTargetDict.IsNullOrEmpty() && File.Exists(settingsTargetDict))
                SaveDictionary = new DictionaryFile(settingsTargetDict);
        }

        public (IOneString str, IEditableFile container) GetPreviousString(IOneString currentString)
        {
            var records = EditorGrid.View.Records;

            // searching for the string in files
            var (recordIndex, record) = records.FindWithIndex(_ => _.Data.As<IEditableFile>().Details.Contains(currentString));

            if (recordIndex == -1)
                return (null, null);

            var editableFile = record.Data.As<IEditableFile>();

            // ordering strings in the file according to the sorting descriptions
            var orderedStrings = OrderFileStrings(editableFile).ToList();

            int strIndex = orderedStrings.IndexOf(currentString);

            strIndex--;

            // if the previous string is in the current file then return current
            if (strIndex >= 0)
                return (orderedStrings[strIndex], editableFile);

            // else try to find in the previous files
            for (int i = recordIndex - 1; i >= 0; i--)
            {
                var file = records[i].Data.As<IEditableFile>();

                if (file.Details.Count > 0)
                    return (OrderFileStrings(file).Last(), file);
            }

            // failed
            return (null, null);
        }

        public (IOneString str, IEditableFile container) GetNextString(IOneString currentString)
        {
            var records = EditorGrid.View.Records;

            // searching for the string in files
            var (recordIndex, record) = records.FindWithIndex(_ => _.Data.As<IEditableFile>().Details.Contains(currentString));

            if (recordIndex == -1)
                return (null, null);

            var editableFile = record.Data.As<IEditableFile>();

            // ordering strings in the file according to the sorting descriptions
            var orderedStrings = OrderFileStrings(editableFile).ToList();

            int strIndex = orderedStrings.IndexOf(currentString);

            strIndex++;

            // if the next string is in the current file then return current
            if (strIndex < orderedStrings.Count)
                return (orderedStrings[strIndex], editableFile);

            // else try to find in the following files
            for (int i = recordIndex + 1; i < records.Count; i++)
            {
                var file = records[i].Data.As<IEditableFile>();

                if (file.Details.Count > 0)
                    return (OrderFileStrings(file).First(), file);
            }

            // failed
            return (null, null);
        }

        /// <summary>
        /// Orders file's strings according to the sorting descriptions
        /// </summary>
        /// <param name="file">File with strings</param>
        private IEnumerable<IOneString> OrderFileStrings(IEditableFile file)
        {
            var sortDescriptions = EditorGrid.DetailsViewDefinition[0].As<GridViewDefinition>().DataGrid.SortColumnDescriptions.Cast<SortColumnDescription>();

            return Utils.SortWithDescriptions(file.Details, sortDescriptions);
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

            UnsubscribeFromEvents();

            StringFiles.Clear();

            WindowManager.CloseWindow<EditorSearchWindow>();

            GC.Collect();
        }

        #endregion

        #region Глобальные события

        /// <summary>
        /// Подписывает текущий экземпляр на глобальные события редактора
        /// </summary>
        private void SubscribeToEvents()
        {
            _queueEventManager.AddEvent<EditorScrollToStringAndSelectEvent>(OnScrollToFileAndSelectString);
            _queueEventManager.AddEvent<EditorScrollToFileAndSelectEvent>(OnScrollToFileAndSelect);
            _queueEventManager.AddEvent<EditorWindowTranslateTextEvent>(OnTranslateText);
            _queueEventManager.AddEvent<AddEditableFilesEvent>(OnAddEditableFiles);
        }

        /// <summary>
        /// Отписывает текущий экземпляр от глобальных событий редактора
        /// </summary>
        private void UnsubscribeFromEvents()
        {
            _queueEventManager.RemoveEvent<EditorScrollToStringAndSelectEvent>();
            _queueEventManager.RemoveEvent<EditorScrollToFileAndSelectEvent>();
            _queueEventManager.RemoveEvent<EditorWindowTranslateTextEvent>();
            _queueEventManager.RemoveEvent<AddEditableFilesEvent>();
        }

        private void OnScrollToFileAndSelectString(EditorScrollToStringAndSelectEvent parameter)
        {
            EditorGrid.ScrollToFileAndSelectString(parameter.FilePredicate, parameter.StringPredicate);
        }

        private void OnScrollToFileAndSelect(EditorScrollToFileAndSelectEvent editorScrollToFileAndSelectEvent)
        {
            EditorGrid.ScrollToFileAndSelect(editorScrollToFileAndSelectEvent.FilePredicate, editorScrollToFileAndSelectEvent.ExpandRecord);
        }

        private void OnTranslateText(EditorWindowTranslateTextEvent args)
        {
            if (args.OldText.IsNullOrEmpty() || args.NewText.IsNullOrEmpty())
                return;
            
            foreach (var file in StringFiles.Where(args.Filter))
            {
                foreach (var str in file.Details)
                {
                    if (str.OldText.Equals(args.OldText, StringComparison.Ordinal) && str.NewText.IsNullOrEmpty())
                        str.NewText = args.NewText;
                }
            }
        }

        private void OnAddEditableFiles(AddEditableFilesEvent args)
        {
            if (args.Files == null)
                return;

            var union = StringFiles.Join(args.Files, f => f.FileName, f => f.FileName, (f, _) => f, StringComparer.Ordinal).ToList();

            if (args.ClearExisting)
                StringFiles.Clear();

            IEnumerable<IEditableFile> toAdd;

            if (union.Count > 0)
            {
                toAdd =
                    args.Files.Except(union,
                        new ComparisonWrapper<IEditableFile>(
                            (f, s) => string.Compare(f.FileName, s.FileName, StringComparison.Ordinal),
                            f => f.FileName.GetHashCode()));
            }
            else
            {
                toAdd = args.Files;
            }

            if (args.ClearExisting && union.Count > 0)
                toAdd = union.Concat(toAdd);

            if (args.Files.Count > 0)
                StringFiles.AddRange(toAdd);

            TranslateWithSessionDict();
        }

        #endregion

        #region События элементов меню

        private void ShowSearchClick(object sender, RoutedEventArgs e)
        {
            WindowManager.ActivateWindow<EditorSearchWindow>();
        }

        private void ChooseDictionariesClick(object sender, RoutedEventArgs e)
        {
            var dialog = new CommonOpenFileDialog
            {
                EnsureFileExists = true,
                EnsurePathExists = true,
                DefaultExtension = ".xml",
                Filters = { new CommonFileDialogFilter(StringResources.DictionaryFiles + @" (*.xml)", "*.xml") },
                Multiselect = true
            };

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
                return;

            dialog.FileNames.ForEach(AddSourceDictIfNotAdded);
        }

        private void UseDictionariesClick(object sender, RoutedEventArgs e)
        {
            var dictsToUse = GlobalVariables.SourceDictionaries.Where(dict => dict.IsChecked && File.Exists(dict.Text)).ToArray();

            if (dictsToUse.Length == 0)
            {
                MessBox.ShowDial(StringResources.YouNeedToChooseDictionary, StringResources.ErrorLower);
                return;
            }

            int translated = 0;

            LoadingWindow.ShowWindow(
                beforeStarting: () =>
                {
                    IsEnabled = false;
                    WindowManager.DisableWindow<MainWindow>();
                },
                threadActions: cts =>
                {
                    var dictWords = 
                        dictsToUse.SelectMany(d => new DictionaryFile(d.Text).Details)
                            .DistinctBy(it => it.OldText)
                            .ToDictionary(it => it.OldText, it => it.NewText, StringComparer.Ordinal);

                    Parallel.ForEach(StringFiles, file =>
                    {
                        if (file is DictionaryFile)
                            return;

                        int trans = 0;

                        foreach (IOneString str in file.Details)
                        {
                            if (str.IsChanged)
                                continue;

                            string found;

                            if (dictWords.TryGetValue(str.OldText, out found))
                            {
                                str.NewText = found;
                                trans++;
                            }
                        }

                        Interlocked.Add(ref translated, trans);
                    });
                },
                finishActions: () =>
                {
                    IsEnabled = true;
                    WindowManager.EnableWindow<MainWindow>();
                    MessBox.ShowDial(StringResources.StringsTranslated + " " + translated, StringResources.Finished);
                },
                cancelVisibility: Visibility.Collapsed,
                ownerWindow: this
            );
        }

        private void CurrentDictionariesDragEnter(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.All : DragDropEffects.None;
            e.Handled = true;
        }

        private void CurrentDictionariesDragDrop(object sender, DragEventArgs e)
        {
            e.Handled = true;

            string[] files = e.GetFilesDrop();

            files?.ForEach(AddSourceDictIfNotAdded);
        }

        private void AddSourceDictIfNotAdded(string dictionaryFile)
        {
            if (GlobalVariables.SourceDictionaries.Any(dict => dict.Text == dictionaryFile))
                return;

            try
            {
                // ReSharper disable once ObjectCreationAsStatement
                new DictionaryFile(dictionaryFile);
                GlobalVariables.SourceDictionaries.Add(new CheckableSetting(dictionaryFile, true));
            }
            catch (Exception)
            {
                MessBox.ShowDial(dictionaryFile + "\n" + StringResources.TheFileIsCorrupted, StringResources.ErrorLower);
            }
        }

        private void RemoveSourceDict_MouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = (sender as Grid)?.DataContext as CheckableSetting;

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
            var dialog = new CommonOpenFileDialog
            {
                EnsureFileExists = false,
                EnsurePathExists = true,
                DefaultExtension = ".xml",
                Filters = { new CommonFileDialogFilter(StringResources.DictionaryFiles + @" (*.xml)", "*.xml") },
                Multiselect = false
            };

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
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
                MessBox.ShowDial(StringResources.TheFileIsCorrupted, StringResources.ErrorLower);
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

            string[] files = e.GetFilesDrop();

            if (files == null || files.Length == 0)
                return;

            try
            {
                SaveDictionary = new DictionaryFile(files[0]);
            }
            catch (Exception)
            {
                MessBox.ShowDial(StringResources.TheFileIsCorrupted, StringResources.ErrorLower);
            }
        }

        #endregion

        #region Контекстное меню

        private void EditorGrid_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            MenuItem CreateItem(string header, RoutedEventHandler clicked, string gesture = null)
            {
                var item = new MenuItem
                {
                    Header = header,
                    InputGestureText = gesture,
                };
                item.Click += clicked;
                return item;
            }

            var menu = new ContextMenu
            {
                Items =
                {
                    CreateItem(StringResources.Expand, Expand_Click),
                    CreateItem(StringResources.Collapse, Collapse_Click)
                },
                FontSize = GlobalVariables.AppSettings.FontSize
            };

            void Add(string header, RoutedEventHandler clicked, string gesture = null)
            {
                menu.Items.Add(CreateItem(header, clicked, gesture));
            }

            IEditableFile selectedFile = GetSelectedFile();

            if (selectedFile != null)
            {
                var items = new List<(string header, RoutedEventHandler handler, string gesture)>
                {
                    ( StringResources.ShowInExplorer, (o, args) => Utils.ShowInExplorer(selectedFile.FileName), default ),
                    ( StringResources.FullFilePathToClipboard, (o, args) => Clipboard.SetText(selectedFile.FileName), default ),
                    ( StringResources.FileNameToClipboard, (o, args) => Clipboard.SetText(Path.GetFileName(selectedFile.FileName) ?? string.Empty), default ),
                    ( StringResources.DirectoryPathToClipboard, (o, args) => Clipboard.SetText(Path.GetDirectoryName(selectedFile.FileName) ?? string.Empty), default ),
                    ( StringResources.Delete, (o, args) => StringFiles.Remove(selectedFile), "Delete" )
                };

                items.ForEach(it => Add(it.header, it.handler, it.gesture));
            }

            IOneString selectedString = GetSelectedString(out RowColumnIndex rowColumn);

            if (selectedString != null)
            {
                Add(StringResources.Copy, (o, args) =>
                {
                    switch (rowColumn.ColumnIndex)
                    {
                        case 1:
                            Clipboard.SetText(selectedString.Name ?? string.Empty);
                            break;
                        case 2:
                            Clipboard.SetText(selectedString.OldText ?? string.Empty);
                            break;
                        case 3:
                            Clipboard.SetText(selectedString.NewText ?? string.Empty);
                            break;
                    }
                });

                if (rowColumn.ColumnIndex == 2 && !selectedString.IsOldTextReadOnly)
                {
                    Add(StringResources.Paste, (o, args) =>
                    {
                        string text = Clipboard.GetText();

                        if (!text.IsNullOrEmpty())
                            selectedString.OldText = text;
                    });
                    Add(StringResources.Clear, (o, args) => selectedString.OldText = string.Empty);
                }

                if (rowColumn.ColumnIndex == 3 && !selectedString.IsNewTextReadOnly)
                {
                    Add(StringResources.Paste, (o, args) =>
                    {
                        string text = Clipboard.GetText();

                        if (!text.IsNullOrEmpty())
                        {
                            selectedString.NewText = text;

                            if (!(selectedString is OneDictionaryString))
                            {
                                Utils.AddToSessionDict(selectedString.OldText, selectedString.NewText);

                                TranslateWithSessionDictIfNeeded(selectedString.OldText, selectedString.NewText);
                            }
                        }
                    });
                    Add(StringResources.Clear, (o, args) => selectedString.NewText = string.Empty);
                }
            }

            menu.IsOpen = true;
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
            void CollapseAll() => EditorGrid.CollapseAllDetailsView();

            void ExpandAll() => EditorGrid.ExpandAllDetailsView();

            void EnterAction()
            {
                var str = GetSelectedString();

                if (str != null)
                {
                    WindowManager.ActivateWindow<StringEditorWindow>();

                    ManualEventManager
                        .GetEvent<EditStringEvent>()
                        .Publish(new EditStringEvent(str, GetStringFile(str)));
                }
                else
                {
                    int row;

                    IEditableFile file = GetSelectedFile(out row);

                    if (file != null)
                    {
                        var expanded = GetFileSelectedEntry().IsExpanded;

                        if (expanded)
                            EditorGrid.CollapseDetailsViewAt(row);
                        else
                            EditorGrid.ExpandDetailsViewAt(row);
                    }
                }
            }

            void CopyToNew()
            {
                var str = GetSelectedString();

                if (str != null)
                    str.NewText = str.OldText;
            }

            void TranslateSelectedWithDictionary()
            {
                var str = GetSelectedString();

                if (str != null)
                {
                    var dictsToUse = 
                        GlobalVariables.SourceDictionaries
                            .Where(dict => dict.IsChecked && File.Exists(dict.Text))
                            .ToList();

                    if (dictsToUse.Count == 0)
                    {
                        MessBox.ShowDial(StringResources.YouNeedToChooseDictionary, StringResources.ErrorLower);
                        return;
                    }

                    var dictWords =
                        dictsToUse.SelectMany(d => new DictionaryFile(d.Text).Details)
                            .DistinctBy(f => f.OldText)
                            .ToDictionary(it => it.OldText, it => it.NewText, StringComparer.Ordinal);

                    if (dictWords.TryGetValue(str.OldText, out string translation))
                    {
                        str.NewText = translation;
                    }
                }
            }

            void CopyToClipboard()
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
            }

            void PasteFromClipboard()
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

                                if (!text.IsNullOrEmpty())
                                    str.OldText = text;
                            }
                            break;
                        case 3:
                            if (!str.IsNewTextReadOnly)
                            {
                                string text = Clipboard.GetText();

                                if (!text.IsNullOrEmpty())
                                {
                                    str.NewText = text;

                                    if (!(str is OneDictionaryString))
                                    {
                                        Utils.AddToSessionDict(str.OldText, str.NewText);

                                        TranslateWithSessionDictIfNeeded(str.OldText, str.NewText);
                                    }
                                }
                            }
                            break;
                    }
                });
            }

            void MoveToHead()
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
            }

            void MoveToNext()
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

            }

            void AddEmpty()
            {
                if (StringFiles.Count == 0)
                    return;

                DictionaryFile dict;

                if (StringFiles.Count > 1)
                    dict = EditorGrid.CurrentItem as DictionaryFile;
                else
                    dict = StringFiles[0] as DictionaryFile;

                dict?.Add("", "");
            }

            void RemoveRow()
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
            }

            void ClearCell()
            {
                RowColumnIndex coords;
                var cell = GetSelectedString(out coords);

                if (cell == null)
                    return;

                // old text
                if (coords.ColumnIndex == 2 && !cell.IsOldTextReadOnly)
                    cell.OldText = string.Empty;
                // new text
                else if (coords.ColumnIndex == 3 && !cell.IsNewTextReadOnly)
                    cell.NewText = string.Empty;
            }

            // Свернуть всё
            AddGridKeyEvent(Key.C, CollapseAll, Ctrls, Shifts);

            // Развернуть всё
            AddGridKeyEvent(Key.E, ExpandAll, Ctrls, Shifts);

            // Скопировать текущую ячейку в буфер
            AddGridKeyEvent(Key.C, CopyToClipboard, Ctrls);

            // Вставить текст из буфера
            AddGridKeyEvent(Key.V, PasteFromClipboard, Ctrls);

            // Переводит выделенную строку с помощью словаря
            AddGridKeyEvent(Key.D, TranslateSelectedWithDictionary, Ctrls);

            // Обработать нажатие Enter (развернуть/свернуть строки файла / открыть окно редактора текущей строки)
            AddGridKeyEvent(Key.Enter, EnterAction);

            // Скопировать старый текст в новый
            AddGridKeyEvent(Key.Right, CopyToNew, Ctrls, Shifts);

            // Перевести выбранную строку/файл
            AddGridKeyEvent(Key.Right, TranslateSelected, Ctrls);

            // Выделить файл, содержащий выделенную строку
            AddGridKeyEvent(Key.Up, MoveToHead, Ctrls, Shifts);

            // Перейти к следующему файлу
            AddGridKeyEvent(Key.Down, MoveToNext, Ctrls, Shifts);

            // Добавить пустую строку в словарь
            AddGridKeyEvent(Key.A, AddEmpty, Ctrls);

            // Удалить строку из словаря
            AddGridKeyEvent(Key.Delete, RemoveRow, Ctrls);

            // Очистить ячейку
            AddGridKeyEvent(Key.Delete, ClearCell);
        }

        private readonly List<(Key key, Key[][] modifers, Action action)> _editorGridKeyEvents = new List<(Key, Key[][], Action)>();

        private void AddGridKeyEvent(Key key, Action action, params Key[][] modifers)
        {
            _editorGridKeyEvents.Add((key, modifers, action));
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
            string[] files = e.GetFilesDrop();

            if (files == null)
                return;

            string firstAdded = null;

            var alreadyAddedFiles = new List<string>();

            foreach (string file in files)
            {
                if (StringFiles.Any(f => f.FileName == file) || GlobalVariables.SourceDictionaries.Any(d => d.Text == file) || SaveDictionary?.FileName == file)
                {
                    if (firstAdded == null)
                        firstAdded = file;

                    alreadyAddedFiles.Add(file);
                    continue;
                }

                IEditableFile current = AndroidFilesUtils.GetSuitableEditableFile(file);

                if (firstAdded == null && current != null)
                    firstAdded = current.FileName;

                StringFiles.AddIfNotNull(current);
            }

            if (alreadyAddedFiles.Count > 0)
            {
                MessBox.ShowDial($"{StringResources.TheFollowingFilesHaveBeenAlreadyAdded}: {Environment.NewLine}{alreadyAddedFiles.Select(it => "  - " + it).JoinStr(Environment.NewLine)}", StringResources.ErrorLower);
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
            bool KeysDown(Key[][] keys) => keys.All(ks => ks.Any(e.KeyboardDevice.IsKeyDown));

            foreach (var ev in _editorGridKeyEvents)
            {
                if (e.Key == ev.key && KeysDown(ev.modifers))
                {
                    ev.action();
                    e.Handled = true;
                    return true;
                }
            }

            return false;
        }

        private void EditorGrid_OnTextInput(object sender, TextCompositionEventArgs e)
        {
            if (GlobalVariables.AppSettings.AlternativeEditingKeys)
            {
                IOneString current = GetSelectedString();

                if (current == null)
                    return;

                WindowManager.ActivateWindow<StringEditorWindow>();

                ManualEventManager.GetEvent<EditStringEvent>()
                    .Publish(new EditStringEvent(current, prev: e.Text));
            }
        }

        private void SfDataGrid_OnCellDoubleTapped(object sender, GridCellDoubleTappedEventArgs args)
        {
            switch (args.Record)
            {
                case IOneString str:
                    WindowManager.ActivateWindow<StringEditorWindow>(this, this);

                    ManualEventManager
                        .GetEvent<EditStringEvent>()
                        .Publish(new EditStringEvent(str, GetStringFile(str)));   

                    break;
                case IEditableFile _:
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

        private IOneString GetSelectedString()
        {
            var result = EditorGrid.SelectedDetailsViewGrid?.SelectedItem as IOneString;

            return result;
        }

        private IOneString GetSelectedString(out RowColumnIndex rowColumn)
        {
            rowColumn = RowColumnIndex.Empty;

            var result = EditorGrid.SelectedDetailsViewGrid?.SelectedItem as IOneString;

            if (result == null)
                return null;

            rowColumn = EditorGrid.SelectedDetailsViewGrid.SelectionController.CurrentCellManager.CurrentRowColumnIndex;

            return result;
        }

        private IEditableFile GetSelectedFile()
        {
            return EditorGrid.SelectedItem as IEditableFile;
        }

        private IEditableFile GetSelectedFile(out int recordRowIndex)
        {
            recordRowIndex = -1;

            var result = EditorGrid.SelectedItem as IEditableFile;

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
        /// Переводит все файлы в редакторе с помощью онлайн переводчика
        /// </summary>
        private void TranslateAllFiles()
        {
            string errorMessage = null;
            var translated = new ConcurrentQueue<(IOneString source, string translatedValue)>();

            void StartActions()
            {
                Disable();
                WindowManager.DisableWindow<MainWindow>();
            }

            void ThreadActions(CancellationToken token, ILoadingProcessWindowInvoker invoker)
            {
                int max = StringFiles.Sum(file => file.Details.Count);
                invoker.ProcessMax = max;
                var cantTranslate = 0;
                var list = StringFiles.SelectMany(file => file.Details);

                max = Math.Min(max, 4);

                Parallel.ForEach(list, (str, state) =>
                {
                    if (token.IsCancellationRequested)
                        return;

                    if (!string.IsNullOrEmpty(str.NewText))
                    {
                        invoker.ProcessValue++;
                        return;
                    }
                    try
                    {
                        invoker.ProcessValue++;

                        string translatedStr = TranslationUtils.TranslateTextWithSettings(str.OldText);

                        translated.Enqueue((str, translatedStr));

                        cantTranslate = 0;
                    }
                    catch (Exception ex)
                    {
                        lock (_lock)
                        {
                            cantTranslate++;

                            if (!token.IsCancellationRequested && cantTranslate >= max)
                            {
                                state.Stop();
                                errorMessage = ex.Message;
                            }
                        }
                    }
                });
            }

            void FinishActions()
            {
                foreach (var vals in translated)
                    vals.source.NewText = vals.translatedValue;

                if (errorMessage != null)
                    MessBox.ShowDial(StringResources.CantTranslate + Environment.NewLine + errorMessage, StringResources.ErrorLower);

                Enable();
                WindowManager.EnableWindow<MainWindow>();
                EditorGrid.Focus();
            }

            LoadingProcessWindow.ShowWindow(StartActions, ThreadActions, FinishActions, ownerWindow: this);
        }

        /// <summary>
        /// Применяет изменения в файлах
        /// </summary>
        /// <param name="onFinished">Действие после обработки</param>
        private void Save(Action onFinished = null)
        {
            void SavingProcess(CancellationToken cts, ILoadingProcessWindowInvoker invoker)
            {
                bool saveToDict = SaveToDict && SaveDictionary != null;
                IDictionaryFile dict = SaveDictionary;

                foreach (var file in StringFiles)
                {
                    invoker.ProcessValue++;

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
            Visibility.Collapsed, StringFiles.Count, ownerWindow: this);
        }

        /// <summary>
        /// Переводит все строки с помощью словаря сессии, если соответствующая настройка активна
        /// </summary>
        private void TranslateWithSessionDict()
        {
            void TranslateWithSessionDict(IEditableFile file)
            {
                if (file == null || file is DictionaryFile)
                    return;

                foreach (var str in file.Details)
                {
                    if (str.NewText.IsNullOrEmpty() && GlobalVariables.SessionDictionary.TryGetValue(str.OldText, out string found))
                        str.NewText = found;
                }
            }

            if (!GlobalVariables.AppSettings.SessionAutoTranslate)
                return;

            StringFiles.ForEach(TranslateWithSessionDict);
        }

        /// <summary>
        /// Переводит выбранные строки с помощью онлайн переводчика
        /// </summary>
        private void TranslateSelected()
        {
            var rows = new List<IOneString>();

            var it = GetSelectedString();

            if (it == null)
            {
                var file = GetSelectedFile();

                if (file != null)
                {
                    rows.AddRange(file.Details);
                }
            }
            else
            {
                rows.Add(it);
            }

            if (rows.Count == 0)
            {
                return;
            }

            string errorMessage = null;
            var translated = new ConcurrentQueue<(IOneString source, string translated)>();

            void ThreadActions(CancellationToken token, ILoadingProcessWindowInvoker invoker)
            {
                int cantTranslate = 0;

                int max = Math.Min(4, rows.Count);

                Parallel.ForEach(rows, (str, state) =>
                {
                    token.ThrowIfCancellationRequested();

                    try
                    {
                        invoker.ProcessValue++;
                        translated.Enqueue((str, TranslationUtils.TranslateTextWithSettings(str.OldText)));

                        cantTranslate = 0;
                    }
                    catch (Exception ex)
                    {
                        if (++cantTranslate >= max)
                        {
                            state.Stop();
                            errorMessage = ex.Message;
                        }
                    }
                });
            }

            void FinishActions()
            {
                foreach (var str in translated)
                    str.source.NewText = str.translated;

                if (errorMessage != null)
                    MessBox.ShowDial(string.Concat(StringResources.CantTranslate, Environment.NewLine, errorMessage), StringResources.ErrorLower);

                Enable();
                EditorGrid.Focus();
            }

            LoadingProcessWindow.ShowWindow(Disable, ThreadActions, FinishActions, progressMax: rows.Count, ownerWindow: this);
        }

        private IEditableFile GetStringFile(IOneString str)
        {
            return EditorGrid.View.Records.Select(_ => _.Data.As<IEditableFile>()).First(_ => _.Details.Contains(str));
        }

        private static void Enable()
        {
            WindowManager.EnableWindow<EditorWindow>();
        }

        private static void Disable()
        {
            WindowManager.DisableWindow<EditorWindow>();
        }

        private bool CheckIfNeedToSave()
        {
            if (StringFiles?.Any(f => f.IsChanged) == true)
            {
                string result = MessBox.ShowDial(StringResources.ApplyTheChanges, "", MessBox.MessageButtons.Yes,
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
            if (GlobalVariables.AppSettings.SessionAutoTranslate)
            {
                ManualEventManager.GetEvent<EditorWindowTranslateTextEvent>()
                    .Publish(new EditorWindowTranslateTextEvent(oldText, newText,
                        EditorWindowTranslateTextEvent.NotDictionaryFileFilter));
            }
        }

        private void EditorGrid_OnLoaded(object sender, RoutedEventArgs e)
        {
            _queueEventManager.SetProcessingType(QueueEventManager.ProcessingTypes.Invoking);

            /*typeof(SfDataGrid).GetEvents().ForEach(ev =>
            {
                AddEventHandler(ev, EditorGrid, () => {Debug.WriteLine(ev.Name);});
            });*/
        }

        /*static void AddEventHandler(EventInfo eventInfo, object item, Action action)
        {
            var parameters = eventInfo.EventHandlerType
                .GetMethod("Invoke")
                .GetParameters()
                .Select(parameter => Expression.Parameter(parameter.ParameterType))
                .ToArray();

            var handler = Expression.Lambda(
                    eventInfo.EventHandlerType,
                    Expression.Call(Expression.Constant(action), "Invoke", Type.EmptyTypes),
                    parameters
                )
                .Compile();

            eventInfo.AddEventHandler(item, handler);
        }*/

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        /*private void ClearDictList_OnClick(object sender, RoutedEventArgs e)
        {
            GlobalVariables.SourceDictionaries.Clear();
        }*/
    }
}