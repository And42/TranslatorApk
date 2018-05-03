using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AndroidHelper.Logic;
using AndroidTranslator.Classes.Files;
using AndroidTranslator.Interfaces.Files;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.EventManagerLogic;
using TranslatorApk.Logic.Events;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.PluginItems;
using TranslatorApk.Logic.Utils;
using TranslatorApk.Logic.ViewModels.TreeViewModels;
using TranslatorApk.Resources.Localizations;
using TranslatorApk.Windows;
using TranslatorApkPluginLib;
using UsefulFunctionsLib;

namespace TranslatorApk.Logic.ViewModels.Windows.MainWindow
{
    internal partial class MainWindowViewModel
    {
        public void AddActionToMenu(PluginPart<IAdditionalAction> action)
        {
            var item =
                new PluginMenuItemModel(
                    action.Item.GetActionTitle(),
                    PluginItemCommand,
                    action: action
                );

            _pluginMenuItems.Add(item);
        }

        public void RemoveActionFromMenu(Guid actionGuid)
        {
            var found = PluginMenuItems.FirstOrDefault(it => it.Action?.Item.Guid == actionGuid);

            if (found != null)
                _pluginMenuItems.Remove(found);
        }

        private static void DeleteSmthPromt(FilesTreeViewNodeModel node, string confirmation, Action<string> deleteAction, Predicate<string> checkAction)
        {
            Options opts = node.Options;

            if (MessBox.ShowDial(
                    $"{confirmation} {opts.FullPath}?",
                    StringResources.Confirmation, StringResources.Yes, StringResources.No
                ) == StringResources.No)
                return;

            try
            {
                deleteAction(opts.FullPath);

                if (checkAction(opts.FullPath))
                    MessBox.ShowDial(StringResources.ErrorLower);
                else
                    node.RemoveFromParent();
            }
            catch (Exception)
            {
                MessBox.ShowDial(StringResources.ErrorLower);
            }
        }

        private static void DeleteFilePromt(FilesTreeViewNodeModel node)
        {
            DeleteSmthPromt(node, StringResources.FileDeleteConfirmation, IOUtils.DeleteFile, IOUtils.FileExists);
        }

        private static void DeleteFolderPromt(FilesTreeViewNodeModel node)
        {
            DeleteSmthPromt(node, StringResources.FolderDeleteConfirmation, IOUtils.DeleteFolder, IOUtils.FolderExists);
        }

        private void DecompileFile(string file)
        {
            GlobalVariables.CurrentProjectFile = file;

            string logPath = Path.Combine(Path.GetDirectoryName(file) ?? string.Empty,
                $"{Path.GetFileNameWithoutExtension(file)}_log.txt");

            if (!TryCreateNewLog(logPath))
                return;

            if (Apk != null)
                Apk.Logging -= VisLog;

            Apk = new Apktools(
                file,
                GlobalVariables.PathToResources,
                GlobalVariables.CurrentApktoolPath
            );

            Apk.Logging += VisLog;

            if (!Apk.HasJava())
            {
                MessBox.ShowDial(StringResources.JavaNotFoundError, StringResources.ErrorLower);
                return;
            }

            ClearVisLog();

            bool success = false;

            LoadingWindow.ShowWindow(
                beforeStarting: () => IsBusy = true,
                threadActions: source => success = DefaultSettingsContainer.Instance.OnlyResources ? Apk.Decompile(options: "-s") : Apk.Decompile(),
                finishActions: () =>
                {
                    IsBusy = false;

                    VisLog(GlobalVariables.LogLine);
                    VisLog(StringResources.Finished);
                    VisLog(GlobalVariables.LogLine);

                    if (DefaultSettingsContainer.Instance.ShowNotifications)
                    {
                        NotificationService.Instance.ShowMessage(StringResources.DecompilationFinished);
                    }

                    if (success)
                    {
                        Application.Current.Dispatcher.InvokeAction(() => LoadFolder(GlobalVariables.CurrentProjectFolder, true));
                    }
                },
                cancelVisibility: Visibility.Collapsed,
                ownerWindow: _window
            );
        }

        private async Task InstallFrameworkAsync(string fileName)
        {
            await Task.Factory.StartNew(() => InstallFramework(fileName));
        }

        private void InstallFramework(string fileName)
        {
            using (BusyDisposable())
            {
                var apktool = new Apktools(null, GlobalVariables.PathToResources,
                    Path.Combine(GlobalVariables.PathToApktoolVersions,
                        $"apktool_{DefaultSettingsContainer.Instance.ApktoolVersion}.jar"));

                if (!apktool.HasJava())
                {
                    MessBox.ShowDial(StringResources.JavaNotFoundError, StringResources.ErrorLower);
                    return;
                }

                apktool.Logging += VisLog;
                apktool.InstallFramework(fileName);
                apktool.Logging -= VisLog;
            }
        }

