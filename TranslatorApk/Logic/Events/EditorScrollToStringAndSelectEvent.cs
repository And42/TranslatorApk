using System;
using AndroidTranslator.Interfaces.Files;
using AndroidTranslator.Interfaces.Strings;

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
        public Predicate<IEditableFile> FilePredicate { get; }

        /// <summary>
        /// Условие для выбора строки
        /// </summary>
        public Predicate<IOneString> StringPredicate { get; }

        /// <summary>
        /// Создаёт новый экземпляр класса <see cref="EditorScrollToStringAndSelectEvent"/> на основе условий для файла и строки
        /// </summary>
        /// <param name="filePredicate">Условие для выбора файла</param>
        /// <param name="stringPredicate">Условие для выбора строки</param>
        public EditorScrollToStringAndSelectEvent(Predicate<IEditableFile> filePredicate, Predicate<IOneString> stringPredicate)
        {
            FilePredicate = filePredicate ?? throw new ArgumentNullException(nameof(filePredicate));
            StringPredicate = stringPredicate ?? throw new ArgumentNullException(nameof(stringPredicate));
        }
    }
}
