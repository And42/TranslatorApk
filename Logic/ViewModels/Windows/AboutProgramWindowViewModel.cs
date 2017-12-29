using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Expression.Interactivity.Core;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Windows;

namespace TranslatorApk.Logic.ViewModels.Windows
{
    internal class AboutProgramWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// Возвращает версию программы
        /// </summary>
        public string Version { get; } = GlobalVariables.ProgramVersion;

        /// <summary>
        /// Иконка программы
        /// </summary>
        public ImageSource ImgSrc { get; } = new BitmapImage(new Uri("pack://application:,,,/TranslatorApk;component/TranslatorApk.ico"));

        public ICommand WebMoneyClickedCommand { get; } = new ActionCommand(() =>
        {
            Clipboard.SetText("R897735207346");
            MessBox.ShowDial(Resources.Localizations.Resources.AccountNumberIsCopied);
        });

        public override void UnsubscribeFromEvents() { }
    }
}
