using TranslatorApk.Logic.OrganisationItems;
using AndroidTranslator.Interfaces.Strings;

namespace TranslatorApk.Logic.Classes
{
    public sealed class OneFoundItem
    {
        public string FileName { get; }

        public string Text { get; }

        public string FormattedName { get; }

        public IOneString EditString { get; }

        public OneFoundItem(string fileName, string text, IOneString str)
        {
            FileName = fileName;
            FormattedName = 
                GlobalVariables.CurrentProjectFolder == null 
                    ? fileName
                    : "..." + fileName.Remove(0, GlobalVariables.CurrentProjectFolder.Length);
            Text = text;
            EditString = str;
        }
    }
}
