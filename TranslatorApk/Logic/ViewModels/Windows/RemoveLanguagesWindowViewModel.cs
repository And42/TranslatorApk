using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using MVVM_Tools.Code.Classes;
using MVVM_Tools.Code.Commands;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.Utils;
using TranslatorApk.Resources.Localizations;
using TranslatorApk.Windows;

namespace TranslatorApk.Logic.ViewModels.Windows
{
    public class RemoveLanguagesWindowViewModel : ViewModelBase
    {
        public class SourceLanguageModel : BindableBase
        {
            public string Title { get; }
            public string Folder { get; }
            public BitmapImage Icon { get; }

            public SourceLanguageModel(string title, string folder, BitmapImage icon)
            {
                Title = title;
                Folder = folder;
                Icon = icon;
            }

            public bool IsSelected
            {
                get => _isSelected;
                set => SetProperty(ref _isSelected, value);
            }
            private bool _isSelected;
        }

        private static readonly Regex[] IgnoreRegexes =
        {
            new Regex(@".*-v[\d]+$"),
            new Regex(@".*-land$")
        };

        /// <summary>
        /// Список исходных языков
        /// </summary>
        public ReadOnlyObservableCollection<SourceLanguageModel> Languages { get; }
        private readonly ObservableRangeCollection<SourceLanguageModel> _languages;

        public IActionCommand RemoveLanguagesCommand { get; }

        public RemoveLanguagesWindowViewModel()
        {
            _languages = new ObservableRangeCollection<SourceLanguageModel>();
            Languages = new ReadOnlyObservableCollection<SourceLanguageModel>(_languages);

            RemoveLanguagesCommand = new ActionCommand(RemoveLanguagesCommand_Execute, () => !IsBusy);

            PropertyChanged += OnPropertyChanged;
        }

        private async void RemoveLanguagesCommand_Execute()
        {
            var sourcedir = Path.Combine(GlobalVariables.CurrentProjectFolder, "res");

            if (!Directory.Exists(sourcedir))
                return;

            using (BusyDisposable())
            {
                List<SourceLanguageModel> toDelete = Languages.Where(it => it.IsSelected).ToList();

                await Task.Factory.StartNew(() =>
                {
                    foreach (var item in toDelete)
                    {
                        IOUtils.DeleteFolder(Path.Combine(sourcedir, item.Folder));
                    }
                });

                _languages.RemoveRange(toDelete);

                MessBox.ShowDial(StringResources.AllDone);
            }
        }

        public override async Task LoadItems()
        {
            if (GlobalVariables.CurrentProjectFolder == null)
                return;

            using (BusyDisposable())
            {
                List<SourceLanguageModel> languages = await GetLanguageFoldersAsync(CancellationToken.None);
                _languages.ReplaceRange(languages);
            }
        }

        private static Task<List<SourceLanguageModel>> GetLanguageFoldersAsync(CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() => GetLanguageFolders(cancellationToken), cancellationToken);
        }

        private static List<SourceLanguageModel> GetLanguageFolders(CancellationToken cancellationToken)
        {
            var existingFolders =
                Directory.EnumerateDirectories(
                        Path.Combine(GlobalVariables.CurrentProjectFolder, "res"), "values-*",
                        SearchOption.TopDirectoryOnly
                    )
                    .Select(Path.GetFileName)
                    .Where(it => IgnoreRegexes.All(reg => !reg.IsMatch(it)));

            var sourceLanguages = new List<SourceLanguageModel>();

            var flagFiles = 
                Directory.EnumerateFiles(GlobalVariables.PathToFlags)
                    .Select(Path.GetFileNameWithoutExtension).ToHashSet();

            foreach (string folder in existingFolders)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string folderPart = folder.Substring(folder.IndexOf('-') + 1);

                string language = LanguageCodesHelper.Instanse.GetLangNameForFolder(folder);

                string languageIso = folder.Split('-')[1];
                string countryIso = LanguageCodesHelper.Instanse.GetCountryIsoByLanguageIso(languageIso);

                string title = folder;
                BitmapImage icon;

                // default folder
                if (!string.IsNullOrEmpty(language))
                {
                    title = language;
                    icon = ImageUtils.GetFlagImage(countryIso);
                }
                // custom folder
                else if (flagFiles.Contains(countryIso))
                {
                    icon = ImageUtils.GetFlagImage(countryIso + ".png");
                }
                else if (flagFiles.Contains(folderPart))
                {
                    icon = ImageUtils.GetFlagImage(folderPart + ".png");
                }
                else
                {
                    continue;
                }

                sourceLanguages.Add(new SourceLanguageModel(title, folder, icon));
            }

            return sourceLanguages;
        }

        private void RefreshCommandsCanExecute()
        {
            RemoveLanguagesCommand.RaiseCanExecuteChanged();
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(IsBusy):
                    RefreshCommandsCanExecute();
                    break;
            }
        }

        public override void UnsubscribeFromEvents()
        {
            PropertyChanged -= OnPropertyChanged;
        }
    }
}
