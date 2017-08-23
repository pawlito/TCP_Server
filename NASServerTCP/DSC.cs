using NASServerCP;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NASServerTCP
{
    class DSC
    {
        private DbWrapper db = new DbWrapper(ConfigurationManager.AppSettings["dbpath"].ToString() + ConfigurationManager.AppSettings["database"].ToString());
        private string sourcePath = ConfigurationManager.AppSettings["sourcePath"];
        private string backupPath = ConfigurationManager.AppSettings["backupPath"];
        private FileSplitter splitter = new FileSplitter();
        public DSC()
        { }

        public bool SearchDSC()
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
                
            }

            return false;
        }

        private IEnumerable<System.IO.FileInfo> DirSnapshot()
        {
            System.IO.DirectoryInfo dir1 = new System.IO.DirectoryInfo(sourcePath);
            IEnumerable<System.IO.FileInfo> list1 = dir1.GetFiles("*.*", System.IO.SearchOption.AllDirectories);
            return list1;
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
                //fManager.DeleteFile(Path.Combine(ConfigurationManager.AppSettings["sourcePath"], packet));
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
