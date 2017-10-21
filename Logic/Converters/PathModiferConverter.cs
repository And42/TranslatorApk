using System;
using System.Globalization;
using System.IO;

namespace TranslatorApk.Logic.Converters
{
    public class PathModiferConverter : ConverterBase<string>
    {
        public enum ConvertOptions
        {
            PathAndName,
            NameAndExt,
            Name
        }

        protected override object Convert(string value, Type targetType, object parameter, CultureInfo culture)
        {
            ConvertOptions param = parameter == null ? ConvertOptions.Name : (ConvertOptions) Enum.Parse(typeof(ConvertOptions), parameter.ToString());

            switch (param)
            {
                case ConvertOptions.Name:
                    return Path.GetFileNameWithoutExtension(value);
                case ConvertOptions.NameAndExt:
                    return Path.GetFileName(value);
                case ConvertOptions.PathAndName:
                    return UsefulFunctionsLib.UsefulFunctions_FileInfo.GetFullFNWithoutExt(value);
            }

            return null;
        }
    }
}
