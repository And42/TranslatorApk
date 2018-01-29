using System.ComponentModel;
using System.Windows.Shell;
using TranslatorApk.Logic.Utils;
using TranslatorApk.Logic.ViewModels.Windows;

namespace TranslatorApk.Windows
{
    public partial class ChangesDetectorWindow
    {
        public ChangesDetectorWindow()
        {
            InitializeComponent();
            TaskbarItemInfo = new TaskbarItemInfo { ProgressState = TaskbarItemProgressState.Normal };

            ViewModel = new ChangesDetectorWindowViewModel();

            ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
        }

        internal ChangesDetectorWindowViewModel ViewModel
        {
            get => DataContext as ChangesDetectorWindowViewModel;
            set => DataContext = value;
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            switch (args.PropertyName)
            {
                case nameof(ViewModel.ProgressValue):
                    Dispatcher.InvokeAction(() => 
                        TaskbarItemInfo.ProgressValue = ViewModel.ProgressValue.Value / (double)ViewModel.ProgressMax.Value
                    );
                    break;
            }
        }
    }
}
