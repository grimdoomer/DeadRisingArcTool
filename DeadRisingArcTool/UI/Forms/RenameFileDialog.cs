using DeadRisingArcTool.FileFormats.Archive;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeadRisingArcTool.UI.Forms
{
    public partial class RenameFileDialog : Form
    {
        /// <summary>
        /// List of original file names
        /// </summary>
        public string[] OriginalNames { get; private set; }
        /// <summary>
        /// List of new file names
        /// </summary>
        public string[] NewFileNames { get; private set; }

        public RenameFileDialog(string[] fileNames)
        {
            InitializeComponent();

            // Initialize fields.
            this.OriginalNames = fileNames;
        }

        private void RenameFileDialog_Load(object sender, EventArgs e)
        {
            // Loop through all of the file names to rename and add them to the list view.
            for (int i = 0; i < this.OriginalNames.Length; i++)
            {
                // Create a new list view item.
                ListViewItem item = new ListViewItem(this.OriginalNames[i]);
                item.SubItems.Add(this.OriginalNames[i]);
                item.Tag = i;

                // If the file name is too long mark the item in red.
                if (this.OriginalNames[i].Length > ArchiveFileEntry.kUsableFileNameLength)
                {
                    // Archive name is too long.
                    item.BackColor = Color.PaleVioletRed;
                }

                // Add the item to the list view.
                this.listView1.Items.Add(item);
            }

            // Sort the list view.
            this.listView1.Sort();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            // Set the dialog result and close.
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            // List of new file names.
            string[] newFileNames = new string[this.OriginalNames.Length];
            bool namesAreValid = true;

            // Loop through all of the list view items and validate each one.
            for (int i = 0; i < this.listView1.Items.Count; i++)
            {
                // Make sure the new file name is valid.
                if (ValidateFileName(this.listView1.Items[i].SubItems[0].Text) == false)
                {
                    // Set the list view item back color.
                    this.listView1.Items[i].SubItems[0].BackColor = Color.PaleVioletRed;
                    namesAreValid = false;
                }
                else
                {
                    // Set the back color to normal.
                    this.listView1.Items[i].SubItems[0].BackColor = Color.FromKnownColor(KnownColor.Window);
                }

                // Add the file name to the list of renamed files.
                newFileNames[(int)this.listView1.Items[i].Tag] = this.listView1.Items[i].SubItems[0].Text;
            }

            // Check if the file names were all valid.
            if (namesAreValid == false)
            {
                // Display an error to the user.
                MessageBox.Show("Some file names are invalid, please correct them before continuing!", "Invalid file names", MessageBoxButtons.OK);
                return;
            }

            // Set the new file names array and dialog result, then close.
            this.NewFileNames = newFileNames;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// Determines if the specified file name is valid or not
        /// </summary>
        /// <param name="fileName">File name to validate</param>
        /// <returns>True if the file name is valid, false otherwise</returns>
        private bool ValidateFileName(string fileName)
        {
            // Make sure the file name is at least 1 character long and no more than 63 characters.
            if (fileName.Length == 0 || fileName.Length > ArchiveFileEntry.kUsableFileNameLength)
                return false;

            // Make sure there are no empty folder names.
            string[] pieces = fileName.Split('\\').Where(s => s.Length == 0).ToArray();
            if (pieces.Length > 0)
                return false;

            // Make sure the file name does not start or end with a slash.
            if (fileName[0] == '\\' || fileName[fileName.Length - 1] == '\\')
                return false;

            // Good enough for me...
            return true;
        }

        private void listView1_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            // Make sure the length of the item text is valid.
            if (e.Label.Length == 0 || e.Label.Length > ArchiveFileEntry.kUsableFileNameLength)
            {
                // Display an error to the user and cancel the operation.
                MessageBox.Show("Item name is either too short or too long, must be between 1 and 63 characters!");
                e.CancelEdit = true;
            }
            else
            {
                // Restore the item color in case it was red.
                this.listView1.Items[e.Item].BackColor = Color.FromKnownColor(KnownColor.Window);
            }
        }
    }
}
