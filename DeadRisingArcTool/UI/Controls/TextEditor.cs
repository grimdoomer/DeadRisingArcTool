using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DeadRisingArcTool.FileFormats;
using DeadRisingArcTool.FileFormats.Misc;
using DeadRisingArcTool.FileFormats.Archive;

namespace DeadRisingArcTool.Controls
{
    [GameResourceEditor(ResourceType.rCameraListXml,
                    ResourceType.rClothXml,
                    ResourceType.rEnemyLayoutXml,
                    ResourceType.rFSMBrainXml,
                    ResourceType.rMarkerLayoutXml,
                    ResourceType.rModelInfoXml,
                    ResourceType.rModelLayoutXml,
                    ResourceType.rNMMachineXml,
                    ResourceType.rRouteNodeXml,
                    ResourceType.rSchedulerXml,
                    ResourceType.rSoundSegXml,
                    ResourceType.rUBCellXml,
                    ResourceType.rEventTimeSchedule,
                    ResourceType.rHavokVehicleData,
                    ResourceType.rAreaHitLayout,
                    ResourceType.rHavokConstraintLayout,
                    ResourceType.rHavokLinkCollisionLayout,
                    ResourceType.rHavokVertexLayout,
                    ResourceType.rItemLayout,
                    ResourceType.rMobLayout,
                    ResourceType.rSprLayout,
                    ResourceType.rSMAdd,
                    ResourceType.rMapLink)]
    public partial class TextEditor : GameResourceEditorControl
    {
        public XmlFile TextFile { get { return (XmlFile)this.GameResource; } }

        public TextEditor()
        {
            InitializeComponent();
        }

        protected override void OnGameResourceUpdated()
        {
            // Make sure the arc file and game resource are valid.
            if (this.ArcFile == null || this.GameResource == null)
            {
                // Clear the textbox contents and return.
                this.textbox.Text = "";
                return;
            }

            // Only allow editing for patch files.
            this.textbox.ReadOnly = !this.ArcFile.IsPatchFile;

            // The game resource is a XmlFile.
            this.textbox.Text = Encoding.Default.GetString(this.TextFile.Buffer);
            this.HasBeenModified = false;
            this.textbox.IsChanged = false;
        }

        public override bool SaveResource()
        {
            // Set the UI state to disabled while we write to file.
            this.EditorOwner.SetUIState(false);

            // Update the xml file buffer.
            this.TextFile.Buffer = Encoding.ASCII.GetBytes(this.textbox.Text);

            // Get a list of duplicate datums that we should update and update all of them.
            DatumIndex[] datums = this.EditorOwner.GetDatumsToUpdateForResource(this.GameResource.FileName);
            if (ArchiveCollection.Instance.InjectFile(datums, this.TextFile.Buffer) == false)
            {
                // Failed to update files.
                return false;
            }

            // Flag that we no longer have changes made to the resource.
            this.HasBeenModified = false;
            this.textbox.IsChanged = false;

            // Changes saved successfully, re-enable the UI.
            this.EditorOwner.SetUIState(true);
            MessageBox.Show("Changes saved successfully!");
            return true;
        }

        private void textbox_TextChanged(object sender, FastColoredTextBoxNS.TextChangedEventArgs e)
        {
            // Flag that the resource data has been modified.
            this.HasBeenModified = true;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Check for the Ctrl+S hotkey.
            if (keyData == (Keys.Control | Keys.S))
            {
                // If changes have been made save them.
                if (this.HasBeenModified == true)
                {
                    SaveResource();
                }

                // Skip passing the event to the base class.
                return true;
            }

            // Let the base class handle it.
            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}
