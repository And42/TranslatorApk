using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using UsefulFunctionsLib;

namespace TranslatorApk.Logic.Converters
{
    class VisibilityToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool val = value.As<Visibility>() == Visibility.Visible;
            return parameter.As<string>().ToUpper() == "TRUE" ? !val : val;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
