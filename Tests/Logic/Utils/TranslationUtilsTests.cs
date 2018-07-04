using Microsoft.VisualStudio.TestTools.UnitTesting;
using TranslatorApk.Logic.Utils;

namespace TranslatorApkTests.Logic.Utils
{
    [TestClass]
    public class TranslationUtilsTests
    {
        [TestMethod]
        public void FixOnlineTranslationTest()
        {
            (string source, string expected)[] tests = 
            {
                ("", ""),
                ("% 1 $ s", "%1$s"),
                ("%  1 $ s", "%1$s"),
                ("%  1 $  s", "%1$s"),
                ("%  1 $  S", "%1$S"),
                ("hello % a $d", "hello %a $d")
            };

            foreach (var (source, expected) in tests)
                Assert.AreEqual(expected, TranslationUtils.FixOnlineTranslation(source));
        }
    }
}
