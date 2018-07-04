using System;
using System.IO;
using System.Windows.Media;
using MVVM_Tools.Code.Commands;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.PluginItems;
using TranslatorApk.Logic.Utils;
using TranslatorApk.Resources.Localizations;
using TranslatorApk.Windows;
using TranslatorApkPluginLib;

namespace TranslatorApk.Logic.ViewModels.Windows.MainWindow
{
    internal partial class MainWindowViewModel
    {
        public class PluginMenuItemModel
        {
            public string Title { get; }
            public IActionCommand<PluginPart<IAdditionalAction>> Command { get; }
            public ImageSource Icon { get; }
            public PluginPart<IAdditionalAction> Action { get; }

            public PluginMenuItemModel(string title, IActionCommand<PluginPart<IAdditionalAction>> command, ImageSource icon = null, PluginPart<IAdditionalAction> action = null)
            {
                Title = title;
                Command = command;
                Icon = icon;
                Action = action;
            }
        }

        public ObservableRangeCollection<PluginMenuItemModel> PluginMenuItems { get; private set; }

        public IActionCommand OpenAboutCommand { get; private set; }
        public IActionCommand OpenEditorCommand { get; private set; }
        public IActionCommand OpenPluginsCommand { get; private set; }
        public IActionCommand OpenSettingsCommand { get; private set; }
        public IActionCommand OpenXmlRulesCommand { get; private set; }
        public IActionCommand OpenChangesDetectorCommand { get; private set; }

        public IActionCommand<PluginPart<IAdditionalAction>> PluginItemCommand { get; private set; }

        private void InitMenuPart()
        {
            PluginMenuItems = new ObservableRangeCollection<PluginMenuItemModel>();

            ActionCommand ActCom(Action action) => new ActionCommand(action);

            OpenAboutCommand = ActCom(OpenAboutCommand_Execute);
            OpenEditorCommand = ActCom(OpenEditorCommand_Execute);
            OpenPluginsCommand = ActCom(OpenPluginsCommand_Execute);
            OpenSettingsCommand = ActCom(OpenSettingsCommand_Execute);
            OpenXmlRulesCommand = ActCom(OpenXmlRulesCommand_Execute);
            OpenChangesDetectorCommand = ActCom(OpenChangesDetectorCommand_Execute);

            PluginItemCommand = new ActionCommand<PluginPart<IAdditionalAction>>(PluginItemCommand_Execute);

            PluginMenuItems.Add(
                new PluginMenuItemModel(
                    StringResources.Catalog, OpenPluginsCommand,
                    ImageUtils.GetImageFromApp("Resources/Icons/folder.png")
                )
            );
        }

        private static void OpenSettingsCommand_Execute()
        {
            new SettingsWindow().ShowDialog();
        }

        private static void OpenEditorCommand_Execute()
        {
            WindowManager.ActivateWindow<EditorWindow>();
        }

        private static void OpenXmlRulesCommand_Execute()
        {
            new XmlRulesWindow().ShowDialog();
        }

        private static void OpenChangesDetectorCommand_Execute()
        {
            WindowManager.ActivateWindow<ChangesDetectorWindow>(ownerWindow: WindowManager.GetActiveWindow<TranslatorApk.Windows.MainWindow>());
        }

        private static void OpenPluginsCommand_Execute()
        {
            new PluginsWindow().ShowDialog();
        }

        private static void OpenAboutCommand_Execute()
        {
            new AboutProgramWindow().ShowDialog();
        }

        private void PluginItemCommand_Execute(PluginPart<IAdditionalAction> parameter)
        {
            parameter.Item.Process(Apk?.FileName, Apk?.FolderOfProject, Apk?.NewApk, Apk?.SignedApk,
                GlobalVariables.PathToResources,
                GlobalVariables.PathToFiles,
                Path.Combine(GlobalVariables.PathToResources, "jre"),
                Path.Combine(GlobalVariables.PathToApktoolVersions, $"apktool_{DefaultSettingsContainer.Instance.ApktoolVersion}.jar"),
                Path.Combine(GlobalVariables.PathToPlugins, parameter.Host.Name)
            );
        }
    }
}
