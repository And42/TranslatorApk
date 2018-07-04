using System.Globalization;
using MVVM_Tools.Code.Classes;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Resources.Localizations;

namespace TranslatorApk.Logic.Converters
{
    public class InstallTextConverter : ConverterBase<InstallOptionsEnum, string>
    {
        public override string ConvertInternal(InstallOptionsEnum value, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case InstallOptionsEnum.ToUninstall:
                    return StringResources.Uninstall;
                case InstallOptionsEnum.ToInstall:
                    return StringResources.Install;
                case InstallOptionsEnum.ToUpdate:
                    return StringResources.Update;
            }

            return "Not found";
        }
    }
}
