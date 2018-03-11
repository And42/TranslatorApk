using System;
using System.Windows;
using System.Windows.Input;
using Syncfusion.UI.Xaml.Grid;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.ViewModels.Windows;

namespace TranslatorApk.Windows
{
    public sealed partial class EditorSearchWindow
    {
        public EditorSearchWindow()
        {
            InitializeComponent();

            FoundItemsGrid.SelectionController = new GridSelectionControllerExt(FoundItemsGrid, FoundedItemsView_OnKeyDown);

            ViewModel = new EditorSearchWindowViewModel(this);
        }

        internal EditorSearchWindowViewModel ViewModel
        {
            get => DataContext as EditorSearchWindowViewModel;
            private set => DataContext = value;
        }

        private bool FoundedItemsView_OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                SelectedToEditor();
                return true;
            }

            return false;
        }

        private void FoundedItemsGrid_OnCellDoubleTapped(object sender, GridCellDoubleTappedEventArgs e)
        {
            SelectedToEditor();
        }

        private void SelectedToEditor()
        {
            ViewModel.ShowItemInEditorCommand.Execute(FoundItemsGrid.SelectedItem as OneFoundItem);
        }

        private void EditorSearchWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            SearchBox.Focus();
        }

        private void EditorSearchWindow_OnClosed(object sender, EventArgs e)
        {
            ViewModel.Dispose();
        }
    }
}
