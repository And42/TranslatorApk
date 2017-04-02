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

using UsefulFunctionsLib;

using Res = TranslatorApk.Resources.Localizations.Resources;

namespace TranslatorApk.Windows
{
    /// <summary>
    /// Логика взаимодействия для SearchWindow.xaml
    /// </summary>
    public sealed partial class SearchWindow : INotifyPropertyChanged
    {
        public ObservableCollection<string> Files
        {
            get { return _files; }
            private set
            {
                if (_files == value) return;
                _files = value;
                OnPropertyChanged(nameof(Files));
            }
        }
        private ObservableCollection<string> _files = new ObservableCollection<string>();

        public bool Working { get; set; }

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

        public Setting<bool> OnlyFullWords { get; set; }
        public Setting<bool> MatchCase { get; set; }

        public ObservableCollection<string> SearchAdds { get; }

        public SearchWindow()
        {
            SearchAdds = new ObservableCollection<string>(Properties.Settings.Default.FullSearchAdds?.Cast<string>() ?? new string[0]);
            OnlyFullWords = new Setting<bool>("OnlyFullWords", Res.OnlyFullWords);
            MatchCase = new Setting<bool>("MatchCase", Res.MatchCase);

            InitializeComponent();
            SearchBoxIndex = -1;
        }

        private void FindFiles(object sender, RoutedEventArgs e)
        {
            if (GlobalVariables.CurrentProjectFolder == null)
            {
                MessBox.ShowDial(Res.SearchWindow_FolderIsNotSelected);
                return;
            }

            string GetFormattedName(string fileName) => "...\\" + fileName.Substring(GlobalVariables.CurrentProjectFolder.Length + 1);

            Files.Clear();
            AddToSearchAdds(TextToSearch);

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

                    if (file.Details?.Any(str => CheckRules(str.OldText, TextToSearch)) == true)
                    {
                        Dispatcher.InvokeAction(() => Files.Add(GetFormattedName(file.FileName)));
                    }

                    LoadingProcessWindow.Instance.ProcessValue++;
                }
            }, () =>
            {
                Working = false;

                if (Files.Count == 0)
                {
                    MessBox.ShowDial(Res.TextNotFound);
                }
            });
        }

        private void AddToSearchAdds(string text)
        {
            Dispatcher.InvokeAction(() =>
            {
                SearchAdds.Remove(text);
                SearchAdds.Insert(0, text);
                SearchBoxIndex = 0;

                if (Properties.Settings.Default.FullSearchAdds == null)
                    Properties.Settings.Default.FullSearchAdds = new StringCollection();
                Properties.Settings.Default.FullSearchAdds.Remove(text);
                Properties.Settings.Default.FullSearchAdds.Insert(0, text);
                if (SearchAdds.Count > 20)
                {
                    SearchAdds.RemoveAt(20);
                    Properties.Settings.Default.FullSearchAdds.RemoveAt(20);
                }

                Properties.Settings.Default.Save();
            });
        }

        private void ListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            LoadSelected();
        }

        private void LoadSelected()
        {
            if (FilesView.SelectedIndex > -1)
                Functions.LoadFile($"{GlobalVariables.CurrentProjectFolder}\\{Files[FilesView.SelectedIndex].Substring(4)}");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SearchBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                FindFiles(null, null);
            }
        }

        private void SearchWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            SearchBox.Focus();
        }

        private void FilesView_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                LoadSelected();
            }
        }
    }
}
