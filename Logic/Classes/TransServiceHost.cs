using System;
using TranslatorApkPluginLib;

using static TranslatorApk.Logic.OrganisationItems.Functions;

namespace TranslatorApk.Logic.Classes
{
    public class TransServiceHost : MarshalByRefObject, ITranslateService
    {
        private readonly string serviceName;
        private readonly Func<string, string, string, string> translate;

        public TransServiceHost(object innerClass)
        {
            Type type = innerClass.GetType();

            serviceName = ExecRefl<string>(type, innerClass, "GetServiceName");
            Guid = ExecRefl<Guid>(type, innerClass, "get_Guid");

            translate = CreateDelegate<Func<string, string, string, string>>(innerClass, "Translate");
        }

        public string GetServiceName() => serviceName;

        public string Translate(string text, string targetLanguage, string apiKey)
        {
            return translate(text, targetLanguage, apiKey);
        }

        public Guid Guid { get; }

        public override object InitializeLifetimeService() => null;
    }
}
