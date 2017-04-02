using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using TranslatorApk.Logic.Classes;
using UsefulFunctionsLib;

namespace TranslatorApk.Logic.Converters
{
    class OptionsToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Options options = value.As<Options>();

            if (options.FullPath == "")
                return Visibility.Collapsed;

            bool param = parameter != null;

            if (param)
                return options.IsFolder ? Visibility.Visible : Visibility.Collapsed;
            return !options.IsFolder ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
