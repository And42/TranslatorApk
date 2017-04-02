using System;
using System.Globalization;
using System.Windows.Data;
using TranslatorApk.Logic.Classes;
using UsefulFunctionsLib;

namespace TranslatorApk.Logic.Converters
{
    public class InstallTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (value.As<InstallOptionsEnum>())
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

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
