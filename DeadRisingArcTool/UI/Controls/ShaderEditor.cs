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
using DeadRisingArcTool.FileFormats.Geometry;
using System.Reflection;
using DeadRisingArcTool.Utilities;

namespace DeadRisingArcTool.Controls
{
    [GameResourceEditor(ResourceType.rShader)]
    public partial class ShaderEditor : GameResourceEditorControl
    {
        public rShader Shader { get { return (rShader)this.GameResource; } }

        public ShaderEditor()
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

            // Loop and print all of the parameters.
            string parameters = "*** Parameters\n";
            for (int i = 0; i < this.Shader.parameters.Length; i++)
            {
                parameters += string.Format("   Parameter: {0} Type: {1} RegCount: {2}\n", this.Shader.parameters[i].ParameterId,
                    this.Shader.parameters[i].Type, this.Shader.parameters[i].RegCount);
            }

            string techniques = "*** Techniques\n";
            for (int i = 0; i < this.Shader.techniques.Length; i++)
            {
                string tech = string.Format("   TechniqueId: {0}\n\tShaderDescSize: {1}\n\tByteCodeSize: {2}\n",
                    this.Shader.techniques[i].TechniqueId, this.Shader.techniques[i].ShaderDescSize, this.Shader.techniques[i].ByteCodeSize);
                tech += StructureToString(this.Shader.techniques[i].ShaderDescriptor) + "\n";

                // Loop and print all of the shader indices.
                for (int x = 0; x < this.Shader.techniques[i].ShaderDescriptor.ShaderIndexCount; x++)
                {
                    tech += string.Format("\tShader Set {0}: VertexShaderIndex: {1} PixelShaderIndex: {2}\n", x,
                        this.Shader.techniques[i].ShaderDescriptor.ShaderIndices[x].VertexShaderIndex,
                        this.Shader.techniques[i].ShaderDescriptor.ShaderIndices[x].PixelShaderIndex);
                }
                tech += "\n";

                // Loop and print all the vertex shaders for the technique.
                for (int x = 0; x < this.Shader.techniques[i].ShaderDescriptor.VertexShaderCount; x++)
                {
                    tech += string.Format("\tVertexShader {0}: Offset: {1} Size: {2} Flags: {3}\n", x,
                        this.Shader.techniques[i].ShaderDescriptor.VertexShaders[x].ByteCodeOffset,
                        this.Shader.techniques[i].ShaderDescriptor.VertexShaders[x].ByteCodeSize,
                        this.Shader.techniques[i].ShaderDescriptor.VertexShaders[x].Flags.ToString("X16"));

                    for (int z = 0; z < this.Shader.techniques[i].ShaderDescriptor.VertexShaders[x].Parameters.Length; z++)
                    {
                        tech += string.Format("\t\tParam {0}: {1} Offset: {2} Size: {3}\n", z,
                            this.Shader.parameters[this.Shader.techniques[i].ShaderDescriptor.VertexShaders[x].Parameters[z].ParameterIndex].ParameterId,
                            this.Shader.techniques[i].ShaderDescriptor.VertexShaders[x].Parameters[z].Offset.ToString("X4"),
                            this.Shader.techniques[i].ShaderDescriptor.VertexShaders[x].Parameters[z].Size);
                    }
                    tech += "\n";
                }
                tech += "\n";

                // Loop and print all of the pixel shaders for the technique.
                for (int x = 0; x < this.Shader.techniques[i].ShaderDescriptor.PixelShaderCount; x++)
                {
                    tech += string.Format("\tPixelShader {0}: Offset: {1} Size: {2} Flags: {3}\n", x,
                        this.Shader.techniques[i].ShaderDescriptor.PixelShaders[x].ByteCodeOffset,
                        this.Shader.techniques[i].ShaderDescriptor.PixelShaders[x].ByteCodeSize,
                        this.Shader.techniques[i].ShaderDescriptor.PixelShaders[x].Flags.ToString("X16"));

                    for (int z = 0; z < this.Shader.techniques[i].ShaderDescriptor.PixelShaders[x].Parameters.Length; z++)
                    {
                        tech += string.Format("\t\tParam {0}: {1} Offset: {2} Size: {3}\n", z,
                            this.Shader.parameters[this.Shader.techniques[i].ShaderDescriptor.PixelShaders[x].Parameters[z].ParameterIndex].ParameterId,
                            this.Shader.techniques[i].ShaderDescriptor.PixelShaders[x].Parameters[z].Offset.ToString("X4"),
                            this.Shader.techniques[i].ShaderDescriptor.PixelShaders[x].Parameters[z].Size);
                    }
                    tech += "\n";
                }

                techniques += tech + "\n";
            }

            this.textBox1.Text = parameters.Replace("\n", "\r\n") + "\r\n" + techniques.Replace("\n", "\r\n");
        }

        public override bool SaveResource()
        {
            return false;
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
                {
                    // Check if the field has a hex attribute on it.
                    if (fields[i].GetCustomAttribute<HexAttribute>() != null)
                        outputString += string.Format("\t{0}: {1}\n", fields[i].Name, int.Parse(value.ToString(), System.Globalization.NumberStyles.Integer).ToString("X"));
                    else
                        outputString += string.Format("\t{0}: {1}\n", fields[i].Name, value.ToString());
                }
            }

            // Return the string.
            return outputString;
        }
    }
}
