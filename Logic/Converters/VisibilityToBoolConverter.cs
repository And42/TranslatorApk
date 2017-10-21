using System;
using System.Globalization;
using System.Windows;
using UsefulFunctionsLib;

namespace TranslatorApk.Logic.Converters
{
    class VisibilityToBoolConverter : ConverterBase<Visibility>
    {
        protected override object Convert(Visibility value, Type targetType, object parameter, CultureInfo culture)
        {
            bool val = value == Visibility.Visible;
            return parameter.As<string>().ToUpper() == "TRUE" ? !val : val;
        }
    }
}
