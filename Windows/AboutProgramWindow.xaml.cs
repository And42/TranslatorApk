using System;
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

        internal AboutProgramWindowViewModel ViewModel
        {
            get => DataContext as AboutProgramWindowViewModel;
            private set => DataContext = value;
        }

        private void AboutProgramWindow_OnClosed(object sender, EventArgs e)
        {
            ViewModel.UnsubscribeFromEvents();
        }
    }
}
