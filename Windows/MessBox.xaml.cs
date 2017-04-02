using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TranslatorApk.Logic;
using UsefulFunctionsLib;

using StringResources = TranslatorApk.Resources.Localizations.Resources;

namespace TranslatorApk.Windows
{
    /// <summary>
    /// Логика взаимодействия для MessBox.xaml
    /// </summary>
    public partial class MessBox
    {
        private static string _result;

        private bool _canClose;

        public static class MessageButtons
        {
            // ReSharper disable once InconsistentNaming
            public const string OK = "OK";
            public static string Yes => StringResources.Yes;
            public static string No => StringResources.No;
            public static string Cancel => StringResources.Cancel;
        }

        public MessBox()
        {
            InitializeComponent();
        }

        public static string ShowDial(string message, string caption = null, params string[] buttons)
        {
            Application.Current.Dispatcher.InvokeAction(() => new MessBox().ShowD(message, caption, buttons));
            return _result;
        }

        private void ShowD(string message, string caption = null, params string[] buttons)
        {
            if (!caption.NE())
            {
                Title = caption;
            }

            if (!message.NE())
            {
                MessLabel.Text = message;
            }

            if (buttons.Length == 0)
            {
                FirstButton.Content = MessageButtons.OK;
            }
            else
            {
                FirstButton.Content = buttons[0];

                for (var i = 1; i < buttons.Length; i++)
                {
                    var button = new Button
                    {
                        Content = buttons[i],
                        Margin = new Thickness(0, 15, 15, 15),
                        FontSize = 13,
                        Padding = new Thickness(20, 5, 20, 5)
                    };

                    button.Click += Button_Click;

                    Grid.SetColumn(button, ButtonsGrid.ColumnDefinitions.Count);

                    ButtonsGrid.ColumnDefinitions.Add(new ColumnDefinition {Width = new GridLength(1, GridUnitType.Star)});
                    ButtonsGrid.Children.Add(button);
                }
            }

            ShowDialog();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _result = sender.As<Button>().Content.As<string>();

            _canClose = true;
            Close();
        }

        private void MessBox_OnLoaded(object sender, RoutedEventArgs e)
        {
            Focus();

            ButtonsGrid.Children.OfType<Button>().LastOrDefault()?.Focus();
        }

        private void MessBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                _canClose = true;
                Close();
            }
        }

        private void MessBox_OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
            }
        }

        private void MessBox_OnClosing(object sender, CancelEventArgs e)
        {
            if (!_canClose)
            {
                e.Cancel = true;
            }
        }
    }
}
