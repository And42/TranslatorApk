using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using UsefulFunctionsLib;

namespace TranslatorApk.Logic.Converters
{
    public class WindowTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? string.Empty : " - " + Path.GetFileName(value.As<string>());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
