﻿using System;
using System.Windows;
using TranslatorApk.Logic;
using TranslatorApk.Windows;

namespace TranslatorApk
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                Clipboard.SetText("Message: " + (args.ExceptionObject as Exception)?.ToString());
                MessBox.ShowDial("Обнаружена непредвиденная ошибка, текст ошибки в буфере обмена");
            };
            DispatcherUnhandledException += (sender, args) =>
            {
                Clipboard.SetText($"Message: {args.Exception.Message}\nStackTrace: {args.Exception.StackTrace}");
                MessBox.ShowDial($"Обнаружена ошибка (\"{args.Exception.Message}\"), текст скопирован в буфер. Пожалуйста, отправьте её разработчику");
#if !DEBUG
                args.Handled = true;
#endif
            };

            if (e.Args.Length > 0 && e.Args[0] == "update")
            {
                new DownloadWindow().Show();
                return;
            }

            EventModuler.Init();

            Functions.LoadSettings();

#if !DEBUG
            if (!GlobalVariables.Portable)
#endif
                Functions.CheckForUpdate();

            new MainWindow(e.Args).Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Functions.ExitActions();
        }
    }
}
