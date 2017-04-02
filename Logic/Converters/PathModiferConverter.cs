using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;

namespace TranslatorApk.Logic.Converters
{
    public class PathModiferConverter : IValueConverter
    {
        public enum ConvertOptions
        {
            PathAndName,
            NameAndExt,
            Name
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string val = (string) value;

            ConvertOptions param = parameter == null ? ConvertOptions.Name : (ConvertOptions) Enum.Parse(typeof(ConvertOptions), parameter.ToString());

            switch (param)
            {
                case ConvertOptions.Name:
                    return Path.GetFileNameWithoutExtension(val);
                case ConvertOptions.NameAndExt:
                    return Path.GetFileName(val);
                case ConvertOptions.PathAndName:
                    return UsefulFunctionsLib.UsefulFunctions_FileInfo.GetFullFNWithoutExt(val);
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
