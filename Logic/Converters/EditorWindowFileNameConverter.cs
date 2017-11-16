using System;
using System.Globalization;
using TranslatorApk.Logic.OrganisationItems;

namespace TranslatorApk.Logic.Converters
{
    public class EditorWindowFileNameConverter : ConverterBase<string>
    {
        protected override object Convert(string value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value.StartsWith(GlobalVariables.CurrentProjectFolder))
                return "..." + value.Substring(GlobalVariables.CurrentProjectFolder.Length);

            return value;
        }
    }
}
