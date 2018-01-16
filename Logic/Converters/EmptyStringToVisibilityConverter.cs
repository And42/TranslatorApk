using System.Globalization;
using System.Windows;
using MVVM_Tools.Code.Classes;

namespace TranslatorApk.Logic.Converters
{
    public class EmptyStringToVisibilityConverter : ConverterBase<string, Visibility>
    {
        public override Visibility ConvertInternal(string value, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
