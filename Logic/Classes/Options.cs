using System.IO;

namespace TranslatorApk.Logic.Classes
{
    public sealed class Options : BindableBase
    {
        /// <summary>
        /// Возвращает расширение файла
        /// </summary>
        public string Ext
        {
            get => _ext;
            private set => SetProperty(ref _ext, value);
        }
        private string _ext;

        /// <summary>
        /// Возвращает или задаёт полный путь к файлу
        /// </summary>
        public string FullPath
        {
            get => _fullPath;
            set
            {
                _fullPath = value;
                Ext = Path.GetExtension(value);
                IsFolder = Directory.Exists(value);
                RaisePropertyChanged();
            }
        }
        private string _fullPath;

        /// <summary>
        /// Определяет, является ли файл папкой
        /// </summary>
        public bool IsFolder
        {
            get => _isFolder;
            private set => SetProperty(ref _isFolder, value);
        }
        private bool _isFolder;

        /// <summary>
        /// Возвращает или задаёт, загружена ли иконка файла
        /// </summary>
        public bool IsImageLoaded
        {
            get => _isImageLoaded;
            set => SetProperty(ref _isImageLoaded, value);
        }
        private bool _isImageLoaded;

        public bool HasPreview
        {
            get => _hasPreview;
            set => SetProperty(ref _hasPreview, value);
        }
        private bool _hasPreview;

        public Options(string fullPath, bool isImageLoaded = false)
        {
            IsImageLoaded = isImageLoaded;
            FullPath = fullPath;
            Ext = Path.GetExtension(fullPath);
            IsFolder = Directory.Exists(fullPath);
        }
    }
}
