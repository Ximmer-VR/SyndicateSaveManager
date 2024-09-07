using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SyndicateSaveManager
{
    public partial class SettingsDialog : Form
    {
        public string LogFilePath { get; private set; }

        public SettingsDialog(string currentLogFilePath)
        {
            InitializeComponent();
            textBoxLogFilePath.Text = currentLogFilePath;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            // Validate the file path if needed
            if (string.IsNullOrWhiteSpace(textBoxLogFilePath.Text))
            {
                MessageBox.Show("Please enter a valid file path.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            LogFilePath = textBoxLogFilePath.Text;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            using (var folderBrowser = new FolderBrowserDialog())
            {
                folderBrowser.InitialDirectory = textBoxLogFilePath.Text;
                if (folderBrowser.ShowDialog() == DialogResult.OK)
                {
                    textBoxLogFilePath.Text = folderBrowser.SelectedPath;
                }
            }
        }
    }
}
