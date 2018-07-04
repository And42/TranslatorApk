using TranslatorApk.Logic.Interfaces;
using TranslatorApk.Windows;

namespace TranslatorApk.Logic.Classes
{
    public class LoadingProcessWindowInvoker : ILoadingProcessWindowInvoker
    {
        public int ProcessMax
        {
            get => _window.ProcessMax;
            set => _window.ProcessMax = value;
        }

        public int ProcessValue
        {
            get => _window.ProcessValue;
            set => _window.ProcessValue = value;
        }

        public bool IsIndeterminate
        {
            get => _window.IsIndeterminate;
            set => _window.IsIndeterminate = value;
        }

        private readonly LoadingProcessWindow _window;

        public LoadingProcessWindowInvoker(LoadingProcessWindow window)
        {
            _window = window;
        }
    }
}
