using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading; 

namespace StorageController
{
    class FileSender
    {
        private string _ip;
        private int _port;
        private string _dir;
        public FileSender(string ip, int port, string dir)
        {
            _ip = ip;
            _port = port;
            _dir = dir;
        }

        public void Start()
        {
            TcpListener listener = new TcpListener(IPAddress.Parse(_ip), _port);
            listener.Start();
            Console.WriteLine("File Sender initialized and listening to " + _ip + ":" + _port);
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();

                new Thread((cl) =>
                {
                    using (NetworkStream stream = ((TcpClient)cl).GetStream())
                    {
                        NetworkStream strm = client.GetStream();
                        byte[] data = new byte[client.ReceiveBufferSize];
                        int bytesRead = strm.Read(data, 0, Convert.ToInt32(client.ReceiveBufferSize)); // Get the name's length
                        string request = Encoding.ASCII.GetString(data, 0, bytesRead); //Decode the name

                        if (File.Exists(_dir  + request))
                        {
                            byte[] msg = File.ReadAllBytes(_dir  + request); 
                            strm.Write(msg, 0, msg.Length); 
                        }
                        else
                        {
                            strm.Write(new byte[1] { (byte)1 }, 0, sizeof(byte)); // if file does not exist respond null
                        }
                        client.Close();
 
                    }
                }).Start(client);
            }
        }
    }
}
