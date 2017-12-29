using TranslatorApk.Logic.Interfaces;

namespace TranslatorApk.Logic.Classes
{
    public abstract class ViewModelBase : BindableBase, IViewModelBase
    {
        public abstract void UnsubscribeFromEvents();
    }
}
