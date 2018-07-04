namespace TranslatorApk.Logic.Interfaces.SettingsPages
{
    public interface ISettingsPageViewModel : IViewModelBase
    {
        string PageTitle { get; }

        void RefreshData();
    }
}
