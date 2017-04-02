namespace TranslatorApk.Logic.Events
{
    public class ActivateEditorWindowEvent
    {
        public bool CreateIfNotExists { get; }

        public ActivateEditorWindowEvent(bool createIfNotExists)
        {
            CreateIfNotExists = createIfNotExists;
        }
    }
}
