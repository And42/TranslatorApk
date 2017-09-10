using AndroidTranslator.Interfaces.Files;
using AndroidTranslator.Interfaces.Strings;

namespace TranslatorApk.Logic.Events
{
    public class EditStringEvent
    {
        public IOneString StringToEdit { get; }
        public IEditableFile ContainerFile { get; }
        public string Typed { get; }
        public bool ScrollToEnd { get; }

        public EditStringEvent(IOneString str, IEditableFile containerFile = null, bool scrollToEnd = true, string prev = "")
        {
            StringToEdit = str;
            ContainerFile = containerFile;
            Typed = prev;
            ScrollToEnd = scrollToEnd;
        }
    }
}
