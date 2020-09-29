using DeadRisingArcTool.FileFormats;
using DeadRisingArcTool.FileFormats.Archive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.Utilities
{
    public class FileNameTreeNode
    {
        /// <summary>
        /// File or folder name of the node
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// DatumIndex of the file is this node represents a file, or DatumIndex.Unassigned if a folder
        /// </summary>
        public DatumIndex FileDatum { get; set; } = new DatumIndex(DatumIndex.Unassigned);
        /// <summary>
        /// Child nodes
        /// </summary>
        public LinkedList<FileNameTreeNode> Nodes { get; set; } = new LinkedList<FileNameTreeNode>();
    }

    public class FileNameTree : FileNameTreeNode
    {
        public static FileNameTree BuildFileNameTree(params ResourceType[] fileTypes)
        {
            List<string> fileNames = new List<string>();
            Dictionary<string, DatumIndex> fileNamesDatumLookup = new Dictionary<string, DatumIndex>();

            // Build a list of every file that matches the file types specified.
            for (int i = 0; i < ArchiveCollection.Instance.Archives.Length; i++)
            {
                // Loop through all the files in the archive.
                Archive archive = ArchiveCollection.Instance.Archives[i];
                for (int x = 0; x < archive.FileEntries.Length; x++)
                {
                    // Check if the file type matches before checking the file name to save time.
                    if (fileTypes.Contains(archive.FileEntries[x].FileType) == true)
                    {
                        // Check if we already have an entry for this file name.
                        if (fileNamesDatumLookup.ContainsKey(archive.FileEntries[x].FileName.ToLower()) == false)
                        {
                            // Add the file to the list.
                            fileNames.Add(archive.FileEntries[x].FileName.ToLower());
                            fileNamesDatumLookup.Add(archive.FileEntries[x].FileName.ToLower(), new DatumIndex(archive.ArchiveId, archive.FileEntries[x].FileId));
                        }
                    }
                }
            }

            // Sort the list of file names.
            fileNames.Sort();

            // TODO: This building algorithm could be optimized.

            // Loop through the list of files names and build the file name tree.
            FileNameTree tree = new FileNameTree();
            for (int i = 0; i < fileNames.Count; i++)
            {
                // Split the file name into pieces.
                string[] pieces = fileNames[i].Split('\\');

                // Loop and add nodes for each piece needed.
                FileNameTreeNode parent = tree;
                for (int x = 0; x < pieces.Length; x++)
                {
                    // Check if there is a node for this file name piece.
                    bool found = false;
                    foreach (FileNameTreeNode node in parent.Nodes)
                    {
                        // Check if the node name matches.
                        if (node.Name == pieces[x])
                        {
                            // Set the parent and continue.
                            parent = node;
                            found = true;
                            break;
                        }
                    }

                    // If no node was found create a new one.
                    if (found == false)
                    {
                        // Create a new node and set it as the parent.
                        FileNameTreeNode node = new FileNameTreeNode();
                        node.Name = pieces[x];
                        parent.Nodes.AddLast(node);
                        parent = node;
                    }
                }

                // The last node is the file name node, set the datum index for it.
                parent.FileDatum = fileNamesDatumLookup[fileNames[i]];
            }

            // Return the file name tree.
            return tree;
        }
    }
}
