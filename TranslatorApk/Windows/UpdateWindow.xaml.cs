using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
using TranslatorApk.Logic.Utils;

// ReSharper disable PossibleNullReferenceException

namespace TranslatorApk.Windows
{
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow
    {
        public UpdateWindow(string nowVersion, string changes)
        {
            InitializeComponent();
            var xdoc = new XmlDocument();
            xdoc.LoadXml(changes);
            List<XmlNode> versions = 
                xdoc.GetElementsByTagName("version")
                    .Cast<XmlNode>()
                    .Where(node => Utils.CompareVersions(node.Attributes["version"].InnerText, nowVersion) > 0)
                    .ToList();

            var sb = new StringBuilder();

            foreach (XmlNode version in versions)
            {
                sb.Append(version.Attributes["version"].InnerText + ":");

                foreach (XmlNode item in version.ChildNodes)
                    sb.Append(Environment.NewLine + " - " + item.InnerText);

                sb.Append(Environment.NewLine);
                sb.Append(Environment.NewLine);
            }

            ChangesBox.Text = sb.ToString();
        }

        private void YesClick(object sender, RoutedEventArgs e)
        {
            if (Utils.IsAdmin())
            {
                new DownloadWindow().ShowDialog();
                Close();
            }
            else
            {
                Utils.RunAsAdmin(System.Reflection.Assembly.GetExecutingAssembly().Location, "update", out _);
                Environment.Exit(0);
            }
        }

        private void NoClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
