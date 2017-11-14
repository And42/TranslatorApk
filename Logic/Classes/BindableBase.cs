using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TranslatorApk.Logic.Classes
{
    public abstract class BindableBase : INotifyPropertyChanged
    {
        public virtual bool SetProperty<TPropType>(ref TPropType storage, TPropType value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<TPropType>.Default.Equals(storage, value))
                return false;

            storage = value;
            RaisePropertyChanged(propertyName);

            return true;
        }

        public virtual bool SetPropertyRef<TPropType>(ref TPropType storage, TPropType value, [CallerMemberName] string propertyName = null) where TPropType : class
        {
            if (ReferenceEquals(storage, value))
                return false;

            storage = value;
            RaisePropertyChanged(propertyName);

            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
