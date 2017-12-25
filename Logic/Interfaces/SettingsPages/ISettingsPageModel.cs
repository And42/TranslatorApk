using System;

namespace TranslatorApk.Logic.Interfaces.SettingsPages
{
    public interface ISettingsPageViewModel : IDisposable
    {
        string PageTitle { get; }

        void RefreshData();
    }
}
