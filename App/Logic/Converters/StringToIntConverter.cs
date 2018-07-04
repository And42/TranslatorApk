using System.Globalization;
using MVVM_Tools.Code.Classes;

namespace TranslatorApk.Logic.Converters
{
    class StringToIntConverter : ConverterBase<string, int>
    {
        public override int ConvertInternal(string value, object parameter, CultureInfo culture)
        {
            return int.Parse(value);
        }

        public override string ConvertBackInternal(int value, object parameter, CultureInfo culture)
        {
            return value.ToString();
        }
    }
}
