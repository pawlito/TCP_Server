using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NASServerTCP
{
    [Serializable]
    public class ClsFileInformation

    {
        static List<ClsFileInformation> filesInformations = new List<ClsFileInformation>();
        private string _fileName = string.Empty;
        private string _fileSize = string.Empty;
        private string _fileHash = string.Empty;
        private int _status = 0;
        private long _fileLength = 0;

        public string FileName

        {

            get { return _fileName; }
            set { _fileName = value; }

        }

        public string FileSize

        {

            get { return _fileSize; }
            set { _fileSize = value; }

        }

        public string FileHash

        {

            get { return _fileHash; }
            set { _fileHash = value; }

        }

        public long FileLength

        {

            get { return _fileLength; }
            set { _fileLength = value; }

        }

        [System.Runtime.Serialization.OnSerializing()]
        internal void SaveFileInfo(System.Runtime.Serialization.StreamingContext context)
        {
            using (StreamWriter sw = File.AppendText("test.txt"))
            {
                sw.WriteLine(FileName.ToString() + ";" + FileHash.ToString() + ";");
            }
        }

    }
}
