using System.ComponentModel;
using System.IO;
using TranslatorApk.Annotations;

namespace TranslatorApk.Logic.Classes
{
    public sealed class Options : INotifyPropertyChanged
    {
        /// <summary>
        /// Возвращает расширение файла
        /// </summary>
        public string Ext
        {
            get { return _ext; }
            private set
            {
                _ext = value;
                OnPropertyChanged(nameof(Ext));
            }
        }
        private string _ext;

        /// <summary>
        /// Возвращает или задаёт полный путь к файлу
        /// </summary>
        public string FullPath
        {
            get
            {
                return _fullPath;
            }
            set
            {
                _fullPath = value;
                Ext = Path.GetExtension(value);
                IsFolder = Directory.Exists(value);
                OnPropertyChanged(nameof(FullPath));
            }
        }
        private string _fullPath;

        /// <summary>
        /// Определяет, является ли файл папкой
        /// </summary>
        public bool IsFolder
        {
            get
            {
                return _isFolder;
            }
            private set
            {
                _isFolder = value;
                OnPropertyChanged(nameof(IsFolder));
            }
        }
        private bool _isFolder;

        /// <summary>
        /// Возвращает или задаёт, загружена ли иконка файла
        /// </summary>
        public bool IsImageLoaded
        {
            get
            {
                return _isImageLoaded;
            }
            set
            {
                _isImageLoaded = value;
                OnPropertyChanged(nameof(IsImageLoaded));
            }
        }
        private bool _isImageLoaded;

        public bool HasPreview
        {
            get { return _hasPreview; }
            set
            {
                if (_hasPreview == value) return;
                _hasPreview = value;
                OnPropertyChanged(nameof(HasPreview));
            }
        }
        private bool _hasPreview;

        public Options(string fullPath, bool isImageLoaded = false)
        {
            IsImageLoaded = isImageLoaded;
            FullPath = fullPath;
            Ext = Path.GetExtension(fullPath);
            IsFolder = Directory.Exists(fullPath);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
