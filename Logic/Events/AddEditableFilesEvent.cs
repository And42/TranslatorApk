using System.Collections.Generic;
using AndroidTranslator.Interfaces.Files;

namespace TranslatorApk.Logic.Events
{
    /// <summary>
    /// Событие, вызываемое для добавления файлов в список файлов редактора
    /// </summary>
    public class AddEditableFilesEvent
    {
        /// <summary>
        /// Список добавляемых файлов
        /// </summary>
        public IList<IEditableFile> Files { get; }

        /// <summary>
        /// Определяет необходимость очистки текущего списка
        /// </summary>
        public bool ClearExisting { get; }

        /// <summary>
        /// Создаёт новый экземпляр класса <see cref="AddEditableFilesEvent"/> на основе изменяемых файлов и параметра очистки
        /// </summary>
        /// <param name="files">Список добавляемых файлов</param>
        /// <param name="clearExisting">Определяет необходимость очистки текущего списка</param>
        public AddEditableFilesEvent(IList<IEditableFile> files, bool clearExisting = true)
        {
            Files = files;
            ClearExisting = clearExisting;
        }

        /// <summary>
        /// Создаёт новый экземпляр класса <see cref="AddEditableFilesEvent"/> на основе изменяемого файла и параметра очистки
        /// </summary>
        /// <param name="file">Добавляемый файл</param>
        /// <param name="clearExisting">Определяет необходимость очистки текущего списка</param>
        public AddEditableFilesEvent(IEditableFile file, bool clearExisting = true): this(new [] {file}, clearExisting) { }
    }
}
