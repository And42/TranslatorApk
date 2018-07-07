using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace TranslatorApk.Logic.Utils
{
    internal static class JsonUtils
    {
        private static readonly JsonSerializer JsonSerializer = new JsonSerializer
        {
            Formatting = Formatting.Indented
        };

        public static T DeserializeFromFile<T>(string filePath)
        {
            using (var reader = new StreamReader(filePath, Encoding.UTF8))
            using (var jsonReader = new JsonTextReader(reader))
            {
                return JsonSerializer.Deserialize<T>(jsonReader);
            }
        }

        public static void SerializeToFile(object obj, string filePath)
        {
            var fileDir = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(fileDir))
                Directory.CreateDirectory(fileDir);

            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                JsonSerializer.Serialize(writer, obj);
            }
        }
    }
}
