using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TranslatorApk.Logic.Utils;

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
        public static void ActivateWindow<T>(Window notHittableWindow = null, Window ownerWindow = null, Func<T> createNew = null) where T : Window
        {
            ActivateWindow(typeof(T), notHittableWindow, ownerWindow, createNew);
        }

        /// <summary>
        /// Создаёт (если не создано раннее) и активирует окно
        /// </summary>
        /// <param name="windowType">Тип окна</param>
        /// <param name="notHittableWindow">Окно, которое не должно получать события</param>
        /// <param name="ownerWindow">Окно - родитель</param>
        /// <param name="createNew">Функция создания нового экземпляра окна</param>
        public static void ActivateWindow(Type windowType, Window notHittableWindow = null, Window ownerWindow = null, Func<Window> createNew = null)
        {
            if (notHittableWindow != null)
                notHittableWindow.IsHitTestVisible = false;

            string type = windowType.FullName;

            Window window;

            if (!WindowsDict.TryGetValue(type ?? throw new InvalidOperationException(), out window))
            {
                window = createNew?.Invoke() ?? (Window) Activator.CreateInstance(windowType);
                window.Closing += ChildWindowOnClosing;
                WindowsDict.Add(type, window);
            }
            else
            {
                if (!window.IsLoaded)
                {
                    window = createNew?.Invoke() ?? (Window) Activator.CreateInstance(windowType);
                    window.Closing += ChildWindowOnClosing;
                    WindowsDict[type] = window;
                }
            }

            if (ownerWindow != null)
                window.Owner = ownerWindow;

            var currentDispatcher = Dispatcher.CurrentDispatcher;

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ContextIdle, new Action(() =>
            {
                if (!window.IsLoaded)
                    window.Show();

                window.Activate();
                window.Focus();

                if (window.WindowState == WindowState.Minimized)
                    window.WindowState = WindowState.Normal;

                if (notHittableWindow != null)
                {
                    Task.Factory.StartNew(() =>
                    {
                        Thread.Sleep(500);

                        currentDispatcher.InvokeAction(() => notHittableWindow.IsHitTestVisible = true);
                    });
                }
            }));
        }

        private static void ChildWindowOnClosing(object sender, CancelEventArgs cancelEventArgs)
        {
            if (!cancelEventArgs.Cancel)
                RemoveFromList(sender.GetType());
        }

        /// <summary>
        /// Закрывает окно, которое ранее было активировано методом <see cref="ActivateWindow"/>
        /// </summary>
        /// <typeparam name="T">Тип окна</typeparam>
        public static void CloseWindow<T>() where T : Window
        {
            CloseWindow(typeof(T));
        }

        /// <summary>
        /// Закрывает окно, которое ранее было активировано методом <see cref="ActivateWindow"/>
        /// </summary>
        /// <param name="windowType">Тип окна</param>
        public static void CloseWindow(Type windowType)
        {
            if (WindowsDict.TryGetValue(windowType.FullName ?? throw new InvalidOperationException(), out Window window) && window.IsLoaded)
            {
                window.Close();
                WindowsDict.Remove(windowType.FullName);
            }
        }

        public static void RemoveFromList<T>() where T : Window
        {
            RemoveFromList(typeof(T));
        }

        public static void RemoveFromList(Type windowType)
        {
            WindowsDict.Remove(windowType.FullName ?? throw new InvalidOperationException());
        }

        public static void EnableWindow<T>() where T : Window
        {
            EnableWindow(typeof(T));
        }

        public static void EnableWindow(Type windowType)
        {
            if (WindowsDict.TryGetValue(windowType.FullName ?? throw new InvalidOperationException(), out Window window) && window.IsLoaded)
                window.IsEnabled = true;
        }

        public static void DisableWindow<T>() where T : Window
        {
            DisableWindow(typeof(T));
        }

        public static void DisableWindow(Type windowType)
        {
            if (WindowsDict.TryGetValue(windowType.FullName ?? throw new InvalidOperationException(), out Window window) && window.IsLoaded)
                window.IsEnabled = false;
        }

        public static T GetActiveWindow<T>() where T : Window
        {
            return WindowsDict.TryGetValue(typeof(T).FullName ?? throw new InvalidOperationException(), out Window result) ? (T)result : null;
        }
    }
}