        private void LoadFolder(string folderPath, bool haveLogger = false)
        {
            if (string.IsNullOrEmpty(folderPath))
                return;

            if (!haveLogger)
            {
                string logPath = folderPath + "_log.txt";

                if (!TryCreateNewLog(logPath))
                    return;
            }

            GlobalVariables.CurrentProjectFile = folderPath + ".apk";

            if (Apk != null)
                Apk.Logging -= VisLog;

            Apk = new Apktools(
                GlobalVariables.CurrentProjectFile,
                GlobalVariables.PathToResources,
                GlobalVariables.CurrentApktoolPath
            );

            Apk.Logging += VisLog;

            FilesFilesTreeViewModel.Children.Clear();
            FilesFilesTreeViewModel.Children.Add(
                new FilesTreeViewNodeModel
                {
                    Name = StringResources.AllXml,
                    Options = new Options(fullPath: string.Empty, isFolder: false, isImageLoaded: true),
                    DoubleClicked = LoadXmlFiles,
                    Image = GlobalResources.IconUnknownFile
                }
            );
            FilesFilesTreeViewModel.Children.Add(
                new FilesTreeViewNodeModel
                {
                    Name = StringResources.AllSmali,
                    Options = new Options(fullPath: string.Empty, isFolder: false, isImageLoaded: true),
                    DoubleClicked = LoadSmaliFiles,
                    Image = GlobalResources.IconUnknownFile
                }
            );

            LoadingProcessWindow.ShowWindow(
                beforeStarting: () => IsBusy = true,
                threadActions: (cts, invoker) =>
                {
                    invoker.IsIndeterminate = true;

                    invoker.ProcessMax = Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories).Count();

                    invoker.IsIndeterminate = false;

                    Utils.Utils.LoadFilesToTreeView(Application.Current.Dispatcher, folderPath, FilesFilesTreeViewModel, DefaultSettingsContainer.Instance.EmptyFolders, cts, () => invoker.ProcessValue++);
                },
                finishActions: () =>
                {
                    IsBusy = false;

                    FilesFilesTreeViewModel.Children.ForEach(ImageUtils.LoadIconForItem);
                },
                ownerWindow: _window
            );
        }

        private static void Expand(FilesTreeViewNodeModel item, bool expand = true)
        {
            item.IsExpanded = expand;

            foreach (FilesTreeViewNodeModel child in item.Children)
                Expand(child, expand);
        }

        private static void Log(string text)
        {
            AndroidLogger.Log(text);
        }

        private void VisLog(string text)
        {
            Log(text);

            _logTextBuilder.Append(text);
            _logTextBuilder.Append(Environment.NewLine);
            OnPropertyChanged(nameof(LogBoxText));
        }

        private void ClearVisLog()
        {
            _logTextBuilder.Clear();
            OnPropertyChanged(nameof(LogBoxText));
        }

        private static bool TryCreateNewLog(string logPath)
        {
            try
            {
                AndroidLogger.NewLog(true, logPath);
                return true;
            }
            catch (IOException)
            {
                MessBox.ShowDial(string.Format(StringResources.FileIsInUse, logPath), StringResources.ErrorLower);
                return false;
            }
        }

        private void LoadXmlFiles()
        {
            LoadFiles(".xml", XmlFile.Create);
        }

        private void LoadSmaliFiles()
        {
            LoadFiles(".smali", file => new SmaliFile(file));
        }

        private void LoadFiles(string extension, Func<string, IEditableFile> createNew)
        {
            ConcurrentQueue<IEditableFile> filesList = null;

            LoadingProcessWindow.ShowWindow(
                beforeStarting: () => IsBusy = true,
                threadActions: (cts, invoker) =>
                {
                    invoker.IsIndeterminate = true;

                    string[] files = Directory.GetFiles(GlobalVariables.CurrentProjectFolder, "*" + extension, SearchOption.AllDirectories);

                    invoker.IsIndeterminate = false;

                    invoker.ProcessMax = files.Length;

                    filesList = new ConcurrentQueue<IEditableFile>();

                    Parallel.ForEach(files, file =>
                    {
                        if (!Utils.Utils.CheckFilePath(file))
                            return;

                        cts.ThrowIfCancellationRequested();

                        try
                        {
                            filesList.Enqueue(createNew(file));
                        }
                        catch (Exception)
                        {
                            // ignored
                        }

                        invoker.ProcessValue++;
                    });
                },
                finishActions: () =>
                {
                    List<IEditableFile> res =
                        (filesList ?? Enumerable.Empty<IEditableFile>())
                            .Where(file => file.Details != null && file.Details.Count > 0)
                            .ToList();

                    IsBusy = false;

                    WindowManager.ActivateWindow<EditorWindow>();

                    ManualEventManager.GetEvent<AddEditableFilesEvent>()
                        .Publish(new AddEditableFilesEvent(res));
                },
                ownerWindow: _window
            );
        }
    }
}
