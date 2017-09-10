using System;
using AndroidTranslator.Interfaces.Files;

namespace TranslatorApk.Logic.Events
{
    /// <summary>
    /// Событие, вызываемое для прокрутки окна редактора к файлу и его выбора
    /// </summary>
    public class EditorScrollToFileAndSelectEvent
    {
        /// <summary>
        /// Условие для выбора файла
        /// </summary>
        public Predicate<IEditableFile> FilePredicate { get; }

        /// <summary>
        /// Определяет необходимость разворачивания найденной записи
        /// </summary>
        public bool ExpandRecord { get; }

        /// <summary>
        /// Создаёт новый экземпляр класса <see cref="EditorScrollToFileAndSelectEvent"/> на основе условия для файла и значения для разворачивания записи
        /// </summary>
        /// <param name="filePredicate">Условие для выбора файла</param>
        /// <param name="expandRecord">Определяет необходимость разворачивания найденной записи</param>
        public EditorScrollToFileAndSelectEvent(Predicate<IEditableFile> filePredicate, bool expandRecord = true)
        {
            FilePredicate = filePredicate ?? throw new ArgumentNullException(nameof(filePredicate));
            ExpandRecord = expandRecord;
        }
    }
}
