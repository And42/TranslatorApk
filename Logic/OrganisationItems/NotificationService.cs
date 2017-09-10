using System.Drawing;
using System.Windows.Forms;

namespace TranslatorApk.Logic.OrganisationItems
{
    public class NotificationService : INotificationService
    {
        private static readonly NotifyIcon TrayIcon;

        static NotificationService()
        {
            TrayIcon = new NotifyIcon
            {
                Icon = Icon.ExtractAssociatedIcon(GlobalVariables.PathToExe)
            };
        }

        private NotificationService() { }

        public static NotificationService Instance { get; } = new NotificationService();

        public void ShowMessage(string message, string title = "TranslatorApk", ToolTipIcon icon = ToolTipIcon.Info)
        {
            TrayIcon.Visible = true;
            TrayIcon.ShowBalloonTip(3000, title, message, icon);
            TrayIcon.Visible = false;
        }
    }
}
