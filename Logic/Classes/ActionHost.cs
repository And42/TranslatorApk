using System;
using TranslatorApkPluginLib;
using static TranslatorApk.Logic.OrganisationItems.Utils;

namespace TranslatorApk.Logic.Classes
{
    public class ActionHost : MarshalByRefObject, IAdditionalAction
    {
        private readonly string _title;
        private readonly Action<string, string, string, string, string, string, string, string, string> _process;

        public ActionHost(object innerClass)
        {
            var type = innerClass.GetType();

            _title = ExecRefl<string>(type, innerClass, "GetActionTitle");
            Guid = ExecRefl<Guid>(type, innerClass, "get_Guid");

            _process =
                CreateDelegate<Action<string, string, string, string, string, string, string, string, string>>
                    (innerClass, "Process");
        }

        public string GetActionTitle() => _title;

        public void Process(string pathToApk, string pathToProjectFolder, string pathToCompiledApk, string pathToSignedApk,
            string pathToResourcesFolder, string pathToFilesFolder, string pathToPortableJava, string pathToApktool,
            string pathToPluginFolder)
        {
            _process(pathToApk, pathToProjectFolder, pathToCompiledApk, pathToSignedApk, pathToResourcesFolder,
                pathToFilesFolder, pathToPortableJava, pathToApktool, pathToPluginFolder);
        }

        public Guid Guid { get; }

        public override object InitializeLifetimeService() => null;
    }
}
