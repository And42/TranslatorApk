using System;
using System.Globalization;
using System.Windows.Data;

namespace TranslatorApk.Logic.Converters
{
    class StringToIntConverter :IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return int.Parse((string)value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((int)value).ToString();
        }
    }
}
