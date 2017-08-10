using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;


namespace NASServerTCP 
{
    class Listener : IbytesConvertable, IhashGenerator
    {
        Thread listenThread;
        string bufferincmessage;
        System.Net.Sockets.TcpListener tcplistener;
        IProgress<string> progress;
        private int requestCount;
        private int bytesRead;

        public void serverstart(IProgress<string> progress) 
        {
            this.progress = progress;
            progress.Report("waiting for connections\n");
            listenThread = new Thread(new ThreadStart(ListenForClients));
            listenThread.Start();
            
        }
        private void ListenForClients()
        {
            
            tcplistener = new TcpListener(IPAddress.Loopback, 5555);
            tcplistener.Start();
            while (true)
            {
                //blocks until a client has connected to the server
                TcpClient client = tcplistener.AcceptTcpClient();
                // here was first an message that send hello client
                //
                ///////////////////////////////////////////////////
                //create a thread to handle communication
                //with connected client
                Thread clientThread = new Thread(new ParameterizedThreadStart(HandleClientComm));
                clientThread.Start(client);
            }
        }
        public void StopListener()
        { 
            tcplistener.Stop();
            listenThread.Abort();
        }
        private void HandleClientComm(object obj)
        {
            //NetworkStream clientStream = tcpClient.GetStream();
            progress.Report("client connected\n");
            ClsFileInformation objFileInfo = new ClsFileInformation();
            string str = "";
            // retrieve client from parameter passed to thread
            TcpClient client = (TcpClient)obj;
            // sets two streams
            //StreamWriter sWriter = new StreamWriter(client.GetStream(), Encoding.ASCII);
            //StreamReader sReader = new StreamReader(client.GetStream(), Encoding.ASCII);
            NetworkStream ns = client.GetStream();
            // Get file info
            long fileLength;
            string fileName;
            string fileSize;
            {
                byte[] fileNameBytes;
                byte[] fileNameLengthBytes = new byte[4]; //int32
                byte[] fileLengthBytes = new byte[8]; //int64

                ns.Read(fileLengthBytes, 0, 8); // int64
                ns.Read(fileNameLengthBytes, 0, 4); // int32
                fileNameBytes = new byte[BitConverter.ToInt32(fileNameLengthBytes, 0)];
                ns.Read(fileNameBytes, 0, fileNameBytes.Length);

                fileLength = BitConverter.ToInt64(fileLengthBytes, 0);
                fileName = ASCIIEncoding.ASCII.GetString(fileNameBytes);
            }
            Thread.Sleep(1000);
            FileStream fileStream = File.Open(fileName, FileMode.Create, FileAccess.ReadWrite);
            int read;
            int totalRead = 0;
            byte[] buffer = new byte[32 * 1024]; // 32k chunks
            while ((read = ns.Read(buffer, 0, buffer.Length)) > 0)
            {
                fileStream.Write(buffer, 0, read);
                totalRead += read;
                //progressBar1.Value = (int)((100d * totalRead) / fileLength);
            }

            fileStream.Dispose();
            var fileStream2 = new FileStream(fileName, FileMode.OpenOrCreate,
                FileAccess.Read);
            str = GetChecksumBuffered(fileStream2);
            objFileInfo.FileLength = fileLength;
            objFileInfo.FileName = fileName;
            objFileInfo.FileHash = str;
            objFileInfo.FileSize = (Convert.ToSingle(fileLength) / 1024).ToString();
            fileSize = (Convert.ToSingle(fileLength) / 1024).ToString();
            using (StreamWriter sw = File.AppendText("test.txt"))
            {
                sw.WriteLine(objFileInfo.FileName.ToString() + ";" + objFileInfo.FileHash.ToString());
            }
            ////////////////////////////////////////////////////////////////////////////////////////
            str = "";
            fileStream2.Close();
            //client.Close();
        
    }
        public byte[] GetBytesFromString(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }

        public string GetString(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                using (var streamReader = new StreamReader(stream))
                {
                    return streamReader.ReadToEnd();
                }
            }
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
