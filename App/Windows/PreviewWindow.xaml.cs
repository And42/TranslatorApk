using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using TranslatorApk.Logic.Interfaces;
using TranslatorApk.Logic.Utils;

namespace TranslatorApk.Windows
{
    public partial class PreviewWindow : IRaisePropertyChanged
    {
        public BitmapSource Image
        {
            get => _image;
            set => this.SetProperty(ref _image, value);
        }
        private BitmapSource _image;

        public PreviewWindow(BitmapSource image)
        {
            Image = image;
            InitializeComponent();
        }

        private void PreviewWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            var anim = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(100)));
            BeginAnimation(OpacityProperty, anim);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
