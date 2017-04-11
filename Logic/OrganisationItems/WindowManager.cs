using System.Collections.Generic;
using System.Windows;

namespace TranslatorApk.Logic.OrganisationItems
{
    /// <summary>
    /// Класс для работы с окнами программы
    /// </summary>
    public static class WindowManager
    {
        private static readonly Dictionary<string, Window> WindowsDict = new Dictionary<string, Window>();

        /// <summary>
        /// Создаёт (если не создано раннее) и активирует окно
        /// </summary>
        /// <typeparam name="T">Тип окна</typeparam>
        public static void ActivateWindow<T>() where T : Window, new()
        {
            string type = typeof(T).FullName;

            Window window;

            if (!WindowsDict.TryGetValue(type, out window))
            {
                window = new T();
                WindowsDict.Add(type, window);
                window.Show();
            }

            if (!window.IsLoaded)
            {
                window = new T();
                WindowsDict[type] = window;
                window.Show();
            }

            window.Activate();
            window.Focus();

            if (window.WindowState == WindowState.Minimized)
                window.WindowState = WindowState.Normal;
        }

        /// <summary>
        /// Закрывает окно, которое ранее было активировано методом <see cref="ActivateWindow{T}"/>
        /// </summary>
        /// <typeparam name="T">Тип окна</typeparam>
        public static void CloseWindow<T>() where T : Window, new()
        {
            if (WindowsDict.TryGetValue(typeof(T).FullName, out Window window) && window.IsLoaded)
            {
                window.Close();
            }
        }
    }
}
