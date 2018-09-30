using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Windows.Shell;
using TranslatorApk.Logic.Utils;

namespace TranslatorApk.Themes.Elements
{
    public static class WindowThemeParameters
    {
        public static readonly DependencyProperty IsResizableProperty = DependencyProperty.RegisterAttached(
            "IsResizable", typeof(bool), typeof(WindowThemeParameters), new PropertyMetadata(true));

        public static void SetIsResizable(DependencyObject element, bool value)
        {
            element.SetValue(IsResizableProperty, value);
        }

        public static bool GetIsResizable(DependencyObject element)
        {
            return (bool) element.GetValue(IsResizableProperty);
        }

        public static readonly DependencyProperty CanMinimizeProperty = DependencyProperty.RegisterAttached(
            "CanMinimize", typeof(bool), typeof(WindowThemeParameters), new PropertyMetadata(true));

        public static void SetCanMinimize(DependencyObject element, bool value)
        {
            element.SetValue(CanMinimizeProperty, value);
        }

        public static bool GetCanMinimize(DependencyObject element)
        {
            return (bool) element.GetValue(CanMinimizeProperty);
        }

        public static readonly DependencyProperty CanCloseProperty = DependencyProperty.RegisterAttached(
            "CanClose", typeof(bool), typeof(WindowThemeParameters), new PropertyMetadata(true));

        public static void SetCanClose(DependencyObject element, bool value)
        {
            element.SetValue(CanCloseProperty, value);
        }

        public static bool GetCanClose(DependencyObject element)
        {
            return (bool) element.GetValue(CanCloseProperty);
        }
    }

    internal static class LocalExtensions
    {
        public static void ForWindowFromChild(this object childDependencyObject, Action<Window> action)
        {
            var element = childDependencyObject as DependencyObject;
            while (element != null)
            {
                element = VisualTreeHelper.GetParent(element);
                if (element is Window window)
                {
                    action(window);
                    break;
                }
            }
        }

        public static void ForWindowFromTemplate(this object templateFrameworkElement, Action<Window> action)
        {
            if (templateFrameworkElement.As<FrameworkElement>().TemplatedParent is Window window)
                action(window);
        }

        public static IntPtr GetWindowHandle(this Window window)
        {
            var helper = new WindowInteropHelper(window);
            return helper.Handle;
        }
    }

    public partial class WindowStyle
    {
        #region sizing event handlers

        public string Some => "";

        // ReSharper disable once UnusedMember.Local
        void IconMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount > 1)
            {
                sender.ForWindowFromTemplate(w => w.Close());
            }
            else
            {
                sender.ForWindowFromTemplate(w =>
                    SendMessage(w.GetWindowHandle(), WM_SYSCOMMAND, (IntPtr)SC_KEYMENU, (IntPtr)' '));
            }
        }

        void CloseButtonClick(object sender, RoutedEventArgs e)
        {
            sender.ForWindowFromTemplate(w => w.Close());
        }

        void MinButtonClick(object sender, RoutedEventArgs e)
        {
            sender.ForWindowFromTemplate(w => w.WindowState = WindowState.Minimized);
        }

        void MaxButtonClick(object sender, RoutedEventArgs e)
        {
            sender.ForWindowFromTemplate(w =>
            {
                w.WindowState = w.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
            });
        }

        #endregion

        #region P/Invoke

        // ReSharper disable InconsistentNaming

        const int WM_SYSCOMMAND = 0x112;
        const int SC_KEYMENU = 0xF100;

        // ReSharper restore InconsistentNaming

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        // ReSharper disable once InconsistentNaming
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        #endregion

        private void LoadedHandler(object sender, RoutedEventArgs e)
        {
            var currentWindow = sender.As<Window>();

            if (!WindowThemeParameters.GetIsResizable(currentWindow))
            {
                WindowChrome.GetWindowChrome(currentWindow).ResizeBorderThickness = default;
                currentWindow.ResizeMode = ResizeMode.NoResize;
            }

            currentWindow.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }
    }
}