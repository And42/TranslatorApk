using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using MVVM_Tools.Code.Commands;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.Utils;
using TranslatorApk.Resources.Localizations;
using TranslatorApk.Windows;

namespace TranslatorApk.Logic.ViewModels.Windows
{
    public class AddLanguageWindowViewModel : ViewModelBase
    {
        private readonly GlobalVariables _globalVariables = GlobalVariables.Instance;

        [DebuggerDisplay("{" + nameof(Title) + "} - {" + nameof(LanguageIso) + "}")]
        public class LanguageViewModel
        {
            public BitmapImage LanguageIcon { get; }
            public string Title { get; }
            public string LanguageIso { get; }

            public LanguageViewModel(BitmapImage languageIcon, string title, string languageIso)
            {
                LanguageIcon = languageIcon;
                Title = title;
                LanguageIso = languageIso;
            }
        }

        /// <summary>
        /// Список целевых языков
        /// </summary>
        public ReadOnlyObservableCollection<LanguageViewModel> TargetLanguages { get; }
        private readonly ObservableRangeCollection<LanguageViewModel> _targetLanguages;

        private string _currentProjectFolder;

        public ICommand AddLanguageCommand => _addLanguageCommand;
        private readonly ActionCommand _addLanguageCommand;

        public LanguageViewModel NewLanguage
        {
            get => _newLanguage;
            set => SetPropertyRef(ref _newLanguage, value);
        }
        private LanguageViewModel _newLanguage;

        public AddLanguageWindowViewModel()
        {
            _targetLanguages = new ObservableRangeCollection<LanguageViewModel>();
            TargetLanguages = new ReadOnlyObservableCollection<LanguageViewModel>(_targetLanguages);

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
            var targetdir = Path.Combine(_currentProjectFolder, "res", "values-" + NewLanguage.LanguageIso);

            if (IOUtils.FolderExists(targetdir))
                IOUtils.DeleteFolder(targetdir);

            IOUtils.CreateFolder(targetdir);

            var filesToCopy = new[] { "strings.xml", "arrays.xml" };

            foreach (var file in filesToCopy)
            {
                string src = Path.Combine(sourcedir, file);

                if (IOUtils.FileExists(src))
                    File.Copy(src, Path.Combine(targetdir, file), true);
            }

            _targetLanguages.Remove(NewLanguage);

            MessBox.ShowDial(StringResources.AllDone);
        }

        public override async Task LoadItems()
        {
            if (string.IsNullOrEmpty(_globalVariables.CurrentProjectFolder.Value))
                return;

            _currentProjectFolder = _globalVariables.CurrentProjectFolder.Value;

            using (BusyDisposable())
            {
                var items = await Task.Factory.StartNew(() =>
                {
                    var values =
                        Directory.EnumerateDirectories(
                                Path.Combine(_currentProjectFolder, "res"), "values-*",
                                SearchOption.TopDirectoryOnly
                            )
                            .Select(Path.GetFileName);

                    var sourceLanguages = new List<LanguageViewModel>();
                    var targetLanguages = new List<LanguageViewModel>();

                    foreach (string value in values)
                    {
                        string languageIso = value.Split('-')[1];
                        string language = LanguageCodesHelper.Instanse.GetLangNameForFolder(value);

                        if (!string.IsNullOrEmpty(language))
                        {
                            sourceLanguages.Add(
                                new LanguageViewModel(null, language, languageIso)
                            );
                        }
                    }

                    foreach (string languageIso in LanguageCodesHelper.Instanse.IsoLanguages)
                    {
                        if (sourceLanguages.All(src => src.LanguageIso != languageIso))
                        {
                            string language = LanguageCodesHelper.Instanse.GetLanguageByLanguageIso(languageIso);
                            string countryIso = LanguageCodesHelper.Instanse.GetCountryIsoByLanguageIso(languageIso);

                            targetLanguages.Add(
                                new LanguageViewModel(ImageUtils.GetFlagImage(countryIso), language, languageIso)
                            );
                        }
                    }

                    return Tuple.Create(sourceLanguages, targetLanguages);
                });

                _targetLanguages.ReplaceRange(items.Item2);
            }
        }

        public override void UnsubscribeFromEvents() { }
    }
}
