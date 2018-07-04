namespace TranslatorApk.Logic.Interfaces
{
    public interface ILoadingProcessWindowInvoker
    {
        int ProcessMax { get; set; }
        int ProcessValue { get; set; }
        bool IsIndeterminate { get; set; }
    }
}