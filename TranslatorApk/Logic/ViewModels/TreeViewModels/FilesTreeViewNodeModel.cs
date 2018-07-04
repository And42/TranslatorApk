using System;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.Interfaces;
using TranslatorApk.Logic.Utils;

namespace TranslatorApk.Logic.ViewModels.TreeViewModels
{
    public class FilesTreeViewNodeModel : TreeViewNodeModelBase<FilesTreeViewNodeModel>
    {
        public FilesTreeViewNodeModel(IHaveChildren<FilesTreeViewNodeModel> parent = null) : base(parent)
        { }

        public BitmapSource Image
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }
        private BitmapSource _image;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        private string _name;

        public Options Options
        {
            get => _options;
            set => SetProperty(ref _options, value);
        }
        private Options _options;

        public override bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (SetProperty(ref _isExpanded, value) && value)
                {
                    Task.Factory.StartNew(() => Children.ForEach(ImageUtils.LoadIconForItem));
                }
            }
        }
        private bool _isExpanded;

        public Action DoubleClicked { get; set; }

        public void RemoveFromParent()
        {
            Parent?.Children.Remove(this);
        }
    }
}
