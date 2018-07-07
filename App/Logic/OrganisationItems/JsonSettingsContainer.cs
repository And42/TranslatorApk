using System;
using System.IO;
using System.Reflection;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.JsonModels;
using TranslatorApk.Logic.Utils;

namespace TranslatorApk.Logic.OrganisationItems
{
    internal class JsonSettingsContainer : AppSettingsBase
    {
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
                    ? JsonUtils.DeserializeFromFile<AppSettingsJson>(settingsFilePath) 
                    : new AppSettingsJson();
        }

        public override void Save()
        {
            JsonUtils.SerializeToFile(_settingsJson, _settingsFilePath);
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
    }
}
