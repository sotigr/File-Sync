using Newtonsoft.Json;
using StorageController;
using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
namespace FileSyncServer
{
  public class settings {
        public string FileSaveDirectory { set; get; }
    }
    class MyApplicationContext : ApplicationContext
    {

        //Component declarations
        private static NotifyIcon TrayIcon;
        private ContextMenuStrip TrayIconContextMenu;
        private ToolStripMenuItem CloseMenuItem;
        private   void SaveSettings(settings setts)
        {
            string sz = JsonConvert.SerializeObject(setts, Formatting.Indented);
            File.WriteAllText("settings.json", sz);
        }
        private settings LoadSettings()
        {

            settings setts = null;
            string path = "settings.json";

            if (!File.Exists(path))
                SaveSettings(new settings() {  FileSaveDirectory = "C:\\FileSync_backups" });
            StreamReader reader = new StreamReader(path);
            setts = JsonConvert.DeserializeObject<settings>(reader.ReadToEnd());
            reader.Close();
            return setts;

        }
        public MyApplicationContext()
        {
            Application.ApplicationExit += new EventHandler(this.OnApplicationExit);
            InitializeComponent();
            TrayIcon.Visible = true;
        }

        private void InitializeComponent()
        {
            settings importedsettings = LoadSettings();
            if (!System.IO.Directory.Exists(importedsettings.FileSaveDirectory))
                System.IO.Directory.CreateDirectory(importedsettings.FileSaveDirectory);
            new Thread(() => { Console.WriteLine("Loading File Receiver"); new FileReceiver(dsp.core.utility.GetLocalIPAddress(), 3578, importedsettings.FileSaveDirectory).Start(); }).Start();
            new Thread(() => { Console.WriteLine("Loading File Deleter"); new FileDeleter(dsp.core.utility.GetLocalIPAddress(), 3577, importedsettings.FileSaveDirectory).Start(); }).Start();


            TrayIcon = new NotifyIcon();

            TrayIcon.BalloonTipIcon = ToolTipIcon.Info;
            TrayIcon.BalloonTipText =
              "I noticed that you double-clicked me! What can I do for you?";
            TrayIcon.BalloonTipTitle = "You called Master?";
            TrayIcon.Text = "File Sync Server";


            //The icon is added to the project resources.
            //Here I assume that the name of the file is 'TrayIcon.ico'
            TrayIcon.Icon = FileSyncServer.Properties.Resources.Esxxi_me_Hdrv_Blue_Sync_W;

            //Optional - handle doubleclicks on the icon:
            TrayIcon.DoubleClick += TrayIcon_DoubleClick;

            //Optional - Add a context menu to the TrayIcon:
            TrayIconContextMenu = new ContextMenuStrip();
            CloseMenuItem = new ToolStripMenuItem(); 
            TrayIconContextMenu.SuspendLayout();

            // 
            // TrayIconContextMenu
            // 
            this.TrayIconContextMenu.Items.AddRange(new ToolStripItem[] {  this.CloseMenuItem});
            this.TrayIconContextMenu.Name = "TrayIconContextMenu";
            this.TrayIconContextMenu.Size = new Size(153, 70);
            // 
            // CloseMenuItem
            // 
            this.CloseMenuItem.Name = "CloseMenuItem";
            this.CloseMenuItem.Size = new Size(152, 22);
            this.CloseMenuItem.Text = "Close";
            this.CloseMenuItem.Click += new EventHandler(this.CloseMenuItem_Click);

      

            TrayIconContextMenu.ResumeLayout(false);
            TrayIcon.ContextMenuStrip = TrayIconContextMenu; 


        } 
      
       
        private void OnApplicationExit(object sender, EventArgs e)
        {
            //Cleanup so that the icon will be removed when the application is closed
            TrayIcon.Visible = false;
        }
 

        private void CloseMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Close File Sync Server?",
                    "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                Environment.Exit(0);
            }
        }
        private void TrayIcon_DoubleClick(object sender, EventArgs e)
        {
         
        }

    }
}
