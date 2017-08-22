using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace NASServerTCP
{
    class FileSplitter : IhashGenerator
    {
        FileManager fileM = new FileManager();
        
        public FileSplitter()
        { }

        public List<string> SplitFile(string SourceFile, int nNoofFiles)
        {
            List<string> Packets = new List<string>();
            
            bool Split = false;
            try
            {
                FileStream fs = new FileStream(SourceFile, FileMode.Open, FileAccess.Read);
                int SizeofEachFile = (int)Math.Ceiling((double)fs.Length / nNoofFiles);

                for (int i = 0; i < nNoofFiles; i++)
                {
                    string hash = "";
                    string baseFileName = Path.GetFileNameWithoutExtension(SourceFile);
                    string Extension = Path.GetExtension(SourceFile);

                    FileStream outputFile = new FileStream(Path.GetDirectoryName(SourceFile) + "\\" + baseFileName + "." +
                        i.ToString().PadLeft(5, Convert.ToChar("0")) + Extension + ".tmp", FileMode.Create, FileAccess.Write);

                    int bytesRead = 0;
                    byte[] buffer = new byte[SizeofEachFile];

                    if ((bytesRead = fs.Read(buffer, 0, SizeofEachFile)) > 0)
                    {
                       
                        outputFile.Write(buffer, 0, bytesRead);
                        //outp.Write(buffer, 0, BytesRead);

                        string packet = baseFileName + "." + i.ToString().PadLeft(3, Convert.ToChar("0")) + Extension.ToString();
                        Packets.Add(packet);
                    }
                    
                    outputFile.Close();
                }
                fs.Close();
            }
            catch (Exception Ex)
            {
                throw new ArgumentException(Ex.Message);
            }

            return Packets;
        }
        public string GetChecksumBuffered(Stream stream)
        {
            using (var bufferedStream = new BufferedStream(stream, 1024 * 32))
            {
                var sha = new SHA256Managed();
                byte[] checksum = sha.ComputeHash(bufferedStream);
                return BitConverter.ToString(checksum).Replace("-", String.Empty);
            }
        }
    }
}
