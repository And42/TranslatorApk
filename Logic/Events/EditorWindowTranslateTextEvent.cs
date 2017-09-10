using System;
using AndroidTranslator.Interfaces.Files;

namespace TranslatorApk.Logic.Events
{
    /// <summary>
    /// Событие вызываемое для перевода определённых строк
    /// </summary>
    public class EditorWindowTranslateTextEvent
    {
        /// <summary>
        /// Фильтр файлов для перевода
        /// </summary>
        public Func<IEditableFile, bool> Filter { get; }

        /// <summary>
        /// Текст, который нужно заменить
        /// </summary>
        public string OldText { get; }
        
        /// <summary>
        /// Текст, на который нужно заменить
        /// </summary>
        public string NewText { get; }

        /// <summary>
        /// Функция файла, возвращающая <c>True</c>, если файл не является словарём
        /// </summary>
        public static readonly Func<object, bool> NotDictionaryFileFilter = f => !(f is IDictionaryFile);

        /// <summary>
        /// Создаёт новый экземпляр класса <see cref="EditorWindowTranslateTextEvent"/> на основе исходного текста, нового текста и фильтра по файлам
        /// </summary>
        /// <param name="oldText">Текст, который нужно заменить</param>
        /// <param name="newText">Текст, на который нужно заменить</param>
        /// <param name="filter">Фильтр файлов для перевода</param>
        public EditorWindowTranslateTextEvent(string oldText, string newText, Func<IEditableFile, bool> filter = null)
        {
            Filter = filter ?? (f => true);
            OldText = oldText;
            NewText = newText;
        }
    }
}
