using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AndroidTranslator.Classes.Files;
using AndroidTranslator.Interfaces.Files;
using AndroidTranslator.Interfaces.Strings;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.EventManagerLogic;
using TranslatorApk.Logic.Events;
using TranslatorApk.Logic.Interfaces;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.Utils;
using UsefulFunctionsLib;

using Res = TranslatorApk.Resources.Localizations.Resources;

namespace TranslatorApk.Windows
{
    /// <summary>
    /// Логика взаимодействия для SearchWindow.xaml
    /// </summary>
    public sealed partial class SearchWindow : IRaisePropertyChanged
    {
        public class FoundItem
        {
            public string FileName { get; }
            public string Text { get; }

            public FoundItem(string fileName, string text)
            {
                FileName = fileName;
                Text = text;
            }
        }

        public ObservableRangeCollection<FoundItem> Files { get; } = new ObservableRangeCollection<FoundItem>();
        public ObservableCollection<string> SearchAdds { get; }

        public Setting<bool> OnlyFullWords { get; }
        public Setting<bool> MatchCase { get; }

        public bool Working
        {
            get => _working;
            set => this.SetProperty(ref _working, value);
        }
        private bool _working;

        public string TextToSearch
        {
            get => _textToSearch;
            set => this.SetProperty(ref _textToSearch, value);
        }
        private string _textToSearch;

        public int SearchBoxIndex
        {
            get => _searchBoxIndex;
            set => this.SetProperty(ref _searchBoxIndex, value);
        }
        private int _searchBoxIndex;

        private static readonly string StartFormattedString = "..." + Path.DirectorySeparatorChar;

        public SearchWindow()
        {
            SearchAdds = new ObservableCollection<string>(SettingsIncapsuler.Instance.FullSearchAdds?.Cast<string>() ?? Enumerable.Empty<string>());
            OnlyFullWords = new Setting<bool>(nameof(SettingsIncapsuler.OnlyFullWords), Res.OnlyFullWords);
            MatchCase = new Setting<bool>(nameof(SettingsIncapsuler.MatchCase), Res.MatchCase);

            InitializeComponent();
            SearchBoxIndex = -1;
        }

        private void SearchWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            SearchBox.Focus();
        }

        private void FindFilesClick(object sender, RoutedEventArgs e)
        {
            if (GlobalVariables.CurrentProjectFolder == null)
            {
                MessBox.ShowDial(Res.SearchWindow_FolderIsNotSelected);
                return;
            }

            string GetFormattedName(string fileName) => StartFormattedString + fileName.Substring(GlobalVariables.CurrentProjectFolder.Length + 1);

            Files.Clear();
            AddToSearchAdds(TextToSearch);

            var filesToAdd = new List<FoundItem>();

            LoadingProcessWindow.ShowWindow(
                beforeStarting: () => Working = true, 
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
                        checkRules = (f, s) => f.IndexOf(s, StringComparison.OrdinalIgnoreCase) > 0;
                    else if (!matchCase /*&& onlyFullWords*/)
                        checkRules = (f, s) => f.Equals(s, StringComparison.OrdinalIgnoreCase);
                    else if (/*matchCase &&*/ !onlyFullWords)
                        checkRules = (f, s) => f.IndexOf(s, StringComparison.Ordinal) > 0;
                    else /*if (matchCase && onlyFullWords)*/
                        checkRules = (f, s) => f.Equals(s, StringComparison.Ordinal);

                    IEnumerable<IEditableFile> union =
                        xmlFiles.SelectSafe<string, IEditableFile>(XmlFile.Create)
                            .UnionWOEqCheck(smaliFiles.SelectSafe(it => new SmaliFile(it)));

                    foreach (IEditableFile file in union)
                    {
                        if (cts.IsCancellationRequested)
                        {
                            return;
                        }

                        IOneString found = file.Details?.FirstOrDefault(str => checkRules(str.OldText, TextToSearch));

                        if (found != null)
                        {
                            filesToAdd.Add(new FoundItem(GetFormattedName(file.FileName), found.OldText));
                        }

                        invoker.ProcessValue++;
                    }
                }, 
                finishActions: () =>
                {
                    Working = false;

                    if (filesToAdd.Count == 0)
                    {
                        MessBox.ShowDial(Res.TextNotFound);
                    }
                    else
                    {
                        Dispatcher.InvokeAction(() => Files.AddRange(filesToAdd));
                    }
                }
            );
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            LoadSelected();
        } 

        private void SearchBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                FindFilesClick(null, null);
            }
        }

        private void FilesView_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                LoadSelected();
            }
        }

        private void AddToSearchAdds(string text)
        {
            SearchAdds.Remove(text);
            SearchAdds.Insert(0, text);
            SearchBoxIndex = 0;

            if (SettingsIncapsuler.Instance.FullSearchAdds == null)
                SettingsIncapsuler.Instance.FullSearchAdds = new StringCollection();

            StringCollection adds = SettingsIncapsuler.Instance.FullSearchAdds;

            adds.Remove(text);
            adds.Insert(0, text);

            if (SearchAdds.Count > 20)
            {
                SearchAdds.RemoveAt(20);
                adds.RemoveAt(20);
            }

            SettingsIncapsuler.Save();
        }

        private void LoadSelected()
        {
            if (FilesView.SelectedIndex > -1)
            {
                var selectedFile = (FoundItem)FilesView.SelectedItem;

                Utils.LoadFile(Path.Combine(GlobalVariables.CurrentProjectFolder, selectedFile.FileName.Substring(StartFormattedString.Length)));

                ManualEventManager.GetEvent<EditorScrollToStringAndSelectEvent>()
                    .Publish(new EditorScrollToStringAndSelectEvent(
                        f => true, str => selectedFile.Text.Equals(str.OldText, StringComparison.Ordinal)));
            }
        }

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
