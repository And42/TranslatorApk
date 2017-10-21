using System;
using System.Globalization;
using System.Windows;
using TranslatorApk.Logic.Classes;

namespace TranslatorApk.Logic.Converters
{
    class OptionsToVisibilityConverter : ConverterBase<Options>
    {
        protected override object Convert(Options options, Type targetType, object parameter, CultureInfo culture)
        {
            if (options.FullPath == string.Empty)
                return Visibility.Collapsed;

            bool param = parameter != null;

            if (param)
                return options.IsFolder ? Visibility.Visible : Visibility.Collapsed;
            return !options.IsFolder ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
