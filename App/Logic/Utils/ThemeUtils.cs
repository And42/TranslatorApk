using System;
using System.Linq;
using System.Windows;
using TranslatorApk.Logic.OrganisationItems;

namespace TranslatorApk.Logic.Utils
{
    internal static class ThemeUtils
    {
        public const string ThemeLight = "Light";
        public const string ThemeDark = "Dark";

        /// <summary>
        /// Меняет тему на одну из доступных
        /// </summary>
        /// <param name="name">ThemeLight / ThemeDark</param>
        public static void ChangeTheme(string name)
        {
            if (name != ThemeLight && name != ThemeDark)
                throw new ArgumentException(nameof(name));

            var appDicts = Application.Current.Resources.MergedDictionaries;
            if (appDicts.Any(d =>
            {
                string path = d.Source?.OriginalString;

                if (string.IsNullOrEmpty(path))
                    return false;

                // ReSharper disable once PossibleNullReferenceException
                var split = path.Split('/');

                return split.Length > 1 && split[1] == name;
            }))
            {
                return;
            }

            string resourcesFile = $"Themes/{name}/ThemeResources.xaml";

            appDicts.Insert(1, new ResourceDictionary
            {
                Source = new Uri(resourcesFile, UriKind.Relative)
            });
            appDicts.RemoveAt(2);

            GlobalVariables.AppSettings.Theme = name;
        }
    }
}
