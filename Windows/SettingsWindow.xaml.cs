using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using TranslatorApk.Logic.Interfaces.SettingsPages;
using TranslatorApk.Logic.ViewModels.SettingsPages;
using TranslatorApk.Logic.ViewModels.TreeViewModels;
using UsefulFunctionsLib;

namespace TranslatorApk.Windows
{
    /// <summary>
    /// Логика взаимодействия для SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            ViewModel = SettingsViewModel.GetNewInstanse();

            InitializeComponent();
        }

        private SettingsViewModel ViewModel
        {
            get => DataContext as SettingsViewModel;
            set => DataContext = value;
        }

        private void TreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            ViewModel.CurrentPage = e.NewValue.As<SettingsTreeViewNodeModel>();
        }

        private void Frame_OnNavigated(object sender, NavigationEventArgs e)
        {
            ISettingsPageViewModel currentModel = ViewModel.CurrentPage.PageViewModel;

            currentModel.RefreshData();

            NavigationFrame.Content.As<Page>().DataContext = currentModel;
        }
    }
}
