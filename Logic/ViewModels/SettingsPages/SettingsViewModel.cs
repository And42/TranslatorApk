using System;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.Interfaces.SettingsPages;
using TranslatorApk.Logic.ViewModels.TreeViewModels;
using TranslatorApk.Pages.Settings;

namespace TranslatorApk.Logic.ViewModels.SettingsPages
{
    public class SettingsViewModel : BindableBase
    {
        public static SettingsViewModel GetNewInstanse() => new SettingsViewModel();

        private const string PagesStartPath = "/Pages/Settings/";

        private SettingsTreeViewNodeModel _currentPage;       

        private SettingsViewModel()
        {
            PagesRoot.Children.AddRange(
                CreateNodeModel<AppSettingsPage>(AppSettingsPageViewModel.InstanseLazy),
                CreateNodeModel<TranslationSettingsPage>(TranslationPageViewModel.InstanseLazy),
                CreateNodeModel<EditorSettingsPage>(EditorSettingsPageViewModel.InstanseLazy)
            );

            CurrentPage = PagesRoot.Children[0];
        }

        public SettingsTreeViewNodeModel CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        public SettingsTreeViewNodeModel PagesRoot { get; } = new SettingsTreeViewNodeModel(null, null);

        private static Uri FormatPage(string pageName) => new Uri($"{PagesStartPath}{pageName}.xaml", UriKind.Relative);

        private static SettingsTreeViewNodeModel CreateNodeModel<TPage>(Lazy<ISettingsPageViewModel> modelInstanse)
        {
            return 
                new SettingsTreeViewNodeModel(
                    FormatPage(typeof(TPage).Name),
                    modelInstanse
                );
        }
    }
}
