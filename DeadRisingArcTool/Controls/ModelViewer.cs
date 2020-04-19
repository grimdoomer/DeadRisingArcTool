using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DeadRisingArcTool.FileFormats.Geometry;
using DeadRisingArcTool.Forms;
using DeadRisingArcTool.FileFormats.Archive;
using DeadRisingArcTool.FileFormats;

namespace DeadRisingArcTool.Controls
{
    [GameResourceEditor(ResourceType.rModel)]
    public partial class ModelViewer : GameResourceEditorControl
    {
        public ModelViewer()
        {
            InitializeComponent();
        }

        protected override void OnGameResourceUpdated()
        {
            // Make sure the arc file and game resource are valid.
            if (this.ArcFile == null || this.GameResource == null)
            {
                // Clear the textbox contents and return.
                this.textBox1.Text = "";
                return;
            }

            // Cast the game resource to a rModel object.
            rModel model = (rModel)this.GameResource;

            // Print all the model information out to the textbox.
            string headerInfo = "*** Header\n";
            headerInfo += "\tJoint count: " + model.header.JointCount.ToString() + "\n";
            headerInfo += "\tPrimitive count: " + model.header.PrimitiveCount.ToString() + "\n";
            headerInfo += "\tMaterial count: " + model.header.MaterialCount.ToString() + "\n";
            headerInfo += "\tVertice count: " + model.header.VerticeCount.ToString() + "\n";
            headerInfo += "\tIndice count: " + model.header.IndiceCount.ToString() + "\n";
            headerInfo += "\tPolygon count: " + model.header.PolygonCount.ToString() + "\n";
            headerInfo += "\tTexture count: " + model.header.NumberOfTextures.ToString() + "\n\n";
            headerInfo += "\tVertex data 1 size: " + model.header.VertexData1Size.ToString() + "\n";
            headerInfo += "\tVertex data 2 size: " + model.header.VertexData2Size.ToString() + "\n\n";
            //header

            string materials = "*** Materials\n";
            for (int i = 0; i < model.materials.Length; i++)
            {
                string mat = "   Material #" + i.ToString() + "\n";
                mat += "\tFlags: x" + model.materials[i].Flags.ToString("X") + "\n";
                mat += "\tUnk1: " + model.materials[i].Unk1.ToString() + "\n";
                mat += "\tUnk2: " + model.materials[i].Unk2.ToString() + "\n";
                mat += "\tUnk3: " + model.materials[i].Unk3.ToString() + "\n";
                mat += "\tUnk9: x" + model.materials[i].Unk9.ToString("X") + "\n";
                mat += "\tUnk1: " + model.materials[i].Unk1.ToString() + "\n";
                mat += "\tUnk5: " + model.materials[i].Unk5.ToString() + "\n";
                mat += "\tUnk6: " + model.materials[i].Unk6.ToString() + "\n";
                mat += "\tUnk7: " + model.materials[i].Unk7.ToString() + "\n";
                mat += "\tUnk8: " + model.materials[i].Unk8.ToString() + "\n";
                materials += mat + "\n";
            }

            string primitives = "*** Primitives\n";
            for (int i = 0; i < model.primitives.Length; i++)
            {
                string prim = "   Primitive #" + i.ToString() + "\n";
                prim += "\tUnk1: " + model.primitives[i].Unk1.ToString() + "\n";
                prim += "\tMaterial index: " + model.primitives[i].MaterialIndex.ToString() + "\n";
                prim += "\tUnk2: " + model.primitives[i].Enabled.ToString() + "\n";
                prim += "\tUnk3: " + model.primitives[i].Unk3.ToString() + "\n";
                prim += "\tUnk11: " + model.primitives[i].Unk11.ToString() + "\n";
                prim += "\tUnk12: " + model.primitives[i].Unk12.ToString() + "\n";
                prim += "\tVertex stride 1: " + model.primitives[i].VertexStride1.ToString() + "\n";
                prim += "\tVertex stride 2: " + model.primitives[i].VertexStride2.ToString() + "\n";
                prim += "\tUnk13: " + model.primitives[i].Unk13.ToString() + "\n";
                prim += "\tVertex count 1: " + model.primitives[i].VertexCount.ToString() + "\n";
                prim += "\tStartin vertex 1: " + model.primitives[i].StartingVertex.ToString() + "\n";
                prim += "\tUnk16: " + model.primitives[i].Unk16.ToString() + "\n";
                prim += "\tUnk5: " + model.primitives[i].Unk5.ToString() + "\n";
                prim += "\tVertex count 2: " + model.primitives[i].StartingIndexLocation.ToString() + "\n";
                prim += "\tStarting vertex 2: " + model.primitives[i].IndexCount.ToString() + "\n";
                prim += "\tUnk8: " + model.primitives[i].Unk8.ToString() + "\n";
                primitives += prim + "\n";
            }

            // Set textbox text.
            this.textBox1.Text = headerInfo.Replace("\n", "\r\n") + materials.Replace("\n", "\r\n") + primitives.Replace("\n", "\r\n");
        }

        private void btnRender_Click(object sender, EventArgs e)
        {
            // Display a new render window.
            RenderView renderer = new RenderView(this.ArcFile, (rModel)this.GameResource);
            renderer.Show();
        }
    }
}
