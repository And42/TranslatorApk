using System.Collections.Generic;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.Utils;

namespace TranslatorApk.Logic.WebServices
{
    public static class GoogleTranslateService
    {
        private class GoogleTranslateResponseJson
        {
            [JsonProperty("sentences")]
            public List<SentenceJson> Sentences { get; set; }

            [JsonProperty("server_time")]
            public int ServerTime { get; set; }

            [JsonProperty("src")]
            public string Src { get; set; }

            public class SentenceJson
            {
                [JsonProperty("trans")]
                public string Translated { get; set; }

                [JsonProperty("orig")]
                public string Original { get; set; }

                [JsonProperty("translit")]
                public string Translit { get; set; }

                [JsonProperty("src_translit")]
                public string SrcTranslit { get; set; }
            }

            public override string ToString()
            {
                if (Sentences == null || Sentences.Count == 0)
                    return string.Empty;

                var sb = new StringBuilder();
                foreach (SentenceJson str in Sentences)
                    sb.Append(str.Translated);

                return sb.ToString();
            }
        }

        public static string Translate(string text, string targetLanguage)
        {
            string link = "http://" + $"translate.google.com/translate_a/t?client=p&text={HttpUtility.UrlEncode(text)}&sl=auto&tl={targetLanguage}";
            string downloaded = WebUtils.DownloadString(link, GlobalVariables.AppSettings.TranslationTimeout);
            return JsonConvert.DeserializeObject<GoogleTranslateResponseJson>(downloaded).ToString();
        }
    }
}
