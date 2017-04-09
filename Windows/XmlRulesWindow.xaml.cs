using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Xml;
using AndroidTranslator;
using Microsoft.Win32;
using TranslatorApk.Annotations;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.OrganisationItems;
using Res = TranslatorApk.Resources.Localizations.Resources;

using static TranslatorApk.Resources.Localizations.Resources;

namespace TranslatorApk.Windows
{
    /// <summary>
    /// Логика взаимодействия для XmlRulesWindow.xaml
    /// </summary>
    public partial class XmlRulesWindow : INotifyPropertyChanged
    {
        public ObservableCollection<CheckBoxSetting> Items { get; }

        public XmlRulesWindow()
        {
            Items = new ObservableCollection<CheckBoxSetting>();

            foreach (string item in SettingsIncapsuler.XmlRules)
                Items.Add(new CheckBoxSetting(item, true));

            InitializeComponent();
        }    

        private void ChooseFileClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = XmlFiles + " (*.xml)|*.xml",
                Multiselect = false
            };
            // ReSharper disable once PossibleInvalidOperationException
            if (dialog.ShowDialog().Value)
            {
                var xdoc = new XmlDocument();
                xdoc.Load(dialog.FileName);
                var itms = new List<string>(Items.Select(item => item.Text));
                itms = Functions.GetAllAttributes(xdoc.DocumentElement, itms);

                for (int i = Items.Count; i < itms.Count; i++)
                    Items.Add(new CheckBoxSetting(itms[i]));
            }
        }

        private void SaveChangesClick(object sender, RoutedEventArgs e)
        {
            var items = Items.Where(it => it.Value).Select(it => it.Text).ToArray();

            SettingsIncapsuler.XmlRules = items;

            XmlFile.XmlRules = items.ToList();

            MessBox.ShowDial(Finished);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
