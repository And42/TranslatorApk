using System.Windows;
using System.Windows.Media;

namespace TranslatorApk.Logic.UserControls
{
    public partial class BusyContainer
    {
        public static readonly DependencyProperty InnerTemplateProperty = DependencyProperty.Register(
            "InnerTemplate", typeof(DataTemplate), typeof(BusyContainer), new PropertyMetadata(default(DataTemplate)));

        public DataTemplate InnerTemplate
        {
            get => (DataTemplate) GetValue(InnerTemplateProperty);
            set => SetValue(InnerTemplateProperty, value);
        }

        public static readonly DependencyProperty BrushProperty = DependencyProperty.Register(
            "Brush", typeof(Brush), typeof(BusyContainer), new PropertyMetadata(default));

        public Brush Brush
        {
            get => (Brush) GetValue(BrushProperty);
            set => SetValue(BrushProperty, value);
        }

        public static readonly DependencyProperty IsBusyProperty = DependencyProperty.Register(
            "IsBusy", typeof(bool), typeof(BusyContainer), new PropertyMetadata(default));

        public bool IsBusy
        {
            get => (bool) GetValue(IsBusyProperty);
            set => SetValue(IsBusyProperty, value);
        }

        public BusyContainer()
        {
            InitializeComponent();
        }
    }
}
