using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TranslatorApk.Logic.OrganisationItems;

using Res = TranslatorApk.Resources.Localizations.Resources;

namespace TranslatorApk.Windows
{
    /// <summary>
    /// Логика взаимодействия для AboutProgram.xaml
    /// </summary>
    public partial class AboutProgramWindow
    {
        /// <summary>
        /// Возвращает версию программы
        /// </summary>
        public string Version { get; } = GlobalVariables.ProgramVersion;

        /// <summary>
        /// Иконка программы
        /// </summary>
        public ImageSource ImgSrc { get; } = new BitmapImage(new Uri("pack://application:,,,/TranslatorApk;component/TranslatorApk.ico"));

        /// <summary>
        /// Создаёт новый экземпляр окна информации
        /// </summary>
        public AboutProgramWindow()
        {
            InitializeComponent();
        }

        private void WebMoneyClick(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText("R897735207346");
            MessBox.ShowDial(Res.AccountNumberIsCopied);
        }
    }
}
