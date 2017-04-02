using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Media;
using TranslatorApk.Annotations;
using TranslatorApk.Logic.Interfaces;

namespace TranslatorApk.Logic.Classes
{
    public class TreeViewNodeModel : INotifyPropertyChanged, IHaveChildren
    {
        public IHaveChildren Parent { get; }

        public ObservableCollection<TreeViewNodeModel> Children { get; } = new ObservableCollection<TreeViewNodeModel>();

        public ImageSource Image
        {
            get
            {
                return _image;
            }
            set
            {
                if (Equals(_image, value)) return;
                _image = value;
                OnPropertyChanged(nameof(Image));
            }
        }
        private ImageSource _image;

        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (_name == value) return;
                _name = value;
                OnPropertyChanged(nameof(Name));
            }
        }
        private string _name;

        public Options Options
        {
            get
            {
                return _options;
            }
            set
            {
                _options = value;
                OnPropertyChanged(nameof(Options));
            }
        }
        private Options _options;

        public bool IsExpanded
        {
            get
            {
                return _isExpanded;
            }
            set
            {
                if (_isExpanded == value) return;
                _isExpanded = value;
                OnPropertyChanged(nameof(IsExpanded));
                if (value)
                    foreach (var item in Children)
                        Functions.LoadIconForItem(item);
            }
        }
        private bool _isExpanded;

        public Action DoubleClicked { get; set; }

        public TreeViewNodeModel(IHaveChildren parent)
        {
            Parent = parent;
        }

        public void RemoveFromParent()
        {
            Parent?.Children.Remove(this);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
