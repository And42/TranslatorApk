using System;
using System.Windows;
using TranslatorApk.Logic.ViewModels.Windows;

namespace TranslatorApk.Windows
{
    public partial class RemoveLanguagesWindow
    {
        public RemoveLanguagesWindow()
        {
            InitializeComponent();

            ViewModel = new RemoveLanguagesWindowViewModel();
        }

        internal RemoveLanguagesWindowViewModel ViewModel
        {
            get => DataContext as RemoveLanguagesWindowViewModel;
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
