using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Shell;
using System.Windows.Threading;
using AndroidLibs;
using AndroidTranslator;
using Microsoft.WindowsAPICodePack.Dialogs;
using TranslatorApk.Annotations;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.EventManagerLogic;
using TranslatorApk.Logic.Events;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.PluginItems;
using TranslatorApkPluginLib;
using UsefulClasses;
using UsefulFunctionsLib;

using LocRes = TranslatorApk.Resources.Localizations.Resources;
using DataFormats = System.Windows.DataFormats;
using DragDropEffects = System.Windows.DragDropEffects;
using DragEventArgs = System.Windows.DragEventArgs;
using KeyEventArgs = System.Windows.Input.KeyEventArgs;
using MenuItem = System.Windows.Controls.MenuItem;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Point = System.Drawing.Point;
using Settings = TranslatorApk.Properties.Settings;
using MButtons = TranslatorApk.Windows.MessBox.MessageButtons;
using SetInc = TranslatorApk.Logic.OrganisationItems.SettingsIncapsuler;

namespace TranslatorApk.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        #region Поля

        public static MainWindow Instance { get; private set; }

        private static string[] _arguments;

        private readonly Timer _fileImagePreviewTimer = new Timer { Interval = 500 };
        private PreviewWindow _fileImagePreviewWindow;

        #endregion

        #region Свойства

        public TreeViewNodeModel FilesTreeViewModel { get; } = new TreeViewNodeModel(null); 

        public WindowState MainWindowState
        {
            get
            {
                if (_isMinimized)
                    return WindowState.Minimized;

                return SetInc.MainWMaximized ? WindowState.Maximized : WindowState.Normal;
            }
            set
            {
                if (value != WindowState.Minimized)
                    SetInc.MainWMaximized = value == WindowState.Maximized;

                _isMinimized = value == WindowState.Minimized;

                OnPropertyChanged(nameof(MainWindowState));
            }
        }
        private bool _isMinimized;

        public Apktools Apk;

        public Setting<bool>[] MainWindowSettings { get; }

        public string LogBoxText
        {
            get => _logTextBuilder.ToString();
            set
            {
                _logTextBuilder.Clear();
                _logTextBuilder.Append(value);
                OnPropertyChanged(nameof(LogBoxText));
            }
        }

        private static readonly Logger AndroidLogger = new Logger(GlobalVariables.PathToLogs, false);

        private readonly StringBuilder _logTextBuilder = new StringBuilder();

        //public ObservableCollection<LogItem> LogItems { get; } = new ObservableCollection<LogItem>();

        #endregion

        public MainWindow(string[] args)
        {
            _arguments = args;

            MainWindowSettings = new []
            {
                new Setting<bool>(nameof(SetInc.EmptyXml),        LocRes.EmptyXml,        val => SetInc.EmptyXml = val, false),
                new Setting<bool>(nameof(SetInc.EmptySmali),      LocRes.EmptySmali,      val => SetInc.EmptySmali = val, false),
                new Setting<bool>(nameof(SetInc.EmptyFolders),    LocRes.EmptyFolders,    val => SetInc.EmptyFolders = val, false),
                new Setting<bool>(nameof(SetInc.Images),          LocRes.Images,          val => SetInc.Images = val, false),
                new Setting<bool>(nameof(SetInc.FilesWithErrors), LocRes.FilesWithErrors, val => SetInc.FilesWithErrors = val, false),
                new Setting<bool>(nameof(SetInc.OnlyXml),         LocRes.OnlyXml,         val => SetInc.OnlyXml = val, false),
                new Setting<bool>(nameof(SetInc.OtherFiles),      LocRes.OtherFiles,      val => SetInc.OtherFiles = val, false),
                new Setting<bool>(nameof(SetInc.OnlyResources),   LocRes.OnlyResources,   val => SetInc.OnlyResources = val, false)
            };

            InitializeComponent();

            Instance = this;

            LoadSettings();

            _fileImagePreviewTimer.Tick += (o, arg) =>
            {
                _fileImagePreviewTimer.Stop();

                var pos = System.Windows.Forms.Cursor.Position;
                _fileImagePreviewWindow.Left = pos.X + 5;
                _fileImagePreviewWindow.Top = pos.Y + 5;

                _fileImagePreviewWindow.Show();
            };

            TaskbarItemInfo = new TaskbarItemInfo();
        }

        #region Кнопки

        /// <summary>
        /// Обрабатывает нажатие на кнопку "Выбрать файл"
        /// </summary>
        private void ChooseFileClick(object sender = null, RoutedEventArgs e = null)
        {
            var fd = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = ".apk",
                Filter = LocRes.AndroidApps + @" (*.apk)|*.apk",
                Multiselect = false
            };

            if (fd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            DecompileFile(fd.FileName);
        }

        /// <summary>
        /// Обрабатывает нажатие на кнопку "Выбрать папку"
        /// </summary>
        private void ChooseFolderClick(object sender = null, RoutedEventArgs e = null)
        {
            var dialog = new CommonOpenFileDialog
            {
                Title = LocRes.SelectAFolder,
                Multiselect = false,
                IsFolderPicker = true,
                EnsurePathExists = true
            };
            
            if (dialog.ShowDialog() != CommonFileDialogResult.Ok) return;

            LoadFolder(dialog.FileName);
        }

        /// <summary>
        /// Обрабатывает нажатие на кнопку "Собрать проект"
        /// </summary>
        private void BuildClick(object sender = null, RoutedEventArgs e = null)
        {
            if (Apk == null) return;

            if (!Apk.HasJava())
            {
                MessBox.ShowDial(LocRes.JavaNotFoundError, LocRes.ErrorLower);
                return;
            }

            Disable();
            LogBoxText = "";
            bool success = false;
            List<Error> errors = new List<Error>();

            LoadingWindow.ShowWindow(() => { }, source => success = Apk.Compile(out errors), () =>
            {
                Enable();
                VisLog(Log("".PadLeft(30, '-')));
                VisLog(Log(success ? LocRes.Finished : LocRes.ErrorWhileCompiling));
                VisLog(Log("".PadLeft(30, '-')));

                if (SettingsIncapsuler.ShowNotifications)
                {
                    ServiceLocator.GetInstance<NotificationService>().ShowMessage(LocRes.CompilationFinished);
                }

                if (!success && errors.Any(error => error.Type != Error.ErrorType.None))
                {
                    if (MessBox.ShowDial("Обнаружены ошибки. Попробовать исправить?", "", MButtons.Yes, MButtons.No) == MButtons.Yes)
                    {
                        Apktools.FixErrors(errors);
                        BuildClick();
                    }
                }
                else
                {
                    VisLog(Log(LocRes.FileIsSituatedIn + " " + Apk.NewApk));
                }
            }, Visibility.Collapsed);
        }

        /// <summary>
        /// Обрабатывает нажатие на кнопку "Framework"
        /// </summary>
        private void InstallFrameworkClick(object sender = null, RoutedEventArgs e = null)
        {
            var fd = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = ".apk",
                Filter = LocRes.AndroidApps + @" (*.apk)|*.apk",
                Multiselect = false
            };

            if (fd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            InstallFramework(fd.FileName);
        }

        /// <summary>
        /// Обрабатывает нажатие на кнопку "Подписать"
        /// </summary>
        private void SignClick(object sender = null, RoutedEventArgs e = null)
        {
            if (Apk == null) return;

            if (!Apk.HasJava())
            {
                MessBox.ShowDial(LocRes.JavaNotFoundError, LocRes.ErrorLower);
                return;
            }

            Disable();
            bool success = false;

            string line = "".PadLeft(30, '-');

            LoadingWindow.ShowWindow(() => { }, source => success = Apk.Sign(), () =>
            {
                Enable();
                VisLog(Log(line));
                VisLog(Log(success ? LocRes.Finished : LocRes.ErrorWhileSigning));

                if (success)
                {
                    string message = $"{LocRes.FileIsSituatedIn} {Apk.SignedApk}";

                    VisLog(Log(message));

                    string dir = Path.GetDirectoryName(Apk.SignedApk);

                    if (dir != null && MessBox.ShowDial(message, LocRes.Finished, MessBox.MessageButtons.OK, LocRes.Open) == LocRes.Open)
                    {
                        Process.Start(dir);
                    }
                }
            }, Visibility.Collapsed);

            VisLog(Log(string.Format("{0}{1}Signing...{1}{0}", line, Environment.NewLine)));
        }

        /// <summary>
        /// Обрабатывает нажатие на кнопку "Поиск"
        /// </summary>
        private void SearchClick(object sender = null, RoutedEventArgs e = null)
        {
            new SearchWindow().ShowDialog();
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

        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            if (!GlobalVariables.CurrentProjectFolder.NE())
                LoadFolder(GlobalVariables.CurrentProjectFolder);
        }

        private void ExpandClick(object sender = null, RoutedEventArgs e = null)
        {
            foreach (TreeViewNodeModel item in FilesTreeViewModel.Children)
                item.IsExpanded = true;
        }

        private void CollapseClick(object sender = null, RoutedEventArgs e = null)
        {
            foreach (TreeViewNodeModel item in FilesTreeViewModel.Children)
                Expand(item, false);
        }

        private void AddNewLanguageClick(object sender = null, RoutedEventArgs e = null)
        {
            new AddLanguageWindow(GlobalVariables.CurrentProjectFolder).ShowDialog();
        }

        #endregion

        #region Кнопки контекстного меню файла

        private void OpenClick(object sender, RoutedEventArgs e)
        {
            dynamic send = sender;
            TreeViewNodeModel node = send.DataContext;
            if (node.Options.IsFolder) return;

            Process.Start(node.Options.FullPath);
        }

        private void OpenWithClick(object sender, RoutedEventArgs e)
        {
            dynamic send = sender;
            TreeViewNodeModel node = send.DataContext;
            if (node.Options.IsFolder) return;

            Functions.OpenAs(node.Options.FullPath);
        }

        private void ReplaceFileClick(object sender, RoutedEventArgs e)
        {
            dynamic send = sender;
            TreeViewNodeModel node = send.DataContext;
            if (node.Options.IsFolder) return;

            Options opts = node.Options;
            var fd = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false
            };
            if (fd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                File.Copy(fd.FileName, opts.FullPath, true);
                MessBox.ShowDial(LocRes.Finished);
            }
            else
                MessBox.ShowDial(LocRes.ErrorLower);
        }

        private void DeleteFileClick(object sender, RoutedEventArgs e)
        {
            dynamic send = sender;
            TreeViewNodeModel node = send.DataContext;

            DeleteFilePromt(node);
        }

        #endregion

        #region Кнопки контекстного меню папки

        private void OpenInExplorerClick(object sender, RoutedEventArgs e)
        {
            TreeViewNodeModel node = sender.As<MenuItem>().DataContext.As<TreeViewNodeModel>();

            Functions.ShowInExplorer(node.Options.FullPath);
        }

        private void DeleteFolderClick(object sender, RoutedEventArgs e)
        {
            dynamic send = sender;
            TreeViewNodeModel node = send.DataContext;

            DeleteFolderPromt(node);

        }

        private void ExpandFolderClick(object sender, RoutedEventArgs e)
        {
            dynamic send = sender;
            TreeViewNodeModel node = send.DataContext;

            Expand(node);
        }

        private void CollapseFolderClick(object sender, RoutedEventArgs e)
        {
            dynamic send = sender;
            TreeViewNodeModel node = send.DataContext;
            Expand(node, false);
        }

        #endregion

        #region События окна

        private void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (SetInc.ApktoolVersion.NE())
            {
                MessBox.ShowDial(LocRes.ApktoolNotFound);
            }

            if (_arguments.Length == 1)
            {
                var file = _arguments[0];
                var ext = Path.GetExtension(file);

                if (Directory.Exists(file))
                {
                    LoadFolder(file);
                    return;
                }

                if (!File.Exists(file)) return;

                switch (ext)
                {
                    case ".xml":
                    case ".smali":
                        Functions.LoadFile(file);
                        break;
                    case ".apk":
                        DecompileFile(file);
                        break;
                    case ".yml":
                        LoadFolder(Path.GetDirectoryName(file));
                        break;
                }
            }

            Task.Factory.StartNew(Functions.LoadPlugins);
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            AndroidLogger.Stop();
            SetInc.MainWindowSize = new Point((int)Width, (int)Height);
        }

        private void MainWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && e.Key == Key.F)
                SearchClick();
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
            if (files != null && files.Length == 1 && Path.GetExtension(files[0]) == ".apk") DecompileFile(files[0]);
            e.Handled = true;
        }

        private void FolderDragDrop(object sender, DragEventArgs e)
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files != null && files.Length == 1 && Directory.Exists(files[0])) LoadFolder(files[0]);
            e.Handled = true;
        }

        private void FrameworkDrop(object sender, DragEventArgs e)
        {
            var files = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (files != null && files.Length == 1 && Path.GetExtension(files[0]) == ".apk") InstallFramework(files[0]);
            e.Handled = true;
        }

        private void FilesTreeView_OnDrop(object sender, DragEventArgs e)
        {
            e.Handled = true;
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;

            var list = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (list == null) return;

            List<EditableFile> files = list.Select(Functions.GetSuitableEditableFile).Where(it => it != null).ToList();

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
            if (e.ChangedButton != MouseButton.Left || e.ClickCount != 2) return;

            TreeViewNodeModel node = sender.As<FrameworkElement>().DataContext.As<TreeViewNodeModel>();

            if (node.Options.IsFolder) return;

            if (node.DoubleClicked != null)
            {
                node.DoubleClicked.Invoke();
            }
            else
            {
                Functions.LoadFile(node.Options.FullPath);
            }
        }

        private void LoadXmlFiles()
        {
            var filesList = new List<XmlFile>();
            LoadingProcessWindow.ShowWindow(
                Disable,
                cts => {
                    LoadingProcessWindow.Instance.IsIndeterminate = true;

                    var files = Directory.GetFiles(GlobalVariables.CurrentProjectFolder, "*.xml", SearchOption.AllDirectories);

                    LoadingProcessWindow.Instance.IsIndeterminate = false;

                    LoadingProcessWindow.Instance.ProcessMax = files.Length;

                    filesList = new List<XmlFile>(files.Length);

                    Parallel.ForEach(files, file =>
                    {
                        if (!Functions.CheckFilePath(file)) return;
                        if (cts.IsCancellationRequested) return;

                        filesList.Add(XmlFile.Create(file));

                        LoadingProcessWindow.Instance.ProcessValue++;
                    });
                },
                () =>
                { 
                    List<EditableFile> res = filesList.FindAll(file => file.Details != null && file.Details.Count > 0).ConvertAll(f => (EditableFile)f);

                    Enable();

                    WindowManager.ActivateWindow<EditorWindow>();

                    ManualEventManager.GetEvent<AddEditableFilesEvent>()
                        .Publish(new AddEditableFilesEvent(res));
                }
            );
        }

        private void LoadSmaliFiles()
        {
            var filesList = new List<SmaliFile>();
            LoadingProcessWindow.ShowWindow(
                Disable,
                cts =>
                {
                    LoadingProcessWindow.Instance.IsIndeterminate = true;

                    var files = Directory.GetFiles(GlobalVariables.CurrentProjectFolder, "*.smali", SearchOption.AllDirectories);

                    LoadingProcessWindow.Instance.IsIndeterminate = false;

                    LoadingProcessWindow.Instance.ProcessMax = files.Length;
                    filesList = new List<SmaliFile>(files.Length);

                    Parallel.ForEach(files, file =>
                    {
                        if (!Functions.CheckFilePath(file)) return;
                        if (cts.IsCancellationRequested) return;
                        filesList.Add(new SmaliFile(file));
                        LoadingProcessWindow.Instance.ProcessValue++;
                    });
                },
                () =>
                {
                    List<EditableFile> res = 
                        filesList
                            .Where(file => file.Details != null && file.Details.Count > 0)
                            .Select(f => (EditableFile)f).ToList();

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
            if (!SettingsIncapsuler.ShowPreviews)
                return;

            var node = sender.As<FrameworkElement>().DataContext.As<TreeViewNodeModel>();

            if (node.Options.HasPreview)
            {
                _fileImagePreviewWindow = new PreviewWindow(node.Image);
                _fileImagePreviewTimer.Start();
            }
        }

        private void TreeViewItem_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!SettingsIncapsuler.ShowPreviews)
                return;

            var node = (sender as FrameworkElement)?.DataContext as TreeViewNodeModel;

            if (node?.Options.HasPreview == true)
            {
                _fileImagePreviewTimer.Stop();
                try
                {
                    _fileImagePreviewWindow?.Close();
                }
                catch (Exception)
                {
                    // ignored
                }
                _fileImagePreviewWindow = null;
            }
        }

        private void TreeViewItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (!SettingsIncapsuler.ShowPreviews)
                return;

            var node = sender.As<FrameworkElement>().DataContext.As<TreeViewNodeModel>();

            if (node.Options.HasPreview)
            {
                _fileImagePreviewTimer.Stop();
                _fileImagePreviewTimer.Start();

                try
                {
                    _fileImagePreviewWindow.Close();
                    _fileImagePreviewWindow = new PreviewWindow(node.Image);
                }
                catch (Exception)
                {
                    // ignored
                }
            }
        }

        #endregion

        #region Functions

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
            var found = AddActionsMenuItem.Items.Cast<MenuItem>()
                .FirstOrDefault(it => (it.DataContext as PluginPart<IAdditionalAction>)?.Item.Guid == actionGuid);

            if (found != null)
                AddActionsMenuItem.Items.Remove(found);
        }

        private void PluginItem_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            PluginPart<IAdditionalAction> act = sender.As<MenuItem>().DataContext.As<PluginPart<IAdditionalAction>>();

            act.Item.Process(Apk?.FileName, Apk?.FolderOfProject, Apk?.NewApk, Apk?.SignedApk,
                GlobalVariables.PathToResources,
                GlobalVariables.PathToFiles,
                GlobalVariables.PathToResources + "\\jre",
                $"{GlobalVariables.PathToApktoolVersions}\\apktool_{Settings.Default.ApktoolVersion}.jar",
                $"{GlobalVariables.PathToPlugins}\\{act.Host.Name}");

            //Debug.WriteLine(AppDomain.CurrentDomain.GetAssemblies().Select(it => it.FullName).JoinStr("\n"));
        }

        private void DeleteSmthPromt(TreeViewNodeModel node, string confirmation, Action<string> deleteAction, Func<string, bool> checkAction)
        {
            Options opts = node.Options;

            if (MessBox.ShowDial($"{confirmation} {opts.FullPath}?", LocRes.Confirmation, LocRes.Yes, LocRes.No) == LocRes.Yes)
            {
                try
                {
                    deleteAction(opts.FullPath);

                    if (checkAction(opts.FullPath))
                        MessBox.ShowDial(LocRes.ErrorLower);
                    else
                        node.RemoveFromParent();
                }
                catch (Exception)
                {
                    MessBox.ShowDial(LocRes.ErrorLower);
                }
            }
        }

        private void DeleteFilePromt(TreeViewNodeModel node)
        {
            DeleteSmthPromt(node, LocRes.FileDeleteConfirmation, File.Delete, File.Exists);
        }

        private void DeleteFolderPromt(TreeViewNodeModel node)
        {
            DeleteSmthPromt(node, LocRes.FolderDeleteConfirmation, str => Directory.Delete(str, true), Directory.Exists);
        }
     
        private void LoadSettings()
        {
            Point size = Settings.Default.MainWindowSize;

            if (!size.IsEmpty)
            {
                Width = size.X;
                Height = size.Y;
            }
        }

        public void DecompileFile(string file)
        {
            GlobalVariables.CurrentProjectFile = file;

            AndroidLogger.NewLog(true, $"{Path.GetDirectoryName(file)}\\{Path.GetFileNameWithoutExtension(file)}_log.txt");

            Apk = new Apktools(file, GlobalVariables.PathToResources, $"{GlobalVariables.PathToApktoolVersions}\\apktool_{Settings.Default.ApktoolVersion}.jar");
            Apk.Logging += s => VisLog(Log(s));

            if (!Apk.HasJava())
            {
                MessBox.ShowDial(LocRes.JavaNotFoundError, LocRes.ErrorLower);
                return;
            }
            
            LogBoxText = string.Empty;
            Disable();
            bool success = false;
            LoadingWindow.ShowWindow(() => {}, source => success = SettingsIncapsuler.OnlyResources ? Apk.Decompile(options: "-s") : Apk.Decompile(), () =>
            {
                Enable();
                VisLog(GlobalVariables.LogLine);
                VisLog(LocRes.Finished);
                VisLog(GlobalVariables.LogLine);

                if (SettingsIncapsuler.ShowNotifications)
                {
                    ServiceLocator.GetInstance<NotificationService>().ShowMessage(LocRes.DecompilationFinished);
                }

                if (success)
                {
                    Dispatcher.InvokeAction(() => LoadFolder(GlobalVariables.CurrentProjectFolder, true));
                }
            }, Visibility.Collapsed);
        }

        private void InstallFramework(string fileName)
        {
            Disable();

            Task proc = new Task(() =>
            {
                var apktool = new Apktools(null, GlobalVariables.PathToResources,
                    $"{GlobalVariables.PathToApktoolVersions}\\apktool_{Settings.Default.ApktoolVersion}.jar");

                if (!apktool.HasJava())
                {
                    MessBox.ShowDial(LocRes.JavaNotFoundError, LocRes.ErrorLower);
                    return;
                }

                apktool.Logging += s => VisLog(s);
                apktool.InstallFramework(fileName);
            });

            proc.ContinueWith(task => Enable());
            proc.Start();
        }

        public void LoadFolder(string folderPath, bool haveLogger = false)
        {
            if (folderPath == null)
                return;

            if (!haveLogger)
            {
                AndroidLogger.NewLog(true, folderPath + "_log.txt");
            }

            GlobalVariables.CurrentProjectFile = folderPath + ".apk";

            Apk = new Apktools(GlobalVariables.CurrentProjectFile, GlobalVariables.PathToResources, $"{GlobalVariables.PathToApktoolVersions}\\apktool_{Settings.Default.ApktoolVersion}.jar");
            Apk.Logging += s => VisLog(Log(s));
            FilesTreeViewModel.Children.Clear();
            FilesTreeViewModel.Children.Add(new TreeViewNodeModel(null)
            {
                Name = LocRes.AllXml,
                Options = new Options("", true),
                DoubleClicked = LoadXmlFiles,
                Image = GlobalResources.Icon_UnknownFile
            });
            FilesTreeViewModel.Children.Add(new TreeViewNodeModel(null)
            {
                Name = LocRes.AllSmali,
                Options = new Options("", true),
                DoubleClicked = LoadSmaliFiles,
                Image = GlobalResources.Icon_UnknownFile
            });

            Dispatcher dispatcher = Dispatcher;

            LoadingProcessWindow.ShowWindow(() => Instance.IsEnabled = false,
                cts =>
                {
                    LoadingProcessWindow.Instance.IsIndeterminate = true;

                    LoadingProcessWindow.Instance.ProcessMax = Directory.EnumerateFiles(folderPath, "*", SearchOption.AllDirectories).Count();

                    LoadingProcessWindow.Instance.IsIndeterminate = false;

                    Functions.LoadFilesToTreeView(dispatcher, folderPath, FilesTreeViewModel, SettingsIncapsuler.EmptyFolders, cts, () => LoadingProcessWindow.Instance.ProcessValue++);
                },
                () =>
                {
                    Instance.IsEnabled = true;
                    foreach (var node in FilesTreeViewModel.Children) Functions.LoadIconForItem(node);
                });
        }

        private void LoadAllInXml(object sender, RoutedEventArgs e)
        {
            dynamic send = sender;
            TreeViewNodeModel node = send.DataContext;
            if (node.Options.IsFolder || node.Options.Ext != ".xml") return;

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

        public static void Enable()
        {
            Instance?.Dispatcher.InvokeAction(() => Instance.IsEnabled = true);
        }

        public static void Disable()
        {
            Instance?.Dispatcher.InvokeAction(() => Instance.IsEnabled = false);
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
