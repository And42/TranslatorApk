using System;
using System.Threading.Tasks;

namespace TranslatorApk.Logic.Interfaces
{
    public interface IViewModelBase : IDisposable
    {
        bool IsBusy { get; set; }

        Task LoadItems();

        void UnsubscribeFromEvents();
    }
}
