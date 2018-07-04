using System;
using System.Globalization;
using System.Windows.Data;

namespace TranslatorApk.Logic.Converters
{
    class PercentageTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var progress = (int) values[0];
            var progressMax = (int) values[1];

            int currentProgress = (int) (100.0 * progress / progressMax);

            return (currentProgress >= 0 && currentProgress <= 100 ? currentProgress : 0) + "%";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
