using System;
using System.IO;
using System.Net;
using System.Text;
using Newtonsoft.Json;
using TranslatorApk.Logic.OrganisationItems;

namespace TranslatorApk.Logic.WebServices
{
    public static class BaiduTranslateService
    {
        public static string Translate(string text, string targetLanguage)
        {
            return Translate(text, Detect(text), targetLanguage);
        }

        public static string Translate(string text, string sourceLanguage, string targetLanguage)
        {
            text = $"query={text}&from={sourceLanguage}&to={targetLanguage}&transtype=trans&simple_means_flag=3";

            string resp = UploadString("http://fanyi.baidu.com/v2transapi", text);

            var obj = new
            {
                trans_result = new
                {
                    status = 0,
                    data = new []
                    {
                        new { dst = "\u043a\u0430\u043a \u0442\u044b?\u044f"}
                    }
                }
            };

            var convResp = JsonConvert.DeserializeAnonymousType(resp, obj);
            if (convResp.trans_result.status != 0)
                throw new Exception(resp);

            return convResp.trans_result.data[0].dst;
        }

        public static string Detect(string text)
        {
            text = "query=" + text;

            string resp = UploadString("http://fanyi.baidu.com/langdetect", text);

            var obj = new
            {
                error = 0,
                msg = "success",
                lan = "en"
            };

            var convResp = JsonConvert.DeserializeAnonymousType(resp, obj);
            if (convResp.msg != "success")
                throw new Exception("Detect error");

            return convResp.lan;
        }

        private static string UploadString(string uri, string text)
        {
            var request = (HttpWebRequest) WebRequest.Create(uri);

            request.Headers = new WebHeaderCollection
            {
                {HttpRequestHeader.AcceptCharset, "utf-8"}
            };

            request.UserAgent = GlobalVariables.MozillaAgent;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded; charset=utf-8";

            var bytes = Encoding.UTF8.GetBytes(text);
            request.ContentLength = bytes.Length;
            request.GetRequestStream().Write(bytes, 0, bytes.Length);

            var responseStream = request.GetResponse().GetResponseStream();

            if (responseStream == null)
                return string.Empty;

            using (responseStream)
                using (var reader = new StreamReader(responseStream))
                    return reader.ReadToEnd();
        }
    }
}
