using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using AndroidLibs;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using MVVM_Tools.Code.Commands;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.Utils;
using TranslatorApk.Resources.Localizations;
using TranslatorApk.Windows;

namespace TranslatorApk.Logic.ViewModels.Windows.MainWindow
{
    internal partial class MainWindowViewModel
    {
        public IActionCommand SignCommand { get; private set; }
        public IActionCommand BuildCommand { get; private set; }
        public IActionCommand ChooseFileCommand { get; private set; }
        public IActionCommand ChooseFolderCommand { get; private set; }
        public IActionCommand InstallFrameworkCommand { get; private set; }
        public IActionCommand ShowSearchWindowCommand { get; private set; }

        public IActionCommand<DragEventArgs> ApkDropCommand { get; private set; }
        public IActionCommand<DragEventArgs> FolderDropCommand { get; private set; }
        public IActionCommand<DragEventArgs> FrameworkDropCommand { get; private set; }

        private void InitMainButtonsPart()
        {
            ActionCommand ActCom(Action action) => new ActionCommand(action);

            SignCommand = ActCom(SignCommand_Execute);
            BuildCommand = ActCom(BuildCommand_Execute);
            ChooseFileCommand = ActCom(ChooseFileCommand_Execute);
            ChooseFolderCommand = ActCom(ChooseFolderCommand_Execute);
            InstallFrameworkCommand = ActCom(InstallFrameworkCommand_Execute);
            ShowSearchWindowCommand = ActCom(OpenSearchCommand_Execute);

            ApkDropCommand = new ActionCommand<DragEventArgs>(ApkDropCommand_Execute);
            FolderDropCommand = new ActionCommand<DragEventArgs>(FolderDropCommand_Execute);
            FrameworkDropCommand = new ActionCommand<DragEventArgs>(FrameworkDropCommand_Execute);
        }

        #region Click

        private void ChooseFileCommand_Execute()
        {
            var fd = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = ".apk",
                Filter = StringResources.AndroidApps + @" (*.apk)|*.apk",
                Multiselect = false
            };

            if (fd.ShowDialog() != true)
                return;

            DecompileFile(fd.FileName);
        }

        private void ChooseFolderCommand_Execute()
        {
            var dialog = new CommonOpenFileDialog
            {
                Title = StringResources.SelectAFolder,
                Multiselect = false,
                IsFolderPicker = true,
                EnsurePathExists = true
            };

            if (dialog.ShowDialog() != CommonFileDialogResult.Ok)
                return;

            LoadFolder(dialog.FileName);
        }

        private void OpenSearchCommand_Execute()
        {
            new SearchWindow().ShowDialog();
        }

        private void BuildCommand_Execute()
        {
            if (Apk == null)
                return;

            if (!Apk.HasJava())
            {
                MessBox.ShowDial(StringResources.JavaNotFoundError, StringResources.ErrorLower);
                return;
            }

            Disable();
            ClearVisLog();

            bool success = false;
            var errors = new List<Error>();

            LoadingWindow.ShowWindow(
                beforeStarting: () => { },
                threadActions: source => success = Apk.Compile(out errors),
                finishActions: () =>
                {
                    Enable();
                    VisLog(Log(GlobalVariables.LogLine));
                    VisLog(Log(success ? StringResources.Finished : StringResources.ErrorWhileCompiling));
                    VisLog(Log(GlobalVariables.LogLine));

                    if (SettingsIncapsuler.Instance.ShowNotifications)
                    {
                        NotificationService.Instance.ShowMessage(StringResources.CompilationFinished);
                    }

                    if (!success && errors.Any(error => error.Type != Error.ErrorType.None))
                    {
                        if (MessBox.ShowDial("Обнаружены ошибки. Попробовать исправить?", "", MessBox.MessageButtons.Yes, MessBox.MessageButtons.No) == MessBox.MessageButtons.Yes)
                        {
                            Apktools.FixErrors(errors);
                            BuildCommand_Execute();
                        }
                    }
                    else
                    {
                        VisLog(Log(StringResources.FileIsSituatedIn + " " + Apk.NewApk));
                    }
                },
                cancelVisibility: Visibility.Collapsed
            );
        }

        private async void InstallFrameworkCommand_Execute()
        {
            var fd = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = ".apk",
                Filter = StringResources.AndroidApps + @" (*.apk)|*.apk",
                Multiselect = false
            };

            if (fd.ShowDialog() != true)
                return;

            await InstallFramework(fd.FileName);
        }

        private void SignCommand_Execute()
        {
            if (Apk == null)
                return;

            if (!Apk.HasJava())
            {
                MessBox.ShowDial(StringResources.JavaNotFoundError, StringResources.ErrorLower);
                return;
            }

            Disable();

            bool success = false;

            var line = GlobalVariables.LogLine;

            LoadingWindow.ShowWindow(
                beforeStarting: () => { },
                threadActions: source => success = Apk.Sign(),
                finishActions: () =>
                {
                    Enable();
                    VisLog(Log(line));
                    VisLog(Log(success ? StringResources.Finished : StringResources.ErrorWhileSigning));

                    if (success)
                    {
                        string message = $"{StringResources.FileIsSituatedIn} {Apk.SignedApk}";

                        VisLog(Log(message));

                        string dir = Path.GetDirectoryName(Apk.SignedApk);

                        if (dir != null && MessBox.ShowDial(message, StringResources.Finished, MessBox.MessageButtons.OK, StringResources.Open) == StringResources.Open)
                        {
                            Process.Start(dir);
                        }
                    }
                },
                cancelVisibility: Visibility.Collapsed);

            VisLog(Log(string.Format("{0}{1}Signing...{1}{0}", line, Environment.NewLine)));
        }

        #endregion

        #region Drag & Drop

        private void ApkDropCommand_Execute(DragEventArgs args)
        {
            string[] files = args.GetFilesDrop();

            if (files != null && files.Length == 1 && Path.GetExtension(files[0]) == ".apk")
                DecompileFile(files[0]);

            args.Handled = true;
        }

        private void FolderDropCommand_Execute(DragEventArgs args)
        {
            string[] files = args.GetFilesDrop();

            if (files != null && files.Length == 1 && Directory.Exists(files[0]))
                LoadFolder(files[0]);

            args.Handled = true;
        }

        private async void FrameworkDropCommand_Execute(DragEventArgs args)
        {
            string[] files = args.GetFilesDrop();

            if (files != null && files.Length == 1 && Path.GetExtension(files[0]) == ".apk")
                await InstallFramework(files[0]);

            args.Handled = true;
        }

        #endregion
    }
}
