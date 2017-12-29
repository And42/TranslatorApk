using System;
using System.Collections.Generic;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.Interfaces.SettingsPages;
using TranslatorApk.Logic.ViewModels.TreeViewModels;
using TranslatorApk.Pages.Settings;

namespace TranslatorApk.Logic.ViewModels.SettingsPages
{
    public class SettingsViewModel : ViewModelBase
    {
        private const string PagesStartPath = "/Pages/Settings/";

        private SettingsTreeViewNodeModel _currentPage;       

        public SettingsViewModel()
        {
            PagesRoot.AddRange(
                CreateNodeModel<AppSettingsPage>(new AppSettingsPageViewModel()),
                CreateNodeModel<TranslationSettingsPage>(new TranslationPageViewModel()),
                CreateNodeModel<EditorSettingsPage>(new EditorSettingsPageViewModel())
            );

            CurrentPage = PagesRoot[0];
        }

        public SettingsTreeViewNodeModel CurrentPage
        {
            get => _currentPage;
            set => SetProperty(ref _currentPage, value);
        }

        public ObservableRangeCollection<SettingsTreeViewNodeModel> PagesRoot { get; } = new ObservableRangeCollection<SettingsTreeViewNodeModel>();

        private static Uri FormatPage(string pageName) => new Uri($"{PagesStartPath}{pageName}.xaml", UriKind.Relative);

        private static SettingsTreeViewNodeModel CreateNodeModel<TPage>(ISettingsPageViewModel modelInstanse)
        {
            return 
                new SettingsTreeViewNodeModel(
                    FormatPage(typeof(TPage).Name),
                    modelInstanse
                );
        }

        public override void UnsubscribeFromEvents()
        {
            foreach (var page in PagesRoot)
                UnsubscribeFromEvents(page);
        }

        private static void UnsubscribeFromEvents(SettingsTreeViewNodeModel rootNode)
        {
            if (rootNode.Children.Count == 0)
            {
                rootNode.PageViewModel.UnsubscribeFromEvents();
                return;
            }

            var nodes = new List<SettingsTreeViewNodeModel> {rootNode};

            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];

                node.PageViewModel.UnsubscribeFromEvents();

                nodes.AddRange(node.Children);
            }
        }
    }
}
