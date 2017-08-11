using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NASServerTCP
{
    class FilesMonitor
    {
        Thread monitorThread;
        static IProgress<string> progress;

        public void StartMonitor(IProgress<string> Progress)
        {
            progress = Progress;
            monitorThread = new Thread(new ThreadStart(WatchChanges));
            monitorThread.Start();
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private void WatchChanges()
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = @"C:\Users\paweł\Documents\Visual Studio 2015\Projects\NASServerTCP\NASServerTCP\bin\Debug";
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite
           | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            // Only watch text files.
            watcher.Filter = "*.txt";

            watcher.Changed += new FileSystemEventHandler(OnChanged);
            watcher.Created += new FileSystemEventHandler(OnChanged);
            watcher.Deleted += new FileSystemEventHandler(OnChanged);
            watcher.Renamed += new RenamedEventHandler(OnRenamed);

            // Begin watching.
            watcher.EnableRaisingEvents = true;

        }

        private static void OnChanged(object source, FileSystemEventArgs e)
        {
            // Specify what is done when a file is changed, created, or deleted.
            Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
            Console.WriteLine("File: " + e.FullPath + " " + e.ChangeType);
            //progress.Report("File: " + e.FullPath + " " + e.ChangeType);

        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.
            progress.Report($"File: {e.OldFullPath} renamed to {e.FullPath}");
        }
    }

    
}
