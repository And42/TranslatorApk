using System;
using System.Linq.Expressions;
using System.Reflection;
using MVVM_Tools.Code.Classes;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.Utils;

namespace TranslatorApk.Logic.Classes
{
    public sealed class Setting<T> : BindableBase
    {
        private readonly AppSettings _appSettings = GlobalVariables.AppSettings;
        private readonly PropertyInfo _property;

        private readonly bool _isReadOnly;

        public Setting(Expression<Func<AppSettings, T>> setting, string localizedName, bool isReadOnly = false)
        {
            LocalizedName = localizedName;
            _isReadOnly = isReadOnly;
            _property = ReflectionUtils.GetPropertyInfo(setting);
        }

        /// <summary>
        /// Локализованное имя настройки
        /// </summary>
        public string LocalizedName
        {
            get => _localizedName;
            set => SetProperty(ref _localizedName, value);
        }
        private string _localizedName;

        /// <summary>
        /// Значение настройки
        /// </summary>
        public T Value
        {
            get => (T)_property.GetValue(_appSettings, null);
            set
            {
                if (_isReadOnly)
                    throw new NotSupportedException("Can't set readonly value");

                _property.SetValue(_appSettings, value, null);

                OnPropertyChanged();
            }
        }
    }
}
