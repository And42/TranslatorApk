using System.ComponentModel;

namespace TranslatorApk.Logic.Interfaces
{
    public interface ISettingsContainer : INotifyPropertyChanged
    {
        T GetValue<T>(string settingName);

        void SetValue<T>(string settingName, T value);

        void Save();
    }
}