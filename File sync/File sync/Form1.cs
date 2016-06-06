using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace File_sync
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        delegate void SetTextCallback(string text);
        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Text = settings.current.ServerIp;
            textBox2.Text = settings.current.ServerPortDelete;
            textBox3.Text = settings.current.ServerPortSend;
            checkBox1.Checked = Convert.ToBoolean(settings.current.StartWithWindows);
            FillListBox();


            MyApplicationContext.SyncProgress += SyncProgressEvent;
            MyApplicationContext.SyncStatus += SyncStatusEvent;
            this.Text = "File Sync";

        }
        private void SyncStatusEvent(object sender, SyncStatusArgs args)
        {
            if (args.FilesToSync == 0)
                SetText("File Sync");
            else
                SetText("File Sync - Need to sync - " + args.FilesToSync + " files");
        }
        private void SetText(string text)
        {
            // InvokeRequired required compares the thread ID of the
            // calling thread to the thread ID of the creating thread.
            // If these threads are different, it returns true.
            if (this.InvokeRequired)
            {
                SetTextCallback d = new SetTextCallback(SetText);
                this.Invoke(d, new object[] { text });
            }
            else
            {
                this.Text = text;
            }
        }
        private void SyncProgressEvent(object sender, SyncProgressArgs args)
        {
            label4.Text = args.CurrentFileName;
            progressBar1.Value = args.OverallProgress;
        }
        private void FillListBox()
        {
            listBox1.Items.Clear();
            foreach (KeyValuePair<string, List<string>> pair in FileGroups.current.Groups)
            {
                listBox1.Items.Add(pair.Key);
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {
            settings.current.ServerIp = textBox1.Text;
            settings.current.ServerPortDelete = textBox2.Text;
            settings.current.ServerPortSend = textBox3.Text;
            Program.SaveSettings(settings.current);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
                Registry.SetValue(@"HKEY_CURRENT_USER\SOFTWARE\Microsoft\Windows\CurrentVersion\Run", "FileSync", System.Reflection.Assembly.GetExecutingAssembly().Location, Microsoft.Win32.RegistryValueKind.String);
            else
            {
                
                using (RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Run", true))
                {
                    if (key == null)
                    {
                        // Key doesn't exist. Do whatever you want to handle
                        // this case
                    }
                    else
                    {
                        key.DeleteValue("FileSync");
                    }
                }
            }
            settings.current.StartWithWindows = checkBox1.Checked.ToString();
            Program.SaveSettings(settings.current);
        }

        private void addGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            inputDialog dia = new inputDialog();
            dia.DialogTitle = "Enter the name of the group";
            dia.ShowDialog();
            if (dia.Applied)
            {
                if (FileGroups.current.Groups.ContainsKey(dia.InputText))
                {
                    MessageBox.Show("This group already exists delete the old one and try again.");
                }
                else
                {
                    FileGroups.current.Groups.Add(dia.InputText, new List<string>());
                    Program.SaveGroups(FileGroups.current);
                    FillListBox();
                }

            }
        }

        private void deleteGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Are you sure you want to delete \n " + listBox1.SelectedItem, "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                FileGroups.current.Groups.Remove((string)listBox1.SelectedItem);
                Program.SaveGroups(FileGroups.current);
                MyApplicationContext.InitializeDirectoryMonitors();
                FillListBox();
            }
        }

        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            FilePrev p = new FilePrev((string)listBox1.SelectedItem);
            p.ShowDialog();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MyApplicationContext.SyncListedFiles();
        }

        
    }
}
