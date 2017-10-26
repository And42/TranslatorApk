using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;
using System.Windows.Threading;
using AndroidLibs;
using AndroidTranslator.Classes.Exceptions;
using AndroidTranslator.Classes.Files;
using AndroidTranslator.Interfaces.Files;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using TranslatorApk.Annotations;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.CustomCommandContainers;
using TranslatorApk.Logic.EventManagerLogic;
using TranslatorApk.Logic.Events;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.PluginItems;
using TranslatorApk.Logic.Utils;
using TranslatorApkPluginLib;
using UsefulClasses;
using UsefulFunctionsLib;

using StringResources = TranslatorApk.Resources.Localizations.Resources;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MenuItem = System.Windows.Controls.MenuItem;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Drawing.Point;
using MButtons = TranslatorApk.Windows.MessBox.MessageButtons;

namespace TranslatorApk.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        #region Поля

        private static string[] _arguments;
        private static readonly Logger AndroidLogger;

        private const int ItemPreviewDelayMs = 500;

        public ICommand SignCommand { get; }
        public ICommand BuildCommand { get; }
        public ICommand ChooseFileCommand { get; }
        public ICommand ChooseFolderCommand { get; }
        public ICommand InstallFrameworkCommand { get; }
        public ICommand ShowSearchWindowCommand { get; }
        public ICommand RefreshFilesListCommand { get; }

        public Setting<bool>[] MainWindowSettings { get; }

        public string LogBoxText => _logTextBuilder.ToString();

        public WindowState MainWindowState
        {
            get
            {
                if (_isMinimized)
                    return WindowState.Minimized;

                return SettingsIncapsuler.Instance.MainWMaximized ? WindowState.Maximized : WindowState.Normal;
            }
            set
            {
                if (value != WindowState.Minimized)
                    SettingsIncapsuler.Instance.MainWMaximized = value == WindowState.Maximized;

                _isMinimized = value == WindowState.Minimized;

                OnPropertyChanged(nameof(MainWindowState));
            }
        }
        private bool _isMinimized;

        public Apktools Apk;
        public TreeViewNodeModel FilesTreeViewModel { get; } = new TreeViewNodeModel();

        private readonly StringBuilder _logTextBuilder = new StringBuilder();
        private Timer _fileImagePreviewTimer;
        private readonly PreviewWindowHandler _fileImagePreviewWindowHandler = new PreviewWindowHandler();

        //public ObservableCollection<LogItem> LogItems { get; } = new ObservableCollection<LogItem>();

        #endregion

        static MainWindow()
        {
            AndroidLogger = new Logger(GlobalVariables.PathToLogs, false);
        }

        public MainWindow(string[] args)
        {
            _arguments = args;

            MainWindowSettings = new[]
            {
                new Setting<bool>(nameof(SettingsIncapsuler.Instance.EmptyXml),        StringResources.EmptyXml),
                new Setting<bool>(nameof(SettingsIncapsuler.Instance.EmptySmali),      StringResources.EmptySmali),
                new Setting<bool>(nameof(SettingsIncapsuler.Instance.EmptyFolders),    StringResources.EmptyFolders),
                new Setting<bool>(nameof(SettingsIncapsuler.Instance.Images),          StringResources.Images),
                new Setting<bool>(nameof(SettingsIncapsuler.Instance.FilesWithErrors), StringResources.FilesWithErrors),
                new Setting<bool>(nameof(SettingsIncapsuler.Instance.OnlyXml),         StringResources.OnlyXml),
                new Setting<bool>(nameof(SettingsIncapsuler.Instance.OtherFiles),      StringResources.OtherFiles),
                new Setting<bool>(nameof(SettingsIncapsuler.Instance.OnlyResources),   StringResources.OnlyResources)
            };

            SignCommand = new ActionCommand(SignCommand_Execute);
            BuildCommand = new ActionCommand(BuildCommand_Execute);
            ChooseFileCommand = new ActionCommand(ChooseFileCommand_Execute);
            ChooseFolderCommand = new ActionCommand(ChooseFolderCommand_Execute);
            InstallFrameworkCommand = new ActionCommand(InstallFrameworkCommand_Execute);
            ShowSearchWindowCommand = new ActionCommand(OpenSearchCommand_Execute);
            RefreshFilesListCommand = new ActionCommand(RefreshFilesListCommand_Execute);

            InitializeComponent();

            LoadSettings();

            TaskbarItemInfo = new TaskbarItemInfo();
        }  

        private void StartPreviewTimer()
        {
            _fileImagePreviewTimer = new Timer(state =>
            {
                CancelPreviewTimer();

                Dispatcher.InvokeAction(() =>
                {
                    var pos = PointToScreen(Mouse.GetPosition(this));

                    _fileImagePreviewWindowHandler.Update(pos);
                });
            }, null, ItemPreviewDelayMs, ItemPreviewDelayMs);
        }

        private void CancelPreviewTimer()
        {
            if (_fileImagePreviewTimer != null)
            {
                _fileImagePreviewTimer.Dispose();
                _fileImagePreviewTimer = null;
            }
        }

        private void RefreshFilesListCommand_Execute(object obj)
        {
            if (!GlobalVariables.CurrentProjectFolder.NE())
                LoadFolder(GlobalVariables.CurrentProjectFolder);
        }

        #region Команды

        private void ChooseFileCommand_Execute(object arg)
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

        private void ChooseFolderCommand_Execute(object arg)
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

        private void OpenSearchCommand_Execute(object arg)
        {
            new SearchWindow().ShowDialog();
        }

        private void BuildCommand_Execute(object arg)
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
                        if (MessBox.ShowDial("Обнаружены ошибки. Попробовать исправить?", "", MButtons.Yes, MButtons.No) == MButtons.Yes)
                        {
                            Apktools.FixErrors(errors);
                            BuildCommand_Execute(arg);
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

        private void InstallFrameworkCommand_Execute(object arg)
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

            InstallFramework(fd.FileName);
        }

        private void SignCommand_Execute(object arg)
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

        #region Кнопки меню

        private void OpenPluginsWindowClick(object sender, RoutedEventArgs e)
        {
            new Plugins().ShowDialog();
        }

        private void OpenSettingsClick(object sender, RoutedEventArgs e)
        {
            new SettingsWindow().ShowDialog();
        }

        private void OpenEditorClick(object sender, RoutedEventArgs e)
        {
            WindowManager.ActivateWindow<EditorWindow>();
        }

        private void OpenXmlRulesClick(object sender, RoutedEventArgs e)
        {
            new XmlRulesWindow().ShowDialog();
        }

        private void OpenChangesDetectorClick(object sender, RoutedEventArgs e)
        {
            WindowManager.ActivateWindow<ChangesDetectorWindow>();
        }

        private void OpenAboutClick(object sender, RoutedEventArgs e)
        {
            new AboutProgramWindow().ShowDialog();
        }

        #endregion

        #region Кнопки общего контекстного меню

        private void ExpandClick(object sender = null, RoutedEventArgs e = null)
        {
            FilesTreeViewModel.Children.ForEach(it => it.IsExpanded = true);
        }

        private void CollapseClick(object sender = null, RoutedEventArgs e = null)
        {
            FilesTreeViewModel.Children.ForEach(it => Expand(it, false));
        }

        private void AddNewLanguageClick(object sender = null, RoutedEventArgs e = null)
        {
            new AddLanguageWindow(GlobalVariables.CurrentProjectFolder).ShowDialog();
        }

        #endregion

        #region Кнопки контекстного меню файла

        private void OpenClick(object sender, RoutedEventArgs e)
        {
            var node = sender.As<FrameworkElement>().DataContext.As<TreeViewNodeModel>();

            if (node.Options.IsFolder)
                return;

            Process.Start(node.Options.FullPath);
        }

        private void OpenWithClick(object sender, RoutedEventArgs e)
        {
            var node = sender.As<FrameworkElement>().DataContext.As<TreeViewNodeModel>();

            if (node.Options.IsFolder)
                return;

            Utils.OpenAs(node.Options.FullPath);
        }

        private void ReplaceFileClick(object sender, RoutedEventArgs e)
        {
            var node = sender.As<FrameworkElement>().DataContext.As<TreeViewNodeModel>();

            if (node.Options.IsFolder)
                return;

            Options opts = node.Options;

            var fd = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false
            };
            if (fd.ShowDialog() == true)
            {
                File.Copy(fd.FileName, opts.FullPath, true);
                MessBox.ShowDial(StringResources.Finished);
            }
            else
            {
                MessBox.ShowDial(StringResources.ErrorLower);
            }
        }

        private void DeleteFileClick(object sender, RoutedEventArgs e)
        {
            var node = sender.As<FrameworkElement>().DataContext.As<TreeViewNodeModel>();

            DeleteFilePromt(node);
        }

        #endregion

        #region Кнопки контекстного меню папки

        private void OpenInExplorerClick(object sender, RoutedEventArgs e)
        {
            var node = sender.As<FrameworkElement>().DataContext.As<TreeViewNodeModel>();

            Utils.ShowInExplorer(node.Options.FullPath);
        }

        private void DeleteFolderClick(object sender, RoutedEventArgs e)
        {
            var node = sender.As<FrameworkElement>().DataContext.As<TreeViewNodeModel>();

            DeleteFolderPromt(node);

        }

        private void ExpandFolderClick(object sender, RoutedEventArgs e)
        {
            var node = sender.As<FrameworkElement>().DataContext.As<TreeViewNodeModel>();

            Expand(node);
        }

        private void CollapseFolderClick(object sender, RoutedEventArgs e)
        {
            var node = sender.As<FrameworkElement>().DataContext.As<TreeViewNodeModel>();

            Expand(node, false);
        }

        #endregion

        #region События окна

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (SettingsIncapsuler.Instance.ApktoolVersion.NE())
            {
                MessBox.ShowDial(StringResources.ApktoolNotFound);
            }

            if (_arguments.Length == 1)
            {
                var file = _arguments[0];
                var ext = Path.GetExtension(file);

                if (Directory.Exists(file))
                {
                    LoadFolder(file);
                }
                else if (File.Exists(file))
                {
                    switch (ext)
                    {
                        case ".xml":
                        case ".smali":
                            Utils.LoadFile(file);
                            break;
                        case ".apk":
                            DecompileFile(file);
                            break;
                        case ".yml":
                            LoadFolder(Path.GetDirectoryName(file));
                            break;
                    }
                }
            }

            Task.Factory.StartNew(PluginUtils.LoadPlugins);
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            AndroidLogger.Stop();
            SettingsIncapsuler.Instance.MainWindowSize = new Point((int)Width, (int)Height);
        }

        #endregion

        #region Drag & Drop

        private void CheckApkDrag(object sender, DragEventArgs e)
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];

            if (files != null && files.Length == 1 && Path.GetExtension(files[0]) == ".apk")
                e.Effects = DragDropEffects.Move;
            else 
                e.Effects = DragDropEffects.None;

            e.Handled = true;
        }

        private void CheckFolderDrag(object sender, DragEventArgs e)
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];

            if (files != null && files.Length == 1 && Directory.Exists(files[0]))
                e.Effects = DragDropEffects.Move;
            else 
                e.Effects = DragDropEffects.None;

            e.Handled = true;
        }

        private void CheckFileDrag(object sender, DragEventArgs e)
        {
            e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;

            e.Handled = true;
        }

        private void ApkDragDrop(object sender, DragEventArgs e)
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];

            if (files != null && files.Length == 1 && Path.GetExtension(files[0]) == ".apk")
                DecompileFile(files[0]);

            e.Handled = true;
        }

        private void FolderDragDrop(object sender, DragEventArgs e)
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];

            if (files != null && files.Length == 1 && Directory.Exists(files[0]))
                LoadFolder(files[0]);

            e.Handled = true;
        }

        private void FrameworkDrop(object sender, DragEventArgs e)
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];

            if (files != null && files.Length == 1 && Path.GetExtension(files[0]) == ".apk")
                InstallFramework(files[0]);

            e.Handled = true;
        }

        private void FilesTreeView_OnDrop(object sender, DragEventArgs e)
        {
            e.Handled = true;

            if (!e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            var list = e.Data.GetData(DataFormats.FileDrop) as string[];

            if (list == null)
                return;

            List<IEditableFile> files = list.Select(Utils.GetSuitableEditableFile).Where(it => it != null).ToList();

            WindowManager.ActivateWindow<EditorWindow>();

            if (files.Count > 0)
            {
                ManualEventManager.GetEvent<AddEditableFilesEvent>()
                    .Publish(new AddEditableFilesEvent(files, false));

                string fileName = files[0].FileName;

                ManualEventManager.GetEvent<EditorScrollToFileAndSelectEvent>()
                    .Publish(new EditorScrollToFileAndSelectEvent(f => f.FileName.Equals(fileName, StringComparison.Ordinal)));
            }
        }

        #endregion

        #region TreeViewEvents

        private void OneFileDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left || e.ClickCount != 2)
                return;

            var node = sender.As<FrameworkElement>().DataContext.As<TreeViewNodeModel>();

            if (node.Options.IsFolder)
                return;

            if (node.DoubleClicked != null)
                node.DoubleClicked.Invoke();
            else
                Utils.LoadFile(node.Options.FullPath);

            e.Handled = true;
        }

        private void LoadXmlFiles()
        {
            var filesList = new List<XmlFile>();

            LoadingProcessWindow.ShowWindow(
                beforeStarting: Disable,
                threadActions: (cts, invoker) => 
                {
                    invoker.IsIndeterminate = true;

                    var files = Directory.GetFiles(GlobalVariables.CurrentProjectFolder, "*.xml", SearchOption.AllDirectories);

                    invoker.IsIndeterminate = false;

                    invoker.ProcessMax = files.Length;

                    filesList = new List<XmlFile>(files.Length);

                    Parallel.ForEach(files, file =>
                    {
                        if (!Utils.CheckFilePath(file)) return;
                        if (cts.IsCancellationRequested) return;

                        try
                        {
                            filesList.Add(XmlFile.Create(file));
                        }
                        catch (XmlParserException)
                        { }

                        invoker.ProcessValue++;
                    });
                },
                finishActions: () =>
                { 
                    List<IEditableFile> res = filesList.FindAll(file => file.Details != null && file.Details.Count > 0).ConvertAll(f => (IEditableFile)f);

                    Enable();

                    WindowManager.ActivateWindow<EditorWindow>();

                    ManualEventManager.GetEvent<AddEditableFilesEvent>()
                        .Publish(new AddEditableFilesEvent(res));
                }
            );
        }

        private void LoadSmaliFiles()
        {
            var filesList = new List<ISmaliFile>();

            LoadingProcessWindow.ShowWindow(
                beforeStarting: Disable,
                threadActions: (cts, invoker) =>
                {
                    invoker.IsIndeterminate = true;

                    var files = Directory.GetFiles(GlobalVariables.CurrentProjectFolder, "*.smali", SearchOption.AllDirectories);

                    invoker.IsIndeterminate = false;

                    invoker.ProcessMax = files.Length;

                    filesList = new List<ISmaliFile>(files.Length);

                    Parallel.ForEach(files, file =>
                    {
                        if (!Utils.CheckFilePath(file))
                            return;

                        if (cts.IsCancellationRequested)
                            return;

                        filesList.Add(new SmaliFile(file));

                        invoker.ProcessValue++;
                    });
                },
                finishActions: () =>
                {
                    List<IEditableFile> res = 
                        filesList
                            .Where(file => file.Details != null && file.Details.Count > 0)
                            .Cast<IEditableFile>()
                            .ToList();

                    Enable();

                    WindowManager.ActivateWindow<EditorWindow>();

                    ManualEventManager.GetEvent<AddEditableFilesEvent>()
                        .Publish(new AddEditableFilesEvent(res));
                }
            );
        }

        private void TreeView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                object item = FilesTreeView.SelectedItem;
                if (item == null) return;
                TreeViewNodeModel node = item.As<TreeViewNodeModel>();
                Options opt = node.Options;
                
                if (opt.IsFolder)
                    DeleteFolderPromt(node);
                else 
                    DeleteFilePromt(node);
            }
        }

        private void TreeViewItem_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!SettingsIncapsuler.Instance.ShowPreviews)
                return;

            var node = sender.As<FrameworkElement>().DataContext.As<TreeViewNodeModel>();

            if (node.Options.HasPreview)
            {
                _fileImagePreviewWindowHandler.Init(node.Image);
                StartPreviewTimer();
            }
        }

        private void TreeViewItem_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!SettingsIncapsuler.Instance.ShowPreviews)
                return;

            var node = (sender as FrameworkElement)?.DataContext as TreeViewNodeModel;

            if (node?.Options.HasPreview == true)
            {
                CancelPreviewTimer();

                _fileImagePreviewWindowHandler.Close();
            }
        }

        private void TreeViewItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (!SettingsIncapsuler.Instance.ShowPreviews)
                return;

            var node = sender.As<FrameworkElement>().DataContext.As<TreeViewNodeModel>();

            if (node.Options.HasPreview)
            {
                if (!_fileImagePreviewWindowHandler.IsShown)
                {
                    CancelPreviewTimer();
                    _fileImagePreviewWindowHandler.Init(node.Image);
                    StartPreviewTimer();
                }
                else
                {
                    _fileImagePreviewWindowHandler.Update(PointToScreen(Mouse.GetPosition(this)), node.Image);
                }
            }
        }

        #endregion

        #region Utils

        public void AddActionToMenu(PluginPart<IAdditionalAction> action)
        {
            Dispatcher.InvokeAction(() =>
            {
                var item = new MenuItem
                {
                    Header = action.Item.GetActionTitle(),
                    DataContext = action
                };

                item.Click += PluginItem_Click;

                AddActionsMenuItem.Items.Add(item);
            });
        }

        public void RemoveActionFromMenu(Guid actionGuid)
        {
            var found = 
                AddActionsMenuItem.Items
                    .Cast<MenuItem>()
                    .FirstOrDefault(it => (it.DataContext as PluginPart<IAdditionalAction>)?.Item.Guid == actionGuid);

            if (found != null)
                AddActionsMenuItem.Items.Remove(found);
        }

        private void PluginItem_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            var act = sender.As<MenuItem>().DataContext.As<PluginPart<IAdditionalAction>>();

            act.Item.Process(Apk?.FileName, Apk?.FolderOfProject, Apk?.NewApk, Apk?.SignedApk,
                GlobalVariables.PathToResources,
                GlobalVariables.PathToFiles,
                Path.Combine(GlobalVariables.PathToResources, "jre"),
                Path.Combine(GlobalVariables.PathToApktoolVersions, $"apktool_{SettingsIncapsuler.Instance.ApktoolVersion}.jar"),
                Path.Combine(GlobalVariables.PathToPlugins, act.Host.Name)
            );

            //Debug.WriteLine(AppDomain.CurrentDomain.GetAssemblies().Select(it => it.FullName).JoinStr("\n"));
        }

        private void DeleteSmthPromt(TreeViewNodeModel node, string confirmation, Action<string> deleteAction, Predicate<string> checkAction)
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

        private void DeleteFilePromt(TreeViewNodeModel node)
        {
            DeleteSmthPromt(node, StringResources.FileDeleteConfirmation, File.Delete, File.Exists);
        }

        private void DeleteFolderPromt(TreeViewNodeModel node)
        {
            DeleteSmthPromt(node, StringResources.FolderDeleteConfirmation, str => Directory.Delete(str, true), Directory.Exists);
        }
     
        private void LoadSettings()
        {
            Point size = SettingsIncapsuler.Instance.MainWindowSize;

            if (!size.IsEmpty)
            {
                Width = size.X;
                Height = size.Y;
            }
        }

        public void DecompileFile(string file)
        {
            GlobalVariables.CurrentProjectFile = file;

            string logPath = Path.Combine(Path.GetDirectoryName(file) ?? string.Empty,
                $"{Path.GetFileNameWithoutExtension(file)}_log.txt");

            if (!TryCreateNewLog(logPath))
                return;

            Apk = new Apktools(file, GlobalVariables.PathToResources, Path.Combine(GlobalVariables.PathToApktoolVersions, $"apktool_{SettingsIncapsuler.Instance.ApktoolVersion}.jar"));
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
                beforeStarting: () => {}, 
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
                        Dispatcher.InvokeAction(() => LoadFolder(GlobalVariables.CurrentProjectFolder, true));
                    }
                }, 
                cancelVisibility: Visibility.Collapsed
            );
        }

        private void InstallFramework(string fileName)
        {
            Disable();

            Task.Factory.StartNew(() =>
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
            }).ContinueWith(task => Enable());
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
                Path.Combine(GlobalVariables.PathToApktoolVersions, $"apktool_{SettingsIncapsuler.Instance.ApktoolVersion}.jar")
            );

            Apk.Logging += s => VisLog(Log(s));
            FilesTreeViewModel.Children.Clear();
            FilesTreeViewModel.Children.Add(new TreeViewNodeModel
            {
                Name = StringResources.AllXml,
                Options = new Options("", true),
                DoubleClicked = LoadXmlFiles,
                Image = GlobalResources.IconUnknownFile
            });
            FilesTreeViewModel.Children.Add(new TreeViewNodeModel
            {
                Name = StringResources.AllSmali,
                Options = new Options("", true),
                DoubleClicked = LoadSmaliFiles,
                Image = GlobalResources.IconUnknownFile
            });

            Dispatcher dispatcher = Dispatcher;

            LoadingProcessWindow.ShowWindow(() => IsEnabled = false,
                (cts, invoker) =>
                {
                    invoker.IsIndeterminate = true;

                    invoker.ProcessMax = Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories).Count();

                    invoker.IsIndeterminate = false;

                    Utils.LoadFilesToTreeView(dispatcher, folderPath, FilesTreeViewModel, SettingsIncapsuler.Instance.EmptyFolders, cts, () => invoker.ProcessValue++);
                },
                () =>
                {
                    IsEnabled = true;

                    FilesTreeViewModel.Children.ForEach(ImageUtils.LoadIconForItem);
                });
        }

        private void LoadAllInXml(object sender, RoutedEventArgs e)
        {
            var node = sender.As<FrameworkElement>().DataContext.As<TreeViewNodeModel>();

            if (node.Options.IsFolder || node.Options.Ext != ".xml")
                return;

            var file = new XmlFile(node.Options.FullPath, XmlFile.XmlRules, true);

            WindowManager.ActivateWindow<EditorWindow>();

            ManualEventManager.GetEvent<AddEditableFilesEvent>()
                .Publish(new AddEditableFilesEvent(file));
        }
        
        private void Expand(TreeViewNodeModel item, bool expand = true)
        {
            item.IsExpanded = expand;
            
            foreach (TreeViewNodeModel child in item.Children)
                Expand(child, expand);
        }

        private static void Enable()
        {
            WindowManager.EnableWindow<MainWindow>();
        }

        private static void Disable()
        {
            WindowManager.DisableWindow<MainWindow>();
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
            Dispatcher.InvokeAction(() => LogBox.ScrollToEnd());

            //Dispatcher.InvokeAction(() => LogItems.Add(new LogItem(text)));

            return text;
        }

        public void ClearVisLog()
        {
            _logTextBuilder.Clear();
            OnPropertyChanged(nameof(LogBoxText));
        }

        private bool TryCreateNewLog(string logPath)
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

        #endregion

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            var handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
