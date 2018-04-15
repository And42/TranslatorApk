using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TranslatorApk.Annotations;
using TranslatorApk.Logic.OrganisationItems;

using Point = System.Windows.Point;

namespace TranslatorApk.Logic.Utils
{
    internal static class ThemeUtils
    {
        /// <summary>
        /// Меняет тему на одну из доступных
        /// </summary>
        /// <param name="name">Light / Dark</param>
        public static void ChangeTheme(string name)
        {
            var dicts = Application.Current.Resources.MergedDictionaries;

            if (dicts.Any(d =>
            {
                string path = d.Source?.OriginalString;

                if (string.IsNullOrEmpty(path))
                    return false;

                // ReSharper disable once PossibleNullReferenceException
                var split = path.Split('/');

                return split.Length > 1 && split[1] == name;
            }))
            {
                return;
            }

            var windows = GetWindows();

            var sourceScreenshots = GetScreenshots(windows);

            string[] themesToAdd;

            switch (name)
            {
                case "Light":
                    themesToAdd = new[] {"Themes/Light/ThemeResources.xaml"};
                    break;
                default:
                    themesToAdd = new[] {"Themes/Dark/ThemeResources.xaml"};
                    break;
            }

            foreach (var theme in themesToAdd)
            {
                dicts.Insert(1, new ResourceDictionary
                {
                    Source = new Uri(theme, UriKind.Relative)
                });
            }

            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < themesToAdd.Length; i++)
                dicts.RemoveAt(1 + themesToAdd.Length);

            SettingsIncapsuler.Instance.Theme = name;

            foreach (var item in sourceScreenshots)
                StartTransitionForWindow(item.Key, item.Value);
        }

        private static List<Window> GetWindows()
        {
            return Application.Current.Windows.Cast<Window>().Where(it => !string.IsNullOrEmpty(it.Title)).ToList();
        }

        private static Dictionary<Window, Bitmap> GetScreenshots(IEnumerable<Window> windows)
        {
            var result = new Dictionary<Window, Bitmap>();

            foreach (var window in windows)
            {
                var element = (FrameworkElement)window.Content;

                double left = element.PointToScreen(new Point(0, 0)).X;
                double top = element.PointToScreen(new Point(0, 0)).Y;

                result.Add(window, TakeScreenshot((int)left, (int)top, (int)element.ActualWidth, (int)element.ActualHeight));
            }

            return result;
        }

        private static void StartTransitionForWindow(Window window, Bitmap source)
        {
            var adornedElement = (FrameworkElement) window.Content;

            new AdornerWindow(adornedElement, source).Show();
        }

        private static Bitmap TakeScreenshot(int startX, int startY, int width, int height)
        {
            // Bitmap in right size
            Bitmap screenshot = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(screenshot);
            // snip wanted area
            g.CopyFromScreen(startX, startY, 0, 0, new System.Drawing.Size(width, height), CopyPixelOperation.SourceCopy);

            return screenshot;
        }

        private class AdornerWindow : Adorner
        {
            private readonly FrameworkElement _adornedElement;
            private readonly BitmapImage _source;
            private bool _showed;

            private double _width;
            private double _height;

            private double _elementWidth;
            private double _elementHeight;

            public AdornerWindow([NotNull] FrameworkElement adornedElement, Bitmap source) : base(adornedElement)
            {
                _adornedElement = adornedElement;
                _source = Convert(source);
            }

            public void Show()
            {
                if (_showed)
                    return;

                _showed = true;

                _elementWidth = _adornedElement.ActualWidth;
                _elementHeight = _adornedElement.ActualHeight;

                _width = _elementWidth;
                _height = _elementHeight;

                AdornerLayer.GetAdornerLayer(_adornedElement).Add(this);

                const double secondsTime = 0.3;

                Task.Factory.StartNew(() =>
                {
                    const int max = 30;

                    for (int i = max; i >= 1; i--)
                    {
                        _width = _elementWidth / max * i;
                        _height = _elementHeight / max * i;

                        Application.Current.Dispatcher.InvokeAction(InvalidateVisual);

                        Thread.Sleep((int)(secondsTime / max * 1000));
                    }
                }).ContinueWith(task =>
                {
                    Application.Current.Dispatcher.InvokeAction(Cancel);
                });
            }

            private void Cancel()
            {
                AdornerLayer.GetAdornerLayer(AdornedElement).Remove(this);
            }

            private static BitmapImage Convert(Bitmap src)
            {
                MemoryStream ms = new MemoryStream();
                src.Save(ms, System.Drawing.Imaging.ImageFormat.Bmp);
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                ms.Seek(0, SeekOrigin.Begin);
                image.StreamSource = ms;
                image.EndInit();
                image.Freeze();
                return image;
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                const double left = 0;
                const double top = 0;

                double innerLeft = left + _elementWidth / 2 - _width / 2;
                double innerTop = top + _elementHeight / 2 - _height / 2;

                DrawImageRect(drawingContext, _source,
                    new Rect(left, top, _elementWidth, _elementHeight),
                    new Rect(innerLeft, innerTop, _width, _height)
                );
            }

            private static void DrawImageRect(DrawingContext drawingContext, ImageSource image, Rect outerRect,
                Rect innerRect)
            {
                if (!outerRect.Contains(innerRect))
                    return;

                drawingContext.PushClip(new RectangleGeometry(innerRect));
                drawingContext.DrawImage(image, outerRect);
                drawingContext.Pop();
            }

            /*private static void DrawImageRectInverted(DrawingContext drawingContext, ImageSource image, Rect outerRect,
                Rect innerRect)
            {
                if (!outerRect.Contains(innerRect))
                    return;

                Rect[] parts = {
                    new Rect(outerRect.TopLeft, new Size(innerRect.Left - outerRect.Left, outerRect.Height)),
                    new Rect(outerRect.TopLeft, new Size(outerRect.Width, innerRect.Top - outerRect.Top)), 
                    new Rect(innerRect.Right, outerRect.Top, outerRect.Right - innerRect.Right, outerRect.Height), 
                    new Rect(outerRect.Left, innerRect.Bottom, outerRect.Width, outerRect.Bottom - innerRect.Bottom)
                };

                foreach (var part in parts)
                {
                    drawingContext.PushClip(new RectangleGeometry(part));
                    drawingContext.DrawImage(image, outerRect);
                    drawingContext.Pop();
                }
            }*/
        }
    }
}
