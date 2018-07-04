using System;

namespace TranslatorApk.Logic.Classes
{
    public class LogItem
    {
        public enum LogItemType
        {
            Info,
            Warning,
            ErrorW,
            Error
        }

        public string Message { get; }

        public DateTime Date { get; }

        public LogItemType ItemType { get; }

        public LogItem(string message, LogItemType itemType = LogItemType.Info)
        {
            Message = message;
            Date = DateTime.Now;
            ItemType = itemType;
        }
    }
}
