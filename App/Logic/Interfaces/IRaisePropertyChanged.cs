using System.ComponentModel;

namespace TranslatorApk.Logic.Interfaces
{
    public interface IRaisePropertyChanged : INotifyPropertyChanged
    {
        void RaisePropertyChanged(string propertyName);
    }
}