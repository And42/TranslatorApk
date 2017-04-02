using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using Microsoft.Windows.Shell;
using UsefulFunctionsLib;

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
                if (element is Window) { action(element as Window); break; }
            }
        }

        public static void ForWindowFromTemplate(this object templateFrameworkElement, Action<Window> action)
        {
            var window = ((FrameworkElement)templateFrameworkElement).TemplatedParent as Window;
            if (window != null) action(window);
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

        const int WM_SYSCOMMAND = 0x112;
        const int SC_SIZE = 0xF000;
        const int SC_KEYMENU = 0xF100;

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        void DragSize(IntPtr handle, SizingAction sizingAction)
        {
            SendMessage(handle, WM_SYSCOMMAND, (IntPtr)(SC_SIZE + sizingAction), IntPtr.Zero);
            SendMessage(handle, 514, IntPtr.Zero, IntPtr.Zero);
        }

        private enum SizingAction
        {
            North = 3,
            South = 6,
            East = 2,
            West = 1,
            NorthEast = 5,
            NorthWest = 4,
            SouthEast = 8,
            SouthWest = 7
        }

        #endregion

        private void LoadedHandler(object sender, RoutedEventArgs e)
        {
            var currentWindow = sender.As<Window>();

            if (!WindowThemeParameters.GetIsResizable(currentWindow))
            {
                WindowChrome.GetWindowChrome(currentWindow).ResizeBorderThickness = default(Thickness);
                currentWindow.ResizeMode = ResizeMode.NoResize;
            }

            currentWindow.MaxHeight = SystemParameters.MaximizedPrimaryScreenHeight;
        }
    }
}