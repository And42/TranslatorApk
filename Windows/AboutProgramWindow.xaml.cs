using System;
using System.Windows;
using TranslatorApk.Logic.ViewModels.Windows;

namespace TranslatorApk.Windows
{
    public partial class AboutProgramWindow
    {
        public AboutProgramWindow()
        {
            InitializeComponent();

            ViewModel = new AboutProgramWindowViewModel();
        }

        public AboutProgramWindowViewModel ViewModel
        {
            get => DataContext as AboutProgramWindowViewModel;
            set => DataContext = value;
        }

        private async void AboutProgramWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadItems();
        }

        private void AboutProgramWindow_OnClosed(object sender, EventArgs e)
        {
            ViewModel.UnsubscribeFromEvents();
        }
    }
}
