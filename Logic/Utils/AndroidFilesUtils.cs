using System.IO;
using System.Xml;
using AndroidTranslator.Classes.Exceptions;
using AndroidTranslator.Classes.Files;
using AndroidTranslator.Interfaces.Files;
using TranslatorApk.Logic.OrganisationItems;

namespace TranslatorApk.Logic.Utils
{
    internal static class AndroidFilesUtils
    {
        /// <summary>
        /// Возвращает изменяемый файл, соответствующий файлу на диске, или null, если подходящий файл не найден
        /// </summary>
        /// <param name="filePath">Путь к файлу на диске</param>
        public static IEditableFile GetSuitableEditableFile(string filePath)
        {
            switch (Path.GetExtension(filePath))
            {
                case ".xml":
                    if (IsDictionaryFile(filePath))
                        return new DictionaryFile(filePath);

                    return XmlFile.Create(filePath);
                case ".smali":
                    return new SmaliFile(filePath);
            }

            return null;
        }

        /// <summary>
        /// Проверяет, являются ли данные в указанном потоке словарём
        /// </summary>
        /// <param name="stream">Поток</param>
        public static bool IsDictionaryFile(Stream stream)
        {
            var xDoc = new XmlDocument();

            try
            {
                xDoc.Load(stream);
            }
            catch (XmlException)
            {
                return false;
            }

            XmlElement docElem = xDoc.DocumentElement;

            return docElem?.Name == "translations" && docElem.Attributes["name"]?.Value == "AeDict";
        }

        /// <summary>
        /// Проверяет, является ли указанный файл словарём
        /// </summary>
        /// <param name="file">Файл</param>
        public static bool IsDictionaryFile(string file)
        {
            if (Path.GetExtension(file) != ".xml")
                return false;

            using (FileStream stream = File.OpenRead(file))
                return IsDictionaryFile(stream);
        }

        /// <summary>
        /// Проверяет текущий файл на соответствие настройкам
        /// </summary>
        /// <param name="file">Файл</param>
        /// <param name="extension">Расширение файла</param>
        public static bool CheckFileWithSettings(string file, string extension)
        {
            if (extension != ".xml" && SettingsIncapsuler.Instance.OnlyXml)
                return false;

            if (extension == ".xml")
            {
                if (!SettingsIncapsuler.Instance.EmptyXml)
                {
                    try
                    {
                        var details = XmlFile.Create(file).Details;

                        return details != null && details.Count != 0;
                    }
                    catch (XmlParserException)
                    {
                        return false;
                    }
                }
            }
            else if (extension == ".smali")
            {
                if (!SettingsIncapsuler.Instance.EmptySmali && !SmaliFile.HasLines(file))
                    return false;
            }
            else if (SettingsIncapsuler.Instance.ImageExtensions.Contains(extension))
            {
                if (!SettingsIncapsuler.Instance.Images)
                    return false;
            }
            else if (SettingsIncapsuler.Instance.OtherExtensions.Contains(extension))
            {
                if (!SettingsIncapsuler.Instance.OtherFiles)
                    return false;
            }

            return true;
        }
    }
}
