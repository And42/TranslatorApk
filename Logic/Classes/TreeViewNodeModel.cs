using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using TranslatorApk.Logic.Interfaces;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.Utils;
using UsefulFunctionsLib;

namespace TranslatorApk.Logic.Classes
{
    public class TreeViewNodeModel : IRaisePropertyChanged, IHaveChildren
    {
        public IHaveChildren Parent { get; }

        public ICommand RefreshFilesListCommand
        {
            get => _refreshFilesListCommand;
            set => this.SetProperty(ref _refreshFilesListCommand, value);
        }
        private ICommand _refreshFilesListCommand;

        public ObservableCollection<TreeViewNodeModel> Children { get; } = new ObservableCollection<TreeViewNodeModel>();

        public BitmapSource Image
        {
            get => _image;
            set => this.SetProperty(ref _image, value);
        }
        private BitmapSource _image;

        public string Name
        {
            get => _name;
            set => this.SetProperty(ref _name, value);
        }
        private string _name;

        public Options Options
        {
            get => _options;
            set => this.SetProperty(ref _options, value);
        }
        private Options _options;

        public bool IsExpanded
        {
            get => _isExpanded;
            set
            {
                if (this.SetProperty(ref _isExpanded, value) && value)
                    Children.ForEach(Utils.ImageUtils.LoadIconForItem);
            }
        }
        private bool _isExpanded;

        public Action DoubleClicked { get; set; }

        public TreeViewNodeModel(IHaveChildren parent = null)
        {
            Parent = parent;
        }

        public void RemoveFromParent()
        {
            Parent?.Children.Remove(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
