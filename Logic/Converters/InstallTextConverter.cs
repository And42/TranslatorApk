using System.Globalization;
using MVVM_Tools.Code.Classes;
using TranslatorApk.Logic.Classes;

namespace TranslatorApk.Logic.Converters
{
    public class InstallTextConverter : ConverterBase<InstallOptionsEnum, string>
    {
        public override string ConvertInternal(InstallOptionsEnum value, object parameter, CultureInfo culture)
        {
            switch (value)
            {
                case InstallOptionsEnum.ToUninstall:
                    return Resources.Localizations.Resources.Uninstall;
                case InstallOptionsEnum.ToInstall:
                    return Resources.Localizations.Resources.Install;
                case InstallOptionsEnum.ToUpdate:
                    return Resources.Localizations.Resources.Update;
            }

            return "Not found";
        }
    }
}
