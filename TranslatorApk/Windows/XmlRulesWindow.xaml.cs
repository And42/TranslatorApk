using TranslatorApk.Logic.ViewModels.Windows;

namespace TranslatorApk.Windows
{
    public partial class XmlRulesWindow
    {
        public XmlRulesWindow()
        {
            InitializeComponent();

            DataContext = new XmlRulesWindowViewModel();
        }    
    }
}
