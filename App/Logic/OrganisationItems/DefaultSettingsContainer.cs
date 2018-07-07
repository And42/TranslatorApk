using TranslatorApk.Logic.Classes;
using TranslatorApk.Properties;

namespace TranslatorApk.Logic.OrganisationItems
{
    /// <summary>
    /// !!! will not work properly as you need to actually assign setting to update it in the source file !!!
    /// </summary>
    internal class DefaultSettingsContainer : AppSettingsBase
    {
        public override void Save()
        {
            Settings.Default.Save();
        }

        protected override void SetSetting(string settingName, object value)
        {
            Settings.Default[settingName] = value;
        }

        protected override object GetSetting(string settingName)
        {
            return Settings.Default[settingName];
        }
    }
}
