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
                if (SetProperty(ref _fullPath, value))
                {
                    Ext = string.Intern(Path.GetExtension(value) ?? string.Empty);
                }
            }
        }
        private string _fullPath;

        /// <summary>
        /// Определяет, является ли файл папкой
        /// </summary>
        public bool IsFolder { get; }

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

        public Options(string fullPath, bool isFolder, bool isImageLoaded = false)
        {
            _isImageLoaded = isImageLoaded;
            _fullPath = fullPath;
            _ext = string.Intern(Path.GetExtension(fullPath) ?? string.Empty);
            IsFolder = isFolder;
        }
    }
}
