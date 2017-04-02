using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TranslatorApk.Logic;

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
        public string Version => GlobalVariables.ProgramVersion;

        /// <summary>
        /// Иконка программы
        /// </summary>
        public ImageSource ImgSrc => new BitmapImage(new Uri("pack://application:,,,/TranslatorApk;component/TranslatorApk.ico"));

        /// <summary>
        /// Создаёт класс окна информации
        /// </summary>
        public AboutProgramWindow()
        {
            InitializeComponent();
        }

        private void WebMoneyClick(object sender, MouseButtonEventArgs e)
        {
            Clipboard.SetText("R897735207346");
            MessBox.ShowDial(TranslatorApk.Resources.Localizations.Resources.AccountNumberIsCopied);
        }
    }
}
