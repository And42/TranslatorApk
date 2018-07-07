using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Expression.Interactivity.Core;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.Utils;
using TranslatorApk.Resources.Localizations;
using TranslatorApk.Windows;

namespace TranslatorApk.Logic.ViewModels.Windows
{
    public class AboutProgramWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// Возвращает версию программы
        /// </summary>
        public string Version { get; } = GlobalVariables.ProgramVersion;

        /// <summary>
        /// Иконка программы
        /// </summary>
        public ImageSource ImgSrc { get; } = ImageUtils.GetImageFromApp("Resources/Icons/app_icon.ico").FreezeIfCan();

        public ICommand WebMoneyClickedCommand { get; } = new ActionCommand(() =>
        {
            Clipboard.SetText("R897735207346");
            MessBox.ShowDial(StringResources.AccountNumberIsCopied);
        });

        public override void UnsubscribeFromEvents() { }
    }
}
