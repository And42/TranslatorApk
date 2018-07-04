using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;
using System.Windows.Threading;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.Utils;
using TranslatorApk.Logic.ViewModels.TreeViewModels;
using TranslatorApk.Logic.ViewModels.Windows.MainWindow;
using UsefulClasses;

using Point = System.Drawing.Point;

namespace TranslatorApk.Windows
{
    public partial class MainWindow
    {
        private const int ItemPreviewDelayMs = 500;
        private static readonly TimeSpan ItemPreviewDelay = TimeSpan.FromMilliseconds(ItemPreviewDelayMs);

        private DispatcherTimer _fileImagePreviewTimer;
        private readonly PreviewWindowHandler _fileImagePreviewWindowHandler = new PreviewWindowHandler();

        internal MainWindowViewModel ViewModel
        {
            get => DataContext as MainWindowViewModel;
            private set => DataContext = value;
        }

        static MainWindow()
        {
            MainWindowViewModel.AndroidLogger = new Logger(GlobalVariables.PathToLogs, false);
        }

        public MainWindow(string[] args)
        {
            InitializeComponent();

            TaskbarItemInfo = new TaskbarItemInfo();

            ViewModel = new MainWindowViewModel(args, this);

            LoadSettings();

            ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
        }

        #region Drap&Drop

        private void Apk_Drop(object sender, DragEventArgs e)
        {
            ViewModel.ApkDropCommand.Execute(e);
        }

        private void ChooseFolder_Drop(object sender, DragEventArgs e)
        {
            ViewModel.FolderDropCommand.Execute(e);
        }

        private void Framework_Drop(object sender, DragEventArgs e)
        {
            ViewModel.FrameworkDropCommand.Execute(e);
        }

        private void TreeView_Drop(object sender, DragEventArgs e)
        {
            ViewModel.TV_DropCommand.Execute(e);
        }

        #endregion

        private void StartPreviewTimer()
        {
            _fileImagePreviewTimer =
                new DispatcherTimer(
                    ItemPreviewDelay,
                    DispatcherPriority.Normal,
                    PreviewTimerCallback,
                    Dispatcher
                );
        }

        private void PreviewTimerCallback(object sender, EventArgs args)
        {
            CancelPreviewTimer();

            var pos = PointToScreen(Mouse.GetPosition(this));

            _fileImagePreviewWindowHandler.Update(pos);
        }

        private void CancelPreviewTimer()
        {
            if (_fileImagePreviewTimer != null)
            {
                _fileImagePreviewTimer.IsEnabled = false;
                _fileImagePreviewTimer = null;
            }
        }

        private void LoadSettings()
        {
            Point size = DefaultSettingsContainer.Instance.MainWindowSize;

            if (!size.IsEmpty)
            {
                Width = size.X;
                Height = size.Y;
            }
        }

        private void OneFileDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ViewModel.OneFileDoubleClick(sender, e);
        }

        private void TreeView_KeyUp(object sender, KeyEventArgs e)
        {
            ViewModel.TreeView_KeyUp(sender, e);   
        }

        public void TreeViewItem_MouseEnter(object sender, MouseEventArgs e)
        {
            if (!DefaultSettingsContainer.Instance.ShowPreviews)
                return;

            var node = sender.As<FrameworkElement>().DataContext.As<FilesTreeViewNodeModel>();

            if (node.Options.HasPreview)
            {
                _fileImagePreviewWindowHandler.Init(node.Image);
                StartPreviewTimer();
            }
        }

        public void TreeViewItem_MouseMove(object sender, MouseEventArgs e)
        {
            if (!DefaultSettingsContainer.Instance.ShowPreviews)
                return;

            var node = sender.As<FrameworkElement>().DataContext.As<FilesTreeViewNodeModel>();

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

        public void TreeViewItem_MouseLeave(object sender, MouseEventArgs e)
        {
            if (!DefaultSettingsContainer.Instance.ShowPreviews)
                return;

            var node = (sender as FrameworkElement)?.DataContext as FilesTreeViewNodeModel;

            if (node?.Options.HasPreview == true)
            {
                CancelPreviewTimer();

                _fileImagePreviewWindowHandler.Close();
            }
        }

        private void TreeViewElement_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            var model = sender.As<FrameworkElement>().DataContext.As<FilesTreeViewNodeModel>();

            var builder = new ContextMenuBuilder();

            ViewModel.FillTreeItemContextMenu(builder.GetItemsBuilder(), model);

            builder.Build().IsOpen = true;
        }

        private void TreeView_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;

            var builder = new ContextMenuBuilder();

            ViewModel.FillTreeContextMenu(builder.GetItemsBuilder());

            builder.Build().IsOpen = true;
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(ViewModel.TV_FilteringBoxIsVisible):
                    if (ViewModel.TV_FilteringBoxIsVisible.Value)
                        Dispatcher.BeginInvokeAction(DispatcherPriority.Render, () => FilterBox.Focus());

                    break;
            }
        }

        private void FilterBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    ViewModel.TV_CloseFilterBoxCommand.Execute(null);
                    e.Handled = true;
                    break;
            }
        }

        private async void MainWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadItems();
        }

        private void MainWindow_OnClosed(object sender, EventArgs e)
        {
            ViewModel.PropertyChanged -= ViewModelOnPropertyChanged;
            ViewModel.Dispose();

            MainWindowViewModel.AndroidLogger.Stop();
            DefaultSettingsContainer.Instance.MainWindowSize = new Point((int)Width, (int)Height);

            Application.Current.Shutdown();
        }
    }
}
