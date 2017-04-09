using System;
using AndroidTranslator;

namespace TranslatorApk.Logic.Events
{
    /// <summary>
    /// Событие, вызываемое для прокрутки окна редактора к строке и её выбора
    /// </summary>
    public class EditorScrollToStringAndSelectEvent
    {
        /// <summary>
        /// Условие для выбора файла
        /// </summary>
        public Predicate<EditableFile> FilePredicate { get; }

        /// <summary>
        /// Условие для выбора строки
        /// </summary>
        public Predicate<OneString> StringPredicate { get; }

        /// <summary>
        /// Создаёт новый экземпляр класса <see cref="EditorScrollToStringAndSelectEvent"/> на основе условий для файла и строки
        /// </summary>
        /// <param name="filePredicate">Условие для выбора файла</param>
        /// <param name="stringPredicate">Условие для выбора строки</param>
        public EditorScrollToStringAndSelectEvent(Predicate<EditableFile> filePredicate, Predicate<OneString> stringPredicate)
        {
            FilePredicate = filePredicate ?? throw new ArgumentNullException(nameof(filePredicate));
            StringPredicate = stringPredicate ?? throw new ArgumentNullException(nameof(stringPredicate));
        }
    }
}
