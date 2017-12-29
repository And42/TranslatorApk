using System;
using TranslatorApk.Logic.Interfaces.SettingsPages;

namespace TranslatorApk.Logic.ViewModels.TreeViewModels
{
    public class SettingsTreeViewNodeModel : TreeViewNodeModelBase<SettingsTreeViewNodeModel>
    {
        public Uri PageUri { get; }

        public ISettingsPageViewModel PageViewModel { get; }

        public SettingsTreeViewNodeModel(Uri pageUri, ISettingsPageViewModel pageViewModel)
        {
            PageUri = pageUri ?? throw new ArgumentNullException(nameof(pageUri));
            PageViewModel = pageViewModel ?? throw new ArgumentNullException(nameof(pageViewModel));
        }
    }
}
