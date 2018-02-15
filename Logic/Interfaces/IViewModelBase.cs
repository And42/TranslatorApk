using System.Threading.Tasks;

namespace TranslatorApk.Logic.Interfaces
{
    public interface IViewModelBase
    {
        bool IsBusy { get; set; }

        Task LoadItems();

        void UnsubscribeFromEvents();
    }
}
