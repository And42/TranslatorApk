using System.Threading.Tasks;

namespace TranslatorApk.Logic.Interfaces
{
    public interface IViewModelBase
    {
        bool IsLoading { get; set; }

        Task LoadItems();

        void UnsubscribeFromEvents();
    }
}
