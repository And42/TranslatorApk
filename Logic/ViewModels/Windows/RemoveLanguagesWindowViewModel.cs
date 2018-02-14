using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
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

        private readonly List<string> _folderLangs = GlobalVariables.SettingsFoldersOfLanguages;
        private readonly List<string> _folderLocalizedLangs = GlobalVariables.SettingsNamesOfFolderLanguages;

        public ICommand RemoveLanguagesCommand => _removeLanguagesCommand;
        private readonly ActionCommand _removeLanguagesCommand;

        public RemoveLanguagesWindowViewModel()
        {
            _languages = new ObservableRangeCollection<SourceLanguageModel>();
            Languages = new ReadOnlyObservableCollection<SourceLanguageModel>(_languages);

            _removeLanguagesCommand = new ActionCommand(RemoveLanguagesCommand_Execute, () => !IsLoading);

            PropertyChanged += OnPropertyChanged;
        }

        private async void RemoveLanguagesCommand_Execute()
        {
            var sourcedir = Path.Combine(GlobalVariables.CurrentProjectFolder, "res");

            if (!Directory.Exists(sourcedir))
                return;

            using (LoadingDiposable())
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

            using (LoadingDiposable())
            {
                List<SourceLanguageModel> languages = await GetLanguageFoldersAsync(CancellationToken.None);
                _languages.ReplaceRange(languages);
            }
        }

        private Task<List<SourceLanguageModel>> GetLanguageFoldersAsync(CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() => GetLanguageFolders(cancellationToken), cancellationToken);
        }

        private List<SourceLanguageModel> GetLanguageFolders(CancellationToken cancellationToken)
        {
            var existingFolders =
                Directory.EnumerateDirectories(
                        Path.Combine(GlobalVariables.CurrentProjectFolder, "res"), "values*",
                        SearchOption.TopDirectoryOnly
                    )
                    .Select(Path.GetFileName)
                    .Where(it => IgnoreRegexes.All(reg => !reg.IsMatch(it)));

            var sourceLanguages = new List<SourceLanguageModel>();

            foreach (string folder in existingFolders)
            {
                cancellationToken.ThrowIfCancellationRequested();

                int index = _folderLangs.IndexOf(folder);

                string title = folder;
                BitmapImage icon = null;

                if (index > -1)
                {
                    title = _folderLocalizedLangs[index];
                    icon = ImageUtils.GetFlagImage(folder);
                }

                sourceLanguages.Add(new SourceLanguageModel(title, folder, icon));
            }

            return sourceLanguages;
        }

        private void RefreshCommandsCanExecute()
        {
            _removeLanguagesCommand.RaiseCanExecuteChanged();
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(IsLoading):
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
