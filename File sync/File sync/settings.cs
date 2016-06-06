using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File_sync
{
    public class settings
    {
        public static settings current = null;
        public  string ServerIp;
        public  string ServerPortDelete; 
        public string ServerPortSend;
        public  string StartWithWindows;

    }
}
