using System.Web;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.WebJsonResponses;

namespace TranslatorApk.Logic.WebServices
{
    public static class YandexDetectService
    {
        public static YandexDetectResponse Detect(string text)
        {
            string link = "http://translate.yandex.net/api/v1.5/tr.json/detect?srv=tr-text&text=" +
                          HttpUtility.UrlEncode(text);
            return TranslateService.GetResponseFromJson<YandexDetectResponse>(Utils.WebUtils.DownloadString(link, SettingsIncapsuler.Instance.TranslationTimeout));
        }
    }
}
