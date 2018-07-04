using System.Windows.Media.Imaging;
using TranslatorApk.Logic.Utils;

namespace TranslatorApk.Logic.OrganisationItems
{
    public static class GlobalResources
    {
        public static readonly BitmapImage IconFolderVerticalOpen =
            ImageUtils.GetImageFromApp("Resources/Icons/folder_vertical_open.png").FreezeIfCan();

        public static readonly BitmapImage IconUnknownFile =
            ImageUtils.GetImageFromApp("Resources/Icons/unknown_file.png").FreezeIfCan();
    }
}
