using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeadRisingArcTool.FileFormats.Archive
{
    #region DatumIndex

    /// <summary>
    /// Handle tracking for individual arc files.
    /// </summary>
    public struct DatumIndex
    {
        /// <summary>
        /// Index of the archive in the ArcFileCollection.
        /// </summary>
        public short ArcIndex { get; set; }
        /// <summary>
        /// Index of the file in the ArcFile file collection.
        /// </summary>
        public short FileIndex { get; set; }

        public DatumIndex(short arcIndex, short fileIndex)
        {
            this.ArcIndex = arcIndex;
            this.FileIndex = fileIndex;
        }

        public DatumIndex(int datum)
        {
            this.ArcIndex = (short)((datum >> 16) & 0xFFFF);
            this.FileIndex = (short)(datum & 0xFFFF);
        }

        public int ToInt32()
        {
            return (int)((((int)this.ArcIndex << 16) & 0xFFFF0000) | (this.FileIndex & 0xFFFF));
        }

        public static explicit operator DatumIndex(int datum)
        {
            return new DatumIndex(datum);
        }

        public static int DatumFromIndices(short arcIndex, short fileIndex)
        {
            return (int)((((int)arcIndex << 16) & 0xFFFF0000) | (fileIndex & 0xFFFF));
        }
    }

    #endregion

    public enum TreeNodeOrder
    {
        /// <summary>
        /// Build the tree view based on arc file folder structure
        /// </summary>
        FolderPath,
        /// <summary>
        /// Build the tree view based on arc file name
        /// </summary>
        ArcFile,
        /// <summary>
        /// Build the tree view based on the resource type of the arc files
        /// </summary>
        ResourceType
    }

    public class ArcFileCollection
    {
        private List<ArcFile> arcFiles = new List<ArcFile>();
        /// <summary>
        /// List of arc files that have been loaded into the collection.
        /// </summary>
        public ArcFile[] ArcFiles { get { return this.arcFiles.ToArray(); } }

        // Dictionary of arc file datums to file names.
        private Dictionary<DatumIndex, string> arcFileNameDictionary = new Dictionary<DatumIndex, string>();

        /// <summary>
        /// Root directory the arc files were loaded from.
        /// </summary>
        public string RootDirectory { get; private set; }

        public ArcFileCollection(string rootFolder)
        {
            // Initialize fields.
            this.RootDirectory = rootFolder;
        }

        public void AddArcFile(ArcFile arcFile)
        {
            // Add the arc file to the collection.
            int arcIndex = this.arcFiles.Count;
            this.arcFiles.Add(arcFile);

            // Loop through the arc file entry list and create a datum for each file.
            for (int i = 0; i < this.arcFiles[arcIndex].FileEntries.Length; i++)
            {
                // Create a datum for the file entry and add it to the dictionary.
                this.arcFileNameDictionary.Add(new DatumIndex((short)arcIndex, (short)i), this.arcFiles[arcIndex].FileEntries[i].FileName);
            }
        }

        public void GetArcFileEntryFromDatum(DatumIndex datum, out ArcFile arcFile, out ArcFileEntry fileEntry)
        {
            // Return the arc file and file entry.
            arcFile = this.arcFiles[datum.ArcIndex];
            fileEntry = arcFile.FileEntries[datum.FileIndex];
        }

        #region TreeNode Utilities

        public TreeNodeCollection BuildTreeNodeArray(TreeNodeOrder order)
        {
            // Check the order type and handle accordingly.
            if (order == TreeNodeOrder.FolderPath)
                return BuildTreeNodeByFolderPath();
            else if (order == TreeNodeOrder.ArcFile)
                return BuildTreeNodeByArcFile();
            else
                return BuildTreeNodeByFileType();
        }

        private TreeNodeCollection BuildTreeNodeByFolderPath()
        {
            // Tree node collection we add to.
            TreeNode root = new TreeNode();

            // Loop through all of the file names in the arc file name dictionary and add each one to the tree node collection.
            foreach (DatumIndex datum in this.arcFileNameDictionary.Keys)
            {
                // Add the file path as tree nodes
                string fileName = this.arcFiles[datum.ArcIndex].FileEntries[datum.FileIndex].FileName + "." + 
                    this.arcFiles[datum.ArcIndex].FileEntries[datum.FileIndex].FileType.ToString();
                AddFilePathAsTreeNodes(root, fileName, datum);
            }

            // Return the tree node collection.
            return root.Nodes;
        }

        private TreeNodeCollection BuildTreeNodeByArcFile()
        {
            // Tree node collection we add to.
            TreeNode root = new TreeNode();

            // Loop through all of the arc files and create a node for each one.
            for (int i = 0; i < this.arcFiles.Count; i++)
            {
                // Create a tree node for the arc file.
                TreeNode arcNode = new TreeNode(this.arcFiles[i].FileName.Substring(this.arcFiles[i].FileName.LastIndexOf("\\") + 1));
                arcNode.Name = arcNode.Text;
                root.Nodes.Add(arcNode);

                // Loop through all the files in the arc file and add them to the arc node.
                for (int x = 0; x < this.arcFiles[i].FileEntries.Length; x++)
                {
                    // Add the file path as tree nodes
                    string fileName = this.arcFiles[i].FileEntries[x].FileName + "." + this.arcFiles[i].FileEntries[x].FileType.ToString();
                    AddFilePathAsTreeNodes(arcNode, fileName, new DatumIndex((short)i, (short)x));
                }
            }

            // Return the tree node collection.
            return root.Nodes;
        }

        private TreeNodeCollection BuildTreeNodeByFileType()
        {
            // Root tree node collection we will add to.
            TreeNode root = new TreeNode();

            // Loop through all of the arc files in the collection.
            for (int i = 0; i < this.arcFiles.Count; i++)
            {
                // Loop through all the files in the current arc.
                for (int x = 0; x < this.arcFiles[i].FileEntries.Length; x++)
                {
                    TreeNode previousNode = null;

                    // Check if the root node already has a tree node for this resource type.
                    TreeNode[] nodesFound = root.Nodes.Find(this.arcFiles[i].FileEntries[x].FileType.ToString(), false);
                    if (nodesFound.Length > 0)
                    {
                        // Set the previous node to the resource type node.
                        previousNode = nodesFound[0];
                    }
                    else
                    {
                        // Create a new tree node for this resource type.
                        previousNode = new TreeNode(this.arcFiles[i].FileEntries[x].FileType.ToString());
                        previousNode.Name = this.arcFiles[i].FileEntries[x].FileType.ToString();

                        // Add the node to the root node.
                        root.Nodes.Add(previousNode);
                    }

                    // Add the file path as tree nodes to the resource type node.
                    string fileName = this.arcFiles[i].FileEntries[x].FileName + "." + this.arcFiles[i].FileEntries[x].FileType.ToString();
                    AddFilePathAsTreeNodes(previousNode, fileName, new DatumIndex((short)i, (short)x));
                }
            }

            // Return the tree node collection.
            return root.Nodes;
        }

        private void AddFilePathAsTreeNodes(TreeNode root, string filePath, object tag)
        {
            // Split the file name.
            string[] pieces = filePath.Split(new string[] { "\\" }, StringSplitOptions.None);

            // Loop through all of the pieces and add each one to the tree node.
            TreeNode previousNode = root;
            for (int z = 0; z < pieces.Length; z++)
            {
                // Check if the previous node has an existing node for the current folder.
                TreeNode[] nodesFound = previousNode.Nodes.Find(pieces[z], false);
                if (nodesFound.Length > 0)
                {
                    // Set the new previous node and continue.
                    previousNode = nodesFound[0];
                }
                else
                {
                    // Create a new tree node.
                    TreeNode newNode = new TreeNode(pieces[z]);
                    newNode.Name = pieces[z];

                    // Check if we need to set the tag property.
                    if (z == pieces.Length - 1)
                    {
                        // Set the tag to the datum of the file entry.
                        newNode.Tag = tag;
                    }

                    // Add the node to the parent node's collection.
                    previousNode.Nodes.Add(newNode);
                    previousNode = newNode;
                }
            }
        }

        #endregion
    }
}
