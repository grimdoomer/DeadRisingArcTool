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
using System.Reflection;

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
            headerInfo += StructureToString(model.header) + "\n";

            string textures = "*** Textures\n";
            if (model.textureFileNames != null)
            {
                for (int i = 0; i < model.textureFileNames.Length; i++)
                {
                    textures += string.Format("\t[{0}]: {1}\n", i + 1, model.textureFileNames[i]);
                }
            }
            textures += "\n";

            string joints = "*** Joints\n";
            for (int i = 0; i < model.joints.Length; i++)
            {
                string joint = "   Joint #" + i.ToString() + "\n";
                joint += StructureToString(model.joints[i]);
                joints += joint + "\n";
            }

            string materials = "*** Materials\n";
            for (int i = 0; i < model.materials.Length; i++)
            {
                string mat = "   Material #" + i.ToString() + "\n";
                mat += StructureToString(model.materials[i]);
                materials += mat + "\n";
            }

            string primitives = "*** Primitives\n";
            for (int i = 0; i < model.primitives.Length; i++)
            {
                string prim = "   Primitive #" + i.ToString() + "\n";
                prim += StructureToString(model.primitives[i]);
                primitives += prim + "\n";
            }

            // Set textbox text.
            this.textBox1.Text = headerInfo.Replace("\n", "\r\n") + textures.Replace("\n", "\r\n") + joints.Replace("\n", "\r\n") + materials.Replace("\n", "\r\n") + primitives.Replace("\n", "\r\n");
        }

        public override bool SaveResource()
        {
            throw new NotImplementedException();
        }

        private void btnRender_Click(object sender, EventArgs e)
        {
            // Display a new render window.
            RenderView render = new RenderView(this.GameResource.Datum);

            // TODO: Figure out what the fuck is up with this...
            try
            {
                render.Visible = true;
            }
            catch (ObjectDisposedException ex)
            {
            }
        }

        private string StructureToString(object obj)
        {
            string outputString = "";

            // Get a list of all fields for the structure and format them all into a string.
            FieldInfo[] fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < fields.Length; i++)
            {
                // Get the value of field and make sure it's not null.
                object value = fields[i].GetValue(obj);
                if (value != null)
                    outputString += string.Format("\t{0}:{1}\n", fields[i].Name, value.ToString());
            }

            // Return the string.
            return outputString;
        }
    }
}
