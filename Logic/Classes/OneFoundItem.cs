using AndroidTranslator;

namespace TranslatorApk.Logic.Classes
{
    public sealed class OneFoundItem
    {
        public string FileName { get; }

        public string Text { get; }

        public string FormattedName { get; }

        public OneString EditString { get; }

        public OneFoundItem(string fileName, string text, OneString str)
        {
            FileName = fileName;
            FormattedName = "..." + fileName.Remove(0, GlobalVariables.CurrentProjectFolder.Length);
            Text = text;
            EditString = str;
        }
    }
}
