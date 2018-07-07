using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Resources.Localizations;

namespace TranslatorApk.Logic.WebServices
{
    public static class YandexTranslateService
    {
        private class YandexTranslateResponse
        {
            [JsonProperty("code")]
            public int Code;

            [JsonProperty("lang")]
            public string Lang;

            [JsonProperty("text")]
            public List<string> Text;

            public override string ToString()
            {
                var sb = new StringBuilder();

                foreach (string str in Text)
                    sb.Append(str);

                return sb.ToString();
            }
        }

        public static string TranslateApi(string text, string targetLanguage, string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new Exception(StringResources.ApiKeyIsEmpty);

            string link = "https://" + $"translate.yandex.net/api/v1.5/tr.json/translate?key={apiKey}&lang={targetLanguage}&text={text}";
            string downloaded = Utils.WebUtils.DownloadString(link, GlobalVariables.AppSettings.TranslationTimeout);

            return JsonConvert.DeserializeObject<YandexTranslateResponse>(downloaded).ToString();
        }

        public static string Translate(string text, string targetLanguage)
        {
            string link = "https://" + $"translate.yandex.net/api/v1/tr.json/translate?lang={targetLanguage}&text={text}&srv=tr-text&id=55";
            var client = new WebClient
            {
                Headers = new WebHeaderCollection
                {
                    { HttpRequestHeader.Referer, "http://translate.yandex.net" },
                    { HttpRequestHeader.UserAgent, "Mozilla/4.0 (compatible; MSIE 6.0b; Windows NT 5.1)" }
                },
                Encoding = Encoding.UTF8
            };
            string downloaded = client.DownloadString(link);

            return JsonConvert.DeserializeObject<YandexTranslateResponse>(downloaded).ToString();
        }
    }
}
