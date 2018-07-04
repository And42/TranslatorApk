using System;
using MVVM_Tools.Code.Commands;
using TranslatorApk.Windows;

namespace TranslatorApk.Logic.ViewModels.Windows.MainWindow
{
    internal partial class MainWindowViewModel
    {
        public IActionCommand AddNewLanguageCommand { get; private set; }
        public IActionCommand RemoveLanguagesCommand { get; private set; }

        private void InitCommon()
        {
            ActionCommand ActCom(Action action) => new ActionCommand(action);

            AddNewLanguageCommand = ActCom(AddNewLanguageCommand_Execute);
            RemoveLanguagesCommand = ActCom(RemoveLanguagesCommand_Execute);

            InitMenuPart();
            InitTreeViewPart();
			InitMainButtonsPart();
        }

        private static void AddNewLanguageCommand_Execute()
        {
            new AddLanguageWindow().ShowDialog();
        }

        private static void RemoveLanguagesCommand_Execute()
        {
            new RemoveLanguagesWindow().ShowDialog();
        }
    }
}
