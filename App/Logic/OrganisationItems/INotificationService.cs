using System.Windows.Forms;

namespace TranslatorApk.Logic.OrganisationItems
{
    public interface INotificationService
    {
        /// <summary>
        /// Показывает сообщение
        /// </summary>
        /// <param name="message">Сообщение</param>
        /// <param name="title">Название</param>
        /// <param name="icon">Иконка</param>
        void ShowMessage(string message, string title, ToolTipIcon icon = ToolTipIcon.Info);
    }
}
