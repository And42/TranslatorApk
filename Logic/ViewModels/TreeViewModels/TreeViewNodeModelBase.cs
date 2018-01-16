using MVVM_Tools.Code.Classes;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.Interfaces;

namespace TranslatorApk.Logic.ViewModels.TreeViewModels
{
    public abstract class TreeViewNodeModelBase<TNodeModel> : BindableBase, IHaveChildren<TNodeModel>
    {
        private bool _isExpanded;

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
    }
}
