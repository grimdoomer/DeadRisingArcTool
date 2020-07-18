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
    public partial class MainForm : Form, IResourceEditorOwner
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

        // Specialized editors.
        List<GameResourceEditorControl> resourceEditors = new List<GameResourceEditorControl>();
        int activeResourceEditor = -1;

        public MainForm()
        {
            InitializeComponent();

#if !DEBUG
            // Hide the debug menu.
            this.dEBUGToolStripMenuItem.Visible = false;
#endif

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
                editorControl.EditorOwner = this;

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

#region MenuStrip Buttons

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Disable the main form.
            this.Enabled = false;

            // Show the archive select dialog.
            ArchiveSelectDialog selectDialog = new ArchiveSelectDialog();
            if (selectDialog.ShowDialog() == DialogResult.OK)
            {
                // Setup the background worker.
                this.asyncWorker = new BackgroundWorker();
                this.asyncWorker.DoWork += AsyncWorker_LoadArcFolder;
                this.asyncWorker.ProgressChanged += AsyncWorker_LoadArcFolderProgress;
                this.asyncWorker.RunWorkerCompleted += AsyncWorker_LoadArcFolderCompleted;
                this.asyncWorker.WorkerReportsProgress = true;
                this.asyncWorker.WorkerSupportsCancellation = true;

                // Initialize the arc file collection.
                ArchiveCollection.Instance = new ArchiveCollection(selectDialog.SelectedFolder);

                // Run the background worker.
                this.asyncWorker.RunWorkerAsync(selectDialog.SelectedArchives);

                // Disable the main form and display the loading dialog.
                this.loadingDialog = new LoadingDialog();
                if (this.loadingDialog.ShowDialog() == DialogResult.Cancel)
                {
                    // Cancel the background worker.
                    this.asyncWorker.CancelAsync();
                }
            }
            else
            {
                // Re-enable the main form.
                this.Enabled = true;
            }
        }

        private void closeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Just restart the application, it's easier than trying to close everything out.
            Application.Restart();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Quit the application.
            Application.Exit();
        }

#endregion

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

            // Set the tree view image list.
            this.treeView1.ImageList = ArchiveCollection.Instance.TreeNodeImages;

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

            // Get the list of archives to load from the worker argument.
            Tuple<string, bool>[] archives = (Tuple<string, bool>[])e.Argument;

            // Report the number of archives to the progress bar.
            worker.ReportProgress(archives.Length, null);

            // Loop and load each of the archives
            for (int i = 0; i < archives.Length; i++)
            {
                // Check if we should cancel the operation.
                if (e.Cancel == true)
                    return;

                // Report progress on the current archive.
                worker.ReportProgress(i, archives[i].Item1.Substring(archives[i].Item1.LastIndexOf("\\") + 1));

                // Add the archive to the collection.
                ArchiveCollection.Instance.AddArchive(archives[i].Item1, archives[i].Item2);
            }

            // Build the tree view node collection now so we don't tie up the GUI thread.
            e.Result = ArchiveCollection.Instance.BuildTreeNodeArray(TreeNodeOrder.FolderPath);
        }

        #endregion

#region Utilities

        /// <summary>
        /// Gets the DatumIndex for the selected tree node
        /// </summary>
        /// <returns>The DatumIndex of the selected tree node or DatumIndex.Unassigned if there is no node selected</returns>
        private DatumIndex GetSelectedResourceDatum()
        {
            // Make sure the selected node is valid and has a valid tag.
            if (this.treeView1.SelectedNode != null && this.treeView1.SelectedNode.Tag != null)
            {
                // Return the tag as a datum index.
                return (DatumIndex)this.treeView1.SelectedNode.Tag;
            }

            // Return a null datum.
            return new DatumIndex(DatumIndex.Unassigned);
        }

#endregion

