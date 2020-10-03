using DeadRisingArcTool.FileFormats.Archive;
using DeadRisingArcTool.Utilities;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ImVector2 = System.Numerics.Vector2;

namespace DeadRisingArcTool.FileFormats.Geometry.DirectX.UI.Controls
{
    public delegate void OnResourceSelectionChangedEvent(DatumIndex old, DatumIndex @new);
    public delegate void OnTreeNodeCheckedChangedEvent(FileNameTreeNode node);
    public delegate void OnTreeNodeDoubleClickedEvent(FileNameTreeNode node);

    public class ImGuiResourceSelectTree : ImGuiControl
    {
        /// <summary>
        /// Resource type filter applied to the tree view.
        /// </summary>
        public ResourceType[] ResourceTypes { get; protected set; }
        /// <summary>
        /// The selected file in the tree view.
        /// </summary>
        public DatumIndex SelectedFile { get; protected set; } = new DatumIndex(DatumIndex.Unassigned);
        /// <summary>
        /// Gets or sets the size of the tree view control.
        /// </summary>
        public ImVector2 Size { get; set; } = new ImVector2(0, 0);
        /// <summary>
        /// Indicates if the control should be drawn with a border or not.
        /// </summary>
        public bool Border { get; set; } = true;
        /// <summary>
        /// True if the tree view nodes should have checkboxes.
        /// </summary>
        public bool Checkboxes { get; set; } = false;

        /// <summary>
        /// Event handler for when the tree view selection has changed.
        /// </summary>
        public OnResourceSelectionChangedEvent OnResourceSelectionChanged { get; set; } = null;
        public OnTreeNodeCheckedChangedEvent OnTreeNodeCheckedChanged { get; set; } = null;
        public OnTreeNodeDoubleClickedEvent OnTreeNodeDoubleClicked { get; set; } = null;

        // Tree of file names to display.
        private FileNameTree fileNameTree;

        public ImGuiResourceSelectTree(FileNameTree fileNameTree)
        {
            // Initialize fields.
            this.ResourceTypes = new ResourceType[] { ResourceType.Unknown };
            this.fileNameTree = fileNameTree;
        }

        public ImGuiResourceSelectTree(params ResourceType[] resourceTypes)
        {
            // Initialize fields.
            this.ResourceTypes = resourceTypes;

            // Build the file tree.
            this.fileNameTree = FileNameTree.BuildFileNameTree(this.ResourceTypes);
        }

        public override void DrawControl()
        {
            // Create a tree view for the file names list.
            ImGui.BeginChild("FileTree", this.Size, this.Border);

            // Loop and create nodes for anything that is visible.
            foreach (FileNameTreeNode node in this.fileNameTree.Nodes)
                ProcessTreeNodes(node);

            ImGui.EndChild();
        }

        private void ProcessTreeNodes(FileNameTreeNode node)
        {
            // Check if this node is a leaf or not.
            if (node.Nodes.Count == 0)
            {
                // Check if we should draw checkboxes or not.
                if (this.Checkboxes == true)
                {
                    // Draw a checkbox for the node.
                    if (ImGui.Checkbox("##chk_" + node.Name, ref node.Checked) == true)
                    {
                        // If there is a checked change event handler call it.
                        if (this.OnTreeNodeCheckedChanged != null)
                            this.OnTreeNodeCheckedChanged(node);
                    }
                    ImGui.SameLine();
                }

                // Create a node for the file.
                if (ImGui.Selectable(node.Name, this.SelectedFile == node.FileDatum, ImGuiSelectableFlags.AllowDoubleClick) == true)
                {
                    // Check if the item was double clicked or not.
                    if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left) == true)
                    {
                        // If there is a double click event handler call it.
                        if (this.OnTreeNodeDoubleClicked != null)
                            this.OnTreeNodeDoubleClicked(node);
                    }
                    else
                    {
                        // Save the old datum.
                        DatumIndex oldDatum = this.SelectedFile;

                        // Set the selected file datum.
                        this.SelectedFile = node.FileDatum;

                        // If there is a selection changed handler call it.
                        if (this.OnResourceSelectionChanged != null)
                            this.OnResourceSelectionChanged(oldDatum, this.SelectedFile);
                    }
                }
            }
            else
            {
                // Draw a checkbox for the node.
                if (ImGui.Checkbox("##chk_" + node.Name, ref node.Checked) == true)
                {
                    // Get a list of all child nodes and update the checked state for each one.
                    FileNameTreeNode[] childNodes = node.GetChildNodes();
                    for (int i = 0; i < childNodes.Length; i++)
                    {
                        // Check if the node checked state is actually changing or not.
                        if (childNodes[i].Checked != node.Checked)
                        {
                            // Update the child node checked state.
                            childNodes[i].Checked = node.Checked;

                            // If there is a checked change event handler call it.
                            if (this.OnTreeNodeCheckedChanged != null)
                                this.OnTreeNodeCheckedChanged(childNodes[i]);
                        }
                    }

                    // If there is a checked change event handler call it.
                    if (this.OnTreeNodeCheckedChanged != null)
                        this.OnTreeNodeCheckedChanged(node);
                }
                ImGui.SameLine();

                // Create a tree node for this node.
                if (ImGui.TreeNodeEx(node.Name) == true)
                {
                    // Loop through all the child nodes and process recursively.
                    foreach (FileNameTreeNode child in node.Nodes)
                    {
                        // Recursively process the node.
                        ProcessTreeNodes(child);
                    }

                    ImGui.TreePop();
                }
            }
        }
    }
}
