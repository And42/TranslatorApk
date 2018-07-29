using TranslatorApk.Logic.OrganisationItems;
using AndroidTranslator.Interfaces.Strings;

namespace TranslatorApk.Logic.Classes
{
    public sealed class OneFoundItem
    {
        private readonly GlobalVariables _globalVariables = GlobalVariables.Instance;

        public string FileName { get; }

        public string Text { get; }

        public string FormattedName { get; }

        public IOneString EditString { get; }

        public OneFoundItem(string fileName, string text, IOneString str)
        {
            FileName = fileName;
            FormattedName = 
                _globalVariables.CurrentProjectFolder.Value == null 
                    ? fileName
                    : "..." + fileName.Remove(0, _globalVariables.CurrentProjectFolder.Value.Length);
            Text = text;
            EditString = str;
        }
    }
}
