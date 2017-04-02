using System.Web;
using Newtonsoft.Json.Linq;

namespace TranslatorApk.Logic.WebServices
{
    public static class GoogleTranslateServiceSecond
    {
        public static string Translate(string text, string targetLanguage)
        {
            string link = "http://" + $"translate.googleapis.com/translate_a/single?client=gtx&sl=en&tl={targetLanguage}&dt=t&q={HttpUtility.UrlEncode(text)}";
            string downloaded = Functions.DownloadString(link);

            var obj = JArray.Parse(downloaded);

            var translated = obj[0][0][0].ToString();

            return translated;
        }
    }
}
