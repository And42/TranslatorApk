using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using TranslatorApk.Annotations;
using System.Windows;
using System.Windows.Shell;
using System.Windows.Threading;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.Interfaces;
using TranslatorApk.Logic.Utils;

namespace TranslatorApk.Windows
{
    /// <summary>
    /// Логика взаимодействия для LoadingProcessWindow.xaml
    /// </summary>
    public partial class LoadingProcessWindow : INotifyPropertyChanged
    {
        public int ProcessValue
        {
            get => _processValue;
            set
            {
                if (SetProperty(ref _processValue, value))
                    TaskBarProgress = (int)(value * 10.0 / ProcessMax);
            }
        }
        private int _processValue;

        public int ProcessMax
        {
            get => _processMax;
            set => SetProperty(ref _processMax, value);
        }
        private int _processMax = 100;

        public bool IsIndeterminate
        {
            get => _isIndeterminate;
            set => SetProperty(ref _isIndeterminate, value);
        }
        private bool _isIndeterminate;

        public int TaskBarProgress
        {
            get => _taskBarProgress;
            set
            {
                if (SetProperty(ref _taskBarProgress, value))
                    Dispatcher.InvokeAction(() => TaskbarItemInfo.ProgressValue = value / 10.0);
            }
        }
        private int _taskBarProgress;

        public Visibility CancelVisibility
        {
            get => _cancelVisibility;
            set => SetProperty(ref _cancelVisibility, value);
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

        public static void ShowWindow(Action beforeStarting, Action<CancellationTokenSource, ILoadingProcessWindowInvoker> threadActions, Action finishActions, Visibility cancelVisibility = Visibility.Visible, int progressMax = 0)
        {
#if DEBUG
            var localStack = Environment.StackTrace;
#endif

            cancellationToken = new CancellationTokenSource();
            Application.Current.Dispatcher.Invoke(beforeStarting);

            var culture = Application.Current.Dispatcher.Thread.CurrentUICulture;

            LoadingProcessWindow window = null;

            bool created = false;
            var loadingWindowTh = new Thread(() =>
            {
                window = new LoadingProcessWindow
                {
                    CancelVisibility = cancelVisibility,
                    ProcessMax = progressMax
                };

                created = true;
                window.DoFinishActions = true;
                window.ShowDialog();
            })
            {
                CurrentCulture = culture,
                CurrentUICulture = culture
            };
            loadingWindowTh.SetApartmentState(ApartmentState.STA);
            loadingWindowTh.Start();

            while (!created)
            {
                Thread.Sleep(100);
            }

//#if !DEBUG
            var th = new Thread(() =>
            {
                try
                {
                    threadActions(cancellationToken, new LoadingProcessWindowInvoker(window));
                }
                catch (Exception ex)
                {
#if DEBUG
                    Trace.WriteLine(localStack, $"{nameof(LoadingProcessWindow)} thread stack");
#endif
                    Application.Current.Dispatcher.InvokeAction(() => throw ex);
                }
            })
            {
                CurrentCulture = culture,
                CurrentUICulture = culture
            };
            th.SetApartmentState(ApartmentState.STA);
            th.Start();
//#else
            //threadActions(cancellationToken);
//#endif

            Task.Factory.StartNew(() =>
            {
//#if !DEBUG
                th.Join();
//#endif
                Dispatcher thr = Dispatcher.FromThread(loadingWindowTh);

                if (thr != null && loadingWindowTh.IsAlive && !thr.HasShutdownStarted)
                {
                    window._canClose = true;
                    thr.InvokeAction(() => window.Close());
                }

                if (window.DoFinishActions)
                {
                    Application.Current.Dispatcher.Invoke(finishActions);
                }
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

        private bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value))
                return false;

            storage = value;
            OnPropertyChanged(propertyName);

            return true;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
