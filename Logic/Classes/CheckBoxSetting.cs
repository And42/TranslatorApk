using System.ComponentModel;
using TranslatorApk.Annotations;

namespace TranslatorApk.Logic.Classes
{
    public class CheckBoxSetting : INotifyPropertyChanged
    {
        public string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                OnPropertyChanged(nameof(Text));
            }
        }
        private string _text;

        public bool Value
        {
            get
            {
                return _value;
            }
            set
            {
                _value = value;
                OnPropertyChanged(nameof(Value));
            }
        }
        private bool _value;

        public CheckBoxSetting(string text, bool value = false)
        {
            _text = text;
            _value = value;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
