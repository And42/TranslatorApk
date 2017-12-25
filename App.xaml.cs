using System;
using System.Windows;
using HockeyApp;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.Utils;
using TranslatorApk.Windows;
using UsefulFunctionsLib;

namespace TranslatorApk
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            CrashHandler.Instance.Configure(this, AppDomain.CurrentDomain, "8ed481f6b5074c5fa2236accea3c9229", "TranslatorApk", "Andrey");

            /*AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Clipboard.SetText("Message: " + (args.ExceptionObject as Exception)?.ToString());
                MessageBox.Show("Обнаружена непредвиденная ошибка, текст ошибки в буфере обмена");
            };

            DispatcherUnhandledException += (sender, args) =>
            {
                Clipboard.SetText($"Message: {args.Exception.Message}\nStackTrace: {args.Exception.StackTrace}");
                MessageBox.Show($"Обнаружена ошибка (\"{args.Exception.Message}\"), текст скопирован в буфер. Пожалуйста, отправьте её разработчику");
#if !DEBUG
                args.Handled = true;
#endif
            };*/

            if (e.Args.Length > 0 && e.Args[0] == "update")
            {
                new DownloadWindow().Show();
                return;
            }

            EventModuler.Init();

            Utils.LoadSettings();

#if !DEBUG
            if (!GlobalVariables.Portable)
#endif
                Utils.CheckForUpdate();

            if (SettingsIncapsuler.Instance.ApktoolVersion.NE())
            {
                MessBox.ShowDial(TranslatorApk.Resources.Localizations.Resources.ApktoolNotFound);
            }

            WindowManager.ActivateWindow(createNew: () => new MainWindow(e.Args));
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Utils.ExitActions();
        }
    }
}
