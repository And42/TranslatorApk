using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AndroidTranslator.Classes.Files;
using AndroidTranslator.Interfaces.Files;
using Microsoft.Win32;
using MVVM_Tools.Code.Commands;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.EventManagerLogic;
using TranslatorApk.Logic.Events;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.Utils;
using TranslatorApk.Logic.ViewModels.TreeViewModels;
using TranslatorApk.Resources.Localizations;
using TranslatorApk.Windows;
using UsefulFunctionsLib;

using TVItemMenuCommand = MVVM_Tools.Code.Commands.IActionCommand<TranslatorApk.Logic.ViewModels.TreeViewModels.FilesTreeViewNodeModel>;

// ReSharper disable InconsistentNaming

namespace TranslatorApk.Logic.ViewModels.Windows.MainWindow
{
    internal partial class MainWindowViewModel
    {
        private class CommandContainer
        {
            public string Caption { get; }
            public ICommand Command { get; }
            public ImageSource Image { get; }
            public string Gesture { get; }

            public CommandContainer(string caption, ICommand command, string imageName, string gesture = null)
            {
                Caption = caption;
                Command = command;
                Image = ImageUtils.GetImageFromApp("Resources/Icons/" + imageName);
                Gesture = gesture;
            }
        }

        private class CommandContainers
        {
            public CommandContainer Expand { get; }
            public CommandContainer Collapse { get; }
            public CommandContainer OpenInExplorer { get; }
            public CommandContainer AddNewLanguage { get; }
            public CommandContainer RefreshFilesList { get; }
            public CommandContainer RemoveLanguages { get; }

            public CommandContainers(MainWindowViewModel vm)
            {
                Expand = Cont(StringResources.Expand, vm.TV_ExpandCommand, "folders_explorer.png");
                Collapse = Cont(StringResources.Collapse, vm.TV_CollapseCommand, "folders.png");
                OpenInExplorer = Cont(StringResources.OpenInExplorer, vm.TV_OpenInExplorerCommand, "folder.png");
                AddNewLanguage = Cont(StringResources.AddLanguage, vm.AddNewLanguageCommand, "change_language.png");
                RemoveLanguages = Cont(StringResources.RemoveLanguages, vm.RemoveLanguagesCommand, "change_language.png");
                RefreshFilesList = Cont(StringResources.Refresh, vm.TV_RefreshFilesListCommand, "arrow_refresh.png", "Ctrl+R");
            }

            private static CommandContainer Cont(string caption, ICommand command, string imageName, string gesture = null)
                => new CommandContainer(caption, command, imageName, gesture);
        }

        private CommandContainers _tvCommands;

        public FilesTreeViewNodeModel FilesFilesTreeViewModel { get; } = new FilesTreeViewNodeModel();

        public IActionCommand TV_ExpandCommand { get; private set; }
        public IActionCommand TV_CollapseCommand { get; private set; }
        public IActionCommand TV_RefreshFilesListCommand { get; private set; }

        public TVItemMenuCommand TV_OpenFileCommand { get; private set; }
        public TVItemMenuCommand TV_OpenWithCommand { get; private set; }
        public TVItemMenuCommand TV_ReplaceFileCommand { get; private set; }
        public TVItemMenuCommand TV_LoadAllInXmlCommand { get; private set; }
        public TVItemMenuCommand TV_ExpandFolderCommand { get; private set; }
        public TVItemMenuCommand TV_DeleteElementCommand { get; private set; }
        public TVItemMenuCommand TV_OpenInExplorerCommand { get; private set; }
        public TVItemMenuCommand TV_CollapseFolderCommand { get; private set; }

        public IActionCommand<DragEventArgs> TV_DropCommand { get; private set; }

