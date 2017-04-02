using System;
using System.ComponentModel;
using TranslatorApk.Annotations;
using TranslatorApk.Properties;

namespace TranslatorApk.Logic.Classes
{
    public sealed class Setting<T> : INotifyPropertyChanged
    {
        private readonly Action<T> OnSetAction;
        private readonly bool setSettingManually;

        public Setting(string settingName, string localizedName, Action<T> onSetValue = null, bool setSettingManually = true)
        {
            SettingName = settingName;
            LocalizedName = localizedName;
            OnSetAction = onSetValue ?? (obj => { });
            this.setSettingManually = setSettingManually;
        }

        /// <summary>
        /// Имя настройки
        /// </summary>
        public string SettingName
        {
            get { return _settingName; }
            private set
            {
                _settingName = value;
                OnPropertyChanged(nameof(SettingName));
            }
        }
        private string _settingName;

        /// <summary>
        /// Локализованное имя настройки
        /// </summary>
        public string LocalizedName
        {
            get { return _localizedName; }
            set
            {
                _localizedName = value;
                OnPropertyChanged(nameof(LocalizedName));
            }
        }
        private string _localizedName;

        /// <summary>
        /// Значение настройки
        /// </summary>
        public T Value
        {
            get { return (T)Settings.Default[SettingName]; }
            set
            {
                if (setSettingManually)
                {
                    Settings.Default[SettingName] = value;
                    Settings.Default.Save();
                }

                OnSetAction(value);
                OnPropertyChanged(nameof(Value));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
