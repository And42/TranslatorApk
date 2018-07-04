using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace TranslatorApk.Logic.UserControls
{
    public partial class EditableTextBlock
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
            "Text", typeof(string), typeof(EditableTextBlock), new PropertyMetadata(default(string)));

        public string Text
        {
            get => (string) GetValue(TextProperty);
            set => SetValue(TextProperty, value);
        }

        public static readonly DependencyProperty IsEditingProperty = DependencyProperty.Register(
            "IsEditing", typeof(bool), typeof(EditableTextBlock), new PropertyMetadata(default(bool)));

        public bool IsEditing
        {
            get => (bool) GetValue(IsEditingProperty);
            set => SetValue(IsEditingProperty, value);
        }

        private bool _clickedOnce;
        private Timer _mouseClickTimer;

        public EditableTextBlock()
        {
            InitializeComponent();
        }

        private void InitMouseClickTimer()
        {
            _mouseClickTimer = new Timer(state =>
            {
                _clickedOnce = false;
                _mouseClickTimer.Dispose();
                _mouseClickTimer = null;
            }, null, 300, Timeout.Infinite);
        }

        private void TextBlock_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (_clickedOnce)
            {
                _clickedOnce = false;
                IsEditing = true;
                TextBoxField.Focus();
                return;
            }

            _clickedOnce = true;

            InitMouseClickTimer();
        }

        private void TextBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                IsEditing = false;
        }

        private void TextBox_OnLostFocus(object sender, RoutedEventArgs e)
        {
            IsEditing = false;
        }
    }
}
