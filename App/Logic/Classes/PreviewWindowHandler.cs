using System.Windows.Media.Imaging;
using TranslatorApk.Windows;

namespace TranslatorApk.Logic.Classes
{
    internal class PreviewWindowHandler
    {
        private PreviewWindow _fileImagePreviewWindow;
        private BitmapSource _image;

        public bool IsShown { get; private set; }

        public void Init(BitmapSource image)
        {
            _image = image;

            Close();
        }

        public void Update(System.Windows.Point screenPosition, BitmapSource image = null)
        {
            if (ReferenceEquals(_image, image))
                image = null;

            if (image != null)
                _image = image;

            if (_fileImagePreviewWindow == null)
            {
                _fileImagePreviewWindow = new PreviewWindow(_image)
                {
                    Left = screenPosition.X + 5,
                    Top = screenPosition.Y + 5
                };

                _fileImagePreviewWindow.Show();

                IsShown = true;
            }
            else
            {
                if (image != null)
                    _fileImagePreviewWindow.Image = _image;

                _fileImagePreviewWindow.Left = screenPosition.X + 5;
                _fileImagePreviewWindow.Top = screenPosition.Y + 5;
            }
        }

        public void Close()
        {
            if (_fileImagePreviewWindow != null)
            {
                if (_fileImagePreviewWindow.IsLoaded)
                    _fileImagePreviewWindow.Close();

                _fileImagePreviewWindow = null;

                IsShown = false;
            }
        }
    }
}
