using System.Collections.Generic;
using System.Windows;

namespace TranslatorApk.Logic.OrganisationItems
{
    public static class WindowManager
    {
        private static readonly Dictionary<string, Window> windowsDict = new Dictionary<string, Window>();

        public static void ActivateWindow<T>() where T : Window, new()
        {
            string type = typeof(T).FullName;

            Window window;

            if (!windowsDict.TryGetValue(type, out window))
            {
                window = new T();
                windowsDict.Add(type, window);
                window.Show();
            }

            if (!window.IsLoaded)
            {
                window = new T();
                windowsDict[type] = window;
                window.Show();
            }

            window.Activate();
            window.Focus();

            if (window.WindowState == WindowState.Minimized)
                window.WindowState = WindowState.Normal;
        }

        public static void CloseWindow<T>() where T : Window, new()
        {
            if (windowsDict.TryGetValue(typeof(T).FullName, out Window window) && window.IsLoaded)
            {
                window.Close();
            }
        }
    }
}
