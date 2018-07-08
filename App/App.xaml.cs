using System;
using System.Linq;
using System.Windows;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.Utils;
using TranslatorApk.Resources.Localizations;
using TranslatorApk.Windows;

namespace TranslatorApk
{
    public partial class App
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                var ex = args.ExceptionObject is Exception exception ? exception : new Exception("Domain exception");

                Logger.Fatal(ex);
                GlobalVariables.BugSnagClient.Notify(ex);

                Clipboard.SetText("Message: " + (args.ExceptionObject as Exception)?.ToString());
                MessageBox.Show("Обнаружена непредвиденная ошибка, текст ошибки в буфере обмена");
            };

            DispatcherUnhandledException += (sender, args) =>
            {
                Logger.Error(args.Exception);
                GlobalVariables.BugSnagClient.Notify(args.Exception);

                Clipboard.SetText($"Message: {args.Exception.Message}\nStackTrace: {args.Exception.StackTrace}");
                MessageBox.Show($"Обнаружена ошибка (\"{args.Exception.Message}\"), текст скопирован в буфер. Пожалуйста, отправьте её разработчику");
#if !DEBUG
                args.Handled = true;
#endif
            };

            if (e.Args.Length > 0 && e.Args[0] == "update")
            {
                new DownloadWindow().Show();
                return;
            }

            Utils.LoadSettings();

#if !DEBUG
            if (!GlobalVariables.Portable)
#endif
                Utils.CheckForUpdate();

            if (string.IsNullOrEmpty(GlobalVariables.AppSettings.ApktoolVersion))
            {
                Logger.Error("Apktool not found");
                MessBox.ShowDial(StringResources.ApktoolNotFound);
            }

            WindowManager.ActivateWindow<MainWindow>();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            NotificationService.Instance.Dispose();

            GlobalVariables.AppSettings.SourceDictionaries = GlobalVariables.SourceDictionaries.ToList();
            Utils.UpdateSettingsApiKeys();

            GlobalVariables.AppSettings.Save();
        }
    }
}
