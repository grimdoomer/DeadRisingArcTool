using DeadRisingArcTool.FileFormats.Archive;
using DeadRisingArcTool.UI;
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
    public enum ArchiveSelectReason
    {
        /// <summary>
        /// Browse for a folder of archives and select which ones to load
        /// </summary>
        LoadArchives,
        /// <summary>
        /// Select a loaded archive to move a file to
        /// </summary>
        CopyTo,
    }

    public partial class ArchiveSelectDialog : Form
    {
        /// <summary>
        /// Selection reason the dialog was displayed for
        /// </summary>
        public ArchiveSelectReason SelectReason { get; private set; }
        /// <summary>
        /// Full path of the selected archive folder
        /// </summary>
        public string SelectedFolder { get; private set; }

        private List<Tuple<string, bool>> archivesList = new List<Tuple<string, bool>>();
        /// <summary>
        /// List of tuples containing the file path to the selected archive and a boolean indicating if it is a patch file or not.
        /// </summary>
        public Tuple<string, bool>[] SelectedArchives { get; private set; } = new Tuple<string, bool>[0];
        /// <summary>
        /// Selected path relative to the root of the selected archive.
        /// </summary>
        public string SelectedRelativePath { get; private set; } = "\\";
        /// <summary>
        /// Determines if the game archives were hidden from view or not.
        /// </summary>
        public bool HideGameArchives { get; private set; }

        public ArchiveSelectDialog(ArchiveSelectReason reason, bool hideGameArchives)
        {
            InitializeComponent();

            // Initialize fields.
            this.SelectReason = reason;
            this.HideGameArchives = hideGameArchives;

            // Set the image list.
            this.lstArchives.SmallImageList = IconSet.Instance.IconImageList;
            this.treeView1.ImageList = IconSet.Instance.IconImageList;

            // Change UI options for the selection reason.
            if (this.SelectReason == ArchiveSelectReason.LoadArchives)
            {
                // Hide the tree view.
                this.treeView1.Visible = false;
            }
            else if (this.SelectReason == ArchiveSelectReason.CopyTo)
            {
                // Hide the list view.
                this.lstArchives.Visible = false;

                // Change the load button text.
                this.btnLoadArchives.Text = "Copy Files";
            }
        }

        private void ArchiveSelectDialog_Load(object sender, EventArgs e)
        {
            // Hide the dialog until a folder is selected.
            this.Visible = false;

            // Check the dialog select reason and handle accordingly.
            if (this.SelectReason == ArchiveSelectReason.LoadArchives)
            {
                // Suspend the list view from updating.
                this.lstArchives.SuspendLayout();

                // Browse for a folder of archives.
                BrowseForArchiveFolder();

                // Resume the list view layout.
                this.lstArchives.ResumeLayout(false);

                // Set the width of the column header to the length of the longest list view item.
                this.lstArchives.Columns[0].Width = -1;
                if (this.lstArchives.Columns[0].Width < this.lstArchives.Width)
                {
                    // Largest file name is less than width of the list view, set column header width to list view width.
                    this.lstArchives.Columns[0].Width = -2;
                }
            }
            else if (this.SelectReason == ArchiveSelectReason.CopyTo)
            {
                // Suspend the tree view layout.
                this.treeView1.SuspendLayout();

                // Use the list of currently loaded archives.
                MoveToArchive();

                // Resume the tree view layout.
                this.treeView1.ResumeLayout(true);
            }

            // Re-enable the dialog.
            this.Visible = true;
        }

        #region Context menu

        private void newArchiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Show a save file dialog to save the new archive.
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Archive (*.arc)|*.arc";
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                // Add the new arc file location to the selected archives list.
                this.SelectedArchives = new Tuple<string, bool>[1]
                {
                    new Tuple<string, bool>(sfd.FileName, true)
                };

                // Set the dialog result and close.
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void selectAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Loop through all of the list view items and select each one.
            for (int i = 0; i < this.lstArchives.Items.Count; i++)
                this.lstArchives.Items[i].Checked = true;
        }

        private void clearAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Loop through all of the list view items and unselect each one.
            for (int i = 0; i < this.lstArchives.Items.Count; i++)
                this.lstArchives.Items[i].Checked = false;
        }

        #endregion

        private void btnLoadArchives_Click(object sender, EventArgs e)
        {
            // Create a list to hold the selected archive file paths.
            List<Tuple<string, bool>> selectedArchives = new List<Tuple<string, bool>>();

            // Check the archive select reason and handle accordingly.
            if (this.SelectReason == ArchiveSelectReason.LoadArchives)
            {
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
            }
            else if (this.SelectReason == ArchiveSelectReason.CopyTo)
            {
                // Find the upper most parent for the selected node to get the archive index.
                int archiveIndex = -1;
                for (TreeNode node = this.treeView1.SelectedNode; node != null; node = node.Parent)
                {
                    // Check if this node has a valid tag.
                    if (node.Tag != null)
                    {
                        // Get the archive index and break the loop.
                        archiveIndex = (int)node.Tag;
                        break;
                    }
                }

                // Add the archive info for the selected archive to the list of selected archives.
                selectedArchives.Add(this.archivesList[archiveIndex]);

                // Set the selected folder path relative to the archive node.
                int index = this.treeView1.SelectedNode.FullPath.IndexOf("\\");
                if (index != -1)
                {
                    // Remove the archive name from the folder path.
                    this.SelectedRelativePath = this.treeView1.SelectedNode.FullPath.Substring(index + 1);
                }
            }

            // There must be at least one archive selected.
            if (selectedArchives.Count == 0)
            {
                // Display an error and then return.
                MessageBox.Show("Please select at least one archive to load");
                return;
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

        #region Archive loading

        private void BrowseForArchiveFolder()
        {
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
                    item.ImageIndex = (int)UIIcon.PatchArchive;
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
                    item.ImageIndex = (int)UIIcon.Archive;
                    this.lstArchives.Items.Add(item);
                }
            }
        }

        private void MoveToArchive()
        {
            // TODO: This displays all archives in the root of the tree view. If there are multiple
            // archives with the same name but in different folder locations, two TreeNodes will be
            // added to the root of the tree view with the same name. Eventually we need to fix this.
            // The ListView code has the same issue.

            // Loop through every archvie and create the tree view hierarchy.
            for (int i = 0; i < ArchiveCollection.Instance.Archives.Length; i++)
            {
                // Pull out the archive for easy access.
                Archive archive = ArchiveCollection.Instance.Archives[i];

                // Check if we should hide game archives.
                if (this.HideGameArchives == true && archive.IsPatchFile == false)
                    continue;

                // Add the archive to the list of archives listed.
                int archiveIndex = this.archivesList.Count;
                this.archivesList.Add(new Tuple<string, bool>(archive.FileName, archive.IsPatchFile));

                // Create a new tree node for the archive.
                TreeNode archiveNode = new TreeNode(archive.FileName.Substring(archive.FileName.LastIndexOf("\\") + 1));
                archiveNode.ImageIndex = (int)(archive.IsPatchFile == true ? UIIcon.PatchArchive : UIIcon.Archive);
                archiveNode.Tag = archiveIndex;

                // Loop through all of the files in the archive and built the folder hierarchy.
                for (int j = 0; j < archive.FileEntries.Length; j++)
                {
                    // Split the file name into folder names.
                    string[] pieces = archive.FileEntries[j].FileName.Split(new string[] { "\\" }, StringSplitOptions.None);

                    // Loop through all the folder pieces and add each one to the tree node.
                    TreeNode parentNode = archiveNode;
                    for (int k = 0; k < pieces.Length - 1; k++)
                    {
                        // Check if the parent node has a child node with the same name as the current folder.
                        TreeNode[] nodesFound = parentNode.Nodes.Find(pieces[k], false);
                        if (nodesFound.Length > 0)
                        {
                            // Set the new parent tree node.
                            parentNode = nodesFound[0];
                        }
                        else
                        {
                            // Create a new child node for the folder.
                            TreeNode child = new TreeNode(pieces[k]);
                            child.Name = pieces[k];

                            // Color the node accordingly.
                            if (archive.IsPatchFile == true)
                            {
                                child.ForeColor = Color.Blue;
                                child.ImageIndex = (int)UIIcon.FolderBlue;
                            }
                            else
                            {
                                child.ImageIndex = (int)UIIcon.Folder;
                            }

                            // Set the selected image index.
                            child.SelectedImageIndex = child.ImageIndex;

                            // Add the new tree node to the parent.
                            parentNode.Nodes.Add(child);
                            parentNode = child;
                        }
                    }
                }

                // Add the archive node to the tree view.
                this.treeView1.Nodes.Add(archiveNode);
            }

            // Sort the tree view nodes.
            this.treeView1.Sort();
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

        #endregion
    }
}
