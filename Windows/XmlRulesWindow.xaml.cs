using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Xml;
using AndroidTranslator.Classes.Files;
using Microsoft.Win32;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Logic.Utils;
using Res = TranslatorApk.Resources.Localizations.Resources;

namespace TranslatorApk.Windows
{
    /// <summary>
    /// Логика взаимодействия для XmlRulesWindow.xaml
    /// </summary>
    public partial class XmlRulesWindow
    {
        public ObservableCollection<CheckBoxSetting> Items { get; }

        public XmlRulesWindow()
        {
            Items = new ObservableCollection<CheckBoxSetting>();

            foreach (string item in SettingsIncapsuler.Instance.XmlRules)
                Items.Add(new CheckBoxSetting(item, true));

            InitializeComponent();
        }    

        private void ChooseFileClick(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = Res.XmlFiles + " (*.xml)|*.xml",
                Multiselect = false
            };
            
            if (dialog.ShowDialog() == true)
            {
                var xdoc = new XmlDocument();

                try
                {
                    xdoc.Load(dialog.FileName);
                }
                catch (XmlException ex)
                {
                    // todo: Localize
                    MessBox.ShowDial(Res.ErrorLower,
                        $"Can't open file '{dialog.FileName}'{Environment.NewLine}Error: {ex}");
                    return;
                }

                var itms = new List<string>(Items.Select(item => item.Text));
                itms = Utils.GetAllAttributes(xdoc.DocumentElement, itms);

                for (int i = Items.Count; i < itms.Count; i++)
                    Items.Add(new CheckBoxSetting(itms[i]));
            }
        }

        private void SaveChangesClick(object sender, RoutedEventArgs e)
        {
            string[] items = Items.Where(it => it.Value).Select(it => it.Text).ToArray();

            SettingsIncapsuler.Instance.XmlRules = items;

            XmlFile.XmlRules = items.ToList();

            MessBox.ShowDial(Res.Finished);
        }
    }
}
