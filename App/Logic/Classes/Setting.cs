using System;
using System.ComponentModel;
using System.Reflection;
using TranslatorApk.Logic.Interfaces;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.Utils;

namespace TranslatorApk.Logic.Classes
{
    public sealed class Setting<T> : IRaisePropertyChanged
    {
        private readonly PropertyInfo _property;

        private readonly bool _isReadOnly;

        public Setting(string settingName, string localizedName, bool isReadOnly = false)
        {
            SettingName = settingName;
            LocalizedName = localizedName;
            _isReadOnly = isReadOnly;

            _property = GlobalVariables.AppSettings.GetType().GetProperty(settingName);
        }

        /// <summary>
        /// Имя настройки
        /// </summary>
        public string SettingName
        {
            get => _settingName;
            private set => this.SetProperty(ref _settingName, value);
        }
        private string _settingName;

        /// <summary>
        /// Локализованное имя настройки
        /// </summary>
        public string LocalizedName
        {
            get => _localizedName;
            set => this.SetProperty(ref _localizedName, value);
        }
        private string _localizedName;

        /// <summary>
        /// Значение настройки
        /// </summary>
        public T Value
        {
            get => (T)_property.GetValue(GlobalVariables.AppSettings, null);
            set
            {
                if (_isReadOnly)
                    throw new NotSupportedException("Can't set readonly value");

                _property.SetValue(GlobalVariables.AppSettings, value, null);

                RaisePropertyChanged(nameof(Value));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
