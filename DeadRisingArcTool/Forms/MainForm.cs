using DeadRisingArcTool.Controls;
using DeadRisingArcTool.FileFormats;
using DeadRisingArcTool.FileFormats.Archive;
using DeadRisingArcTool.FileFormats.Bitmaps;
using DeadRisingArcTool.FileFormats.Geometry;
using DeadRisingArcTool.FileFormats.Misc;
using DeadRisingArcTool.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeadRisingArcTool
{
    public partial class MainForm : Form
    {
        public enum TreeViewMenuState
        {
            /// <summary>
            /// No tree view context menu options are available.
            /// </summary>
            None,
            /// <summary>
            /// Tree view context menu options that are universal are enabled.
            /// </summary>
            AnyFile,
            /// <summary>
            /// Tree view context menu options that are file-only are enabled.
            /// </summary>
            PerFile
        }

        // Background worker for async operations.
        BackgroundWorker asyncWorker = null;

        // Loading dialog used while loading the arc folder.
        LoadingDialog loadingDialog = null;

        // List of currently loaded arc files.
        ArcFileCollection arcFileCollection = null;

        // Specialized editors.
        List<GameResourceEditorControl> resourceEditors = new List<GameResourceEditorControl>();
        int activeResourceEditor = -1;

        public MainForm()
        {
            InitializeComponent();

            // Set the tag properties of the tree view context menu options.
            this.extractToolStripMenuItem.Tag = TreeViewMenuState.PerFile;
            this.sortByToolStripMenuItem.Tag = TreeViewMenuState.AnyFile;
            this.fileNameToolStripMenuItem.Tag = TreeViewMenuState.AnyFile;
            this.arcFileToolStripMenuItem.Tag = TreeViewMenuState.AnyFile;
            this.resourceTypeToolStripMenuItem.Tag = TreeViewMenuState.AnyFile;

            // Disable all the tree view context menu items.
            SetTreeViewMenuState(TreeViewMenuState.None);
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Get a list of all editor controls that have the GameResourceEditor attribute.
            Type[] resourceEditorTypes = Assembly.GetExecutingAssembly().GetTypes().Where(t => t.GetCustomAttribute(typeof(GameResourceEditorAttribute)) != null).ToArray();

            // Loop through all the resource editor types and create each one.
            for (int i = 0; i < resourceEditorTypes.Length; i++)
            {
                // Create a new instance of the game resource editor.
                GameResourceEditorControl editorControl = (GameResourceEditorControl)Activator.CreateInstance(resourceEditorTypes[i]);

                // Setup the editor control.
                editorControl.Visible = false;
                editorControl.Location = new Point(this.FileInfoBox.Location.X - 3, this.FileInfoBox.Location.Y + this.FileInfoBox.Size.Height + 5);
                editorControl.Size = new Size(this.splitContainer1.Panel2.Width - 15, this.splitContainer1.Panel2.Height - editorControl.Location.Y);
                editorControl.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;

                // Add the editor to the editor panel and the resource editor list.
                this.splitContainer1.Panel2.Controls.Add(editorControl);
                this.resourceEditors.Add(editorControl);
            }
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Browse for the game's arc file folder.
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            fbd.Description = "Dead Rising arc folder";

            // Debug hacks because I'm lazy.
#if DEBUG
            fbd.SelectedPath = "K:\\_SteamLibrary\\steamapps\\common\\Dead Rising\\nativeWin64";
#endif

            // Show the dialog.
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                // Setup the background worker.
                this.asyncWorker = new BackgroundWorker();
                this.asyncWorker.DoWork += AsyncWorker_LoadArcFolder;
                this.asyncWorker.ProgressChanged += AsyncWorker_LoadArcFolderProgress;
                this.asyncWorker.RunWorkerCompleted += AsyncWorker_LoadArcFolderCompleted;
                this.asyncWorker.WorkerReportsProgress = true;
                this.asyncWorker.WorkerSupportsCancellation = true;

                // Initialize the arc file collection.
                this.arcFileCollection = new ArcFileCollection(fbd.SelectedPath);

                // Run the background worker.
                this.asyncWorker.RunWorkerAsync(this.arcFileCollection);

                // Disable the main form and display the loading dialog.
                this.Enabled = false;
                this.loadingDialog = new LoadingDialog();
                if (this.loadingDialog.ShowDialog() == DialogResult.Cancel)
                {
                    // Cancel the background worker.
                    this.asyncWorker.CancelAsync();
                }
            }
        }

        #region AsyncWorker: Arc loading

        private void AsyncWorker_LoadArcFolderCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            // Check if the operation was canceled or not.
            if (e.Cancelled == true || e.Result == null)
            {
                // Set the dialog result and close it.

                // Re-enable the main form and return.
                this.Enabled = true;
                return;
            }

            // Suspend the form layout while we update the tree view.
            this.treeView1.SuspendLayout();

            // Build the tree node graph from the arc file collection.
            TreeNodeCollection treeNodes = (TreeNodeCollection)e.Result;
            this.treeView1.Nodes.AddRange(treeNodes.Cast<TreeNode>().ToArray());

            // Sort the tree view.
            this.treeView1.Sort();

            // Resume the layout and enable the form.
            this.treeView1.ResumeLayout();
            this.Enabled = true;

            // Close the loading dialog which will unblock the GUI thread.
            this.loadingDialog.DialogResult = DialogResult.OK;
            this.loadingDialog.Close();
        }

        private void AsyncWorker_LoadArcFolderProgress(object sender, ProgressChangedEventArgs e)
        {
            // Update progress on the loading dialog.
            if (e.UserState != null)
            {
                // Check the progress state object type and handle accordingly.
                if (e.UserState.GetType() == typeof(string))
                {
                    // Update progress bar.
                    this.loadingDialog.UpdateProgress((string)e.UserState);
                }
            }
            else
            {
                // The percentage is the number of files to process.
                this.loadingDialog.SetupProgress(e.ProgressPercentage);
            }
        }

        private void AsyncWorker_LoadArcFolder(object sender, DoWorkEventArgs e)
        {
            // Get the BackgroundWorker instance from the sender object.
            BackgroundWorker worker = (BackgroundWorker)sender;

            // Get the arc file collection from the thread argument.
            ArcFileCollection arcCollection = (ArcFileCollection)e.Argument;

            // Build a list of arc files to process.
            string[] arcFiles = FindArcFilesInFolder(arcCollection.RootDirectory, true, false);
            if (arcFiles.Length == 0)
            {
                // No arc files were found.
                // TODO: Bubble this up to the UI.
                e.Result = null;
                return;
            }

            // Report the number of arc files to the progress bar.
            worker.ReportProgress(arcFiles.Length, null);

            // Loop and load each arc file.
            for (int i = 0; i < arcFiles.Length; i++)
            {
                // Check if we should cancel the operation.
                if (e.Cancel == true)
                    return;

                // Report progress on the current arc file.
                worker.ReportProgress(i, arcFiles[i].Substring(arcFiles[i].LastIndexOf("\\") + 1));

                // Create a new arc file and parse the file table.
                ArcFile arcFile = new ArcFile(arcFiles[i]);
                if (arcFile.OpenAndRead() == false)
                {
                    // Failed to read the arc file.
                    // TODO: Bubble this up to the UI, for now just skip.
                    continue;
                }

                // Add the arc file to the collection.
                arcCollection.AddArcFile(arcFile);
            }

            // Build the tree view node collection now so we don't tie up the GUI thread.
            e.Result = arcCollection.BuildTreeNodeArray(TreeNodeOrder.FolderPath);
        }

        #endregion

        #region Utilities

        private static string[] FindArcFilesInFolder(string folderPath, bool recursive, bool includeNonArcFiles)
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
                    filesFound.AddRange(FindArcFilesInFolder(dirInfo.FullName, recursive, includeNonArcFiles));
                }
            }

            // Return the list of files found.
            return filesFound.ToArray();
        }

        #endregion

        #region TreeView

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Check if the selected node is valid and has a the tag property set.
            if (e.Node == null || e.Node.Tag == null)
            {
                // Clear properties view.
                this.lblArcFile.Text = "";
                this.lblCompressedSize.Text = "";
                this.lblDecompressedSize.Text = "";
                this.lblOffset.Text = "";
                this.lblFileType.Text = "";

                // Loop through all of the resource editors and hide all of them.
                for (int i = 0; i < this.resourceEditors.Count; i++)
                    this.resourceEditors[i].Visible = false;

                // Do options for this node.
                SetTreeViewMenuState(TreeViewMenuState.AnyFile);
                return;
            }

            // Get the datum index for the arc file entry.
            DatumIndex datum = (DatumIndex)e.Node.Tag;
            ArcFileEntry fileEntry = this.arcFileCollection.ArcFiles[datum.ArcIndex].FileEntries[datum.FileIndex];

            // Update the properties view.
            this.lblArcFile.Text = this.arcFileCollection.ArcFiles[datum.ArcIndex].FileName.Substring(this.arcFileCollection.ArcFiles[datum.ArcIndex].FileName.LastIndexOf("\\") + 1);
            this.lblCompressedSize.Text = fileEntry.CompressedSize.ToString();
            this.lblDecompressedSize.Text = fileEntry.DecompressedSize.ToString();
            this.lblOffset.Text = fileEntry.DataOffset.ToString();
            this.lblFileType.Text = fileEntry.FileType.ToString();

            // Check if there is a resource parser for this game resource.
            if (GameResource.ResourceParsers.ContainsKey(fileEntry.FileType) == true)
            {
                // Decompress the file entry so we can load specialied editors for it.
                byte[] decompressedData = this.arcFileCollection.ArcFiles[datum.ArcIndex].DecompressFileEntry(datum.FileIndex);
                if (decompressedData != null)
                {
                    // Parse the game resource using the parser type.
                    GameResource resource = GameResource.FromGameResource(decompressedData, fileEntry.FileName, fileEntry.FileType, 
                        this.arcFileCollection.ArcFiles[datum.ArcIndex].Endian == IO.Endianness.Big);
                    if (resource != null)
                    {
                        // Loop through all of the resource editors and see if we have one that supports this resource type.
                        for (int i = 0; i < this.resourceEditors.Count; i++)
                        {
                            // Check if this editor supports this resource type.
                            if (this.resourceEditors[i].CanEditResource(fileEntry.FileType) == true)
                            {
                                // Update the resource being edited and make the editor visible.
                                this.resourceEditors[i].UpdateResource(this.arcFileCollection.ArcFiles[datum.ArcIndex], resource);
                                this.resourceEditors[i].Visible = true;

                                // Set this as the active resource editor.
                                this.activeResourceEditor = i;
                            }
                            else
                            {
                                // Clear any old game resources and make sure the editor is invisible.
                                this.resourceEditors[i].UpdateResource(null, null);
                                this.resourceEditors[i].Visible = false;
                            }
                        }
                    }
                }
            }

            // Enable context menu options.
            SetTreeViewMenuState(TreeViewMenuState.PerFile);
        }

        private void SetTreeViewMenuState(TreeViewMenuState state)
        {
            // Get a list of buttons from the tree view context menu.
            ToolStripItem[] menuItems = GetTreeViewContextMenuItems(this.treeViewContextMenu.Items);

            // Loop through and enable/disable them based on their Tag property.
            for (int i = 0; i < menuItems.Length; i++)
            {
                // Check if the tag property is valid.
                if (menuItems[i].Tag != null && menuItems[i].Tag.GetType() == typeof(TreeViewMenuState))
                {
                    // Enable or disable the menu item based on the menu state.
                    if (state == TreeViewMenuState.None)
                        menuItems[i].Enabled = false;
                    else if (state == (TreeViewMenuState)menuItems[i].Tag)
                        menuItems[i].Enabled = true;
                    else if (state == TreeViewMenuState.PerFile && (TreeViewMenuState)menuItems[i].Tag == TreeViewMenuState.AnyFile)
                        menuItems[i].Enabled = true;
                    else
                        menuItems[i].Enabled = false;
                }
            }
        }

        private ToolStripMenuItem[] GetTreeViewContextMenuItems(ToolStripItemCollection rootCollection)
        {
            // Create a list to hold all of the tree view context menu items.
            List<ToolStripMenuItem> menuItems = new List<ToolStripMenuItem>();

            // Add all of the items in the root collection to the list.
            foreach (ToolStripMenuItem item in rootCollection)
            {
                // Add the item to the collection.
                menuItems.Add(item);

                // If the menu item has child items recursively add them as well.
                if (item.HasDropDownItems)
                    menuItems.AddRange(GetTreeViewContextMenuItems(item.DropDownItems));
            }

            // Return the collection of menu items found.
            return menuItems.ToArray();
        }

        private void extractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if the selected node is valid and has a the tag property set.
            if (this.treeView1.SelectedNode == null || this.treeView1.SelectedNode.Tag == null)
            {
                return;
            }

            // Get the datum index for the arc file entry.
            DatumIndex datum = (DatumIndex)this.treeView1.SelectedNode.Tag;

            // Prompt the user where to save the file.
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.FileName = this.treeView1.SelectedNode.Text;
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                // Disable the form while we extract the file.
                this.Enabled = false;

                // Extract the file.
                this.arcFileCollection.ArcFiles[datum.ArcIndex].ExtractFile(datum.FileIndex, sfd.FileName);

                // Re-enable the form.
                this.Enabled = true;
                MessageBox.Show("Done!");
            }
        }

        private void fileNameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // If we are already checked then there is nothing to do.
            if (this.fileNameToolStripMenuItem.Checked == true)
                return;

            // Update the checked state of the sort buttons.
            this.fileNameToolStripMenuItem.Checked = true;
            this.arcFileToolStripMenuItem.Checked = false;
            this.resourceTypeToolStripMenuItem.Checked = false;

            // Resort the tree view.
            SetTreeViewSortOrder(TreeNodeOrder.FolderPath);
        }

        private void arcFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // If we are already checked then there is nothing to do.
            if (this.arcFileToolStripMenuItem.Checked == true)
                return;

            // Update the checked state of the sort buttons.
            this.fileNameToolStripMenuItem.Checked = false;
            this.arcFileToolStripMenuItem.Checked = true;
            this.resourceTypeToolStripMenuItem.Checked = false;

            // Resort the tree view.
            SetTreeViewSortOrder(TreeNodeOrder.ArcFile);
        }

        private void resourceTypeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // If we are already checked then there is nothing to do.
            if (this.resourceTypeToolStripMenuItem.Checked == true)
                return;

            // Update the checked state of the sort buttons.
            this.fileNameToolStripMenuItem.Checked = false;
            this.arcFileToolStripMenuItem.Checked = false;
            this.resourceTypeToolStripMenuItem.Checked = true;

            // Resort the tree view.
            SetTreeViewSortOrder(TreeNodeOrder.ResourceType);
        }

        private void SetTreeViewSortOrder(TreeNodeOrder order)
        {
            // Set the main form to be disabled while we populate the tree view.
            this.Enabled = false;
            this.treeView1.SuspendLayout();

            // If there is game resource node selected save its datum index.
            //DatumIndex selectedNode = (DatumIndex)(this.treeView1.SelectedNode != null ? this.treeView1.SelectedNode.Tag : null);

            // Clear all the old nodes out of the tree view.
            this.treeView1.Nodes.Clear();

            // Build the tree node graph from the arc file collection.
            TreeNodeCollection treeNodes = this.arcFileCollection.BuildTreeNodeArray(order);
            this.treeView1.Nodes.AddRange(treeNodes.Cast<TreeNode>().ToArray());

            // Sort the tree view.
            this.treeView1.Sort();

            // Resume the layout and enable the form.
            this.treeView1.ResumeLayout();
            this.Enabled = true;
        }

        #endregion
    }
}
