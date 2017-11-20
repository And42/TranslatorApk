using System;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.OrganisationItems;
using UsefulClasses;

namespace TranslatorApk.Logic.Utils
{
    internal static class ImageUtils
    {
        //todo: Исправить ошибку от Aid5 (IconHandler.IconFromExtension(item.Options.Ext, IconSize.Large))

        private static bool _canLoadIcons = true;

        /// <summary>
        /// Загружает иконку для TreeViewItem
        /// </summary>
        /// <param name="item">Целевой объект</param>
        public static void LoadIconForItem(TreeViewNodeModel item)
        {
            if (item.Options.IsImageLoaded)
                return;

            BitmapSource icon;

            if (item.Options.IsFolder)
            {
                icon = GlobalResources.IconFolderVerticalOpen;
            }
            else
            {
                if (_canLoadIcons)
                {
                    try
                    {
                        icon = LoadIconFromFile(item.Options).FreezeIfCan();
                    }
                    catch (RuntimeWrappedException)
                    {
                        icon = GlobalResources.IconUnknownFile;
                        _canLoadIcons = false;
                    }
                }
                else
                {
                    icon = GlobalResources.IconUnknownFile;
                }
            }

            if (icon != null)
            {
                item.Image = icon;
            }

            item.Options.IsImageLoaded = true;
        }

        /// <summary>
        /// Загружает изображение из файла, указанного в объекте типа <see cref="Options"/>
        /// </summary>
        /// <param name="file">Объект для обработки</param>
        private static BitmapSource LoadIconFromFile(Options file)
        {
            if (SettingsIncapsuler.Instance.ImageExtensions.Contains(file.Ext))
            {
                try
                {
                    BitmapImage image = LoadThumbnailFromFile(file.FullPath);
                    file.HasPreview = true;
                    return image;
                }
                catch (NotSupportedException)
                {
                    return ShellIcon.IconToBitmapSource(GetIconFromFile(file.FullPath)).FreezeIfCan();
                }
            }

            return ShellIcon.IconToBitmapSource(GetIconFromFile(file.FullPath)).FreezeIfCan();
        }

        /// <summary>
        /// Загружает иконку, ассоциированную с указанным файлом
        /// </summary>
        /// <param name="filePath">Файл, иконку которого необходимо получить</param>
        private static Icon GetIconFromFile(string filePath)
        {
            //return ShellIcon.GetLargeIcon(filePath);

            return Icon.ExtractAssociatedIcon(filePath);
        }

        /// <summary>
        /// Загружает изображение из файла
        /// </summary>
        /// <param name="fileName">Файл</param>
        private static BitmapImage LoadThumbnailFromFile(string fileName)
        {
            return new BitmapImage(new Uri(fileName));
        }

        /// <summary>
        /// Возвращает флаг указанного языка
        /// </summary>
        /// <param name="title">Язык</param>
        public static BitmapImage GetFlagImage(string title)
        {
            string file = Path.Combine(GlobalVariables.PathToFlags, $"{title}.png");

            if (File.Exists(file))
                return new BitmapImage(new Uri(file));

            return null;
            //return GetImageFromApp($"/Resources/Flags/{title}.png");
        }

        /// <summary>
        /// Возвращает изображение из ресурсов программы
        /// </summary>
        /// <param name="pathInApp">Путь в ресурсах</param>
        public static BitmapImage GetImageFromApp(string pathInApp)
        {
            return new BitmapImage(new Uri(pathInApp, UriKind.Relative));
        }
    }
}
