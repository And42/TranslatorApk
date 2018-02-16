using MVVM_Tools.Code.Classes;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.Interfaces;

namespace TranslatorApk.Logic.ViewModels.TreeViewModels
{
    public abstract class TreeViewNodeModelBase<TNodeModel> : BindableBase, IHaveChildren<TNodeModel>
    {
        protected TreeViewNodeModelBase(IHaveChildren<TNodeModel> parent = null)
        {
            Parent = parent;
        }

        public IHaveChildren<TNodeModel> Parent { get; }

        public ObservableRangeCollection<TNodeModel> Children { get; } = new ObservableRangeCollection<TNodeModel>();

        public virtual bool IsExpanded
        {
            get => _isExpanded;
            set => SetProperty(ref _isExpanded, value);
        }
        private bool _isExpanded;

        public virtual bool IsVisible
        {
            get => _isVisible;
            set => SetProperty(ref _isVisible, value);
        }
        private bool _isVisible = true;
    }
}
