using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;
using MVVM_Tools.Code.Commands;
using TranslatorApk.Logic.Interfaces;
using TranslatorApk.Logic.Utils;

namespace TranslatorApk.Windows
{
    public partial class LoadingWindow : IRaisePropertyChanged
    {
        public Visibility CancelVisibility
        {
            get => _cancelVisibility;
            set => this.SetProperty(ref _cancelVisibility, value);
        }
        private Visibility _cancelVisibility = Visibility.Visible;

        public ICommand CancelCommand { get; }

        public bool DoFinishActions { get; set; }

        private bool _canClose;

        private static CancellationTokenSource _cancellationToken;

        private LoadingWindow()
        {
            InitializeComponent();
            TaskbarItemInfo = new TaskbarItemInfo();

            CancelCommand = new ActionCommand(_cancellationToken.Cancel);
        }

        public static void ShowWindow(
            Action beforeStarting,
            Action<CancellationToken> threadActions,
            Action finishActions,
            Visibility cancelVisibility = Visibility.Visible,
            Window ownerWindow = null)
        {
            _cancellationToken = new CancellationTokenSource();
            Application.Current.Dispatcher.Invoke(beforeStarting);

            var window = new LoadingWindow
            {
                CancelVisibility = cancelVisibility,
                DoFinishActions = true,
                Owner = ownerWindow
            };

            window.SetTaskBarState(TaskbarItemProgressState.Indeterminate);
            window.Show();

            var th = new Thread(() =>
            {
                try
                {
                    threadActions(_cancellationToken.Token);
                }
                catch (OperationCanceledException)
                { }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString(), "Error");
                    Debug.WriteLine(Environment.StackTrace, "Current Stack Trace");

                    Application.Current.Dispatcher.InvokeAction(() => throw new Exception(nameof(LoadingWindow), ex));
                }
            })
            {
                CurrentCulture = Thread.CurrentThread.CurrentCulture,
                CurrentUICulture = Thread.CurrentThread.CurrentUICulture
            };
            th.SetApartmentState(ApartmentState.STA);
            th.Start();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    th.Join();

                    Application.Current.Dispatcher.InvokeAction(() =>
                    {
                        window.SetTaskBarState(TaskbarItemProgressState.None);
                        window._canClose = true;
                        window.Close();

                        if (window.DoFinishActions)
                        {
                            finishActions();
                        }
                    });       
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.InvokeAction(() => throw new Exception(nameof(LoadingWindow), ex));
                }
            }); 
        }

        private void SetTaskBarState(TaskbarItemProgressState state)
        {
            TaskbarItemInfo.ProgressState = state;
        }

        private void LoadingWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (!_canClose)
            {
                e.Cancel = true;
            }
        }

        #region PropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
