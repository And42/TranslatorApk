using Microsoft.VisualStudio.TestTools.UnitTesting;
using TranslatorApk.Logic.WebServices;

namespace TranslatorApkTests
{
    [TestClass]
    public class TranslateTests
    {
        [TestMethod]
        public void YandexTranslateWithApi()
        {
            string translated = YandexTranslateService.TranslateApi("Hello", "ru",
                "trnsl.1.1.20130715T181535Z.c1b04b19ecc33ab4.853ee79af0aac1609fe60d8f74341e6ef680ebca");

            Assert.AreEqual("Привет", translated);
        }

        /*[TestMethod]
        public void YandexTranslate()
        {
            string translated = YandexTranslateService.Translate("Hello", "ru");

            Assert.AreEqual("Привет", translated);
        }*/

        /*[TestMethod]
        public void BabylonTranslate()
        {
            string translated = TranslateService.TranslateWithBabylon("Hello", 1, 0);

            Assert.AreEqual("Привет", translated);
        }*/

        [TestMethod]
        public void BaiduDetect()
        {
            string detected = BaiduTranslateService.Detect("Hello");

            Assert.AreEqual("en", detected);
        }

        [TestMethod]
        public void BaiduTranslate()
        {
            string translated = BaiduTranslateService.Translate("Hello", "ru");

            Assert.AreEqual("привет", translated);
        }

        [TestMethod]
        public void GoogleTranslate()
        {
            string translated = GoogleTranslateServiceSecond.Translate("Hello", "ru");

            Assert.AreEqual("Здравствуйте", translated);
        }
    }
}
