using System;
using System.Drawing;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Media.Imaging;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.ViewModels.TreeViewModels;
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
        public static void LoadIconForItem(FilesTreeViewNodeModel item)
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
            if (GlobalVariables.AppSettings.ImageExtensions.Contains(file.Ext))
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
            return BitmapImageFromUri(new Uri(fileName)).FreezeIfCan();
        }

        /// <summary>
        /// Возвращает флаг указанного языка
        /// </summary>
        /// <param name="title">Язык</param>
        public static BitmapImage GetFlagImage(string title)
        {
            string file = Path.Combine(GlobalVariables.PathToFlags, $"{title}.png");

            if (File.Exists(file))
                return BitmapImageFromUri(new Uri(file)).FreezeIfCan();

            return null;
            //return GetImageFromApp($"/Resources/Flags/{title}.png");
        }

        public static BitmapImage BitmapImageFromUri(Uri uri)
        {
            if (uri == null)
                throw new ArgumentNullException(nameof(uri));

            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = uri;
            image.EndInit();

            return image;
        }

        /// <summary>
        /// Возвращает изображение из ресурсов программы
        /// </summary>
        /// <param name="relativePath">Путь в ресурсах</param>
        public static BitmapImage GetImageFromApp(string relativePath)
        {
            string uriString = "pack://application:,,,/TranslatorApk;component/" + relativePath;

            return BitmapImageFromUri(new Uri(uriString));
        }
    }
}
