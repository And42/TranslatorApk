using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using TranslatorApk.Logic.Utils;

namespace TranslatorApk.Logic.AttachedProperties
{
    public static class Grid
    {
        public static readonly DependencyProperty RowsProperty = DependencyProperty.RegisterAttached(
            "Rows", typeof(string), typeof(Grid), new PropertyMetadata(default, PropertyChangedCallback));

        public static void SetRows(DependencyObject element, string value)
        {
            element.SetValue(RowsProperty, value);
        }

        public static string GetRows(DependencyObject element)
        {
            return (string) element.GetValue(RowsProperty);
        }

        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.RegisterAttached(
            "Columns", typeof(string), typeof(Grid), new PropertyMetadata(default, PropertyChangedCallback));

        public static void SetColumns(DependencyObject element, string value)
        {
            element.SetValue(ColumnsProperty, value);
        }

        public static string GetColumns(DependencyObject element)
        {
            return (string) element.GetValue(ColumnsProperty);
        }

        private static void PropertyChangedCallback(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs args)
        {
            switch (args.Property.Name)
            {
                case "Rows":
                    ProcessRows(dependencyObject, (string)args.NewValue);
                    break;

                case "Columns":
                    ProcessColumns(dependencyObject, (string)args.NewValue);
                    break;
            }
        }

        private static readonly Regex FillRegex = new Regex(@"^(?<number>\d+(.\d)*)?\*$", RegexOptions.Compiled);

        private static void ProcessRows(DependencyObject element, string value)
        {
            // ReSharper disable once UsePatternMatching
            var grid = element as System.Windows.Controls.Grid;

            if (grid == null)
                throw new ArgumentException("Element must be of Grid type");

            grid.RowDefinitions.Clear();

            var rowsPlain = value.SplitRemove(',').SelectArray(it => it.Trim());

            foreach (var plainRow in rowsPlain)
            {
                if (plainRow.ToLower() == "auto" || plainRow.ToLower() == "a")
                {
                    grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
                }
                else if (double.TryParse(plainRow, out double pixelHeight))
                {
                    grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(pixelHeight) });
                }
                else
                {
                    var fillPartMatch = FillRegex.Match(plainRow);

                    if (!fillPartMatch.Success)
                        throw new ArgumentException($"Can't parse value '{plainRow}' in '{rowsPlain}'");

                    var numberPart = fillPartMatch.Groups["number"].Value;

                    grid.RowDefinitions.Add(
                        new RowDefinition
                        {
                            Height = new GridLength(numberPart.IsNullOrEmpty() ? 1 : double.Parse(numberPart), GridUnitType.Star)
                        }
                    );
                }
            }
        }

        private static void ProcessColumns(DependencyObject element, string value)
        {
            // ReSharper disable once UsePatternMatching
            var grid = element as System.Windows.Controls.Grid;

            if (grid == null)
                throw new ArgumentException("Element must be of Grid type");

            grid.ColumnDefinitions.Clear();

            var plainColumns = value.SplitRemove(',').SelectArray(it => it.Trim());

            foreach (var plainColumn in plainColumns)
            {
                if (plainColumn.ToLower() == "auto" || plainColumn.ToLower() == "a")
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
                }
                else if (double.TryParse(plainColumn, out double pixelWidth))
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(pixelWidth) });
                }
                else
                {
                    var fillPartMatch = FillRegex.Match(plainColumn);

                    if (!fillPartMatch.Success)
                        throw new ArgumentException($"Can't parse value '{plainColumn}' in '{plainColumns}'");

                    var numberPart = fillPartMatch.Groups["number"].Value;

                    grid.ColumnDefinitions.Add(
                        new ColumnDefinition
                        {
                            Width = new GridLength(numberPart.IsNullOrEmpty() ? 1 : double.Parse(numberPart), GridUnitType.Star)
                        }
                    );
                }
            }
        }
    }
}
