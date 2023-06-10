using System;
using System.IO;
using System.Net;
using System.Threading;
using Ionic.Zip;

namespace AdminScripter
{
    class Program
    {
        static ManualResetEvent _processEvent;

        private static int _progress;

        private static readonly object _lockObj = new object();

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("info:\n\tdelete folder - удаляет папку с содержимым\n\tdelete file - удаляет файл\n\tdelete files - удаляет файлы\n\tdelete folder files - удаляет файлы и папки, содержащиеся в указанной директории\n\tdownload - загружает файл в указанное место\n\tunzip - распаковывает архив в указанную директорию");
                Console.Read();
                return;
            }

            foreach (var argument in args)
            {
                string[] split = argument.Split('|');

                string command = split[0].Trim();

                _processEvent = new ManualResetEvent(true);

                switch (command)
                {
                    case "delete folder":
                        Directory.Delete(split[1].Trim(), true);
                        break;
                    case "delete file":
                        File.Delete(split[1].Trim());
                        break;
                    case "delete files":
                        for (int i = 1; i < split.Length; i++)
                            File.Delete(split[i].Trim());
                        break;
                    case "delete folder files":
                        foreach (var file in Directory.EnumerateFiles(split[1].Trim()))
                            File.Delete(file);
                        foreach (var dir in Directory.EnumerateDirectories(split[1].Trim()))
                            Directory.Delete(dir, true);
                        break;
                    case "download":
                        string directory = Path.GetDirectoryName(split[2].Trim());

                        if (!Directory.Exists(directory))
                            Directory.CreateDirectory(directory);

                        _processEvent = new ManualResetEvent(false);
                        _progress = 0;

                        WebClient client = new WebClient();
                        
                        client.DownloadProgressChanged += (sender, eventArgs) =>
                        {
                            if (_progress >= eventArgs.ProgressPercentage)
                                return;

                            lock (_lockObj)
                            {
                                _progress = eventArgs.ProgressPercentage;
                                Console.CursorLeft = 0;

                                string text = $"Downloading [{_progress} : 100]%";
                                Console.Title = text;
                                Console.Write(text);
                            }
                        };
                        client.DownloadFileCompleted += (sender, eventArgs) =>
                        {
                            _processEvent.Set();
                            Console.WriteLine();
                        };
                        client.DownloadFileAsync(new Uri(split[1].Trim()), split[2].Trim());
                        break;
                    case "unzip":
                        if (!Directory.Exists(split[2].Trim()))
                            Directory.CreateDirectory(split[2].Trim());

                        _processEvent = new ManualResetEvent(false);

                        var zip = new ZipFile(split[1].Trim());

                        zip.ExtractProgress += (sender, eventArgs) =>
                        {
                            if (eventArgs.EventType == ZipProgressEventType.Extracting_AfterExtractAll)
                            {
                                _processEvent.Set();
                                return;
                            }

                            if (eventArgs.EventType != ZipProgressEventType.Extracting_AfterExtractEntry)
                                return;

                            lock (_lockObj)
                            {
                                Console.CursorLeft = 0;

                                string text = $"Unzipping [{eventArgs.EntriesExtracted} : {eventArgs.EntriesTotal}]";
                                Console.Title = text;
                                Console.Write(text);
                            }
                        };

                        zip.ExtractAll(split[2].Trim(), ExtractExistingFileAction.OverwriteSilently);

                        zip.Dispose();
                        break;
                }

                _processEvent.WaitOne();
            }
        }
    }
}
