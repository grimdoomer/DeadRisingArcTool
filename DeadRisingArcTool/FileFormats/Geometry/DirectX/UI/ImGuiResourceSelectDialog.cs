using DeadRisingArcTool.FileFormats.Archive;
using DeadRisingArcTool.Utilities;
using ImGuiNET;
using ImVector2 = System.Numerics.Vector2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Geometry.DirectX.UI
{
    public class ImGuiResourceSelectDialog : ImGuiDialogBox
    {
        /// <summary>
        /// List of resource types to browse for
        /// </summary>
        public ResourceType[] ResourceTypes { get; private set; }
        /// <summary>
        /// DatumIndex for the file that was selected
        /// </summary>
        public DatumIndex SelectedFile { get; private set; } = new DatumIndex(DatumIndex.Unassigned);

        public delegate bool OnResourceSelectedHandler(DatumIndex datum, out string errorMessage);
        /// <summary>
        /// Callback for when a file is selected and the ok button is pressed, returns true if the resource
        /// is acceptible, or false otherwise.
        /// </summary>
        public OnResourceSelectedHandler OnResourceSelected { get; set; } = null;

        // File names to display in the treeview.
        private FileNameTree fileTree;

        // Used to display errors to the user.
        private ImGuiMessageBox errorDialog;

        public ImGuiResourceSelectDialog(string title, params ResourceType[] resourceTypes) : base(title)
        {
            // Initialize fields.
            this.ResourceTypes = resourceTypes;
        }

        public override void ShowDialog()
        {
            // Build the file tree based on the resource types set.
            this.fileTree = FileNameTree.BuildFileNameTree(this.ResourceTypes);

            // Display the dialog.
            base.ShowDialog();
        }

        public override bool DrawDialog()
        {
            bool result = false;

            // Always center the window when appearing.
            ImVector2 center = new ImVector2(ImGui.GetIO().DisplaySize.X * 0.5f, ImGui.GetIO().DisplaySize.Y * 0.5f);
            ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new ImVector2(0.5f, 0.5f));
            ImGui.SetNextWindowSize(new ImVector2(600, 420), ImGuiCond.Appearing);

            // Show the file selection dialog.
            if (ImGui.BeginPopupModal(this.Title) == true)
            {
                // Create a tree view for the file names list.
                ImGui.BeginChild("FileTree", new ImVector2(0, 360), true);
                foreach (FileNameTreeNode node in this.fileTree.Nodes)
                    ProcessTreeNodes(node);
                ImGui.EndChild();

                // Create the cancel and okay buttons.
                if (ImGui.Button("Cancel", new ImVector2(120, 0)) == true)
                {
                    // Close the dialog and clear the file name list.
                    ImGui.CloseCurrentPopup();
                    this.fileTree = null;
                }

                // If there is no selected file in the treeview, disable the okay button.
                if (this.SelectedFile.Datum == DatumIndex.Unassigned)
                {
                    ImGui.PushItemFlag(ImGuiItemFlags.Disabled, true);
                    ImGui.PushStyleVar(ImGuiStyleVar.Alpha, ImGui.GetStyle().Alpha * 0.5f);
                }

                ImGui.SameLine();
                if (ImGui.Button("Ok", new ImVector2(120, 0)) == true)
                {
                    // If there is a callback set for datum index approval, call it.
                    if (this.OnResourceSelected != null)
                    {
                        // Call the callback with the selected file datum.
                        string errorMessage = null;
                        if (this.OnResourceSelected(this.SelectedFile, out errorMessage) == true)
                        {
                            // Close the dialog and clear the file name list.
                            ImGui.CloseCurrentPopup();
                            this.fileTree = null;
                        }
                        else if (errorMessage != null)
                        {
                            // Create a new dialog to display the error to the user.
                            this.errorDialog = new ImGuiMessageBox("File select error", errorMessage, ImGuiMessageBoxOptions.Ok);
                            this.errorDialog.ShowDialog();
                        }
                    }
                    else
                    {
                        // Assume the datum is fine.
                        ImGui.CloseCurrentPopup();
                        this.fileTree = null;
                    }
                }

                // Restore style if needed.
                if (this.SelectedFile.Datum == DatumIndex.Unassigned)
                {
                    ImGui.PopItemFlag();
                    ImGui.PopStyleVar();
                }

                // Check if we need to draw the error dialog.
                if (this.errorDialog != null)
                {
                    // Draw the dialog.
                    if (this.errorDialog.DrawDialog() == true)
                    {
                        // Destroy the dialog instance.
                        this.errorDialog = null;
                    }
                }

                ImGui.EndPopup();
            }
            else
            {
                // TODO:
            }

            // Return the draw result.
            return result;
        }

        private void ProcessTreeNodes(FileNameTreeNode node)
        {
            // Check if this node is a leaf or not.
            if (node.Nodes.Count == 0)
            {
                // Create a node for the file.
                if (ImGui.Selectable(node.Name, this.SelectedFile == node.FileDatum) == true)
                {
                    // Set the selected file datum.
                    this.SelectedFile = node.FileDatum;
                }
            }
            else
            {
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
