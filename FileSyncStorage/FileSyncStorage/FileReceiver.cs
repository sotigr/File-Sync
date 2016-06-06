using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace StorageController
{
    class FileReceiver
    {
        private string _ip;
        private int _port;
        private string _dir;
        public FileReceiver(string ip, int port, string dir)
        {
            _ip = ip;
            _port = port;
            _dir = dir;
        }

        public void Start()
        {
            TcpListener listener = new TcpListener(IPAddress.Parse(_ip), _port);
            listener.Start();
            Console.WriteLine("File Receiver initialized and listening to " + _ip + ":" + _port);
            while (true)
            {
                TcpClient client = listener.AcceptTcpClient();
                new Thread((cl) =>
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead;
                    MemoryStream final = new MemoryStream();
        
                    using (NetworkStream stream = ((TcpClient)cl).GetStream())
                    { 
                        while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            final.Write(buffer, 0, bytesRead);
                        }
                    }
                    final.Seek(0, SeekOrigin.Begin);
                    BinaryReader r = new BinaryReader(final);

                    string filename = "";
                    string dir = "";
                    string rawname = r.ReadString();
                    if (rawname.Contains("\\"))
                    {
                        string[] arr = rawname.Split('\\');
                        for (int i = 0; i < arr.Length - 1; i++)
                        {
                            dir += "\\" + arr[i];
                        }
                        dir = _dir + dir;
                        filename = arr[arr.Length - 1];
                        try
                        {
                            if (File.Exists(dir + "\\" + filename))
                                File.Delete(dir + "\\" + filename);
                            Directory.CreateDirectory(dir);
                            Console.WriteLine("Create: "+dir + "\\" + filename);
                        }
                        catch { }
                    }
                    else
                    {
                        dir = _dir;
                    }
                    byte[] filebytes = new byte[final.Length- r.BaseStream.Position];
                    int cn = 0;
                    while (r.BaseStream.Position != r.BaseStream.Length)
                    {
                        filebytes[cn] = r.ReadByte();
                        cn += 1;
                    }
                    File.WriteAllBytes(dir + "\\" + filename, filebytes);

                    r.Close(); 
                }).Start(client);
            }
        }
    }
}
/*

     */
