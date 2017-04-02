using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using TranslatorApk.Logic.Classes;
using UsefulFunctionsLib;

namespace TranslatorApk.Logic.Converters
{
    public class TranslateServiceToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.As<KeyValuePair<Guid, OneTranslationService>>().Value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
