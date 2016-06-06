using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
namespace io 
{
    public class StorageWriter : IDisposable
    {
        private string _f_name;
        private string _ip;
        private int _buffer_size = 1024;
        public bool Success = false;

        //Event declaration
        public event EventHandler WriteComplete;
        protected virtual void OnWriteComplete(object sender, EventArgs e)
        {
            EventHandler handler = WriteComplete;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public StorageWriter(  string filename)
        {
            _ip = File_sync.settings.current.ServerIp;
            _f_name = filename;
        }
        public void Write(string data)
        {
            Write(Encoding.ASCII.GetBytes(data));
        }
        public void Write(byte[] data)
        {
            try
            {
                if (_f_name.Length > _buffer_size)
                {
                    throw new System.ArgumentException("The file name excedes the buffer's size.", "original");
                }
                //Opens a socket to send the file to the storage machine
                IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(_ip), int.Parse(File_sync.settings.current.ServerPortSend));

                Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                
                client.Connect(ipEndPoint);
                //Defining the size of the memory required
                MemoryStream stream = new MemoryStream(data.Length + (sizeof(byte) * _f_name.Length));
                using (BinaryWriter r = new BinaryWriter(stream))
                {
                    //Inserting the file name to the start of the stream.
                    //Notice that the filename must not be longer than the buffer size on the server socket
                    //in this case 1024 bytes.
                    r.Write(_f_name);
                    r.Write(data);
                }

                client.Send(stream.ToArray());
                stream = null;

                client.Shutdown(SocketShutdown.Both);
                client.Close();
                Success = true;

                //Raising the OnWriteComplete event
                OnWriteComplete(this, new EventArgs());
            }
            catch { }
        }

        public void Write(string data, bool async)
        {
            Write(Encoding.ASCII.GetBytes(data), async);
        }

        public void Write(byte[] data, bool async)
        {
            if (async)
            {
                new Thread(() => { Write(data); }).Start();
            }
            else
            {
                Write(data);
            }
        }

        public void Dispose()
        {
            _f_name = null;
            _ip = null;
        }
    }
}