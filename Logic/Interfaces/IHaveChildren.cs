using System.Collections.ObjectModel;
using TranslatorApk.Logic.Classes;

namespace TranslatorApk.Logic.Interfaces
{
    public interface IHaveChildren
    {
        ObservableCollection<TreeViewNodeModel> Children { get; }
    }
}
