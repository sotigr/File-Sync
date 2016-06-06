using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Net;
using System.Net.Sockets;

namespace io
{
    public class storage
    {

        public static bool DeleteFile(string filename)
        {
            byte[] file_name = System.Text.Encoding.ASCII.GetBytes(filename);
            int file_name_length = file_name.Length;

            if (file_name_length > 1024)
            {
                throw new System.ArgumentException("The file name excedes the buffer's size.", "original");
            }

            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName());
            IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(File_sync.settings.current.ServerIp), int.Parse(File_sync.settings.current.ServerPortDelete));

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

            byte[] response = stream.ToArray();

            if (response == new byte[1] { (byte)6 })
            {
                return true;
            }
            else
            {
                return false;
            }

        }
    }
}