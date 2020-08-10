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
        public struct ListViewItemTag
        {
            /// <summary>
            /// Index into the <see cref="OriginalNames"/> list for the original name.
            /// </summary>
            public int OriginalNameIndex;
            /// <summary>
            /// File extension for the file type.
            /// </summary>
            public string FileExtension;
        }

        /// <summary>
        /// List of original file names
        /// </summary>
        public string[] OriginalNames { get; private set; }
        /// <summary>
        /// List of new file names
        /// </summary>
        public string[] NewFileNames { get; private set; }
        /// <summary>
        /// File path for the archive the files are to be copied into, used for duplicate name validation.
        /// </summary>
        public string ArchivePath { get; private set; }

        // Archive instance for duplicate name validation.
        private Archive validationArchive = null;

        // List of existing file names in the archive the files are being copied to.
        private string[] existingFileNames;

        public RenameFileDialog(string[] fileNames, string archivePath = null)
        {
            InitializeComponent();

            // Initialize fields.
            this.OriginalNames = fileNames;
            this.ArchivePath = archivePath;

            // If an archive path was specified get the archive instance for it.
            if (this.ArchivePath != null)
            {
                // Get the archive instance.
                this.validationArchive = ArchiveCollection.Instance.GetArchiveFromFilePath(this.ArchivePath);
                if (this.validationArchive != null)
                {
                    // Build a list of existing file names to validate against.
                    this.existingFileNames = this.validationArchive.FileEntries.Select(f => f.FileName).ToArray();
                }
            }
        }

        private void RenameFileDialog_Load(object sender, EventArgs e)
        {
            // Loop through all of the file names to rename and add them to the list view.
            bool errorsExist = false;
            for (int i = 0; i < this.OriginalNames.Length; i++)
            {
                // Split the file extension from the file path.
                int dotIndex = this.OriginalNames[i].LastIndexOf('.');
                string fileNameNoExt = this.OriginalNames[i].Substring(0, dotIndex);
                string FileExtension = this.OriginalNames[i].Substring(dotIndex + 1);

                // Create a new list view item.
                ListViewItem item = new ListViewItem(fileNameNoExt);
                item.SubItems.Add(this.OriginalNames[i]);

                // Setup the item tag.
                ListViewItemTag itemTag;
                itemTag.OriginalNameIndex = i;
                itemTag.FileExtension = FileExtension;
                item.Tag = itemTag;

                // If the file name is too long mark the item in red.
                if (fileNameNoExt.Length > ArchiveFileEntry.kUsableFileNameLength)
                {
                    // Archive name is too long.
                    item.BackColor = Color.PaleVioletRed;
                    item.ToolTipText = "File name is too long or too short!";
                    errorsExist = true;
                }
                else if (this.OriginalNames.Length > 1 && this.existingFileNames != null && this.existingFileNames.Contains(this.OriginalNames[i]) == true)
                {
                    // File name is a duplicate.
                    item.BackColor = Color.PaleVioletRed;
                    item.ToolTipText = "File name is already in use!";
                    errorsExist = true;
                }

                // Add the item to the list view.
                this.listView1.Items.Add(item);
            }

            // Sort the list view.
            this.listView1.Sort();

            // If errors were found display a message to the user.
            if (errorsExist == true)
            {
                // Display an error.
                this.Visible = true;
                MessageBox.Show("One or more files names are invalid, hover over the file names to get more info", "Invalid file names", MessageBoxButtons.OK);
            }
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
                // Get the full file name using the file extension from the item's tag.
                ListViewItemTag itemTag = (ListViewItemTag)this.listView1.Items[i].Tag;
                string fullFileName = this.listView1.Items[i].SubItems[0].Text + "." + itemTag.FileExtension;

                // Make sure the new file name is valid.
                if (this.listView1.Items[i].SubItems[0].Text.Length < 1 || this.listView1.Items[i].SubItems[0].Text.Length > ArchiveFileEntry.kUsableFileNameLength)
                {
                    // Set the list view item back color.
                    this.listView1.Items[i].SubItems[0].BackColor = Color.PaleVioletRed;
                    this.listView1.Items[i].ToolTipText = "File name is too long or too short!";
                    namesAreValid = false;
                }
                else if (ValidateFileName(this.listView1.Items[i].SubItems[0].Text) == false)
                {
                    // Set the list view item back color.
                    this.listView1.Items[i].SubItems[0].BackColor = Color.PaleVioletRed;
                    this.listView1.Items[i].ToolTipText = "File name is invalid!";
                    namesAreValid = false;
                }
                else if ((this.existingFileNames != null && this.existingFileNames.Contains(fullFileName) == true) || ListViewContainsDuplicate(fullFileName) == true)
                {
                    // Set the list view item back color.
                    this.listView1.Items[i].SubItems[0].BackColor = Color.PaleVioletRed;
                    this.listView1.Items[i].ToolTipText = "File name is already in use!";
                    namesAreValid = false;
                }
                else
                {
                    // Set the back color to normal.
                    this.listView1.Items[i].SubItems[0].BackColor = Color.FromKnownColor(KnownColor.Window);
                    this.listView1.Items[i].ToolTipText = "";
                }

                // Add the file name to the list of renamed files.
                newFileNames[itemTag.OriginalNameIndex] = this.listView1.Items[i].SubItems[0].Text;
            }

            // Check if the file names were all valid.
            if (namesAreValid == false)
            {
                // Display an error to the user.
                MessageBox.Show("One or more files names are invalid, hover over the file names to get more info", "Invalid file names", MessageBoxButtons.OK);
                return;
            }

            // Set the new file names array and dialog result, then close.
            this.NewFileNames = newFileNames;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void listView1_AfterLabelEdit(object sender, LabelEditEventArgs e)
        {
            // If the label is null return.
            if (e.Label == null)
                return;

            // Get the new full file name using the file extension from the list view item.
            ListViewItemTag itemTag = (ListViewItemTag)this.listView1.Items[e.Item].Tag;
            string fullFileName = e.Label + "." + itemTag.FileExtension;

            // Make sure the length of the item text is valid.
            if (e.Label.Length == 0 || e.Label.Length > ArchiveFileEntry.kUsableFileNameLength)
            {
                // Set the item color to red.
                this.listView1.Items[e.Item].BackColor = Color.PaleVioletRed;
                this.listView1.Items[e.Item].ToolTipText = "File name is too long or too short!";

                // Display an error to the user.
                MessageBox.Show("File name is either too short or too long, must be between 1 and 63 characters!", "Invalid file names", MessageBoxButtons.OK);
            }
            else if ((this.existingFileNames != null && this.existingFileNames.Contains(fullFileName) == true) || FindListViewItem(e.Label, itemTag.FileExtension) == true)
            {
                // Set the item color to red.
                this.listView1.Items[e.Item].BackColor = Color.PaleVioletRed;
                this.listView1.Items[e.Item].ToolTipText = "File name is already in use!";

                // Display an error to the user.
                MessageBox.Show("File name is already in use!", "Invalid file names", MessageBoxButtons.OK);
            }
            else
            {
                // Restore the item color in case it was red.
                this.listView1.Items[e.Item].BackColor = Color.FromKnownColor(KnownColor.Window);
                this.listView1.Items[e.Item].ToolTipText = "";

                // In case there was a duplicate item, find it, and change the color and tooltip.
                ListViewItem dupItem = FindDuplicateItem(this.listView1.Items[e.Item]);
                if (dupItem != null)
                {
                    // Restore item color and tooltip text.
                    dupItem.BackColor = Color.FromKnownColor(KnownColor.Window);
                    dupItem.ToolTipText = "";
                }
            }
        }

        /// <summary>
        /// Determines if the specified file name is valid or not
        /// </summary>
        /// <param name="fileName">File name to validate</param>
        /// <returns>True if the file name is valid, false otherwise</returns>
        private bool ValidateFileName(string fileName)
        {
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

        private bool ListViewContainsDuplicate(string itemText)
        {
            // Loop through all of the list view items and search for duplicates.
            int count = 0;
            for (int i = 0; i < this.listView1.Items.Count; i++)
            {
                // Get the full file name using the file extension from the item tag.
                ListViewItemTag itemTag = (ListViewItemTag)this.listView1.Items[i].Tag;
                string fullFileName = this.listView1.Items[i].Text + "." + itemTag.FileExtension;

                // Check if this item has the same text.
                if (fullFileName.Equals(itemText, StringComparison.OrdinalIgnoreCase) == true)
                {
                    // Increment the counter and check if we have a duplicate.
                    if (++count > 1)
                        return true;
                }
            }

            // If we made it here then there are no duplicates.
            return false;
        }

        private ListViewItem FindDuplicateItem(ListViewItem item)
        {
            // Loop through all of the list view items and search for one with the same text that is no the same instance.
            for (int i = 0; i < this.listView1.Items.Count; i++)
            {
                // Check if this item has the same text but different instance.
                if (item.Index != i && this.listView1.Items[i].Text.Equals(item.Text, StringComparison.OrdinalIgnoreCase) == true &&
                    ((ListViewItemTag)item.Tag).FileExtension == ((ListViewItemTag)this.listView1.Items[i].Tag).FileExtension)
                {
                    // Found the duplicate item.
                    return this.listView1.Items[i];
                }
            }

            // No duplicate item was found.
            return null;
        }

        private bool FindListViewItem(string text, string fileExtension)
        {
            // Loop through all of the list view items and search for the one specified.
            for (int i = 0; i < this.listView1.Items.Count; i++)
            {
                // Check if this item has matching name and extension.
                if (this.listView1.Items[i].Text.Equals(text, StringComparison.OrdinalIgnoreCase) == true && ((ListViewItemTag)this.listView1.Items[i].Tag).FileExtension == fileExtension)
                {
                    // Found the item.
                    return true;
                }
            }

            // If we made it here no such item exists.
            return false;
        }
    }
}
