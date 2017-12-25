namespace TranslatorApk.Logic.Classes
{
    public class DownloadableApktool : BindableBase
    {
        private InstallOptionsEnum _installed;

        public string Version { get; set; }
        public string Size { get; set; }
        public string Link { get; set; }

        public InstallOptionsEnum Installed
        {
            get => _installed;
            set => SetProperty(ref _installed, value);
        }
    }
}
