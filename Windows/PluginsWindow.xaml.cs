using System.Windows;
using TranslatorApk.Logic.ViewModels.Windows;

namespace TranslatorApk.Windows
{
    /// <summary>
    /// Логика взаимодействия для Plugins.xaml
    /// </summary>
    public partial class PluginsWindow
    {
        public PluginsWindow()
        {
            InitializeComponent();

            ViewModel = PluginsWindowViewModel.Instanse;
        }

        public PluginsWindowViewModel ViewModel
        {
            get => DataContext as PluginsWindowViewModel;
            private set => DataContext = value;
        }

        private async void Plugins_OnLoaded(object sender, RoutedEventArgs e)
        {
            await ViewModel.LoadItems();
        }
    }
}
