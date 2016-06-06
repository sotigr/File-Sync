using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;

namespace File_sync
{
    public class SyncProgressArgs
    {
        public string CurrentFileName { set; get; }
        public int OverallProgress { set; get; }
    }
    public class SyncStatusArgs
    { 
        public int FilesToSync { set; get; }
    }
    class MyApplicationContext : ApplicationContext
    {
        //Component declarations
        private static NotifyIcon TrayIcon;
        private ContextMenuStrip TrayIconContextMenu;
        private ToolStripMenuItem CloseMenuItem;
        private ToolStripMenuItem SyncMenuItem;
        private Form1 form1 = new Form1();
        private static Dictionary<string, List<FileSystemWatcher>> watchers;
        public static Dictionary<string, string> changedDirectories = new Dictionary<string, string>();
        private static bool Syncing = false;
        private static int filesync_interval = 50;
        public static event EventHandler<SyncProgressArgs> SyncProgress;
        public static event EventHandler<SyncStatusArgs> SyncStatus;
        public static bool NeedToSync = false;
        public MyApplicationContext()
        {
            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
            InitializeComponent();
            TrayIcon.Visible = true;
        }

        private void InitializeComponent()
        {
            watchers = new Dictionary<string, List<FileSystemWatcher>>();
            
            TrayIcon = new NotifyIcon();

            TrayIcon.BalloonTipIcon = ToolTipIcon.Info;
            TrayIcon.BalloonTipText =
              "I noticed that you double-clicked me! What can I do for you?";
            TrayIcon.BalloonTipTitle = "You called Master?";
            TrayIcon.Text = "File Sync - 0 files";


            //The icon is added to the project resources.
            //Here I assume that the name of the file is 'TrayIcon.ico'
            TrayIcon.Icon = Properties.Resources.tray_icon;

            //Optional - handle doubleclicks on the icon:
            TrayIcon.DoubleClick += TrayIcon_DoubleClick;

            //Optional - Add a context menu to the TrayIcon:
            TrayIconContextMenu = new ContextMenuStrip();
            CloseMenuItem = new ToolStripMenuItem();
            SyncMenuItem = new ToolStripMenuItem();
            TrayIconContextMenu.SuspendLayout();

            // 
            // TrayIconContextMenu
            // 
            this.TrayIconContextMenu.Items.AddRange(new ToolStripItem[] { this.SyncMenuItem,
            this.CloseMenuItem});
            this.TrayIconContextMenu.Name = "TrayIconContextMenu";
            this.TrayIconContextMenu.Size = new Size(153, 70);
            // 
            // CloseMenuItem
            // 
            this.CloseMenuItem.Name = "CloseMenuItem";
            this.CloseMenuItem.Size = new Size(152, 22);
            this.CloseMenuItem.Text = "Close";
            this.CloseMenuItem.Click += new EventHandler(this.CloseMenuItem_Click);

            this.SyncMenuItem.Name = "SyncMenuItem";
            this.SyncMenuItem.Size = new Size(152, 22);
            this.SyncMenuItem.Text = "Sync";
            this.SyncMenuItem.Click += new EventHandler(this.SyncMenuItem_Click);

            TrayIconContextMenu.ResumeLayout(false);
            TrayIcon.ContextMenuStrip = TrayIconContextMenu;
            InitializeDirectoryMonitors();
        }
        public static void InitializeDirectoryMonitors()
        {
            foreach (KeyValuePair<string, List<FileSystemWatcher>> pair in watchers)
            {
                for (int i = 0; i < pair.Value.Count; i++)
                {
                    watchers[pair.Key][i].EnableRaisingEvents = false;
                }
                watchers[pair.Key].Clear();
            }
            watchers.Clear();
            foreach (KeyValuePair<string, List<string>> pair in FileGroups.current.Groups)
            {
                watchers[pair.Key] = new List<FileSystemWatcher>();
                int cn = 0;
                foreach (string path in FileGroups.current.Groups[pair.Key])
                {

                    foreach (string dir in GetSubdirectoriesContainingOnlyFiles(path))
                    {
                        watchers[pair.Key].Add(new FileSystemWatcher());

                        watchers[pair.Key][cn].Path = dir;


                        /* Watch for changes in LastAccess and LastWrite times, and
                           the renaming of files or directories. */
                        watchers[pair.Key][cn].NotifyFilter = NotifyFilters.LastWrite
                           | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                        // Only watch text files.
                        watchers[pair.Key][cn].Filter = "*";

                        // Add event handlers.
                        watchers[pair.Key][cn].Changed += new FileSystemEventHandler(OnFileChanged);
                        watchers[pair.Key][cn].Created += new FileSystemEventHandler(OnFileChanged);
                        watchers[pair.Key][cn].Renamed += new RenamedEventHandler(OnRenamed);
                        watchers[pair.Key][cn].Deleted += new FileSystemEventHandler(OnFileDeleted);
                        // Begin watching.
                        watchers[pair.Key][cn].EnableRaisingEvents = true;
                        cn += 1;

                    }
                    watchers[pair.Key].Add(new FileSystemWatcher());

                    watchers[pair.Key][cn].Path = path;


                    /* Watch for changes in LastAccess and LastWrite times, and
                       the renaming of files or directories. */
                    watchers[pair.Key][cn].NotifyFilter = NotifyFilters.LastWrite
                       | NotifyFilters.FileName | NotifyFilters.DirectoryName;
                    // Only watch text files.
                    watchers[pair.Key][cn].Filter = "*";

                    // Add event handlers.
                    watchers[pair.Key][cn].Changed += new FileSystemEventHandler(OnFileChanged);
                    watchers[pair.Key][cn].Created += new FileSystemEventHandler(OnFileChanged);
                    watchers[pair.Key][cn].Renamed += new RenamedEventHandler(OnRenamed);
                    watchers[pair.Key][cn].Deleted += new FileSystemEventHandler(OnFileDeleted);
                    // Begin watching.
                    watchers[pair.Key][cn].EnableRaisingEvents = true;

                }
            }
        }
        static IEnumerable<string> GetSubdirectoriesContainingOnlyFiles(string path)
        {
            return Directory.GetDirectories(path, "*", SearchOption.AllDirectories);
        }
        private List<String> DirSearch(string sDir)
        {
            List<String> files = new List<String>();
            try
            {
                foreach (string f in Directory.GetFiles(sDir))
                {
                    files.Add(f);
                }
                foreach (string d in Directory.GetDirectories(sDir))
                {
                    files.AddRange(DirSearch(d));
                }
            }
            catch (System.Exception excpt)
            {
                MessageBox.Show(excpt.Message);
            }

            return files;
        }
        static void sendFile(string filepath)
        { 
            new System.Threading.Thread(() =>
            {
                try
                {
                    if (File.Exists(filepath))
                    {
                        using (io.StorageWriter r = new io.StorageWriter(filepath.Remove(0, 3).Replace("\\\\", "\\")))
                        {
                            r.Write(File.ReadAllBytes(filepath), false);
                        }
                    }
                }
                catch (Exception ex) { }
            }).Start();
        }
        static void deleteFile(string filepath)
        { 
            new System.Threading.Thread(() =>
            {
                try
                {
                    io.storage.DeleteFile(filepath.Remove(0, 3).Replace("\\\\", "\\"));

                }
                catch (Exception ex) { }
            }).Start();
        }
        private static void OnFileDeleted(object source, FileSystemEventArgs e)
        {
            if (!Syncing)
            {
                NeedToSync = true;
                changedDirectories[e.FullPath] = "Delete";
                if (SyncStatus != null)
                    SyncStatus(new object(), new SyncStatusArgs() { FilesToSync = changedDirectories.Count() });
                TrayIcon.Text = "File Sync - "+ changedDirectories.Count() + " files";
            }
        }
        // Define the event handlers.
        private static void OnFileChanged(object source, FileSystemEventArgs e)
        {
            if (!Syncing)
            {
                NeedToSync = true;
                changedDirectories[e.FullPath] = "Sync";
                if (SyncStatus != null)
                    SyncStatus(new object(), new SyncStatusArgs() { FilesToSync = changedDirectories.Count() });
                TrayIcon.Text = "File Sync - " + changedDirectories.Count() + " files";
            }
        }
        private static void OnRenamed(object source, RenamedEventArgs e)
        {
            if (!Syncing)
            {
                NeedToSync = true;
                changedDirectories[e.FullPath] = "Sync";
                if (SyncStatus != null)
                    SyncStatus(new object(), new SyncStatusArgs() { FilesToSync = changedDirectories.Count() });
                TrayIcon.Text = "File Sync - " + changedDirectories.Count() + " files";
            }
        }

