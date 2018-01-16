using System.Windows.Media.Imaging;
using MVVM_Tools.Code.Classes;

namespace TranslatorApk.Logic.ViewModels
{
    public class NewLanguageViewModel : BindableBase
    {
        public BitmapImage LanguageIcon
        {
            get => _languageIcon;
            set => SetProperty(ref _languageIcon, value);
        }
        private BitmapImage _languageIcon;

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }
        private string _title;
    }
}
