using System;
using System.Windows;
using TranslatorApk.Logic.ViewModels.Windows;

namespace TranslatorApk.Windows
{
    /// <summary>
    /// Логика взаимодействия для ApktoolCatalogWindow.xaml
    /// </summary>
    public partial class ApktoolCatalogWindow
    {
        public ApktoolCatalogWindow()
        {
            InitializeComponent();

            ViewModel = new ApktoolCatalogWindowViewModel();
        }

        public ApktoolCatalogWindowViewModel ViewModel
        {
            get => DataContext as ApktoolCatalogWindowViewModel;
            set => DataContext = value;
        }

        private async void ApktoolCatalogWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadItems();
        }

        private void ApktoolCatalogWindow_OnClosed(object sender, EventArgs e)
        {
            ViewModel.UnsubscribeFromEvents();
        }
    }
}
