using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NASServerTCP
{
    public partial class Form1 : Form
    {
        public static Form1 _Form1;
        static List<Listener> listeners = new List<Listener>();
        public static TcpListener _server;
        TcpClient client;
        //private Boolean _isRunning;
        private int PORT = 5555;
        
        public Form1()
        {
            InitializeComponent();
            listView1.View = View.Details;
            listView1.HeaderStyle = ColumnHeaderStyle.None;
            listView1.Columns.Add(new ColumnHeader { Width = listView1.ClientSize.Width - SystemInformation.VerticalScrollBarWidth });
            _Form1 = this;

        }

        private void StartServer(object sender, EventArgs e)
        {
            Progress<string> progress = new Progress<string>(text => listView1.Items.Add(text));
            Listener newlistener;
            newlistener = new Listener();
            newlistener.serverstart(progress);
            listeners.Add(newlistener);
            listView1.Items.Add("server started at port 5555");
            FilesMonitor fm = new FilesMonitor();
            fm.StartMonitor(progress);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }

        private void StopServer(object sender, EventArgs e)
        {
            foreach (Listener item in listeners)
            {
                item.StopListener();
            }
            listView1.Items.Add("Server stopped");
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
