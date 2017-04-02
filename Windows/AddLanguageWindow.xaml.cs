using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using TranslatorApk.Annotations;
using TranslatorApk.Logic.OrganisationItems;
using UsefulFunctionsLib;

using Res = TranslatorApk.Resources.Localizations.Resources;

namespace TranslatorApk.Windows
{
    /// <summary>
    /// Interaction logic for AddLanguageWindow.xaml
    /// </summary>
    public partial class AddLanguageWindow : INotifyPropertyChanged
    {
        /// <summary>
        /// Список исходных языков
        /// </summary>
        public ObservableCollection<string> SourceLanguages { get; set; } = new ObservableCollection<string>();

        /// <summary>
        /// Список целевых языков
        /// </summary>
        public ObservableCollection<Tuple<ImageSource, string>> TargetLanguages { get; set; } = new ObservableCollection<Tuple<ImageSource, string>>();

        private readonly List<string> FolderLangs = GlobalVariables.Settings_FoldersOfLanguages;
        private readonly List<string> FolderLocalizedLangs = GlobalVariables.Settings_NamesOfFolderLanguages;

        private readonly string folder;

        /// <summary>
        /// Создаёт окно добавления новых языков
        /// </summary>
        /// <param name="folderOfProject">Папка проекта</param>
        public AddLanguageWindow(string folderOfProject)
        {
            InitializeComponent();

            if (folderOfProject == null)
                return;

            folder = folderOfProject + "\\res";

            var values = Directory.GetDirectories(folder, "values*", SearchOption.TopDirectoryOnly).Select(Path.GetFileName);

            foreach (var value in values)
            {
                int index = FolderLangs.IndexOf(value);

                if (index > -1)
                {
                    SourceLanguages.Add(FolderLocalizedLangs[index]);
                }
            }

            foreach (var lang in FolderLocalizedLangs)
            {
                if (SourceLanguages.All(src => src != lang))
                {
                    string name = FolderLangs[FolderLocalizedLangs.IndexOf(lang)];
                    TargetLanguages.Add(new Tuple<ImageSource, string>(Functions.GetFlagImage(name), lang));
                }
            }
        }

        private void AddLangClick(object sender, RoutedEventArgs e)
        {
            if (TargetBox.SelectedValue == null)
            {
                MessBox.ShowDial(Res.LanguageIsNotSelected, Res.ErrorLower);
                return;
            }

            string target = TargetBox.SelectedValue.As<Tuple<ImageSource, string>>().Item2;

            var sourcedir = $"{folder}\\values";
            var targetdir = $"{folder}\\{FolderLangs[FolderLocalizedLangs.IndexOf(target)]}";

            if (Directory.Exists(targetdir))
                Directory.Delete(targetdir, true);

            Directory.CreateDirectory(targetdir);

            var filesToCopy = new[] {"strings.xml", "arrays.xml"};

            foreach (var file in filesToCopy)
            {
                string src = sourcedir + "\\" + file;
                if (File.Exists(src))
                    File.Copy(src, $"{targetdir}\\{file}", true);
            }

            SourceLanguages.Add(target);
            TargetLanguages.RemoveAt(TargetLanguages.FindIndex(it => it.Item2 == target));

            MessBox.ShowDial(Res.AllDone);
        }

        #pragma warning disable CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]

        protected virtual void OnPropertyChanged(string propertyName)
        #pragma warning restore CS1591 // Отсутствует комментарий XML для открытого видимого типа или члена
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
