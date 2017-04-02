using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;
using System.Windows.Threading;
using TranslatorApk.Annotations;
using TranslatorApk.Logic;

namespace TranslatorApk.Windows
{
    /// <summary>
    /// Логика взаимодействия для LoadingWindow.xaml
    /// </summary>
    public partial class LoadingWindow : INotifyPropertyChanged
    {
        private LoadingWindow()
        {
            InitializeComponent();
            Instance = this;
            TaskbarItemInfo = new TaskbarItemInfo();
        }

        public static LoadingWindow Instance;

        public Visibility CancelVisibility
        {
            get
            {
                return _cancelVisibility;
            }
            set
            {
                _cancelVisibility = value;
                OnPropertyChanged(nameof(CancelVisibility));
            }
        }
        private Visibility _cancelVisibility = Visibility.Visible;

        public bool DoFinishActions { get; set; }

        private bool _canClose;

        // ReSharper disable once InconsistentNaming
        private static CancellationTokenSource cancellationToken;

        public static void ShowWindow(Action beforeStarting, Action<CancellationTokenSource> threadActions, Action finishActions, Visibility cancelVisibility = Visibility.Visible)
        {
            cancellationToken = new CancellationTokenSource();
            Application.Current.Dispatcher.Invoke(beforeStarting);

            bool created = false;
            var loadingWindowTh = new Thread(() =>
            {
                var window = new LoadingWindow {CancelVisibility = cancelVisibility};
                created = true;
                SetTaskBarState(TaskbarItemProgressState.Indeterminate);
                Instance.DoFinishActions = true;
                window.ShowDialog();
            })
            {
                CurrentCulture = Thread.CurrentThread.CurrentCulture,
                CurrentUICulture = Thread.CurrentThread.CurrentUICulture
            };
            loadingWindowTh.SetApartmentState(ApartmentState.STA);
            loadingWindowTh.Start();

            while (!created) Thread.Sleep(100);
            
            bool finished = false;

            var th = new Thread(() =>
            {
                try
                {
                    threadActions(cancellationToken);
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.InvokeAction(() =>
                    {
                        throw ex;
                    });
                }

                finished = true;
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
                    while (!finished)
                        Thread.Sleep(1000);

                    var thr = Dispatcher.FromThread(loadingWindowTh);
                    if (thr != null && loadingWindowTh.IsAlive && !thr.HasShutdownStarted)
                    {
                        Instance._canClose = true;
                        thr.Invoke(new Action(() => Instance.Close()));
                    }
                    if (Instance.DoFinishActions)
                        Application.Current.Dispatcher.InvokeAction(finishActions);

                    SetTaskBarState(TaskbarItemProgressState.None);
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.InvokeAction(() => { throw ex; });
                }
            }); 
        }

        private void StopClicked(object sender, RoutedEventArgs e)
        {
            cancellationToken.Cancel();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public static void SetTaskBarState(TaskbarItemProgressState state)
        {
            Instance?.Dispatcher.InvokeAction(() => Instance.TaskbarItemInfo.ProgressState = state);
        }

        private void LoadingWindow_OnClosing(object sender, CancelEventArgs e)
        {
            if (!_canClose)
            {
                e.Cancel = true;
            }
        }
    }
}
