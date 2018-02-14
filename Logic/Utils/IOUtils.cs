using System.IO;

namespace TranslatorApk.Logic.Utils
{
    // ReSharper disable once InconsistentNaming
    internal static class IOUtils
    {
        public static void DeleteFile(string path)
        {
            UsefulFunctionsLib.UsefulFunctions_IOHelper.DeleteFile(path);
        }

        public static void DeleteFolder(string path)
        {
            UsefulFunctionsLib.UsefulFunctions_IOHelper.DeleteFolder(path);
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
