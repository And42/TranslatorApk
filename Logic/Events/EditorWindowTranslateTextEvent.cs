using System;
using AndroidTranslator;

namespace TranslatorApk.Logic.Events
{
    public class EditorWindowTranslateTextEvent
    {
        public Func<EditableFile, bool> Filter { get; }
        public string OldText { get; }
        public string NewText { get; }

        public static readonly Func<EditableFile, bool> NotDictionaryFileFilter = f => !(f is DictionaryFile);

        public EditorWindowTranslateTextEvent(string oldText, string newText, Func<EditableFile, bool> filter = null)
        {
            Filter = filter ?? (f => true);
            OldText = oldText;
            NewText = newText;
        }
    }
}