        private void InitTreeViewPart()
        {
            ActionCommand ActCom(Action action) => new ActionCommand(action);
            TVItemMenuCommand TVCom(Action<FilesTreeViewNodeModel> action) => new ActionCommand<FilesTreeViewNodeModel>(action);

            TV_ExpandCommand = ActCom(TV_ExpandCommand_Execute);
            TV_CollapseCommand = ActCom(TV_CollapseCommand_Execute);
            TV_RefreshFilesListCommand = ActCom(TV_RefreshFilesListCommand_Execute);

            TV_OpenFileCommand = TVCom(TV_OpenFileCommand_Execute);
            TV_OpenWithCommand = TVCom(TV_OpenWithCommand_Execute);
            TV_ReplaceFileCommand = TVCom(TV_ReplaceFileCommand_Execute);
            TV_LoadAllInXmlCommand = TVCom(TV_LoadAllInXmlCommand_Execute);
            TV_ExpandFolderCommand = TVCom(TV_ExpandFolderCommand_Execute);
            TV_DeleteElementCommand = TVCom(TV_DeleteElementCommand_Execute);
            TV_OpenInExplorerCommand = TVCom(TV_OpenInExplorerCommand_Execute);
            TV_CollapseFolderCommand = TVCom(TV_CollapseFolderCommand_Execute);
            
            TV_DropCommand = new ActionCommand<DragEventArgs>(TV_DropCommand_Execute);

            _tvCommands = new CommandContainers(this);
        }

        #region Кнопки контекстного меню дерева

        private void TV_ExpandCommand_Execute()
        {
            FilesFilesTreeViewModel.Children.ForEach(it => it.IsExpanded = true);
        }

        private void TV_CollapseCommand_Execute()
        {
            FilesFilesTreeViewModel.Children.ForEach(it => Expand(it, false));
        }

        private void TV_RefreshFilesListCommand_Execute()
        {
            if (!GlobalVariables.CurrentProjectFolder.NE())
                LoadFolder(GlobalVariables.CurrentProjectFolder);
        }

        #endregion

        #region Кнопки контекстного меню элемента

        private static void TV_OpenFileCommand_Execute(FilesTreeViewNodeModel model)
        {
            if (IOUtils.FileExists(model.Options.FullPath))
                Process.Start(model.Options.FullPath);
        }

        private static void TV_OpenWithCommand_Execute(FilesTreeViewNodeModel model)
        {
            if (IOUtils.FileExists(model.Options.FullPath))
                Utils.Utils.OpenAs(model.Options.FullPath);
        }

        private static void TV_LoadAllInXmlCommand_Execute(FilesTreeViewNodeModel model)
        {
            if (model.Options.Ext != ".xml" || !IOUtils.FileExists(model.Options.FullPath))
                return;

            var file = new XmlFile(model.Options.FullPath, XmlFile.XmlRules, true);

            WindowManager.ActivateWindow<EditorWindow>();

            ManualEventManager.GetEvent<AddEditableFilesEvent>()
                .Publish(new AddEditableFilesEvent(file));
        }

        private static void TV_ReplaceFileCommand_Execute(FilesTreeViewNodeModel model)
        {
            if (!IOUtils.FileExists(model.Options.FullPath))
                return;

            var fd = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Multiselect = false
            };
            if (fd.ShowDialog() == true)
            {
                File.Copy(fd.FileName, model.Options.FullPath, true);
                MessBox.ShowDial(StringResources.Finished);
            }
            else
            {
                MessBox.ShowDial(StringResources.ErrorLower);
            }
        }

        private static void TV_OpenInExplorerCommand_Execute(FilesTreeViewNodeModel model)
        {
            Utils.Utils.ShowInExplorer(model.Options.FullPath);
        }

        private static void TV_DeleteElementCommand_Execute(FilesTreeViewNodeModel model)
        {
            var options = model.Options;

            if (!options.IsFolder && IOUtils.FileExists(options.FullPath))
                DeleteFilePromt(model);
            else if (options.IsFolder && IOUtils.FolderExists(options.FullPath))
                DeleteFolderPromt(model);
        }

        private static void TV_ExpandFolderCommand_Execute(FilesTreeViewNodeModel model)
        {
            Expand(model);
        }

        private static void TV_CollapseFolderCommand_Execute(FilesTreeViewNodeModel model)
        {
            Expand(model, false);
        }

        #endregion

        #region Заполнение контекстного меню

        private void AddMenuItem(ContextMenuBuilder.IItemsBuilder builder, CommandContainer container, object commandParameter)
        {
            var img = new Image { Source = container.Image };

            RenderOptions.SetBitmapScalingMode(img, BitmapScalingMode.NearestNeighbor);

            builder.Add(
                new MenuItem
                {
                    Header = container.Caption,
                    Command = container.Command,
                    CommandParameter = commandParameter,
                    Icon = img,
                    InputGestureText = container.Gesture,
                    FontSize = _windowControl.FontSize
                }
            );
        }

