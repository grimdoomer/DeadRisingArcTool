﻿using ImGuiNET;
using ImVector2 = System.Numerics.Vector2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DeadRisingArcTool.FileFormats.Geometry.DirectX.UI
{
    public enum ImGuiMessageBoxOptions
    {
        Ok,
        OkCancel,
        YesNo,
        YesNoCancel
    }

    public class ImGuiMessageBox : ImGuiDialogBox
    {
        /// <summary>
        /// Caption to be displayed
        /// </summary>
        public string Caption { get; set; }
        /// <summary>
        /// Button options
        /// </summary>
        public ImGuiMessageBoxOptions Options { get; set; }

        public ImGuiMessageBox(string title, string caption, ImGuiMessageBoxOptions options) : base(title)
        {
            // Initialize fields.
            this.Caption = caption;
            this.Options = options;
        }

        public override bool DrawDialog()
        {
            bool dialogResult = false;

            // Always center the window when appearing.
            ImVector2 center = new ImVector2(ImGui.GetIO().DisplaySize.X * 0.5f, ImGui.GetIO().DisplaySize.Y * 0.5f);
            ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new ImVector2(0.5f, 0.5f));

            // Show the dialog box.
            bool isOpen = true;
            if (ImGui.BeginPopupModal(this.Title, ref isOpen, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse) == true)
            {
                // Calculate the number of buttons to display.
                int buttonCount = 1;
                switch (this.Options)
                {
                    case ImGuiMessageBoxOptions.OkCancel:
                    case ImGuiMessageBoxOptions.YesNo:
                        buttonCount = 2; break;
                    case ImGuiMessageBoxOptions.YesNoCancel:
                        buttonCount = 3; break;
                }

                float startPos = 0.0f;
                float buttonWidth = 65.0f;
                float buttonWidthTotal = (buttonWidth * buttonCount) + (ImGui.GetStyle().ItemInnerSpacing.X * (buttonCount - 1));

                // Draw the message box text.
                ImGui.Text(this.Caption);
                ImGui.Separator();

                // Calculate the width of the buttons so we can center them in the dialog.
                ImVector2 dialogSize = ImGui.GetWindowSize();
                if (dialogSize.X > buttonWidthTotal)
                    startPos = (dialogSize.X / 2) - (buttonWidthTotal / 2);

                // Set the starting x position to center the buttons.
                ImGui.SetCursorPosX(startPos);

                // Check the message box style and handle accordingly.
                switch (this.Options)
                {
                    case ImGuiMessageBoxOptions.Ok:
                        {
                            if (ImGui.Button("Ok", new ImVector2(buttonWidth, 0)) == true)
                                this.Result = ImGuiDialogBoxResult.Ok;
                            break;
                        }
                    case ImGuiMessageBoxOptions.OkCancel:
                        {
                            if (ImGui.Button("Ok", new ImVector2(buttonWidth, 0)) == true)
                                this.Result = ImGuiDialogBoxResult.Ok;
                            ImGui.SameLine();
                            if (ImGui.Button("Cancel", new ImVector2(buttonWidth, 0)) == true)
                                this.Result = ImGuiDialogBoxResult.Cancel;
                            break;
                        }
                    case ImGuiMessageBoxOptions.YesNo:
                        {
                            if (ImGui.Button("Yes") == true)
                                this.Result = ImGuiDialogBoxResult.Yes;
                            ImGui.SameLine();
                            if (ImGui.Button("No") == true)
                                this.Result = ImGuiDialogBoxResult.No;
                            break;
                        }
                    case ImGuiMessageBoxOptions.YesNoCancel:
                        {
                            if (ImGui.Button("Yes", new ImVector2(buttonWidth, 0)) == true)
                                this.Result = ImGuiDialogBoxResult.Yes;
                            ImGui.SameLine();
                            if (ImGui.Button("No", new ImVector2(buttonWidth, 0)) == true)
                                this.Result = ImGuiDialogBoxResult.No;
                            ImGui.SameLine();
                            if (ImGui.Button("Cancel", new ImVector2(buttonWidth, 0)) == true)
                                this.Result = ImGuiDialogBoxResult.Cancel;
                            break;
                        }
                }

                // If the dialog result was set close the dialog box.
                if (this.Result != ImGuiDialogBoxResult.None)
                {
                    // Close the dialog box.
                    dialogResult = true;
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }
            //else
            //{
            //    // If the dialog is was previously opened and no dialog result is set then the close button was pressed.
            //    if (this.dialogOpen == true && this.Result == ImGuiMessageBoxResult.None)
            //    {
            //        // The close button was pressed, set the cancel dialog result.
            //        this.dialogOpen = false;
            //        dialogResult = true;
            //        this.Result = ImGuiMessageBoxResult.Cancel;
            //    }
            //}

            // Return the dialog result.
            return dialogResult;
        }
    }
}