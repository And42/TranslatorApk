using System;
using TranslatorApkPluginLib;
using static TranslatorApk.Logic.OrganisationItems.Functions;

namespace TranslatorApk.Logic.Classes
{
    public class ActionHost : MarshalByRefObject, IAdditionalAction
    {
        private readonly string title;
        private readonly Action<string, string, string, string, string, string, string, string, string> process;

        public ActionHost(object innerClass)
        {
            Type type = innerClass.GetType();

            title = ExecRefl<string>(type, innerClass, "GetActionTitle");
            Guid = ExecRefl<Guid>(type, innerClass, "get_Guid");

            process =
                CreateDelegate<Action<string, string, string, string, string, string, string, string, string>>
                    (innerClass, "Process");
        }

        public string GetActionTitle() => title;

        public void Process(string pathToApk, string pathToProjectFolder, string pathToCompiledApk, string pathToSignedApk,
            string pathToResourcesFolder, string pathToFilesFolder, string pathToPortableJava, string pathToApktool,
            string pathToPluginFolder)
        {
            process(pathToApk, pathToProjectFolder, pathToCompiledApk, pathToSignedApk, pathToResourcesFolder,
                pathToFilesFolder, pathToPortableJava, pathToApktool, pathToPluginFolder);
        }

        public Guid Guid { get; }

        public override object InitializeLifetimeService() => null;
    }
}
