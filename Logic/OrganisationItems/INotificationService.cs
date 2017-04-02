using System.Windows.Forms;

namespace TranslatorApk.Logic.OrganisationItems
{
    public interface INotificationService
    {
        void ShowMessage(string message, string title, ToolTipIcon icon = ToolTipIcon.Info);
    }
}