        private void OnApplicationExit(object sender, EventArgs e)
        {
            //Cleanup so that the icon will be removed when the application is closed
            TrayIcon.Visible = false;
        }

        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
            //Here you can do stuff if the tray icon is doubleclicked
            form1.ShowDialog();
        }

        private void CloseMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Close File Sync?",
                    "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                Application.Exit();
            }
        }
        private void SyncMenuItem_Click(object sender, EventArgs e)
        {
            SyncListedFiles();
        }
        public static void SyncListedFiles()
        {
            if (NeedToSync)
            {
                Syncing = true;
                int cn = 1;
                int max = changedDirectories.Count();
                foreach (KeyValuePair<string, string> pair in changedDirectories)
                {
                    if (pair.Value == "Sync")
                        sendFile(pair.Key);
                    else
                        deleteFile(pair.Value);
                    if (SyncProgress != null)
                        SyncProgress(new object(), new SyncProgressArgs() { CurrentFileName = pair.Key, OverallProgress = (int)(100 * cn / max) });
                    cn += 1;
                    System.Threading.Thread.Sleep(filesync_interval);
                }
                changedDirectories.Clear();
                if (SyncProgress != null)
                    SyncProgress(new object(), new SyncProgressArgs() { CurrentFileName = "", OverallProgress = 0 });
                if (SyncStatus != null)
                    SyncStatus(new object(), new SyncStatusArgs() { FilesToSync = changedDirectories.Count() });
                TrayIcon.Text = "File Sync - " + changedDirectories.Count() + " files";
                NeedToSync = false;
                Syncing = false;
            }
        }

    }
}
