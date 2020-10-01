using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Geometry.DirectX.UI
{
    public enum ImGuiDialogBoxResult
    {
        None,
        Ok,
        Yes,
        No,
        Cancel
    }

    public abstract class ImGuiDialogBox
    {
        /// <summary>
        /// Title of the message box
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Result of the dialog based on user interaction
        /// </summary>
        public ImGuiDialogBoxResult Result { get; set; } = ImGuiDialogBoxResult.None;

        // Indicates if the dialog was opened so we can track when it closes.
        protected bool dialogOpen = false;

        /// <summary>
        /// Creates a new ImGuiDialog with the specified title
        /// </summary>
        /// <param name="title">Title of the dialog box</param>
        public ImGuiDialogBox(string title)
        {
            // Initialize fields.
            this.Title = title;
        }

        /// <summary>
        /// Called when the dialog should be displayed
        /// </summary>
        public virtual void ShowDialog()
        {
            // Display the message box.
            ImGui.OpenPopup(this.Title);
            this.dialogOpen = true;
        }

        /// <summary>
        /// Called each frame to draw the dialog box if it is being displayed.
        /// </summary>
        /// <returns>True if the dialog was closed, false otherwise</returns>
        public abstract bool DrawDialog();
    }
}
