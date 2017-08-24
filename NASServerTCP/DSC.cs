using NASServerCP;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NASServerTCP
{
    class DSC
    {
        Thread DSCthread;
        private DbWrapper db = new DbWrapper(ConfigurationManager.AppSettings["dbpath"].ToString() + ConfigurationManager.AppSettings["database"].ToString());
        private string sourcePath = ConfigurationManager.AppSettings["sourcePath"];
        private string backupPath = ConfigurationManager.AppSettings["backupPath"];
        private FileSplitter splitter = new FileSplitter();
        private FileManager fileMobj = new FileManager();
        IProgress<string> progress;
        public DSC()
        { }

        public void StartDSC(IProgress<string> Progress)
        {
            this.progress = Progress;

            DSCthread = new Thread(new ThreadStart(Run));
            DSCthread.Start();
            DSCthread.IsBackground = true;
            progress.Report("DSC monitorung started");
        }

        public void Run()
        {
            progress.Report("searching for DSC errors");
            var startTimeSpan = TimeSpan.Zero;
            var periodTimeSpan = TimeSpan.FromMinutes(5);

            var timer = new Timer((ee) =>
            {
                SearchDSC(progress);
            }, 
            null, startTimeSpan, periodTimeSpan);
        }

        public bool SearchDSC(IProgress<string> progress)
        {
            IEnumerable<System.IO.FileInfo> filesList = DirSnapshot();
            List<System.IO.FileInfo> Corrupted = new List<System.IO.FileInfo>();
            foreach ( var v in filesList)
            {
                Dictionary<string, byte[]> Packets = new Dictionary<string, byte[]>();
                Dictionary<string, string> FileChecksums = new Dictionary<string, string>();
                Dictionary<string, string> ChecksumsFromDB = new Dictionary<string, string>();

                Packets = GetPackets(v.FullName);
                FileChecksums = GetFileChecksums(Packets);
                ChecksumsFromDB = GetChecksumDataFromDB(v.Name);
                Corrupted = GetCorruptedFiles(FileChecksums, ChecksumsFromDB, v);
                if (Corrupted.Count > 0)
                {
                    //ae.WriteToLog("SDC encountered", System.Diagnostics.EventLogEntryType.Error,
                    //  AppEvents.CategoryType.UserInput, AppEvents.EventIDType.SecurityFailure);
                    foreach (var item in Corrupted)
                    {
                        string backupFrom = string.Empty;
                        string backupTo = string.Empty;
                        if (item.DirectoryName == sourcePath)
                        {
                            backupFrom = backupPath;
                            backupTo = sourcePath;
                        }
                        else
                        {
                            backupFrom = sourcePath;
                            backupTo = backupPath;
                        }
                        CorrectDSC(item, backupFrom, backupTo);
                    }
                }
                else
                    progress.Report("No DSC errors detected");
                
            }

            return false;
        }

        public bool CorrectDSC(System.IO.FileInfo CorruptedFile, string backupFrom, string backupTo)
        {
            try
            {
                string destination = Path.Combine(backupTo, CorruptedFile.Name);
                string source = Path.Combine(backupFrom, CorruptedFile.Name);
                File.Copy(source, destination, true);
                RecalculateFileChecksum(backupTo, CorruptedFile.Name);

            }
            catch(Exception e)
            {
                return false;
            }
            return true;
        }

        private IEnumerable<System.IO.FileInfo> DirSnapshot()
        {
            DirectoryInfo dir1 = new System.IO.DirectoryInfo(sourcePath);
            IEnumerable<System.IO.FileInfo> filesList = dir1.GetFiles("*.*", System.IO.SearchOption.AllDirectories);
            return filesList;
        }

        private Boolean RecalculateFileChecksum(string path, string fileName)
        {
            try
            {
                Hashtable row = new Hashtable();
                row = db.SelectRow(fileName);
                int ID = int.Parse(row["ID"].ToString());
                string hash = string.Empty;
                Dictionary<string, byte[]> Packets = new Dictionary<string, byte[]>();
                Packets = GetPackets(Path.Combine(path, fileName));
                foreach (KeyValuePair<string, byte[]> item in Packets)
                {
                    hash = GetChecksumBufferedFromByteArray(item.Value);
                    db.UpInsertChecksum("checksums", ID, hash, item.Key);
                }
                return true;

            }
            catch (Exception e)
            {
                return false;
            }
        }

        private Dictionary<string, byte[]> GetPackets(string fileName)
        {
            return splitter.SplitFile(fileName, 4);
        }

        private Dictionary<string, string> GetFileChecksums(Dictionary<string, byte[]> filePackets)
        {
            Dictionary<string, string> Checksums = new Dictionary<string, string>();
            foreach (KeyValuePair<string, byte[]> item in filePackets)
            {
                string hash = "";
                hash = GetChecksumBufferedFromByteArray(item.Value);
                Checksums.Add(item.Key, hash);
                hash = "";
            }
            return Checksums;
        }

        private Dictionary<string, string> GetChecksumDataFromDB(string fileName)
        {
            Dictionary<string, string> ChecksumsFromDB = new Dictionary<string, string>();
            string[] rowItems = new string[4];
            List<string> rows = new List<string>();

            rows = db.SelectAllRows(fileName);
            foreach (string item in rows)
            {
                rowItems = item.Split(',').ToArray();
                ChecksumsFromDB.Add(rowItems[3], rowItems[4]);
            }
            return ChecksumsFromDB;
        }

        private List<System.IO.FileInfo> GetCorruptedFiles(Dictionary<string, string> dict1, Dictionary<string, string> dict2,
            System.IO.FileInfo fileInfo)
        {
            List<System.IO.FileInfo> corrupted = new List<System.IO.FileInfo>();
            foreach (var d1 in dict1)
            {
                foreach (var d2 in dict2)
                {
                    if (d1.Key == d2.Key)
                    {
                        if (!CompareChecksums(d1.Value, d2.Value))
                        {
                            corrupted.Add(fileInfo);
                        }
                    }
                }
            }
            return corrupted;
        }

        private Boolean CompareChecksums(string str1, string str2)
        {
            Boolean returnCode = true;
            if (string.Compare(str1, str2) != 0)
            {
                returnCode = false;
            }

            return returnCode;
        }   

        public string GetChecksumBufferedFromByteArray(byte[] item)
        {
            var sha = new SHA256Managed();
            byte[] checksum = sha.ComputeHash(item);
            return BitConverter.ToString(checksum).Replace("-", String.Empty);

        }
    }
}
