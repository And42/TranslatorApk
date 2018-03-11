using System;
using System.Windows;
using System.Windows.Input;
using TranslatorApk.Logic.ViewModels.Windows;

namespace TranslatorApk.Windows
{
    public sealed partial class SearchWindow
    {
        public SearchWindow()
        {
            InitializeComponent();

            ViewModel = new SearchWindowViewModel(this);
        }

        internal SearchWindowViewModel ViewModel
        {
            get => DataContext as SearchWindowViewModel;
            private set => DataContext = value;
        }

        private void SearchBox_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                ViewModel.FindFilesCommand.Execute();
            }
        }

        private void FilesView_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                ViewModel.LoadSelectedFileCommand.Execute();
            }
        }

        private void FilesView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ViewModel.LoadSelectedFileCommand.Execute();
        }

        private async void SearchWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadItems();

            SearchBox.Focus();
        }

        private void SearchWindow_OnClosed(object sender, EventArgs e)
        {
            ViewModel.Dispose();
        }
    }
}
