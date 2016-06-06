using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace File_sync
{
    static class Program
    {
        static Mutex mutex = new Mutex(true, "{8F6F0AC4-B9A1-45fd-A8CF-72F04D6BDE8A}");
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool AllocConsole();

        public static void SaveSettings(settings setts)
        {
            string sz = JsonConvert.SerializeObject(setts, Formatting.Indented);
            File.WriteAllText("settings.json", sz);
        }
        public static settings LoadSettings()
        {
            settings setts = null;
            string path = "settings.json";

            StreamReader reader = new StreamReader(path);
            setts = JsonConvert.DeserializeObject<settings>(reader.ReadToEnd());
            reader.Close();
            return setts;
        }
        public static void SaveGroups(FileGroups grps)
        {
            string sz = JsonConvert.SerializeObject(grps, Formatting.Indented);
            File.WriteAllText("groups.json", sz);
        }
        public static FileGroups LoadGroups()
        {
            FileGroups setts = null;
            string path = "groups.json";

            StreamReader reader = new StreamReader(path);
            setts = JsonConvert.DeserializeObject<FileGroups>(reader.ReadToEnd());
            reader.Close();
            return setts;
        }

        [STAThread]
        static void Main()
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                //AllocConsole();
                if (File.Exists("settings.json"))
                    settings.current = LoadSettings();
                else
                {
                    settings.current = new settings()
                    {
                        ServerIp = "192.168.1.3",
                        ServerPortDelete = "3577",
                        ServerPortSend = "3578",
                        StartWithWindows = "true"
                    };
                    SaveSettings(settings.current);
                }
                if (File.Exists("groups.json"))
                {
                    FileGroups.current = LoadGroups();
                    foreach (KeyValuePair<string, List<string>> pair in FileGroups.current.Groups)
                    {
                        if (pair.Value == null)
                            FileGroups.current.Groups[pair.Key] = new List<string>();
                    }
                }
                else
                {
                    FileGroups.current = new FileGroups();
                    FileGroups.current.Groups = new Dictionary<string, List<string>>();
                    SaveGroups(FileGroups.current);
                }
               Application.EnableVisualStyles();

                Application.Run(new MyApplicationContext());
            }
            else {
                MessageBox.Show("File sync is already running.");
            }
        }
    }
}
