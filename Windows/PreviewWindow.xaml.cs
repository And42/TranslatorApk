using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using TranslatorApk.Annotations;

namespace TranslatorApk.Windows
{
    /// <summary>
    /// Логика взаимодействия для PreviewWindow.xaml
    /// </summary>
    public partial class PreviewWindow : INotifyPropertyChanged
    {
        public ImageSource Image { get; }

        public PreviewWindow(ImageSource image)
        {
            Image = image;
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void PreviewWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            DoubleAnimation anim = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(100)));
            BeginAnimation(OpacityProperty, anim);
        }
    }
}
