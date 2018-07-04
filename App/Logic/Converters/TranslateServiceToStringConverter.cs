using System;
using System.Collections.Generic;
using System.Globalization;
using MVVM_Tools.Code.Classes;
using TranslatorApk.Logic.Classes;

namespace TranslatorApk.Logic.Converters
{
    public class TranslateServiceToStringConverter : ConverterBase<KeyValuePair<Guid, OneTranslationService>, string>
    {
        public override string ConvertInternal(KeyValuePair<Guid, OneTranslationService> value, object parameter, CultureInfo culture)
        {
            return value.Value.ToString();
        }
    }
}
