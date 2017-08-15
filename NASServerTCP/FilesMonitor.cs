using System;
using System.Collections.Generic;
using System.Configuration;
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
        public static FileManager fm = new FileManager();
        //public static CompareDirs cd = new CompareDirs();
        public void StartMonitor(IProgress<string> Progress)
        {
            progress = Progress;
            monitorThread = new Thread(new ThreadStart(WatchChanges));
            monitorThread.Start();
            monitorThread.IsBackground = true;
        }

        [PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
        private void WatchChanges()
        {
            FileSystemWatcher watcher = new FileSystemWatcher();
            watcher.Path = ConfigurationManager.AppSettings["sourcePath"];
            watcher.NotifyFilter =  NotifyFilters.LastWrite| NotifyFilters.FileName | NotifyFilters.DirectoryName;
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
            progress.Report("File: " + e.FullPath + " " + e.ChangeType);
            if (fm.CopyFile(e.Name.ToString()))
            {
                progress.Report("File: " + e.FullPath + "copied succesfully");
            }
            //cd.CompareDirectories();

        }

        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            // Specify what is done when a file is renamed.
            progress.Report($"File: {e.OldFullPath} renamed to {e.FullPath}");
           if (fm.RenameFile(e.OldName, e.Name))
            {
                progress.Report("File: " + e.FullPath + "renamed succesfully");
            }
        }
    }

    
}
