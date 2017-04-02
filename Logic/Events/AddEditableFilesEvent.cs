using System.Collections.Generic;
using AndroidTranslator;

namespace TranslatorApk.Logic.Events
{
    public class AddEditableFilesEvent
    {
        public IList<EditableFile> Files { get; }
        public bool ClearExisting { get; }

        public AddEditableFilesEvent(IList<EditableFile> files, bool clearExisting = true)
        {
            Files = files;
            ClearExisting = clearExisting;
        }

        public AddEditableFilesEvent(EditableFile file, bool clearExisting = true): this(new [] {file}, clearExisting) { }
    }
}
