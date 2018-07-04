using System.Globalization;
using MVVM_Tools.Code.Classes;
using TranslatorApk.Logic.OrganisationItems;

namespace TranslatorApk.Logic.Converters
{
    public class EditorWindowFileNameConverter : ConverterBase<string, string>
    {
        public override string ConvertInternal(string value, object parameter, CultureInfo culture)
        {
            if (GlobalVariables.CurrentProjectFolder != null && value.StartsWith(GlobalVariables.CurrentProjectFolder))
                return "..." + value.Substring(GlobalVariables.CurrentProjectFolder.Length);

            return value;
        }
    }
}
