using System;
using System.Drawing;
using System.Windows.Forms;

namespace TranslatorApk.Logic.OrganisationItems
{
    public class NotificationService : IDisposable
    {
        private NotifyIcon _trayIcon;

        private NotificationService()
        {
            _trayIcon = new NotifyIcon
            {
                Icon = Icon.ExtractAssociatedIcon(GlobalVariables.PathToExe)
            };
        }

        public static NotificationService Instance { get; } = new NotificationService();

        public void ShowMessage(string message, string title = "TranslatorApk", ToolTipIcon icon = ToolTipIcon.Info)
        {
            if (CheckDisposed())
                throw new ObjectDisposedException(nameof(NotificationService));

            _trayIcon.Visible = true;
            _trayIcon.ShowBalloonTip(3000, title, message, icon);
        }

        public void Dispose()
        {
            if (CheckDisposed())
                return;

            _trayIcon.Visible = false;
            _trayIcon.Dispose();
            _trayIcon = null;
        }

        private bool CheckDisposed()
        {
            return _trayIcon == null;
        }
    }
}
