using DeadRisingArcTool.FileFormats.Archive;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DeadRisingArcTool.Utilities
{
    public class CustomTreeViewNodeSorter : System.Collections.IComparer
    {
        public int Compare(object x, object y)
        {
            // Cast the objects to TreeNodes.
            TreeNode a = (TreeNode)x;
            TreeNode b = (TreeNode)y;

            // First compare by patch status.
            int patchCompare = ((TreeNodeTag)b.Tag).IsPatchFile.CompareTo(((TreeNodeTag)a.Tag).IsPatchFile);
            if (patchCompare != 0)
                return patchCompare;

            // Next compare by the number of child nodes.
            int nodesA = a.Nodes.Count > 0 ? 1 : 0;
            int nodesB = b.Nodes.Count > 0 ? 1 : 0;
            if (nodesA != nodesB)
                return nodesB.CompareTo(nodesA);    // Sort in descending order

            // Finally compare by node text.
            return a.Text.CompareTo(b.Text);
        }
    }
}
