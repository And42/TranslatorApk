using System;
using System.Globalization;

namespace TranslatorApk.Logic.Converters
{
    class StringToIntConverter : ConverterBase<string>
    {
        protected override object Convert(string value, Type targetType, object parameter, CultureInfo culture)
        {
            return int.Parse(value);
        }

        protected override string ConvertBackObj(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((int)value).ToString();
        }
    }
}
