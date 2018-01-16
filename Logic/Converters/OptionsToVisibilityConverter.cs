using System.Globalization;
using System.Windows;
using MVVM_Tools.Code.Classes;
using TranslatorApk.Logic.Classes;

namespace TranslatorApk.Logic.Converters
{
    class OptionsToVisibilityConverter : ConverterBase<Options, Visibility>
    {
        public override Visibility ConvertInternal(Options value, object parameter, CultureInfo culture)
        {
            if (value.FullPath == string.Empty)
                return Visibility.Collapsed;

            bool param = parameter != null;

            if (param)
                return value.IsFolder ? Visibility.Visible : Visibility.Collapsed;

            return !value.IsFolder ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
