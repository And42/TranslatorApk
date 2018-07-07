using MVVM_Tools.Code.Classes;

namespace TranslatorApk.Logic.Classes
{
    public class CheckableSetting : BindableBase
    {
        public string Text
        {
            get => _text;
            set => SetProperty(ref _text, value);
        }
        private string _text;

        public bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }
        private bool _isChecked;

        public CheckableSetting(string text, bool isChecked = false)
        {
            _text = text;
            _isChecked = isChecked;
        }
    }
}
