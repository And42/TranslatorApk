using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using AndroidTranslator;
using TranslatorApk.Annotations;
using TranslatorApk.Logic;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.EventManagerLogic;
using TranslatorApk.Logic.Events;
using TranslatorApk.Properties;
using UsefulFunctionsLib;

using Res = TranslatorApk.Resources.Localizations.Resources;

namespace TranslatorApk.Windows
{
    /// <summary>
    /// Логика взаимодействия для SearchWindow.xaml
    /// </summary>
    public sealed partial class SearchWindow : INotifyPropertyChanged
    {
        public enum FileTypes
        {
            Xml, Smali
        }

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
            get { return _working; }
            set
            {
                if (_working == value) return;
                _working = value;
                OnPropertyChanged(nameof(Working));
            }
        }
        private bool _working;

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

        public int SearchBoxIndex
        {
            get
            {
                return _searchBoxIndex;
            }
            set
            {
                _searchBoxIndex = value;
                OnPropertyChanged(nameof(SearchBoxIndex));
            }
        }
        private int _searchBoxIndex;

        public SearchWindow()
        {
            SearchAdds      = new ObservableCollection<string>(Settings.Default.FullSearchAdds?.Cast<string>() ?? new string[0]);
            OnlyFullWords   = new Setting<bool>(nameof(Settings.Default.OnlyFullWords), Res.OnlyFullWords);
            MatchCase       = new Setting<bool>(nameof(Settings.Default.MatchCase), Res.MatchCase);

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

            string GetFormattedName(string fileName) => "...\\" + fileName.Substring(GlobalVariables.CurrentProjectFolder.Length + 1);

            Files.Clear();
            AddToSearchAdds(TextToSearch);

            var filesToAdd = new List<FoundItem>();

            LoadingProcessWindow.ShowWindow(() => Working = true, cts =>
            {
                var xmlFiles = Directory.GetFiles(GlobalVariables.CurrentProjectFolder, "*.xml", SearchOption.AllDirectories);
                var smaliFiles = Directory.GetFiles(GlobalVariables.CurrentProjectFolder, "*.smali", SearchOption.AllDirectories);

                LoadingProcessWindow.Instance.ProcessValue = 0;
                LoadingProcessWindow.Instance.ProcessMax = xmlFiles.Length + smaliFiles.Length;
                
                bool onlyFullWords = OnlyFullWords.Value;
                bool matchCase = MatchCase.Value;

                bool CheckRules(string first, string second)
                {
                    if (!matchCase)
                    {
                        first = first.ToUpper();
                        second = second.ToUpper();
                    }

                    return onlyFullWords ? first == second : first.Contains(second);
                }

                var union =
                    xmlFiles.Select<string, EditableFile>(XmlFile.Create)
                        .UnionWOEqCheck(smaliFiles.Select(it => new SmaliFile(it)));

                foreach (var file in union)
                {
                    if (cts.IsCancellationRequested)
                    {
                        return;
                    }

                    OneString found = file.Details?.FirstOrDefault(str => CheckRules(str.OldText, TextToSearch));

                    if (found != null)
                    {
                        filesToAdd.Add(new FoundItem(GetFormattedName(file.FileName), found.OldText));
                    }

                    LoadingProcessWindow.Instance.ProcessValue++;
                }
            }, () =>
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
            });
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

            if (Settings.Default.FullSearchAdds == null)
                Settings.Default.FullSearchAdds = new StringCollection();

            var adds = Settings.Default.FullSearchAdds;

            adds.Remove(text);
            adds.Insert(0, text);

            if (SearchAdds.Count > 20)
            {
                SearchAdds.RemoveAt(20);
                adds.RemoveAt(20);
            }

            Settings.Default.Save();
        }

        private void LoadSelected()
        {
            if (FilesView.SelectedIndex > -1)
            {
                var selectedFile = (FoundItem)FilesView.SelectedItem;

                Functions.LoadFile($"{GlobalVariables.CurrentProjectFolder}\\{selectedFile.FileName.Substring(4)}");

                ManualEventManager.GetEvent<EditorScrollToStringAndSelectEvent>()
                    .Publish(new EditorScrollToStringAndSelectEvent(
                        f => true, str => selectedFile.Text.Equals(str.OldText, StringComparison.Ordinal)));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
