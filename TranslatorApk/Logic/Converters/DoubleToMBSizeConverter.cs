using System.Globalization;
using MVVM_Tools.Code.Classes;

namespace TranslatorApk.Logic.Converters
{
    public sealed class DoubleToMbSizeConverter : ConverterBase<double, string>
    {
        public override string ConvertInternal(double value, object parameter, CultureInfo culture)
        {
            return value.ToString("0,0", CultureInfo.InvariantCulture);
        }
    }
}
