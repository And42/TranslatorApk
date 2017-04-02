using System;
using AndroidTranslator;

namespace TranslatorApk.Logic.Events
{
    public class EditorScrollToFileAndSelectEvent
    {
        public Predicate<EditableFile> FilePredicate { get; }
        public bool ExpandRecord { get; }

        public EditorScrollToFileAndSelectEvent(Predicate<EditableFile> filePredicate, bool expandRecord = true)
        {
            FilePredicate = filePredicate ?? throw new ArgumentNullException(nameof(filePredicate));
            ExpandRecord = expandRecord;
        }
    }
}
