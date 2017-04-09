using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using TranslatorApk.Annotations;
using System.Windows;
using System.Windows.Shell;
using System.Windows.Threading;
using TranslatorApk.Logic.OrganisationItems;

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
                _processValue = value;
                TaskBarProgress = (int)(value * 10.0 / ProcessMax);
                OnPropertyChanged(nameof(ProcessValue));
            }
        }
        private int _processValue;

        public int ProcessMax
        {
            get => _processMax;
            set
            {
                _processMax = value;
                OnPropertyChanged(nameof(ProcessMax));
            }
        }
        private int _processMax = 100;

        public bool IsIndeterminate
        {
            get => _isIndeterminate;
            set
            {
                if (_isIndeterminate == value) return;
                _isIndeterminate = value;
                OnPropertyChanged(nameof(IsIndeterminate));
            }
        }
        private bool _isIndeterminate;

        public int TaskBarProgress
        {
            get => _taskBarProgress;
            set
            {
                if (_taskBarProgress == value)
                    return;
                Dispatcher.InvokeAction(() => TaskbarItemInfo.ProgressValue = value / 10.0);
                _taskBarProgress = value;
            }
        }
        private int _taskBarProgress;

        public Visibility CancelVisibility
        {
            get => _cancelVisibility;
            set
            {
                _cancelVisibility = value;
                OnPropertyChanged(nameof(CancelVisibility));
            }
        }
        private Visibility _cancelVisibility = Visibility.Visible;

        public static LoadingProcessWindow Instance;
        // ReSharper disable once InconsistentNaming
        private static CancellationTokenSource cancellationToken;

        private bool _canClose;

        public bool DoFinishActions { get; set; }

        private LoadingProcessWindow()
        {
            InitializeComponent();

            TaskbarItemInfo = new TaskbarItemInfo
            {
                ProgressState = TaskbarItemProgressState.Normal,
            };
        }

        public static void ShowWindow(Action beforeStarting, Action<CancellationTokenSource> threadActions, Action finishActions, Visibility cancelVisibility = Visibility.Visible, int progressMax = 0)
        {
            cancellationToken = new CancellationTokenSource();
            Application.Current.Dispatcher.Invoke(beforeStarting);

            var culture = Application.Current.Dispatcher.Thread.CurrentUICulture;

            bool created = false;
            var loadingWindowTh = new Thread(() =>
            {
                Instance = new LoadingProcessWindow { CancelVisibility = cancelVisibility, ProcessMax = progressMax };
                created = true;
                Instance.DoFinishActions = true;
                Instance.ShowDialog();
            })
            {
                CurrentCulture = culture,
                CurrentUICulture = culture
            };
            loadingWindowTh.SetApartmentState(ApartmentState.STA);
            loadingWindowTh.Start();

            while (!created) Thread.Sleep(100);

//#if !DEBUG
            var th = new Thread(() =>
            {
                try
                {
                    threadActions(cancellationToken);
                }
                catch (Exception ex)
                {
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
                    Instance._canClose = true;
                    thr.InvokeAction(() => Instance.Close());
                }
                if (Instance.DoFinishActions)
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

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
