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
                    ResourceType.rSMAdd)]
    public partial class TextEditor : GameResourceEditorControl
    {
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

            // The game resource is a XmlFile.
            XmlFile file = (XmlFile)this.GameResource;
            this.textbox.Text = Encoding.Default.GetString(file.Buffer);
        }
    }
}
