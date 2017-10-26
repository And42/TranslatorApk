using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;
using System.Windows.Threading;
using TranslatorApk.Logic.CustomCommandContainers;
using TranslatorApk.Logic.Interfaces;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.Utils;

namespace TranslatorApk.Windows
{
    /// <summary>
    /// Логика взаимодействия для LoadingWindow.xaml
    /// </summary>
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

            CancelCommand = new ActionCommand(_ => _cancellationToken.Cancel());
        }

        public static void ShowWindow(Action beforeStarting, Action<CancellationTokenSource> threadActions, Action finishActions, Visibility cancelVisibility = Visibility.Visible)
        {
            _cancellationToken = new CancellationTokenSource();
            Application.Current.Dispatcher.Invoke(beforeStarting);

            LoadingWindow window = null;

            bool created = false;
            var loadingWindowTh = new Thread(() =>
            {
                window = new LoadingWindow
                {
                    CancelVisibility = cancelVisibility
                };

                created = true;
                window.SetTaskBarState(TaskbarItemProgressState.Indeterminate);
                window.DoFinishActions = true;
                window.ShowDialog();
            })
            {
                CurrentCulture = Thread.CurrentThread.CurrentCulture,
                CurrentUICulture = Thread.CurrentThread.CurrentUICulture
            };

            loadingWindowTh.SetApartmentState(ApartmentState.STA);
            loadingWindowTh.Start();

            while (!created)
            {
                Thread.Sleep(100);
            }
            
            bool finished = false;

            var th = new Thread(() =>
            {
                try
                {
                    threadActions(_cancellationToken);
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.InvokeAction(() => throw ex);
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
                    {
                        Thread.Sleep(1000);
                    }

                    var thr = Dispatcher.FromThread(loadingWindowTh);

                    thr?.InvokeAction(() =>
                    {
                        if (loadingWindowTh.IsAlive && !thr.HasShutdownStarted)
                        {
                            window.SetTaskBarState(TaskbarItemProgressState.None);
                            window._canClose = true;
                            window.Close();
                        }
                    });

                    if (window.DoFinishActions)
                    {
                        Application.Current.Dispatcher.InvokeAction(finishActions);
                    }
                }
                catch (Exception ex)
                {
                    Application.Current.Dispatcher.InvokeAction(() => throw ex);
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
