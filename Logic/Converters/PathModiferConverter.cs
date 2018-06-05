using System;
using System.Globalization;
using System.IO;
using MVVM_Tools.Code.Classes;

namespace TranslatorApk.Logic.Converters
{
    public class PathModiferConverter : ConverterBase<string, string>
    {
        public enum ConvertOptions
        {
            PathAndName,
            NameAndExt,
            Name
        }

        public override string ConvertInternal(string value, object parameter, CultureInfo culture)
        {
            ConvertOptions param = parameter == null ? ConvertOptions.Name : (ConvertOptions) Enum.Parse(typeof(ConvertOptions), parameter.ToString());

            switch (param)
            {
                case ConvertOptions.Name:
                    return Path.GetFileNameWithoutExtension(value);
                case ConvertOptions.NameAndExt:
                    return Path.GetFileName(value);
                case ConvertOptions.PathAndName:
                    return value.Remove(value.Length - Path.GetExtension(value).Length);
            }

            return null;
        }
    }
}
