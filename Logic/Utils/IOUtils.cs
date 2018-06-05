using System.IO;

namespace TranslatorApk.Logic.Utils
{
    // ReSharper disable once InconsistentNaming
    internal static class IOUtils
    {
        public static bool FileExists(string path)
        {
            return File.Exists(path);
        }

        public static bool FolderExists(string path)
        {
            return Directory.Exists(path);
        }

        public static void DeleteFile(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        public static void DeleteFolder(string path)
        {
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }

        public static void CreateFolder(string path)
        {
            Directory.CreateDirectory(path);
        }

        public static void CopyFilesRecursively(string sourceDirectory, string targetDirectory, bool overwrite = true)
        {
            CopyFilesRecursively(new DirectoryInfo(sourceDirectory), new DirectoryInfo(targetDirectory), overwrite);
        }

        public static void CopyFilesRecursively(DirectoryInfo source, DirectoryInfo target, bool overwrite = true)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                CopyFilesRecursively(dir, target.CreateSubdirectory(dir.Name), overwrite);

            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name), overwrite);
        }
    }
}
