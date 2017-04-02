using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
using TranslatorApk.Logic;
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
            XmlDocument xdoc = new XmlDocument();
            xdoc.LoadXml(changes);
            List<XmlNode> versions = xdoc.GetElementsByTagName("version").Cast<XmlNode>().Where(node =>
                Functions.CompareVersions(node.Attributes["version"].InnerText, nowVersion) > 0).ToList();

            var sb = new StringBuilder();
            foreach (var version in versions)
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
            if (Functions.IsAdmin())
            {
                new DownloadWindow().ShowDialog();
                Close();
            }
            else
            {
                Process process;
                Functions.RunAsAdmin(System.Reflection.Assembly.GetExecutingAssembly().Location, "update", out process);
                Environment.Exit(0);
            }
        }

        private void NoClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
