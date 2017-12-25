using System;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TranslatorApk.Logic.Interfaces;
using TranslatorApk.Logic.Utils;
using TranslatorApk.Logic.ViewModels.TreeViewModels;
using UsefulFunctionsLib;

namespace TranslatorApk.Logic.Classes
{
    public class FilesTreeViewNodeModel : TreeViewNodeModelBase<FilesTreeViewNodeModel>
    {
        private ICommand _refreshFilesListCommand;
        private BitmapSource _image;
        private string _name;
        private Options _options;
        private bool _isExpanded;

        public FilesTreeViewNodeModel(ICommand refreshFilesListCommand, IHaveChildren<FilesTreeViewNodeModel> parent = null) : base(parent)
        {
            _refreshFilesListCommand = refreshFilesListCommand;
        }

        public ICommand RefreshFilesListCommand
        {
            get => _refreshFilesListCommand;
            set => SetProperty(ref _refreshFilesListCommand, value);
        }
        
        public BitmapSource Image
        {
            get => _image;
            set => SetProperty(ref _image, value);
        }
        
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        
        public Options Options
        {
            get => _options;
            set => SetProperty(ref _options, value);
        }
        
        public override bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (SetProperty(ref _isExpanded, value) && value)
                    Children.ForEach(ImageUtils.LoadIconForItem);
            }
        }

        public Action DoubleClicked { get; set; }

        public void RemoveFromParent()
        {
            Parent?.Children.Remove(this);
        }
    }
}
