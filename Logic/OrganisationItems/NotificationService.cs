using System.Drawing;
using System.Windows.Forms;

namespace TranslatorApk.Logic.OrganisationItems
{
    public class NotificationService: INotificationService
    {
        private static readonly NotifyIcon trayIcon;

        static NotificationService()
        {
            trayIcon = new NotifyIcon
            {
                Icon = Icon.ExtractAssociatedIcon(System.Reflection.Assembly.GetEntryAssembly().ManifestModule.Name)
            };
        }

        public void ShowMessage(string message, string title = "TranslatorApk", ToolTipIcon icon = ToolTipIcon.Info)
        {
            trayIcon.Visible = true;
            trayIcon.ShowBalloonTip(3000, title, message, icon);
            trayIcon.Visible = false;
        }
    }
}
