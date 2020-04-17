﻿using DeadRisingArcTool.Controls;
using DeadRisingArcTool.FileFormats.Archive;
using DeadRisingArcTool.FileFormats.Bitmaps;
using DeadRisingArcTool.FileFormats.Geometry;
using DeadRisingArcTool.Forms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeadRisingArcTool
{
    public partial class MainForm : Form
    {
        // Background worker for async operations.
        BackgroundWorker asyncWorker = null;

        // Loading dialog used while loading the arc folder.
        LoadingDialog loadingDialog = null;

        // List of currently loaded arc files.
        ArcFileCollection arcFileCollection = null;

        // Specialized editors.
        BitmapViewer bitmapViewer = new BitmapViewer();
        ModelViewer modelViewer = new ModelViewer();

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            // Set the default editor position.
            Point defaultPosition = new Point(this.FileInfoBox.Location.X, this.FileInfoBox.Location.Y + this.FileInfoBox.Size.Height + 20);

            // Set the default anchor points.
            AnchorStyles anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;

            // Setup the bitmap viewer.
            this.bitmapViewer.Visible = false;
            this.bitmapViewer.Location = defaultPosition;
            this.bitmapViewer.Anchor = anchor;
            //this.bitmapViewer.Size = new Size(this.FileInfoBox.Width, this.bitmapViewer.Height);

            // Setup the model viewer.
            this.modelViewer.Visible = false;
            this.modelViewer.Location = defaultPosition;
            this.modelViewer.Anchor = anchor;
            this.modelViewer.Size = new Size(this.splitContainer1.Panel2.Width, this.splitContainer1.Panel2.Height - defaultPosition.Y);

            // Add all of the specialized editors to the editor panel.
            this.splitContainer1.Panel2.Controls.AddRange(new Control[]
                {
                    this.bitmapViewer,
                    this.modelViewer
                });
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

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            // Set all the viewer controls invisible.
            SetViewControlsInvisible();

            // Check if the selected node is valid and has a the tag property set.
            if (e.Node == null || e.Node.Tag == null)
            {
                // Clear properties view.
                this.lblArcFile.Text = "";
                this.lblCompressedSize.Text = "";
                this.lblDecompressedSize.Text = "";
                this.lblOffset.Text = "";
                this.lblFileType.Text = "";

                // Do options for this node.
                this.treeViewContextMenu.Enabled = false;
                return;
            }

            // Get the datum index for the arc file entry.
            DatumIndex datum = (DatumIndex)((int)e.Node.Tag);
            ArcFileEntry fileEntry = this.arcFileCollection.ArcFiles[datum.ArcIndex].FileEntries[datum.FileIndex];

            // Update the properties view.
            this.lblArcFile.Text = this.arcFileCollection.ArcFiles[datum.ArcIndex].FileName.Substring(this.arcFileCollection.ArcFiles[datum.ArcIndex].FileName.LastIndexOf("\\") + 1);
            this.lblCompressedSize.Text = fileEntry.CompressedSize.ToString();
            this.lblDecompressedSize.Text = fileEntry.DecompressedSize.ToString();
            this.lblOffset.Text = fileEntry.DataOffset.ToString();
            this.lblFileType.Text = fileEntry.FileType.ToString("X");

            // Decompress the file entry so we can load specialied editors for it.
            byte[] decompressedData = this.arcFileCollection.ArcFiles[datum.ArcIndex].DecompressFileEntry(datum.FileIndex);
            if (decompressedData != null)
            {
                // Determine the resource type from the decompressed data.
                ResourceType resourceType = ArcFile.DetermineResouceTypeFromBuffer(decompressedData);

                // Load specialized editors for the resource type.
                switch (resourceType)
                {
                    case ResourceType.Texture:
                        {
                            // Parse the texture and check it is valid.
                            rTexture texture = rTexture.FromBuffer(decompressedData);
                            if (texture == null)
                            {
                                // Failed to load texture data.
                                break;
                            }

                            // Update the bitmap viewer with the new image to display and make it visible.
                            this.bitmapViewer.Bitmap = texture;
                            this.bitmapViewer.Visible = true;
                            break;
                        }
                    case ResourceType.Model:
                        {
                            // Parse the model and check it is valid.
                            rModel model = rModel.FromBuffer(decompressedData);
                            if (model == null)
                            {
                                // Failed to load the model data.
                                break;
                            }

                            // Update the model viewer with the model and make it visible.
                            this.modelViewer.arcFile = this.arcFileCollection.ArcFiles[datum.ArcIndex];
                            this.modelViewer.Model = model;
                            this.modelViewer.Visible = true;
                            break;
                        }
                    case ResourceType.Invalid:
                        {
                            break;
                        }
                }
            }

            // Enable context menu options.
            this.treeViewContextMenu.Enabled = true;
        }

        private void SetViewControlsInvisible()
        {
            // Set all the viewer controls to be invisible.
            this.bitmapViewer.Visible = false;
            this.modelViewer.Visible = false;
        }

        private void extractToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Check if the selected node is valid and has a the tag property set.
            if (this.treeView1.SelectedNode == null || this.treeView1.SelectedNode.Tag == null)
            {
                return;
            }

            // Get the datum index for the arc file entry.
            DatumIndex datum = (DatumIndex)((int)this.treeView1.SelectedNode.Tag);

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
    }
}
