using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace TranslatorApk.Windows
{
    /// <summary>
    /// Логика взаимодействия для PreviewWindow.xaml
    /// </summary>
    public partial class PreviewWindow
    {
        public ImageSource Image { get; }

        public PreviewWindow(ImageSource image)
        {
            Image = image;
            InitializeComponent();
        }

        private void PreviewWindow_OnLoaded(object sender, RoutedEventArgs e)
        {
            DoubleAnimation anim = new DoubleAnimation(0, 1, new Duration(TimeSpan.FromMilliseconds(100)));
            BeginAnimation(OpacityProperty, anim);
        }
    }
}
