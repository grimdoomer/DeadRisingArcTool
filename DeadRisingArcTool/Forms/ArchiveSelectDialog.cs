using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeadRisingArcTool.Forms
{
    public partial class ArchiveSelectDialog : Form
    {
        /// <summary>
        /// Full path of the selected archive folder
        /// </summary>
        public string SelectedFolder { get; private set; }

        private List<Tuple<string, bool>> archivesList = new List<Tuple<string, bool>>();
        /// <summary>
        /// List of tuples containing the file path to the selected archive and a boolean indicating if it is a patch file or not.
        /// </summary>
        public Tuple<string, bool>[] SelectedArchives { get; private set; } = new Tuple<string, bool>[0];

        public ArchiveSelectDialog()
        {
            InitializeComponent();
        }

        private void ArchiveSelectDialog_Load(object sender, EventArgs e)
        {
            // Show the dialog but make it disabled.
            this.Enabled = false;
            this.Visible = true;

            // Browse for the game's arc file folder.
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Dead Rising arc folder";

            // Check if there is a saved file path in the settings file.
            if (Properties.Settings.Default.ArcFolder != string.Empty)
                fbd.SelectedPath = Properties.Settings.Default.ArcFolder;

            // Show the dialog.
            if (fbd.ShowDialog() != DialogResult.OK)
            {
                // Set the dialog result and close.
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }

            // Save the selected folder path for next time.
            Properties.Settings.Default.ArcFolder = fbd.SelectedPath;
            Properties.Settings.Default.Save();

            // Set the selected folder path.
            this.SelectedFolder = fbd.SelectedPath;

            // Check if we should load patch files or not.
            if (Properties.Settings.Default.LoadPatchFiles == true)
            {
                // Get the length of the patch folder path for string manipulation.
                int fileNameStartIndex = Properties.Settings.Default.PatchFileDirectory.Length + 1;

                // Scan the patch folder and add each archive to the list.
                string[] patchArchives = FindArchivesInFolder(Properties.Settings.Default.PatchFileDirectory, true, false);
                for (int i = 0; i < patchArchives.Length; i++)
                {
                    // Add the archive to our tracking list
                    this.archivesList.Add(new Tuple<string, bool>(patchArchives[i], true));

                    // Add the archive to the list view.
                    ListViewItem item = new ListViewItem(patchArchives[i].Substring(fileNameStartIndex));
                    item.Checked = true;
                    item.ForeColor = Color.Blue;
                    this.lstArchives.Items.Add(item);
                }
            }

            // Scan the directory for archives and add each one to the list.
            string[] folderArchives = FindArchivesInFolder(fbd.SelectedPath, true, false);
            for (int i = 0; i < folderArchives.Length; i++)
            {
                // Get the length of the patch folder path for string manipulation.
                int fileNameStartIndex = fbd.SelectedPath.Length + 1;

                // Make sure we didn't already load this archive.
                if (this.archivesList.FindIndex(t => t.Item1 == folderArchives[i]) == -1)
                {
                    // Add the archive to our tracking list
                    this.archivesList.Add(new Tuple<string, bool>(folderArchives[i], false));

                    // Add the archive to the list view.
                    ListViewItem item = new ListViewItem(folderArchives[i].Substring(fileNameStartIndex));
                    item.Checked = true;
                    this.lstArchives.Items.Add(item);
                }
            }

            // Set the width of the column header to the length of the longest list view item.
            this.lstArchives.Columns[0].Width = -1;
            if (this.lstArchives.Columns[0].Width < this.lstArchives.Width)
            {
                // Largest file name is less than width of the list view, set column header width to list view width.
                this.lstArchives.Columns[0].Width = -2;
            }

            // Re-enable the dialog.
            this.Enabled = true;
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            // Loop through all of the list view items and select each one.
            for (int i = 0; i < this.lstArchives.Items.Count; i++)
                this.lstArchives.Items[i].Checked = true;
        }

        private void btnClearAll_Click(object sender, EventArgs e)
        {
            // Loop through all of the list view items and unselect each one.
            for (int i = 0; i < this.lstArchives.Items.Count; i++)
                this.lstArchives.Items[i].Checked = false;
        }

        private void btnLoadArchives_Click(object sender, EventArgs e)
        {
            // Create a list to hold the selected archive file paths.
            List<Tuple<string, bool>> selectedArchives = new List<Tuple<string, bool>>();

            // Loop through all of the archives found and check which ones the user wants to load.
            for (int i = 0; i < this.archivesList.Count; i++)
            {
                // Check if the archive was selected.
                if (this.lstArchives.Items[i].Checked == true)
                {
                    // Add the archive to the selection list.
                    selectedArchives.Add(this.archivesList[i]);
                }
            }

            // Set the selected archive list.
            this.SelectedArchives = selectedArchives.ToArray();

            // Set the dialog result and close.
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            // Set the dialog result and close.
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private static string[] FindArchivesInFolder(string folderPath, bool recursive, bool includeNonArcFiles)
        {
            List<string> filesFound = new List<string>();

            // Get the directory info for the specified folder.
            DirectoryInfo rootInfo = new DirectoryInfo(folderPath);

            // Loop through all child files in the folder.
            foreach (FileInfo fileInfo in rootInfo.GetFiles())
            {
                // If we are including non-arc files add the child file, otherwise check the file extension.
                if (includeNonArcFiles == true || fileInfo.Extension.Equals(".arc", StringComparison.OrdinalIgnoreCase) == true)
                {
                    // Add the file to the list.
                    filesFound.Add(fileInfo.FullName);
                }
            }

            // Check if we should search recursively.
            if (recursive == true)
            {
                // Loop through all child directories.
                foreach (DirectoryInfo dirInfo in rootInfo.GetDirectories())
                {
                    // Add any files found to the list.
                    filesFound.AddRange(FindArchivesInFolder(dirInfo.FullName, recursive, includeNonArcFiles));
                }
            }

            // Return the list of files found.
            return filesFound.ToArray();
        }
    }
}
