using TranslatorApk.Logic.Classes;

namespace TranslatorApk.Logic.Interfaces
{
    public interface IHaveChildren<TChildType>
    {
        ObservableRangeCollection<TChildType> Children { get; }
    }
}
