using System.Threading.Tasks;
using MVVM_Tools.Code.Classes;
using MVVM_Tools.Code.Disposables;
using TranslatorApk.Logic.Interfaces;

namespace TranslatorApk.Logic.Classes
{
    public abstract class ViewModelBase : BindableBase, IViewModelBase
    {
        protected static readonly Task EmptyTask = new Task(() => {});

        public virtual bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }
        private bool _isBusy;

        protected CustomBoolDisposable BusyDisposable()
        {
            return new CustomBoolDisposable(val => IsBusy = val);
        }

        public virtual Task LoadItems() => EmptyTask;

        public abstract void UnsubscribeFromEvents();
    }
}
