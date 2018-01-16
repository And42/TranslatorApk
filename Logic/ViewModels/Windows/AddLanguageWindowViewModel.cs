using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.CustomCommandContainers;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.Utils;
using TranslatorApk.Resources.Localizations;
using TranslatorApk.Windows;
using UsefulFunctionsLib;

namespace TranslatorApk.Logic.ViewModels.Windows
{
    public class AddLanguageWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// Список исходных языков
        /// </summary>
        public ReadOnlyObservableCollection<string> SourceLanguages { get; }
        private readonly ObservableRangeCollection<string> _sourceLanguages;

        /// <summary>
        /// Список целевых языков
        /// </summary>
        public ReadOnlyObservableCollection<NewLanguageViewModel> TargetLanguages { get; }
        private readonly ObservableRangeCollection<NewLanguageViewModel> _targetLanguages;

        private readonly List<string> _folderLangs = GlobalVariables.SettingsFoldersOfLanguages;
        private readonly List<string> _folderLocalizedLangs = GlobalVariables.SettingsNamesOfFolderLanguages;

        private string _currentProjectFolder;

        public ICommand AddLanguageCommand => _addLanguageCommand;
        private readonly ActionCommand _addLanguageCommand;

        public NewLanguageViewModel NewLanguage
        {
            get => _newLanguage;
            set => SetPropertyRef(ref _newLanguage, value);
        }
        private NewLanguageViewModel _newLanguage;

        public AddLanguageWindowViewModel()
        {
            _sourceLanguages = new ObservableRangeCollection<string>();
            SourceLanguages = new ReadOnlyObservableCollection<string>(_sourceLanguages);

            _targetLanguages = new ObservableRangeCollection<NewLanguageViewModel>();
            TargetLanguages = new ReadOnlyObservableCollection<NewLanguageViewModel>(_targetLanguages);

            _addLanguageCommand = new ActionCommand(AddLanguageCommand_Execute);
        }

        private void AddLanguageCommand_Execute()
        {
            if (NewLanguage == null)
            {
                MessBox.ShowDial(StringResources.LanguageIsNotSelected, StringResources.ErrorLower);
                return;
            }

            var sourcedir = Path.Combine(_currentProjectFolder, "res", "values");
            var targetdir = Path.Combine(_currentProjectFolder, "res", _folderLangs[_folderLocalizedLangs.IndexOf(NewLanguage.Title)]);

            if (Directory.Exists(targetdir))
                Directory.Delete(targetdir, true);

            Directory.CreateDirectory(targetdir);

            var filesToCopy = new[] { "strings.xml", "arrays.xml" };

            foreach (var file in filesToCopy)
            {
                string src = Path.Combine(sourcedir, file);

                if (File.Exists(src))
                    File.Copy(src, Path.Combine(targetdir, file), true);
            }

            _sourceLanguages.Add(NewLanguage.Title);
            _targetLanguages.RemoveAt(TargetLanguages.FindIndex(it => it.Title == NewLanguage.Title));

            MessBox.ShowDial(StringResources.AllDone);
        }

        public override async Task LoadItems()
        {
            if (IsLoading || GlobalVariables.CurrentProjectFolder == null)
                return;

            _currentProjectFolder = GlobalVariables.CurrentProjectFolder;

            using (new LoadingDisposable(this))
            {
                var items = await Task.Factory.StartNew(() =>
                {
                    var values =
                        Directory.EnumerateDirectories(
                                Path.Combine(_currentProjectFolder, "res"), "values*",
                                SearchOption.TopDirectoryOnly
                            )
                            .Select(Path.GetFileName);

                    var sourceLanguages = new List<string>();
                    var targetLanguages = new List<NewLanguageViewModel>();

                    foreach (string value in values)
                    {
                        int index = _folderLangs.IndexOf(value);

                        if (index > -1)
                        {
                            sourceLanguages.Add(_folderLocalizedLangs[index]);
                        }
                    }

                    foreach (string lang in _folderLocalizedLangs)
                    {
                        if (SourceLanguages.All(src => src != lang))
                        {
                            string name = _folderLangs[_folderLocalizedLangs.IndexOf(lang)];

                            targetLanguages.Add(
                                new NewLanguageViewModel
                                {
                                    LanguageIcon = ImageUtils.GetFlagImage(name),
                                    Title = lang
                                }
                            );
                        }
                    }

                    return Tuple.Create(sourceLanguages, targetLanguages);
                });

                _sourceLanguages.ReplaceRange(items.Item1);
                _targetLanguages.ReplaceRange(items.Item2);
            }
        }

        public override void UnsubscribeFromEvents() { }
    }
}
