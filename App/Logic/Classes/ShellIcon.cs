using System;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace TranslatorApk.Logic.Classes
{
    /// <summary>
    /// Summary description for ShellIcon.  Get a small or large Icon with an easy C# function call
    /// that returns a 32x32 or 16x16 System.Drawing.Icon depending on which function you call
    /// either GetSmallIcon(string fileName) or GetLargeIcon(string fileName)
    /// </summary>
    public static class ShellIcon
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct SHFILEINFO
        {
            internal IntPtr hIcon;
            internal IntPtr iIcon;
            internal uint dwAttributes;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            internal string szDisplayName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            internal string szTypeName;
        };

        internal static class Win32
        {
            public const uint SHGFI_ICON = 0x100;
            public const uint SHGFI_LARGEICON = 0x0; // 'Large icon
            public const uint SHGFI_SMALLICON = 0x1; // 'Small icon

            [DllImport("shell32.dll")]
            public static extern IntPtr SHGetFileInfo(string pszPath, uint dwFileAttributes, ref SHFILEINFO psfi, uint cbSizeFileInfo, uint uFlags);

            [DllImport("User32.dll")]
            public static extern int DestroyIcon(IntPtr hIcon);

        }

        /// <summary>
        /// Возвращяет маленькую иконку для файла
        /// </summary>
        /// <param name="fileName">Путь к файлу</param>
        public static Icon GetSmallIcon(string fileName)
        {
            return GetIcon(fileName, Win32.SHGFI_SMALLICON);
        }

        /// <summary>
        /// Возвращяет большую иконку для файла
        /// </summary>
        /// <param name="fileName">Путь к файлу</param>
        public static Icon GetLargeIcon(string fileName)
        {
            return GetIcon(fileName, Win32.SHGFI_LARGEICON);
        }

        private static Icon GetIcon(string fileName, uint flags)
        {
            SHFILEINFO shinfo = new SHFILEINFO();
            Win32.SHGetFileInfo(fileName, 0, ref shinfo, (uint)Marshal.SizeOf(shinfo), Win32.SHGFI_ICON | flags);

            Icon icon = (Icon)Icon.FromHandle(shinfo.hIcon).Clone();
            Win32.DestroyIcon(shinfo.hIcon);
            return icon;
        }

        [DllImport("gdi32.dll", SetLastError = true)]
        private static extern bool DeleteObject(IntPtr hObject);

        /// <summary>
        /// Преобразует Icon в ImageSource
        /// </summary>
        /// <param name="icon">Иконка</param>
        /// <exception cref="Win32Exception"></exception>
        public static BitmapSource IconToBitmapSource(Icon icon)
        {
            Bitmap bitmap = icon.ToBitmap();
            IntPtr hBitmap = bitmap.GetHbitmap();

            var wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
                hBitmap,
                IntPtr.Zero,
                Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions());

            if (!DeleteObject(hBitmap))
            {
                throw new Win32Exception();
            }

            return wpfBitmap;
        }
    }
}
