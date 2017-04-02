using System.Collections.Generic;
using AndroidTranslator;

namespace TranslatorApk.Logic.Events
{
    public class EditFilesEvent
    {
        public IList<EditableFile> Files { get; }
        public bool ClearExisting { get; }

        public EditFilesEvent(IList<EditableFile> files, bool clearExisting = true)
        {
            Files = files;
            ClearExisting = clearExisting;
        }

        public EditFilesEvent(EditableFile file, bool clearExisting = true): this(new [] {file}, clearExisting) { }
    }
}
