using System.Globalization;
using System.IO;
using MVVM_Tools.Code.Classes;

namespace TranslatorApk.Logic.Converters
{
    public class WindowTitleConverter : ConverterBase<string, string>
    {
        public override string ConvertInternal(string value, object parameter, CultureInfo culture)
        {
            return value == null ? string.Empty : " - " + Path.GetFileName(value);
        }
    }
}
