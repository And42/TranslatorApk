using System.ComponentModel;

namespace TranslatorApk.Logic.Interfaces
{
    public interface ISettingsContainer : INotifyPropertyChanged
    {
        bool AutoFlush { get; set; }

        T GetValue<T>(string settingName);

        void SetValue<T>(string settingName, T value);

        void Save();
    }
}