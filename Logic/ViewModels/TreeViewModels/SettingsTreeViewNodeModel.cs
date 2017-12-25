using System;
using TranslatorApk.Logic.Interfaces.SettingsPages;

namespace TranslatorApk.Logic.ViewModels.TreeViewModels
{
    public class SettingsTreeViewNodeModel : TreeViewNodeModelBase<SettingsTreeViewNodeModel>
    {
        public Uri PageUri { get; }

        public ISettingsPageViewModel PageViewModel => _pageViewModelLazy?.Value;

        private readonly Lazy<ISettingsPageViewModel> _pageViewModelLazy;

        public SettingsTreeViewNodeModel(Uri pageUri, Lazy<ISettingsPageViewModel> pageViewModel)
        {
            PageUri = pageUri;
            _pageViewModelLazy = pageViewModel;
        }
    }
}
