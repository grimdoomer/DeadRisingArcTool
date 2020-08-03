using DeadRisingArcTool.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeadRisingArcTool.FileFormats.Archive
{
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

    public struct TreeNodeTag
    {
        /// <summary>
        /// Indicates if this file/folder is for a patch file.
        /// </summary>
        public bool IsPatchFile;
        /// <summary>
        /// DatumIndex if this node is a file, or <see cref="DatumIndex.Unassigned"/> otherwise
        /// </summary>
        public DatumIndex Datum;
        /// <summary>
        /// Indicates if the node supports any context menu options.
        /// </summary>
        public bool SupportsMenuOperations;
        /// <summary>
        /// Indicates if the node is override by a patch file.
        /// </summary>
        public bool IsOverrided;

        public TreeNodeTag(bool isPatchFile, DatumIndex datum, bool supportsMenu = true, bool isOverrided = false)
        {
            // Initialize fields.
            this.IsPatchFile = isPatchFile;
            this.Datum = datum;
            this.SupportsMenuOperations = supportsMenu;
            this.IsOverrided = isOverrided;
        }
    }

    public class ArchiveCollection
    {
        /// <summary>
        /// Singleton instance
        /// </summary>
        public static ArchiveCollection Instance { get; set; } = null;

        // List of file paths for the currently loaded archives.
        private List<string> loadedArchives = new List<string>();

        private List<Archive> archives = new List<Archive>();
        /// <summary>
        /// List of archives that have been loaded into the collection.
        /// </summary>
        public Archive[] Archives { get { return this.archives.ToArray(); } }

        // Unique archive id counter.
        private uint nextArchiveId = 0x80000000;

        // Dictionary of archive ids and the index of the archive in the archive list.
        private Dictionary<uint, int> archiveLookupDictionary = new Dictionary<uint, int>();

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
        }

        #region Loading/Unloading

        public bool AddArchive(string filePath, bool bIsPatchArchive = false)
        {
            // Make sure this archive has not already been loade.
            if (this.loadedArchives.Contains(filePath) == true)
                return true;

            // Create a new archive file and parse the file table.
            Archive archive = new Archive(filePath, nextArchiveId++, bIsPatchArchive);
            if (archive.OpenAndRead() == false)
            {
                // Failed to read the archive.
                return false;
            }

            // Call helper routine to add the archive.
            return AddArchive(archive, filePath, bIsPatchArchive);
        }

        private bool AddArchive(Archive archive, string filePath, bool bIsPatchArchive = false)
        {
            // Add the archive to the collection.
            int arcIndex = this.archives.Count;
            this.archives.Add(archive);
            this.loadedArchives.Add(filePath);
            this.archiveLookupDictionary.Add(this.archives[arcIndex].ArchiveId, arcIndex);

            // Loop through the archive file entry list and create a datum for each file.
            for (int i = 0; i < this.archives[arcIndex].FileEntries.Length; i++)
            {
                // Create a datum for the file entry and add it to the dictionary.
                DatumIndex datum = new DatumIndex(this.archives[arcIndex].ArchiveId, this.archives[arcIndex].FileEntries[i].FileId);

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

        private void UnloadFile(DatumIndex datum)
        {
            // Make sure we have an archive with this id.
            if (this.archiveLookupDictionary.ContainsKey(datum.ArchiveId) == false)
            {
                // Archive for this file is not loaded.
                return;
            }

            // Get the index of the archive.
            int archiveIndex = this.archiveLookupDictionary[datum.ArchiveId];

            // Get the file path for this file.
            int fileIndex = this.archives[archiveIndex].FindFileFromDatum(datum);
            string filePath = this.archives[archiveIndex].FileEntries[fileIndex].FileName;

            // Remove the file id from the patch file dictionary.
            if (this.patchArchiveFileNameReverseDictionary.ContainsKey(filePath) == true)
            {
                // Make sure this datum is in the list.
                if (this.patchArchiveFileNameReverseDictionary[filePath].Contains(datum) == true)
                {
                    // Remvoe this datum from the list.
                    this.patchArchiveFileNameReverseDictionary[filePath] = this.patchArchiveFileNameReverseDictionary[filePath].Where(p => p != datum).ToArray();
                    if (this.patchArchiveFileNameReverseDictionary[filePath].Length == 0)
                    {
                        // Remove the entry from the dictionary.
                        this.patchArchiveFileNameReverseDictionary.Remove(filePath);
                    }
                }
            }

            // Remove the file id from the game files dictionary.
            if (this.archiveFileNameReverseDictionary.ContainsKey(filePath) == true)
            {
                // Make sure this datum is in the list.
                if (this.archiveFileNameReverseDictionary[filePath].Contains(datum) == true)
                {
                    // Remove the datum from the list.
                    this.archiveFileNameReverseDictionary[filePath] = this.archiveFileNameReverseDictionary[filePath].Where(p => p != datum).ToArray();
                    if (this.archiveFileNameReverseDictionary[filePath].Length == 0)
                    {
                        // Remove the entry from the dictionary.
                        this.archiveFileNameReverseDictionary.Remove(filePath);
                    }
                }
            }
        }

        public void UnloadArchive(uint archiveId)
        {
            // Make sure we have an entry for this archive in the lookup table.
            if (this.archiveLookupDictionary.ContainsKey(archiveId) == false)
            {
                // Archive not found in the lookup table.
                return;
            }

            // Loop through all of the files in the archive and unload each one from the lookup dictionaries.
            int archiveIndex = this.archiveLookupDictionary[archiveId];
            for (int i = 0; i < this.archives[archiveIndex].FileEntries.Length; i++)
            {
                // Unload the file.
                UnloadFile(new DatumIndex(archiveId, this.archives[archiveIndex].FileEntries[i].FileId));
            }

            // Remove the archive from all lookup dictionaries/lists.
            this.archives.RemoveAt(archiveIndex);
            this.loadedArchives.RemoveAt(archiveIndex);

            // Rebuild the archive lookup dictionary.
            this.archiveLookupDictionary.Clear();
            for (int i = 0; i < this.archives.Count; i++)
                this.archiveLookupDictionary.Add(this.archives[i].ArchiveId, i);
        }

        #endregion

        #region File searching/access

        /// <summary>
        /// Finds the archive and file entry for the specified file datum
        /// </summary>
        /// <param name="datum">File datum to find in the archive collection</param>
        /// <param name="arcFile">Archive instance the file is located in</param>
        /// <param name="fileEntry">File entry for the specified file</param>
        public void GetArchiveFileEntryFromDatum(DatumIndex datum, out Archive arcFile, out ArchiveFileEntry fileEntry)
        {
            // Make sure there is an entry in the reverse lookup table for this archive id.
            if (this.archiveLookupDictionary.ContainsKey(datum.ArchiveId) == false)
            {
                // No entry for this archive id found.
                arcFile = null;
                fileEntry = null;
                return;
            }

            // Get the file and archive indices from the datum.
            int archiveIndex = this.archiveLookupDictionary[datum.ArchiveId];
            int fileIndex = this.archives[archiveIndex].FindFileFromDatum(datum);
            if (fileIndex == -1)
            {
                // No file for this datum was found.
                arcFile = null;
                fileEntry = null;
                return;
            }

            // Return the archive and file entry.
            arcFile = this.archives[archiveIndex];
            fileEntry = arcFile.FileEntries[fileIndex];
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
                GetArchiveFileEntryFromDatum(patchDatum, out arcFile, out fileEntry);
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
            GetArchiveFileEntryFromDatum(datum, out arcFile, out fileEntry);
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

        /// <summary>
        ///  Parses the resource data for the file with the specified DatumIndex
        /// </summary>
        /// <typeparam name="T">Type of <see cref="GameResource"/> that will be returned</typeparam>
        /// <param name="datum">DatumIndex of the file to parse</param>
        /// <returns>The parsed game resource object, or null otherwise</returns>
        public T GetFileAsResource<T>(DatumIndex datum) where T : GameResource
        {
            // Make sure there is an entry in the reverse lookup table for this archive id.
            if (this.archiveLookupDictionary.ContainsKey(datum.ArchiveId) == false)
            {
                // No archive found for this file datum.
                return null;
            }

            // Get the file instance from the archive.
            return this.archives[this.archiveLookupDictionary[datum.ArchiveId]].GetFileAsResource<T>(datum.FileId);
        }

        #endregion

        #region File manipulation

        /// <summary>
        /// Overwrites the file contents for all DatumIndexes specified by <paramref name="fileIds"/> with <paramref name="newFileData"/>
        /// </summary>
        /// <param name="fileIds">DatumIndexes for files to be updated</param>
        /// <param name="newFileData">New file data to write</param>
        /// <returns>True if all files were updated successfully, false otherwise</returns>
        public bool InjectFile(DatumIndex[] fileIds, byte[] newFileData)
        {
            // Loop through all of the datums and update the file for each one.
            for (int i = 0; i < fileIds.Length; i++)
            {
                // Make sure we have an archive for this datum.
                if (this.archiveLookupDictionary.ContainsKey(fileIds[i].ArchiveId) == false)
                {
                    // No archive found for this datum.
                    return false;
                }

                // Update the file contents for this file.
                int archiveIndex = this.archiveLookupDictionary[fileIds[i].ArchiveId];
                if (this.archives[archiveIndex].InjectFile(fileIds[i].FileId, newFileData) == false)
                {
                    // Failed to update file contents.
                    string archiveName = this.archives[archiveIndex].FileName.Substring(this.archives[archiveIndex].FileName.LastIndexOf("\\") + 1);
                    MessageBox.Show(string.Format("Failed to write to arc file {0}!", archiveName));
                    return false;
                }
            }

            // Successfully updated all files.
            return true;
        }

        public bool DeleteFiles(DatumIndex[] fileIds, out DatumIndex[] filesDeleted)
        {
            // List of files that were successfully deleted.
            List<DatumIndex> deletedFiles = new List<DatumIndex>();

            // Create a list of archive ids we have already processed..
            List<uint> processedArchiveIds = new List<uint>();

            // Loop through all of the datums and delete each one from the archives.
            for (int i = 0; i < fileIds.Length; i++)
            {
                // Check if we have already processed this archive or not.
                if (processedArchiveIds.Contains(fileIds[i].ArchiveId) == true)
                    continue;

                // Flag that we have processed the files for this archive.
                processedArchiveIds.Add(fileIds[i].ArchiveId);

                // Get all the datums that belong to this archive so we can delete them in bulk.
                DatumIndex[] datumsForArchive = fileIds.Where(p => p.ArchiveId == fileIds[i].ArchiveId).ToArray();

                // Get the index of the archive.
                int archiveIndex = this.archiveLookupDictionary[fileIds[i].ArchiveId];

                // Check to see if we are deleting the last file in the archive.
                if (datumsForArchive.Length == this.archives[archiveIndex].FileEntries.Length)
                {
                    // Setup the dialog format string.
                    string dialogMsg = "Deleting all the files in {0} will also delete the archive, are you sure you want to continue?\r\n" +
                        "\tYes - Delete archive\r\n" +
                        "\tNo - Skip deleting files for this archive and continue\r\n" +
                        "\tCancel - Abort the delete operation, files already deleted will not be restored";

                    // Prompt the user to delete the archive outright and handle accordingly.
                    string archiveName = this.archives[archiveIndex].FileName.Substring(this.archives[archiveIndex].FileName.LastIndexOf("\\") + 1);
                    DialogResult result = MessageBox.Show(string.Format(dialogMsg, archiveName), "Delete archive", MessageBoxButtons.YesNoCancel);
                    if (result == DialogResult.Yes)
                    {
                        // Mark the files as being successfully deleted.
                        deletedFiles.AddRange(datumsForArchive);

                        // Unload the archive.
                        string archiveFilePath = this.loadedArchives[archiveIndex];
                        UnloadArchive(fileIds[i].ArchiveId);

                        // Delete the archive file.
                        File.Delete(archiveFilePath);
                    }
                    else if (result == DialogResult.No)
                    {
                        // Skip deleting files for this archive.
                        continue;
                    }
                    else
                    {
                        // Abort the delete operation.
                        filesDeleted = deletedFiles.ToArray();
                        return false;
                    }
                }
                else
                {
                    // Loop through all of the files to be deleted and remove them from the lookup dictionaries.
                    for (int x = 0; x < datumsForArchive.Length; x++)
                    {
                        // Cleanup dictionary entries.
                        UnloadFile(datumsForArchive[x]);
                    }

                    // Delete the files from the archive.
                    if (this.archives[archiveIndex].DeleteFiles(datumsForArchive) == false)
                    {
                        // Failed to delete files from the archive.
                        string archiveName = this.archives[archiveIndex].FileName.Substring(this.archives[archiveIndex].FileName.LastIndexOf("\\") + 1);
                        MessageBox.Show(string.Format("Failed to delete files from '{0}'!", archiveName));
                    }
                    else
                    {
                        // Mark the files as being successfully deleted.
                        deletedFiles.AddRange(datumsForArchive);
                    }
                }
            }

            // Files successfully deleted.
            filesDeleted = deletedFiles.ToArray();
            return true;
        }

        public bool AddFilesToArchive(string archiveFilePath, DatumIndex[] datums, string[] newFileNames)
        {
            // Check if this archive is in the list of loaded archives.
            int archiveIndex = this.loadedArchives.IndexOf(archiveFilePath);
            if (archiveIndex != -1)
            {
                // Add the files to the archive.
                return this.archives[archiveIndex].AddFilesFromDatums(datums, newFileNames);
            }

            // Create a new archive using the file path provided.
            Archive archive = new Archive(archiveFilePath, this.nextArchiveId++, true);

            // Add the files to the archive.
            if (archive.AddFilesFromDatums(datums, newFileNames) == false)
            {
                // Failed to create and add the files to the archive.
                return false;
            }

            // Add the archive to the collection.
            return AddArchive(archive, archiveFilePath, true);
        }

        #endregion

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

        private void CreateTreeNodeRoot(out TreeNode root, out TreeNode modsNode, out TreeNode gameFilesNode)
        {
            // Tree node collection we add to.
            root = new TreeNode();

            // Create the mods tree node.
            modsNode = new TreeNode("Mods");
            modsNode.ForeColor = System.Drawing.Color.Blue;
            modsNode.SelectedImageIndex = modsNode.ImageIndex = (int)UIIcon.FolderBlue;
            modsNode.Tag = new TreeNodeTag(true, new DatumIndex(DatumIndex.Unassigned), false);
            root.Nodes.Add(modsNode);

            // Create the game files tree node.
            gameFilesNode = new TreeNode("Game Files");
            gameFilesNode.SelectedImageIndex = gameFilesNode.ImageIndex = (int)UIIcon.Folder;
            gameFilesNode.Tag = new TreeNodeTag(false, new DatumIndex(DatumIndex.Unassigned), false);
            root.Nodes.Add(gameFilesNode);
        }

        private TreeNodeCollection BuildTreeNodeByFolderPath()
        {
            // Setup tree node root.
            CreateTreeNodeRoot(out TreeNode root, out TreeNode modsNode, out TreeNode gameFilesNode);

            // Loop through all of the archives and build the tree node from the file entries.
            for (int i = 0; i < this.archives.Count; i++)
            {
                // If this is a patch archive create a root node for it so we can bubble it to the top of the list.
                TreeNode parent;
                if (this.archives[i].IsPatchFile == true)
                {
                    // Create a new tree node for the archive.
                    TreeNode archiveNode = new TreeNode(this.archives[i].FileName.Substring(this.archives[i].FileName.LastIndexOf("\\") + 1));
                    archiveNode.SelectedImageIndex = archiveNode.ImageIndex = (int)UIIcon.FolderBlue;
                    archiveNode.ForeColor = System.Drawing.Color.Blue;
                    archiveNode.Tag = new TreeNodeTag(true, new DatumIndex(DatumIndex.Unassigned)); ;

                    // Add this node to the list and change the parent for adding files.
                    modsNode.Nodes.Add(archiveNode);
                    parent = archiveNode;
                }
                else
                {
                    // Add the file nodes to the game files node.
                    parent = gameFilesNode;
                }

                // Loop through all of the file entries for this archive and add each one to the tree view.
                for (int x = 0; x < this.archives[i].FileEntries.Length; x++)
                {
                    // Add the file path as tree nodes.
                    AddFilePathAsTreeNodes(parent, this.archives[i].FileEntries[x].FileName, this.archives[i].FileEntries[x].FileType, 
                        new DatumIndex(this.archives[i].ArchiveId, this.archives[i].FileEntries[x].FileId), this.archives[i].IsPatchFile);
                }
            }

            // Return the tree node collection.
            return root.Nodes;
        }

        private TreeNodeCollection BuildTreeNodeByArchive()
        {
            // Setup tree node root.
            CreateTreeNodeRoot(out TreeNode root, out TreeNode modsNode, out TreeNode gameFilesNode);

            // Loop through all of the archives and create a node for each one.
            for (int i = 0; i < this.archives.Count; i++)
            {
                // Create a tree node for the archive.
                TreeNode arcNode = new TreeNode(this.archives[i].FileName.Substring(this.archives[i].FileName.LastIndexOf("\\") + 1));
                arcNode.Name = arcNode.Text;
                arcNode.SelectedImageIndex = arcNode.ImageIndex = (int)(this.archives[i].IsPatchFile == true ? UIIcon.PatchArchive : UIIcon.Archive);
                arcNode.Tag = new TreeNodeTag(this.archives[i].IsPatchFile, new DatumIndex(DatumIndex.Unassigned));

                // Add the node to the corresponding files node.
                if (this.archives[i].IsPatchFile == true)
                    modsNode.Nodes.Add(arcNode);
                else
                    gameFilesNode.Nodes.Add(arcNode);

                // If this archive is a patch file color the node blue.
                if (this.archives[i].IsPatchFile == true)
                    arcNode.ForeColor = System.Drawing.Color.Blue;

                // Loop through all the files in the archive and add them to the arc node.
                for (int x = 0; x < this.archives[i].FileEntries.Length; x++)
                {
                    // Add the file path as tree nodes.
                    AddFilePathAsTreeNodes(arcNode, this.archives[i].FileEntries[x].FileName, this.archives[i].FileEntries[x].FileType, 
                        new DatumIndex(this.archives[i].ArchiveId, this.archives[i].FileEntries[x].FileId), this.archives[i].IsPatchFile);
                }
            }

            // Return the tree node collection.
            return root.Nodes;
        }

        private TreeNodeCollection BuildTreeNodeByFileType()
        {
            // Setup tree node root.
            CreateTreeNodeRoot(out TreeNode root, out TreeNode modsNode, out TreeNode gameFilesNode);

            // Loop through all of the archives in the collection.
            for (int i = 0; i < this.archives.Count; i++)
            {
                // Loop through all the files in the current arc.
                for (int x = 0; x < this.archives[i].FileEntries.Length; x++)
                {
                    TreeNode previousNode = (this.archives[i].IsPatchFile == true ? modsNode : gameFilesNode);

                    // Check if the root node already has a tree node for this resource type.
                    TreeNode[] nodesFound = previousNode.Nodes.Find(this.archives[i].FileEntries[x].FileType.ToString(), false);
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
                        previousNode.Tag = new TreeNodeTag(this.archives[i].IsPatchFile, new DatumIndex(DatumIndex.Unassigned), false);

                        // Add the node to the corresponding files node.
                        if (this.archives[i].IsPatchFile == true)
                            modsNode.Nodes.Add(previousNode);
                        else
                            gameFilesNode.Nodes.Add(previousNode);
                    }

                    // Add the file path as tree nodes to the resource type node.
                    AddFilePathAsTreeNodes(previousNode, this.archives[i].FileEntries[x].FileName, this.archives[i].FileEntries[x].FileType, 
                        new DatumIndex(this.archives[i].ArchiveId, this.archives[i].FileEntries[x].FileId), this.archives[i].IsPatchFile);
                }
            }

            // Return the tree node collection.
            return root.Nodes;
        }

        private void AddFilePathAsTreeNodes(TreeNode root, string filePath, ResourceType fileType, DatumIndex datum, bool isPatchFile)
        {
            // Split the file name.
            string[] pieces = filePath.Split(new string[] { "\\" }, StringSplitOptions.None);

            // Check if this file is overrided by a patch file.
            bool isOverrided = this.patchArchiveFileNameReverseDictionary.ContainsKey(filePath);

            // Loop through all of the pieces and add each one to the tree node.
            TreeNode previousNode = root;
            for (int z = 0; z < pieces.Length; z++)
            {
                // Check if the previous node has an existing node for the current folder.
                TreeNode[] nodesFound = previousNode.Nodes.Find(pieces[z], false);
                if (nodesFound.Length > 0)
                {
                    // Check if there is a node that has the same patch status as us.
                    bool found = false;
                    for (int j = 0; j < nodesFound.Length; j++)
                    {
                        // Check if this node has the same patch status.
                        if (nodesFound[j].Tag != null && ((TreeNodeTag)nodesFound[j].Tag).IsPatchFile == isPatchFile)
                        {
                            // Set the new previous node and continue.
                            previousNode = nodesFound[j];
                            found = true;
                            break;
                        }
                    }

                    // If we found the correct parent node skip to next loop interation.
                    if (found == true)
                        continue;
                }

                // Create a new tree node.
                TreeNode newNode = new TreeNode(pieces[z]);
                newNode.Name = pieces[z];
                newNode.ImageIndex = (int)(isPatchFile == true ? UIIcon.FolderBlue : UIIcon.Folder);
                newNode.Tag = new TreeNodeTag(isPatchFile, new DatumIndex(DatumIndex.Unassigned));

                // If there is a patch file that overrides this file set the node text to blue.
                if (isOverrided == true)
                {
                    // Set the color of this node.
                    newNode.ForeColor = System.Drawing.Color.Blue;

                    // Recursively set the color of parent nodes to match.
                    for (TreeNode pnode = previousNode; pnode != null && pnode.Text != "Game Files"; pnode = pnode.Parent)
                        pnode.ForeColor = System.Drawing.Color.Blue;
                }

                // Check if we need to set additional properties.
                if (z == pieces.Length - 1)
                {
                    // Set the file datum for the node tag.
                    newNode.Tag = new TreeNodeTag(isPatchFile, datum, true, isOverrided);

                    // If there is no resource editor for this node set the color to red.
                    if (GameResource.ResourceParsers.ContainsKey(fileType) == false)
                    {
                        // No resource editor for this file type, set the node color to red.
                        newNode.ForeColor = System.Drawing.Color.Red;
                    }

                    // Check the type and see if we should set an icon for this node.
                    switch (fileType)
                    {
                        case ResourceType.rModel: newNode.ImageIndex = (int)UIIcon.Model; break;
                        case ResourceType.rTexture: newNode.ImageIndex = (int)UIIcon.Texture; break;
                        default: newNode.ImageIndex = (int)UIIcon.File; break;
                    }
                }
                else
                {
                    // Set the node tag.
                    newNode.Tag = new TreeNodeTag(isPatchFile, new DatumIndex(DatumIndex.Unassigned), true, isOverrided);
                }

                // Set the selected image index.
                newNode.SelectedImageIndex = newNode.ImageIndex;

                // Add the node to the parent node's collection.
                previousNode.Nodes.Add(newNode);
                previousNode = newNode;
            }
        }

        #endregion

        #region Misc

        public void CreateDuplicateFileReport(string filePath)
        {
            // Create a new StreamWriter to write the report file.
            StreamWriter writer = new StreamWriter(filePath);

            // Build a list of all files that have more than 1 datum index associated with the file name.
            KeyValuePair<string, DatumIndex[]>[] duplicateFiles = this.archiveFileNameReverseDictionary.Where(p => p.Value.Length > 1).ToArray();
            writer.WriteLine("Found {0} files with at least one duplicate\r\n", duplicateFiles.Count());

            // Loop through all the duplicate files and write each one to file.
            for (int i = 0; i < duplicateFiles.Length; i++)
            {
                // Write the relative path of the file.
                writer.WriteLine(duplicateFiles[i].Key);

                // Loop and write the name of each archive that has an instance of this file.
                for (int x = 0; x < duplicateFiles[i].Value.Length; x++)
                {
                    // Write the name of the archive.
                    int arcIndex = this.archiveLookupDictionary[duplicateFiles[i].Value[x].ArchiveId];
                    string fileName = this.archives[arcIndex].FileName;
                    writer.WriteLine("\t" + fileName.Substring(fileName.LastIndexOf("\\") + 1));
                }

                // Write a new line for spacing.
                writer.WriteLine();
            }

            // Close the stream writer.
            writer.Close();
        }

        #endregion
    }
}
