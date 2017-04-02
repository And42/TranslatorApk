using System;
using AndroidTranslator;

namespace TranslatorApk.Logic.Events
{
    public class EditorScrollToStringAndSelectEvent
    {
        public Predicate<EditableFile> FilePredicate { get; }
        public Predicate<OneString> StringPredicate { get; }

        public EditorScrollToStringAndSelectEvent(Predicate<EditableFile> filePredicate, Predicate<OneString> stringPredicate)
        {
            FilePredicate = filePredicate ?? throw new ArgumentNullException(nameof(filePredicate));
            StringPredicate = stringPredicate ?? throw new ArgumentNullException(nameof(stringPredicate));
        }
    }
}
