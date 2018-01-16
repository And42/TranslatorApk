using MVVM_Tools.Code.Classes;

namespace TranslatorApk.Logic.Classes
{
    public class CheckBoxSetting : BindableBase
    {
        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }
        private string _text;

        public bool Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }
        private bool _value;

        public CheckBoxSetting(string text, bool value = false)
        {
            _text = text;
            _value = value;
        }
    }
}
