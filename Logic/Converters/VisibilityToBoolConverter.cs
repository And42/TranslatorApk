using System.Globalization;
using System.Windows;
using MVVM_Tools.Code.Classes;
using UsefulFunctionsLib;

namespace TranslatorApk.Logic.Converters
{
    class VisibilityToBoolConverter : ConverterBase<Visibility, bool>
    {
        public override bool ConvertInternal(Visibility value, object parameter, CultureInfo culture)
        {
            bool val = value == Visibility.Visible;
            return parameter.As<string>().ToUpper() == "TRUE" ? !val : val;
        }
    }
}
