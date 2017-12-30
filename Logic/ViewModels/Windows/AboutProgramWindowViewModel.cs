using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Expression.Interactivity.Core;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.Utils;
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
        public ImageSource ImgSrc { get; } = ImageUtils.GetImageFromApp("TranslatorApk.ico").FreezeIfCan();

        public ICommand WebMoneyClickedCommand { get; } = new ActionCommand(() =>
        {
            Clipboard.SetText("R897735207346");
            MessBox.ShowDial(Resources.Localizations.Resources.AccountNumberIsCopied);
        });

        public override void UnsubscribeFromEvents() { }
    }
}
