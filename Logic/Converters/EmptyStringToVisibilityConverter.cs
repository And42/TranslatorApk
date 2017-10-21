using System;
using System.Globalization;
using System.Windows;

namespace TranslatorApk.Logic.Converters
{
    public class EmptyStringToVisibilityConverter: ConverterBase<string>
    {
        protected override object Convert(string value, Type targetType, object parameter, CultureInfo culture)
        {
            return string.IsNullOrEmpty(value) ? Visibility.Collapsed : Visibility.Visible;
        }
    }
}
