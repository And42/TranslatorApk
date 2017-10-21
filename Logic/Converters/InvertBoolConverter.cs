using System;
using System.Globalization;

namespace TranslatorApk.Logic.Converters
{
    class InvertBoolConverter : ConverterBase<bool>
    {
        protected override object Convert(bool value, Type targetType, object parameter, CultureInfo culture)
        {
            return !value;
        }

        protected override bool ConvertBackObj(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !(bool)value;
        }
    }
}
