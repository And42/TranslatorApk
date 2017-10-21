using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using TranslatorApk.Logic.OrganisationItems;

// ReSharper disable InconsistentNaming
// ReSharper disable CollectionNeverUpdated.Local
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable MemberCanBePrivate.Local

#pragma warning disable 169
#pragma warning disable 649

namespace TranslatorApk.Logic.WebServices
{
    public static class YandexTranslateService
    {
        private class YandexTranslateResponse
        {
            public int code;
            public string lang;
            public List<string> text;

            public override string ToString()
            {
                var sb = new StringBuilder();

                foreach (string str in text)
                    sb.Append(str);

                return sb.ToString();
            }
        }

        public static string TranslateApi(string text, string targetLanguage, string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
                throw new Exception(Resources.Localizations.Resources.ApiKeyIsEmpty);

            string link = "https://" + $"translate.yandex.net/api/v1.5/tr.json/translate?key={apiKey}&lang={targetLanguage}&text={text}";
            string downloaded = Utils.DownloadString(link);

            return TranslateService.GetResponseFromJson<YandexTranslateResponse>(downloaded).ToString();
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

            return TranslateService.GetResponseFromJson<YandexTranslateResponse>(downloaded).ToString();
        }
    }
}
