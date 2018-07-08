using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TranslatorApk.Logic.Utils;

namespace TranslatorApkTests.Logic
{
    [TestClass]
    public class FunctionsTest
    {
        [TestMethod]
        public void CompareVersionsTest()
        {
            Assert.AreEqual(1, CommonUtils.CompareVersions("1.2", "1.0.0.0"));
            Assert.AreEqual(1, CommonUtils.CompareVersions("1.2", "1.0.0.0"));
            Assert.AreEqual(1, CommonUtils.CompareVersions("1.2.3.000001", "1.2.3.00000"));
            Assert.AreEqual(1, CommonUtils.CompareVersions("1.2.3.000100", "1.2.3.000001"));
            Assert.AreEqual(1, CommonUtils.CompareVersions("1.2.3.000100", "1.2.3.0001"));
            Assert.AreEqual(0, CommonUtils.CompareVersions("1.2.3.00000", "1.2.3.00000"));
            Assert.AreEqual(-1, CommonUtils.CompareVersions("1.2.3.000000", "1.2.3.001000"));
            Assert.AreEqual(-1, CommonUtils.CompareVersions("1.2", "1.2.3"));
        }

        [TestMethod]
        public void GetAllAttributesProcessTest()
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml("<root><elem attrib1=\"test1\" attrib2=\"test2\" attrib3=\"test3\" attrib4=\"test4\"/></root>");
            
            HashSet<string> result = new HashSet<string> {"attrib1", "attrib2", "attrib3", "attrib4"};

            HashSet<string> source = new HashSet<string>();
            HashSet<string> source2 = new HashSet<string> {"attrib1", "attrib3"};

            // ReSharper disable once PossibleNullReferenceException
            CommonUtils.GetAllAttributesProcess(doc.DocumentElement.ChildNodes[0], source);
            CommonUtils.GetAllAttributesProcess(doc.DocumentElement.ChildNodes[0], source2);

            Assert.IsTrue(source.SetEquals(result));
            Assert.IsTrue(source2.SetEquals(result));
        }

        [TestMethod]
        public void IsDictionaryTest()
        {
            string[] tests =
            {
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><translations name=\"AeDict\" version=\"1.0\"></translations>",
                "<translations name=\"AeDict\" version=\"1.0\"></translations>",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><translations name=\"no\" version=\"1.0\"></translations>",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><translations version=\"1.0\"></translations>",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><translations name=\"AeDict\" version=\"1.0\"><translations>",
                "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?><translations name=\"no\" version=\"1.0\"><translations>",
                ""
            };
            bool[] results =
            {
                true, true, false, false, false, false, false
            };

            for (int i = 0; i < tests.Length; i++)
            {
                using (StringWriter writer = new StringWriter())
                {
                    writer.Write(tests[i]);
                    
                    using (MemoryStream memoryStream = new MemoryStream(Encoding.UTF8.GetBytes(writer.ToString())))
                        Assert.AreEqual(results[i], AndroidFilesUtils.IsDictionaryFile(memoryStream));
                }
            }
        }
    }
}