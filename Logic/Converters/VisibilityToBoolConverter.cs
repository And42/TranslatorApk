using System.Globalization;
using System.Windows;
using MVVM_Tools.Code.Classes;

namespace TranslatorApk.Logic.Converters
{
    class VisibilityToBoolConverter : ConverterBase<Visibility, string, bool>
    {
        public override bool ConvertInternal(Visibility value, string parameter, CultureInfo culture)
        {
            bool val = value == Visibility.Visible;
            return parameter.ToUpper() == "TRUE" ? !val : val;
        }
    }
}
