using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using TranslatorApk.Logic.Classes;

namespace TranslatorApk.Logic.PluginItems
{
    public class PluginHost : MarshalByRefObject 
    {
        public readonly ReadOnlyCollection<ActionHost> Actions;
        private readonly List<ActionHost> _actions;

        public readonly ReadOnlyCollection<TransServiceHost> Translators;
        private readonly List<TransServiceHost> _translators;

        public AppDomain Domain { get; }

        public string Name { get; private set; }

        public PluginHost()
        {
            _actions = new List<ActionHost>();
            Actions = _actions.AsReadOnly();

            _translators = new List<TransServiceHost>();
            Translators = _translators.AsReadOnly();

            Domain = AppDomain.CurrentDomain;
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
                            Assembly.LoadFrom(file);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex.ToString(), "Error");
                        }
                    }
                }
            }

            Assembly plug;

            try
            {
                plug = Assembly.LoadFrom(name);
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
                Debug.WriteLine(ex.ToString(), "Error");
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
            var constructor = type.GetConstructor(new Type[0]);

            return constructor?.Invoke(new object[0]);
        }

        public override object InitializeLifetimeService() => null;
    }
}
