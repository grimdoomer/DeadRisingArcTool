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
    /// Handle tracking for individual files in an archive.
    /// </summary>
    public struct DatumIndex
    {
        /// <summary>
        /// Value used for resources that are not assigned to a file.
        /// </summary>
        public const int Unassigned = -1;

        /// <summary>
        /// Index of the archive in the ArcFileCollection.
        /// </summary>
        public short ArchiveIndex { get; set; }
        /// <summary>
        /// Index of the file in the Archive's file collection.
        /// </summary>
        public short FileIndex { get; set; }

        public DatumIndex(short arcIndex, short fileIndex)
        {
            this.ArchiveIndex = arcIndex;
            this.FileIndex = fileIndex;
        }

        public DatumIndex(int datum)
        {
            this.ArchiveIndex = (short)((datum >> 16) & 0xFFFF);
            this.FileIndex = (short)(datum & 0xFFFF);
        }

        public int ToInt32()
        {
            return (int)((((int)this.ArchiveIndex << 16) & 0xFFFF0000) | (this.FileIndex & 0xFFFF));
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
        /// Build the tree view based on the archive's folder structure
        /// </summary>
        FolderPath,
        /// <summary>
        /// Build the tree view based on archive name
        /// </summary>
        ArchiveName,
        /// <summary>
        /// Build the tree view based on the resource type of the archive files
        /// </summary>
        ResourceType
    }

    public enum TreeNodeIcon : int
    {
        File,
        Folder,
        FolderBlue,
        Model,
        Texture
    }

    public class ArchiveCollection
    {
        public static ArchiveCollection Instance { get; set; } = null;

        // List of file paths for the currently loaded archives.
        private List<string> loadedArchives = new List<string>();

        private List<Archive> archives = new List<Archive>();
        /// <summary>
        /// List of archives that have been loaded into the collection.
        /// </summary>
        public Archive[] Archives { get { return this.archives.ToArray(); } }

        /// <summary>
        /// List of images for the tree nodes.
        /// </summary>
        public ImageList TreeNodeImages { get; private set; } = new ImageList();

        // Dictionary of archive file datums to file names.
        private Dictionary<DatumIndex, string> archiveFileNameDictionary = new Dictionary<DatumIndex, string>();

        // Dictionary of archive file names to datums for reverse lookup.
        private Dictionary<string, DatumIndex[]> archiveFileNameReverseDictionary = new Dictionary<string, DatumIndex[]>();

        // Dictionary of patch archive file names to datums for reverse lookup.
        private Dictionary<string, DatumIndex[]> patchArchiveFileNameReverseDictionary = new Dictionary<string, DatumIndex[]>();

        /// <summary>
        /// Root directory the archive files were loaded from.
        /// </summary>
        public string RootDirectory { get; private set; }

        public ArchiveCollection(string rootFolder)
        {
            // Initialize fields.
            this.RootDirectory = rootFolder;

            // Build the tree node images list (must be added in the same order as TreeNodeIcon).
            this.TreeNodeImages.Images.Add(Properties.Resources.File);
            this.TreeNodeImages.Images.Add(Properties.Resources.Folder);
            this.TreeNodeImages.Images.Add(Properties.Resources.FolderBlue);
            this.TreeNodeImages.Images.Add(Properties.Resources.ObjectIcon);
            this.TreeNodeImages.Images.Add(Properties.Resources.Texture);

            // Set additional image list properties.
            this.TreeNodeImages.ColorDepth = ColorDepth.Depth32Bit;
            this.TreeNodeImages.ImageSize = new System.Drawing.Size(18, 18);
        }

        public bool AddArchive(string filePath, bool bIsPatchArchive = false)
        {
            // Make sure this archive has not already been loade.
            if (this.loadedArchives.Contains(filePath) == true)
                return true;

            // Create a new archive file and parse the file table.
            Archive archive = new Archive(filePath, bIsPatchArchive);
            if (archive.OpenAndRead() == false)
            {
                // Failed to read the archive.
                return false;
            }

            // Add the archive to the collection.
            int arcIndex = this.archives.Count;
            archive.Index = arcIndex;   // HACK: I hate this but it will have to do for now, one day convert to an ID
            this.archives.Add(archive);
            this.loadedArchives.Add(filePath);

            // Loop through the archive file entry list and create a datum for each file.
            for (int i = 0; i < this.archives[arcIndex].FileEntries.Length; i++)
            {
                // Create a datum for the file entry and add it to the dictionary.
                DatumIndex datum = new DatumIndex((short)arcIndex, (short)i);
                this.archiveFileNameDictionary.Add(datum, this.archives[arcIndex].FileEntries[i].FileName);

                // Pick the right dictionary to add the entry to based on if this is a patch archive or not.
                Dictionary<string, DatumIndex[]> dict = bIsPatchArchive == true ? this.patchArchiveFileNameReverseDictionary : this.archiveFileNameReverseDictionary;

                // TODO: Handle patch file load order.

                // Add the file name to the reverse lookup dictionary.
                if (dict.ContainsKey(this.archives[arcIndex].FileEntries[i].FileName) == false)
                {
                    // Add to the reverse lookup dictionary.
                    dict.Add(this.archives[arcIndex].FileEntries[i].FileName, new DatumIndex[] { datum });
                }
                else
                {
                    // Allocate a new array that is 1 element larger than the existing array (gross).
                    string fileName = this.archives[arcIndex].FileEntries[i].FileName;
                    DatumIndex[] datumArray = new DatumIndex[dict[fileName].Length + 1];
                    Array.Copy(dict[fileName], datumArray, dict[fileName].Length);
                    
                    // Add the new datum and set the array back to the revese lookup dictionary.
                    datumArray[dict[fileName].Length] = datum;
                    dict[fileName] = datumArray;
                }
            }

            // Successfully loaded the archive.
            return true;
        }

        public void GetArchiveFileEntryFromDatum(DatumIndex datum, out Archive arcFile, out ArchiveFileEntry fileEntry)
        {
            // Return the archive and file entry.
            arcFile = this.archives[datum.ArchiveIndex];
            fileEntry = arcFile.FileEntries[datum.FileIndex];
        }

        /// <summary>
        /// Checks if there is an archive containing the specified file name
        /// </summary>
        /// <param name="fileName">Full file name including file extension of the file to search for</param>
        /// <param name="arcFile">The <see cref="Archive"/> the file was found in</param>
        /// <param name="fileEntry">The <see cref="ArchiveFileEntry"/> for the file</param>
        public void GetArchiveFileEntryFromFileName(string fileName, out Archive arcFile, out ArchiveFileEntry fileEntry)
        {
            // Satisfy the compiler.
            arcFile = null;
            fileEntry = null;

            // Check if we have a patch file entry for this file name.
            if (this.patchArchiveFileNameReverseDictionary.ContainsKey(fileName) == true)
            {
                // Load the file info from the patch archive.
                DatumIndex patchDatum = this.patchArchiveFileNameReverseDictionary[fileName][0];
                arcFile = this.archives[patchDatum.ArchiveIndex];
                fileEntry = arcFile.FileEntries[patchDatum.FileIndex];
                return;
            }

            // Check if we have an entry for this file name.
            if (this.archiveFileNameReverseDictionary.ContainsKey(fileName) == false)
            {
                // No matching file name found.
                return;
            }

            // Get the datum for this file entry.
            // TODO: One day, in the maybe not so distant future, we will need to handle loading from
            // a specific archive.
            DatumIndex datum = this.archiveFileNameReverseDictionary[fileName][0];
            arcFile = this.archives[datum.ArchiveIndex];
            fileEntry = arcFile.FileEntries[datum.FileIndex];
        }

        /// <summary>
        /// Gets a list of all the <see cref="DatumIndex"/>'s for the specified file name. Used to find
        /// datums for duplicate files.
        /// </summary>
        /// <param name="fileName">Name of the file</param>
        /// <returns>A <see cref="DatumIndex"/> array</returns>
        public DatumIndex[] GetDatumsForFileName(string fileName)
        {
            // Check if there are patches loaded for this file.
            if (this.patchArchiveFileNameReverseDictionary.ContainsKey(fileName) == true)
            {
                // Return the list of patch file datums.
                return this.patchArchiveFileNameReverseDictionary[fileName];
            }

            // Check if this file name is present in the reverse lookup dictionary.
            if (this.archiveFileNameReverseDictionary.ContainsKey(fileName) == true)
            {
                // Return the list of datums for this file name.
                return this.archiveFileNameReverseDictionary[fileName];
            }

            // Reverse lookup dictionary does not contain this file name.
            return new DatumIndex[0];
        }

        #region TreeNode Utilities

        public TreeNodeCollection BuildTreeNodeArray(TreeNodeOrder order)
        {
            // Check the order type and handle accordingly.
            if (order == TreeNodeOrder.FolderPath)
                return BuildTreeNodeByFolderPath();
            else if (order == TreeNodeOrder.ArchiveName)
                return BuildTreeNodeByArchive();
            else
                return BuildTreeNodeByFileType();
        }

        private TreeNodeCollection BuildTreeNodeByFolderPath()
        {
            // Tree node collection we add to.
            TreeNode root = new TreeNode();

            // Loop through all of the file names in the file name dictionary and add each one to the tree node collection.
            foreach (DatumIndex datum in this.archiveFileNameDictionary.Keys)
            {
                // Add the file path as tree nodes
                AddFilePathAsTreeNodes(root, this.archives[datum.ArchiveIndex].FileEntries[datum.FileIndex].FileName,
                    this.archives[datum.ArchiveIndex].FileEntries[datum.FileIndex].FileType, datum, this.archives[datum.ArchiveIndex].IsPatchFile);
            }

            // Return the tree node collection.
            return root.Nodes;
        }

        private TreeNodeCollection BuildTreeNodeByArchive()
        {
            // Tree node collection we add to.
            TreeNode root = new TreeNode();

            // Loop through all of the archives and create a node for each one.
            for (int i = 0; i < this.archives.Count; i++)
            {
                // Create a tree node for the archive.
                TreeNode arcNode = new TreeNode(this.archives[i].FileName.Substring(this.archives[i].FileName.LastIndexOf("\\") + 1));
                arcNode.Name = arcNode.Text;
                root.Nodes.Add(arcNode);

                // If this archive is a patch file color the node blue.
                if (this.archives[i].IsPatchFile == true)
                    arcNode.ForeColor = System.Drawing.Color.Blue;

                // Loop through all the files in the archive and add them to the arc node.
                for (int x = 0; x < this.archives[i].FileEntries.Length; x++)
                {
                    // Add the file path as tree nodes
                    AddFilePathAsTreeNodes(arcNode, this.archives[i].FileEntries[x].FileName, 
                        this.archives[i].FileEntries[x].FileType, new DatumIndex((short)i, (short)x), this.archives[i].IsPatchFile);
                }
            }

            // Return the tree node collection.
            return root.Nodes;
        }

        private TreeNodeCollection BuildTreeNodeByFileType()
        {
            // Root tree node collection we will add to.
            TreeNode root = new TreeNode();

            // Loop through all of the archives in the collection.
            for (int i = 0; i < this.archives.Count; i++)
            {
                // Loop through all the files in the current arc.
                for (int x = 0; x < this.archives[i].FileEntries.Length; x++)
                {
                    TreeNode previousNode = null;

                    // Check if the root node already has a tree node for this resource type.
                    TreeNode[] nodesFound = root.Nodes.Find(this.archives[i].FileEntries[x].FileType.ToString(), false);
                    if (nodesFound.Length > 0)
                    {
                        // Set the previous node to the resource type node.
                        previousNode = nodesFound[0];
                    }
                    else
                    {
                        // Create a new tree node for this resource type.
                        previousNode = new TreeNode(this.archives[i].FileEntries[x].FileType.ToString());
                        previousNode.Name = this.archives[i].FileEntries[x].FileType.ToString();

                        // Add the node to the root node.
                        root.Nodes.Add(previousNode);
                    }

                    // Add the file path as tree nodes to the resource type node.
                    AddFilePathAsTreeNodes(previousNode, this.archives[i].FileEntries[x].FileName, 
                        this.archives[i].FileEntries[x].FileType, new DatumIndex((short)i, (short)x), this.archives[i].IsPatchFile);
                }
            }

            // Return the tree node collection.
            return root.Nodes;
        }

        private void AddFilePathAsTreeNodes(TreeNode root, string filePath, ResourceType fileType, object tag, bool isPatchFile)
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
                    newNode.ImageIndex = (int)(isPatchFile == true ? TreeNodeIcon.FolderBlue : TreeNodeIcon.Folder);

                    // Check if we need to set additional properties.
                    if (z == pieces.Length - 1)
                    {
                        // Set the tag to the datum of the file entry.
                        newNode.Tag = tag;

                        // Color the node accordingly.
                        if (isPatchFile == true)
                        {
                            // Patch file, set the node color to blue.
                            newNode.ForeColor = System.Drawing.Color.Blue;
                        }
                        else if (GameResource.ResourceParsers.ContainsKey(fileType) == false)
                        {
                            // No resource editor for this file type, set the node color to red.
                            newNode.ForeColor = System.Drawing.Color.Red;
                        }

                        // Check the type and see if we should set an icon for this node.
                        switch (fileType)
                        {
                            case ResourceType.rModel: newNode.ImageIndex = (int)TreeNodeIcon.Model; break;
                            case ResourceType.rTexture: newNode.ImageIndex = (int)TreeNodeIcon.Texture; break;
                            default: newNode.ImageIndex = (int)TreeNodeIcon.File; break;
                        }
                    }

                    // Set the selected image index.
                    newNode.SelectedImageIndex = newNode.ImageIndex;

                    // Add the node to the parent node's collection.
                    previousNode.Nodes.Add(newNode);
                    previousNode = newNode;
                }
            }
        }

        #endregion
    }
}
