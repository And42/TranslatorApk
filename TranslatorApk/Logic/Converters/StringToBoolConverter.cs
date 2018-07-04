using System;
using System.Globalization;
using System.Windows.Data;
using TranslatorApk.Logic.Utils;

namespace TranslatorApk.Logic.Converters
{
    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return bool.Parse(value.As<string>());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.As<bool>().ToString();
        }
    }
}
