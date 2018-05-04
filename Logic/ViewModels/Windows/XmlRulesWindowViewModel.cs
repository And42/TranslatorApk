using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using System.Xml;
using AndroidTranslator.Classes.Files;
using Microsoft.Win32;
using MVVM_Tools.Code.Commands;
using MVVM_Tools.Code.Providers;
using TranslatorApk.Logic.Classes;
using TranslatorApk.Logic.OrganisationItems;
using TranslatorApk.Resources.Localizations;
using TranslatorApk.Windows;

namespace TranslatorApk.Logic.ViewModels.Windows
{
    public class XmlRulesWindowViewModel : ViewModelBase
    {
        public ObservableRangeCollection<CheckBoxSetting> Items { get; }

        public Property<CheckBoxSetting> SelectedItem { get; private set; }

        public ICommand ChooseFileCommand { get; }
        public ICommand SaveChangesCommand { get; }
        public ICommand AddItemCommand { get; }
        public ICommand RemoveItemCommand { get; }

        public XmlRulesWindowViewModel()
        {
            Items = new ObservableRangeCollection<CheckBoxSetting>(
                DefaultSettingsContainer.Instance.XmlRules.Select(it => new CheckBoxSetting(it, true))
            );

            BindProperty(() => SelectedItem);

            ChooseFileCommand = new ActionCommand(ChooseFileCommand_Execute);
            SaveChangesCommand = new ActionCommand(SaveChangesCommand_Execute);
            AddItemCommand = new ActionCommand(AddItemCommand_Execute);
            RemoveItemCommand = new ActionCommand(RemoveItemCommand_Execute);
        }

        private void SaveChangesCommand_Execute()
        {
            var items = Items.Where(it => it.Value && !string.IsNullOrEmpty(it.Text)).ToArray();

            DefaultSettingsContainer.Instance.XmlRules = items.Select(it => it.Text).ToArray();

            XmlFile.XmlRules = items.Select(it => new Regex(it.Text)).ToList();

            MessBox.ShowDial(StringResources.Finished);
        }

        private void ChooseFileCommand_Execute()
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                Filter = StringResources.XmlFiles + " (*.xml)|*.xml",
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
                    MessBox.ShowDial(StringResources.ErrorLower,
                        $"Can't open file '{dialog.FileName}'{Environment.NewLine}{StringResources.ErrorLower}: {ex}");
                    return;
                }

                var itms = Utils.Utils.GetAllAttributes(xdoc.DocumentElement, Items.Select(it => it.Text));

                for (int i = Items.Count; i < itms.Count; i++)
                    Items.Add(new CheckBoxSetting(itms[i]));
            }
        }

        private void AddItemCommand_Execute()
        {
            Items.Add(new CheckBoxSetting(string.Empty));
        }

        private void RemoveItemCommand_Execute()
        {
            if (SelectedItem.Value == null)
                return;

            Items.Remove(SelectedItem.Value);
        }

        public override void UnsubscribeFromEvents() { }
    }
}
