using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using AndroidTranslator;
using TranslatorApk.Annotations;
using TranslatorApk.Logic;
using TranslatorApk.Logic.EventManagerLogic;
using TranslatorApk.Logic.Events;
using TranslatorApk.Logic.OrganisationItems;

using Res = TranslatorApk.Resources.Localizations.Resources;

namespace TranslatorApk.Windows
{
    /// <summary>
    /// Логика взаимодействия для StringEditorWindow.xaml
    /// </summary>
    public partial class StringEditorWindow : INotifyPropertyChanged
    {
        public OneString Str { get; }

        private readonly string backup;
        private readonly bool scrollToEnd;

        public StringEditorWindow(OneString str, bool scrollToEnd = true, string prev = "")
        {
            Str = str;
            backup = str.NewText;
            str.NewText += prev;
            this.scrollToEnd = scrollToEnd;

            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void StringEditorWindow_OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    if (SettingsIncapsuler.AlternativeEditingKeys)
                        Str.NewText = backup;

                    e.Handled = true;
                    Close();  

                    break;
                case Key.Enter:
                    if (SettingsIncapsuler.AlternativeEditingKeys && (e.KeyboardDevice.Modifiers & ModifierKeys.Shift) == 0)
                    {
                        e.Handled = true;
                        Close();
                    }

                    break;
            }
        }

        private void StringEditorWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            if (Str.IsOldTextReadOnly)
            {
                NewTextBox.Focus();

                if (scrollToEnd && Str.NewText != null)
                    NewTextBox.CaretIndex = Str.NewText.Length;
            }
            else
                OldTextBox.Focus();
        }

        private void StringEditorWindow_OnClosed(object sender, EventArgs e)
        {
            if (Str.NewText != backup)
            {
                Functions.AddToSessionDict(Str.OldText, Str.NewText);

                if (SettingsIncapsuler.SessionAutoTranslate)
                {
                    ManualEventManager.GetEvent<EditorWindowTranslateTextEvent>()
                        .Publish(new EditorWindowTranslateTextEvent(Str.OldText, Str.NewText,
                            EditorWindowTranslateTextEvent.NotDictionaryFileFilter));
                }
            }
        }
    }
}
