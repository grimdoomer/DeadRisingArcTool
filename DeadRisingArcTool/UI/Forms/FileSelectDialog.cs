using DeadRisingArcTool.FileFormats;
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
    public partial class FileSelectDialog : Form
    {
        private Tuple<string, bool>[] fileNames;

        /// <summary>
        /// List of selected file names
        /// </summary>
        public string[] SelectedFiles { get; private set; }

        public FileSelectDialog(Tuple<string, bool>[] fileNames)
        {
            InitializeComponent();

            // Initialize fields.
            this.fileNames = fileNames;

            // Set the image list.
            this.treeView1.ImageList = IconSet.Instance.IconImageList;
        }

        private void FileSelectDialog_Load(object sender, EventArgs e)
        {
            // Suspend the tree view layout while we build the tree node collection.
            this.treeView1.SuspendLayout();

            // List of tree nodes to be expanded.
            List<TreeNode> nodesToExpand = new List<TreeNode>();

            // Loop through all the file names to display and create tree nodes for each.
            TreeNode rootNode = new TreeNode();
            for (int i = 0; i < this.fileNames.Length; i++)
            {
                // Split the file path into individual folder names.
                string[] pieces = this.fileNames[i].Item1.Split('\\');

                // Loop through all of the folder pieces and add each one to the tree view.
                TreeNode parentNode = rootNode;
                for (int x = 0; x < pieces.Length; x++)
                {
                    // Check if the parent node has a child node with the same name.
                    TreeNode[] nodesFound = parentNode.Nodes.Find(pieces[x], false);
                    if (nodesFound.Length > 0)
                    {
                        // Set the parent node to the node found.
                        parentNode = nodesFound[0];
                    }
                    else
                    {
                        // Create a new child node for the folder.
                        TreeNode child = new TreeNode(pieces[x]);
                        child.Name = pieces[x];

                        // Set the image index accordingly.
                        if (x == pieces.Length - 1)
                        {
                            // Get the file extension from the file name.
                            string fileExtension = pieces[x].Substring(pieces[x].LastIndexOf('.') + 1);
                            ResourceType resType = (ResourceType)Enum.Parse(typeof(ResourceType), fileExtension);

                            // Set the image index accordingly.
                            child.ImageIndex = (int)IconSet.UIIconFromResourceType(resType);

                            // Check the node.
                            child.Checked = this.fileNames[i].Item2;
                            if (child.Checked == true)
                            {
                                // Set the parent node to be expanded.
                                nodesToExpand.Add(parentNode);
                            }
                        }
                        else
                        {
                            // Set the image index to the folder icon.
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

            // Add the nodes to the tree view.
            for (int i = 0; i < rootNode.Nodes.Count; i++)
                this.treeView1.Nodes.Add(rootNode.Nodes[i]);

            // Loop through all the nodes to be expanded and expand them recursively.
            for (int i = 0; i < nodesToExpand.Count; i++)
            {
                // Expand the node recursively.
                TreeNode child = nodesToExpand[i];
                do
                {
                    child.Expand();
                    child = child.Parent;
                } while (child != null);
            }

            // Sort the node collection.
            this.treeView1.Sort();

            // Resume the tree view layout.
            this.treeView1.ResumeLayout(false);
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            // Suspend the tree view layout.
            this.treeView1.SuspendLayout();

            // Loop through all the nodes in the tree view and all file nodes.
            for (int i = 0; i < this.treeView1.Nodes.Count; i++)
            {
                // Set the file nodes to be checked.
                SetFileNodeCheckedState(this.treeView1.Nodes[i], true);
            }

            // Resume the tree view layout.
            this.treeView1.ResumeLayout(true);
        }

        private void SetFileNodeCheckedState(TreeNode node, bool @checked)
        {
            // Check if this node is a file node.
            if (node.Nodes.Count == 0)
            {
                // Set the checked state and return.
                node.Checked = @checked;
                return;
            }

            // Loop through all child nodes and set their checked state.
            for (int i = 0; i < node.Nodes.Count; i++)
                SetFileNodeCheckedState(node.Nodes[i], @checked);
        }

        private void btnClearAll_Click(object sender, EventArgs e)
        {
            // Suspend the tree view layout.
            this.treeView1.SuspendLayout();

            // Loop through all the nodes in the tree view and all file nodes.
            for (int i = 0; i < this.treeView1.Nodes.Count; i++)
            {
                // Set the file nodes to be unchecked.
                SetFileNodeCheckedState(this.treeView1.Nodes[i], false);
            }

            // Resume the tree view layout.
            this.treeView1.ResumeLayout(true);
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            // Set the dialog result and close.
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void btnSelectFiles_Click(object sender, EventArgs e)
        {
            // Build the list of selected file nodes.
            this.SelectedFiles = GetSelectedNodePaths();

            // Set the dialog result and close.
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private string[] GetSelectedNodePaths(TreeNode node = null)
        {
            // Create a list to hold the node names.
            List<string> selectedNodes = new List<string>();

            // Check if a parent node was provided or not.
            if (node == null)
            {
                // Loop through all of the nodes in the tree view and search for selected ones.
                for (int i = 0; i < this.treeView1.Nodes.Count; i++)
                {
                    // Find selected nodes recursively.
                    selectedNodes.AddRange(GetSelectedNodePaths(this.treeView1.Nodes[i]));
                }
            }
            else
            {
                // Check if this node has child nodes or not.
                if (node.Nodes.Count == 0)
                {
                    // If the node is checked return the full path.
                    if (node.Checked == true)
                        return new string[] { node.FullPath };
                    else
                        return new string[0];
                }
                else
                {
                    // Loop through all child nodes.
                    for (int i = 0; i < node.Nodes.Count; i++)
                    {
                        // Find selected nodes recursively.
                        selectedNodes.AddRange(GetSelectedNodePaths(node.Nodes[i]));
                    }
                }
            }

            // Return the list of selected nodes.
            return selectedNodes.ToArray();
        }
    }
}
