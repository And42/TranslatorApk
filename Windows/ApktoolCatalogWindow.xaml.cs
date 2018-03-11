using System;
using System.Windows;
using TranslatorApk.Logic.ViewModels.Windows;

namespace TranslatorApk.Windows
{
    public partial class ApktoolCatalogWindow
    {
        public ApktoolCatalogWindow()
        {
            InitializeComponent();

            ViewModel = new ApktoolCatalogWindowViewModel();
        }

        internal ApktoolCatalogWindowViewModel ViewModel
        {
            get => DataContext as ApktoolCatalogWindowViewModel;
            private set => DataContext = value;
        }

        private async void ApktoolCatalogWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadItems();
        }

        private void ApktoolCatalogWindow_OnClosed(object sender, EventArgs e)
        {
            ViewModel.Dispose();
        }
    }
}
