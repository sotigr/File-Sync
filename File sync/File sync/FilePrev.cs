using System;
using System.Windows.Forms;

namespace File_sync
{
    public partial class FilePrev : Form
    {
        private string g_name = "";
        public FilePrev(string groupName)
        {
            InitializeComponent();
            this.Text = "Editing: " +  groupName;
            g_name = groupName;
        }
        private void UpdateListBox() {
            listBox1.Items.Clear();
            listBox1.Items.AddRange(FileGroups.current.Groups[g_name].ToArray());

        }
        private void FilePrev_Load(object sender, EventArgs e)
        {
            UpdateListBox();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            inputDialogBrowse  dia = new inputDialogBrowse();
            dia.ShowDialog();
            if (dia.Applied)
            {
                FileGroups.current.Groups[g_name].Add(dia.InputText);
                Program.SaveGroups(FileGroups.current);
                MyApplicationContext.InitializeDirectoryMonitors();
                UpdateListBox();
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show(this, "Are you sure you want to delete \n " + listBox1.SelectedItem, "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                FileGroups.current.Groups[g_name].Remove((string)listBox1.SelectedItem);
                Program.SaveGroups(FileGroups.current);
                MyApplicationContext.InitializeDirectoryMonitors();
                UpdateListBox();
            }
        }
    }
}
