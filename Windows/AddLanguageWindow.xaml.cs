using System;
using System.Windows;
using TranslatorApk.Logic.ViewModels.Windows;

namespace TranslatorApk.Windows
{
    public partial class AddLanguageWindow
    {
        public AddLanguageWindow()
        {
            InitializeComponent();

            ViewModel = new AddLanguageWindowViewModel();
        }

        internal AddLanguageWindowViewModel ViewModel
        {
            get => DataContext as AddLanguageWindowViewModel;
            private set => DataContext = value;
        }

        private async void AddLanguageWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadItems();
        }

        private void AddLanguageWindow_OnClosed(object sender, EventArgs e)
        {
            ViewModel.Dispose();
        }
    }
}
