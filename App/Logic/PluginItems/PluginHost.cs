using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using NLog;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.OrganisationItems;

namespace TranslatorApk.Logic.PluginItems
{
    public class PluginHost
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        public ReadOnlyCollection<ActionHost> Actions { get; }
        private readonly List<ActionHost> _actions;

        public ReadOnlyCollection<TransServiceHost> Translators { get; }
        private readonly List<TransServiceHost> _translators;

        public AssemblyLoadContext LoadContext { get; }

        public string Name { get; private set; }

        public PluginHost(AssemblyLoadContext loadContext)
        {
            Actions = (_actions = new List<ActionHost>()).AsReadOnly();
            Translators = (_translators = new List<TransServiceHost>()).AsReadOnly();

            LoadContext = loadContext;
        }

        public void Load(string name)
        {
            Name = Path.GetFileNameWithoutExtension(name);

            {
                string libsPath = Path.Combine(Path.GetDirectoryName(name) ?? string.Empty, Path.GetFileNameWithoutExtension(name) ?? string.Empty, "Libraries");

                if (Directory.Exists(libsPath))
                {
                    foreach (string file in Directory.EnumerateFiles(libsPath, "*.dll"))
                    {
                        try
                        {
                            LoadContext.LoadFromAssemblyPath(file);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                            GlobalVariables.BugSnagClient.Notify(ex);
                            Debug.Fail(ex.ToString());
                        }
                    }
                }
            }

            Assembly plug;

            try
            {
                plug = LoadContext.LoadFromAssemblyPath(name);
            }
            catch (Exception)
            {
                return;
            }

            Type[] types;

            try
            {
                types = plug.GetTypes();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                GlobalVariables.BugSnagClient.Notify(ex);
                Debug.Fail(ex.ToString());
                return;
            }         

            foreach (Type type in types)
            {
                object[] customAttribs = type.GetCustomAttributes(false);

                if (customAttribs.Any(a => a.GetType().FullName == "TranslatorApkPluginLib.TranslateServiceAttribute"))
                    _translators.Add(new TransServiceHost(LoadService(type)));
                else if (customAttribs.Any(a => a.GetType().FullName == "TranslatorApkPluginLib.AdditionalActionAttribute"))
                    _actions.Add(new ActionHost(LoadService(type)));
            }
        }

        private static object LoadService(Type type)
        {
            var constructor = type.GetConstructor(Type.EmptyTypes);

            return constructor?.Invoke(Array.Empty<object>());
        }
    }
}
