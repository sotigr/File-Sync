using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace io 
{
    public class StorageReader : IDisposable
    {
        private string _ip;
        private int _buffer_size = 1024;
        public bool Success = false;

        //Event declaration
        public event EventHandler ReadComplete;
        protected virtual void OnReadComplete(object sender, EventArgs e)
        {
            EventHandler handler = ReadComplete;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public StorageReader()
        {
            _ip = File_sync.settings.current.ServerIp;
        }

        public byte[] Read(string filename)
        {
            byte[] file_name = System.Text.Encoding.ASCII.GetBytes(filename);
            int file_name_length = file_name.Length;
            if (file_name_length > _buffer_size)
            {
                throw new System.ArgumentException("The file name excedes the buffer's size.", "original");
            }
            //Opens a socket to send the file to the storage machine
            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(_ip), int.Parse(File_sync.settings.current.ServerPortDelete));



            Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            client.Connect(ipEndPoint);

            client.Send(file_name, file_name_length, SocketFlags.None);

            
            byte[] buffer = new byte[1024];
            int bytesRead = 1024;

            MemoryStream stream = new MemoryStream();
            while ((bytesRead = client.Receive(buffer, 0, buffer.Length, SocketFlags.None)) > 0)
            {
                stream.Write(buffer, 0, bytesRead);
            }

            client.Shutdown(SocketShutdown.Both);
            client.Close();


            file_name = null;
            Success = true;

            byte[] fba = stream.ToArray();

            //Raising the OnWriteComplete event
            OnReadComplete(this, new StorageReadCompleteArgs() { Data = fba });
            return fba;
        }


        public void Read(string filename, bool async)
        {
            if (async)
            {
                new Thread(() => { Read(filename); }).Start();
            }
            else
            {
                Read(filename);
            }
        }

        public void Dispose()
        {
            _ip = null;
        }
    }
    class StorageReadCompleteArgs : EventArgs
    {
        public byte[] Data { set; get; }
    }
}