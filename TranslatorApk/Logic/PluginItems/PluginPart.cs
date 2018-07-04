namespace TranslatorApk.Logic.PluginItems
{
    public class PluginPart<T>
    {
        public PluginHost Host { get; }
        public T Item { get; }

        public PluginPart(PluginHost host, T item)
        {
            Host = host;
            Item = item;
        }
    }
}
