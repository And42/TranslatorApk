using System;
using System.IO;
using System.Reflection;
using System.Threading;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.JsonModels;
using TranslatorApk.Logic.Utils;

namespace TranslatorApk.Logic.OrganisationItems
{
    internal class JsonSettingsContainer : AppSettingsBase
    {
        // this is needed as there are more than a one instance of JsonSettingsContainer created at once
        // that is because every plugin creates its own instance of this static class
        private const int MaxReadWriteTries = 50;
        private const int ReadWriteFailWaitMs = 100;

        private readonly string _settingsFilePath;
        private readonly AppSettingsJson _settingsJson;

        public JsonSettingsContainer() : this(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                Assembly.GetExecutingAssembly().GetName().Name,
                "appSettings.json"
            )
        ) { }

        public JsonSettingsContainer(string settingsFilePath)
        {
            _settingsFilePath = settingsFilePath;

            string settingsDir = Path.GetDirectoryName(settingsFilePath);
            if (!Directory.Exists(settingsDir))
                Directory.CreateDirectory(settingsDir);

            _settingsJson = 
                File.Exists(settingsFilePath)
                    ? ReadFromFile()
                    : new AppSettingsJson();
        }

        public override void Save()
        {
            WriteToFile();
        }

        protected override void SetSetting(string settingName, object value)
        {
            PropertyInfo property = GetProperty(settingName);

            property.SetValue(_settingsJson, value, null);
        }

        protected override object GetSetting(string settingName)
        {
            PropertyInfo property = GetProperty(settingName);

            return property.GetValue(_settingsJson, null);
        }

        private PropertyInfo GetProperty(string settingName)
        {
            var property = _settingsJson.GetType().GetProperty(settingName);

            if (property == null)
                throw new Exception($"Setting \"{settingName}\" not found");

            return property;
        }

        private void WriteToFile()
        {
            int currentTry = 1;
            while (true)
            {
                try
                {
                    JsonUtils.SerializeToFile(_settingsJson, _settingsFilePath);
                    return;
                }
                catch (Exception)
                {
                    if (currentTry == MaxReadWriteTries)
                        throw;

                    currentTry++;
                    Thread.Sleep(ReadWriteFailWaitMs);
                }
            }
        }

        private AppSettingsJson ReadFromFile()
        {
            int currentTry = 1;
            while (true)
            {
                try
                {
                    return JsonUtils.DeserializeFromFile<AppSettingsJson>(_settingsFilePath);
                }
                catch (Exception)
                {
                    if (currentTry == MaxReadWriteTries)
                        throw;

                    currentTry++;
                    Thread.Sleep(ReadWriteFailWaitMs);
                }
            }
        }
    }
}
