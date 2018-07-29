using System.Globalization;
using MVVM_Tools.Code.Classes;
using TranslatorApk.Logic.OrganisationItems;

namespace TranslatorApk.Logic.Converters
{
    public class EditorWindowFileNameConverter : ConverterBase<string, string>
    {
        private readonly GlobalVariables _globalVariables = GlobalVariables.Instance;

        public override string ConvertInternal(string value, object parameter, CultureInfo culture)
        {
            if (_globalVariables.CurrentProjectFolder.Value != null && value.StartsWith(_globalVariables.CurrentProjectFolder.Value))
                return "..." + value.Substring(_globalVariables.CurrentProjectFolder.Value.Length);

            return value;
        }
    }
}
