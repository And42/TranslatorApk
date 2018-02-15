using System.Collections.Generic;
using System.Text;
using System.Web;
using TranslatorApk.Logic.OrganisationItems;

// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedMember.Local

namespace TranslatorApk.Logic.WebServices
{
    public static class GoogleTranslateService
    {
        private class GoogleTranslateResponse
        {
            public List<_sentences> sentences { get; set; }
            public int server_time { get; set; }
            public string src { get; set; }

            public class _sentences
            {
                public string trans { get; set; }
                public string orig { get; set; }
                public string translit { get; set; }
                public string src_translit { get; set; }
            }

            public override string ToString()
            {
                if (sentences == null || sentences.Count == 0) return string.Empty;
                var sb = new StringBuilder();
                foreach (_sentences str in sentences)
                    sb.Append(str.trans);
                return sb.ToString();
            }
        }

        public static string Translate(string text, string targetLanguage)
        {
            string link = "http://" + $"translate.google.com/translate_a/t?client=p&text={HttpUtility.UrlEncode(text)}&sl=auto&tl={targetLanguage}";
            string downloaded = Utils.WebUtils.DownloadString(link, SettingsIncapsuler.Instance.TranslationTimeout);
            return TranslateService.GetResponseFromJson<GoogleTranslateResponse>(downloaded).ToString();
        }
    }
}