        public void FillTreeContextMenu(ContextMenuBuilder.IItemsBuilder builder)
        {
            CommandContainer[] items =
            {
                _tvCommands.RefreshFilesList,
                _tvCommands.AddNewLanguage,
                _tvCommands.RemoveLanguages,
                _tvCommands.Expand,
                _tvCommands.Collapse
            };

            items.ForEach(it => AddMenuItem(builder, it, null));
        }

        public void FillTreeItemContextMenu(ContextMenuBuilder.IItemsBuilder builder, FilesTreeViewNodeModel model)
        {
            if (model.Options.IsFolder)
                FillTreeItemFolderContextMenu(builder, model);
            else
                FillTreeItemFileContextMenu(builder, model);
        }

        private void FillTreeItemFolderContextMenu(ContextMenuBuilder.IItemsBuilder builder, FilesTreeViewNodeModel model)
        {
            CommandContainer[] items =
            {
                _tvCommands.OpenInExplorer,
                _tvCommands.RefreshFilesList,
                new CommandContainer(StringResources.Delete, TV_DeleteElementCommand, "folder_delete.png"),
                _tvCommands.AddNewLanguage,
                _tvCommands.RemoveLanguages,
                new CommandContainer(StringResources.Expand, TV_ExpandFolderCommand, "folders_explorer.png"),
                new CommandContainer(StringResources.Collapse, TV_CollapseFolderCommand, "folders.png")
            };

            items.ForEach(it => AddMenuItem(builder, it, model));
        }

        private void FillTreeItemFileContextMenu(ContextMenuBuilder.IItemsBuilder builder, FilesTreeViewNodeModel model)
        {
            CommandContainer[] items =
            {
                new CommandContainer(StringResources.Open, TV_OpenFileCommand, "open_source.png"),
                new CommandContainer(StringResources.OpenWith, TV_OpenWithCommand, "open_share.png"),
                new CommandContainer(StringResources.LoadWithTextStrings, TV_LoadAllInXmlCommand, "text.png"),
                new CommandContainer(StringResources.Replace, TV_ReplaceFileCommand, "card_file.png"),
                _tvCommands.OpenInExplorer,
                _tvCommands.RefreshFilesList,
                new CommandContainer(StringResources.Delete, TV_DeleteElementCommand, "page_delete.png"),
                _tvCommands.AddNewLanguage,
                _tvCommands.RemoveLanguages,
                _tvCommands.Expand,
                _tvCommands.Collapse
            };

            items.ForEach(it => AddMenuItem(builder, it, model));
        }

        #endregion

        #region Drag & Drop

        private static void TV_DropCommand_Execute(DragEventArgs args)
        {
            args.Handled = true;

            string[] list = args.GetFilesDrop();

            if (list == null)
                return;

            List<IEditableFile> files = list.Select(Utils.Utils.GetSuitableEditableFile).Where(it => it != null).ToList();

            WindowManager.ActivateWindow<EditorWindow>();

            if (files.Count == 0)
                return;

            ManualEventManager.GetEvent<AddEditableFilesEvent>()
                .Publish(new AddEditableFilesEvent(files, false));

            string fileName = files[0].FileName;

            ManualEventManager.GetEvent<EditorScrollToFileAndSelectEvent>()
                .Publish(new EditorScrollToFileAndSelectEvent(f => f.FileName.Equals(fileName, StringComparison.Ordinal)));
        }

        #endregion

        #region TreeViewEvents

        public void OneFileDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left || e.ClickCount != 2)
                return;

            var node = sender.As<FrameworkElement>().DataContext.As<FilesTreeViewNodeModel>();

            if (node.Options.IsFolder)
                return;

            if (node.DoubleClicked != null)
                node.DoubleClicked.Invoke();
            else
                Utils.Utils.LoadFile(node.Options.FullPath);

            e.Handled = true;
        }

        public void TreeView_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                var node = sender.As<TreeView>().SelectedItem as FilesTreeViewNodeModel;

                if (node == null)
                    return;

                Options opt = node.Options;

                if (opt.IsFolder)
                    DeleteFolderPromt(node);
                else
                    DeleteFilePromt(node);
            }
        }

        #endregion
    }
}
