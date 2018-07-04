using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.Utils;

namespace TranslatorApk.Logic.WebServices
{
    public static class TranslateService
    {
        #region Списки языков

        #region Список сокращений языков для онлайн перевода

        /// <summary>
        /// Список сокращений языков для онлайн перевода
        /// </summary>
        public static ReadOnlyCollection<string> ShortTargetLanguages = 
            new ReadOnlyCollection<string>(Properties.Resources.OnlineTranslationsShortLanguages.Split('|'));

        /// <summary>
        /// Список сокращений языков для онлайн перевода
        /// </summary>
        public static ReadOnlyCollection<string> LongTargetLanguages;

        #endregion

        #endregion

        /// <summary>
        /// Список онлайн переводчиков
        /// </summary>
        public static readonly Dictionary<Guid, OneTranslationService> OnlineTranslators =
            new Dictionary<Guid, OneTranslationService>();

        /// <summary>
        /// Двусимвольный список доступных языков для интерфейса
        /// </summary>
        public static readonly ReadOnlyCollection<string> SupportedProgramLangs =
            new ReadOnlyCollection<string>(new[] { "ru", "en-US", "de", "lt", "vi-VN", "uk-UA" });

        /// <summary>
        /// Список доступных языков для интерфейса
        /// </summary>
        public static readonly ReadOnlyCollection<string> SupportedProgramLangsFullNames =
            new ReadOnlyCollection<string>(new[] { "Русский", "English", "Deutch", "Lietuvių", "Việt", "Українська" });

        static TranslateService()
        {
            //OnlineTranslators.Add("Yandex", TranslateWithYandex);
            var gGuid = Guid.Parse("ec9ecfae-ee23-482e-b96d-e616c518c16f");
            var yGuid = Guid.Parse("ddc03c26-9ca9-481d-bbb6-7473ee5a1319");
            var bGuid = Guid.Parse("6d01675a-7e6a-4618-8eb4-78fbf8d85875");

            OnlineTranslators.Add(gGuid, new OneTranslationService("Google", TranslateWithGoogleSecond, gGuid));
            OnlineTranslators.Add(yGuid, new OneTranslationService("Yandex (API)", TranslateWithYandexApi, yGuid));
            OnlineTranslators.Add(bGuid, new OneTranslationService("Baidu", TranslateWithBaidu, bGuid));
        }

        /// <summary>
        /// Возвращает двусимвольное название языка
        /// </summary>
        /// <param name="longName">Полное название языка</param>
        // ReSharper disable once InconsistentNaming
        public static string GetShortTL(string longName)
        {
            return ShortTargetLanguages[LongTargetLanguages.IndexOf(longName)];
        }

        /// <summary>
        /// Возвращает полное названия языка
        /// </summary>
        /// <param name="shortName">Двусимвольное название языка</param>
        // ReSharper disable once InconsistentNaming
        public static string GetLongTL(string shortName)
        {
            return LongTargetLanguages[ShortTargetLanguages.IndexOf(shortName)];
        }

        /// <summary>
        /// Возвращает текст, переведённый через яндекс переводчик
        /// </summary>
        /// <param name="text">Текст для перевода</param>
        /// <param name="targetLanguage">Целевой язык (сокращение)</param>
        public static string TranslateWithYandex(string text, string targetLanguage)
        {
            string result;
            try
            {
                result = YandexTranslateService.Translate(text, targetLanguage);
            }
            catch (Exception e)
            {
                throw new Exception("Can't translate. Reason: " + e.Message);
            }
            return result;
        }

        /// <summary>
        /// Возвращает текст, переведённый через яндекс переводчик
        /// </summary>
        /// <param name="text">Текст для перевода</param>
        /// <param name="targetLanguage">Целевой язык (сокращение)</param>
        /// <param name="apiKey">Ключ api</param>
        public static string TranslateWithYandexApi(string text, string targetLanguage, string apiKey)
        {
            string result;
            try
            {
                result = YandexTranslateService.TranslateApi(text, targetLanguage, apiKey);
            }
            catch (Exception e)
            {
                throw new Exception("Can't translate. Reason: " + e.Message);
            }
            return result;
        }

        /// <summary>
        /// Возвращает текст, переведённый через baidu переводчик
        /// </summary>
        /// <param name="text">Текст для перевода</param>
        /// <param name="targetLanguage">Целевой язык (сокращение)</param>
        /// <param name="apiKey">Ключ api</param>
        public static string TranslateWithBaidu(string text, string targetLanguage, string apiKey = null)
        {
            string result;
            try
            {
                result = BaiduTranslateService.Translate(text, targetLanguage);
            }
            catch (Exception e)
            {
                throw new Exception("Can't translate. Reason: " + e.Message);
            }
            return result;
        }

        /// <summary>
        /// Возвращает текст, переведённый через google переводчик
        /// </summary>
        /// <param name="text">Текст для перевода</param>
        /// <param name="targetLanguage">Целевой язык (сокращение)</param>
        /// <param name="apiKey">Ключ api (не требуется)</param>
        public static string TranslateWithGoogleSecond(string text, string targetLanguage, string apiKey = null)
        {
            string result;
            try
            {
                result = GoogleTranslateServiceSecond.Translate(text, targetLanguage);
            }
            catch (Exception e)
            {
                throw new Exception("Can't translate. Reason: " + e.Message);
            }
            return result;
        }

        /// <summary>
        /// Возвращает текст, переведённый через google переводчик
        /// </summary>
        /// <param name="text">Текст для перевода</param>
        /// <param name="targetLanguage">Целевой язык (сокращение)</param>
        public static string TranslateWithGoogle(string text, string targetLanguage)
        {
            string result;
            try
            {
                result = GoogleTranslateService.Translate(text, targetLanguage);
            }
            catch (Exception e)
            {
                throw new Exception("Can't translate. Reason: " + e.Message);
            }
            return result;
        }

        public static string TranslateWithBabylon(string text, int sourceLanguage, int targetLanguage)
        {
            string link =
                "http://" + $"translation.babylon.com/translate/babylon.php?v=1.0&q={HttpUtility.UrlEncode(text)}&langpair={sourceLanguage}|{targetLanguage}&callback=callbackFn&context=babylon";
            return 
                WebUtils.DownloadString(link, DefaultSettingsContainer.Instance.TranslationTimeout)
                    .Split('{')[1].Split('}')[0].SplitRemove('"')
                    .Last();
        }

        /// <summary>
        /// Парсит объект из json
        /// </summary>
        /// <typeparam name="TResponseType">Тип объекта</typeparam>
        /// <param name="data">Строка формата json</param>
        public static TResponseType GetResponseFromJson<TResponseType>(string data)
        {
            return JsonConvert.DeserializeObject<TResponseType>(data);
        }
    }
}