using System;
using System.Windows.Media.Imaging;

namespace TranslatorApk.Logic.OrganisationItems
{
    public static class GlobalResources
    {
        public static readonly BitmapImage Icon_FolderVerticalOpen =
            new BitmapImage(new Uri("/Resources/Icons/folder_vertical_open.png", UriKind.Relative));

        public static readonly BitmapImage Icon_UnknownFile =
            new BitmapImage(new Uri("/Resources/Icons/unknown_file.png", UriKind.Relative));
    }
}
