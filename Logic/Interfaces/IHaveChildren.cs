using TranslatorApk.Logic.Classes;

namespace TranslatorApk.Logic.Interfaces
{
    public interface IHaveChildren
    {
        ObservableRangeCollection<TreeViewNodeModel> Children { get; }
    }
}
