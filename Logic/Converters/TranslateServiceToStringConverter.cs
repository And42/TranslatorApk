using System;
using System.Collections.Generic;
using System.Globalization;
using TranslatorApk.Logic.Classes;

namespace TranslatorApk.Logic.Converters
{
    public class TranslateServiceToStringConverter : ConverterBase<KeyValuePair<Guid, OneTranslationService>>
    {
        protected override object Convert(KeyValuePair<Guid, OneTranslationService> value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.Value.ToString();
        }
    }
}