#region TreeView

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // If there is an active resource editor and changes have been made, check if we should save or discard.
            if (this.activeResourceEditor != -1 && this.resourceEditors[this.activeResourceEditor].HasBeenModified == true)
            {
                // Prompt if we should save changes or discard them.
                string fileName = this.resourceEditors[this.activeResourceEditor].GameResource.FileName;
                if (MessageBox.Show("There are unsaved changes to " + Path.GetFileName(fileName) + ", would you like to save them?", 
                    "Save changes", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    // Have the editor save changes to file.
                    this.resourceEditors[this.activeResourceEditor].SaveResource();
                }
            }

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
                this.activeResourceEditor = -1;
                for (int i = 0; i < this.resourceEditors.Count; i++)
                    this.resourceEditors[i].Visible = false;

                // Do options for this node.
                SetTreeViewMenuState(TreeViewMenuState.AnyFile);
                return;
            }

            // Get the datum index for the arc file entry.
            DatumIndex datum = (DatumIndex)e.Node.Tag;
            ArchiveFileEntry fileEntry = ArchiveCollection.Instance.Archives[datum.ArchiveIndex].FileEntries[datum.FileIndex];

            // Update the properties view.
            this.lblArcFile.Text = ArchiveCollection.Instance.Archives[datum.ArchiveIndex].FileName.Substring(ArchiveCollection.Instance.Archives[datum.ArchiveIndex].FileName.LastIndexOf("\\") + 1);
            this.lblCompressedSize.Text = fileEntry.CompressedSize.ToString();
            this.lblDecompressedSize.Text = fileEntry.DecompressedSize.ToString();
            this.lblOffset.Text = fileEntry.DataOffset.ToString();
            this.lblFileType.Text = fileEntry.FileType.ToString();

            // Check if there is a resource parser for this game resource.
            if (GameResource.ResourceParsers.ContainsKey(fileEntry.FileType) == true)
            {
                // Parse the game resource using the parser type.
                GameResource resource = ArchiveCollection.Instance.Archives[datum.ArchiveIndex].GetFileAsResource<GameResource>(datum.FileIndex);
                if (resource != null)
                {
                    // Loop through all of the resource editors and see if we have one that supports this resource type.
                    for (int i = 0; i < this.resourceEditors.Count; i++)
                    {
                        // Check if this editor supports this resource type.
                        if (this.resourceEditors[i].CanEditResource(fileEntry.FileType) == true)
                        {
                            // Update the resource being edited and make the editor visible.
                            this.resourceEditors[i].UpdateResource(ArchiveCollection.Instance.Archives[datum.ArchiveIndex], resource);
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
            else
            {
                // Loop through all of the resource editors and hide all of them.
                for (int i = 0; i < this.resourceEditors.Count; i++)
                    this.resourceEditors[i].Visible = false;
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
                ArchiveCollection.Instance.Archives[datum.ArchiveIndex].ExtractFile(datum.FileIndex, sfd.FileName);

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
            SetTreeViewSortOrder(TreeNodeOrder.ArchiveName);
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
            TreeNodeCollection treeNodes = ArchiveCollection.Instance.BuildTreeNodeArray(order);
            this.treeView1.Nodes.AddRange(treeNodes.Cast<TreeNode>().ToArray());

            // Sort the tree view.
            this.treeView1.Sort();

            // Resume the layout and enable the form.
            this.treeView1.ResumeLayout();
            this.Enabled = true;
        }

#endregion

#region TreeView ToolStrip

        private void renderToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // If this is a single file get the datum and render it solo.
            if (this.treeView1.SelectedNode.Tag != null)
            {
                // Get the file datum from the tree view node.
                DatumIndex selectedNode = (DatumIndex)(this.treeView1.SelectedNode != null ? this.treeView1.SelectedNode.Tag : null);

                // Create a new render window.
                RenderView render = new RenderView(selectedNode);

                // TODO: Figure out what the fuck is up with this...
                try
                {
                    render.Visible = true;
                }
                catch (ObjectDisposedException ex)
                {
                }
            }
            else
            {
                // Recursively build the list of all model files in this node.
                DatumIndex[] modelDatums = BuildModelListFromTreeNode(treeView1.SelectedNode);

                // Make sure we found at least 1 datum to render.
                if (modelDatums.Length > 0)
                {
                    // Create the render window.
                    RenderView render = new RenderView(modelDatums);

                    // TODO: Figure out what the fuck is up with this...
                    try
                    {
                        render.Visible = true;
                    }
                    catch (ObjectDisposedException ex)
                    {
                    }
                }
            }
        }

        private DatumIndex[] BuildModelListFromTreeNode(TreeNode node)
        {
            // List of model files found.
            List<DatumIndex> modelDatums = new List<DatumIndex>();

            // Loop through all of the child nodess.
            foreach (TreeNode childNode in node.Nodes)
            {
                // If this node has no children check it for the .rModel extension.
                if (childNode.Nodes.Count == 0 && childNode.Text.EndsWith(".rModel") == true)
                {
                    // Skip the skybox for now.
                    if (childNode.Text.Contains("sky") == true)
                        continue;

                    // Add the datum to the list.
                    modelDatums.Add((DatumIndex)childNode.Tag);
                }
                else if (childNode.Nodes.Count > 0)
                {
                    // Recursively search all children.
                    modelDatums.AddRange(BuildModelListFromTreeNode(childNode));
                }
            }

            // Return the list of datums.
            return modelDatums.ToArray();
        }

#endregion

#region DEBUG Menu

        private void texturesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Browse for a folder to save all the textures to.
            FolderBrowserDialog fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                // Loop through every single arc file and extract all bitmaps.
                for (int i = 0; i < ArchiveCollection.Instance.Archives.Length; i++)
                {
                    // Loop through all of the files in the arc file and save textures.
                    for (int x = 0; x < ArchiveCollection.Instance.Archives[i].FileEntries.Length; x++)
                    {
                        // Check if this is a texture.
                        if (ArchiveCollection.Instance.Archives[i].FileEntries[x].FileType != ResourceType.rTexture)
                            continue;

                        // Parse the game resource.
                        rTexture texture = ArchiveCollection.Instance.Archives[i].GetFileAsResource<rTexture>(x);

                        // Convert to a DDS image.
                        DDSImage ddsImage = DDSImage.FromGameTexture(texture);

                        // Get the name of the bitmap.
                        string fileName = System.IO.Path.GetFileName(texture.FileName);
                        fileName = fileName.Substring(0, fileName.LastIndexOf('.'));

                        // Write to file.
                        ddsImage.WriteToFile(string.Format("{0}\\{1}.dds", fbd.SelectedPath, fileName));
                    }
                }

                // Done.
                MessageBox.Show("Done!");
            }
        }

        private void restoreBackupsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Disable the main form.
            this.Enabled = false;

            // Loop through the list of loaded arc files.
            for (int i = 0; i < ArchiveCollection.Instance.Archives.Length; i++)
            {
                // Check if a backup file exists for this arc file.
                if (File.Exists(ArchiveCollection.Instance.Archives[i].FileName + "_bak") == true)
                {
                    // Delete the arc file.
                    File.Delete(ArchiveCollection.Instance.Archives[i].FileName);

                    // Rename the backup file.
                    File.Move(ArchiveCollection.Instance.Archives[i].FileName + "_bak", ArchiveCollection.Instance.Archives[i].FileName);
                }
            }

            // Done, restart the application so we can reload the arc collection.
            MessageBox.Show("Done!");
            Application.Restart();
        }

#endregion

#region IResourceEditorOwner

        public void SetUIState(bool enabled)
        {
            // Set the UI state.
            this.Enabled = enabled;
        }

        public DatumIndex[] GetDatumsToUpdateForResource(string fileName)
        {
            // Check if we should overwrite all duplicated or not.
            if (Properties.Settings.Default.OverwriteAllDuplicates == false)
            {
                // Only return the datum index for the selected file.
                return new DatumIndex[] { GetSelectedResourceDatum() };
            }

            // Return the datums for all copies.
            return ArchiveCollection.Instance.GetDatumsForFileName(fileName);
        }

#endregion
    }
}
