using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.Interfaces;
using TranslatorApk.Logic.Utils;

namespace TranslatorApk.Windows
{
    public partial class LoadingProcessWindow : IRaisePropertyChanged
    {
        public int ProcessValue
        {
            get => _processValue;
            set
            {
                if (this.SetProperty(ref _processValue, value))
                    TaskBarProgress = (int)(value * 10.0 / ProcessMax);
            }
        }
        private int _processValue;

        public int ProcessMax
        {
            get => _processMax;
            set => this.SetProperty(ref _processMax, value);
        }
        private int _processMax = 100;

        public bool IsIndeterminate
        {
            get => _isIndeterminate;
            set => this.SetProperty(ref _isIndeterminate, value);
        }
        private bool _isIndeterminate = true;

        public int TaskBarProgress
        {
            get => _taskBarProgress;
            set
            {
                if (this.SetProperty(ref _taskBarProgress, value))
                    Dispatcher.InvokeAction(() => TaskbarItemInfo.ProgressValue = value / 10.0);
            }
        }
        private int _taskBarProgress;

        public Visibility CancelVisibility
        {
            get => _cancelVisibility;
            set => this.SetProperty(ref _cancelVisibility, value);
        }
        private Visibility _cancelVisibility = Visibility.Visible;

        // ReSharper disable once InconsistentNaming
        private static CancellationTokenSource cancellationToken;

        private bool _canClose;

        public bool DoFinishActions { get; set; }

        private LoadingProcessWindow()
        {
            InitializeComponent();

            TaskbarItemInfo = new TaskbarItemInfo
            {
                ProgressState = TaskbarItemProgressState.Normal
            };
        }

        public static void ShowWindow(
            Action beforeStarting,
            Action<CancellationToken, ILoadingProcessWindowInvoker> threadActions,
            Action finishActions,
            Visibility cancelVisibility = Visibility.Visible,
            int progressMax = 0,
            Window ownerWindow = null)
        {
#if DEBUG
            var localStack = Environment.StackTrace;
#endif

            cancellationToken = new CancellationTokenSource();
            Application.Current.Dispatcher.Invoke(beforeStarting);

            var culture = Application.Current.Dispatcher.Thread.CurrentUICulture;

            var window = new LoadingProcessWindow
            {
                CancelVisibility = cancelVisibility,
                ProcessMax = progressMax,
                DoFinishActions = true,
                Owner = ownerWindow
            };

            window.Show();

            var th = new Thread(() =>
            {
                try
                {
                    window.IsIndeterminate = false;
                    threadActions(cancellationToken.Token, new LoadingProcessWindowInvoker(window));
                }
                catch (OperationCanceledException)
                { }
                catch (Exception ex)
                {
#if DEBUG
                    Trace.WriteLine(localStack, $"{nameof(LoadingProcessWindow)} thread stack");
#endif
                    Application.Current.Dispatcher.InvokeAction(() => throw new Exception(nameof(LoadingProcessWindow), ex));
                }
            })
            {
                CurrentCulture = culture,
                CurrentUICulture = culture
            };
            th.SetApartmentState(ApartmentState.STA);
            th.Start();

            Task.Factory.StartNew(() =>
            {
                th.Join();

                Application.Current.Dispatcher.InvokeAction(() =>
                {
                    window._canClose = true;
                    window.Close();

                    if (window.DoFinishActions)
                    {
                        finishActions();
                    }
                });
            });
        }

        private void StopClicked(object sender, RoutedEventArgs e)
        {
            cancellationToken?.Cancel();
        }

        private void LoadingProcessWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (!_canClose)
            {
                e.Cancel = true;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public  void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
