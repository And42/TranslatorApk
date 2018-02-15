using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using AndroidLibs;
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
            _windowDispatcher.InvokeAction(() =>
            {
                var item =
                    new PluginMenuItemModel(
                        action.Item.GetActionTitle(),
                        PluginItemCommand,
                        action: action
                    );

                _pluginMenuItems.Add(item);
            });
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

            if (MessBox.ShowDial($"{confirmation} {opts.FullPath}?", StringResources.Confirmation, StringResources.Yes, StringResources.No) == StringResources.Yes)
            {
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
        }

        private static void DeleteFilePromt(FilesTreeViewNodeModel node)
        {
            DeleteSmthPromt(node, StringResources.FileDeleteConfirmation, File.Delete, File.Exists);
        }

        private static void DeleteFolderPromt(FilesTreeViewNodeModel node)
        {
            DeleteSmthPromt(node, StringResources.FolderDeleteConfirmation, str => Directory.Delete(str, true), Directory.Exists);
        }

        private void DecompileFile(string file)
        {
            GlobalVariables.CurrentProjectFile = file;

            string logPath = Path.Combine(Path.GetDirectoryName(file) ?? string.Empty,
                $"{Path.GetFileNameWithoutExtension(file)}_log.txt");

            if (!TryCreateNewLog(logPath))
                return;

            Apk = new Apktools(file, GlobalVariables.PathToResources, GlobalVariables.CurrentApktoolPath);
            Apk.Logging += s => VisLog(Log(s));

            if (!Apk.HasJava())
            {
                MessBox.ShowDial(StringResources.JavaNotFoundError, StringResources.ErrorLower);
                return;
            }

            Disable();
            ClearVisLog();

            bool success = false;

            LoadingWindow.ShowWindow(
                beforeStarting: () => { },
                threadActions: source => success = SettingsIncapsuler.Instance.OnlyResources ? Apk.Decompile(options: "-s") : Apk.Decompile(),
                finishActions: () =>
                {
                    Enable();
                    VisLog(GlobalVariables.LogLine);
                    VisLog(StringResources.Finished);
                    VisLog(GlobalVariables.LogLine);

                    if (SettingsIncapsuler.Instance.ShowNotifications)
                    {
                        NotificationService.Instance.ShowMessage(StringResources.DecompilationFinished);
                    }

                    if (success)
                    {
                        _windowDispatcher.InvokeAction(() => LoadFolder(GlobalVariables.CurrentProjectFolder, true));
                    }
                },
                cancelVisibility: Visibility.Collapsed
            );
        }

        private async Task InstallFramework(string fileName)
        {
            Disable();

            await Task.Factory.StartNew(() =>
            {
                var apktool = new Apktools(null, GlobalVariables.PathToResources,
                    Path.Combine(GlobalVariables.PathToApktoolVersions, $"apktool_{SettingsIncapsuler.Instance.ApktoolVersion}.jar"));

                if (!apktool.HasJava())
                {
                    MessBox.ShowDial(StringResources.JavaNotFoundError, StringResources.ErrorLower);
                    return;
                }

                apktool.Logging += s => VisLog(s);
                apktool.InstallFramework(fileName);
            });

            Enable();
        }

        public void LoadFolder(string folderPath, bool haveLogger = false)
        {
            if (folderPath == null)
                return;

            if (!haveLogger)
            {
                string logPath = folderPath + "_log.txt";

                if (!TryCreateNewLog(logPath))
                    return;
            }

            GlobalVariables.CurrentProjectFile = folderPath + ".apk";

            Apk = new Apktools(
                GlobalVariables.CurrentProjectFile,
                GlobalVariables.PathToResources,
                GlobalVariables.CurrentApktoolPath
            );

            Apk.Logging += s => VisLog(Log(s));

            FilesFilesTreeViewModel.Children.Clear();
            FilesFilesTreeViewModel.Children.Add(
                new FilesTreeViewNodeModel
                {
                    Name = StringResources.AllXml,
                    Options = new Options("", false, true),
                    DoubleClicked = LoadXmlFiles,
                    Image = GlobalResources.IconUnknownFile
                }
            );
            FilesFilesTreeViewModel.Children.Add(
                new FilesTreeViewNodeModel
                {
                    Name = StringResources.AllSmali,
                    Options = new Options("", false, true),
                    DoubleClicked = LoadSmaliFiles,
                    Image = GlobalResources.IconUnknownFile
                }
            );

            LoadingProcessWindow.ShowWindow(
                Disable,
                (cts, invoker) =>
                {
                    invoker.IsIndeterminate = true;

                    invoker.ProcessMax = Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories).Count();

                    invoker.IsIndeterminate = false;

                    Utils.Utils.LoadFilesToTreeView(_windowDispatcher, folderPath, FilesFilesTreeViewModel, SettingsIncapsuler.Instance.EmptyFolders, cts, () => invoker.ProcessValue++);
                },
                () =>
                {
                    Enable();

                    FilesFilesTreeViewModel.Children.ForEach(ImageUtils.LoadIconForItem);
                });
        }

        private static void Expand(FilesTreeViewNodeModel item, bool expand = true)
        {
            item.IsExpanded = expand;

            foreach (FilesTreeViewNodeModel child in item.Children)
                Expand(child, expand);
        }

        private static void Enable()
        {
            WindowManager.EnableWindow<TranslatorApk.Windows.MainWindow>();
        }

        private static void Disable()
        {
            WindowManager.DisableWindow<TranslatorApk.Windows.MainWindow>();
        }

        public string Log(string text)
        {
            AndroidLogger.Log(text);
            return text;
        }

        public string VisLog(string text)
        {
            _logTextBuilder.Append(text);
            _logTextBuilder.Append(Environment.NewLine);
            OnPropertyChanged(nameof(LogBoxText));

            return text;
        }

        public void ClearVisLog()
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

        private static void LoadXmlFiles()
        {
            LoadFiles(".xml", XmlFile.Create);
        }

        private static void LoadSmaliFiles()
        {
            LoadFiles(".smali", file => new SmaliFile(file));
        }

        private static void LoadFiles(string extension, Func<string, IEditableFile> createNew)
        {
            var filesList = new ConcurrentQueue<IEditableFile>();

            LoadingProcessWindow.ShowWindow(
                beforeStarting: Disable,
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

                        if (cts.IsCancellationRequested)
                            return;

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
                        filesList
                            .Where(file => file.Details != null && file.Details.Count > 0)
                            .ToList();

                    Enable();

                    WindowManager.ActivateWindow<EditorWindow>();

                    ManualEventManager.GetEvent<AddEditableFilesEvent>()
                        .Publish(new AddEditableFilesEvent(res));
                }
            );
        }
    }
}
