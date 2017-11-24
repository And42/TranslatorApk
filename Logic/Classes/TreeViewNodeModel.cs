using System;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TranslatorApk.Logic.Interfaces;
using TranslatorApk.Logic.Utils;
using UsefulFunctionsLib;

namespace TranslatorApk.Logic.Classes
{
    public class TreeViewNodeModel : BindableBase, IHaveChildren
    {
        public IHaveChildren Parent { get; }

        public ICommand RefreshFilesListCommand
        {
            get => _refreshFilesListCommand;
            set => SetProperty(ref _refreshFilesListCommand, value);
        }
        private ICommand _refreshFilesListCommand;

        public ObservableRangeCollection<TreeViewNodeModel> Children { get; } = new ObservableRangeCollection<TreeViewNodeModel>();

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

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (SetProperty(ref _isExpanded, value) && value)
                    Children.ForEach(ImageUtils.LoadIconForItem);
            }
        }
        private bool _isExpanded;

        public Action DoubleClicked { get; set; }

        public TreeViewNodeModel(ICommand refreshFilesListCommand, IHaveChildren parent = null)
        {
            _refreshFilesListCommand = refreshFilesListCommand;
            Parent = parent;
        }

        public void RemoveFromParent()
        {
            Parent?.Children.Remove(this);
        }
    }
}
