using System.Windows.Media.Imaging;
using TranslatorApk.Logic.Classes;

namespace TranslatorApk.Logic.ViewModels
{
    public class NewLanguageViewModel : BindableBase
    {
        private BitmapImage _languageIcon;
        private string _title;

        public BitmapImage LanguageIcon
        {
            get => _languageIcon;
            set => SetProperty(ref _languageIcon, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }
    }
}
