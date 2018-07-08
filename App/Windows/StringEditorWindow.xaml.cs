using System;
using System.ComponentModel;
using System.Windows.Input;
using AndroidTranslator.Interfaces.Strings;
using MVVM_Tools.Code.Commands;
using TranslatorApk.Logic.EventManagerLogic;
using TranslatorApk.Logic.Events;
using TranslatorApk.Logic.Interfaces;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.Utils;

namespace TranslatorApk.Windows
{
    public partial class StringEditorWindow : IRaisePropertyChanged
    {
        public IOneString Str
        {
            get => _str;
            set
            {
                if (this.SetRefProperty(ref _str, value))
                    _backup = value.NewText;
            }

        }
        private IOneString _str;

        public ActionCommand GoToPreviousStringCommand { get; }
        public ActionCommand GoToNextStringCommand { get; }

        private string _backup;
        private bool _scrollToEnd;

        public StringEditorWindow()
        {
            GoToPreviousStringCommand = new ActionCommand(GoToPreviousExecute, GoToPreviousCanExecute);
            GoToNextStringCommand = new ActionCommand(GoToNextExecute, GoToNextCanExecute);

            InitializeComponent();

            ManualEventManager.GetEvent<EditStringEvent>().Subscribe(EditStringEventHandler);
        }

        private bool GoToPreviousCanExecute()
        {
            if (Str == null)
                return false;

            var editorWindow = WindowManager.GetActiveWindow<EditorWindow>();

            var nextString = editorWindow.GetPreviousString(Str);

            return nextString.str != null;
        }

        private void GoToPreviousExecute()
        {
            var editorWindow = WindowManager.GetActiveWindow<EditorWindow>();

            var previousString = editorWindow.GetPreviousString(Str);

            if (previousString.str == null)
                return;

            SaveCurrent();

            ManualEventManager
                .GetEvent<EditStringEvent>()
                .Publish(new EditStringEvent(previousString.str, previousString.container));
        }

        private bool GoToNextCanExecute()
        {
            if (Str == null)
                return false;

            var editorWindow = WindowManager.GetActiveWindow<EditorWindow>();

            var nextString = editorWindow.GetNextString(Str);

            return nextString.str != null;
        }

        private void GoToNextExecute()
        {
            var editorWindow = WindowManager.GetActiveWindow<EditorWindow>();

            var nextString = editorWindow.GetNextString(Str);

            if (nextString.str == null)
                return;

            SaveCurrent();

            ManualEventManager
                .GetEvent<EditStringEvent>()
                .Publish(new EditStringEvent(nextString.str, nextString.container));
        }

        private void EditStringEventHandler(EditStringEvent editStringEvent)
        {
            if (editStringEvent.ContainerFile != null)
            {
                var file = editStringEvent.ContainerFile.FileName;

                if (GlobalVariables.CurrentProjectFolder != null)
                    file = $"...{file.Substring(GlobalVariables.CurrentProjectFolder.Length)}";

                Title = $"{file}: {editStringEvent.StringToEdit.Name}";
            }

            Str = editStringEvent.StringToEdit;
            _scrollToEnd = editStringEvent.ScrollToEnd;

            if (!string.IsNullOrEmpty(editStringEvent.Typed))
                Str.NewText += editStringEvent.Typed;

            if (Str.IsOldTextReadOnly)
            {
                NewTextBox.Focus();

                if (_scrollToEnd && Str.NewText != null)
                    NewTextBox.CaretIndex = Str.NewText.Length;
            }
            else
            {
                OldTextBox.Focus();
            }

            UpdateCommands();
        }

        private void StringEditorWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    if (GlobalVariables.AppSettings.AlternativeEditingKeys && Str != null)
                        Str.NewText = _backup;

                    e.Handled = true;
                    Close();  

                    break;
                case Key.Enter:
                    if (GlobalVariables.AppSettings.AlternativeEditingKeys && (e.KeyboardDevice.Modifiers & ModifierKeys.Shift) == 0)
                    {
                        e.Handled = true;
                        Close();
                    }

                    break;
            }
        }

        private void UpdateCommands()
        {
            GoToPreviousStringCommand.RaiseCanExecuteChanged();
            GoToNextStringCommand.RaiseCanExecuteChanged();
        }

        private void SaveCurrent()
        {
            if (Str != null && Str.NewText != _backup)
            {
                CommonUtils.AddToSessionDict(Str.OldText, Str.NewText);

                if (GlobalVariables.AppSettings.SessionAutoTranslate)
                {
                    ManualEventManager.GetEvent<EditorWindowTranslateTextEvent>()
                        .Publish(new EditorWindowTranslateTextEvent(Str.OldText, Str.NewText,
                            EditorWindowTranslateTextEvent.NotDictionaryFileFilter));
                }
            }
        }

        private void StringEditorWindow_OnClosed(object sender, EventArgs e)
        {
            SaveCurrent();

            ManualEventManager.GetEvent<EditStringEvent>().Unsubscribe(EditStringEventHandler);
        }

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
