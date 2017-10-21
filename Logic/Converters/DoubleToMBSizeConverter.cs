using System;
using System.Globalization;

namespace TranslatorApk.Logic.Converters
{
    public sealed class DoubleToMbSizeConverter : ConverterBase<double>
    {
        protected override object Convert(double value, Type targetType, object parameter, CultureInfo culture)
        {
            return value.ToString("0,0", CultureInfo.InvariantCulture);
        }
    }
}
